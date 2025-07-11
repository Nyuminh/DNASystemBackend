using System;
using System.Collections.Generic;

namespace DNASystemBackend.Models;

public partial class Relative
{
    public string RelativeId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string Fullname { get; set; } = null!;

    public string Relationship { get; set; } = null!;

    public string? Gender { get; set; }

    public DateOnly? Birthdate { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? BookingId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual User User { get; set; } = null!;
}
