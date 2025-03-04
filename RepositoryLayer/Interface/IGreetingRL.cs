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
    }
}
