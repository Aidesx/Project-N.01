using Microsoft.Extensions.Options;
using Project_385.Models;
using Project_385.Models.ViewModels;

namespace Project_385.Services
{
    public class CartPricingService
    {
        private const decimal FeePerKm = 10000m;
        private const double MaxDistanceKm = 100d;
        private readonly DeliverySettingsModel _deliverySettings;

        public CartPricingService(IOptions<DeliverySettingsModel> deliverySettings)
        {
            _deliverySettings = deliverySettings.Value;
        }

        public CartItemViewModel BuildSummary(IEnumerable<CartItemModel> cartItems, double? currentLatitude = null, double? currentLongitude = null)
        {
            var items = cartItems?.ToList() ?? new List<CartItemModel>();
            var subTotal = items.Sum(x => x.Price * x.Quantity);

            var viewModel = new CartItemViewModel
            {
                CartItems = items,
                SubTotal = subTotal,
                StoreAddress = _deliverySettings.StoreAddress,
                StoreLatitude = _deliverySettings.StoreLatitude,
                StoreLongitude = _deliverySettings.StoreLongitude,
                CurrentLatitude = currentLatitude,
                CurrentLongitude = currentLongitude
            };

            if (!items.Any())
            {
                viewModel.DeliveryFee = 0m;
                viewModel.GrandTotal = 0m;
                viewModel.CanDeliver = false;
                viewModel.DeliveryMessage = "Giỏ hàng đang trống.";
                return viewModel;
            }

            if (!currentLatitude.HasValue || !currentLongitude.HasValue)
            {
                viewModel.DeliveryFee = 0m;
                viewModel.GrandTotal = subTotal;
                viewModel.CanDeliver = false;
                viewModel.DeliveryMessage = "Bấm lấy vị trí hiện tại để tính phí di chuyển.";
                return viewModel;
            }

            var distanceKm = CalculateDistanceKm(
                currentLatitude.Value,
                currentLongitude.Value,
                _deliverySettings.StoreLatitude,
                _deliverySettings.StoreLongitude);

            viewModel.DistanceKm = Math.Round(distanceKm, 2);

            if (distanceKm > MaxDistanceKm)
            {
                viewModel.DeliveryFee = 0m;
                viewModel.GrandTotal = subTotal;
                viewModel.CanDeliver = false;
                viewModel.DeliveryMessage = "Khoảng cách vượt quá 100km, tạm thời không hỗ trợ giao hàng.";
                return viewModel;
            }

            var rawDeliveryFee = (decimal)Math.Ceiling(distanceKm) * FeePerKm;
            var discountRate = GetDiscountRate(distanceKm);
            var deliveryFee = rawDeliveryFee * (1m - discountRate);

            viewModel.DeliveryFee = decimal.Round(deliveryFee, 0, MidpointRounding.AwayFromZero);
            viewModel.GrandTotal = subTotal + viewModel.DeliveryFee;
            viewModel.CanDeliver = true;
            viewModel.DeliveryMessage = BuildDeliveryMessage(distanceKm, viewModel.DeliveryFee);
            return viewModel;
        }

        public decimal CalculateDeliveryFee(double distanceKm)
        {
            if (distanceKm > MaxDistanceKm)
            {
                throw new InvalidOperationException("Khoảng cách vượt quá 100km.");
            }

            var rawDeliveryFee = (decimal)Math.Ceiling(distanceKm) * FeePerKm;
            var discountRate = GetDiscountRate(distanceKm);
            return decimal.Round(rawDeliveryFee * (1m - discountRate), 0, MidpointRounding.AwayFromZero);
        }

        public double CalculateDistanceKm(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            const double earthRadiusKm = 6371d;

            double dLat = ToRadians(latitude2 - latitude1);
            double dLon = ToRadians(longitude2 - longitude1);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(latitude1)) * Math.Cos(ToRadians(latitude2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        private static double ToRadians(double value) => value * Math.PI / 180d;

        private static decimal GetDiscountRate(double distanceKm)
        {
            if (distanceKm > 50d)
            {
                return 0.30m;
            }

            if (distanceKm > 20d)
            {
                return 0.10m;
            }

            return 0m;
        }

        private static string BuildDeliveryMessage(double distanceKm, decimal deliveryFee)
        {
            var roundedDistance = Math.Round(distanceKm, 2);

            if (distanceKm > 50d)
            {
                return $"Khoảng cách {roundedDistance:0.00}km, áp dụng giảm 30% phí di chuyển. Phí hiện tại: {deliveryFee:#,##0} VND.";
            }

            if (distanceKm > 20d)
            {
                return $"Khoảng cách {roundedDistance:0.00}km, áp dụng giảm 10% phí di chuyển. Phí hiện tại: {deliveryFee:#,##0} VND.";
            }

            return $"Khoảng cách {roundedDistance:0.00}km. Phí hiện tại: {deliveryFee:#,##0} VND.";
        }
    }
}
