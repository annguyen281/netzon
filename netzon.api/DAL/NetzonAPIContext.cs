using Microsoft.EntityFrameworkCore;
using Netzon.Api.Entities;

namespace Netzon.Api.DAL
{
    public class NetzonAPIContext : DbContext
    {
        public NetzonAPIContext(DbContextOptions<NetzonAPIContext> options) : base(options)         
        {         
        }

        public DbSet<User> Users {get; set;}
        public DbSet<UserRole> UserRoles {get; set;}
    }
}