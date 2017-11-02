using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Beursspel.Middleware
{
    public class CheckIfOpenMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckIfOpenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private readonly string[] AllowedPaths = new[]
        {
            "/account/login",
            "/account/register",
        };

        public async Task Invoke(HttpContext httpContext)
        {
            if (!Settings.IsOpen)
            {
                var path = httpContext.Request.Path.ToString().ToLower();
                if (path != "/home/gesloten" && !AllowedPaths.Contains(path))
                {
                    if (!httpContext.User.IsInRole("Admin") && !httpContext.User.IsInRole("Axipr"))
                    {
                        httpContext.Response.Redirect(!Startup.IsDevelopment
                            ? $"https://axiprbeursspel.nl/Home/Gesloten"
                            : $"http://localhost:5000/Home/Gesloten");
                    }
                }
            }
            await _next(httpContext);
        }

    }
}