namespace AgroConnect.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser? ApplicationUser { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        
        // Sifariş ünvanı
        public string ShippingAddress { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        
        // Qapıda Ödəmə
        public string PaymentMethod { get; set; } = "Qapıda Ödəmə";

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }
}
