using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Workshop16.Models;
using Workshop16.Data;
using Microsoft.AspNetCore.Identity;

namespace Workshop16.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<CarService> CarServices { get; set; }

    }
}
