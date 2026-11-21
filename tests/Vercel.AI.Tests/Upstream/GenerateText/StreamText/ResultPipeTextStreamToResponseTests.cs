// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; result.pipeTextStreamToResponse</c>.</summary>
public sealed class ResultPipeTextStreamToResponseTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > result.pipeTextStreamToResponse::should write text deltas to a Node.js response-like object",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_write_text_deltas_to_a_Node_js_response_like_object()
    {
        var response = new ServerResponse();
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Hello"),
                new TextDeltaStreamPart("1", ", "),
                new TextDeltaStreamPart("1", "world!"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
        var result = new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "test-input" });

        await result.PipeTextStreamToResponseAsync(response);
        await response.WaitForEndAsync();

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", response.Headers["content-type"]);
        Assert.Single(response.Headers);
        Assert.Equal(["Hello", ", ", "world!"], response.GetDecodedChunks());
    }
}
