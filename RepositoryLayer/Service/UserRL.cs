using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace RepositoryLayer.Service
{
    public class UserRL : IUserRL
    {
        private readonly GreetingDBContext _context;

        public UserRL(GreetingDBContext context)
        {
            _context = context;
        }

        public void RegisterUser(UserEntity user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public UserEntity LoginUser(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }
    }
}
