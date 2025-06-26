namespace DNASystemBackend.DTOs
{
    public class TestResultCreateDto
    {
        public string? CustomerId { get; set; }
        public string? StaffId { get; set; }
        public string? ServiceId { get; set; }
        public string? BookingId { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
    }
}