using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resource.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : Controller
    {
        private readonly SignInManager<IdentityUser<Guid>> signInManager;
        public ProfileController(SignInManager<IdentityUser<Guid>> signInManager)
        {
            this.signInManager = signInManager;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            const string provider = Extensions.MyAuthServerDefaults.AuthenticationScheme;
            string redirectUrl = Url.Action(nameof(LoginCallback), "Profile", null, Request.Scheme, "localhost:44344");
            var props = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(props, provider);
        }

        [HttpGet("login/callback")]
        public async Task<IActionResult> LoginCallback()
        {
            var info = await signInManager.GetExternalLoginInfoAsync();
            return Ok();
        }
    }
}
