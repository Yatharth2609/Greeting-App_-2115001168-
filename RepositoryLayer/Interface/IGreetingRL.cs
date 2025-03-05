using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface
{
    public interface IGreetingRL
    {
        void SaveGreeting(GreetingEntity greeting);
        List<GreetingEntity> GetAllGreetings();
        GreetingEntity GetGreetingById(int id);

        bool UpdateGreeting(int id, string NewMessage);

        bool DeleteGreeting(int id);
    }
}
