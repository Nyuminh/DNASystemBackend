using System;
using System.Collections.Generic;

namespace DNASystemBackend.Models;

public partial class TestResult
{
    public string ResultId { get; set; } = null!;

    public string? CustomerId { get; set; }

    public string? StaffId { get; set; }

    public string? ServiceId { get; set; }

    public string? BookingId { get; set; }

    public DateTime? Date { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual User? Customer { get; set; }

    public virtual Service? Service { get; set; }

    public virtual User? Staff { get; set; }
}
