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

            
        }

    }
}
