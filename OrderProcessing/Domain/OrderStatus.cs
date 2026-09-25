namespace OrderProcessing.Domain;

/// <summary>Жизненный цикл заказа.</summary>
public enum OrderStatus
{
    New,
    Confirmed,
    Paid,
    Shipped,
    Cancelled
}
