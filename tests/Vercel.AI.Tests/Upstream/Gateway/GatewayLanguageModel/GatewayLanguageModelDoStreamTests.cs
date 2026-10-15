// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Streaming;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; doStream</c>.</summary>
public sealed class GatewayLanguageModelDoStreamTests
{
    private const string Prefix = "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::";

    private static readonly LanguageModelCallOptions TestPrompt = new() { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };

    private static ILanguageModel CreateTestModel(HttpMessageHandler handler)
        => GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler).LanguageModel("test-model");

    private static GatedStreamHandler PrepareStreamResponse(string[] content)
    {
        var handler = new GatedStreamHandler();
        foreach (var text in content)
        {
            handler.Body.Write($"data: {{\"type\":\"text-delta\",\"textDelta\":\"{text}\"}}\n\n");
        }

        handler.Body.Write("data: {\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20}}\n\n");
        handler.Body.Complete();
        return handler;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream text deltas",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks the text deltas and the stop finish part. The port maps usage to its own usage type and does not keep the raw prompt_tokens and completion_tokens keys. See #466.")]
    public async Task Should_stream_text_deltas()
    {
        var handler = PrepareStreamResponse(new[] { "Hello", ", ", "World!" });

        var result = await CreateTestModel(handler).DoStreamAsync(TestPrompt, CancellationToken.None);
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in result)
        {
            parts.Add(part);
        }

        Assert.Collection(
            parts,
            part => Assert.Equal("Hello", Assert.IsType<TextDeltaStreamPart>(part).Delta),
            part => Assert.Equal(", ", Assert.IsType<TextDeltaStreamPart>(part).Delta),
            part => Assert.Equal("World!", Assert.IsType<TextDeltaStreamPart>(part).Delta),
            part => Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(part).FinishReason));
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass streaming headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_streaming_headers()
    {
        var handler = PrepareStreamResponse(new[] { "Test" });

        await CreateTestModel(handler).DoStreamAsync(TestPrompt, CancellationToken.None);

        Assert.Equal("4", handler.RequestHeaders["ai-language-model-specification-version"]);
        Assert.Equal("test-model", handler.RequestHeaders["ai-language-model-id"]);
        Assert.Equal("true", handler.RequestHeaders["ai-language-model-streaming"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert API call errors to Gateway errors in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_convert_API_call_errors_to_Gateway_errors_in_streaming()
    {
        var handler = new UpstreamRecordingHandler(
            "{\"error\":{\"message\":\"Rate limit exceeded\",\"type\":\"rate_limit_exceeded\"}}",
            HttpStatusCode.TooManyRequests);

        var error = await Assert.ThrowsAsync<GatewayRateLimitError>(() => CreateTestModel(handler).DoStreamAsync(TestPrompt, CancellationToken.None));

        Assert.Equal("Rate limit exceeded", error.Message);
        Assert.Equal(429, error.StatusCode);
        Assert.Equal("rate_limit_exceeded", error.Type);
    }
}
