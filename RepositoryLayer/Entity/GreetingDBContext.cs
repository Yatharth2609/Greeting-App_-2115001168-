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

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<GreetingEntity> Greetings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GreetingEntity>()
                .HasOne(g => g.User)
                .WithMany(u => u.Greetings)
                .HasForeignKey(g => g.UserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
