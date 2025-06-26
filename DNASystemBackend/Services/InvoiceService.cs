using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly DnasystemContext _context;

        public InvoiceService(DnasystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await _context.Invoices.Include(i => i.InvoiceDetails).ToListAsync();
        }

        public async Task<Invoice?> GetByIdAsync(string id)
        {
            return await _context.Invoices.Include(i => i.InvoiceDetails)
                                          .FirstOrDefaultAsync(i => i.InvoiceId == id);
        }

        public async Task<Invoice> CreateAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<bool> UpdateAsync(string id, Invoice invoice)
        {
            var existing = await _context.Invoices.FindAsync(id);
            if (existing == null) return false;

            existing.BookingId = invoice.BookingId;
            existing.Date = invoice.Date;
            existing.Price = invoice.Price;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var invoice = await _context.Invoices.Include(i => i.InvoiceDetails)
                                                 .FirstOrDefaultAsync(i => i.InvoiceId == id);
            if (invoice == null) return false;

            _context.InvoiceDetails.RemoveRange(invoice.InvoiceDetails);
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateIdAsync()
        {
            var lastId = await _context.Invoices.OrderByDescending(i => i.InvoiceId)
                                                .Select(i => i.InvoiceId).FirstOrDefaultAsync();
            int num = int.TryParse(lastId?.Substring(1), out var n) ? n + 1 : 1;
            return $"I{num:D4}";
        }
    }

    public class InvoiceDetailService : IInvoiceDetailService
    {
        private readonly DnasystemContext _context;

        public InvoiceDetailService(DnasystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InvoiceDetail>> GetByInvoiceIdAsync(string invoiceId)
        {
            return await _context.InvoiceDetails.Where(d => d.InvoiceId == invoiceId).ToListAsync();
        }

        public async Task<InvoiceDetail> CreateAsync(InvoiceDetail detail)
        {
            _context.InvoiceDetails.Add(detail);
            await _context.SaveChangesAsync();
            return detail;
        }

        public async Task<string> GenerateIdAsync()
        {
            var lastId = await _context.InvoiceDetails.OrderByDescending(d => d.InvoicedetailId)
                                                       .Select(d => d.InvoicedetailId).FirstOrDefaultAsync();
            int num = int.TryParse(lastId?.Substring(1), out var n) ? n + 1 : 1;
            return $"D{num:D5}";
        }

        public async Task DeleteByInvoiceIdAsync(string invoiceId)
        {
            var details = await _context.InvoiceDetails.Where(d => d.InvoiceId == invoiceId).ToListAsync();
            _context.InvoiceDetails.RemoveRange(details);
            await _context.SaveChangesAsync();
        }
    }
}