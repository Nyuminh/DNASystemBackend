using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNASystemBackend.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly DnasystemContext _context;

        public FeedbackRepository(DnasystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Feedback>> GetAllAsync()
        {
            return await _context.Feedbacks
                .Include(f => f.Customer)
                .Include(f => f.Service)
                .ToListAsync();
        }

        public async Task<Feedback?> GetByIdAsync(string id)
        {
            return await _context.Feedbacks
                .Include(f => f.Customer)
                .Include(f => f.Service)
                .FirstOrDefaultAsync(f => f.FeedbackId == id);
        }
        
        public async Task<IEnumerable<Feedback>> GetByServiceIdAsync(string serviceId)
          => await _context.Feedbacks.Where(r => r.ServiceId == serviceId).ToListAsync();
        public async Task<Feedback> CreateAsync(Feedback feedback)
        {
            feedback.FeedbackId = await GenerateFeedbackIdAsync();
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }


        public async Task<bool> UpdateAsync(string id, Feedback updated)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null) return false;

            feedback.Comment = updated.Comment;
            feedback.Rating = updated.Rating;
            feedback.CustomerId = updated.CustomerId;
            feedback.ServiceId = updated.ServiceId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null) return false;

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateFeedbackIdAsync()
        {
            var existingIds = await _context.Feedbacks.Select(f => f.FeedbackId).ToListAsync();
            int counter = 1;
            string newId;

            do
            {
                newId = $"F{counter:D3}";
                counter++;
            } while (existingIds.Contains(newId));

            return newId;
        }
    }
}
