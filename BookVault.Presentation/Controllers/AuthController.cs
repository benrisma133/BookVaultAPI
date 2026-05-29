// BookVault.Presentation/Controllers/AuthController.cs
using BookVault.Presentation.ApiResponses;
using BookVault.Repository.Models.UserModels;
using BookVault.Service.Enums.User;
using BookVault.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookVault.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        // ====================== [ LOGIN ] ======================
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApiResponse<object>> Login([FromBody] LoginModel model)
        {
            if (model is null)
                return BadRequest("Invalid login data.");

            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest("Email is required.");

            if (string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Password is required.");

            // Step 1: verify credentials in service (BCrypt check happens here)
            var (result, service) = UserService.Login(model);

            if (result == enUserLoginResult.InvalidCredentials)
                return Unauthorized("Invalid credentials.");

            if (result == enUserLoginResult.Failed)
                return StatusCode(500, "Something went wrong.");

            // Step 2: build claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, service!.UserID.ToString()),
                new Claim(ClaimTypes.Email,          service.Email),
                new Claim(ClaimTypes.Name,           service.Name),
                new Claim(ClaimTypes.Role, service.Role == 2 ? "Admin" : "Member"),
                new Claim("Permissions",             service.Permissions.ToString())
            };

            // Step 3: build signing key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Step 4: build access token
            int expiryMinutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"]!);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Step 5: return token
            return Ok(new ApiResponse<object>("Login successful.", new
            {
                AccessToken = accessToken,
                ExpiresIn = expiryMinutes * 60,
                UserID = service.UserID,
                Name = service.Name,
                Email = service.Email,
                Role = service.Role
            }));
        }
    }
}