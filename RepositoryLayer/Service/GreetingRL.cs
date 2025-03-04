using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace RepositoryLayer.Service
{
    public class GreetingRL : IGreetingRL
    {
        private readonly GreetingDBContext _context;

        public GreetingRL(GreetingDBContext context)
        {
            _context = context;
        }

        public void SaveGreeting(GreetingEntity greeting)
        {
            _context.Greetings.Add(greeting);
            _context.SaveChanges();
        }

        public List<GreetingEntity> GetAllGreetings()
        {
            return _context.Greetings.ToList();
        }

        public GreetingEntity? GetGreetingById(int id)
        {
            return _context.Greetings.FirstOrDefault(greeting => greeting.Id == id);
        }

        public bool UpdateGreeting(int id, string NewMessage)
        {
            GreetingEntity greeting = _context.Greetings.FirstOrDefault(greeting => greeting.Id == id);
            if (greeting == null)
            {

                return false;
            }

            greeting.Message = NewMessage;
            _context.SaveChanges();
            return true;
        }
    }
}
