using DATN.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace DATN.Data
{
    public static class SeedData
    {
        public static void Initialize(ModelBuilder modelBuilder)
        {
            // Seed data for PitchType
            modelBuilder.Entity<PitchType>().HasData(
                new PitchType { PitchTypeID = 1, Name = "Sân 5" },
                new PitchType { PitchTypeID = 2, Name = "Sân 7" },
                new PitchType { PitchTypeID = 3, Name = "Sân 11" }
            );

            // Seed data for Pitches
            modelBuilder.Entity<Pitch>().HasData(
                new Pitch { PitchID = 1, Name = "Sân 1", PitchTypeID = 1, Location = "Khu A", Price = 200000, IsCombined = false },
                new Pitch { PitchID = 2, Name = "Sân 2", PitchTypeID = 1, Location = "Khu A", Price = 200000, IsCombined = false },
                new Pitch { PitchID = 3, Name = "Sân 7-1", PitchTypeID = 2, Location = "Khu B", Price = 350000, IsCombined = true, ParentPitchID = 1 },
                new Pitch { PitchID = 4, Name = "Sân 11-1", PitchTypeID = 3, Location = "Khu C", Price = 500000, IsCombined = false }
            );

            // Seed data for Users
            modelBuilder.Entity<User>().HasData(
                new User { UserID = 1, FullName = "Nguyen Van A", PhoneNumber = "0123456789", Email = "a@gmail.com", PasswordHash = "hashed_password", Role = "User" },
                new User { UserID = 2, FullName = "Tran Van B", PhoneNumber = "0987654321", Email = "b@gmail.com", PasswordHash = "hashed_password", Role = "Admin" }
            );

            // Seed data for Bookings
            modelBuilder.Entity<Booking>().HasData(
                new Booking { BookingID = 1, UserID = 1, PitchID = 1, BookingDate = DateTime.Today, StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(16, 0, 0), TotalPrice = 200000, Status = "Confirmed" },
                new Booking { BookingID = 2, UserID = 2, PitchID = 3, BookingDate = DateTime.Today, StartTime = new TimeSpan(17, 0, 0), EndTime = new TimeSpan(18, 30, 0), TotalPrice = 350000, Status = "Pending" }
            );

            // Seed data for Payments
            modelBuilder.Entity<Payment>().HasData(
                new Payment { PaymentID = 1, BookingID = 1, PaymentMethod = "MOMO", PaymentStatus = "Paid", PaidAmount = 200000, PaidDate = DateTime.Today, TransactionID = "TXN12345" },
                new Payment { PaymentID = 2, BookingID = 2, PaymentMethod = "VNPAY", PaymentStatus = "Pending", PaidAmount = 0, PaidDate = null, TransactionID = null }
            );
        }
    }
}
