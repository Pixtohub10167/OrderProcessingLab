namespace OrderProcessing.Exceptions;

/// <summary>
/// Оборачивает любую ошибку канала уведомлений (SMTP недоступен, таймаут и т.п.),
/// чтобы вызывающий код мог отличить бизнес-ошибку от сбоя инфраструктуры.
/// </summary>
public class NotificationFailedException : Exception
{
    public NotificationFailedException(string message, Exception inner) : base(message, inner) { }
}
