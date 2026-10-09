// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayDecisionModel;

/// <summary>Fixtures and helpers shared by the <c>gateway-decision-model.test.ts</c> port.</summary>
internal static class GatewayDecisionModelSupport
{
    internal const string TestState = "The capital of France is Paris.";

    internal const string QuestionsJson = """
        {
          "correct": { "type": "boolean", "instructions": "Is the statement factually correct?" },
          "tone": {
            "type": "choice",
            "instructions": "What is the tone?",
            "criteria": { "neutral": "Plain and factual.", "playful": null }
          },
          "quality": {
            "type": "score",
            "instructions": "Rate the quality.",
            "criteria": ["Poor.", "Acceptable.", "Excellent."]
          }
        }
        """;

    internal const string DummyAnswers = """
        {
          "correct": { "type": "boolean", "probability": 0.97 },
          "tone": { "type": "choice", "choice": "neutral", "probabilities": { "neutral": 0.9, "playful": 0.1 } },
          "quality": { "type": "score", "score": 1.8, "probabilities": { "0": 0.05, "1": 0.1, "2": 0.85 } }
        }
        """;

    internal static Dictionary<string, EvaluationQuestion> Questions()
    {
        return new Dictionary<string, EvaluationQuestion>
        {
            ["correct"] = new EvaluationQuestion("boolean", "Is the statement factually correct?"),
            ["tone"] = new EvaluationQuestion("choice", "What is the tone?", new Dictionary<string, object?> { ["neutral"] = "Plain and factual.", ["playful"] = null }),
            ["quality"] = new EvaluationQuestion("score", "Rate the quality.", new List<object?> { "Poor.", "Acceptable.", "Excellent." }),
        };
    }

    internal static EvaluationModelCall Call(string providerOptions = "{}", IReadOnlyDictionary<string, string>? headers = null)
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new EvaluationModelCall(TestState, Questions(), document.RootElement.Clone(), headers ?? new Dictionary<string, string>(), CancellationToken.None);
    }

    /// <summary>Builds a model whose requests are answered with <paramref name="body"/> (a decision-model response).</summary>
    internal static (Vercel.AI.Gateway.GatewayDecisionModel Model, OpenAICapture Capture) Setup(string? body = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        var capture = new OpenAICapture { ResponseJson = body ?? "{\"answers\":" + DummyAnswers + "}", Status = status };
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, capture);
        return (provider.DecisionModel("typesafe-ai/jev"), capture);
    }

    internal static string Response(string? answers = null, params (string Name, string Json)[] extra)
    {
        var body = new JsonObject { ["answers"] = JsonNode.Parse(answers ?? DummyAnswers) };
        foreach (var pair in extra)
        {
            body[pair.Name] = JsonNode.Parse(pair.Json);
        }

        return body.ToJsonString();
    }

    internal static JsonNode? RequestBody(OpenAICapture capture)
    {
        return JsonNode.Parse(capture.Body);
    }
}
