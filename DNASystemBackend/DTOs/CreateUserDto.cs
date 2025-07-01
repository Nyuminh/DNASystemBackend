namespace DNASystemBackend.DTOs
{
    public class CreateUserDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? Image { get; set; }
        public DateOnly? Birthdate { get; set; }
    }

}
