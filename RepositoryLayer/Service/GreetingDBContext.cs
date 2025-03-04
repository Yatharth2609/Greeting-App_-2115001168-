using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Service
{
    public class GreetingDBContext : DbContext
    {
        public GreetingDBContext(DbContextOptions<GreetingDBContext> options) : base(options)
        {
        }
        public DbSet<GreetingEntity> Greetings { get; set; }
    }
}
