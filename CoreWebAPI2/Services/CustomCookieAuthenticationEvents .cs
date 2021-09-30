using CoreWebAPI2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWebAPI2.Services
{
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly ApplicationDbContext dbContext;

        public CustomCookieAuthenticationEvents(ApplicationDbContext dbContext)
        {
            // Get the database from registered DI services.
            this.dbContext = dbContext;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;

            var accessToken = userPrincipal?.Claims
                   .FirstOrDefault(c => c.Type == "access_token");

            if (accessToken == null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            System.Diagnostics.Debug.WriteLine("Token from cookie auth", accessToken.Value);

            // Look for the LastChanged claim.
            //var lastChanged = (from c in userPrincipal.Claims
            //                   where c.Type == "LastChanged"
            //                   select c.Value).FirstOrDefault();

            //if (string.IsNullOrEmpty(lastChanged))
            //{
            //    context.RejectPrincipal();

            //    await context.HttpContext.SignOutAsync(
            //        CookieAuthenticationDefaults.AuthenticationScheme);
            //}
        }
    }
}
