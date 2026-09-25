namespace OrderProcessing.Services;

/// <summary>Абстракция отправки уведомлений клиенту — вторая точка внедрения зависимости.</summary>
public interface INotificationService
{
    void SendOrderConfirmation(string email, int orderId, decimal total);
    void SendCancellationNotice(string email, int orderId);
}
