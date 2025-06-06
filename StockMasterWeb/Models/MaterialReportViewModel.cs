namespace StockMasterWeb.Models
{
    public class MaterialReportViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
