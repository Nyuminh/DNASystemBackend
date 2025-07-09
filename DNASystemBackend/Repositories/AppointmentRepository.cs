using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly DnasystemContext _context;

        public AppointmentRepository(DnasystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
              => await _context.Bookings
    .Include(b => b.Customer)
    .ToListAsync();

        public async Task<IEnumerable<Booking>> GetByServiceIdAsync(string serviceId)
          => await _context.Bookings.Where(r => r.ServiceId == serviceId).ToListAsync();

        public async Task<Booking?> GetByIdAsync(string id)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Staff)
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

       public async Task CreateAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(); 
    }

        public async Task<bool> UpdateAsync(string id, Booking updated)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return false;

            // Update properties from Booking
            booking.CustomerId = updated.CustomerId;
            booking.StaffId = updated.StaffId;
            booking.ServiceId = updated.ServiceId;
            booking.Date = updated.Date;
            booking.Address = updated.Address;
            booking.Method = updated.Method;
            booking.Status = updated.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return false;

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DateTime>> GetAvailableSchedulesAsync()
        {
            var existingDates = await _context.Bookings
                .Where(b => b.Date != null)
                .Select(b => b.Date!.Value.Date)
                .ToListAsync();

            var available = new List<DateTime>();
            var start = DateTime.Today.AddDays(1);

            for (int i = 0; i < 14; i++)
            {
                var date = start.AddDays(i);
                if (!existingDates.Contains(date))
                    available.Add(date);
            }

            return available;
        }

        public async Task<string> GenerateBookingIdAsync()
        {
            var existingIds = await _context.Bookings.Select(b => b.BookingId).ToListAsync();
            int counter = 1;
            string newId;

            do
            {
                newId = $"B{counter:D3}";
                counter++;
            } while (existingIds.Contains(newId));

            return newId;
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}