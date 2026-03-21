namespace SplashCityCarwash.Models
{
    public class NewWashViewModel
    {
        public int CustomerID { get; set; }
        public int VehicleID { get; set; }
        public List<int> SelectedServiceIDs { get; set; } = new();
        public PaymentMethod PaymentMethod { get; set; }
        public string? Notes { get; set; }
    }
}