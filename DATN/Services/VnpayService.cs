using DATN.DTO;
using Microsoft.Extensions.Configuration;
using VNPAY.NET;
using VNPAY.NET.Models;
using DATN.Helpers;
using Microsoft.AspNetCore.Http;
using VNPAY.NET.Enums;
using System;

namespace DATN.Services
{
    public class VnpayService : IVnpayService
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;

        public VnpayService(IConfiguration configuration)
        {
            _configuration = configuration;
            _vnpay = new Vnpay();
            _vnpay.Initialize(
                _configuration["Vnpay:TmnCode"],
                _configuration["Vnpay:HashSecret"],
                _configuration["Vnpay:BaseUrl"],
                _configuration["Vnpay:ReturnUrl"]);
        }

        public string CreatePaymentUrl(PaymentRequestDTO request)
        {
            Console.WriteLine($"VnpayService - CreatePaymentUrl: Bắt đầu tạo URL thanh toán");
            Console.WriteLine($"VnpayService - Cấu hình: TmnCode={_configuration["Vnpay:TmnCode"]}, BaseURL={_configuration["Vnpay:BaseUrl"]}, ReturnUrl={_configuration["Vnpay:ReturnUrl"]}");
            
            try {
                var paymentRequest = new PaymentRequest
                {
                    PaymentId = request.PaymentId,
                    Money = request.Amount,
                    Description = request.Description,
                    IpAddress = request.IpAddress,
                    BankCode = BankCode.ANY,
                    CreatedDate = DateTime.Now,
                    Currency = Currency.VND,
                    Language = DisplayLanguage.Vietnamese
                };

                Console.WriteLine($"VnpayService - Tạo PaymentRequest với PaymentId={request.PaymentId}, Money={request.Amount}");
                
                var paymentUrl = _vnpay.GetPaymentUrl(paymentRequest);
                Console.WriteLine($"VnpayService - Đã tạo URL thanh toán: {paymentUrl}");
                return paymentUrl;
            }
            catch (Exception ex) {
                Console.WriteLine($"VnpayService - Lỗi khi tạo URL thanh toán: {ex.Message}");
                if (ex.InnerException != null) {
                    Console.WriteLine($"VnpayService - Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public PaymentResponseDTO GetPaymentResult(IQueryCollection queryCollection)
        {
            var paymentResult = _vnpay.GetPaymentResult(queryCollection);

            var response = new PaymentResponseDTO
            {
                PaymentId = paymentResult.PaymentId,
                IsSuccess = paymentResult.IsSuccess,
                Description = paymentResult.Description,
                VnpayTransactionId = paymentResult.TransactionId,
                TransactionStatus = new StatusInfo
                {
                    Code = (int)paymentResult.TransactionStatusCode, // Cast enum to int for Code
                    Description = paymentResult.TransactionStatusCode.ToString() // Use ToString() for Description
                },
                
            };

            return response;
        }
    }
} 