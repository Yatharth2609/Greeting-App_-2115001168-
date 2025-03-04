using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Interface;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;

namespace BusinessLayer.Service
{
    public class GreetingService : IGreetingService
    {
        private readonly IGreetingRL _greetingRL;

        public GreetingService(IGreetingRL greetingRL)
        {
            _greetingRL = greetingRL;
        }

        public string GetGreetingMessage(string firstName, string lastName)
        {
            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            {
                return $"Hello, {firstName} {lastName}!";
            }
            else if (!string.IsNullOrWhiteSpace(firstName))
            {
                return $"Hello, {firstName}!";
            }
            else if (!string.IsNullOrWhiteSpace(lastName))
            {
                return $"Hello, Mr./Ms. {lastName}!";
            }
            else
            {
                return "Hello, World!";
            }
        }

        public void SaveGreetingMessage(GreetingEntity greetingEntity)
        {
            _greetingRL.SaveGreeting(greetingEntity);
        }

        public List<GreetingEntity> GetSavedGreetings()
        {
            return _greetingRL.GetAllGreetings();
        }

        public GreetingEntity GetGreetingById(int id)
        {
            return _greetingRL.GetGreetingById(id);
        }

        public bool UpdateGreeting(int id, string NewMessage)
        {
            return _greetingRL.UpdateGreeting(id, NewMessage);
        }
    }
}
