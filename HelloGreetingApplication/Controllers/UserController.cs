using BusinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HelloGreetingApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserBL _userBL;
        private readonly IConfiguration _configuration;

        public UserController(IUserBL userBL, IConfiguration configuration)
        {
            _userBL = userBL;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register([FromBody] UserRegistratoinModel model)
        {
            var existingUser = _userBL.LoginUser(model.Email);
            if (existingUser != null)
            {
                return BadRequest("User already exists.");
            }

            var salt = GenerateSalt();
            var hashedPassword = HashPassword(model.Password, salt);

            var user = new UserEntity
            {
                Email = model.Email,
                PasswordHash = hashedPassword
            };

            _userBL.RegisterUser(user);

            return Ok("User registered successfully.");
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] UserLoginModel model)
        {
            var user = _userBL.LoginUser(model.Email);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var salt = GenerateSalt();
            var hashedPassword = HashPassword(model.Password, salt);
            if (user.PasswordHash != hashedPassword)
            {
                return Unauthorized("Invalid email or password.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost]
        [Route("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordModel model)
        {
            var result = await _userBL.ForgetPasswordAsync(model.Email);
            if (!result)
            {
                return BadRequest("User not found.");
            }

            return Ok("Password reset link has been sent to your email.");
        }

        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model, [FromQuery] string token)
        {
            var result = await _userBL.ResetPasswordAsync(token, model.NewPassword);
            if (!result)
            {
                return BadRequest("Invalid token or user not found.");
            }

            return Ok("Password has been reset successfully.");
        }

        private string GenerateJwtToken(UserEntity user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                       new Claim(ClaimTypes.Name, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var saltedPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);
                var hashBytes = sha256.ComputeHash(saltedPasswordBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
