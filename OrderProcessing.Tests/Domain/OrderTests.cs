using OrderProcessing.Domain;
using Xunit;

namespace OrderProcessing.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void Constructor_ValidEmail_SetsProperties()
    {
        var order = new Order(1, "client@example.com");

        Assert.Equal(1, order.Id);
        Assert.Equal("client@example.com", order.CustomerEmail);
        Assert.Equal(OrderStatus.New, order.Status);
        Assert.Empty(order.Items);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_InvalidEmail_Throws(string? email)
    {
        Assert.Throws<ArgumentException>(() => new Order(1, email!));
    }

    [Fact]
    public void AddItem_NewProduct_AddsToList()
    {
        var order = new Order(1, "a@b.com");
        order.AddItem(10, "Мышь", 1, 990m);

        Assert.Single(order.Items);
        Assert.Equal(10, order.Items[0].ProductId);
    }

    [Fact]
    public void AddItem_ExistingProduct_MergesQuantityInsteadOfDuplicating()
    {
        var order = new Order(1, "a@b.com");
        order.AddItem(10, "Мышь", 2, 990m);   // ветка "не найдено" — добавление
        order.AddItem(10, "Мышь", 3, 990m);   // ветка "найдено" — слияние

        Assert.Single(order.Items);
        Assert.Equal(5, order.Items[0].Quantity);
    }

    [Fact]
    public void AddItem_DifferentProduct_AddsAsNewItem()
    {
        var order = new Order(1, "a@b.com");
        order.AddItem(10, "Мышь", 1, 990m);
        order.AddItem(20, "Клавиатура", 1, 2990m); // другой ProductId, ветка "не найдено" при непустом списке

        Assert.Equal(2, order.Items.Count);
        Assert.Equal(10, order.Items[0].ProductId);
        Assert.Equal(20, order.Items[1].ProductId);
    }

    [Fact]
    public void GetSubtotal_NoItems_ReturnsZero()
    {
        var order = new Order(1, "a@b.com");
        Assert.Equal(0m, order.GetSubtotal());
    }

    [Fact]
    public void GetSubtotal_WithItems_ReturnsSumOfLineTotals()
    {
        var order = new Order(1, "a@b.com");
        order.AddItem(1, "A", 2, 100m);  // 200
        order.AddItem(2, "B", 1, 50m);   // 50

        Assert.Equal(250m, order.GetSubtotal());
    }

    [Theory]
    [InlineData(OrderStatus.New, OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.New, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.New, OrderStatus.Paid, false)]
    [InlineData(OrderStatus.New, OrderStatus.Shipped, false)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Paid, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Shipped, false)]
    [InlineData(OrderStatus.Paid, OrderStatus.Shipped, true)]
    [InlineData(OrderStatus.Paid, OrderStatus.Cancelled, false)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled, false)]
    public void CanTransitionTo_VariousPairs_ReturnsExpected(OrderStatus from, OrderStatus to, bool expected)
    {
        var order = new Order(1, "a@b.com");
        MoveTo(order, from);

        Assert.Equal(expected, order.CanTransitionTo(to));
    }

    [Fact]
    public void CanTransitionTo_FromCancelled_AlwaysFalse()
    {
        var order = new Order(1, "a@b.com");
        order.TransitionTo(OrderStatus.Cancelled);

        Assert.False(order.CanTransitionTo(OrderStatus.New));
        Assert.False(order.CanTransitionTo(OrderStatus.Confirmed));
    }

    [Fact]
    public void CanTransitionTo_InvalidStatusValue_ReturnsFalse()
    {
        var order = new Order(1, "a@b.com");

        // Обходим private setter через рефлексию, чтобы Status получил
        // значение, не входящее в перечисление OrderStatus.
        // Это покрывает default-ветвь switch-выражения в CanTransitionTo.
        typeof(Order)
            .GetProperty(nameof(Order.Status))!
            .SetValue(order, (OrderStatus)99);

        Assert.False(order.CanTransitionTo(OrderStatus.New));
        Assert.False(order.CanTransitionTo(OrderStatus.Confirmed));
        Assert.False(order.CanTransitionTo(OrderStatus.Paid));
        Assert.False(order.CanTransitionTo(OrderStatus.Shipped));
        Assert.False(order.CanTransitionTo(OrderStatus.Cancelled));
    }

    [Fact]
    public void TransitionTo_AllowedTarget_ChangesStatus()
    {
        var order = new Order(1, "a@b.com");
        order.TransitionTo(OrderStatus.Confirmed);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void TransitionTo_DisallowedTarget_ThrowsInvalidOperationException()
    {
        var order = new Order(1, "a@b.com");
        Assert.Throws<InvalidOperationException>(() => order.TransitionTo(OrderStatus.Paid));
    }

    /// <summary>Переводит заказ в нужный статус через цепочку разрешённых переходов.</summary>
    private static void MoveTo(Order order, OrderStatus target)
    {
        if (target == OrderStatus.New) return;
        if (target is OrderStatus.Confirmed or OrderStatus.Paid or OrderStatus.Shipped)
            order.TransitionTo(OrderStatus.Confirmed);
        if (target is OrderStatus.Paid or OrderStatus.Shipped)
            order.TransitionTo(OrderStatus.Paid);
        if (target is OrderStatus.Shipped)
            order.TransitionTo(OrderStatus.Shipped);
    }
}
