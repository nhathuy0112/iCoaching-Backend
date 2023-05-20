using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Core.Entities;
using Core.Entities.Payment;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class VnPayCompare : IComparer<string>
{
    public int Compare(string x, string y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}

public class VnPayService : IVnPayService
{
    private readonly IConfiguration _configuration;
    private readonly SortedDictionary<string, string> _requestData = new SortedDictionary<string, string>(new VnPayCompare());
    private readonly SortedDictionary<string, string> _responseData = new SortedDictionary<string, string>(new VnPayCompare());
    public VnPayService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreatePaymentUrl(CoachingRequest coachingRequest, string callBackUrl, string ipAddress)
    {
        var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
        var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
        var tick = DateTime.Now.Ticks.ToString();
        var rawPrice = coachingRequest.Course.Price;
        if (coachingRequest.Discount != null)
        {
            rawPrice = (long) (rawPrice - (rawPrice * coachingRequest.Discount / 100));
        }
        var price = rawPrice * 100;
        _requestData.Add("vnp_Version", _configuration["Vnpay:Version"]);
        _requestData.Add("vnp_Command", _configuration["Vnpay:Command"]);
        _requestData.Add("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
        _requestData.Add("vnp_Amount", price.ToString());
        _requestData.Add("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
        _requestData.Add("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
        _requestData.Add("vnp_IpAddr", ipAddress);
        _requestData.Add("vnp_Locale", _configuration["Vnpay:Locale"]);
        _requestData.Add("vnp_OrderInfo", $"Khach hang thanh toan yeu cau {coachingRequest.Id} ");
        _requestData.Add("vnp_ReturnUrl", callBackUrl);
        _requestData.Add("vnp_TxnRef", tick);

        var url = CreateRequestUrl();
        return url;

    }
    
    public PaymentResponse GetFullResponseData(IQueryCollection collection)
    {
        foreach (var (key, value) in collection)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                AddResponseData(key, value);
            }
        }

        var orderId = Convert.ToInt64(GetResponseData("vnp_TxnRef"));
        var vnPayTranId = Convert.ToInt64(GetResponseData("vnp_TransactionNo"));
        var vnpResponseCode = GetResponseData("vnp_ResponseCode");
        var vnpSecureHash =
            collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value; 
        var orderInfo = GetResponseData("vnp_OrderInfo");

        var checkSignature = ValidateSignature(vnpSecureHash);

        if (!checkSignature)
            return new PaymentResponse()
            {
                VnPayCallbackResult = false
            };

        return new PaymentResponse()
        {
            VnPayCallbackResult = true,
            PaymentMethod = "VnPay",
            OrderDescription = orderInfo,
            OrderId = orderId.ToString(),
            PaymentId = vnPayTranId.ToString(),
            TransactionId = vnPayTranId.ToString(),
            Token = vnpSecureHash,
            VnPayResponseCode = vnpResponseCode
        };
    }
    
    private string CreateRequestUrl()
    {
        var data = new StringBuilder();
        var baseUrl = _configuration["Vnpay:BaseUrl"];
        foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
        }

        var querystring = data.ToString();

        baseUrl += "?" + querystring;
        var signData = querystring;
        if (signData.Length > 0)
        {
            signData = signData.Remove(data.Length - 1, 1);
        }

        var vnpSecureHash = HmacSha512(signData);
        baseUrl += "vnp_SecureHash=" + vnpSecureHash;

        return baseUrl;
    }
    
    private string HmacSha512(string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(_configuration["Vnpay:HashSecret"]);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }
    
        return hash.ToString();
    }

    private void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    private string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
    }
    
    private string GetResponseData()
    {
        var data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType"))
        {
            _responseData.Remove("vnp_SecureHashType");
        }

        if (_responseData.ContainsKey("vnp_SecureHash"))
        {
            _responseData.Remove("vnp_SecureHash");
        }

        foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
        }

        //remove last '&'
        if (data.Length > 0)
        {
            data.Remove(data.Length - 1, 1);
        }

        return data.ToString();
    }

    private bool ValidateSignature(string inputHash)
    {
        var rspRaw = GetResponseData();
        var myChecksum = HmacSha512(rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }
}