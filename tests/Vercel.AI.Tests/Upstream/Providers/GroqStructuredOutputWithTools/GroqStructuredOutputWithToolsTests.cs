// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Groq;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.GroqStructuredOutputWithTools;

/// <summary>Port of <c>structured-output-with-tools.test.ts</c> (Groq) &gt; <c>structured output with tools</c>.</summary>
public sealed class GroqStructuredOutputWithToolsTests
{
    private const string Url = "https://api.groq.com/openai/v1/chat/completions";
    private const string Schema = "{\"type\":\"object\",\"properties\":{\"date\":{\"type\":\"string\"}},\"required\":[\"date\"],\"additionalProperties\":false}";
    private const string EmptyObject = "{\"type\":\"object\",\"properties\":{}}";

    private static LanguageModelCallOptions Options()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Find the date.") },
            JsonSchema = UpstreamChat.Json(Schema),
            Tools = new[] { new LanguageModelTool("resolveDate", null, UpstreamChat.Json(EmptyObject)) },
        };
    }

    private static string Response(string name = "json", string finishReason = "tool_calls")
    {
        return new JsonObject
        {
            ["choices"] = new JsonArray
            {
                new JsonObject
                {
                    ["index"] = 0,
                    ["finish_reason"] = finishReason,
                    ["message"] = new JsonObject
                    {
                        ["content"] = "Here is the answer.",
                        ["tool_calls"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["id"] = "call-1",
                                ["type"] = "function",
                                ["function"] = new JsonObject { ["name"] = name, ["arguments"] = "{\"date\":\"2031-06-17\"}" },
                            },
                        },
                    },
                },
            },
            ["usage"] = new JsonObject { ["prompt_tokens"] = 4, ["completion_tokens"] = 8 },
        }.ToJsonString();
    }

    private static ILanguageModel Model(UpstreamCapture capture, string modelId)
    {
        return GroqProvider.Create(new OpenAICompatibleOptions { ApiKey = "test" }, capture).LanguageModel(modelId);
    }

    private static string[] ToolNames(JsonObject body)
    {
        return body["tools"]!.AsArray().Select(tool => tool!["function"]!["name"]!.GetValue<string>()).ToArray();
    }

    [Fact]
    [UpstreamTest("packages/groq/src/structured-output-with-tools.test.ts::structured output with tools::allows only the response tool when application tools are disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_only_the_response_tool_when_application_tools_are_disabled()
    {
        var capture = new UpstreamCapture { ResponseBody = Response() };
        var options = Options();
        options.ToolChoice = ToolChoice.None;

        await Model(capture, "openai/gpt-oss-120b").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(Url, capture.Requests[0].Uri!.AbsoluteUri);
        JsonAssert.Equal(UpstreamChat.Body(capture)["tool_choice"], "{\"type\":\"function\",\"function\":{\"name\":\"json\"}}");
    }

    [Theory]
    [InlineData("openai/gpt-oss-120b")]
    [InlineData("llama-3.3-70b-versatile")]
    [UpstreamTest("packages/groq/src/structured-output-with-tools.test.ts::structured output with tools::uses a response tool for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_response_tool_for(string modelId)
    {
        var capture = new UpstreamCapture { ResponseBody = Response() };

        var result = await Model(capture, modelId).DoGenerateAsync(Options(), CancellationToken.None);

        var body = UpstreamChat.Body(capture);
        Assert.False(body.ContainsKey("response_format"));
        Assert.Equal(new[] { "resolveDate", "json" }, ToolNames(body));
        Assert.Equal("required", body["tool_choice"]!.GetValue<string>());
        Assert.False(body["parallel_tool_calls"]!.GetValue<bool>());
        var text = Assert.IsAssignableFrom<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("{\"date\":\"2031-06-17\"}", text.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("tool_calls", result.RawFinishReason);
        Assert.Equal(8, result.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/structured-output-with-tools.test.ts::structured output with tools::preserves named tool choice and ordinary tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_named_tool_choice_and_ordinary_tool_calls()
    {
        var capture = new UpstreamCapture { ResponseBody = Response("resolveDate") };
        var options = Options();
        options.ToolChoice = ToolChoice.Tool("resolveDate");

        var result = await Model(capture, "openai/gpt-oss-120b").DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(UpstreamChat.Body(capture)["tool_choice"], "{\"type\":\"function\",\"function\":{\"name\":\"resolveDate\"}}");
        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("resolveDate", call.ToolName);
        Assert.Equal("{\"date\":\"2031-06-17\"}", call.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/structured-output-with-tools.test.ts::structured output with tools::avoids a caller tool named json", Coverage = UpstreamCoverage.Covered)]
    public async Task Avoids_a_caller_tool_named_json()
    {
        var capture = new UpstreamCapture { ResponseBody = Response("json_1") };
        var options = Options();
        options.Tools = new[] { new LanguageModelTool("json", null, UpstreamChat.Json(EmptyObject)) };

        var result = await Model(capture, "openai/gpt-oss-120b").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(new[] { "json", "json_1" }, ToolNames(UpstreamChat.Body(capture)));
        Assert.Equal("text", result.Content[0].Type);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/structured-output-with-tools.test.ts::structured output with tools::keeps native output formatting when there are no function tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_native_output_formatting_when_there_are_no_function_tools()
    {
        var capture = new UpstreamCapture { ResponseBody = Response() };
        var options = Options();
        options.Tools = Array.Empty<LanguageModelTool>();

        await Model(capture, "openai/gpt-oss-120b").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("json_schema", UpstreamChat.Body(capture)["response_format"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/groq/src/structured-output-with-tools.test.ts::structured output with tools::preserves an incomplete finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_an_incomplete_finish_reason()
    {
        var capture = new UpstreamCapture { ResponseBody = Response("json", "length") };

        var result = await Model(capture, "openai/gpt-oss-120b").DoGenerateAsync(Options(), CancellationToken.None);

        Assert.Equal(FinishReason.Length, result.FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/groq/src/structured-output-with-tools.test.ts::structured output with tools::streams response-tool arguments as text", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_response_tool_arguments_as_text()
    {
        var capture = new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse(
                "{\"choices\":[{\"index\":0,\"delta\":{\"tool_calls\":[{\"index\":0,\"id\":\"call-1\",\"type\":\"function\",\"function\":{\"name\":\"json\",\"arguments\":\"{\\\"date\\\":\"}}]}}]}",
                "{\"choices\":[{\"index\":0,\"delta\":{\"tool_calls\":[{\"index\":0,\"function\":{\"arguments\":\"\\\"2031-06-17\\\"}\"}}]},\"finish_reason\":\"tool_calls\"}],\"x_groq\":{\"usage\":{\"prompt_tokens\":4,\"completion_tokens\":8}}}"),
        };

        var parts = await UpstreamChat.Read(Model(capture, "openai/gpt-oss-120b").DoStreamAsync(Options(), CancellationToken.None));

        Assert.DoesNotContain(parts, part => part.Type.StartsWith("tool-", StringComparison.Ordinal));
        Assert.Equal("{\"date\":\"2031-06-17\"}", string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta)));
        Assert.Equal("call-1", Assert.Single(parts.OfType<TextStartStreamPart>()).Id);
        Assert.Equal("call-1", Assert.Single(parts.OfType<TextEndStreamPart>()).Id);
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("tool_calls", finish.RawFinishReason);
        Assert.Equal(8, finish.Usage.OutputTokens);
    }
}
