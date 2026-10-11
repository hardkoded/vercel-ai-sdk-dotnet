// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResponseHandler;

public sealed class CreateStatusCodeErrorResponseHandlerTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createStatusCodeErrorResponseHandler::should create error with status text and response body",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_create_error_with_status_text_and_response_body()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Error message", Encoding.UTF8),
            ReasonPhrase = "Not Found",
        };
        var requestBodyValues = new Dictionary<string, string> { ["some"] = "data" };

        var error = await ResponseHandlers.CreateStatusCodeErrorResponseAsync(response, "test-url", requestBodyValues);

        Assert.Equal("Not Found", error.Message);
        Assert.Equal(404, error.StatusCode);
        Assert.Equal("Error message", error.ResponseBody);
        Assert.Equal("test-url", error.Url);
        Assert.Equal(requestBodyValues, error.RequestBodyValues);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createStatusCodeErrorResponseHandler::should reject oversized responses before reading the body",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_oversized_responses_before_reading_the_body()
    {
        var (response, content) = OversizedResponseContent.Create(
            "too large",
            HttpStatusCode.InternalServerError,
            "Internal Server Error");

        var error = await Assert.ThrowsAsync<DownloadError>(() => ResponseHandlers.CreateStatusCodeErrorResponseAsync(
            response,
            "test-url",
            new Dictionary<string, string> { ["some"] = "data" }));

        Assert.Contains("exceeded maximum size", error.Message);
        Assert.False(content.BodyRead);
        Assert.True(content.Cancelled);
    }
}
