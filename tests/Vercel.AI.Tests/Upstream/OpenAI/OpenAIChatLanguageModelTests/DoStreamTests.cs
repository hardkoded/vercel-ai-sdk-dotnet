// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIChatLanguageModelTests;

/// <summary>Port of <c>openai-chat-language-model.test.ts</c> &gt; <c>doStream</c>.</summary>
public sealed class DoStreamTests
{
    private const string Prefix = "packages/openai/src/chat/openai-chat-language-model.test.ts::doStream::";

    // "undefined" stands for a stream that ends without any finish_reason chunk; null is a JSON null finish_reason.
    [Theory]
    [InlineData("undefined", FinishReason.Interrupted)]
    [InlineData(null, FinishReason.Interrupted)]
    [InlineData("stop", FinishReason.Stop)]
    [InlineData("length", FinishReason.Length)]
    [InlineData("tool_calls", FinishReason.ToolCalls)]
    [InlineData("unknown_reason", FinishReason.Other)]
    [UpstreamTest(Prefix + "maps a stream ending with finish_reason %j to %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_a_stream_ending_with_finish_reason_to(string? raw, FinishReason unified)
    {
        var chunks = new List<JsonNode>
        {
            new JsonObject
            {
                ["id"] = "chat-1",
                ["object"] = "chat.completion.chunk",
                ["created"] = 1,
                ["model"] = "gpt-4o",
                ["choices"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["index"] = 0,
                        ["delta"] = new JsonObject { ["content"] = "Partial answer" },
                        ["finish_reason"] = null,
                    },
                },
            },
        };
        if (raw != "undefined")
        {
            chunks.Add(new JsonObject
            {
                ["choices"] = new JsonArray
                {
                    new JsonObject { ["index"] = 0, ["delta"] = new JsonObject(), ["finish_reason"] = raw },
                },
            });
        }

        var body = string.Concat(chunks.Select(chunk => "data: " + chunk.ToJsonString() + "\n\n"))
            + (raw == "undefined" ? string.Empty : "data: [DONE]\n\n");
        var capture = new OpenAICapture { ServerSentEvents = body };

        var parts = await OpenAIUpstream.Read(
            await OpenAIUpstream.Provider(capture).ChatModel("gpt-4o").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None));

        Assert.Contains(parts, part => part is TextDeltaStreamPart { Id: "0", Delta: "Partial answer" });
        Assert.Empty(parts.OfType<ErrorStreamPart>());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(unified, finish.FinishReason);
        Assert.Equal(raw == "undefined" ? null : raw, finish.RawFinishReason);
    }
}
