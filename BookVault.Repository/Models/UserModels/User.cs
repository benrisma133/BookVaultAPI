// BookVault.Repository/Models/UserModels/User.cs
namespace BookVault.Repository.Models.UserModels
{
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public byte Role { get; set; }
        public int Permissions { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RegisterModel
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UpdateUserModel
    {
        public string Name { get; set; } = null!;
    }

    public class RefreshToken
    {
        public int RefreshTokenID { get; set; }
        public int UserID { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsRevoked { get; set; }
    }

    public class SaveRefreshTokenModel
    {
        public int UserID { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }

    public class UpdateEmailModel
    {
        public string NewEmail { get; set; } = null!;
    }

    public class UpdatePasswordModel
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class UpdatePermissionsModel
    {
        public int Permissions { get; set; }
    }
}