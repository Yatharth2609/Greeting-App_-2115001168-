using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Interface;

namespace BusinessLayer.Service
{
    public class GreetingService : IGreetingService
    {
        public string GetGreetingMessage()
        {
            return "Hello, World!";
        }
    }
}
