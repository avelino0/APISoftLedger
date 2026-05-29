using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SoftLedger.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;


        private readonly List<(string Email, string Password)> _users = new()

        {

            ("admin@teste.com", "123456")

        };


        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] SoftLedger.Models.LoginRequest request)

        {
            var user = _users.FirstOrDefault(u =>
            u.Email == request.Email &&
            u.Password == request.Password

        );

            if (user == default)

            return Unauthorized("Email ou senha inválidos");

            var jwtKey = _configuration["JwtKey"] ??
                throw new InvalidOperationException("JwtKey configuration is required.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]

            {
                new Claim(ClaimTypes.Name, request.Email)
            };

            var token = new JwtSecurityToken(

                claims: claims,

                expires: DateTime.Now.AddHours(1),

                signingCredentials: creds);


            var jwt = new JwtSecurityTokenHandler().WriteToken(token);


            return Ok(new { token = jwt });

        }
    }
}
