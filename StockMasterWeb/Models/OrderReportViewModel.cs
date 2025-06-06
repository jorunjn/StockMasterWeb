namespace StockMasterWeb.Models
{
    public class OrderReportViewModel
    {
        public int OrderId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
