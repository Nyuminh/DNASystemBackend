namespace DNASystemBackend.DTOs
{
    public class UpdateAppointDto
    {
        public DateTime? Date { get; set; }
        public string? StaffId { get; set; }
        public string? ServiceId { get; set; }

        public string? Address { get; set; }
        public string? Method { get; set; }
        public string? Status { get; set; }
    }
}