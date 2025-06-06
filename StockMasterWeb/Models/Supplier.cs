using System;
using System.Collections.Generic;

namespace StockMasterWeb.Models;

public partial class Supplier
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactPhone { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
}
