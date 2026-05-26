// BookVault.Service/Enums/User/enUserRoleResult.cs
namespace BookVault.Service.Enums.User
{
    public enum enUserRoleResult
    {
        Promoted,
        Demoted,
        AlreadyAdmin,
        AlreadyMember,
        CannotDemoteSelf,
        NotFound,
        Failed
    }
}