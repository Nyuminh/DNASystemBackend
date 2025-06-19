using System;
using System.Collections.Generic;

namespace DNASystemBackend.Models;

public partial class Kit
{
    public string KitId { get; set; } = null!;

    public string? CustomerId { get; set; }

    public string? StaffId { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public DateTime? Receivedate { get; set; }

    public virtual User? Customer { get; set; }

    public virtual User? Staff { get; set; }
}
