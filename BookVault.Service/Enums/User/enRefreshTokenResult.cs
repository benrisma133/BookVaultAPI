// BookVault.Service/Enums/User/enRefreshTokenResult.cs
namespace BookVault.Service.Enums.User
{
    public enum enRefreshTokenResult
    {
        Saved,
        Valid,       // token exists, not revoked, not expired
        Revoked,
        NotFound,
        Expired,
        AlreadyRevoked,
        Failed
    }
}