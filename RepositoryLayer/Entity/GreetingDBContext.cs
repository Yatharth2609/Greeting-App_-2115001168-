using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RepositoryLayer.Entity
{
    public class GreetingDBContext : DbContext
    {
        public GreetingDBContext(DbContextOptions<GreetingDBContext> options) : base(options)
        {
        }

        public DbSet<GreetingModel> Greetings { get; set; }
    }
}
