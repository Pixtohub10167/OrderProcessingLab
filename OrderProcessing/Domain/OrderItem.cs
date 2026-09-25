namespace OrderProcessing.Domain;

/// <summary>Позиция заказа: конкретный товар, его количество и цена на момент добавления.</summary>
public class OrderItem
{
    public int ProductId { get; }
    public string ProductName { get; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; }

    public OrderItem(int productId, string productName, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Количество должно быть больше нуля.");
        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Цена не может быть отрицательной.");

        ProductId = productId;
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>Увеличивает количество — используется при повторном добавлении того же товара.</summary>
    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Величина увеличения должна быть больше нуля.");
        Quantity += amount;
    }

    public decimal GetLineTotal() => Quantity * UnitPrice;
}
