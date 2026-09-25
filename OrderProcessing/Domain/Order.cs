namespace OrderProcessing.Domain;

/// <summary>
/// Заказ клиента. Хранит позиции и управляет собственным статусом:
/// переход в новый статус возможен только через TransitionTo,
/// что не позволяет получить объект в недопустимом состоянии.
/// </summary>
public class Order
{
    private readonly List<OrderItem> _items = new();

    public int Id { get; }
    public string CustomerEmail { get; }
    public OrderStatus Status { get; private set; } = OrderStatus.New;
    public IReadOnlyList<OrderItem> Items => _items;

    public Order(int id, string customerEmail)
    {
        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new ArgumentException("Email клиента обязателен.", nameof(customerEmail));

        Id = id;
        CustomerEmail = customerEmail;
    }

    /// <summary>
    /// Добавляет позицию. Если товар с таким ProductId уже есть в заказе,
    /// количество суммируется вместо создания дубликата строки.
    /// </summary>
    public void AddItem(int productId, string productName, int quantity, decimal unitPrice)
    {
        foreach (var existing in _items)
        {
            if (existing.ProductId == productId)
            {
                existing.IncreaseQuantity(quantity);
                return;
            }
        }

        _items.Add(new OrderItem(productId, productName, quantity, unitPrice));
    }

    public decimal GetSubtotal()
    {
        decimal total = 0m;
        foreach (var item in _items)
            total += item.GetLineTotal();
        return total;
    }

    /// <summary>Матрица допустимых переходов статуса. Исчерпывающий switch — без default.</summary>
    /// <summary>
    /// Матрица допустимых переходов статуса.
    /// Явная ветвь <c>_ => false</c> закрывает случай, когда Status
    /// содержит значение, не входящее в перечисление (например, при
    /// десериализации из внешнего источника) — это делает ветвление
    /// полностью покрываемым тестами.
    /// </summary>
    public bool CanTransitionTo(OrderStatus target) => Status switch
    {
        OrderStatus.New => target is OrderStatus.Confirmed or OrderStatus.Cancelled,
        OrderStatus.Confirmed => target is OrderStatus.Paid or OrderStatus.Cancelled,
        OrderStatus.Paid => target is OrderStatus.Shipped,
        OrderStatus.Shipped => false,
        OrderStatus.Cancelled => false,
        _ => false,
    };

    public void TransitionTo(OrderStatus target)
    {
        if (!CanTransitionTo(target))
            throw new InvalidOperationException($"Переход из статуса {Status} в {target} запрещён.");
        Status = target;
    }
}
