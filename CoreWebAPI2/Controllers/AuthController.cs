using CoreWebAPI2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using CoreWebAPI2.Helpers;

namespace CoreWebAPI2.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;

        public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register([FromBody]RegistrationModel user)
        {
            if(!ModelState.IsValid || user == null)
            {
                return new BadRequestObjectResult(new { Message = "User registration failed" });
            }

            var newUser = new ApplicationUser()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email
            };

            var result = await userManager.CreateAsync(newUser, user.Password);

            if (!result.Succeeded)
            {
                var dictionary = new ModelStateDictionary();
                foreach (IdentityError error in result.Errors)
                {
                    dictionary.AddModelError(error.Code, error.Description);
                }

                return new BadRequestObjectResult(new { Message = "User registration failed", Errors = dictionary });
            }

            return Ok(new { Message = "User registration success" });
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login([FromBody]LoginModel loginCredentials)
        {
            if (!ModelState.IsValid || loginCredentials == null)
            {
                return new BadRequestObjectResult(new { Message = "Login failed" });
            }

            // verify user by email
            var userName = loginCredentials.Username;
            if (Validations.IsValidEmail(userName))
            {
                var user = await userManager.FindByEmailAsync(loginCredentials.Username);
                if (user != null)
                {
                    userName = user.UserName;
                } else return new BadRequestObjectResult(new { Message = "Login failed" });
            }

            // verify user by username
            var loginUser = await userManager.FindByNameAsync(userName);
            if (loginUser == null)
            {
                return new BadRequestObjectResult(new { Message = "Login failed" });
            }

            // verify input password if same as hashed password
            var verifiedPassword = userManager.PasswordHasher.VerifyHashedPassword(loginUser, loginUser.PasswordHash, loginCredentials.Password);
            if (verifiedPassword == PasswordVerificationResult.Failed)
            {
                return new BadRequestObjectResult(new { Message = "Login failed" });
            }

            var token = JwtToken.GetAccessToken(loginUser.Id);

            // create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, loginUser.Email),
                new Claim(ClaimTypes.Name, loginUser.UserName),
                new Claim(ClaimTypes.NameIdentifier, loginUser.Id),
                new Claim("access_token", token)
            };

            // create identity
            var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // create claims principal
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            bool? persistent = loginCredentials.RememberMe | false;

            Response.Cookies.Append("Bearer", token);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal,
                new AuthenticationProperties { 
                    IsPersistent = (bool)persistent,
                    ExpiresUtc = DateTimeOffset.Now.AddHours(1.0)
                });

            return Ok(new { Message = "Login success", token });
        }

        [HttpPost]
        [Route("logout")]
        public async Task<ActionResult> Logout()
        {
            Response.Cookies.Delete("Bearer");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(new { Message = "Logout success" });
        }
    }
}
