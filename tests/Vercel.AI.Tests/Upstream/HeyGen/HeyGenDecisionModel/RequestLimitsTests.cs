// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.HeyGen.HeyGenDecisionModel.HeyGenDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.HeyGen.HeyGenDecisionModel;

/// <summary>Port of <c>heygen-decision-model.test.ts</c> &gt; <c>request limits</c>.</summary>
public sealed class RequestLimitsTests
{
    private const string Prefix =
        "packages/heygen/src/heygen-decision-model.test.ts::request limits::";

    public static TheoryData<int> InvalidLimits => new() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static Dictionary<string, DecisionQuestion> Invalid(int index)
    {
        var blocked = Blocked();
        var team = Team();
        var impact = Impact();
        switch (index)
        {
            case 0:
                return new Dictionary<string, DecisionQuestion>();
            case 1:
                return new Dictionary<string, DecisionQuestion> { [string.Empty] = blocked };
            case 2:
                return new Dictionary<string, DecisionQuestion> { [" padded"] = blocked };
            case 3:
                return new Dictionary<string, DecisionQuestion> { ["a:b"] = blocked };
            case 4:
                return new Dictionary<string, DecisionQuestion> { ["a\nb"] = blocked };
            case 5:
                return Enumerable.Range(0, 401).ToDictionary(Number, _ => blocked);
            case 6:
                return new Dictionary<string, DecisionQuestion>
                {
                    ["team"] = Q(
                        "choice",
                        "Which team should handle this request?",
                        new Dictionary<string, object?>()
                    ),
                };
            case 7:
                return new Dictionary<string, DecisionQuestion>
                {
                    ["team"] = Q(
                        "choice",
                        "Which team should handle this request?",
                        Enumerable.Range(0, 256).ToDictionary(Number, _ => (object?)null)
                    ),
                };
            case 8:
                return new Dictionary<string, DecisionQuestion>
                {
                    ["impact"] = Q(
                        "score",
                        "How much does this affect the customer?",
                        new List<object?>()
                    ),
                };
            case 9:
                return new Dictionary<string, DecisionQuestion>
                {
                    ["impact"] = Q(
                        "score",
                        "How much does this affect the customer?",
                        new List<object?> { "Low", null }
                    ),
                };
            default:
                return new Dictionary<string, DecisionQuestion>
                {
                    ["impact"] = Q(
                        "score",
                        "How much does this affect the customer?",
                        Enumerable.Range(0, 11).Select(i => (object?)Number(i)).ToList()
                    ),
                };
        }
    }

    [Theory]
    [MemberData(nameof(InvalidLimits))]
    [UpstreamTest(
        Prefix + "rejects invalid provider limits before HTTP",
        Coverage = UpstreamCoverage.Covered,
        Commit = "585caa76cd56696750626fa9f3f3f728bc6d8d03"
    )]
    public async Task Rejects_invalid_provider_limits_before_HTTP(int index)
    {
        var (_, model, capture) = Setup();
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            model.DoDecideAsync(Call(Invalid(index)))
        );
        Assert.Equal(0, capture.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts the maximum question count and a single-level score",
        Coverage = UpstreamCoverage.Covered,
        Commit = "585caa76cd56696750626fa9f3f3f728bc6d8d03"
    )]
    public async Task Accepts_the_maximum_question_count_and_a_single_level_score()
    {
        var blocked = Blocked();
        var questions = Enumerable.Range(0, 399).ToDictionary(Number, _ => blocked);
        questions["single"] = Q("score", "Rate", new List<object?> { "Only" });
        var answers = new JsonObject();
        foreach (var id in Enumerable.Range(0, 399))
        {
            answers[Number(id)] = Fixture()["data"]!["answers"]!["blocked"]!.DeepClone();
        }

        answers["single"] = new JsonObject
        {
            ["type"] = "score",
            ["score"] = 0,
            ["probabilities"] = new JsonObject { ["0"] = 1 },
        };
        var (_, model, capture) = Setup(
            new JsonObject { ["data"] = new JsonObject { ["answers"] = answers } }
        );
        await model.DoDecideAsync(Call(questions));
        Assert.Equal(1, capture.Calls);
        Assert.Equal(400, RequestBody(capture)["questions"]!.AsObject().Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects box questions",
        Coverage = UpstreamCoverage.Covered,
        Commit = "585caa76cd56696750626fa9f3f3f728bc6d8d03"
    )]
    public async Task Rejects_box_questions()
    {
        var (_, model, capture) = Setup();
        await Assert.ThrowsAsync<DecisionUnsupportedQuestionTypeException>(() =>
            model.DoDecideAsync(
                Call(
                    new Dictionary<string, DecisionQuestion>
                    {
                        ["box"] = Q("box", "Find the product"),
                    }
                )
            )
        );
        Assert.Equal(0, capture.Calls);
    }
}
