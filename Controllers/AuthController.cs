using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using InternetHospital.WebApi.Auth;
using InternetHospital.WebApi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using static InternetHospital.WebApi.Entities.PostgreContext;
namespace InternetHospital.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {


        private readonly AppSettings _appSettings;
        private ApplicationContext _context;
        private TokenService tokenService;

        public AuthController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _context = new ApplicationContext();
            tokenService = new TokenService(_context, _appSettings);
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public IActionResult SignIn(LoginForm form)
        {
            User user = _context.Users.FirstOrDefault(x => x.Username == form.username && x.Password == form.password);

           
            if (user == null)
                return NotFound(new { message = "Username or password is incorrect" });

            
            return Ok(new
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(tokenService.GenerateAccessToken(user)),
                refresh_token = tokenService.GenerateRefreshToken(user).Token,
                user_id = user.Id,
                user_email = user.FirstName
            });

        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public IActionResult Refresh(RToken refresh)
        {
            var refreshedToken=tokenService.RefreshTokinValidation(refresh.Refresh);

            if (refreshedToken == null)
                return BadRequest("invalid_grant");


            User user = _context.Users.Find(refreshedToken.UserId);

            return Ok(new
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(tokenService.GenerateAccessToken(user)),
                refresh_token = refreshedToken.Token,
                user_id = user.Id,
                user_email = user.FirstName
            });

        }


        [AllowAnonymous]
        [HttpGet("getallfree")]
        public IActionResult GetAllFree()
        {
            return Ok(_context.Users.ToList());
        }


       // [Authorize(Policy = "StatPolicy",Roles =("Admin"))]
        //[Authorize(Roles ="Admin")]
        [HttpGet("getallprotected")]
        public IActionResult GetAllProtected()
        {

            var identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;
            foreach (var item in claims)
            {
                Debug.WriteLine(item);
            }


            var user = _context.Users.FirstOrDefault(x => x.Id == int.Parse(User.Identity.Name));


            if (user.Role=="Admin")
                return Ok(_context.Users.ToList());
            else 
                return Ok(_context.Users.Skip(1).ToList());
            //else
                return Ok(_context.Users.Skip(2).ToList());
        }
    }
    public class StatusRequirement : IAuthorizationRequirement
    {
        public string NeedStatus { get; private set; }

        public StatusRequirement(string status)
        {
            this.NeedStatus = status;
        }
    }

    public class StatusHandler 
    : AuthorizationHandler<StatusRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            StatusRequirement requirement)
        {
            var employmentCommenced = context.User
                .FindFirst(claim => claim.Type == ClaimTypes.GroupSid).Value;

         

            if (employmentCommenced == requirement.NeedStatus)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}