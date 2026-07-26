using System.ComponentModel.DataAnnotations;

namespace SplashCityCarwash.Models
{
    public class Branch
    {
        [Key]
        public int BranchID { get; set; }

        [Required]
        public string Name { get; set; }
            = string.Empty;

        public string? Location { get; set; }
        public string? Phone { get; set; }
        public string? ManagerName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        // Navigation
        public ICollection<Transaction> Transactions
        { get; set; }
            = new List<Transaction>();
        public ICollection<Expense> Expenses
        { get; set; }
            = new List<Expense>();
        public ICollection<ShopSale> ShopSales
        { get; set; }
            = new List<ShopSale>();
    }
}