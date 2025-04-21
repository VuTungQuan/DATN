using DATN.Model;
using Microsoft.EntityFrameworkCore;

namespace DATN.Data
{
    public class OderPitchDbContext : DbContext
    {
        public OderPitchDbContext(DbContextOptions<OderPitchDbContext> options) : base(options) { }

        public DbSet<PitchType> PitchTypes { get; set; }
        public DbSet<Pitch> Pitches { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ràng buộc không cho đặt trùng sân cùng thời gian
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.PitchID, b.BookingDate, b.StartTime, b.EndTime })
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserID)
                .OnDelete(DeleteBehavior.Cascade); // Optional: You can choose to delete bookings if the user is deleted

            // Define one-to-many relationship between PitchType and Pitch
            modelBuilder.Entity<Pitch>()
                .HasOne(p => p.PitchType)
                .WithMany(pt => pt.Pitches)
                .HasForeignKey(p => p.PitchTypeID)
                .OnDelete(DeleteBehavior.Restrict); 

            // Define one-to-one relationship between Booking and Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment) 
                .HasForeignKey<Payment>(p => p.BookingID)
                .OnDelete(DeleteBehavior.Cascade); 

            // Define one-to-many relationship between Pitch and Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Pitch)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PitchID)
                .OnDelete(DeleteBehavior.Restrict); // Optional: Handle deleting pitches that are booked

            // Ensure that all tables have a CreatedAt field for tracking creation dates
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Booking>().Property(b => b.CreatedAt).HasDefaultValueSql("GETDATE()");
            
        }

    }
}
