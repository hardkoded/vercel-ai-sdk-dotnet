// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Perplexity;
using static Vercel.AI.Tests.DecisionFixtures;

namespace Vercel.AI.Tests.Upstream.Perplexity.PerplexityDecisionModel;

/// <summary>Fixtures and helpers shared by the <c>perplexity-decision-model.test.ts</c> port.</summary>
internal static class PerplexityDecisionModelSupport
{
    internal static JsonObject Fixture()
    {
        // Illustrative response from the official OpenAPI schema, not a live capture.
        return (JsonObject)JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "perplexity-decisions.json")))!;
    }

    internal static Dictionary<string, DecisionQuestion> Questions()
    {
        return new Dictionary<string, DecisionQuestion>
        {
            ["defect"] = Q("boolean", "Does the review report a defect?"),
            ["sentiment"] = Q(
                "choice",
                "What is the sentiment?",
                new Dictionary<string, object?> { ["positive"] = "Satisfied", ["mixed"] = "Praise and complaints", ["negative"] = "Dissatisfied" }),
            ["severity"] = Q("score", "How severe is the problem?", new List<object?> { "Cosmetic", "Inconvenient", "Product unusable" }),
        };
    }

    internal const string QuestionsJson = """
        {
          "defect": { "type": "noul", "instructions": "Does the review report a defect?" },
          "sentiment": {
            "type": "choice",
            "instructions": "What is the sentiment?",
            "criteria": { "positive": "Satisfied", "mixed": "Praise and complaints", "negative": "Dissatisfied" }
          },
          "severity": {
            "type": "score",
            "instructions": "How severe is the problem?",
            "criteria": ["Cosmetic", "Inconvenient", "Product unusable"]
          }
        }
        """;

    internal static List<object?> State()
    {
        return new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["type"] = "json",
                ["value"] = new Dictionary<string, object?> { ["review"] = "Great sound, but the battery broke." },
            },
        };
    }

    internal static DecisionModelCall Call(
        IReadOnlyDictionary<string, DecisionQuestion>? questions = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default,
        double? maxOutputTokens = null,
        string? reasoning = null)
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new DecisionModelCall(P(State()), questions ?? Questions(), document.RootElement.Clone(), headers ?? new Dictionary<string, string>(), cancellationToken, maxOutputTokens, reasoning);
    }

    internal static (PerplexityProvider Provider, OpenAICapture Capture) SetupProvider(JsonNode? body = null, HttpStatusCode status = HttpStatusCode.OK, string? rawBody = null)
    {
        var capture = new OpenAICapture { ResponseJson = rawBody ?? (body ?? Fixture()).ToJsonString(), Status = status };
        capture.ResponseHeaders["x-request-id"] = "request-123";
        capture.ResponseHeaders["retry-after"] = "2";
        capture.ResponseHeaders["x-ratelimit-remaining"] = "9";
        var options = new OpenAICompatibleOptions { ApiKey = "test-key", BaseUrl = "https://example.com/" };
        options.Headers["x-provider"] = "custom";
        options.Headers["shared"] = "provider";
        return (PerplexityProvider.Create(options, capture), capture);
    }

    internal static (Vercel.AI.Perplexity.PerplexityDecisionModel Model, OpenAICapture Capture) Setup(JsonNode? body = null, HttpStatusCode status = HttpStatusCode.OK, string? rawBody = null)
    {
        var (provider, capture) = SetupProvider(body, status, rawBody);
        return (provider.DecisionModel("pplx-decider-latest"), capture);
    }

    internal static JsonObject RequestBody(OpenAICapture capture)
    {
        return (JsonObject)JsonNode.Parse(capture.Body)!;
    }
}
