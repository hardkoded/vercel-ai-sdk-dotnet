// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIDecision.OpenAIDecisionSupport;

namespace Vercel.AI.Tests.Upstream.Operations.OpenAIDecisions;

/// <summary>Port of <c>openai-decisions.test.ts</c>.</summary>
public sealed class OpenAIDecisionsTests
{
    private const string Prefix = "packages/ai/src/decide/openai-decisions.test.ts::";

    private const string Answers = """
        [
          {
            "type": "choice", "name": "team", "choice": "billing", "confidence": 0.6,
            "probabilities": [
              { "value": "billing", "probability": 0.34 },
              { "value": "technical", "probability": 0.33 },
              { "value": "other", "probability": 0.32 }
            ]
          },
          {
            "type": "score", "name": "severity", "score": 0.98, "confidence": 0.79,
            "probabilities": [
              { "value": 0, "probability": 0.08 },
              { "value": 1, "probability": 0.86 },
              { "value": 2, "probability": 0.06 }
            ]
          },
          { "type": "predicate", "name": "refund", "probability": 0.96 }
        ]
        """;

    public static TheoryData<int> InvalidAnswers()
    {
        return new TheoryData<int> { 0, 1, 2, 3, 4, 5 };
    }

    private static Dictionary<string, EvaluationQuestion> Questions()
    {
        return new Dictionary<string, EvaluationQuestion>
        {
            ["team"] = new EvaluationQuestion("choice", "Route the ticket.", new Dictionary<string, object?> { ["billing"] = "Charges", ["technical"] = "Bugs", ["other"] = null }),
            ["severity"] = new EvaluationQuestion("score", "Rate severity.", new List<object?> { "Low", "Medium", "High" }),
            ["refund"] = new EvaluationQuestion("boolean", "Is a refund requested?"),
        };
    }

    private static JsonArray ParsedAnswers()
    {
        return (JsonArray)JsonNode.Parse(Answers)!;
    }

    private static (OpenAIDecisionModel Model, OpenAICapture Capture) Setup(JsonNode? responseAnswers = null)
    {
        var capture = new OpenAICapture
        {
            ResponseJson = new JsonObject
            {
                ["answers"] = responseAnswers ?? ParsedAnswers(),
                ["model"] = "gpt-6-luna-resolved",
                ["usage"] = JsonNode.Parse("{\"input_tokens\":387,\"output_tokens\":3,\"total_tokens\":390,\"input_tokens_details\":{\"cached_tokens\":0,\"cache_write_tokens\":0},\"output_tokens_details\":{\"reasoning_tokens\":0}}"),
            }.ToJsonString(),
        };
        var provider = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test" }, capture);
        return (provider.DecisionModel("gpt-6-luna"), capture);
    }

    [Fact]
    [UpstreamTest(Prefix + "decides native Decisions answers through core with rounded distributions and confidence intact", Coverage = UpstreamCoverage.Covered)]
    public async Task Decides_native_Decisions_answers_through_core_with_rounded_distributions_and_confidence_intact()
    {
        var (model, capture) = Setup();
        var state = new Dictionary<string, object?> { ["ticket"] = "Charged twice." };
        var result = await Evaluate.EvaluateAsync(new EvaluateRequest { Model = model, State = state, Questions = Questions() });
        Assert.Equal(1, capture.Calls);
        Assert.Equal("https://api.openai.com/v1/decisions", capture.Uri);
        Assert.Equal(3, result.Answers.Count);
        AssertAnswer(result.Answers["team"], "choice", choice: "billing", probabilities: new[] { ("billing", 0.34), ("technical", 0.33), ("other", 0.32) });
        AssertAnswer(result.Answers["severity"], "score", score: 0.98, probabilities: new[] { ("0", 0.08), ("1", 0.86), ("2", 0.06) });
        AssertAnswer(result.Answers["refund"], "boolean", probability: 0.96);
        JsonAssert.Equal(result.ProviderMetadata!.Value, """
            {
              "openai": {
                "confidence": { "team": 0.6, "severity": 0.79 },
                "usage": {
                  "input_tokens": 387,
                  "output_tokens": 3,
                  "total_tokens": 390,
                  "input_tokens_details": { "cached_tokens": 0, "cache_write_tokens": 0 },
                  "output_tokens_details": { "reasoning_tokens": 0 }
                }
              }
            }
            """);
        Assert.Equal(387, result.Usage.InputTokens);
        Assert.Equal(3, result.Usage.OutputTokens);
        Assert.Equal(390, result.Usage.TotalTokens);
        Assert.Equal("gpt-6-luna-resolved", result.Response.ModelId);
        Assert.Equal(2, result.Rounding!.ProbabilityDecimals);
        Assert.Equal(2, result.Rounding.ScoreDecimals);
    }

    [Fact]
    [UpstreamTest(Prefix + "allows the declared score rounding when validating a two-level distribution", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_the_declared_score_rounding_when_validating_a_two_level_distribution()
    {
        var (model, _) = Setup(JsonNode.Parse("""
            [{ "type": "score", "name": "severity", "score": 0.5, "probabilities": [{ "value": 0, "probability": 0.51 }, { "value": 1, "probability": 0.49 }] }]
            """));
        var result = await Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = model,
            State = "A billing issue with a workaround.",
            Questions = new Dictionary<string, EvaluationQuestion>
            {
                ["severity"] = new EvaluationQuestion("score", "Rate severity.", new List<object?> { "Low", "High" }),
            },
        });
        AssertAnswer(result.Answers["severity"], "score", score: 0.5, probabilities: new[] { ("0", 0.51), ("1", 0.49) });
    }

    [Theory]
    [MemberData(nameof(InvalidAnswers))]
    [UpstreamTest(Prefix + "rejects invalid native semantics without returning partial results", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_native_semantics_without_returning_partial_results(int index)
    {
        JsonObject Edit(int answer, string property, JsonNode? value)
        {
            var copy = (JsonObject)ParsedAnswers()[answer]!;
            copy[property] = value;
            return copy;
        }

        JsonObject EditChoice(JsonNode? probabilities)
        {
            var copy = Edit(0, "probabilities", probabilities);
            copy["choice"] = "technical";
            return copy;
        }

        var invalid = index switch
        {
            0 => JsonNode.Parse("{\"type\":\"predicate\",\"name\":\"team\",\"probability\":0.5}")!,
            1 => Edit(0, "choice", "unknown"),
            2 => EditChoice(JsonNode.Parse("[{\"value\":\"billing\",\"probability\":0.35},{\"value\":\"technical\",\"probability\":0.33},{\"value\":\"other\",\"probability\":0.32}]")),
            3 => Edit(0, "probabilities", JsonNode.Parse("[{\"value\":\"billing\",\"probability\":1}]")),
            4 => Edit(1, "score", 1.5),
            _ => Edit(1, "score", 3),
        };
        var replacement = new JsonArray();
        foreach (var original in ParsedAnswers())
        {
            replacement.Add(original!["name"]!.GetValue<string>() == invalid["name"]!.GetValue<string>() ? invalid.DeepClone() : original.DeepClone());
        }

        await Assert.ThrowsAsync<InvalidResponseDataException>(() => Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = Setup(replacement).Model,
            State = "Ticket",
            Questions = Questions(),
            MaxRetries = 0,
        }));
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects a native refusal with a refusal error without retrying", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_native_refusal_with_a_refusal_error_without_retrying()
    {
        var answers = new JsonArray();
        foreach (var answer in ParsedAnswers())
        {
            answers.Add(answer!["name"]!.GetValue<string>() == "refund" ? JsonNode.Parse("{\"type\":\"refusal\",\"name\":\"refund\"}") : answer.DeepClone());
        }

        var (model, capture) = Setup(answers);
        var error = await Assert.ThrowsAsync<DecisionRefusalError>(() => Evaluate.EvaluateAsync(new EvaluateRequest { Model = model, State = "Ticket", Questions = Questions() }));
        Assert.Equal("AI_DecisionRefusalError", error.ErrorName);
        Assert.Equal(new[] { "refund" }, error.QuestionIds);
        Assert.Equal(1, capture.Calls);
    }
}
