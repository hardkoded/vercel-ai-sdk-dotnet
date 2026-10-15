// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; doStream</c>.</summary>
public sealed class GatewayLanguageModelDoStreamTests
{
    private const string Prefix = "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::";

    [Fact]
    [UpstreamTest(Prefix + "should forward server stream-start warnings exactly once", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_server_stream_start_warnings_exactly_once()
    {
        var parts = await Stream(
            "data: {\"type\":\"stream-start\",\"warnings\":[{\"type\":\"other\",\"message\":\"from provider\"}]}\n\n" +
            "data: {\"type\":\"text-delta\",\"textDelta\":\"Hello\"}\n\n" +
            "data: {\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20}}\n\n");

        var starts = parts.OfType<StreamStartStreamPart>().ToList();
        var start = Assert.Single(starts);
        var warning = Assert.Single(start.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("from provider", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve explicit mid-stream provider error metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_explicit_mid_stream_provider_error_metadata()
    {
        var parts = await Stream(
            "data: {\"type\":\"text-delta\",\"textDelta\":\"Partial output\"}\n\n" +
            "data: {\"type\":\"error\",\"error\":{\"message\":\"Upstream provider overloaded\",\"type\":\"provider_overloaded\",\"statusCode\":503,\"isRetryable\":true}}\n\n");

        Assert.Equal(2, parts.Count);
        Assert.Equal("Partial output", Assert.IsType<TextDeltaStreamPart>(parts[0]).Delta);
        var error = Assert.IsType<ErrorStreamPart>(parts[1]).Error;
        Assert.NotNull(error);
        Assert.Equal("Upstream provider overloaded", error.Message);
        Assert.Equal("provider_overloaded", error.Type);
        Assert.Equal(503, error.StatusCode);
        Assert.True(error.IsRetryable);
        var data = Assert.IsType<JsonElement>(error.Data);
        Assert.Equal("Upstream provider overloaded", data.GetProperty("message").GetString());
    }

    private static async Task<List<LanguageModelStreamPart>> Stream(string body)
    {
        var handler = new UpstreamRecordingHandler(body);
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler);
        var result = await provider.LanguageModel("test-model").DoStreamAsync(
            new LanguageModelCallOptions { IncludeRawChunks = false },
            CancellationToken.None);
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in result)
        {
            parts.Add(part);
        }

        return parts;
    }
}
