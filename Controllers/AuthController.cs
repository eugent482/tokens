using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using InternetHospital.WebApi.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InternetHospital.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        
        private List<User> _users = new List<User>
        {
            new User { Id = 1, FirstName = "Test", LastName = "User", Username = "test", Password = "test",Role="Admin" }
        };
        private List<RefreshToken> _tokens;
        private readonly AppSettings _appSettings;



        public AuthController(IOptions<AppSettings> appSettings)
        {
            _tokens = new List<RefreshToken>();
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public IActionResult SignIn(string userf, string password)
        {
            User user = null;
            //User authentication
            try
            {
                user = _users.SingleOrDefault(x => x.Username == userf && x.Password == password);
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Something bad" });
            }
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });




            var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresDate = DateTime.UtcNow.AddDays(15),
                Revoked=false,
                User=user // TODO delete
                
            };
            _tokens.Add(newRefreshToken);


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;


            var token = new JwtSecurityToken(
                issuer: _appSettings.JwtIssuer,
                notBefore:now,
                claims: claims,
                expires:now.AddMinutes(_appSettings.JwtExpireMinutes),
                signingCredentials: creds);


            return Ok(new
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(token),
                refresh_token = newRefreshToken.Token,
                user_id=user.Id
            });

        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public IActionResult Refresh(string refresh)
        {

            var rtoken = _tokens.FirstOrDefault(x => x.Token == refresh);

            if (rtoken == null)
                return BadRequest();

            if (rtoken.ExpiresDate < DateTime.UtcNow)
                return Unauthorized();

            if (rtoken.Revoked==true)
                return Unauthorized();       



            var claims = new[]
        {
            new Claim(ClaimTypes.Name, rtoken.User.Id.ToString()),
            new Claim(ClaimTypes.Role, rtoken.User.Role)
        };

           


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;


            var token = new JwtSecurityToken(
                issuer: _appSettings.JwtIssuer,
                notBefore: now,
                claims: claims,
                expires: now.AddMinutes(_appSettings.JwtExpireMinutes),
                signingCredentials: creds);


            return Ok(new
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(token),
                refresh_token = rtoken.Token,
                user_id = rtoken.User.Id
            });
        }


        [AllowAnonymous]
        [HttpGet("getallfree")]
        public IActionResult GetAllFree()
        {           
            return Ok(_users);
        }

        [HttpGet("getallprotected")]
        public IActionResult GetAllProtected()
        {
            return Ok(_users);
        }
    }
}