namespace DNASystemBackend.DTOs
{
    public class UpdateUserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string RoleId { get; set; }
        public string? Email { get; set; }
        public string? Fullname { get; set; }
        public string? Phone { get; set; }

        public DateOnly? Birthdate { get; set; }

        public string? Address { get; set; }

        public string? Image { get; set; }
    }

}
