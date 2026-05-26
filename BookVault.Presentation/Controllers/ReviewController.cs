// BookVault.Presentation/Controllers/ReviewController.cs
using BookVault.Presentation.ApiResponses;
using BookVault.Repository.Models.ReviewModels;
using BookVault.Service.Enums.Review;
using BookVault.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVault.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        // ====================== [ GET REVIEW BY ID ] ======================
        [HttpGet("GetReview/{id}", Name = "GetReviewByID")]
        [ProducesResponseType(typeof(ApiResponse<Review>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<Review>> GetReviewByID(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid review ID.");

            var (result, service) = ReviewService.Find(id);

            return result switch
            {
                enReviewRetrieveResult.Found => Ok(new ApiResponse<Review>("Review retrieved successfully.", new Review
                {
                    ReviewID = service!.ReviewID,
                    UserID = service.UserID,
                    UserName = service.UserName,
                    BookID = service.BookID,
                    BookTitle = service.BookTitle,
                    Rating = service.Rating,
                    Comment = service.Comment,
                    CreatedAt = service.CreatedAt,
                    CreatedBy = service.CreatedBy,
                    UpdatedAt = service.UpdatedAt,
                    UpdatedBy = service.UpdatedBy
                })),
                enReviewRetrieveResult.NotFound => NotFound($"No review found with ID {id}."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ GET REVIEWS BY BOOK ID ] ======================
        [HttpGet("BookReviews/{bookID}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Review>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<IEnumerable<Review>>> GetReviewsByBookID(int bookID)
        {
            if (bookID <= 0)
                return BadRequest("Invalid book ID.");

            var (result, reviews) = ReviewService.GetReviewsByBookID(bookID);

            return result switch
            {
                enReviewRetrieveResult.Found => reviews.Count == 0
                                                    ? NotFound("No reviews found for this book.")
                                                    : Ok(new ApiResponse<IEnumerable<Review>>("Reviews retrieved successfully.", reviews.OrderByDescending(r => r.CreatedAt))),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ GET MY REVIEWS ] ======================
        [HttpGet("MyReviews/{userID}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Review>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<IEnumerable<Review>>> GetMyReviews(int userID)
        {
            if (userID <= 0)
                return BadRequest("Invalid user ID.");

            var (result, reviews) = ReviewService.GetMyReviews(userID);

            return result switch
            {
                enReviewRetrieveResult.Found => reviews.Count == 0
                                                    ? NotFound("No reviews found for this user.")
                                                    : Ok(new ApiResponse<IEnumerable<Review>>("Reviews retrieved successfully.", reviews.OrderByDescending(r => r.CreatedAt))),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ CREATE REVIEW ] ======================
        [HttpPost("AddReview")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> CreateReview([FromBody] CreateReviewModel model)
        {
            if (model is null)
                return BadRequest("Invalid review data.");

            if (model.UserID <= 0)
                return BadRequest("Invalid user ID.");

            if (model.BookID <= 0)
                return BadRequest("Invalid book ID.");

            if (model.Rating < 1 || model.Rating > 5)
                return BadRequest("Rating must be between 1 and 5.");

            // TODO: replace with logged-in user ID after JWT is added
            int callerUserID = 1;

            var service = new ReviewService
            {
                UserID = model.UserID,
                BookID = model.BookID,
                Rating = model.Rating,
                Comment = model.Comment
            };

            enReviewSaveResult result = service.Save(callerUserID);

            return result switch
            {
                enReviewSaveResult.Saved => CreatedAtRoute("GetReviewByID",
                                                        new { id = service.ReviewID },
                                                        new ApiResponse<object>("Review created successfully.", new { service.ReviewID })),
                enReviewSaveResult.BookNotFound => NotFound($"No book found with ID {model.BookID}."),
                enReviewSaveResult.UserNotFound => NotFound($"No user found with ID {model.UserID}."),
                enReviewSaveResult.NotBorrowed => Conflict("You can only review a book you have borrowed and returned."),
                enReviewSaveResult.AlreadyReviewed => Conflict("You have already reviewed this book."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ UPDATE REVIEW ] ======================
        [HttpPut("UpdateReview/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> UpdateReview(int id, [FromBody] UpdateReviewModel model)
        {
            if (id <= 0)
                return BadRequest("Invalid review ID.");

            if (model is null)
                return BadRequest("Invalid review data.");

            if (model.Rating < 1 || model.Rating > 5)
                return BadRequest("Rating must be between 1 and 5.");

            var (findResult, service) = ReviewService.Find(id);

            if (findResult == enReviewRetrieveResult.NotFound)
                return NotFound($"No review found with ID {id}.");

            if (findResult == enReviewRetrieveResult.Failed)
                return StatusCode(500, "Something went wrong.");

            // TODO: replace with logged-in user ID after JWT is added
            int callerUserID = 1;

            service!.Rating = model.Rating;
            service.Comment = model.Comment;

            enReviewSaveResult result = service.Save(callerUserID);

            return result switch
            {
                enReviewSaveResult.Saved => Ok(new ApiResponse<object>("Review updated successfully.", new { id })),
                enReviewSaveResult.NotFound => NotFound($"No review found with ID {id}."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ DELETE REVIEW ] ======================
        [HttpDelete("DeleteReview/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> DeleteReview(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid review ID.");

            enReviewDeleteResult result = ReviewService.Delete(id);

            return result switch
            {
                enReviewDeleteResult.Deleted => Ok(new ApiResponse<object>($"Review with ID [{id}] deleted successfully.", new { id })),
                enReviewDeleteResult.NotFound => NotFound($"No review found with ID {id}."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }
    }
}