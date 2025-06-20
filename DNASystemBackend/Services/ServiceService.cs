using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;

namespace DNASystemBackend.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _repo;

        public ServiceService(IServiceRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<Service>> GetAllAsync() => _repo.GetAllAsync();

        public Task<Service?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);

        public Task<Service> CreateAsync(Service model) => _repo.CreateAsync(model);

        public Task<bool> UpdateAsync(string id, Service model) => _repo.UpdateAsync(id, model);

        public Task<bool> DeleteAsync(string id) => _repo.DeleteAsync(id);

        public Task<List<string>> GetCategoriesAsync() => _repo.GetCategoriesAsync();
    }
}
