namespace DNASystemBackend.DTOs
{
    public class InvoiceCreateDto
    {
        public string? BookingId { get; set; }
        public DateTime? Date { get; set; }
        public decimal? Price { get; set; }
        public List<InvoiceDetailDto> Details { get; set; } = new();
    }

    public class InvoiceDetailDto
    {
        public string? ServiceId { get; set; }
        public int? Quantity { get; set; }
    }
}