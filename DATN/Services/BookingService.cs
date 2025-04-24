using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DATN.Data;
using DATN.DTO;
using DATN.Model;
using Microsoft.EntityFrameworkCore;

namespace DATN.Services
{
    public class BookingService : IBookingService
    {
        private readonly OderPitchDbContext _context;
        private readonly IMapper _mapper;

        public BookingService(OderPitchDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Lấy thông tin đặt sân theo ID
        public async Task<ResponseDTO<BookingDTO>> GetBookingByIdAsync(int id)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Pitch)
                        .ThenInclude(p => p.PitchType)
                    .Include(b => b.Payment)
                    .FirstOrDefaultAsync(b => b.BookingID == id);

                if (booking == null)
                {
                    return ResponseDTO<BookingDTO>.ErrorResult("Không tìm thấy thông tin đặt sân");
                }

                var bookingDto = _mapper.Map<BookingDTO>(booking);
                return ResponseDTO<BookingDTO>.SuccessResult(bookingDto);
            }
            catch (Exception ex)
            {
                return ResponseDTO<BookingDTO>.ExceptionResult(ex);
            }
        }

        // Lấy danh sách đặt sân theo người dùng
        public async Task<ResponseDTO<List<BookingDTO>>> GetBookingsByUserIdAsync(int userId)
        {
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Pitch)
                        .ThenInclude(p => p.PitchType)
                    .Include(b => b.Payment)
                    .Where(b => b.UserID == userId)
                    .OrderByDescending(b => b.BookingDate)
                    .ThenByDescending(b => b.StartTime)
                    .ToListAsync();

                var bookingDtos = _mapper.Map<List<BookingDTO>>(bookings);
                return ResponseDTO<List<BookingDTO>>.SuccessResult(bookingDtos);
            }
            catch (Exception ex)
            {
                return ResponseDTO<List<BookingDTO>>.ExceptionResult(ex);
            }
        }

        // Lấy danh sách đặt sân phân trang
        public async Task<PaginatedResponseDTO<BookingDTO>> GetPaginatedBookingsAsync(
            int pageNumber, int pageSize, BookingSearchDTO? searchDto = null)
        {
            try
            {
                IQueryable<Booking> query = _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Pitch)
                        .ThenInclude(p => p.PitchType)
                    .Include(b => b.Payment);

                // Áp dụng các điều kiện tìm kiếm
                if (searchDto != null)
                {
                    if (searchDto.FromDate.HasValue)
                    {
                        query = query.Where(b => b.BookingDate >= searchDto.FromDate.Value.Date);
                    }

                    if (searchDto.ToDate.HasValue)
                    {
                        query = query.Where(b => b.BookingDate <= searchDto.ToDate.Value.Date);
                    }

                    if (searchDto.PitchID.HasValue)
                    {
                        query = query.Where(b => b.PitchID == searchDto.PitchID.Value);
                    }

                    if (searchDto.UserID.HasValue)
                    {
                        query = query.Where(b => b.UserID == searchDto.UserID.Value);
                    }

                    if (!string.IsNullOrEmpty(searchDto.Status))
                    {
                        query = query.Where(b => b.Status == searchDto.Status);
                    }
                }

                // Sắp xếp và phân trang
                var totalItems = await query.CountAsync();
                var bookings = await query
                    .OrderByDescending(b => b.BookingDate)
                    .ThenByDescending(b => b.StartTime)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var bookingDtos = _mapper.Map<List<BookingDTO>>(bookings);
                return PaginatedResponseDTO<BookingDTO>.Create(bookingDtos, totalItems, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Lỗi khi lấy danh sách đặt sân", ex);
            }
        }

        // Tạo đặt sân mới
        public async Task<ResponseDTO<BookingDTO>> CreateBookingAsync(int userId, BookingCreateDTO bookingDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra người dùng
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return ResponseDTO<BookingDTO>.ErrorResult("Người dùng không tồn tại");
                }

                // Kiểm tra sân
                var pitch = await _context.Pitches
                    .Include(p => p.PitchType)
                    .FirstOrDefaultAsync(p => p.PitchID == bookingDto.PitchID);
                if (pitch == null)
                {
                    return ResponseDTO<BookingDTO>.ErrorResult("Sân bóng không tồn tại");
                }

                // Kiểm tra sân có trống không
                bool isAvailable = await CheckPitchAvailabilityAsync(
                    bookingDto.PitchID, 
                    bookingDto.BookingDate, 
                    bookingDto.StartTime, 
                    bookingDto.EndTime
                ).ContinueWith(t => t.Result.Data);

                if (!isAvailable)
                {
                    return ResponseDTO<BookingDTO>.ErrorResult("Sân đã được đặt trong khung giờ này");
                }

                var booking = _mapper.Map<Booking>(bookingDto);
                booking.UserID = userId;
                booking.Status = "Pending";
                booking.CreatedAt = DateTime.Now;

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Lấy thông tin đầy đủ
                var createdBooking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Pitch)
                        .ThenInclude(p => p.PitchType)
                    .FirstOrDefaultAsync(b => b.BookingID == booking.BookingID);

                var result = _mapper.Map<BookingDTO>(createdBooking);
                return ResponseDTO<BookingDTO>.SuccessResult(result, "Đặt sân thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseDTO<BookingDTO>.ExceptionResult(ex);
            }
        }

        // Cập nhật thông tin đặt sân
        public async Task<ResponseDTO<BookingDTO>> UpdateBookingAsync(int id, BookingUpdateDTO bookingDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                {
                    return ResponseDTO<BookingDTO>.ErrorResult("Không tìm thấy thông tin đặt sân");
                }

                // Không cho phép cập nhật đặt sân đã hoàn thành hoặc đã hủy
                if (booking.Status == "Completed" || booking.Status == "Cancelled")
                {
                    return ResponseDTO<BookingDTO>.ErrorResult($"Không thể cập nhật đặt sân có trạng thái {booking.Status}");
                }

                // Kiểm tra xem có thay đổi sân, ngày đặt hoặc giờ không
                bool needsAvailabilityCheck = false;
                int pitchId = booking.PitchID;
                DateTime bookingDate = booking.BookingDate;
                TimeSpan startTime = booking.StartTime;
                TimeSpan endTime = booking.EndTime;

                if (bookingDto.PitchID.HasValue && bookingDto.PitchID != booking.PitchID)
                {
                    pitchId = bookingDto.PitchID.Value;
                    needsAvailabilityCheck = true;
                }

                if (bookingDto.BookingDate.HasValue && bookingDto.BookingDate != booking.BookingDate)
                {
                    bookingDate = bookingDto.BookingDate.Value;
                    needsAvailabilityCheck = true;
                }

                if (bookingDto.StartTime.HasValue && bookingDto.StartTime != booking.StartTime)
                {
                    startTime = bookingDto.StartTime.Value;
                    needsAvailabilityCheck = true;
                }

                if (bookingDto.EndTime.HasValue && bookingDto.EndTime != booking.EndTime)
                {
                    endTime = bookingDto.EndTime.Value;
                    needsAvailabilityCheck = true;
                }

                // Nếu có thay đổi, kiểm tra xem sân có còn trống không
                if (needsAvailabilityCheck)
                {
                    var otherBookings = await _context.Bookings
                        .Where(b => b.BookingID != id && b.PitchID == pitchId && b.BookingDate.Date == bookingDate.Date)
                        .Where(b => b.Status != "Cancelled" &&
                                   ((b.StartTime <= startTime && b.EndTime > startTime) ||
                                    (b.StartTime < endTime && b.EndTime >= endTime) ||
                                    (b.StartTime >= startTime && b.EndTime <= endTime)))
                        .AnyAsync();

                    if (otherBookings)
                    {
                        return ResponseDTO<BookingDTO>.ErrorResult("Sân đã được đặt trong khung giờ này");
                    }
                }

                // Cập nhật thông tin đặt sân
                _mapper.Map(bookingDto, booking);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Lấy thông tin đầy đủ
                var updatedBooking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Pitch)
                        .ThenInclude(p => p.PitchType)
                    .Include(b => b.Payment)
                    .FirstOrDefaultAsync(b => b.BookingID == id);

                var result = _mapper.Map<BookingDTO>(updatedBooking);
                return ResponseDTO<BookingDTO>.SuccessResult(result, "Cập nhật đặt sân thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseDTO<BookingDTO>.ExceptionResult(ex);
            }
        }

        // Hủy đặt sân
        public async Task<ResponseDTO<bool>> CancelBookingAsync(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                {
                    return ResponseDTO<bool>.ErrorResult("Không tìm thấy thông tin đặt sân");
                }

                // Không cho phép hủy đặt sân đã hoàn thành
                if (booking.Status == "Completed")
                {
                    return ResponseDTO<bool>.ErrorResult("Không thể hủy đặt sân đã hoàn thành");
                }

                booking.Status = "Cancelled";
                await _context.SaveChangesAsync();

                return ResponseDTO<bool>.SuccessResult(true, "Hủy đặt sân thành công");
            }
            catch (Exception ex)
            {
                return ResponseDTO<bool>.ExceptionResult(ex);
            }
        }

        // Xác nhận đặt sân
        public async Task<ResponseDTO<bool>> ConfirmBookingAsync(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                {
                    return ResponseDTO<bool>.ErrorResult("Không tìm thấy thông tin đặt sân");
                }

                // Chỉ cho phép xác nhận đặt sân đang chờ xác nhận
                if (booking.Status != "Pending")
                {
                    return ResponseDTO<bool>.ErrorResult($"Không thể xác nhận đặt sân có trạng thái {booking.Status}");
                }

                booking.Status = "Confirmed";
                await _context.SaveChangesAsync();

                return ResponseDTO<bool>.SuccessResult(true, "Xác nhận đặt sân thành công");
            }
            catch (Exception ex)
            {
                return ResponseDTO<bool>.ExceptionResult(ex);
            }
        }

        // Hoàn thành đặt sân
        public async Task<ResponseDTO<bool>> CompleteBookingAsync(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                {
                    return ResponseDTO<bool>.ErrorResult("Không tìm thấy thông tin đặt sân");
                }

                // Chỉ cho phép hoàn thành đặt sân đã xác nhận
                if (booking.Status != "Confirmed")
                {
                    return ResponseDTO<bool>.ErrorResult($"Không thể hoàn thành đặt sân có trạng thái {booking.Status}");
                }

                booking.Status = "Completed";
                await _context.SaveChangesAsync();

                return ResponseDTO<bool>.SuccessResult(true, "Hoàn thành đặt sân thành công");
            }
            catch (Exception ex)
            {
                return ResponseDTO<bool>.ExceptionResult(ex);
            }
        }

        // Lấy danh sách sân trống
        public async Task<ResponseDTO<List<AvailablePitchDTO>>> GetAvailablePitchesAsync(
            DateTime date, TimeSpan startTime, TimeSpan endTime, int? pitchTypeId = null)
        {
            try
            {
                // Lấy danh sách sân đã được đặt trong ngày chỉ định
                var bookedPitchIds = await _context.Bookings
                    .Where(b => b.BookingDate.Date == date.Date &&
                               b.Status != "Cancelled" &&
                              ((b.StartTime <= startTime && b.EndTime > startTime) ||
                               (b.StartTime < endTime && b.EndTime >= endTime) ||
                               (b.StartTime >= startTime && b.EndTime <= endTime)))
                    .Select(b => b.PitchID)
                    .Distinct()
                    .ToListAsync();

                // Lấy danh sách sân còn trống
                IQueryable<Pitch> query = _context.Pitches
                    .Include(p => p.PitchType)
                    .Where(p => !bookedPitchIds.Contains(p.PitchID) && p.Status == "Hoạt động");

                // Lọc theo loại sân nếu được chỉ định
                if (pitchTypeId.HasValue)
                {
                    query = query.Where(p => p.PitchTypeID == pitchTypeId.Value);
                }

                var availablePitches = await query.ToListAsync();

                var result = availablePitches.Select(p => new AvailablePitchDTO
                {
                    PitchID = p.PitchID,
                    Name = p.Name,
                    PitchTypeName = p.PitchType?.Name ?? string.Empty,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    AvailableTimeSlots = new List<TimeSlotDTO>
                    {
                        new TimeSlotDTO
                        {
                            StartTime = startTime,
                            EndTime = endTime,
                            IsAvailable = true
                        }
                    }
                }).ToList();

                return ResponseDTO<List<AvailablePitchDTO>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ResponseDTO<List<AvailablePitchDTO>>.ExceptionResult(ex);
            }
        }

        // Kiểm tra sân có trống trong khung giờ không
        public async Task<ResponseDTO<bool>> CheckPitchAvailabilityAsync(
            int pitchId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                // Kiểm tra sân bóng
                var pitch = await _context.Pitches.FindAsync(pitchId);
                if (pitch == null)
                {
                    return ResponseDTO<bool>.ErrorResult("Sân bóng không tồn tại");
                }

                if (pitch.Status != "Hoạt động")
                {
                    return ResponseDTO<bool>.ErrorResult("Sân bóng đang bảo trì");
                }

                // Kiểm tra xem sân đã được đặt chưa
                var isBooked = await _context.Bookings
                    .Where(b => b.PitchID == pitchId &&
                               b.BookingDate.Date == date.Date &&
                               b.Status != "Cancelled" &&
                              ((b.StartTime <= startTime && b.EndTime > startTime) ||
                               (b.StartTime < endTime && b.EndTime >= endTime) ||
                               (b.StartTime >= startTime && b.EndTime <= endTime)))
                    .AnyAsync();

                return ResponseDTO<bool>.SuccessResult(!isBooked);
            }
            catch (Exception ex)
            {
                return ResponseDTO<bool>.ExceptionResult(ex);
            }
        }

        // Thống kê đặt sân theo trạng thái
        public async Task<ResponseDTO<Dictionary<string, int>>> GetBookingStatsByStatusAsync()
        {
            try
            {
                var stats = await _context.Bookings
                    .GroupBy(b => b.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.Status, g => g.Count);

                return ResponseDTO<Dictionary<string, int>>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                return ResponseDTO<Dictionary<string, int>>.ExceptionResult(ex);
            }
        }

        // Thống kê đặt sân theo khoảng thời gian
        public async Task<ResponseDTO<Dictionary<DateTime, int>>> GetBookingStatsByDateRangeAsync(
            DateTime fromDate, DateTime toDate)
        {
            try
            {
                var bookingCounts = await _context.Bookings
                    .Where(b => b.BookingDate >= fromDate.Date && b.BookingDate <= toDate.Date)
                    .GroupBy(b => b.BookingDate.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .ToListAsync();

                var result = bookingCounts.ToDictionary(item => item.Date, item => item.Count);
                return ResponseDTO<Dictionary<DateTime, int>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ResponseDTO<Dictionary<DateTime, int>>.ExceptionResult(ex);
            }
        }

        // Lấy danh sách khung giờ đã đặt
        public async Task<List<BookedTimeSlotDTO>> GetBookedTimeSlotsAsync(int pitchId, DateTime date)
        {
            try
            {
                var bookings = await _context.Bookings
                    .Where(b => b.PitchID == pitchId && 
                           b.BookingDate.Date == date.Date && 
                           b.Status != "Cancelled") // Chỉ lấy các đặt sân chưa bị hủy
                    .ToListAsync();

                return _mapper.Map<List<BookedTimeSlotDTO>>(bookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy khung giờ đã đặt: {ex.Message}");
                return new List<BookedTimeSlotDTO>();
            }

        }
    }
} 