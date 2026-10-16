// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.DecisionFixtures;

namespace Vercel.AI.Tests.Upstream.Operations.Decide;

/// <summary>Shared fixtures for the <c>decide.test.ts</c> ports.</summary>
internal static class DecideSupport
{
    internal static Dictionary<string, DecisionQuestion> StructuredQuestions()
    {
        return new Dictionary<string, DecisionQuestion>
        {
            ["topic"] = Q(
                "choice",
                "Pick a \"team\".\nKeep this string.",
                new Dictionary<string, object?>
                {
                    ["billing"] = new Dictionary<string, object?> { ["includes"] = new List<object?> { "charges" } },
                    ["support"] = new List<object?> { "help", new Dictionary<string, object?> { ["urgent"] = true } },
                    ["other"] = null,
                }),
            ["severity"] = Q(
                "score",
                new Dictionary<string, object?> { ["task"] = "Rate severity" },
                new List<object?> { new Dictionary<string, object?> { ["level"] = "low" }, new List<object?> { "medium", "high" }, null }),
            ["refund"] = Q(
                "boolean",
                new List<object?> { "Refund?", new Dictionary<string, object?> { ["locale"] = "en" } },
                new Dictionary<string, object?>
                {
                    ["true"] = new Dictionary<string, object?> { ["requested"] = true },
                    ["false"] = new List<object?> { "status only" },
                }),
            ["noCriteria"] = Q("boolean", "No criteria."),
            ["emptyCriteria"] = Q("boolean", "Empty criteria.", new Dictionary<string, object?>()),
            ["nullCriteria"] = Q("boolean", "Null criteria.", new Dictionary<string, object?> { ["true"] = null, ["false"] = null }),
            ["falseCriteria"] = Q("boolean", "False criteria.", new Dictionary<string, object?> { ["true"] = null, ["false"] = "No refund requested" }),
        };
    }

    internal static Dictionary<string, object?> TextPart(string text)
    {
        return new Dictionary<string, object?> { ["type"] = "text", ["text"] = text };
    }

    internal static Dictionary<string, object?> JsonPart(object? value)
    {
        return new Dictionary<string, object?> { ["type"] = "json", ["value"] = value };
    }

    internal static Dictionary<string, object?> FilePart(string mediaType, object data, string? filename)
    {
        var part = new Dictionary<string, object?> { ["type"] = "file", ["mediaType"] = mediaType, ["data"] = new Dictionary<string, object?> { ["type"] = "data", ["data"] = data } };
        if (filename != null)
        {
            part["filename"] = filename;
        }

        return part;
    }

    internal static Dictionary<string, DecisionQuestion> SampleQuestions()
    {
        return new Dictionary<string, DecisionQuestion>
        {
            ["topic"] = Q("choice", "Team?", new Dictionary<string, object?> { ["billing"] = null, ["support"] = new Dictionary<string, object?> { ["includes"] = new List<object?> { "help" } } }),
            ["severity"] = Q("score", new List<object?> { "Severity?" }, new List<object?> { "Low", "Medium", "High" }),
            ["refund"] = Q("boolean", "Refund?", new Dictionary<string, object?> { ["true"] = "Money back", ["false"] = null }),
        };
    }

    internal static Dictionary<string, DecisionAnswer> SampleAnswers()
    {
        return new Dictionary<string, DecisionAnswer>
        {
            ["refund"] = new DecisionAnswer("boolean", probability: 0.92),
            ["severity"] = new DecisionAnswer("score", score: 1.6, probabilities: new Dictionary<string, double> { ["0"] = 0, ["1"] = 0.4, ["2"] = 0.6 }),
            ["topic"] = new DecisionAnswer("choice", choice: "billing", probabilities: new Dictionary<string, double> { ["billing"] = 0.9, ["support"] = 0.1 }),
        };
    }

    internal static Dictionary<string, DecisionAnswer> With(string key, DecisionAnswer replacement)
    {
        var answers = SampleAnswers();
        answers[key] = replacement;
        return answers;
    }
}

/// <summary>Records calls and returns a configured result.</summary>
internal sealed class DecideModel : IDecisionCaller
{
    public string Provider => "mock-provider";

    public string ModelId => "mock-model-id";

    public string SpecificationVersion { get; set; } = "v4";

    public IReadOnlyList<string> Supported { get; set; } = new[] { "choice", "score", "boolean" };

    public IReadOnlyList<string> SupportedQuestionTypes => Supported;

    public DecisionModelResult Result { get; set; } = new DecisionModelResult(DecideSupport.SampleAnswers());

    public bool ReturnNullAnswers { get; set; }

    public int Failures { get; set; }

    public int FailureStatus { get; set; } = 429;

    public Exception? FailWith { get; set; }

    public (CancellationTokenSource Source, Exception Reason)? AbortDuringCall { get; set; }

    public List<DecisionModelCall> Calls { get; } = new List<DecisionModelCall>();

    public Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken)
    {
        Calls.Add(call);
        if (AbortDuringCall is { } abort)
        {
            abort.Source.Cancel();
            return Task.FromResult(Result);
        }

        if (FailWith != null)
        {
            throw FailWith;
        }

        if (Failures > 0)
        {
            Failures--;
            var message = FailureStatus == 529 ? "Overloaded" : "Rate limited";
            throw new RetryableCallException(message, FailureStatus, new Dictionary<string, string> { ["retry-after-ms"] = "0" });
        }

        if (ReturnNullAnswers)
        {
            return Task.FromResult(new DecisionModelResult(new Dictionary<string, DecisionAnswer>()));
        }

        return Task.FromResult(Result);
    }
}
