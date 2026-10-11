using System.ComponentModel.DataAnnotations.Schema;

namespace Project_385.Models
{
    public enum OrderStatus
    {
        Pending = 0,    // Chờ xử lý / Đơn COD mới tạo
        Paid = 1,       // Đã thanh toán (MoMo)
        Processing = 2, // Đang đóng gói / chuẩn bị hàng
        Shipping = 3,   // Đang giao hàng
        Completed = 4,  // Giao hàng thành công
        Cancelled = 5   // Đã hủy đơn
    }

    public class OrderModels
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string UserName { get; set; }

        public string ReceiverName { get; set; }
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; } = "COD";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        public int Status { get; set; }
        public List<OrderDetailModels> OrderDetails { get; set; }
    }
}
