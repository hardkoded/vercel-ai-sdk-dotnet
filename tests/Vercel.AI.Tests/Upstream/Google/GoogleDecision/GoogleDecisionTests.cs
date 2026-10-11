// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Google.GoogleDecision.GoogleDecisionSupport;

namespace Vercel.AI.Tests.Upstream.Google.GoogleDecision;

/// <summary>Port of <c>google-decision.test.ts</c>.</summary>
public sealed class GoogleDecisionTests
{
    private const string Prefix = "packages/google/src/google-decision.test.ts::";

    public static TheoryData<string> FinishReasons()
    {
        return new TheoryData<string> { "SAFETY", "MAX_TOKENS" };
    }

    [Fact]
    [UpstreamTest(Prefix + "uses Gemini structured output and forwards thinking options and cancellation", Coverage = UpstreamCoverage.Partial, Note = "Asserting that the fetch request carries the same AbortSignal instance has no .NET equivalent; the call runs with a CancellationToken.")]
    public async Task Uses_Gemini_structured_output_and_forwards_thinking_options_and_cancellation()
    {
        var (model, capture) = Setup();
        var fixture = Fixture();
        using var source = new CancellationTokenSource();
        var result = await model.DoDecideAsync(Call(
            providerOptions: "{\"google\":{\"thinkingConfig\":{\"thinkingLevel\":\"high\"}}}",
            headers: new Dictionary<string, string> { ["x-call"] = "forwarded" },
            cancellationToken: source.Token), source.Token);
        Assert.Equal("google.decision", model.Provider);
        Assert.Equal(new[] { "choice", "score", "boolean" }, model.SupportedQuestionTypes);
        Assert.Equal("billing", result.Answers["department"].Choice);
        Assert.Equal("choice", result.Answers["department"].Type);
        Assert.Equal("score", result.Answers["severity"].Type);
        Assert.Equal(1.25, result.Answers["severity"].Score);
        Assert.Equal((int)fixture["usageMetadata"]!["promptTokenCount"]!, result.Usage!.InputTokens);
        Assert.Equal((int)fixture["usageMetadata"]!["candidatesTokenCount"]!, result.Usage.OutputTokens);
        Assert.Equal("req-test", result.Response!.Headers!["x-request-id"]);
        Assert.Equal("gemini-3.5-flash-lite", model.ModelId);
        Assert.Equal((string)fixture["responseId"]!, result.Response.Id);
        Assert.True(result.ProviderMetadata!.Value.TryGetProperty("google", out _));
        Assert.Equal(1, capture.Calls);
        Assert.Equal("https://example.com/v1beta/models/gemini-3.5-flash-lite:generateContent", capture.Uri);
        Assert.Equal("test-key", capture.Headers["x-goog-api-key"]);
        Assert.Equal("configured", capture.Headers["x-provider"]);
        Assert.Equal("forwarded", capture.Headers["x-call"]);
        var config = RequestBody(capture)["generationConfig"]!;
        Assert.Equal("application/json", (string)config["responseMimeType"]!);
        Assert.Equal("high", (string)config["thinkingConfig"]!["thinkingLevel"]!);
        var schema = config["responseJsonSchema"]!;
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
    [UpstreamTest(Prefix + "defaults to the minimum thinking level through the provider reasoning mapping", Coverage = UpstreamCoverage.Covered)]
    public async Task Defaults_to_the_minimum_thinking_level_through_the_provider_reasoning_mapping()
    {
        var (model, capture) = Setup();
        await model.DoDecideAsync(Call());
        var thinking = RequestBody(capture)["generationConfig"]!["thinkingConfig"]!;
        Assert.Equal("minimal", (string)thinking["thinkingLevel"]!);
        Assert.Single(thinking.AsObject());
    }

    [Fact]
    [UpstreamTest(Prefix + "ignores thought text while counting reasoning tokens in usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Ignores_thought_text_while_counting_reasoning_tokens_in_usage()
    {
        var body = Fixture();
        var parts = body["candidates"]![0]!["content"]!["parts"]!.AsArray();
        parts.Insert(0, new JsonObject { ["thought"] = true, ["text"] = "Internal reasoning" });
        body["usageMetadata"]!["thoughtsTokenCount"] = 10;
        body["usageMetadata"]!["totalTokenCount"] = (int)body["usageMetadata"]!["totalTokenCount"]! + 10;
        var (model, _) = Setup(body);
        var result = await model.DoDecideAsync(Call());
        Assert.Equal("score", result.Answers["severity"].Type);
        Assert.Equal(1.25, result.Answers["severity"].Score);
        Assert.Equal((int)Fixture()["usageMetadata"]!["candidatesTokenCount"]! + 10, result.Usage!.OutputTokens);
    }

    [Theory]
    [MemberData(nameof(FinishReasons))]
    [UpstreamTest(Prefix + "rejects %s even with valid JSON", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_finish_reason_even_with_valid_JSON(string finishReason)
    {
        var body = Fixture();
        body["candidates"]![0]!["finishReason"] = finishReason;
        var (model, _) = Setup(body);
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => model.DoDecideAsync(Call()));
    }

    [Fact]
    [UpstreamTest(Prefix + "validates score bounds locally", Coverage = UpstreamCoverage.Covered)]
    public async Task Validates_score_bounds_locally()
    {
        var body = Fixture();
        body["candidates"]![0]!["content"] = new JsonObject
        {
            ["role"] = "model",
            ["parts"] = new JsonArray(new JsonObject { ["text"] = "{\"q0\":\"c1\",\"q1\":3}" }),
        };
        var (model, _) = Setup(body);
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => model.DoDecideAsync(Call()));
    }

    [Fact]
    [UpstreamTest(Prefix + "decides Boolean alongside Choice and Score in one Gemini request", Coverage = UpstreamCoverage.Covered)]
    public async Task Decides_Boolean_alongside_Choice_and_Score_in_one_Gemini_request()
    {
        var body = Fixture();
        body["candidates"]![0]!["content"]!["parts"] = new JsonArray(new JsonObject { ["text"] = "{\"q0\":\"c1\",\"q1\":1.25,\"q2\":0.02}" });
        var (model, capture) = Setup(body);
        var questions = Questions();
        questions["flag"] = new DecisionQuestion("boolean", "Is a refund requested?");
        var result = await model.DoDecideAsync(Call(questions));
        Assert.Equal("billing", result.Answers["department"].Choice);
        Assert.Equal(1.25, result.Answers["severity"].Score);
        Assert.Equal("boolean", result.Answers["flag"].Type);
        Assert.Equal(0.02, result.Answers["flag"].Probability);
        Assert.Equal(3, result.Answers.Count);
        Assert.Equal(1, capture.Calls);
        var schema = RequestBody(capture)["generationConfig"]!["responseJsonSchema"]!;
        Assert.Equal("number", (string)schema["properties"]!["q2"]!["type"]!);
        Assert.Equal(new[] { "q0", "q1", "q2" }, schema["required"]!.AsArray().Select(node => (string)node!).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects an aborted decision without a request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_aborted_decision_without_a_request()
    {
        var (model, capture) = Setup();
        using var source = new CancellationTokenSource();
        source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => model.DoDecideAsync(Call(cancellationToken: source.Token), source.Token));
        Assert.Equal(0, capture.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves the deprecated factory and direct model method", Coverage = UpstreamCoverage.Partial, Note = "The port has no EvaluationModel factory on GoogleProvider; the direct DoDecideAsync method is checked through DecisionModel.")]
    public async Task Preserves_the_deprecated_factory_and_direct_model_method()
    {
        var (model, capture) = Setup(withBaseUrl: false);
        var result = await model.DoDecideAsync(Call(), CancellationToken.None);
        Assert.Equal("choice", result.Answers["department"].Type);
        Assert.Equal("billing", result.Answers["department"].Choice);
        Assert.Contains(".decision", model.Provider);
        Assert.Equal(1, capture.Calls);
    }
}
