using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Dashboard;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;

namespace Beursspel.Models.AccountViewModels
{
    public class HangfireAuthentication : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.IsInRole("Admin");
        }
    }
}