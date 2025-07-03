using DNASystemBackend.DTOs;
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

        public async Task<Feedback> CreateAsync(CreateFeedbackDto dto)
        {
            var feedback = new Feedback
            {
                FeedbackId = Guid.NewGuid().ToString("N")[..6].ToUpper(), // eg. "FD1234"
                CustomerId = dto.CustomerId,
                ServiceId = dto.ServiceId,
                Comment = dto.Comment,
                Rating = dto.Rating
            };

            return await _repository.CreateAsync(feedback);
        }

        public Task<bool> UpdateAsync(string id, Feedback updated) => _repository.UpdateAsync(id, updated);

        public Task<bool> DeleteAsync(string id) => _repository.DeleteAsync(id);

        
    }
}
