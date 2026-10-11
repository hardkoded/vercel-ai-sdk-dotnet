// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResponseHandler;

public sealed class CreateJsonErrorResponseHandlerTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createJsonErrorResponseHandler::should reject oversized responses before reading the body",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_oversized_responses_before_reading_the_body()
    {
        var (response, content) = OversizedResponseContent.Create(
            "{\"error\":\"too large\"}",
            HttpStatusCode.InternalServerError,
            "Internal Server Error");
        var schema = JsonSchemas.Object(
            new[] { new KeyValuePair<string, JsonNode>("error", JsonSchemas.String()) },
            new[] { "error" });

        var error = await Assert.ThrowsAsync<DownloadError>(() => ResponseHandlers.CreateJsonErrorResponseAsync(
            response,
            "test-url",
            new Dictionary<string, object?>(),
            schema,
            value => value.GetProperty("error").GetString()!));

        Assert.Contains("exceeded maximum size", error.Message);
        Assert.False(content.BodyRead);
        Assert.True(content.Cancelled);
    }
}
