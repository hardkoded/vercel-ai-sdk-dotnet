// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; result.response</c>.</summary>
public sealed class ResultResponseTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.response::";

    [Fact]
    [UpstreamTest(Prefix + "should resolve with response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_with_response_information()
    {
        var model = new TestLanguageModel("mock-model-id")
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0)),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Hello"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
            StreamResponse = new LanguageModelStreamResponse(new Dictionary<string, string> { ["call"] = "2" }),
        };
        var result = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "prompt",
        });

        var response = await result.Response;
        Assert.Equal(new Dictionary<string, string> { ["call"] = "2" }, response.Headers);
        Assert.Equal("id-0", response.Id);
        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(response.Messages));
        Assert.Equal("Hello", message.Text);
        Assert.Equal("mock-model-id", response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(0), response.Timestamp);
    }
}
