// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream::preserves repeated citation ranges and versioned fragment URLs with Unicode text in generation and streaming",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_repeated_citation_ranges_and_versioned_fragment_URLs_with_Unicode_text_in_generation_and_streaming()
    {
        const string text = "🧪 café — 東京 café";
        const string url = "https://example.com/article?oldid=42#:~:text=caf%C3%A9";
        JsonArray Annotations() => new()
        {
            new JsonObject { ["type"] = "url_citation", ["url"] = url, ["title"] = "Café", ["start_index"] = 2, ["end_index"] = 6 },
            new JsonObject { ["type"] = "url_citation", ["url"] = url, ["title"] = "Café", ["start_index"] = 12, ["end_index"] = 16 },
        };

        var generated = OpenAIResponsesSupport.Generate(new JsonObject
        {
            ["id"] = "response",
            ["model"] = "gpt-5-nano",
            ["output"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "message",
                    ["type"] = "message",
                    ["role"] = "assistant",
                    ["content"] = new JsonArray
                    {
                        new JsonObject { ["type"] = "output_text", ["text"] = text, ["annotations"] = Annotations() },
                    },
                },
            },
            ["usage"] = new JsonObject { ["input_tokens"] = 1, ["output_tokens"] = 1 },
        });
        var generatedText = Assert.IsType<OpenAIText>(Assert.Single(generated.Content.OfType<GeneratedText>()));
        var expectedCitations = $$"""
            [
              { "source": { "type": "source", "sourceType": "url", "id": "{{url}}", "url": "{{url}}", "title": "Café" }, "startIndex": 2, "endIndex": 6 },
              { "source": { "type": "source", "sourceType": "url", "id": "{{url}}", "url": "{{url}}", "title": "Café" }, "startIndex": 12, "endIndex": 16 }
            ]
            """;
        Assert.Equal(text, generatedText.Text);
        JsonAssert.Equal(OpenAIResponsesSupport.CitationsJson(generatedText.Citations), expectedCitations);

        var events = new List<JsonNode>
        {
            new JsonObject
            {
                ["type"] = "response.output_item.added",
                ["output_index"] = 0,
                ["item"] = new JsonObject { ["type"] = "message", ["id"] = "message", ["role"] = "assistant" },
            },
            new JsonObject { ["type"] = "response.output_text.delta", ["item_id"] = "message", ["delta"] = text },
        };
        foreach (var annotation in Annotations())
        {
            events.Add(new JsonObject { ["type"] = "response.output_text.annotation.added", ["annotation"] = annotation!.DeepClone() });
        }

        events.Add(new JsonObject
        {
            ["type"] = "response.output_item.done",
            ["output_index"] = 0,
            ["item"] = new JsonObject { ["type"] = "message", ["id"] = "message" },
        });

        var parts = await OpenAIResponsesSupport.Stream(OpenAIResponsesSupport.Sse(events));

        Assert.Empty(parts.OfType<ErrorStreamPart>());
        Assert.Equal(text, parts.OfType<TextDeltaStreamPart>().First().Delta);
        var end = parts.OfType<TextEndStreamPart>().First();
        JsonAssert.Equal(OpenAIResponsesSupport.CitationsJson(end.Citations), expectedCitations);
        var annotations = end.ProviderMetadata!.Value.GetProperty("openai").GetProperty("annotations");
        JsonAssert.Equal(annotations, Annotations().ToJsonString());
    }

    private static string FunctionCallStream(string model, string itemId, string callId, string? @namespace)
    {
        var response = new JsonObject { ["id"] = "resp_ns", ["created_at"] = 1741362087, ["model"] = model };
        var done = new JsonObject
        {
            ["type"] = "function_call",
            ["id"] = itemId,
            ["call_id"] = callId,
            ["name"] = "get_weather",
            ["arguments"] = "{\"location\":\"NYC\"}",
            ["status"] = "completed",
        };
        if (@namespace != null)
        {
            done["namespace"] = @namespace;
        }

        return OpenAIResponsesSupport.Sse(new JsonNode[]
        {
            new JsonObject { ["type"] = "response.created", ["response"] = response.DeepClone() },
            new JsonObject
            {
                ["type"] = "response.output_item.added",
                ["output_index"] = 0,
                ["item"] = new JsonObject
                {
                    ["type"] = "function_call",
                    ["id"] = itemId,
                    ["call_id"] = callId,
                    ["name"] = "get_weather",
                    ["arguments"] = "",
                    ["status"] = "in_progress",
                },
            },
            new JsonObject
            {
                ["type"] = "response.function_call_arguments.delta",
                ["item_id"] = itemId,
                ["output_index"] = 0,
                ["delta"] = "{\"location\":\"NYC\"}",
            },
            new JsonObject
            {
                ["type"] = "response.function_call_arguments.done",
                ["item_id"] = itemId,
                ["output_index"] = 0,
                ["arguments"] = "{\"location\":\"NYC\"}",
            },
            new JsonObject { ["type"] = "response.output_item.done", ["output_index"] = 0, ["item"] = done },
            new JsonObject
            {
                ["type"] = "response.completed",
                ["response"] = new JsonObject
                {
                    ["id"] = "resp_ns",
                    ["status"] = "completed",
                    ["usage"] = new JsonObject { ["input_tokens"] = 0, ["output_tokens"] = 0 },
                },
            },
        });
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream::should preserve namespace on streaming function_call output",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_namespace_on_streaming_function_call_output()
    {
        var events = await OpenAIResponsesSupport.Stream(
            FunctionCallStream("gpt-5.4", "fc_ns_1", "call_ns_1", "weather_ns"),
            OpenAIResponsesSupport.TestTools);

        var toolCall = Assert.Single(events.OfType<ToolCallStreamPart>());
        var openai = toolCall.ProviderMetadata!.Value.GetProperty("openai");
        Assert.Equal("fc_ns_1", openai.GetProperty("itemId").GetString());
        Assert.Equal("weather_ns", openai.GetProperty("namespace").GetString());

        var toolInputEnd = Assert.Single(events.OfType<ToolInputEndStreamPart>());
        Assert.Equal("weather_ns", toolInputEnd.ProviderMetadata!.Value.GetProperty("openai").GetProperty("namespace").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream::should not set namespace on streaming function_call when absent",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_set_namespace_on_streaming_function_call_when_absent()
    {
        var events = await OpenAIResponsesSupport.Stream(
            FunctionCallStream("gpt-4o", "fc_plain", "call_plain", null),
            OpenAIResponsesSupport.TestTools);

        var toolCall = Assert.Single(events.OfType<ToolCallStreamPart>());
        Assert.False(toolCall.ProviderMetadata!.Value.GetProperty("openai").TryGetProperty("namespace", out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream::should preserve async mode on streamed function calls",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_async_mode_on_streamed_function_calls()
    {
        JsonObject FunctionCall(string arguments, string status) => new()
        {
            ["id"] = "fc_async",
            ["type"] = "function_call",
            ["name"] = "weather",
            ["call_id"] = "call_async",
            ["arguments"] = arguments,
            ["status"] = status,
            ["async"] = true,
        };

        var events = await OpenAIResponsesSupport.Stream(
            OpenAIResponsesSupport.Sse(new JsonNode[]
            {
                new JsonObject
                {
                    ["type"] = "response.created",
                    ["response"] = new JsonObject { ["id"] = "response_async", ["created_at"] = 1, ["model"] = "gpt-6-astra" },
                },
                new JsonObject { ["type"] = "response.output_item.added", ["output_index"] = 0, ["item"] = FunctionCall("", "in_progress") },
                new JsonObject
                {
                    ["type"] = "response.output_item.done",
                    ["output_index"] = 0,
                    ["item"] = FunctionCall("""{"location":"Berlin"}""", "completed"),
                },
                new JsonObject
                {
                    ["type"] = "response.completed",
                    ["response"] = new JsonObject
                    {
                        ["incomplete_details"] = null,
                        ["output"] = new JsonArray(FunctionCall("""{"location":"Berlin"}""", "completed")),
                        ["usage"] = new JsonObject { ["input_tokens"] = 1, ["output_tokens"] = 2 },
                    },
                },
            }),
            OpenAIResponsesSupport.TestTools);

        var toolCall = events.OfType<ToolCallStreamPart>().First();
        Assert.Equal("call_async", toolCall.ToolCallId);
        Assert.Equal("weather", toolCall.ToolName);
        Assert.Equal("""{"location":"Berlin"}""", toolCall.ArgumentsJson);
        JsonAssert.Equal(toolCall.ProviderMetadata!.Value, """{"openai":{"itemId":"fc_async","async":true}}""");
    }
}
