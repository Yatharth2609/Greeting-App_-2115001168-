using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Interface;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace BusinessLayer.Service
{
    public class UserBL : IUserBL
    {
        private readonly IUserRL _userRL;

        public UserBL(IUserRL userRL)
        {
            _userRL = userRL;
        }

        public void RegisterUser(UserEntity user)
        {
            _userRL.RegisterUser(user);
        }

        public UserEntity LoginUser(string email)
        {
            return _userRL.LoginUser(email);
        }

        public async Task<bool> ForgetPasswordAsync(string email)
        {
            return await _userRL.ForgetPasswordAsync(email);
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            return await _userRL.ResetPasswordAsync(token, newPassword);
        }
    }
}
