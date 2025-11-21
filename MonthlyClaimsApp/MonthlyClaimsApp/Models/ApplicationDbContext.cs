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
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Lecturer>().HasData(
                new Lecturer { LecturerID = 1, Name = "Premice Elimie", Email = "epremice@iie.ac.za", Department = "Information Technology" },
                new Lecturer { LecturerID = 2, Name = "Alice Loembe", Email = "itsalloembe@iie.ac.za", Department = "Engineering" },
                new Lecturer { LecturerID = 3, Name = "Cecilia Akoughet", Email = "cycy.syntiche@iie.ac.za", Department = "Arts" }
            );
        }

    }
}
