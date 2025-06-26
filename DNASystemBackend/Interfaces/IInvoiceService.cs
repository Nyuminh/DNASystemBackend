using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<Invoice>> GetAllAsync();
        Task<Invoice?> GetByIdAsync(string id);
        Task<Invoice> CreateAsync(Invoice invoice);
        Task<bool> UpdateAsync(string id, Invoice invoice);
        Task<bool> DeleteAsync(string id);
        Task<string> GenerateIdAsync();
    }
}