using System;
using System.Collections.Generic;

namespace StockMasterWeb.Models;

public partial class WarehouseItem
{
    public int Id { get; set; }

    public int? ProductId { get; set; }

    public int? MaterialId { get; set; }

    public int Quantity { get; set; }

    public virtual Material? Material { get; set; }

    public virtual Product? Product { get; set; }
}
