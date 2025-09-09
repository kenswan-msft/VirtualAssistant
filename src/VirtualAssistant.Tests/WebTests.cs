using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using Projects;

namespace VirtualAssistant.Tests;

public class WebTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

    [Fact(Skip = "Timeout occurs in CI pipeline")]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        IDistributedApplicationTestingBuilder appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<VirtualAssistant_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
            // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using DistributedApplication app =
#pragma warning disable CA2007
            await appHost.BuildAsync(cancellationToken)
                .WaitAsync(DefaultTimeout, cancellationToken);
#pragma warning restore CA2007

        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        HttpClient? httpClient = app.CreateHttpClient("team-web");

        await app.ResourceNotifications.WaitForResourceHealthyAsync("team-web", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        HttpResponseMessage? response = await httpClient.GetAsync("/", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
