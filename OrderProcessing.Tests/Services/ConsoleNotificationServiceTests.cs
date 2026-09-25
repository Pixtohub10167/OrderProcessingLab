using OrderProcessing.Services;
using Xunit;

namespace OrderProcessing.Tests.Services;

public class ConsoleNotificationServiceTests
{
    [Fact]
    public void SendOrderConfirmation_WritesMessageContainingKeyData()
    {
        var service = new ConsoleNotificationService();
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            service.SendOrderConfirmation("a@b.com", 42, 199.99m);
        }
        finally
        {
            Console.SetOut(original);
        }

        var output = writer.ToString();
        Assert.Contains("a@b.com", output);
        Assert.Contains("42", output);
        Assert.Contains("199", output);
    }

    [Fact]
    public void SendCancellationNotice_WritesMessageContainingKeyData()
    {
        var service = new ConsoleNotificationService();
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            service.SendCancellationNotice("a@b.com", 7);
        }
        finally
        {
            Console.SetOut(original);
        }

        var output = writer.ToString();
        Assert.Contains("a@b.com", output);
        Assert.Contains("7", output);
    }
}
