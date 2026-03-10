using System.ComponentModel.DataAnnotations;

namespace SplashCityCarwash.Models
{
    public enum PaymentMethod { Cash, MPesa, Card }
    public enum WashStatus { Waiting, Washing, Completed, Cancelled }

    public class Transaction
    {
        [Key]
        public int TransactionID { get; set; }
        public int CustomerID { get; set; }
        public int VehicleID { get; set; }
        public string StaffID { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public WashStatus Status { get; set; } = WashStatus.Waiting;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }

        public Customer Customer { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public AppUser Staff { get; set; } = null!;
        public ICollection<TransactionService> TransactionServices { get; set; } = new List<TransactionService>();
        public WashQueue? Queue { get; set; }
        public Receipt? Receipt { get; set; }
    }
}