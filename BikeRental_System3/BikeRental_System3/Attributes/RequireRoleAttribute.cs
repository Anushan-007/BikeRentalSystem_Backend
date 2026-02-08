using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BikeRental_System3.Models;

namespace BikeRental_System3.Attributes
{
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly Roles _requiredRole;

        public RequireRoleAttribute(Roles requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userRole = user.FindFirst("roles")?.Value;

            if (string.IsNullOrEmpty(userRole) || !Enum.TryParse<Roles>(userRole, out var role))
            {
                context.Result = new JsonResult(new { message = "Invalid or missing role in token." }) { StatusCode = 403 };
                return;
            }

            // Check if user has the required role or higher privileges
            if (!HasRequiredRole(role, _requiredRole))
            {
                context.Result = new JsonResult(new { message = $"Access denied. {_requiredRole} role required." }) { StatusCode = 403 };
            }
        }

        private bool HasRequiredRole(Roles userRole, Roles requiredRole)
        {
            // Role hierarchy: Admin (1) > Manager (2) > User (3)
            return (int)userRole <= (int)requiredRole;
        }
    }
}
