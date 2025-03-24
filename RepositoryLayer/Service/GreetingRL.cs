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

        public void SaveGreeting(GreetingEntity greeting, int userId)
        {
            try
            {
                Console.WriteLine($"Saving Greeting: {greeting.FirstName} {greeting.LastName} (User: {userId})");

                greeting.UserId = userId;
                _context.Greetings.Add(greeting);
                _context.SaveChanges();

                Console.WriteLine("Greeting saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to DB: {ex.Message}");
                throw;
            }
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
            GreetingEntity greeting = _context.Greetings.FirstOrDefault(greeting => greeting.UserId == id);
            if (greeting == null)
            {

                return false;
            }

            greeting.Message = NewMessage;
            _context.SaveChanges();
            return true;
        }

        public bool DeleteGreeting(int id)
        {
            var greeting = _context.Greetings.FirstOrDefault(g => g.Id == id);
            if(greeting == null)
            {
                return false;
            }

            _context.Greetings.Remove(greeting);
            _context.SaveChanges(true);
            return true;
        }
    }
}
