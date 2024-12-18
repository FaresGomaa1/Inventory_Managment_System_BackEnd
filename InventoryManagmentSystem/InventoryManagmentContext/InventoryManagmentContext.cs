using InventoryManagmentSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagmentSystem.Data
{
    public class InventoryManagmentContext : IdentityDbContext<User>
    {
        #region DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        #endregion
        public InventoryManagmentContext(DbContextOptions<InventoryManagmentContext> options) 
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.Team)
                .WithMany(t => t.Requests)
                .HasForeignKey(r => r.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(u => u.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasIndex(e => e.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
            .HasOne(u => u.Manager) 
            .WithMany(u => u.Subordinates) 
            .HasForeignKey(u => u.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
        }



    }
}
