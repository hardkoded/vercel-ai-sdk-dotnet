// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelStreamSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
    private const string Prefix = TestFile + " > doStream::";

    [Theory]
    [InlineData("missing", FinishReason.Interrupted)]
    [InlineData("unknown_reason", FinishReason.Other)]
    [InlineData("invalid-json", FinishReason.Error)]
    [UpstreamTest(
        Prefix + "handles a stream ending with %s as %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Handles_a_stream_ending_with_ending_as_unified(
        string ending,
        FinishReason unified
    )
    {
        var handler = Chunks(
            new[]
            {
                "data: {\"id\":\"response-1\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Partial answer\"},\"finish_reason\":null}]}\n\n",
            }
                .Concat(
                    ending switch
                    {
                        "missing" => Array.Empty<string>(),
                        "invalid-json" => new[] { "data: {invalid-json}\n\n" },
                        _ => new[]
                        {
                            "data: {\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\""
                                + ending
                                + "\"}]}\n\n",
                        },
                    }
                )
                .ToArray()
        );

        var parts = await Stream(Chat(handler), TestPrompt());

        Assert.Contains(parts.OfType<TextDeltaStreamPart>(), part =>
            part.Delta == "Partial answer"
        );
        Assert.Equal(ending == "invalid-json" ? 1 : 0, parts.OfType<ErrorStreamPart>().Count());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(unified, finish.FinishReason);
        Assert.Equal(ending == "unknown_reason" ? ending : null, finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve a provider error envelope in stream errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_a_provider_error_envelope_in_stream_errors()
    {
        const string data =
            """{"error":{"message":"Rate limit reached","type":"rate_limit_error","code":"rate_limit_exceeded"}}""";
        var handler = Chunks("data: " + data + "\n\n", "data: [DONE]\n\n");

        var chunks = await Stream(Chat(handler), TestPrompt());
        var errorPart = Assert.IsType<ErrorStreamPart>(
            chunks.First(chunk => chunk.Type == "error")
        );

        var error = Assert.IsType<ProviderStreamError>(errorPart.Error);
        Assert.Equal("Rate limit reached", error.Message);
        Assert.Equal("rate_limit_error", error.Type);
        Assert.Equal("rate_limit_exceeded", error.Code);
        Assert.Equal(429, error.StatusCode);
        Assert.True(error.IsRetryable);
        JsonAssert.Equal((JsonElement)error.Data!, data);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should classify insufficient quota as non-retryable",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_classify_insufficient_quota_as_non_retryable()
    {
        const string data =
            """{"error":{"message":"You exceeded your current quota.","type":"rate_limit_error","code":"insufficient_quota"}}""";
        var handler = Chunks("data: " + data + "\n\n", "data: [DONE]\n\n");

        var chunks = await Stream(Chat(handler), TestPrompt());
        var errorPart = Assert.IsType<ErrorStreamPart>(
            chunks.First(chunk => chunk.Type == "error")
        );

        var error = Assert.IsType<ProviderStreamError>(errorPart.Error);
        Assert.Equal("You exceeded your current quota.", error.Message);
        Assert.Equal("rate_limit_error", error.Type);
        Assert.Equal("insufficient_quota", error.Code);
        Assert.Equal(429, error.StatusCode);
        Assert.False(error.IsRetryable);
        JsonAssert.Equal((JsonElement)error.Data!, data);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve the provider type when code is an HTTP status",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_the_provider_type_when_code_is_an_HTTP_status()
    {
        const string data =
            """{"error":{"message":"Rate limit reached","type":"rate_limit_error","code":"429"}}""";
        var handler = Chunks("data: " + data + "\n\n", "data: [DONE]\n\n");

        var chunks = await Stream(Chat(handler), TestPrompt());
        var errorPart = Assert.IsType<ErrorStreamPart>(
            chunks.First(chunk => chunk.Type == "error")
        );

        var error = Assert.IsType<ProviderStreamError>(errorPart.Error);
        Assert.Equal("Rate limit reached", error.Message);
        Assert.Equal("rate_limit_error", error.Type);
        Assert.Equal("429", error.Code);
        Assert.Equal(429, error.StatusCode);
        Assert.True(error.IsRetryable);
        JsonAssert.Equal((JsonElement)error.Data!, data);
    }
}
