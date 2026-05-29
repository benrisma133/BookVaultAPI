// BookVault.Presentation/Controllers/UserController.cs
using BookVault.Presentation.ApiResponses;
using BookVault.Repository.Models.UserModels;
using BookVault.Service.Enums.User;
using BookVault.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookVault.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // ====================== [ GET ALL USERS ] ======================
        [Authorize(Roles = "Admin")]
        [HttpGet("AllUsers")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<User>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<IEnumerable<User>>> GetAllUsers()
        {
            var (result, users) = UserService.GetAll();

            return result switch
            {
                enUserRetrieveResult.Found => users.Count == 0
                                                ? NotFound("No users found.")
                                                : Ok(new ApiResponse<IEnumerable<User>>("Users retrieved successfully.", users.OrderByDescending(u => u.CreatedAt))),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ GET USER BY ID ] ======================
        [Authorize(Roles = "Admin,Member")]
        [HttpGet("GetUser/{id}", Name = "GetUserByID")]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<User>> GetUserByID(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid user ID.");

            var (result, service) = UserService.Find(id);

            return result switch
            {
                enUserRetrieveResult.Found => Ok(new ApiResponse<User>("User retrieved successfully.", new User
                {
                    UserID = service!.UserID,
                    Name = service.Name,
                    Email = service.Email,
                    PasswordHash = service.PasswordHash,
                    Role = service.Role,
                    Permissions = service.Permissions,
                    CreatedAt = service.CreatedAt
                })),
                enUserRetrieveResult.NotFound => NotFound($"No user found with ID {id}."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ UPDATE USER ] ======================
        [Authorize(Roles = "Admin,Member")]
        [HttpPut("UpdateUser/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> UpdateUser(int id, [FromBody] UpdateUserModel model)
        {
            if (id <= 0)
                return BadRequest("Invalid user ID.");

            if (model is null || string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Name is required.");

            var (findResult, service) = UserService.Find(id);

            if (findResult == enUserRetrieveResult.NotFound)
                return NotFound($"No user found with ID {id}.");

            if (findResult == enUserRetrieveResult.Failed)
                return StatusCode(500, "Something went wrong.");

            service!.Name = model.Name;

            enUserSaveResult result = service.Save();

            return result switch
            {
                enUserSaveResult.Saved => Ok(new ApiResponse<object>("User updated successfully.", new { id })),
                enUserSaveResult.NotFound => NotFound($"No user found with ID {id}."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ UPDATE EMAIL ] ======================
        [Authorize(Roles = "Admin,Member")]
        [HttpPatch("UpdateEmail/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> UpdateEmail(int id, [FromBody] UpdateEmailModel model)
        {
            if (id <= 0)
                return BadRequest("Invalid user ID.");

            if (model is null || string.IsNullOrWhiteSpace(model.NewEmail))
                return BadRequest("New email is required.");

            var (findResult, service) = UserService.Find(id);

            if (findResult == enUserRetrieveResult.NotFound)
                return NotFound($"No user found with ID {id}.");

            if (findResult == enUserRetrieveResult.Failed)
                return StatusCode(500, "Something went wrong.");

            enUserSaveResult result = service!.UpdateEmail(model.NewEmail);

            return result switch
            {
                enUserSaveResult.Saved => Ok(new ApiResponse<object>("Email updated successfully.", new { id })),
                enUserSaveResult.EmailTaken => Conflict("This email is already taken."),
                enUserSaveResult.NotFound => NotFound($"No user found with ID {id}."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ UPDATE PASSWORD ] ======================
        [Authorize(Roles = "Admin,Member")]
        [HttpPatch("UpdatePassword/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> UpdatePassword(int id, [FromBody] UpdatePasswordModel model)
        {
            if (id <= 0)
                return BadRequest("Invalid user ID.");

            if (model is null)
                return BadRequest("Invalid data.");

            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                return BadRequest("Current password is required.");

            if (string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest("New password is required.");

            if (model.CurrentPassword == model.NewPassword)
                return BadRequest("New password must be different from current password.");

            var (findResult, service) = UserService.Find(id);

            if (findResult == enUserRetrieveResult.NotFound)
                return NotFound($"No user found with ID {id}.");

            if (findResult == enUserRetrieveResult.Failed)
                return StatusCode(500, "Something went wrong.");

            enUserPasswordResult result = service!.UpdatePassword(model.CurrentPassword, model.NewPassword);

            return result switch
            {
                enUserPasswordResult.Updated => Ok(new ApiResponse<object>("Password updated successfully.", new { id })),
                enUserPasswordResult.InvalidCurrentPassword => BadRequest("Current password is incorrect."),
                enUserPasswordResult.NotFound => NotFound($"No user found with ID {id}."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ PROMOTE TO ADMIN ] ======================
        [Authorize(Roles = "Admin")]
        [HttpPatch("Promote/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> PromoteToAdmin(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid user ID.");

            enUserRoleResult result = UserService.PromoteToAdmin(id);

            return result switch
            {
                enUserRoleResult.Promoted => Ok(new ApiResponse<object>($"User with ID [{id}] promoted to Admin successfully.", new { id })),
                enUserRoleResult.NotFound => NotFound($"No user found with ID {id}."),
                enUserRoleResult.AlreadyAdmin => Conflict($"User with ID [{id}] is already an Admin."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ DEMOTE TO MEMBER ] ======================
        [Authorize(Roles = "Admin")]
        [HttpPatch("Demote/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> DemoteToMember(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid user ID.");

            var claimUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(claimUserID, out int callerUserID))
                return Unauthorized("Invalid token.");

            enUserRoleResult result = UserService.DemoteToMember(id, callerUserID);

            return result switch
            {
                enUserRoleResult.Demoted => Ok(new ApiResponse<object>($"User with ID [{id}] demoted to Member successfully.", new { id })),
                enUserRoleResult.NotFound => NotFound($"No user found with ID {id}."),
                enUserRoleResult.AlreadyMember => Conflict($"User with ID [{id}] is already a Member."),
                enUserRoleResult.CannotDemoteSelf => BadRequest("You cannot demote yourself."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ UPDATE PERMISSIONS ] ======================
        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdatePermissions/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> UpdatePermissions(int id, [FromBody] UpdatePermissionsModel model)
        {
            if (id <= 0)
                return BadRequest("Invalid user ID.");

            if (model is null || model.Permissions < 0)
                return BadRequest("Invalid permissions value.");

            enUserSaveResult result = UserService.UpdatePermissions(id, model.Permissions);

            return result switch
            {
                enUserSaveResult.Saved => Ok(new ApiResponse<object>("Permissions updated successfully.", new { id, model.Permissions })),
                enUserSaveResult.NotFound => NotFound($"No user found with ID {id}."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }

        // ====================== [ DELETE USER ] ======================
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteUser/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> DeleteUser(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid user ID.");

            var claimUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(claimUserID, out int callerUserID))
                return Unauthorized("Invalid token.");

            enUserDeleteResult result = UserService.Delete(id, callerUserID);

            return result switch
            {
                enUserDeleteResult.Deleted => Ok(new ApiResponse<object>($"User with ID [{id}] deleted successfully.", new { id })),
                enUserDeleteResult.NotFound => NotFound($"No user found with ID {id}."),
                enUserDeleteResult.CannotDeleteSelf => BadRequest("You cannot delete yourself."),
                _ => StatusCode(500, "Something went wrong.")
            };
        }
    }
}