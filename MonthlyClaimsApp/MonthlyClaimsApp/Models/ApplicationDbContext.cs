using Microsoft.EntityFrameworkCore;
using MonthlyClaimsApp.Models;
using System.Collections.Generic;

namespace MonthlyClaimsApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<Lecturer> Lecturer { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Users> Users { get; set; }
    }
}
