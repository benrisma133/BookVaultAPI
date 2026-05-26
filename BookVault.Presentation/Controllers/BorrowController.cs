// BookVault.Presentation/Controllers/BorrowController.cs
using BookVault.Presentation.ApiResponses;
using BookVault.Repository.Models.BorrowModels;
using BookVault.Service.Enums.Borrow;
using BookVault.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVault.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController : ControllerBase
    {
        // ====================== [ GET ALL BORROWS ] ======================
        [HttpGet("AllBorrows")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Borrow>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<IEnumerable<Borrow>>> GetAllBorrows()
        {
            var (result, borrows) = BorrowService.GetAll();

            return result switch
            {
                enBorrowRetrieveResult.Found => borrows.Count == 0
                                                    ? NotFound("No borrows found.")
                                                    : Ok(new ApiResponse<IEnumerable<Borrow>>
                                                                ("Borrows retrieved successfully.", 
                                                                 borrows.OrderByDescending(b => b.BorrowedAt))),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ GET BORROW BY ID ] ======================
        [HttpGet("GetBorrow/{id}", Name = "GetBorrowByID")]
        [ProducesResponseType(typeof(ApiResponse<Borrow>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<Borrow>> GetBorrowByID(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid borrow ID.");

            var (result, service) = BorrowService.Find(id);

            return result switch
            {
                enBorrowRetrieveResult.Found => Ok(new ApiResponse<Borrow>("Borrow retrieved successfully.", new Borrow
                {
                    BorrowID = service!.BorrowID,
                    UserID = service.UserID,
                    UserName = service.UserName,
                    BookID = service.BookID,
                    BookTitle = service.BookTitle,
                    BorrowedAt = service.BorrowedAt,
                    DueDate = service.DueDate,
                    ReturnedAt = service.ReturnedAt,
                    Status = service.Status,
                    CreatedBy = service.CreatedBy
                })),
                enBorrowRetrieveResult.NotFound => NotFound($"No borrow found with ID {id}."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ GET MY BORROWS ] ======================
        [HttpGet("MyBorrows/{userID}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Borrow>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<IEnumerable<Borrow>>> GetMyBorrows(int userID)
        {
            if (userID <= 0)
                return BadRequest("Invalid user ID.");

            var (result, borrows) = BorrowService.GetMyBorrows(userID);

            return result switch
            {
                enBorrowRetrieveResult.Found => borrows.Count == 0
                                                    ? NotFound("No borrows found for this user.")
                                                    : Ok(new ApiResponse<IEnumerable<Borrow>>("Borrows retrieved successfully.", borrows.OrderByDescending(b => b.BorrowedAt))),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ GET ACTIVE BORROWS ] ======================
        [HttpGet("ActiveBorrows")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Borrow>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<IEnumerable<Borrow>>> GetActiveBorrows()
        {
            var (result, borrows) = BorrowService.GetActiveBorrows();

            return result switch
            {
                enBorrowRetrieveResult.Found => borrows.Count == 0
                                                    ? NotFound("No active borrows found.")
                                                    : Ok(new ApiResponse<IEnumerable<Borrow>>("Active borrows retrieved successfully.", borrows.OrderByDescending(b => b.BorrowedAt))),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ GET OVERDUE BORROWS ] ======================
        [HttpGet("OverdueBorrows")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Borrow>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<IEnumerable<Borrow>>> GetOverdueBorrows()
        {
            var (result, borrows) = BorrowService.GetOverdueBorrows();

            return result switch
            {
                enBorrowRetrieveResult.Found => borrows.Count == 0
                                                    ? NotFound("No overdue borrows found.")
                                                    : Ok(new ApiResponse<IEnumerable<Borrow>>("Overdue borrows retrieved successfully.", borrows.OrderByDescending(b => b.DueDate))),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ CREATE BORROW ] ======================
        [HttpPost("Borrow")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> CreateBorrow([FromBody] CreateBorrowModel model)
        {
            if (model is null)
                return BadRequest("Invalid borrow data.");

            if (model.UserID <= 0)
                return BadRequest("Invalid user ID.");

            if (model.BookID <= 0)
                return BadRequest("Invalid book ID.");

            if (model.DueDate <= DateTime.UtcNow)
                return BadRequest("Due date must be in the future.");

            // TODO: replace with logged-in user ID after JWT is added
            int callerUserID = 1;

            var (result, newBorrowID) = BorrowService.Create(model, callerUserID);

            return result switch
            {
                enBorrowCreateResult.Created => CreatedAtRoute("GetBorrowByID",
                                                            new { id = newBorrowID },
                                                            new ApiResponse<object>("Book borrowed successfully.", new { BorrowID = newBorrowID })),
                enBorrowCreateResult.BookNotFound => NotFound($"No book found with ID {model.BookID}."),
                enBorrowCreateResult.UserNotFound => NotFound($"No user found with ID {model.UserID}."),
                enBorrowCreateResult.OutOfStock => Conflict("This book is currently out of stock."),
                enBorrowCreateResult.AlreadyBorrowed => Conflict("This user already has an active borrow for this book."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ RETURN BORROW ] ======================
        [HttpPut("Return/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> ReturnBorrow(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid borrow ID.");

            // TODO: replace with logged-in user ID after JWT is added
            int callerUserID = 1;

            enBorrowReturnResult result = BorrowService.Return(id, callerUserID);

            return result switch
            {
                enBorrowReturnResult.Returned => Ok(new ApiResponse<object>("Book returned successfully.", new { BorrowID = id })),
                enBorrowReturnResult.NotFound => NotFound($"No borrow found with ID {id}."),
                enBorrowReturnResult.AlreadyReturned => Conflict($"Borrow with ID {id} has already been returned."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }
    }
}