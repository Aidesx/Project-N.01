namespace Project_385.Models.MoMo
{
    public class MomoExecuteResponseModel
    {
        public string FullName { get; set; }
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string OrderInfo { get; set; }
        public string TransId { get; set; }
        public string ResultCode { get; set; }
        public bool IsValidSignature { get; set; }
        public string Message { get; set; }
    }
}

