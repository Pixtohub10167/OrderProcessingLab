using OrderProcessing.Domain;
using OrderProcessing.Exceptions;

namespace OrderProcessing.Services;

/// <summary>
/// Оформление и отмена заказов. Обе зависимости приходят через конструктор
/// (внедрение зависимостей) — класс не создаёт их самостоятельно и не знает
/// о конкретной реализации скидок или канала уведомлений.
/// </summary>
public class OrderProcessor
{
    private readonly IDiscountCalculator _discountCalculator;
    private readonly INotificationService _notifier;

    public OrderProcessor(IDiscountCalculator discountCalculator, INotificationService notifier)
    {
        _discountCalculator = discountCalculator ?? throw new ArgumentNullException(nameof(discountCalculator));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    /// <summary>
    /// Проводит оплату заказа. Заказ должен быть в статусе Confirmed —
    /// иначе TransitionTo(Paid) отклонит переход.
    /// </summary>
    public decimal Checkout(Order order, PaymentMethod method)
    {
        if (order is null)
            throw new ArgumentNullException(nameof(order));
        if (order.Items.Count == 0)
            throw new InvalidOperationException("Нельзя оформить заказ без позиций.");

        var subtotal = order.GetSubtotal();
        var discount = _discountCalculator.CalculateDiscount(subtotal);
        var total = subtotal - discount;

        switch (method)
        {
            case PaymentMethod.Card:
                break; // дополнительных ограничений нет
            case PaymentMethod.Cash:
                if (total > 100000m)
                    throw new ArgumentException(
                        "Оплата наличными свыше 100 000 руб. не допускается.", nameof(method));
                break;
            case PaymentMethod.BankTransfer:
                if (total < 100m)
                    throw new ArgumentException(
                        "Минимальная сумма банковского перевода — 100 руб.", nameof(method));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(method), method, "Неизвестный способ оплаты.");
        }

        // Может выбросить InvalidOperationException, если заказ не был предварительно подтверждён
        order.TransitionTo(OrderStatus.Paid);

        try
        {
            _notifier.SendOrderConfirmation(order.CustomerEmail, order.Id, total);
        }
        catch (Exception ex)
        {
            throw new NotificationFailedException(
                $"Не удалось отправить подтверждение заказа №{order.Id}.", ex);
        }

        return total;
    }

    public void CancelOrder(Order order)
    {
        if (order is null)
            throw new ArgumentNullException(nameof(order));

        order.TransitionTo(OrderStatus.Cancelled);

        try
        {
            _notifier.SendCancellationNotice(order.CustomerEmail, order.Id);
        }
        catch (Exception ex)
        {
            throw new NotificationFailedException(
                $"Не удалось отправить уведомление об отмене заказа №{order.Id}.", ex);
        }
    }
}
