using BusinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using System.Security.Cryptography;
using System.Text;

namespace HelloGreetingApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserBL _userBL;

        public UserController(IUserBL userBL)
        {
            _userBL = userBL;
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
                PasswordHash = hashedPassword,
                Salt = salt
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

            var hashedPassword = HashPassword(model.Password, user.Salt);
            if (user.PasswordHash != hashedPassword)
            {
                return Unauthorized("Invalid email or password.");
            }

            return Ok("Login successful.");
        }

        // Placeholder for forget password and reset password APIs
        [HttpPost]
        [Route("forget-password")]
        public IActionResult ForgetPassword([FromBody] ForgetPasswordModel model)
        {
           
            return Ok("Forget password logic not implemented.");
        }

        [HttpPost]
        [Route("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordModel model)
        {
           
            return Ok("Reset password logic not implemented.");
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
