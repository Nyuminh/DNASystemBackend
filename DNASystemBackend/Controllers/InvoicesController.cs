using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceDetailService _detailService;

        public InvoicesController(IInvoiceService invoiceService, IInvoiceDetailService detailService)
        {
            _invoiceService = invoiceService;
            _detailService = detailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            var invoices = await _invoiceService.GetAllAsync();
            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(string id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);
            return invoice == null ? NotFound() : Ok(invoice);
        }

        [HttpPost]
        public async Task<ActionResult<Invoice>> CreateInvoice([FromBody] InvoiceCreateDto dto)
        {
            var newId = await _invoiceService.GenerateIdAsync();
            var invoice = new Invoice
            {
                InvoiceId = newId,
                BookingId = dto.BookingId,
                Date = dto.Date,
                Price = dto.Price,
                InvoiceDetails = new List<InvoiceDetail>()
            };

            foreach (var item in dto.Details)
            {
                var detailId = await _detailService.GenerateIdAsync();
                invoice.InvoiceDetails.Add(new InvoiceDetail
                {
                    InvoicedetailId = detailId,
                    InvoiceId = newId,
                    ServiceId = item.ServiceId,
                    Quantity = item.Quantity
                });
            }

            var created = await _invoiceService.CreateAsync(invoice);
            return CreatedAtAction(nameof(GetInvoice), new { id = created.InvoiceId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateInvoice(string id, [FromBody] Invoice invoice)
        {
            var updated = await _invoiceService.UpdateAsync(id, invoice);
            return updated ? Ok(new { message = "Cập nhật thành công" }) : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteInvoice(string id)
        {
            var deleted = await _invoiceService.DeleteAsync(id);
            return deleted ? Ok(new { message = "Xóa thành công" }) : NotFound();
        }
    }
}
