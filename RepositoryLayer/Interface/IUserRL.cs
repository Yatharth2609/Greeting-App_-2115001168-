using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.Model;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface
{
    public interface IUserRL
    {
        bool RegisterUser(UserEntity user);
        string LoginUser(UserLoginModel user);
        Task<bool> ForgetPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}
