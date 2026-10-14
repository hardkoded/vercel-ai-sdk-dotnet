// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.GoogleStructuredOutputWithTools;

/// <summary>Port of <c>structured-output-with-tools.test.ts</c> (Google) &gt; <c>structured output with tools</c>.</summary>
public sealed class GoogleStructuredOutputWithToolsTests
{
    private const string Flash25 = "gemini-2.5-flash-lite";
    private const string Gemini3 = "gemini-3.8-flash";
    private const string Date = "{\"date\":\"2031-06-17\"}";
    private const string Signature = "{\"google\":{\"thoughtSignature\":\"signature\"}}";
    private const string Schema = "{\"type\":\"object\",\"properties\":{\"date\":{\"type\":\"string\"}},\"required\":[\"date\"],\"additionalProperties\":false}";
    private const string EmptyObject = "{\"type\":\"object\",\"properties\":{}}";

    private static LanguageModelCallOptions Options()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Find the date.") },
            JsonSchema = GoogleUpstream.Element(Schema),
            Tools = new[] { GoogleUpstream.Function("resolveDate", EmptyObject, null) },
        };
    }

    private static JsonObject FunctionCall(string id, string name, JsonNode? args, string signature)
    {
        var call = new JsonObject { ["id"] = id, ["name"] = name };
        if (args != null)
        {
            call["args"] = args;
        }

        return new JsonObject { ["functionCall"] = call, ["thoughtSignature"] = signature };
    }

    private static JsonObject Response(string name = "json", params JsonObject[] extraParts)
    {
        var parts = new JsonArray
        {
            new JsonObject { ["text"] = "Here is the answer." },
            FunctionCall("call-1", name, new JsonObject { ["date"] = "2031-06-17" }, "signature"),
        };
        foreach (var part in extraParts)
        {
            parts.Add(part);
        }

        return new JsonObject
        {
            ["candidates"] = new JsonArray
            {
                new JsonObject
                {
                    ["content"] = new JsonObject { ["role"] = "model", ["parts"] = parts },
                    ["finishReason"] = "STOP",
                },
            },
            ["usageMetadata"] = new JsonObject { ["promptTokenCount"] = 4, ["candidatesTokenCount"] = 8 },
        };
    }

    private static JsonObject ApplicationCall()
    {
        return FunctionCall("application", "resolveDate", new JsonObject { ["date"] = "2031-06-17" }, "application-signature");
    }

    private static async Task<(LanguageModelGenerateResult Result, JsonObject Body)> Generate(string modelId, JsonObject response, LanguageModelCallOptions options)
    {
        var handler = new RecordingHandler { ResponseText = response.ToJsonString() };
        var provider = GoogleUpstream.Client(handler);
        var result = await provider.LanguageModel(modelId).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("https://generativelanguage.googleapis.com/v1beta/models/" + modelId + ":generateContent", handler.Uris[0]);
        return (result, JsonNode.Parse(handler.Body)!.AsObject());
    }

    private static async Task<List<LanguageModelStreamPart>> Stream(string modelId, JsonObject chunk, LanguageModelCallOptions options)
    {
        var handler = new RecordingHandler { ResponseText = "data: " + chunk.ToJsonString() + "\n\n", MediaType = "text/event-stream" };
        var provider = GoogleUpstream.Client(handler);
        return await GoogleUpstream.Collect((await provider.LanguageModel(modelId).DoStreamAsync(options, CancellationToken.None)));
    }

    private static string[] DeclarationNames(JsonObject body)
    {
        return body["tools"]![0]!["functionDeclarations"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>()).ToArray();
    }

    private static string Joined(IEnumerable<LanguageModelStreamPart> parts)
    {
        return string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta));
    }

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest("packages/google/src/structured-output-with-tools.test.ts::structured output with tools::preserves an application call alongside the response tool in %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_an_application_call_alongside_the_response_tool_in(string mode)
    {
        var body = Response("json", ApplicationCall());

        if (mode == "generate")
        {
            var (result, _) = await Generate(Flash25, body, Options());

            var text = Assert.IsType<GoogleText>(Assert.Single(result.Content, part => part is GeneratedText));
            Assert.Equal(Date, text.Text);
            GoogleUpstream.JsonEqual(text.ProviderMetadata!.Value, Signature);
            var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content, part => part is GeneratedToolCall));
            Assert.Equal("application", call.ToolCallId);
            Assert.Equal("resolveDate", call.ToolName);
            Assert.Equal(Date, call.ArgumentsJson);
            GoogleUpstream.JsonEqual(call.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"application-signature\"}}");
            Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        }
        else
        {
            var parts = await Stream(Flash25, body, Options());

            var call = Assert.IsType<ToolCallStreamPart>(parts.First(part => part.Type == "tool-call"));
            Assert.Equal("resolveDate", call.ToolName);
            Assert.Equal("application", call.ToolCallId);
            Assert.Equal(Date, Joined(parts));
            Assert.Equal(FinishReason.ToolCalls, Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).FinishReason);
        }
    }

    [Fact]
    [UpstreamTest("packages/google/src/structured-output-with-tools.test.ts::structured output with tools::avoids a caller tool named json and preserves its calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Avoids_a_caller_tool_named_json_and_preserves_its_calls()
    {
        var options = Options();
        options.Tools = new[] { GoogleUpstream.Function("json", EmptyObject, null) };

        var (result, body) = await Generate(Flash25, Response("json"), options);

        Assert.Equal(new[] { "json", "json_1" }, DeclarationNames(body));
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("json", call.ToolName);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/google/src/structured-output-with-tools.test.ts::structured output with tools::allows only the response tool when application tools are disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_only_the_response_tool_when_application_tools_are_disabled()
    {
        var options = Options();
        options.ToolChoice = ToolChoice.None;

        var (_, body) = await Generate(Flash25, Response(), options);

        GoogleUpstream.JsonEqual(body["toolConfig"]!["functionCallingConfig"]!["allowedFunctionNames"], "[\"json\"]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/structured-output-with-tools.test.ts::structured output with tools::streams an empty response-tool argument object", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_an_empty_response_tool_argument_object()
    {
        var chunk = new JsonObject
        {
            ["candidates"] = new JsonArray
            {
                new JsonObject
                {
                    ["content"] = new JsonObject
                    {
                        ["parts"] = new JsonArray { new JsonObject { ["functionCall"] = new JsonObject { ["id"] = "call-1", ["name"] = "json" } } },
                    },
                    ["finishReason"] = "STOP",
                },
            },
        };

        var parts = await Stream(Flash25, chunk, Options());

        Assert.Equal("{}", Joined(parts));
        Assert.Contains(parts, part => part is TextEndStreamPart { Id: "call-1" });
        Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]).FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/google/src/structured-output-with-tools.test.ts::structured output with tools::uses a response tool on Gemini 2.5", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_response_tool_on_Gemini_2_5()
    {
        var (result, body) = await Generate(Flash25, Response(), Options());

        var config = body["generationConfig"]!.AsObject();
        Assert.False(config.ContainsKey("responseMimeType"));
        Assert.False(config.ContainsKey("responseJsonSchema"));
        Assert.Equal(new[] { "resolveDate", "json" }, DeclarationNames(body));
        Assert.Equal("ANY", body["toolConfig"]!["functionCallingConfig"]!["mode"]!.GetValue<string>());
        var text = Assert.IsType<GoogleText>(Assert.Single(result.Content));
        Assert.Equal(Date, text.Text);
        GoogleUpstream.JsonEqual(text.ProviderMetadata!.Value, Signature);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("STOP", result.RawFinishReason);
    }

    [Theory]
    [InlineData("gemini-3.8-flash")]
    [InlineData("gemini-99-pro-preview")]
    [UpstreamTest("packages/google/src/structured-output-with-tools.test.ts::structured output with tools::keeps native formatting on %s with automatic tool choice", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_native_formatting_on_with_automatic_tool_choice(string modelId)
    {
        var options = Options();
        options.ToolChoice = ToolChoice.Auto;

        var (_, body) = await Generate(modelId, Response(), options);

        Assert.Equal("application/json", body["generationConfig"]!["responseMimeType"]!.GetValue<string>());
        Assert.Equal(new[] { "resolveDate" }, DeclarationNames(body));
    }

    [Fact]
    [UpstreamTest("packages/google/src/structured-output-with-tools.test.ts::structured output with tools::uses a response tool for forced function calling on Gemini 3", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_response_tool_for_forced_function_calling_on_Gemini_3()
    {
        var options = Options();
        options.ToolChoice = ToolChoice.Tool("resolveDate");

        var (result, body) = await Generate(Gemini3, Response("resolveDate"), options);

        Assert.False(body["generationConfig"]!.AsObject().ContainsKey("responseMimeType"));
        GoogleUpstream.JsonEqual(body["toolConfig"]!["functionCallingConfig"]!["allowedFunctionNames"], "[\"resolveDate\"]");
        Assert.Equal("resolveDate", Assert.IsType<GeneratedToolCall>(result.Content[0]).ToolName);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }

    [Fact]
    [UpstreamTest("packages/google/src/structured-output-with-tools.test.ts::structured output with tools::keeps native formatting without tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_native_formatting_without_tools()
    {
        var options = Options();
        options.Tools = Array.Empty<LanguageModelTool>();

        var (_, body) = await Generate(Flash25, Response(), options);

        Assert.Equal("application/json", body["generationConfig"]!["responseMimeType"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/google/src/structured-output-with-tools.test.ts::structured output with tools::streams response-tool arguments as text and retains the thought signature", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_response_tool_arguments_as_text_and_retains_the_thought_signature()
    {
        var parts = await Stream(Flash25, Response(), Options());

        Assert.DoesNotContain(parts, part => part.Type.StartsWith("tool-", StringComparison.Ordinal));
        Assert.Equal(Date, Joined(parts));
        var start = Assert.Single(parts.OfType<TextStartStreamPart>(), part => part.Id == "call-1");
        GoogleUpstream.JsonEqual(start.ProviderMetadata!.Value, Signature);
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("STOP", finish.RawFinishReason);
        Assert.Equal(8, finish.Usage.OutputTokens);
    }
}
