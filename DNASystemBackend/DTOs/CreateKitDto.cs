namespace DNASystemBackend.DTOs
{
    public class CreateKitDto
    {
        public string? KitId { get; set; } = null!;
        public string? CustomerId { get; set; }

        public string? StaffId { get; set; }

        public string? BookingId { get; set; }


        public string? Description { get; set; }

        public string? Status { get; set; }
        public DateTime? Receivedate { get; set; }
    }  
}