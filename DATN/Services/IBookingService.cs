using DATN.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Services
{
    public interface IBookingService
    {
        // Lấy thông tin đặt sân
        Task<ResponseDTO<BookingDTO>> GetBookingByIdAsync(int id);
        Task<ResponseDTO<List<BookingDTO>>> GetBookingsByUserIdAsync(int userId);
        Task<PaginatedResponseDTO<BookingDTO>> GetPaginatedBookingsAsync(int pageNumber, int pageSize, BookingSearchDTO? searchDto = null);
        
        // Tạo và cập nhật đặt sân
        Task<ResponseDTO<BookingDTO>> CreateBookingAsync(int userId, BookingCreateDTO bookingDto);
        Task<ResponseDTO<BookingDTO>> UpdateBookingAsync(int id, BookingUpdateDTO bookingDto);
        
        // Xử lý trạng thái đặt sân
        Task<ResponseDTO<bool>> CancelBookingAsync(int id);
        Task<ResponseDTO<bool>> ConfirmBookingAsync(int id);
        Task<ResponseDTO<bool>> CompleteBookingAsync(int id);
        
        // Kiểm tra sân trống
        Task<ResponseDTO<List<AvailablePitchDTO>>> GetAvailablePitchesAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, int? pitchTypeId = null);
        Task<ResponseDTO<bool>> CheckPitchAvailabilityAsync(int pitchId, DateTime date, TimeSpan startTime, TimeSpan endTime);
        
        // Thống kê
        Task<ResponseDTO<Dictionary<string, int>>> GetBookingStatsByStatusAsync();
        Task<ResponseDTO<Dictionary<DateTime, int>>> GetBookingStatsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    }
} 