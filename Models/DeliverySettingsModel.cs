namespace Project_385.Models
{
    public class DeliverySettingsModel
    {
        public string ShopName { get; set; } = string.Empty;

        public string StoreAddress { get; set; } = string.Empty;

        public double StoreLatitude { get; set; }

        public double StoreLongitude { get; set; }
    }
}
