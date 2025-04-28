using System;

namespace DATN.DTO
{
    public class BookedTimeSlotDTO
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int BookingID { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
    }
} 