// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Groq;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class JsonResponseToolStreamTests
{
    private const string Schema = "{\"type\":\"object\",\"properties\":{\"date\":{\"type\":\"string\"}},\"required\":[\"date\"],\"additionalProperties\":false}";
    private const string EmptyObject = "{\"type\":\"object\",\"properties\":{}}";

    [Fact]
    public async Task Groq_streams_response_tool_text_as_each_chunk_arrives()
    {
        var capture = new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse(
                GroqChunk(new JsonObject { ["index"] = 0, ["id"] = "call-1", ["type"] = "function", ["function"] = new JsonObject { ["name"] = "json", ["arguments"] = "{\"date\"" } }),
                GroqChunk(new JsonObject { ["index"] = 0, ["function"] = new JsonObject { ["arguments"] = ":\"2031-" } }),
                GroqChunk(new JsonObject { ["index"] = 0, ["function"] = new JsonObject { ["arguments"] = "06-17\"}" } }),
                "{\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"tool_calls\"}]}"),
        };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Find the date.") },
            JsonSchema = UpstreamChat.Json(Schema),
            Tools = new[] { new LanguageModelTool("resolveDate", null, UpstreamChat.Json(EmptyObject)) },
            IncludeRawChunks = true,
        };

        var parts = await UpstreamChat.Read((await GroqProvider.Create(new OpenAICompatibleOptions { ApiKey = "test" }, capture)
            .LanguageModel("openai/gpt-oss-120b")
            .DoStreamAsync(options, CancellationToken.None)));

        var deltas = parts.OfType<TextDeltaStreamPart>().ToList();
        Assert.Equal(new[] { "{\"date\"", ":\"2031-", "06-17\"}" }, deltas.Select(part => part.Delta));
        Assert.All(deltas, part => Assert.Equal("call-1", part.Id));

        // Each text delta follows the raw chunk that carried it, before the next chunk is read.
        var raw = parts.Select((part, index) => (part, index)).Where(item => item.part is RawStreamPart).Select(item => item.index).ToList();
        var text = deltas.Select(part => parts.IndexOf(part)).ToList();
        Assert.Equal(4, raw.Count);
        for (var i = 0; i < text.Count; i++)
        {
            Assert.InRange(text[i], raw[i], raw[i + 1]);
        }

        Assert.IsType<TextEndStreamPart>(parts[parts.Count - 2]);
        Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).FinishReason);
    }

    [Fact]
    public async Task Google_keeps_reasoning_open_until_the_response_tool_call_is_converted()
    {
        var reasoning = new JsonObject { ["text"] = "Thinking.", ["thought"] = true };
        var call = new JsonObject
        {
            ["functionCall"] = new JsonObject { ["id"] = "call-1", ["name"] = "json", ["args"] = new JsonObject { ["date"] = "2031-06-17" } },
        };
        var handler = new RecordingHandler
        {
            MediaType = "text/event-stream",
            ResponseText = "data: " + GoogleChunk(reasoning, null) + "\n\n" + "data: " + GoogleChunk(call, "STOP") + "\n\n",
        };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Find the date.") },
            JsonSchema = GoogleUpstream.Element(Schema),
            Tools = new[] { GoogleUpstream.Function("resolveDate", EmptyObject, null) },
        };

        var parts = await GoogleUpstream.Collect((await GoogleUpstream.Client(handler).LanguageModel("gemini-2.5-flash-lite").DoStreamAsync(options, CancellationToken.None)));

        Assert.Equal(
            new[] { "stream-start", "reasoning-start", "reasoning-delta", "text-start", "text-delta", "text-end", "reasoning-end", "finish" },
            parts.Select(part => part.Type));
        Assert.Equal("Thinking.", Assert.IsType<ReasoningDeltaStreamPart>(parts[2]).Delta);
        Assert.Equal("call-1", Assert.IsType<TextStartStreamPart>(parts[3]).Id);
        Assert.Equal("{\"date\":\"2031-06-17\"}", Assert.IsType<TextDeltaStreamPart>(parts[4]).Delta);
        Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(parts[7]).FinishReason);
    }

    private static string GroqChunk(JsonObject toolCall)
    {
        return new JsonObject
        {
            ["choices"] = new JsonArray
            {
                new JsonObject { ["index"] = 0, ["delta"] = new JsonObject { ["tool_calls"] = new JsonArray { toolCall } } },
            },
        }.ToJsonString();
    }

    private static string GoogleChunk(JsonObject part, string? finishReason)
    {
        var candidate = new JsonObject { ["content"] = new JsonObject { ["role"] = "model", ["parts"] = new JsonArray { part } } };
        if (finishReason != null)
        {
            candidate["finishReason"] = finishReason;
        }

        return new JsonObject { ["candidates"] = new JsonArray { candidate } }.ToJsonString();
    }
}
