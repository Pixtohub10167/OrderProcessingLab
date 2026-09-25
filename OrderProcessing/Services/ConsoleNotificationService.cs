namespace OrderProcessing.Services;

/// <summary>Простейшая «боевая» реализация — пишет в консоль. В реальном проекте это был бы SMTP/SMS-шлюз.</summary>
public class ConsoleNotificationService : INotificationService
{
    public void SendOrderConfirmation(string email, int orderId, decimal total) =>
        Console.WriteLine($"[MAIL to {email}] Заказ №{orderId} подтверждён. Сумма к оплате: {total:F2} руб.");

    public void SendCancellationNotice(string email, int orderId) =>
        Console.WriteLine($"[MAIL to {email}] Заказ №{orderId} отменён.");
}
