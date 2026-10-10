// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResponseHandler;

public sealed class CreateBinaryResponseHandlerTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createBinaryResponseHandler::should handle binary response successfully",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_binary_response_successfully()
    {
        var binaryData = new byte[] { 1, 2, 3, 4 };
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(binaryData) };

        var result = await ResponseHandlers.CreateBinaryResponseAsync(response, "test-url", new Dictionary<string, object?>());

        Assert.IsType<byte[]>(result.Value);
        Assert.Equal(binaryData, result.Value);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createBinaryResponseHandler::should throw APICallError when response body is null",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_APICallError_when_response_body_is_null()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = null };

        var error = await Assert.ThrowsAsync<ApiException>(() => ResponseHandlers.CreateBinaryResponseAsync(
            response,
            "test-url",
            new Dictionary<string, object?>()));

        Assert.Contains("Response body is empty", error.Message);
    }
}
