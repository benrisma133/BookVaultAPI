// BookVault.Service/Enums/User/enUserDeleteResult.cs
namespace BookVault.Service.Enums.User
{
    public enum enUserDeleteResult
    {
        Deleted,
        NotFound,
        CannotDeleteSelf,
        Failed
    }
}