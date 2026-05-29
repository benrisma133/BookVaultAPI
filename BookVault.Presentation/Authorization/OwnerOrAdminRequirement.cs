using Microsoft.AspNetCore.Authorization;

namespace BookVault.Presentation.Authorization
{
    public class OwnerOrAdminRequirement : IAuthorizationRequirement
    {
    }
}
