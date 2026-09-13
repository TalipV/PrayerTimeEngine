namespace PrayerTimeEngine.Core.Tests.Common;

public class MockHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> handleRequestFunc = null
    ) : HttpMessageHandler
{
    public Func<HttpRequestMessage, HttpResponseMessage> HandleRequestFunc { get; set; } = handleRequestFunc;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(HandleRequestFunc(request));
    }
}
