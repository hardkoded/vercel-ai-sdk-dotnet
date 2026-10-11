// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResponseHandler;

public sealed class CreateBinaryStreamResponseHandlerTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createBinaryStreamResponseHandler::should pass the response body through as a stream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_the_response_body_through_as_a_stream()
    {
        var binaryData = new byte[] { 1, 2, 3, 4 };
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(binaryData) };

        var result = await ResponseHandlers.CreateBinaryStreamResponseAsync(response);

        Assert.IsAssignableFrom<Stream>(result.Value);
        using var collected = new MemoryStream();
        await result.Value.CopyToAsync(collected);
        Assert.Equal(binaryData, collected.ToArray());
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createBinaryStreamResponseHandler::should throw EmptyResponseBodyError when response body is null",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_EmptyResponseBodyError_when_response_body_is_null()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = null };

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => ResponseHandlers.CreateBinaryStreamResponseAsync(response));

        Assert.Contains("Empty response body", error.Message);
    }
}
