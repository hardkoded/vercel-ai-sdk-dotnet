// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.DecisionFixtures;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIDecision;

/// <summary>Fixtures and helpers shared by the <c>openai-decision.test.ts</c> port.</summary>
internal static class OpenAIDecisionSupport
{
    internal const string StateText = "A billing issue with a workaround.";

    internal static List<object?> State()
    {
        return new List<object?> { new Dictionary<string, object?> { ["type"] = "text", ["text"] = StateText } };
    }

    internal static JsonObject Fixture(string name = "decision.json")
    {
        return (JsonObject)JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name)))!;
    }

    internal static Dictionary<string, DecisionQuestion> Questions()
    {
        return new Dictionary<string, DecisionQuestion>
        {
            ["department"] = Q("choice", "Pick the team.", new Dictionary<string, object?> { ["technical"] = "Bugs", ["billing"] = "Charges" }),
            ["severity"] = Q("score", "Rate severity.", new List<object?> { "Low", "Medium", "High" }),
            ["refund"] = Q("boolean", "Is a refund requested?"),
        };
    }

    internal static DecisionModelCall Call(
        IReadOnlyDictionary<string, DecisionQuestion>? questions = null,
        object? state = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default,
        double? maxOutputTokens = null,
        string? reasoning = null)
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new DecisionModelCall(
            P((IEnumerable<object?>)(state ?? State())),
            questions ?? Questions(),
            document.RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            cancellationToken,
            maxOutputTokens,
            reasoning);
    }

    internal static (OpenAIDecisionModel Model, OpenAICapture Capture) Setup(JsonNode? body = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        var capture = new OpenAICapture
        {
            ResponseJson = (body ?? Fixture()).ToJsonString(),
            Status = status,
        };
        capture.ResponseHeaders["x-request-id"] = "req-test";
        var options = new OpenAIOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://example.com/v1/",
            Organization = "org-test",
            Project = "proj-test",
        };
        options.Headers["x-provider"] = "configured";
        options.Headers["shared"] = "provider";
        var provider = OpenAIProvider.Create(options, capture);
        return (provider.DecisionModel("gpt-6-luna"), capture);
    }

    internal static JsonObject RequestBody(OpenAICapture capture)
    {
        return (JsonObject)JsonNode.Parse(capture.Body)!;
    }

    internal static void AssertAnswer(DecisionAnswer answer, string type, string? choice = null, double? score = null, double? probability = null, params (string Key, double Value)[] probabilities)
    {
        Assert.Equal(type, answer.Type);
        Assert.Equal(choice, answer.Choice);
        Assert.Equal(score, answer.Score);
        Assert.Equal(probability, answer.Probability);
        if (probabilities.Length == 0)
        {
            Assert.Null(answer.Probabilities);
            return;
        }

        Assert.Equal(probabilities.ToDictionary(pair => pair.Key, pair => pair.Value), answer.Probabilities);
    }

    /// <summary>Converts parsed JSON into the values <see cref="DecisionQuestion"/> accepts.</summary>
    internal static object? ToValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var map = new Dictionary<string, object?>();
                foreach (var property in element.EnumerateObject())
                {
                    map[property.Name] = ToValue(property.Value);
                }

                return map;
            case JsonValueKind.Array:
                return element.EnumerateArray().Select(ToValue).ToList();
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                return element.GetDouble();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            default:
                return null;
        }
    }
}
