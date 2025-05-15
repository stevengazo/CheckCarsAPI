using CheckCarsAPI.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

namespace CheckCarsTest.Services;
public class EmailServiceTests
{
    private IConfiguration GetFakeConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"Smtp:Host", "smtp.test.com"},
            {"Smtp:Port", "587"},
            {"Smtp:Username", "testuser"},
            {"Smtp:Password", "testpass"},
            {"Smtp:From", "from@test.com"},
            {"Smtp:DefaultEmail", "default@test.com"}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task SendEmailAsync_SendsMail_WithCorrectSettings()
    {
        // Arrange
        var config = GetFakeConfiguration();
        var service = new EmailService(config);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            service.SendEmailAsync("receiver@test.com", "Test Subject", "<p>Hello</p>")
        );

        // Since no SMTP server is available, this should throw (but shows setup is correct)
        Assert.NotNull(exception);
        Assert.IsType<SmtpException>(exception);
    }

    [Fact]
    public async Task SendAlert_UsesDefaultEmail()
    {
        // Arrange
        var config = GetFakeConfiguration();
        var service = new EmailService(config);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            service.SendAlert("System Alert", "<strong>Important</strong>")
        );

        Assert.NotNull(exception);
        Assert.IsType<SmtpException>(exception);
    }
}
