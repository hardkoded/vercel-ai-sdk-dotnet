// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Perplexity;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.PerplexityLanguageModelTests;

/// <summary>Port of <c>perplexity-language-model.test.ts</c> &gt; <c>%s tool choice</c> &gt; <c>model %s</c> (doGenerate and doStream).</summary>
public sealed class ToolChoiceModelTests
{
    private const string AgentResponse =
        "{\"id\":\"resp-123\",\"created_at\":1784292159,\"model\":\"openai/gpt-5.1\",\"object\":\"response\",\"output\":[{\"id\":\"msg-123\",\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello from Perplexity.\",\"annotations\":[]}]}],\"status\":\"completed\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1,\"total_tokens\":2}}";

    private const string NativeTools =
        "[{\"type\":\"web_search\"},{\"type\":\"mcp\",\"server_label\":\"test\",\"server_url\":\"https://example.com/mcp\",\"authorization\":\"test-mcp-token\",\"headers\":{\"x-api-key\":\"test-mcp-key\"}},{\"type\":\"connector\",\"id\":\"test-connector\",\"server_label\":\"test\"}]";

    public static IEnumerable<object[]> Combinations()
    {
        foreach (var method in new[] { "doGenerate", "doStream" })
        {
            foreach (var modelId in new[] { "low", "openai/gpt-5.1" })
            {
                yield return new object[] { method, modelId };
            }
        }
    }

    public static IEnumerable<object[]> NoneCombinations()
    {
        foreach (var combination in Combinations())
        {
            foreach (var maxToolCalls in new int?[] { null, 5 })
            {
                yield return new object[] { combination[0], combination[1], maxToolCalls! };
            }
        }
    }

    public static IEnumerable<object[]> AutoCombinations()
    {
        foreach (var combination in Combinations())
        {
            foreach (var choice in new[] { "undefined", "auto" })
            {
                yield return new[] { combination[0], combination[1], choice };
            }
        }
    }

    [Theory]
    [MemberData(nameof(NoneCombinations))]
    [UpstreamTest("packages/perplexity/src/perplexity-language-model.test.ts::%s tool choice > model %s::disables all tools with none and max_tool_calls %s", Coverage = UpstreamCoverage.Covered, Commit = "7772d0ca0f7abc2cb2a0ad61b0272a72655d780d")]
    public async Task Disables_all_tools_with_none_and_max_tool_calls(string method, string modelId, int? maxToolCalls)
    {
        var capture = Capture(method);
        var call = Prompt();
        call.Tools = new[] { WeatherTool() };
        call.ToolChoice = ToolChoice.None;
        call.ProviderOptions = PerplexityOptions(maxToolCalls);

        var warnings = await Run(capture, method, modelId, call);

        var body = UpstreamChat.Body(capture);
        Assert.Null(body["tools"]);
        Assert.Equal(0, body["max_tool_calls"]!.GetValue<int>());
        Assert.DoesNotContain("test-mcp-token", capture.Requests[0].Body);
        Assert.DoesNotContain("test-mcp-key", capture.Requests[0].Body);
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Combinations))]
    [UpstreamTest("packages/perplexity/src/perplexity-language-model.test.ts::%s tool choice > model %s::disables preset tools even without explicit tools", Coverage = UpstreamCoverage.Covered, Commit = "7772d0ca0f7abc2cb2a0ad61b0272a72655d780d")]
    public async Task Disables_preset_tools_even_without_explicit_tools(string method, string modelId)
    {
        var capture = Capture(method);
        var call = Prompt();
        call.ToolChoice = ToolChoice.None;

        await Run(capture, method, modelId, call);

        var body = UpstreamChat.Body(capture);
        Assert.Null(body["tools"]);
        Assert.Equal(0, body["max_tool_calls"]!.GetValue<int>());
    }

    [Theory]
    [MemberData(nameof(Combinations))]
    [UpstreamTest("packages/perplexity/src/perplexity-language-model.test.ts::%s tool choice > model %s::omits disabled provider-defined tools without unsupported warnings", Coverage = UpstreamCoverage.Covered, Commit = "7772d0ca0f7abc2cb2a0ad61b0272a72655d780d")]
    public async Task Omits_disabled_provider_defined_tools_without_unsupported_warnings(string method, string modelId)
    {
        var capture = Capture(method);
        var call = Prompt();
        call.ToolChoice = ToolChoice.None;
        call.Tools = new[]
        {
            LanguageModelTool.ProviderDefined("perplexity.web_search", "search", JsonDocument.Parse("{}").RootElement.Clone()),
        };

        var warnings = await Run(capture, method, modelId, call);

        var body = UpstreamChat.Body(capture);
        Assert.Null(body["tools"]);
        Assert.Equal(0, body["max_tool_calls"]!.GetValue<int>());
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Combinations))]
    [UpstreamTest("packages/perplexity/src/perplexity-language-model.test.ts::%s tool choice > model %s::keeps prior tool calls and results while disabling new tool use", Coverage = UpstreamCoverage.Covered, Commit = "7772d0ca0f7abc2cb2a0ad61b0272a72655d780d")]
    public async Task Keeps_prior_tool_calls_and_results_while_disabling_new_tool_use(string method, string modelId)
    {
        var capture = Capture(method);
        var call = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Hello"),
                new AssistantModelMessage(null, new[] { new GeneratedToolCall("call-1", "weather", "{}") }, null),
                new ToolModelMessage("call-1", "weather", "Sunny", false, "text"),
            },
            Tools = new[] { WeatherTool() },
            ToolChoice = ToolChoice.None,
            ProviderOptions = PerplexityOptions(5),
        };

        await Run(capture, method, modelId, call);

        var body = UpstreamChat.Body(capture);
        Assert.Null(body["tools"]);
        Assert.Equal(0, body["max_tool_calls"]!.GetValue<int>());
        var expected = JsonNode.Parse("[{\"type\":\"message\",\"role\":\"user\",\"content\":\"Hello\"},{\"type\":\"function_call\",\"call_id\":\"call-1\",\"name\":\"weather\",\"arguments\":\"{}\"},{\"type\":\"function_call_output\",\"call_id\":\"call-1\",\"name\":\"weather\",\"output\":\"Sunny\"}]");
        Assert.True(JsonNode.DeepEquals(expected, body["input"]), body["input"]!.ToJsonString());
    }

    [Theory]
    [MemberData(nameof(AutoCombinations))]
    [UpstreamTest("packages/perplexity/src/perplexity-language-model.test.ts::%s tool choice > model %s::preserves tools and limits with tool choice %s", Coverage = UpstreamCoverage.Covered, Commit = "7772d0ca0f7abc2cb2a0ad61b0272a72655d780d")]
    public async Task Preserves_tools_and_limits_with_tool_choice(string method, string modelId, string choice)
    {
        var capture = Capture(method);
        var call = Prompt();
        call.Tools = new[] { WeatherTool() };
        call.ToolChoice = choice == "auto" ? ToolChoice.Auto : null;
        call.ProviderOptions = PerplexityOptions(5);

        await Run(capture, method, modelId, call);

        var body = UpstreamChat.Body(capture);
        var expected = JsonNode.Parse(NativeTools)!.AsArray();
        expected.Add(JsonNode.Parse("{\"type\":\"function\",\"name\":\"weather\",\"parameters\":{\"type\":\"object\",\"properties\":{}}}"));
        Assert.True(JsonNode.DeepEquals(expected, body["tools"]), body["tools"]!.ToJsonString());
        Assert.Equal(5, body["max_tool_calls"]!.GetValue<int>());
    }

    private static UpstreamCapture Capture(string method)
    {
        return method == "doGenerate"
            ? new UpstreamCapture { ResponseBody = AgentResponse }
            : new UpstreamCapture { ResponseBody = UpstreamChat.Sse("{\"type\":\"response.completed\"}"), MediaType = "text/event-stream" };
    }

    private static LanguageModelCallOptions Prompt() => UpstreamChat.Prompt();

    private static LanguageModelTool WeatherTool()
    {
        return new LanguageModelTool("weather", null, JsonDocument.Parse("{\"type\":\"object\",\"properties\":{}}").RootElement.Clone());
    }

    private static Dictionary<string, JsonElement> PerplexityOptions(int? maxToolCalls)
    {
        var options = new JsonObject { ["tools"] = JsonNode.Parse(NativeTools) };
        if (maxToolCalls is { } limit)
        {
            options["max_tool_calls"] = limit;
        }

        return new Dictionary<string, JsonElement>
        {
            ["perplexity"] = JsonDocument.Parse(options.ToJsonString()).RootElement.Clone(),
        };
    }

    private static async Task<IReadOnlyList<CallWarning>> Run(UpstreamCapture capture, string method, string modelId, LanguageModelCallOptions call)
    {
        var model = PerplexityProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel(modelId);
        if (method == "doGenerate")
        {
            return (await model.DoGenerateAsync(call, CancellationToken.None)).Warnings;
        }

        var parts = await UpstreamChat.Read(await model.DoStreamAsync(call, CancellationToken.None));
        return Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings;
    }
}
