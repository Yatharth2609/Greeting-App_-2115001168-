using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using Microsoft.Extensions.Configuration;
using ModelLayer.Model;

namespace RepositoryLayer.Service
{
    public class UserRL : IUserRL
    {
        private readonly GreetingDBContext _context;
        private readonly IConfiguration _configuration;

        public UserRL(GreetingDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public bool RegisterUser(UserEntity user)
        {
            var ExistingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (ExistingUser == null) 
            {
                var hashedPassword = HashPassword(user.PasswordHash, "10");
                UserEntity newUser = new UserEntity()
                {
                    Email = user.Email,
                    PasswordHash = hashedPassword,
                    UserName = user.UserName,
                };
                _context.Users.Add(newUser);
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        public string LoginUser(UserLoginModel user)
        {
            var exist = _context.Users.FirstOrDefault(e => e.Email == user.Email);

            if (exist == null) 
            {
                return null;
            }

            var hashedPassword = HashPassword(user.Password, "10");
            if (hashedPassword == exist.PasswordHash) 
            {
                return GenerateJwtToken(user.Email, exist.Id);
            }

            return "Error";
        }

        public async Task<bool> ForgetPasswordAsync(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            var token = GenerateJwtToken(email, user.Id);
            var resetLink = $"https://yourapp.com/reset-password?token={token}";

            var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
            {
                Port = int.Parse(_configuration["Smtp:Port"]),
                Credentials = new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Smtp:From"]),
                Subject = "Reset Password",
                Body = $"Please reset your password using the following link: {resetLink}",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            if (principal == null)
            {
                return false;
            }

            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            user.PasswordHash = HashPassword(newPassword, "10");
            _context.SaveChanges();

            return true;
        }

        private string GenerateJwtToken(string email, int Id)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, Id.ToString()) // Ensure ID is stored properly
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
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
