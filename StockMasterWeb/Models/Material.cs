using System;
using System.Collections.Generic;

namespace StockMasterWeb.Models;

public partial class Material
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Quantity { get; set; }

    public string? Unit { get; set; }

    public int? SupplierId { get; set; }

    public virtual Supplier? Supplier { get; set; }

    public virtual ICollection<WarehouseItem> WarehouseItems { get; set; } = new List<WarehouseItem>();
}
