using DNASystemBackend.DTOs;
using DNASystemBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DNASystemBackend.Interfaces
{
    public interface IFeedbackService
    {
        Task<IEnumerable<Feedback>> GetAllAsync();
        Task<Feedback?> GetByIdAsync(string id);
        Task<Feedback> CreateAsync(CreateFeedbackDto feedback);
        Task<bool> UpdateAsync(string id, Feedback updated);
        Task<bool> DeleteAsync(string id);

    }
}
