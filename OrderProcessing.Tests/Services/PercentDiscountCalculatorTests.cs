using OrderProcessing.Services;
using Xunit;

namespace OrderProcessing.Tests.Services;

public class PercentDiscountCalculatorTests
{
    private readonly PercentDiscountCalculator _calculator = new();

    [Theory]
    [InlineData(0, 0)]
    [InlineData(999, 0)]
    [InlineData(1000, 50)]        // 1000 * 5%
    [InlineData(4999, 249.95)]    // 4999 * 5%
    [InlineData(5000, 500)]       // 5000 * 10%
    [InlineData(9999, 999.9)]     // 9999 * 10%
    [InlineData(10000, 1500)]     // 10000 * 15%
    [InlineData(25000, 3750)]     // 25000 * 15%
    public void CalculateDiscount_VariousSubtotals_ReturnsExpectedDiscount(decimal subtotal, decimal expected)
    {
        Assert.Equal(expected, _calculator.CalculateDiscount(subtotal));
    }

    [Fact]
    public void CalculateDiscount_NegativeSubtotal_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.CalculateDiscount(-1m));
    }
}
