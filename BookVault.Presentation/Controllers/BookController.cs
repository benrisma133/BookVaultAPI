// BookVault.Presentation/Controllers/BookController.cs
using BookVault.Presentation.ApiResponses;
using BookVault.Repository.Models.BookModels;
using BookVault.Service.Enums.Book;
using BookVault.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookVault.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        // ====================== [ GET ALL BOOKS ] ======================
        [AllowAnonymous]
        [HttpGet("AllBooks")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Book>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<IEnumerable<Book>>> GetAllBooks()
        {
            var (result, books) = BookService.GetAll();

            return result switch
            {
                enBookRetrieveResult.Found => books.Count == 0
                                                    ? NotFound("No books found.")
                                                    : Ok(new ApiResponse<IEnumerable<Book>>("Books retrieved successfully.", books.OrderByDescending(b => b.CreatedAt))),
                enBookRetrieveResult.NotFound => NotFound("No books found."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }



        // ====================== [ GET BOOK BY ID ] ======================
        [AllowAnonymous]
        [HttpGet("GetBook/{id}", Name = "GetBookByID")]
        [ProducesResponseType(typeof(ApiResponse<Book>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<Book>> GetBookByID(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid book ID.");

            var (result, service) = BookService.Find(id);

            return result switch
            {
                enBookRetrieveResult.Found => Ok(new ApiResponse<Book>("Book retrieved successfully.", new Book
                {
                    BookID = service!.BookID,
                    Title = service.Title,
                    Author = service.Author,
                    Genre = service.Genre,
                    Description = service.Description,
                    TotalStock = service.TotalStock,
                    AvailableStock = service.AvailableStock,
                    CreatedAt = service.CreatedAt,
                    CreatedBy = service.CreatedBy,
                    UpdatedAt = service.UpdatedAt,
                    UpdatedBy = service.UpdatedBy
                })),
                enBookRetrieveResult.NotFound => NotFound($"No book found with ID {id}."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }



        // ====================== [ ADD BOOK ] ======================
        [Authorize(Roles = "Admin")]
        [HttpPost("AddBook")]
        [ProducesResponseType(typeof(ApiResponse<Book>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<Book>> AddBook([FromBody] AddBookModel model)
        {
            if (model is null)
                return BadRequest("Invalid book data.");

            if (string.IsNullOrWhiteSpace(model.Title) ||
                string.IsNullOrWhiteSpace(model.Author) ||
                string.IsNullOrWhiteSpace(model.Genre) ||
                model.TotalStock <= 0)
                return BadRequest("Title, Author, Genre and TotalStock are required.");

            var claimUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(claimUserID, out int callerUserID))
                return Unauthorized("Invalid token.");

            var service = new BookService
            {
                Title = model.Title,
                Author = model.Author,
                Genre = model.Genre,
                Description = model.Description,
                TotalStock = model.TotalStock
            };

            enBookSaveResult result = service.Save(callerUserID);

            return result switch
            {
                enBookSaveResult.Saved => CreatedAtRoute("GetBookByID",
                                                    new { id = service.BookID },
                                                    new ApiResponse<object>("Book created successfully.", new { service.BookID })),
                enBookSaveResult.DuplicateTitle => Conflict($"A book with the title '{model.Title}' already exists."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }



        // ====================== [ UPDATE BOOK ] ======================
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateBook/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> UpdateBook(int id, [FromBody] UpdateBookModel model)
        {
            if (id <= 0)
                return BadRequest("Invalid book ID.");

            if (model is null)
                return BadRequest("Invalid book data.");

            if (string.IsNullOrWhiteSpace(model.Title) ||
                string.IsNullOrWhiteSpace(model.Author) ||
                string.IsNullOrWhiteSpace(model.Genre) ||
                model.TotalStock <= 0)
                return BadRequest("Title, Author, Genre and TotalStock are required.");

            var (findResult, service) = BookService.Find(id);

            if (findResult == enBookRetrieveResult.NotFound)
                return NotFound($"No book found with ID {id}.");

            if (findResult == enBookRetrieveResult.Failed)
                return StatusCode(500, "Something went wrong.");


            var claimUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(claimUserID, out int callerUserID))
                return Unauthorized("Invalid token.");

            service!.Title = model.Title;
            service.Author = model.Author;
            service.Genre = model.Genre;
            service.Description = model.Description;
            service.TotalStock = model.TotalStock;

            enBookSaveResult result = service.Save(callerUserID);

            return result switch
            {
                enBookSaveResult.Saved => Ok(new ApiResponse<object>("Book updated successfully.", new { id })),
                enBookSaveResult.DuplicateTitle => Conflict($"A book with the title '{model.Title}' already exists."),
                enBookSaveResult.NotFound => NotFound($"No book found with ID {id}."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }



        // ====================== [ DELETE BOOK ] ======================
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteBook/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> DeleteBook(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid book ID.");

            enBookDeleteResult result = BookService.Delete(id);

            return result switch
            {
                enBookDeleteResult.Deleted => Ok(new ApiResponse<object>($"Book with ID [{id}] deleted successfully.", new { id })),
                enBookDeleteResult.NotFound => NotFound($"No book found with ID {id}."),
                enBookDeleteResult.HasActiveBorrows => Conflict($"Book with ID [{id}] cannot be deleted because it has active borrows."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }
    }
}