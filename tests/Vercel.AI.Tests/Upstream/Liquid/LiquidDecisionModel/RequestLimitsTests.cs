// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Liquid.LiquidDecisionModel.LiquidDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.Liquid.LiquidDecisionModel;

/// <summary>Port of <c>liquid-decision-model.test.ts</c> &gt; <c>request limits</c>.</summary>
public sealed class RequestLimitsTests
{
    private const string Prefix =
        "packages/liquid/src/liquid-decision-model.test.ts::request limits::";
    private const string Commit = "cf5fe9a22344de9285e77aef4144bbb54e15b788";

    public static TheoryData<int> InvalidLimits => new() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    [Theory]
    [MemberData(nameof(InvalidLimits))]
    [UpstreamTest(
        Prefix + "rejects %s before HTTP",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_invalid_limits_before_HTTP(int index)
    {
        var (questions, state) = Invalid(index);
        var (_, model, capture) = Setup();
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            model.DoDecideAsync(Call(questions, state))
        );
        Assert.Equal(0, capture.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "sends Boolean criteria and null Choice descriptions",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Sends_Boolean_criteria_and_null_Choice_descriptions()
    {
        var answers = TextAnswers();
        var department = (JsonObject)answers["department"]!;
        department["probabilities"] = new JsonObject { ["billing"] = 0.4, ["technical"] = 0.6 };
        var (_, model, capture) = Setup(
            new JsonObject
            {
                ["answers"] = new JsonObject
                {
                    ["department"] = department.DeepClone(),
                    ["is_urgent"] = answers["is_urgent"]!.DeepClone(),
                },
            }
        );
        await model.DoDecideAsync(
            Call(
                new Dictionary<string, DecisionQuestion>
                {
                    ["department"] = Q(
                        "choice",
                        "Which team?",
                        new Dictionary<string, object?> { ["billing"] = null, ["technical"] = null }
                    ),
                    ["is_urgent"] = Q(
                        "boolean",
                        "Urgent?",
                        new Dictionary<string, object?>
                        {
                            ["true"] = "Needs action today",
                            ["false"] = "Can wait",
                        }
                    ),
                }
            )
        );
        JsonAssert.Equal(
            RequestBody(capture)["questions"],
            """
            {
              "department": {
                "type": "choice",
                "instructions": "Which team?",
                "criteria": { "billing": null, "technical": null }
              },
              "is_urgent": {
                "type": "noul",
                "instructions": "Urgent?",
                "criteria": { "true": "Needs action today", "false": "Can wait" }
              }
            }
            """
        );
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static (
        Dictionary<string, DecisionQuestion> Questions,
        IReadOnlyList<DecisionModelStatePart> State
    ) Invalid(int index)
    {
        Dictionary<string, DecisionQuestion> Single(string id, DecisionQuestion question) =>
            new() { [id] = question };

        DecisionQuestion Choice(object? criteria) =>
            Q("choice", "Which team should handle this?", criteria);

        DecisionQuestion Score(List<object?> criteria) =>
            Q("score", "How frustrated is the customer?", criteria);

        switch (index)
        {
            case 0:
                return (new Dictionary<string, DecisionQuestion>(), BoxState);
            case 1:
                return (Enumerable.Range(0, 129).ToDictionary(Number, _ => IsUrgent()), BoxState);
            case 2:
                return (
                    Single(
                        "is_urgent",
                        Q(
                            "boolean",
                            null,
                            new Dictionary<string, object?>
                            {
                                ["true"] = "Urgent",
                                ["false"] = "Not urgent",
                            }
                        )
                    ),
                    BoxState
                );
            case 3:
                return (
                    Single(
                        "department",
                        Choice(new Dictionary<string, object?> { ["billing"] = null })
                    ),
                    BoxState
                );
            case 4:
                return (
                    Single(
                        "department",
                        Choice(Enumerable.Range(0, 256).ToDictionary(Number, _ => (object?)null))
                    ),
                    BoxState
                );
            case 5:
                return (Single("frustration", Score(["Calm"])), BoxState);
            case 6:
                return (
                    Single(
                        "frustration",
                        Score(Enumerable.Range(0, 11).Select(i => (object?)Number(i)).ToList())
                    ),
                    BoxState
                );
            case 7:
                return (Single("frustration", Score(["Calm", null])), BoxState);
            case 8:
                return (BoxQuestions(), DefaultState);
            default:
                return (BoxQuestions(), [.. BoxState, BoxState[1]]);
        }
    }
}
