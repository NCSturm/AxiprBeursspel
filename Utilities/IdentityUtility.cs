using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Beursspel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Beursspel.Utilities
{
    public static class IdentityUtility
    {
        public static async Task<ApplicationUser> GetCurrentUser(this UserManager<ApplicationUser> manager,
            HttpContext httpContext)
        {
            return await manager.GetUserAsync(httpContext.User);
        }
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}