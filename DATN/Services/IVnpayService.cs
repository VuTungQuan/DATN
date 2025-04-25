using DATN.DTO;
using Microsoft.AspNetCore.Http;
using System;

namespace DATN.Services
{
    public interface IVnpayService
    {
        string CreatePaymentUrl(PaymentRequestDTO request);
        PaymentResponseDTO GetPaymentResult(IQueryCollection queryCollection);
    }
} 