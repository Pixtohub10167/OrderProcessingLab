namespace OrderProcessing.Services;

/// <summary>Ступенчатая скидка в процентах от суммы заказа.</summary>
public class PercentDiscountCalculator : IDiscountCalculator
{
    public decimal CalculateDiscount(decimal subtotal)
    {
        if (subtotal < 0)
            throw new ArgumentOutOfRangeException(nameof(subtotal), "Сумма заказа не может быть отрицательной.");

        if (subtotal >= 10000m) return subtotal * 0.15m;
        if (subtotal >= 5000m)  return subtotal * 0.10m;
        if (subtotal >= 1000m)  return subtotal * 0.05m;
        return 0m;
    }
}
