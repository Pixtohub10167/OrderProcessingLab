namespace OrderProcessing.Services;

/// <summary>Абстракция расчёта скидки — позволяет подменять реализацию и мокать в тестах.</summary>
public interface IDiscountCalculator
{
    decimal CalculateDiscount(decimal subtotal);
}
