using OrderProcessing.Domain;
using Xunit;

namespace OrderProcessing.Tests.Domain;

public class OrderItemTests
{
    [Fact]
    public void Constructor_ValidArgs_SetsProperties()
    {
        var item = new OrderItem(1, "Ноутбук", 2, 54990m);

        Assert.Equal(1, item.ProductId);
        Assert.Equal("Ноутбук", item.ProductName);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(54990m, item.UnitPrice);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Constructor_NonPositiveQuantity_Throws(int qty)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrderItem(1, "Товар", qty, 100m));
    }

    [Fact]
    public void Constructor_NegativePrice_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrderItem(1, "Товар", 1, -1m));
    }

    [Fact]
    public void Constructor_NullProductName_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new OrderItem(1, null!, 1, 100m));
    }

    [Fact]
    public void IncreaseQuantity_PositiveAmount_IncreasesQuantity()
    {
        var item = new OrderItem(1, "Товар", 2, 100m);
        item.IncreaseQuantity(3);
        Assert.Equal(5, item.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IncreaseQuantity_NonPositiveAmount_Throws(int amount)
    {
        var item = new OrderItem(1, "Товар", 2, 100m);
        Assert.Throws<ArgumentOutOfRangeException>(() => item.IncreaseQuantity(amount));
    }

    [Fact]
    public void GetLineTotal_ReturnsQuantityTimesPrice()
    {
        var item = new OrderItem(1, "Товар", 3, 150.5m);
        Assert.Equal(451.5m, item.GetLineTotal());
    }
}
