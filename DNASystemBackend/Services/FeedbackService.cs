using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DNASystemBackend.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repository;

        public FeedbackService(IFeedbackRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Feedback>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Feedback?> GetByIdAsync(string id) => _repository.GetByIdAsync(id);

        public Task<Feedback> CreateAsync(Feedback feedback) => _repository.CreateAsync(feedback);

        public Task<bool> UpdateAsync(string id, Feedback updated) => _repository.UpdateAsync(id, updated);

        public Task<bool> DeleteAsync(string id) => _repository.DeleteAsync(id);
    }
}
