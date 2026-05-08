using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Project_385.Models;
using Project_385.Models.MoMo;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Project_385.Services.MoMo
{
    public class MoMoService : IMomoService
    {
        private readonly IOptions<MoMoOptionModel> _options;

        public MoMoService(IOptions<MoMoOptionModel> options)
        {
            _options = options;
        }
        
        public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model)
        {
            model.OrderInfo = "Khách hàng: "+model.FullName +". Nội dung: "+model.OrderInfo;



            var rawData = $"accessKey={_options.Value.AccessKey}" +
              $"&amount={model.Amount}" +
              $"&extraData=" +
              $"&ipnUrl={_options.Value.NotifyUrl}" +
              $"&orderId={model.OrderId}" +
              $"&orderInfo={model.OrderInfo}" +
              $"&partnerCode={_options.Value.PartnerCode}" +
              $"&redirectUrl={_options.Value.ReturnUrl}" +
              $"&requestId={model.OrderId}" +
              $"&requestType={_options.Value.RequestType}";

            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest()
            {
                Method = RestSharp.Method.POST
            };

            var requestData = new
            {
                accessKey = _options.Value.AccessKey,
                partnerCode = _options.Value.PartnerCode,
                requestType = _options.Value.RequestType,
                ipnUrl = _options.Value.NotifyUrl,
                redirectUrl = _options.Value.ReturnUrl,
                orderId = model.OrderId,
                amount = long.Parse(model.Amount),  
                orderInfo = model.OrderInfo,
                requestId = model.OrderId,
                extraData = "",
                signature = signature
            };

            request.AddParameter(
                "application/json",
                JsonConvert.SerializeObject(requestData),
                ParameterType.RequestBody
            );

            var response = await client.ExecuteAsync(request);
            if (string.IsNullOrEmpty(response.Content) || !response.Content.Contains("payUrl"))
            {
                throw new Exception($"Lỗi chi tiết từ MoMo: {response.Content} | SỐ TIỀN THỰC TẾ ĐANG GỬI LÊN LÀ: {model.Amount}");
            }

            return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content)!;
        }
        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection["amount"].ToString();
            var orderId = collection["orderId"].ToString();
            var orderInfo = collection["orderInfo"].ToString();
            var resultCode = collection["resultCode"].ToString();
            var message = collection["message"].ToString();
            var transId = collection["transId"].ToString();
            var payType = collection["payType"].ToString();
            var responseTime = collection["responseTime"].ToString();
            var extraData = collection["extraData"].ToString();
            var partnerCode = collection["partnerCode"].ToString();
            var orderType = collection["orderType"].ToString();
            var requestId = collection["requestId"].ToString();
            var providedSignature = collection["signature"].ToString();

            var rawData = $"accessKey={_options.Value.AccessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&partnerCode={partnerCode}&payType={payType}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";
            var calculatedSignature = ComputeHmacSha256(rawData, _options.Value.SecretKey);
            return new MomoExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo,
                ResultCode = resultCode,
                Message = message,
                TransId = transId,
                IsValidSignature = (providedSignature == calculatedSignature)
            };
        }
        public string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            byte[] hashBytes;

            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return hashString;
        }
    }
}
    

