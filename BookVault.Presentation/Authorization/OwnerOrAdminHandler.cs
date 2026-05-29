using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookVault.Presentation.Authorization
{
    // One handler for all resources.
    // Admin can access anything.
    // Member can only access resources they own.
    // The resource owner ID is passed from the controller.
    public class OwnerOrAdminHandler
        : AuthorizationHandler<OwnerOrAdminRequirement, int>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OwnerOrAdminRequirement requirement,
            int ownerID)
        {
            // Admin override — full access to any resource
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Ownership check — authenticated user must match the resource owner
            var claimUserID = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(claimUserID, out int authenticatedUserID) &&
                authenticatedUserID == ownerID)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
