// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.DecisionFixtures;

namespace Vercel.AI.Tests.AnthropicDecision;

/// <summary>Port of <c>anthropic-decision.test.ts</c>.</summary>
public sealed class AnthropicDecisionTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-decision.test.ts::";

    private static JsonObject Fixture()
    {
        return (JsonObject)JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "anthropic-decision.json")))!;
    }

    private static Dictionary<string, DecisionQuestion> Questions()
    {
        return new Dictionary<string, DecisionQuestion>
        {
            ["department"] = Q("choice", "Pick the team.", new Dictionary<string, object?> { ["technical"] = "Bugs", ["billing"] = "Charges" }),
            ["severity"] = Q("score", "[\"Rate severity.\"]", new List<object?> { "Low", "Medium", "High" }),
        };
    }

    private static DecisionModelCall Call(IReadOnlyDictionary<string, DecisionQuestion>? questions = null, string providerOptions = "{}", IReadOnlyDictionary<string, string>? headers = null)
    {
        using var document = JsonDocument.Parse(providerOptions);
        var state = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["type"] = "json",
                ["value"] = new Dictionary<string, object?> { ["message"] = "A billing issue with a workaround." },
            },
        };
        return new DecisionModelCall(P(state), questions ?? Questions(), document.RootElement.Clone(), headers ?? new Dictionary<string, string>(), CancellationToken.None);
    }

    private static (DecisionLanguageModel Model, CaptureHandler Handler) Setup(JsonNode? body = null)
    {
        var handler = new CaptureHandler { ResponseBody = (body ?? Fixture()).ToJsonString() };
        handler.ResponseHeaders["request-id"] = "req-test";
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-key", BaseUrl = "https://example.com/v1" }, handler);
        provider.Options.Headers["x-provider"] = "configured";
        return (provider.DecisionModel("claude-haiku-4-5-20251001"), handler);
    }

    private static JsonObject ToolUseFixture(string input)
    {
        var body = Fixture();
        body["stop_reason"] = "tool_use";
        body["content"] = new JsonArray(new JsonObject
        {
            ["type"] = "tool_use",
            ["id"] = "tool-test",
            ["name"] = "json",
            ["input"] = JsonNode.Parse(input),
        });
        return body;
    }

    [Fact]
    [UpstreamTest(Prefix + "uses native Messages output with portable constraints and configured settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_native_Messages_output_with_portable_constraints_and_configured_settings()
    {
        var (model, handler) = Setup();
        var fixture = Fixture();
        var result = await model.DoDecideAsync(
            Call(providerOptions: "{\"anthropic\":{\"structuredOutputMode\":\"outputFormat\"}}", headers: new Dictionary<string, string> { ["x-call"] = "forwarded" }),
            CancellationToken.None);
        Assert.Equal("anthropic.decision", model.Provider);
        Assert.Equal(new[] { "choice", "score", "boolean" }, model.SupportedQuestionTypes);
        Assert.Equal(2, result.Answers.Count);
        Assert.Equal("choice", result.Answers["department"].Type);
        Assert.Equal("billing", result.Answers["department"].Choice);
        Assert.Equal("score", result.Answers["severity"].Type);
        Assert.Equal(1.25, result.Answers["severity"].Score);
        Assert.Equal((int)fixture["usage"]!["input_tokens"]!, result.Usage!.InputTokens);
        Assert.Equal((int)fixture["usage"]!["output_tokens"]!, result.Usage.OutputTokens);
        Assert.Equal("req-test", result.Response!.Headers!["request-id"]);
        Assert.True(result.ProviderMetadata!.Value.TryGetProperty("anthropic", out _));
        Assert.Equal(1, handler.Calls);
        Assert.Equal("https://example.com/v1/messages", handler.Uri);
        Assert.Equal("test-key", handler.Headers["x-api-key"]);
        Assert.Equal("configured", handler.Headers["x-provider"]);
        Assert.Equal("forwarded", handler.Headers["x-call"]);
        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal("claude-haiku-4-5-20251001", (string)body["model"]!);
        Assert.Equal("disabled", (string)body["thinking"]!["type"]!);
        var format = body["output_config"]!["format"]!;
        Assert.Equal("json_schema", (string)format["type"]!);
        var schema = format["schema"]!;
        Assert.Equal("object", (string)schema["type"]!);
        Assert.Equal("string", (string)schema["properties"]!["q0"]!["type"]!);
        Assert.Equal(new[] { "c0", "c1" }, schema["properties"]!["q0"]!["enum"]!.AsArray().Select(node => (string)node!).ToArray());
        Assert.Equal("number", (string)schema["properties"]!["q1"]!["type"]!);
        Assert.Equal(new[] { "q0", "q1" }, schema["required"]!.AsArray().Select(node => (string)node!).ToArray());
        Assert.False((bool)schema["additionalProperties"]!);
        Assert.Null(schema["properties"]!["q1"]!["minimum"]);
        Assert.Null(schema["properties"]!["q1"]!["maximum"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "allows provider options to enable thinking for decide", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_provider_options_to_enable_thinking_for_decide()
    {
        var (model, handler) = Setup();
        await model.DoDecideAsync(Call(providerOptions: "{\"anthropic\":{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":1024}}}"), CancellationToken.None);
        var thinking = JsonNode.Parse(handler.Body)!["thinking"]!;
        Assert.Equal("enabled", (string)thinking["type"]!);
        Assert.Equal(1024, (int)thinking["budget_tokens"]!);
        Assert.Equal(2, thinking.AsObject().Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "selects native structured output automatically for supported models", Coverage = UpstreamCoverage.Covered)]
    public async Task Selects_native_structured_output_automatically_for_supported_models()
    {
        var (model, handler) = Setup();
        await model.DoDecideAsync(Call(), CancellationToken.None);
        Assert.Equal("json_schema", (string)JsonNode.Parse(handler.Body)!["output_config"]!["format"]!["type"]!);
    }

    [Fact]
    [UpstreamTest(Prefix + "supports the existing JSON-tool fallback through provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_the_existing_JSON_tool_fallback_through_provider_options()
    {
        var (model, handler) = Setup(ToolUseFixture("{\"q0\":\"c1\",\"q1\":1.25}"));
        var result = await model.DoDecideAsync(Call(providerOptions: "{\"anthropic\":{\"structuredOutputMode\":\"jsonTool\"}}"), CancellationToken.None);
        Assert.Equal("choice", result.Answers["department"].Type);
        Assert.Equal("billing", result.Answers["department"].Choice);
        var body = JsonNode.Parse(handler.Body)!;
        Assert.Null(body["output_config"]?["format"]);
        Assert.Equal("any", (string)body["tool_choice"]!["type"]!);
        Assert.True((bool)body["tool_choice"]!["disable_parallel_tool_use"]!);
        var tool = Assert.Single(body["tools"]!.AsArray());
        Assert.Equal("json", (string)tool!["name"]!);
    }

    [Theory]
    [InlineData("refusal")]
    [InlineData("max_tokens")]
    [UpstreamTest(Prefix + "rejects %s even with valid JSON", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_stop_reason_even_with_valid_JSON(string stopReason)
    {
        var body = Fixture();
        body["stop_reason"] = stopReason;
        var (model, _) = Setup(body);
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => model.DoDecideAsync(Call(), CancellationToken.None));
    }

    [Fact]
    [UpstreamTest(Prefix + "validates score bounds locally", Coverage = UpstreamCoverage.Covered)]
    public async Task Validates_score_bounds_locally()
    {
        var body = Fixture();
        body["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = "{\"q0\":\"c1\",\"q1\":3}" });
        var (model, _) = Setup(body);
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => model.DoDecideAsync(Call(), CancellationToken.None));
    }

    [Theory]
    [InlineData("outputFormat")]
    [InlineData("jsonTool")]
    [UpstreamTest(Prefix + "decides Boolean alongside Choice and Score using %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Decides_Boolean_alongside_Choice_and_Score_using_structuredOutputMode(string structuredOutputMode)
    {
        const string values = "{\"q0\":\"c1\",\"q1\":1.25,\"q2\":0.02}";
        JsonObject response;
        if (structuredOutputMode == "jsonTool")
        {
            response = ToolUseFixture(values);
        }
        else
        {
            response = Fixture();
            response["stop_reason"] = "end_turn";
            response["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = values });
        }

        var (model, handler) = Setup(response);
        var questions = Questions();
        questions["flag"] = Q("boolean", "Is a refund requested?");
        var result = await model.DoDecideAsync(Call(questions, "{\"anthropic\":{\"structuredOutputMode\":\"" + structuredOutputMode + "\"}}"), CancellationToken.None);
        Assert.Equal(3, result.Answers.Count);
        Assert.Equal("billing", result.Answers["department"].Choice);
        Assert.Equal(1.25, result.Answers["severity"].Score);
        Assert.Equal("boolean", result.Answers["flag"].Type);
        Assert.Equal(0.02, result.Answers["flag"].Probability);
        Assert.Equal(1, handler.Calls);
        var body = JsonNode.Parse(handler.Body)!;
        var schema = structuredOutputMode == "jsonTool" ? body["tools"]![0]!["input_schema"]! : body["output_config"]!["format"]!["schema"]!;
        Assert.Equal("number", (string)schema["properties"]!["q2"]!["type"]!);
        Assert.Equal(new[] { "q0", "q1", "q2" }, schema["required"]!.AsArray().Select(node => (string)node!).ToArray());
        Assert.Null(schema["properties"]!["q2"]!["minimum"]);
        Assert.Null(schema["properties"]!["q2"]!["maximum"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves the deprecated factory and direct model method", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_the_deprecated_factory_and_direct_model_method()
    {
        var handler = new CaptureHandler { ResponseBody = Fixture().ToJsonString() };
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-key" }, handler);
#pragma warning disable CS0618
        // The alias returns the same kind of model as DecisionModel.
        Assert.IsType<DecisionLanguageModel>(provider.DecisionModel("claude-sonnet-4-6"));
        var model = provider.EvaluationModel("claude-sonnet-4-6");
        var call = Call();
        var result = await model.DoEvaluateAsync(new EvaluationModelCall(call.State, new Dictionary<string, EvaluationQuestion>
        {
            ["department"] = EQ("choice", "Pick the team.", new Dictionary<string, object?> { ["technical"] = "Bugs", ["billing"] = "Charges" }),
            ["severity"] = EQ("score", "[\"Rate severity.\"]", new List<object?> { "Low", "Medium", "High" }),
        }, call.ProviderOptions, call.Headers, CancellationToken.None), CancellationToken.None);
#pragma warning restore CS0618
        Assert.Equal("billing", result.Answers["department"].Choice);
        Assert.Contains(".decision", model.Provider);
        Assert.Equal(1, handler.Calls);
    }
}
