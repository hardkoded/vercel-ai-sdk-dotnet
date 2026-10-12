// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using static Vercel.AI.Tests.Upstream.AmazonBedrock.BedrockStreamSupport;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModel;

public sealed class DoStreamTextTests
{
    private const string Suite = "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doStream > text::";

    [Fact]
    [UpstreamTest(Suite + "should stream text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text_deltas()
    {
        var parts = await Stream(Fixture("text"));

        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "text-start 0",
                "text-delta 0 \"Let\"",
                "text-delta 0 \" me count the \\\"\"",
                "text-delta 0 \"r\\\"s in \\\"\"",
                "text-delta 0 \"strawberry\\\":\\n\\ns-t-\"",
                "text-delta 0 \"**\"",
                "text-delta 0 \"r**-a-w-b\"",
                "text-delta 0 \"-e-**\"",
                "text-delta 0 \"r**-**\"",
                "text-delta 0 \"r**-y\\n\\nThere\"",
                "text-delta 0 \" are **3\"",
                "text-delta 0 \"** r\"",
                "text-delta 0 \"'s in \\\"strawberry.\\\"\"",
                "text-end 0",
                "finish stop raw=end_turn usage=in(22,22,0,0) out(55,55,undefined) raw={\"inputTokens\":22,\"outputTokens\":55,\"serverToolUsage\":{},\"totalTokens\":77}",
            },
            Render(parts));
    }

    [Fact]
    [UpstreamTest(Suite + "should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_the_raw_response_headers()
    {
        var result = await StreamModel(Fixture("text", new Dictionary<string, string> { ["test-header"] = "test-value" }));

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["cache-control"] = "no-cache",
                ["connection"] = "keep-alive",
                ["content-type"] = "application/vnd.amazon.eventstream",
                ["test-header"] = "test-value",
            },
            result.Response!.Headers);
    }

    [Fact]
    [UpstreamTest(Suite + "should return the request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_the_request_body()
    {
        var result = await StreamModel(Fixture("text"));

        var body = result.Request!.Body!.Value;
        Assert.True(JsonEquals(
            """
            {"additionalModelResponseFieldPaths":["/delta/stop_sequence"],"messages":[{"content":[{"text":"Hello"}],"role":"user"}],"system":[{"text":"System Prompt"}]}
            """,
            body));
    }

    private static async Task<Vercel.AI.Provider.LanguageModelStreamResult> StreamModel(UpstreamEventStreamHandler handler)
    {
        var model = (AmazonBedrockLanguageModel)CreateModel(handler);
        var result = await model.DoStreamAsync(Call(), CancellationToken.None);
        await foreach (var _ in result)
        {
        }

        return result;
    }
}
