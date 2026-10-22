// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAICompletionLanguageModelTests;

/// <summary>Port of <c>openai-completion-language-model.test.ts</c> &gt; <c>doStream</c>.</summary>
public sealed class DoStreamTests
{
    private const string Prefix = "packages/openai/src/completion/openai-completion-language-model.test.ts::doStream::";

    [Theory]
    [InlineData("missing", FinishReason.Interrupted)]
    [InlineData("unknown_reason", FinishReason.Other)]
    [InlineData("invalid-json", FinishReason.Error)]
    [UpstreamTest(Prefix + "handles a stream ending with %s as %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_a_stream_ending_with_as(string ending, FinishReason unified)
    {
        var chunks = new List<string>
        {
            "data: " + new JsonObject
            {
                ["id"] = "response-1",
                ["object"] = "chat.completion.chunk",
                ["created"] = 1,
                ["model"] = "test-model",
                ["choices"] = new JsonArray
                {
                    new JsonObject { ["index"] = 0, ["text"] = "Partial answer", ["finish_reason"] = null },
                },
            }.ToJsonString() + "\n\n",
        };
        if (ending == "invalid-json")
        {
            chunks.Add("data: {invalid-json}\n\n");
        }
        else if (ending != "missing")
        {
            chunks.Add("data: " + new JsonObject
            {
                ["choices"] = new JsonArray
                {
                    new JsonObject { ["index"] = 0, ["text"] = "", ["finish_reason"] = ending },
                },
            }.ToJsonString() + "\n\n");
        }

        var capture = new OpenAICapture { ServerSentEvents = string.Concat(chunks) };

        var parts = await OpenAIUpstream.Read(
            await OpenAIUpstream.Provider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None));

        Assert.Contains(parts, part => part is TextDeltaStreamPart { Delta: "Partial answer" });
        Assert.Equal(ending == "invalid-json" ? 1 : 0, parts.OfType<ErrorStreamPart>().Count());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(unified, finish.FinishReason);
        Assert.Equal(ending == "unknown_reason" ? ending : null, finish.RawFinishReason);
    }
}
