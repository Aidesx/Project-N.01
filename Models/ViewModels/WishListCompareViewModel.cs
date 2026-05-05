namespace Project_385.Models.ViewModels
{
    public class WishListViewModel
    {
        public AppUserModels User { get; set; }
        public ProductModels Product { get; set; }
        public WishListModels WishList { get; set; }
    }

    public class CompareViewModel
    {
        public AppUserModels User { get; set; }
        public ProductModels Product { get; set; }
        public CompareModels Compares { get; set; }
    }
}
