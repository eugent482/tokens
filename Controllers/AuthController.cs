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
            new User { Id = 1, FirstName = "Test", LastName = "User", Username = "test", Password = "test" }
        };


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
            new Claim(ClaimTypes.Name, user.Id.ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("uyruhdsjkfhjkhvnbxcnbvsdhfgdhsgfsdgfh"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;


            var token = new JwtSecurityToken(
                issuer: "https://localhost:44357",
               // audience: "https://localhost:44357",
                notBefore:now,
                claims: claims,
                expires:now.AddMinutes(5),
                signingCredentials: creds);
           

            return Ok(new
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(token),
                refresh_token="hello"
            });

        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public IActionResult Refresh(string access,string refresh)
        {
            


            return Ok();
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