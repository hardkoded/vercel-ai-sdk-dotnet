// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

namespace Vercel.AI.Tests;

public sealed class OpenAIResponsesFunctionCallMetadataTests
{
    private static JsonObject FunctionCall(string arguments, JsonNode? async, JsonNode? caller)
    {
        var item = new JsonObject
        {
            ["type"] = "function_call",
            ["id"] = "fc_1",
            ["call_id"] = "call_1",
            ["name"] = "weather",
            ["arguments"] = arguments,
        };
        if (async != null)
        {
            item["async"] = async;
        }

        if (caller != null)
        {
            item["caller"] = caller;
        }

        return item;
    }

    [Fact]
    public void Generate_maps_a_program_caller_to_caller_id()
    {
        var body = new JsonObject
        {
            ["id"] = "resp_1",
            ["output"] = new JsonArray(FunctionCall("{}", null, new JsonObject { ["type"] = "program", ["caller_id"] = "caller_1" })),
        };

        var call = OpenAIResponsesSupport.Generate(body, OpenAIResponsesSupport.TestTools).Content.OfType<GeneratedToolCall>().Single();

        var caller = call.ProviderMetadata!.Value.GetProperty("openai").GetProperty("caller");
        Assert.Equal("program", caller.GetProperty("type").GetString());
        Assert.Equal("caller_1", caller.GetProperty("callerId").GetString());
    }

    [Fact]
    public void Generate_passes_other_callers_through()
    {
        var body = new JsonObject
        {
            ["id"] = "resp_1",
            ["output"] = new JsonArray(FunctionCall("{}", null, new JsonObject { ["type"] = "direct" })),
        };

        var call = OpenAIResponsesSupport.Generate(body, OpenAIResponsesSupport.TestTools).Content.OfType<GeneratedToolCall>().Single();

        Assert.Equal("direct", call.ProviderMetadata!.Value.GetProperty("openai").GetProperty("caller").GetProperty("type").GetString());
    }

    [Fact]
    public async Task Stream_maps_a_program_caller_and_keeps_async_from_the_added_item()
    {
        var events = await OpenAIResponsesSupport.Stream(
            OpenAIResponsesSupport.Sse(new JsonNode[]
            {
                new JsonObject { ["type"] = "response.output_item.added", ["output_index"] = 0, ["item"] = FunctionCall(string.Empty, true, null) },
                new JsonObject
                {
                    ["type"] = "response.output_item.done",
                    ["output_index"] = 0,
                    ["item"] = FunctionCall("{}", null, new JsonObject { ["type"] = "program", ["caller_id"] = "caller_1" }),
                },
            }),
            OpenAIResponsesSupport.TestTools);

        var openai = events.OfType<ToolCallStreamPart>().Single().ProviderMetadata!.Value.GetProperty("openai");
        Assert.True(openai.GetProperty("async").GetBoolean());
        Assert.Equal("program", openai.GetProperty("caller").GetProperty("type").GetString());
        Assert.Equal("caller_1", openai.GetProperty("caller").GetProperty("callerId").GetString());
    }
}
