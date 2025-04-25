using DATN.Data;
using DATN.Model;
using DATN.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class StatisticsController : ControllerBase
    {
        private readonly OderPitchDbContext _context;

        public StatisticsController(OderPitchDbContext context)
        {
            _context = context;
        }

        // GET: api/statistics/summary
        [HttpGet("summary")]
        public async Task<ActionResult<SummaryStatisticsDTO>> GetSummaryStatistics()
        {
            try
            {
                // Lấy thống kê hiện tại
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var startOfPrevMonth = startOfMonth.AddMonths(-1);
                var endOfPrevMonth = startOfMonth.AddDays(-1);

                // Tính toán doanh thu
                var currentMonthRevenue = await _context.Bookings
                    .Where(b => b.BookingDate >= startOfMonth && b.BookingDate <= endOfMonth && b.Status == "Completed")
                    .SumAsync(b => b.TotalPrice);

                var prevMonthRevenue = await _context.Bookings
                    .Where(b => b.BookingDate >= startOfPrevMonth && b.BookingDate <= endOfPrevMonth && b.Status == "Completed")
                    .SumAsync(b => b.TotalPrice);

                // Tính toán số lượng đặt chỗ
                var currentMonthBookings = await _context.Bookings
                    .Where(b => b.BookingDate >= startOfMonth && b.BookingDate <= endOfMonth)
                    .CountAsync();

                var prevMonthBookings = await _context.Bookings
                    .Where(b => b.BookingDate >= startOfPrevMonth && b.BookingDate <= endOfPrevMonth)
                    .CountAsync();

                // Tính toán số lượng người dùng mới
                var currentMonthUsers = await _context.Users
                    .Where(u => u.CreatedAt >= startOfMonth && u.CreatedAt <= endOfMonth)
                    .CountAsync();

                var prevMonthUsers = await _context.Users
                    .Where(u => u.CreatedAt >= startOfPrevMonth && u.CreatedAt <= endOfPrevMonth)
                    .CountAsync();

                // Tính toán phần trăm thay đổi
                var revenueChange = prevMonthRevenue > 0 
                    ? (currentMonthRevenue - prevMonthRevenue) / prevMonthRevenue * 100 
                    : 100;
                
                var bookingsChange = prevMonthBookings > 0 
                    ? ((decimal)currentMonthBookings - prevMonthBookings) / prevMonthBookings * 100 
                    : 100;
                
                var usersChange = prevMonthUsers > 0 
                    ? ((decimal)currentMonthUsers - prevMonthUsers) / prevMonthUsers * 100 
                    : 100;

                // Tính doanh thu trung bình mỗi đặt chỗ
                var currentAvgRevenue = currentMonthBookings > 0 
                    ? currentMonthRevenue / currentMonthBookings 
                    : 0;
                
                var prevAvgRevenue = prevMonthBookings > 0 
                    ? prevMonthRevenue / prevMonthBookings 
                    : 0;
                
                var avgRevenueChange = prevAvgRevenue > 0 
                    ? (currentAvgRevenue - prevAvgRevenue) / prevAvgRevenue * 100 
                    : 100;

                return Ok(new {
                    success = true,
                    data = new SummaryStatisticsDTO
                    {
                        TotalRevenue = currentMonthRevenue,
                        RevenueChange = Math.Round(revenueChange, 2),
                        TotalBookings = currentMonthBookings,
                        BookingsChange = Math.Round(bookingsChange, 2),
                        TotalUsers = await _context.Users.CountAsync(),
                        UsersChange = Math.Round(usersChange, 2),
                        AverageRevenueChange = Math.Round(avgRevenueChange, 2)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        // GET: api/statistics/revenue
        [HttpGet("revenue")]
        public async Task<ActionResult<RevenueChartDTO>> GetRevenueChart([FromQuery] string period = "month")
        {
            try
            {
                var now = DateTime.Now;
                var result = new RevenueChartDTO();

                if (period.ToLower() == "week")
                {
                    // Lấy doanh thu theo tuần
                    var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
                    
                    for (int i = 0; i < 7; i++)
                    {
                        var day = startOfWeek.AddDays(i);
                        result.Labels.Add(day.ToString("dd/MM"));
                        
                        var revenue = await _context.Bookings
                            .Where(b => b.BookingDate.Date == day.Date && b.Status == "Completed")
                            .SumAsync(b => b.TotalPrice);
                        
                        result.Data.Add(revenue);
                    }
                }
                else if (period.ToLower() == "month")
                {
                    // Lấy doanh thu theo tháng (30 ngày gần nhất)
                    var startDay = now.Date.AddDays(-29);
                    
                    for (int i = 0; i < 30; i++)
                    {
                        var day = startDay.AddDays(i);
                        result.Labels.Add(day.ToString("dd/MM"));
                        
                        var revenue = await _context.Bookings
                            .Where(b => b.BookingDate.Date == day.Date && b.Status == "Completed")
                            .SumAsync(b => b.TotalPrice);
                        
                        result.Data.Add(revenue);
                    }
                }
                else if (period.ToLower() == "year")
                {
                    // Lấy doanh thu theo năm
                    var year = now.Year;
                    
                    for (int i = 1; i <= 12; i++)
                    {
                        var monthStart = new DateTime(year, i, 1);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                        
                        result.Labels.Add(monthStart.ToString("MM/yyyy"));
                        
                        var revenue = await _context.Bookings
                            .Where(b => b.BookingDate >= monthStart && b.BookingDate <= monthEnd && b.Status == "Completed")
                            .SumAsync(b => b.TotalPrice);
                        
                        result.Data.Add(revenue);
                    }
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        // GET: api/statistics/payment-methods
        [HttpGet("payment-methods")]
        public async Task<ActionResult<PaymentMethodChartDTO>> GetPaymentMethodsChart()
        {
            try
            {
                var result = new PaymentMethodChartDTO();
                var payments = await _context.Bookings
                    .Include(b => b.Payment)
                    .Where(b => b.Status == "Completed" && b.Payment != null)
                    .GroupBy(b => b.Payment.PaymentMethod)
                    .Select(g => new 
                    {
                        Method = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var colors = new List<string> { "#4e73df", "#1cc88a", "#36b9cc", "#f6c23e", "#e74a3b" };
                int colorIndex = 0;

                foreach (var payment in payments)
                {
                    result.Labels.Add(payment.Method.ToString());
                    result.Data.Add(payment.Count);
                    result.Colors.Add(colors[colorIndex % colors.Count]);
                    colorIndex++;
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        // GET: api/statistics/recent-bookings
        [HttpGet("recent-bookings")]
        public async Task<ActionResult<IEnumerable<object>>> GetRecentBookings()
        {
            try
            {
                var recentBookings = await _context.Bookings
                    .Include(b => b.User)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(5)
                    .Select(b => new
                    {
                        id = b.BookingID,
                        userName = b.User.FullName,
                        amount = b.TotalPrice,
                        date = b.BookingDate,
                        status = b.Status
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = recentBookings });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }
    }
} 