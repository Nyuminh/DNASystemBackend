namespace DNASystemBackend.DTOs
{
    public class RelativeCreateDto
    {
        public string UserId { get; set; } = null!;
        public string Fullname { get; set; } = null!;
        public string Relationship { get; set; } = null!;
        public string? Gender { get; set; }
        public DateOnly? Birthdate { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public string? BookingId { get; set; }
    }
}
