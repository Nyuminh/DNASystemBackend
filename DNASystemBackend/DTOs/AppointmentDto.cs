namespace DNASystemBackend.DTOs
{
    public class AppointmentDto
    {
        public string BookingId { get; set; } = null!;
        public string? CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public string? StaffId { get; set; }
        public string? ServiceId { get; set; }
    }
}