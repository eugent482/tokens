using InternetHospital.WebApi.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternetHospital.WebApi.Entities
{
    public class PostgreContext
    {
        public class ApplicationContext : DbContext
        {
            public DbSet<User> Users { get; set; }
            public DbSet<RefreshToken> RefreshTokens { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=tokendb;Username=postgres;Password=04081992as");
            }
        }
    }
}
