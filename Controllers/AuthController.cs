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


        public AuthController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _context = new ApplicationContext();
           

        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public IActionResult SignIn(LoginForm form)
        {
            User user = null;
            //User authentication
            try
            {

                user = _context.Users.FirstOrDefault(x => x.Username == form.username && x.Password == form.password);
                Debug.Write(form.username == "test");

            }
            catch (Exception)
            {
                return BadRequest(new { message = "Something bad" });
            }
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });




            var claims = new Claim[]
        {
            new Claim(ClaimTypes.Name, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresDate = DateTime.UtcNow.AddDays(15),
                Revoked = false
            };

            //
            _context.RefreshTokens.Add(newRefreshToken);
            _context.SaveChanges();



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
                refresh_token = newRefreshToken.Token,
                user_id = user.Id
            });

        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public IActionResult Refresh(RToken refresh)
        {
            RefreshToken rtoken = _context.RefreshTokens.FirstOrDefault(x => x.Token == refresh.Refresh);


            if (rtoken == null)
                return BadRequest("invalid_grant");

            if (rtoken.ExpiresDate < DateTime.UtcNow)
                return BadRequest("invalid_grant");

            if (rtoken.Revoked == true)
                return BadRequest("invalid_grant");

            User user = _context.Users.Find(rtoken.UserId);

            var claims = new[]{
            new Claim(ClaimTypes.Name, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
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

            string newrefreshtoken = Guid.NewGuid().ToString();
            rtoken.Token = newrefreshtoken;
            _context.SaveChanges();
            return Ok(new
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(token),
                refresh_token = rtoken.Token,
                user_id = rtoken.UserId
            });

        }


        [AllowAnonymous]
        [HttpGet("getallfree")]
        public IActionResult GetAllFree()
        {
            return Ok(_context.Users.ToList());
        }

        [HttpGet("getallprotected")]
        public IActionResult GetAllProtected()
        {
            return Ok(_context.Users.ToList());
        }
    }
}