// BookVault.Presentation/Requests/User/UpdatePasswordRequest.cs
namespace BookVault.Presentation.Requests.User
{
    public class UpdatePasswordRequest
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}