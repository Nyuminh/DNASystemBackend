using System;
using System.Collections.Generic;

namespace DNASystemBackend.Models;

public partial class InvoiceDetail
{
    public string InvoicedetailId { get; set; } = null!;

    public string? InvoiceId { get; set; }

    public string? ServiceId { get; set; }

    public int? Quantity { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public virtual Service? Service { get; set; }
}
