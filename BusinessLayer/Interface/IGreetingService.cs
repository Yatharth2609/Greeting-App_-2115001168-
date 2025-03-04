using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.Entity;

namespace BusinessLayer.Interface
{
    public interface IGreetingService
    {
        string GetGreetingMessage(string firstName, string lastName);
        void SaveGreetingMessage(GreetingEntity greeting);
        List<GreetingEntity> GetSavedGreetings();
    }
}
