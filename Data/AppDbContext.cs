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
        public DbSet<WashQueue> WashQueues { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Receipt> Receipts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TransactionService>()
                .HasIndex(ts => new { ts.TransactionID, ts.ServiceID });

            builder.Entity<Customer>()
                .HasIndex(c => c.Phone).IsUnique();

            builder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate).IsUnique();

            builder.Entity<Setting>()
                .HasIndex(s => s.SettingKey).IsUnique();

            builder.Entity<Setting>().HasData(
                new Setting { SettingID = 1, SettingKey = "business_name", SettingValue = "Splash City Carwash" },
                new Setting { SettingID = 2, SettingKey = "currency", SettingValue = "KES" },
                new Setting { SettingID = 3, SettingKey = "vat_rate", SettingValue = "16" },
                new Setting { SettingID = 4, SettingKey = "receipt_footer", SettingValue = "Thank you for choosing Splash City!" }
            );

            builder.Entity<ServicePackage>().HasData(
                new ServicePackage { ServiceID = 1, ServiceName = "Basic Wash", Description = "Exterior rinse and dry", Price = 500, EstimatedDuration = 15 },
                new ServicePackage { ServiceID = 2, ServiceName = "Standard Wash", Description = "Exterior + windows + tire shine", Price = 800, EstimatedDuration = 25 },
                new ServicePackage { ServiceID = 3, ServiceName = "Interior Cleaning", Description = "Full interior vacuum and wipe", Price = 1000, EstimatedDuration = 30 },
                new ServicePackage { ServiceID = 4, ServiceName = "Full Detail", Description = "Interior + exterior full detail", Price = 2500, EstimatedDuration = 90 },
                new ServicePackage { ServiceID = 5, ServiceName = "Engine Wash", Description = "Engine bay cleaning", Price = 1500, EstimatedDuration = 45 },
                new ServicePackage { ServiceID = 6, ServiceName = "Waxing", Description = "Full body wax and polish", Price = 2000, EstimatedDuration = 60 }
            );
        }
    }
}