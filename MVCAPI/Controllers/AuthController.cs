﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace MVCAPI.Controllers
{
    //[Produces("application/json")]
    //some thing
    [Route("[controller]/[action]")]
    public class AuthController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
          IConfiguration config,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }


        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider = "Facebook", string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                //return RedirectToAction(nameof(Login));
                return BadRequest();
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // return RedirectToAction(nameof(Login));
                return BadRequest();
            }
          
            var clams = info.Principal.Claims;
            //  var res = clams.ToList();
            var nameidentifier = clams.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var emailaddress = clams.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;
            var name = clams.FirstOrDefault(x => x.Type == ClaimTypes.Name).Value;
            var givenname = clams.FirstOrDefault(x => x.Type == ClaimTypes.GivenName).Value;
            var surname = clams.FirstOrDefault(x => x.Type == ClaimTypes.Surname).Value;

            var claims = new[]
            {
              new Claim(JwtRegisteredClaimNames.Email, emailaddress),
              new Claim(JwtRegisteredClaimNames.NameId,nameidentifier),
              new Claim(JwtRegisteredClaimNames.GivenName,givenname),
              new Claim(JwtRegisteredClaimNames.FamilyName,surname),  
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0123456789ABCDEF"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken("http://bipinpaul.com.np",
                  "http://bipinpaul.com.np",
                  claims,
                  expires: DateTime.Now.AddDays(30),
                  signingCredentials: creds);
                return Ok(token);
        }
    }
}
