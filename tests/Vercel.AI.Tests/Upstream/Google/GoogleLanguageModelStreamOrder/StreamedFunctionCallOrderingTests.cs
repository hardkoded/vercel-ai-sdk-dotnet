// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelStreamOrder;

/// <summary>Port of <c>google-language-model-stream-order.test.ts</c>.</summary>
public sealed class StreamedFunctionCallOrderingTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (var provider in new[] { "google.generative-ai", "google.vertex.chat" })
        {
            foreach (var thought in new[] { false, true })
            {
                foreach (var noArgs in new[] { false, true })
                {
                    foreach (var separateChunks in new[] { false, true })
                    {
                        yield return new object[] { provider, thought, noArgs, separateChunks };
                    }
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(
        "packages/google/src/google-language-model-stream-order.test.ts::%s streamed function call ordering::preserves ordered blocks: %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public async Task Preserves_ordered_blocks(
        string provider,
        bool thought,
        bool noArgs,
        bool separateChunks
    )
    {
        var parts = new JsonArray
        {
            new JsonObject { ["text"] = "A", ["thought"] = thought },
            new JsonObject
            {
                ["functionCall"] = noArgs
                    ? new JsonObject { ["id"] = "call-1", ["name"] = "lookup" }
                    : new JsonObject
                    {
                        ["id"] = "call-1",
                        ["name"] = "lookup",
                        ["args"] = new JsonObject { ["query"] = "example" },
                    },
                ["thoughtSignature"] = "signature",
            },
            new JsonObject { ["text"] = "B", ["thought"] = thought },
        };
        var groups = separateChunks
            ? parts.Select(part => new JsonArray(part!.DeepClone())).ToList()
            : [(JsonArray)parts.DeepClone()];
        var body = string.Concat(
            groups.Select(
                (group, index) =>
                {
                    var candidate = new JsonObject
                    {
                        ["content"] = new JsonObject { ["role"] = "model", ["parts"] = group },
                    };
                    if (index == groups.Count - 1)
                    {
                        candidate["finishReason"] = "STOP";
                    }

                    return "data: "
                        + new JsonObject
                        {
                            ["candidates"] = new JsonArray(candidate),
                        }.ToJsonString()
                        + "\n\n";
                }
            )
        );
        var handler = new RecordingHandler { ResponseText = body, MediaType = "text/event-stream" };
        var isVertex = provider == "google.vertex.chat";
        var model = isVertex
            ? GoogleVertexProvider
                .Create(
                    new VertexOptions
                    {
                        Project = "test-project",
                        Region = "us-central1",
                        ApiKey = "test-api-key",
                    },
                    handler
                )
                .LanguageModel("gemini-pro")
            : GoogleProvider
                .Create(new GoogleOptions { ApiKey = "test-api-key" }, handler)
                .LanguageModel("gemini-pro");

        var events = await GoogleUpstream.Collect(
            await model.DoStreamAsync(GoogleUpstream.Hello(), CancellationToken.None)
        );

        Assert.Empty(events.OfType<ErrorStreamPart>().Select(e => e.Message));
        var block = thought ? "reasoning" : "text";
        var types = events.Select(Kind).ToList();
        var expected = new List<string>
        {
            "stream-start",
            block + "-start",
            block + "-delta",
            block + "-end",
            "tool-input-start",
        };
        if (!noArgs)
        {
            expected.Add("tool-input-delta");
        }

        expected.AddRange([
            "tool-input-end",
            "tool-call",
            block + "-start",
            block + "-delta",
            block + "-end",
            "finish",
        ]);
        Assert.Equal(expected, types);

        Assert.Equal(
            ["0", "1"],
            events
                .Select(part =>
                    part switch
                    {
                        TextStartStreamPart start => start.Id,
                        ReasoningStartStreamPart start => start.Id,
                        _ => null,
                    }
                )
                .Where(id => id != null)
        );

        var call = Assert.Single(events.OfType<ToolCallStreamPart>());
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("lookup", call.ToolName);
        Assert.Equal(noArgs ? "{}" : "{\"query\":\"example\"}", call.ArgumentsJson);
        var key = isVertex ? "vertex" : "google";
        Assert.Equal(
            "signature",
            call.ProviderMetadata!.Value.GetProperty(key)
                .GetProperty("thoughtSignature")
                .GetString()
        );
    }

    private static string Kind(LanguageModelStreamPart part) =>
        part switch
        {
            StreamStartStreamPart => "stream-start",
            TextStartStreamPart => "text-start",
            TextDeltaStreamPart => "text-delta",
            TextEndStreamPart => "text-end",
            ReasoningStartStreamPart => "reasoning-start",
            ReasoningDeltaStreamPart => "reasoning-delta",
            ReasoningEndStreamPart => "reasoning-end",
            ToolInputStartStreamPart => "tool-input-start",
            ToolInputDeltaStreamPart => "tool-input-delta",
            ToolInputEndStreamPart => "tool-input-end",
            ToolCallStreamPart => "tool-call",
            FinishStreamPart => "finish",
            _ => part.GetType().Name,
        };
}
