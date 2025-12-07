namespace WebApi.Orders.Domain;

public class OrderItem
{
    public int Id { get; set; } // Primary key
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public Order Order { get; set; } = null!;
}
