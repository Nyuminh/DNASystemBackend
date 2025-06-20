using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;

namespace DNASystemBackend.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;

        public AppointmentService(IAppointmentRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Booking>> GetAllAsync()
            => _repository.GetAllAsync();

        public Task<Booking?> GetByIdAsync(string id)
            => _repository.GetByIdAsync(id);

        public Task<Booking> CreateAsync(Booking booking)
            => _repository.CreateAsync(booking);

        public Task<bool> UpdateAsync(string id, Booking updated)
            => _repository.UpdateAsync(id, updated);

        public Task<bool> DeleteAsync(string id)
            => _repository.DeleteAsync(id);

        public Task<List<DateTime>> GetAvailableSchedulesAsync()
            => _repository.GetAvailableSchedulesAsync();
    }
}
