using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;

namespace DNASystemBackend.Services
{
    public class KitService : IKitService
    {
        private readonly IKitRepository _repository;

        public KitService(IKitRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Kit>> GetAllAsync()
            => _repository.GetAllAsync();

        public Task<Kit?> GetByIdAsync(string id)
            => _repository.GetByIdAsync(id);

        public Task<Kit> CreateAsync(Kit kit)
            => _repository.CreateAsync(kit);

        public Task<bool> UpdateStatusAsync(string id, string status)
            => _repository.UpdateStatusAsync(id, status);

        public Task<IEnumerable<Kit>> GetTrackingSamplesAsync()
            => _repository.GetTrackingSamplesAsync();

        public Task<IEnumerable<Kit>> GetCollectionSamplesAsync()
            => _repository.GetCollectionSamplesAsync();
    }
}
