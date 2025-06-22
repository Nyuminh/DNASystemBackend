namespace DNASystemBackend.DTOs
{
    public class UpdateAppointDto
    {
        public DateTime? Date { get; set; }
        public string? StaffId { get; set; }
        public string? ServiceId { get; set; }
    }
}