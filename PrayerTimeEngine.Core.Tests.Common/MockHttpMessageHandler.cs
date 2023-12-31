﻿using System.Net;

namespace PrayerTimeEngine.Core.Tests.Common
{
    public class MockHttpMessageHandler(
            HttpStatusCode statusCode,
            Dictionary<string, string> urlToContentMap
        ) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Look up the response content in the dictionary
            string content;
            if (urlToContentMap.TryGetValue(request.RequestUri.AbsoluteUri, out content))
            {
                var responseMessage = new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                };

                return Task.FromResult(responseMessage);
            }
            else
            {
                // You could return a default response here if the URL wasn't found in the dictionary.
                // Or you could let the test fail if an unexpected URL is accessed.
                throw new KeyNotFoundException($"No response registered for URL: {request.RequestUri.AbsoluteUri}");
            }
        }
    }
}
