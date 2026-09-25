using Moq;
using OrderProcessing.Domain;
using OrderProcessing.Exceptions;
using OrderProcessing.Services;
using Xunit;

namespace OrderProcessing.Tests.Services;

public class OrderProcessorTests
{
    private readonly Mock<IDiscountCalculator> _discountMock = new();
    private readonly Mock<INotificationService> _notifierMock = new();
    private readonly OrderProcessor _processor;

    public OrderProcessorTests()
    {
        _processor = new OrderProcessor(_discountMock.Object, _notifierMock.Object);
        _discountMock.Setup(x => x.CalculateDiscount(It.IsAny<decimal>())).Returns(0m);
    }

    private static Order CreateConfirmedOrder(decimal price = 1000m, int qty = 1)
    {
        var order = new Order(1, "client@example.com");
        order.AddItem(1, "Товар", qty, price);
        order.TransitionTo(OrderStatus.Confirmed);
        return order;
    }

    // ---------- конструктор ----------

    [Fact]
    public void Constructor_NullDiscountCalculator_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new OrderProcessor(null!, _notifierMock.Object));
    }

    [Fact]
    public void Constructor_NullNotifier_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new OrderProcessor(_discountMock.Object, null!));
    }

    // ---------- Checkout: базовые проверки ----------

    [Fact]
    public void Checkout_NullOrder_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _processor.Checkout(null!, PaymentMethod.Card));
    }

    [Fact]
    public void Checkout_EmptyItems_ThrowsInvalidOperationException()
    {
        var order = new Order(1, "a@b.com");
        Assert.Throws<InvalidOperationException>(() => _processor.Checkout(order, PaymentMethod.Card));
    }

    [Fact]
    public void Checkout_OrderNotConfirmed_ThrowsInvalidOperationException()
    {
        var order = new Order(1, "a@b.com");
        order.AddItem(1, "Товар", 1, 100m); // остаётся в статусе New

        Assert.Throws<InvalidOperationException>(() => _processor.Checkout(order, PaymentMethod.Card));
    }

    // ---------- Checkout: способы оплаты ----------

    [Fact]
    public void Checkout_CardPayment_Success_ReturnsSubtotalMinusDiscount()
    {
        _discountMock.Setup(x => x.CalculateDiscount(300m)).Returns(30m);
        var order = CreateConfirmedOrder(price: 300m, qty: 1);

        var total = _processor.Checkout(order, PaymentMethod.Card);

        Assert.Equal(270m, total);
        Assert.Equal(OrderStatus.Paid, order.Status);
        _notifierMock.Verify(x => x.SendOrderConfirmation("client@example.com", 1, 270m), Times.Once);
    }

    [Fact]
    public void Checkout_CashPayment_ExceedsLimit_ThrowsArgumentException()
    {
        var order = CreateConfirmedOrder(price: 200000m, qty: 1);
        Assert.Throws<ArgumentException>(() => _processor.Checkout(order, PaymentMethod.Cash));
    }

    [Fact]
    public void Checkout_CashPayment_WithinLimit_Succeeds()
    {
        var order = CreateConfirmedOrder(price: 500m, qty: 2); // 1000
        var total = _processor.Checkout(order, PaymentMethod.Cash);
        Assert.Equal(1000m, total);
    }

    [Fact]
    public void Checkout_BankTransfer_BelowMinimum_ThrowsArgumentException()
    {
        var order = CreateConfirmedOrder(price: 50m, qty: 1);
        Assert.Throws<ArgumentException>(() => _processor.Checkout(order, PaymentMethod.BankTransfer));
    }

    [Fact]
    public void Checkout_BankTransfer_AtMinimum_Succeeds()
    {
        var order = CreateConfirmedOrder(price: 150m, qty: 1);
        var total = _processor.Checkout(order, PaymentMethod.BankTransfer);
        Assert.Equal(150m, total);
    }

    [Fact]
    public void Checkout_UnknownPaymentMethod_ThrowsArgumentOutOfRangeException()
    {
        var order = CreateConfirmedOrder();
        var invalidMethod = (PaymentMethod)42;

        Assert.Throws<ArgumentOutOfRangeException>(() => _processor.Checkout(order, invalidMethod));
    }

    // ---------- Checkout: сбой уведомления ----------

    [Fact]
    public void Checkout_NotifierThrows_WrapsInNotificationFailedException()
    {
        var order = CreateConfirmedOrder();
        _notifierMock
            .Setup(x => x.SendOrderConfirmation(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()))
            .Throws(new InvalidOperationException("SMTP недоступен"));

        var ex = Assert.Throws<NotificationFailedException>(() => _processor.Checkout(order, PaymentMethod.Card));

        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Equal(OrderStatus.Paid, order.Status); // оплата проведена, несмотря на сбой уведомления
    }

    // ---------- CancelOrder ----------

    [Fact]
    public void CancelOrder_NullOrder_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _processor.CancelOrder(null!));
    }

    [Fact]
    public void CancelOrder_NewOrder_TransitionsAndNotifies()
    {
        var order = new Order(5, "client@example.com");

        _processor.CancelOrder(order);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        _notifierMock.Verify(x => x.SendCancellationNotice("client@example.com", 5), Times.Once);
    }

    [Fact]
    public void CancelOrder_AlreadyPaidOrder_ThrowsInvalidOperationException()
    {
        var order = CreateConfirmedOrder();
        _processor.Checkout(order, PaymentMethod.Card); // статус становится Paid

        Assert.Throws<InvalidOperationException>(() => _processor.CancelOrder(order));
    }

    [Fact]
    public void CancelOrder_NotifierThrows_WrapsInNotificationFailedException()
    {
        var order = new Order(9, "client@example.com");
        _notifierMock
            .Setup(x => x.SendCancellationNotice(It.IsAny<string>(), It.IsAny<int>()))
            .Throws(new TimeoutException("нет ответа от шлюза"));

        var ex = Assert.Throws<NotificationFailedException>(() => _processor.CancelOrder(order));

        Assert.IsType<TimeoutException>(ex.InnerException);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }
}
