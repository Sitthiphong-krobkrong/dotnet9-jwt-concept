using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet9_jwt_concept.Core
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Global Query Filter สำหรับ User: เอาเฉพาะ record_status == "A"
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => u.record_status == "A");

            base.OnModelCreating(modelBuilder);
        }
    }



    [Table("user")]
    public class User
    {
        [Key]
        public int user_id { get; set; }
        public string user_name { get; set; } = string.Empty;
        public string user_pass { get; set; } = string.Empty;
        public string user_fname { get; set; } = string.Empty;
        public string user_lname { get; set; } = string.Empty;
        public string record_status { get; set; } = "A";
        public int create_user_id { get; set; } = 1;
        public DateTime create_user_date { get; set; } = DateTime.Now;
        public int ?update_user_id { get; set; }
        public DateTime ?update_user_date { get; set; }
    }
}
