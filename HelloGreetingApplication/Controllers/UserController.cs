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
        public IActionResult Register([FromBody] UserEntity user)
        {
            ResponseBody<UserEntity> response = new ResponseBody<UserEntity>();
            bool isRegistered = _userBL.RegisterUser(user);
            if (isRegistered) 
            {
                response.Success = true;
                response.Message = "User registered successfully.";
                response.Data = user;
            } else
            {
                response.Success = false;
                response.Message = "OOPS Something Went Wrong";
                response.Data = null;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] UserLoginModel model)
        {
            string user = _userBL.LoginUser(model);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }
            ResponseBody<string> response = new ResponseBody<string>();
            response.Success = true;
            response.Message = "User Logged in Successfully";
            response.Data = user;
            return Ok(response);
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
    }
}
