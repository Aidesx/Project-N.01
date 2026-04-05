namespace Project_385.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }

        public decimal GrandTotal { get; set; }
    }
}
