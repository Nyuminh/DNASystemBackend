namespace DNASystemBackend.DTOs
{
    public class RegisterDto

    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }

        public DateOnly? Birthdate { get; set; }
        public string? Image { get; set; }
    }
}