namespace Project_385.Models.ViewModels
{
    public class ProductDetailViewModels
    {
        public ProductModels Product { get; set; }
        public List<RatingModels> Ratings { get; set; } = new List<RatingModels>();
        public RatingModels Rating { get; set; }
    }
}
