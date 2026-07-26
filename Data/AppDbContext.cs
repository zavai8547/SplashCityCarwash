using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SplashCityCarwash.Models;

namespace SplashCityCarwash.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ServicePackage> ServicePackages { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionService> TransactionServices { get; set; }
        public DbSet<TransactionWasher> TransactionWashers { get; set; }
        public DbSet<WashQueue> WashQueues { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Receipt> Receipts { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<ShopSale> ShopSales { get; set; }
        public DbSet<ShopSaleItem> ShopSaleItems { get; set; }

        // Add this with your other DbSets
        public DbSet<Branch> Branches { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Branch → Transactions
            builder.Entity<Transaction>()
                .HasOne(t => t.Branch)
                .WithMany(b => b.Transactions)
                .HasForeignKey(t => t.BranchID)
                .OnDelete(DeleteBehavior.SetNull);

            // Branch → Expenses
            builder.Entity<Expense>()
                .HasOne(e => e.Branch)
                .WithMany(b => b.Expenses)
                .HasForeignKey(e => e.BranchID)
                .OnDelete(DeleteBehavior.SetNull);

            // Branch → ShopSales
            builder.Entity<ShopSale>()
                .HasOne(s => s.Branch)
                .WithMany(b => b.ShopSales)
                .HasForeignKey(s => s.BranchID)
                .OnDelete(DeleteBehavior.SetNull);

            // Existing configurations
            builder.Entity<TransactionService>()
                .HasIndex(ts => new { ts.TransactionID, ts.ServiceID });

            builder.Entity<Customer>()
                .HasIndex(c => c.Phone).IsUnique();

            builder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate).IsUnique();

            builder.Entity<Setting>()
                .HasIndex(s => s.SettingKey).IsUnique();
        }
    }
}