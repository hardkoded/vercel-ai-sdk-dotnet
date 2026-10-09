// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayDecisionModel.GatewayDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayDecisionModel;

/// <summary>Port of <c>gateway-decision-model.test.ts</c> &gt; <c>GatewayDecisionModel &gt; doDecide</c>.</summary>
public sealed class GatewayDecisionModelDoDecideTests
{
    private const string Prefix = "packages/gateway/src/gateway-decision-model.test.ts::GatewayDecisionModel > doDecide::";

    private static void AssertDummyAnswers(IReadOnlyDictionary<string, EvaluationAnswer> answers)
    {
        Assert.Equal(3, answers.Count);
        Assert.Equal("boolean", answers["correct"].Type);
        Assert.Equal(0.97, answers["correct"].Probability);
        Assert.Equal("choice", answers["tone"].Type);
        Assert.Equal("neutral", answers["tone"].Choice);
        Assert.Equal(new Dictionary<string, double> { ["neutral"] = 0.9, ["playful"] = 0.1 }, answers["tone"].Probabilities);
        Assert.Equal("score", answers["quality"].Type);
        Assert.Equal(1.8, answers["quality"].Score);
        Assert.Equal(new Dictionary<string, double> { ["0"] = 0.05, ["1"] = 0.1, ["2"] = 0.85 }, answers["quality"].Probabilities);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_correctly()
    {
        var (model, capture) = Setup();
        await model.DoDecideAsync(Call(headers: new Dictionary<string, string> { ["Custom-Header"] = "test-value" }));
        Assert.Equal("Bearer test-token", capture.Headers["Authorization"]);
        Assert.Equal("test-value", capture.Headers["Custom-Header"]);
        Assert.Equal("4", capture.Headers["ai-decision-model-specification-version"]);
        Assert.Equal("typesafe-ai/jev", capture.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send state and questions in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_state_and_questions_in_request_body()
    {
        var (model, capture) = Setup();
        await model.DoDecideAsync(Call());
        JsonAssert.Equal(RequestBody(capture), "{\"state\":\"" + TestState + "\",\"questions\":" + QuestionsJson + "}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass providerOptions into request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_providerOptions_into_request_body()
    {
        var (model, capture) = Setup();
        await model.DoDecideAsync(Call("{\"typesafe\":{\"effort\":\"high\"}}"));
        JsonAssert.Equal(RequestBody(capture)!["providerOptions"], "{\"typesafe\":{\"effort\":\"high\"}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass conditional model fallbacks into request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_conditional_model_fallbacks_into_request_body()
    {
        var (model, capture) = Setup();
        const string providerOptions = """
            {
              "gateway": {
                "models": [
                  {
                    "model": "openai/gpt-5.6-sol",
                    "when": {
                      "any": [
                        { "question": "tone", "confidenceBelow": 0.6 },
                        { "question": "correct", "probabilityBetween": [0.4, 0.6] }
                      ]
                    }
                  },
                  "anthropic/claude-sonnet-5"
                ]
              }
            }
            """;
        await model.DoDecideAsync(Call(providerOptions));
        JsonAssert.Equal(RequestBody(capture)!["providerOptions"], providerOptions);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass service-owned gateway options through with conditional fallbacks", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_service_owned_gateway_options_through_with_conditional_fallbacks()
    {
        var (model, capture) = Setup();
        const string providerOptions = """
            {
              "gateway": {
                "models": [
                  { "model": "openai/gpt-5.6-sol", "when": { "question": "tone", "confidenceBelow": 0.6 } }
                ],
                "order": ["openai"],
                "serviceOwnedOption": { "nested": ["value", 1, true] }
              },
              "typesafe": { "effort": "high" }
            }
            """;
        await model.DoDecideAsync(Call(providerOptions));
        JsonAssert.Equal(RequestBody(capture), "{\"state\":\"" + TestState + "\",\"questions\":" + QuestionsJson + ",\"providerOptions\":" + providerOptions + "}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject invalid conditional model fallbacks", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_invalid_conditional_model_fallbacks()
    {
        var (model, capture) = Setup();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => model.DoDecideAsync(Call("""
            {
              "gateway": {
                "models": [
                  { "model": "openai/gpt-5.6-sol", "when": { "question": "correct", "probabilityBetween": [0.7, 0.3] } }
                ]
              }
            }
            """)));
        Assert.Contains("invalid gateway provider options", error.Message);
        Assert.Equal(0, capture.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract choice, score, and boolean answers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_choice_score_and_boolean_answers()
    {
        var result = await Setup(Response()).Model.DoDecideAsync(Call());
        AssertDummyAnswers(result.Answers);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract refusal answers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_refusal_answers()
    {
        var answers = "{\"correct\":{\"type\":\"boolean\",\"probability\":0.97},\"tone\":{\"type\":\"refusal\"},\"quality\":{\"type\":\"score\",\"score\":1.8,\"probabilities\":{\"0\":0.05,\"1\":0.1,\"2\":0.85}}}";
        var result = await Setup(Response(answers)).Model.DoDecideAsync(Call());
        Assert.Equal(3, result.Answers.Count);
        Assert.Equal("refusal", result.Answers["tone"].Type);
        Assert.Null(result.Answers["tone"].Choice);
        Assert.Null(result.Answers["tone"].Probabilities);
        Assert.Equal(0.97, result.Answers["correct"].Probability);
        Assert.Equal(1.8, result.Answers["quality"].Score);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract rounding", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_rounding()
    {
        var result = await Setup(Response(null, ("rounding", "{\"probabilityDecimals\":2,\"scoreDecimals\":2}"))).Model.DoDecideAsync(Call());
        Assert.Equal(2, result.Rounding!.ProbabilityDecimals);
        Assert.Equal(2, result.Rounding.ScoreDecimals);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage()
    {
        var result = await Setup(Response(null, ("usage", "{\"inputTokens\":42,\"outputTokens\":7}"))).Model.DoDecideAsync(Call());
        Assert.Equal(42, result.Usage!.InputTokens);
        Assert.Equal(7, result.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should omit rounding and usage when absent", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_omit_rounding_and_usage_when_absent()
    {
        var result = await Setup().Model.DoDecideAsync(Call());
        Assert.Null(result.Rounding);
        Assert.Null(result.Usage);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_warnings()
    {
        var body = Response(null, ("warnings", "[{\"type\":\"unsupported\",\"feature\":\"providerOptions.typesafe.effort\",\"details\":\"This model ignores effort.\"}]"));
        var result = await Setup(body).Model.DoDecideAsync(Call());
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("providerOptions.typesafe.effort", warning.Feature);
        Assert.Equal("This model ignores effort.", warning.Details);
    }

    [Fact]
    [UpstreamTest(Prefix + "should default warnings to an empty array when absent", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_default_warnings_to_an_empty_array_when_absent()
    {
        var result = await Setup().Model.DoDecideAsync(Call());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_response_metadata()
    {
        var (model, capture) = Setup();
        capture.ResponseHeaders["x-request-id"] = "req-123";
        var result = await model.DoDecideAsync(Call());
        Assert.Equal("typesafe-ai/jev", result.Response!.ModelId);
        Assert.Equal("req-123", result.Response.Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should attribute the response to the returned model after a fallback", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_attribute_the_response_to_the_returned_model_after_a_fallback()
    {
        var result = await Setup(Response(null, ("model", "\"anthropic/claude-sonnet-5\""))).Model.DoDecideAsync(Call());
        Assert.Equal("anthropic/claude-sonnet-5", result.Response!.ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "should attribute the response to the requested model when none is returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_attribute_the_response_to_the_requested_model_when_none_is_returned()
    {
        var result = await Setup().Model.DoDecideAsync(Call());
        Assert.Equal("typesafe-ai/jev", result.Response!.ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_provider_metadata()
    {
        var result = await Setup(Response(null, ("providerMetadata", "{\"gateway\":{\"cost\":\"0.002\"}}"))).Model.DoDecideAsync(Call());
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"gateway\":{\"cost\":\"0.002\"}}");
    }
}
