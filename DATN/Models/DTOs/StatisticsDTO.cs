using System.Collections.Generic;

namespace DATN.Models.DTOs
{
    // DTO cho thống kê tổng quan
    public class SummaryStatisticsDTO
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueChange { get; set; }
        public int TotalBookings { get; set; }
        public decimal BookingsChange { get; set; }
        public int TotalUsers { get; set; }
        public decimal UsersChange { get; set; }
        public decimal AverageRevenueChange { get; set; }
    }

    // DTO cho dữ liệu biểu đồ doanh thu
    public class RevenueChartDTO
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Data { get; set; } = new List<decimal>();
    }

    // DTO cho dữ liệu biểu đồ phương thức thanh toán
    public class PaymentMethodChartDTO
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Data { get; set; } = new List<int>();
        public List<string> Colors { get; set; } = new List<string>();
    }
} 