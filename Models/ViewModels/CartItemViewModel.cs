namespace Project_385.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; } = new();

        public decimal SubTotal { get; set; }

        public decimal DeliveryFee { get; set; }

        public decimal GrandTotal { get; set; }

        public double? DistanceKm { get; set; }

        public double? CurrentLatitude { get; set; }

        public double? CurrentLongitude { get; set; }

        public double StoreLatitude { get; set; }

        public double StoreLongitude { get; set; }

        public string StoreAddress { get; set; } = string.Empty;

        public bool CanDeliver { get; set; } = true;

        public string DeliveryMessage { get; set; } = string.Empty;
    }
}
