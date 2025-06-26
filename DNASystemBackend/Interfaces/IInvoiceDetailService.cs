using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IInvoiceDetailService
    {
        Task<IEnumerable<InvoiceDetail>> GetByInvoiceIdAsync(string invoiceId);
        Task<InvoiceDetail> CreateAsync(InvoiceDetail detail);
        Task<string> GenerateIdAsync();
        Task DeleteByInvoiceIdAsync(string invoiceId);
    }
}