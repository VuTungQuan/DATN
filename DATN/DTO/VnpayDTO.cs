namespace DATN.DTO
{
    public class PaymentRequestDTO
    {
        public long PaymentId { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int BookingId { get; set; }
    }

    public class PaymentResponseDTO
    {
        public long PaymentId { get; set; }
        public bool IsSuccess { get; set; }
        public string Description { get; set; } = string.Empty;
        public long VnpayTransactionId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public StatusInfo TransactionStatus { get; set; } = new StatusInfo();
        public string BankCode { get; set; } = string.Empty;
        public string BankTransactionId { get; set; } = string.Empty;
    }

    public class StatusInfo
    {
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }
} 