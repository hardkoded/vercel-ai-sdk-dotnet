// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Perplexity.PerplexityDecisionModel.PerplexityDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.Perplexity.PerplexityDecisionModel;

/// <summary>Port of <c>perplexity-decision-model.test.ts</c> &gt; <c>request limits</c>.</summary>
public sealed class RequestLimitsTests
{
    private const string Prefix = "packages/perplexity/src/perplexity-decision-model.test.ts::request limits::";

    public static TheoryData<int> InvalidLimits => new() { 0, 1, 2, 3, 4, 5, 6, 7 };

    private static Dictionary<string, DecisionQuestion> Invalid(int index)
    {
        var defect = Questions()["defect"];
        switch (index)
        {
            case 0:
                return new Dictionary<string, DecisionQuestion>();
            case 1:
                return new Dictionary<string, DecisionQuestion> { [string.Empty] = defect };
            case 2:
                return Enumerable.Range(0, 129).ToDictionary(i => i.ToString(System.Globalization.CultureInfo.InvariantCulture), _ => defect);
            case 3:
                return new Dictionary<string, DecisionQuestion> { ["sentiment"] = Q("choice", "What is the sentiment?", new Dictionary<string, object?>()) };
            case 4:
                return new Dictionary<string, DecisionQuestion>
                {
                    ["sentiment"] = Q("choice", "What is the sentiment?", Enumerable.Range(0, 256).ToDictionary(i => i.ToString(System.Globalization.CultureInfo.InvariantCulture), _ => (object?)null)),
                };
            case 5:
                return new Dictionary<string, DecisionQuestion> { ["severity"] = Q("score", "How severe is the problem?", new List<object?>()) };
            case 6:
                return new Dictionary<string, DecisionQuestion> { ["severity"] = Q("score", "How severe is the problem?", new List<object?> { "Low", null }) };
            default:
                return new Dictionary<string, DecisionQuestion>
                {
                    ["severity"] = Q("score", "How severe is the problem?", Enumerable.Range(0, 11).Select(i => (object?)i.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToList()),
                };
        }
    }

    [Theory]
    [MemberData(nameof(InvalidLimits))]
    [UpstreamTest(Prefix + "rejects invalid provider limits before HTTP", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_provider_limits_before_HTTP(int index)
    {
        var (model, capture) = Setup();
        await Assert.ThrowsAsync<InvalidArgumentException>(() => model.DoDecideAsync(Call(Invalid(index))));
        Assert.Equal(0, capture.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts the maximum question, choice and score counts without splitting", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_the_maximum_question_choice_and_score_counts_without_splitting()
    {
        var defect = Questions()["defect"];
        var questions = Enumerable.Range(0, 126).ToDictionary(i => i.ToString(System.Globalization.CultureInfo.InvariantCulture), _ => defect);
        questions["choice"] = Q("choice", "Pick", Enumerable.Range(0, 255).ToDictionary(i => i.ToString(System.Globalization.CultureInfo.InvariantCulture), _ => (object?)null));
        var levels = Enumerable.Range(0, 10).Select(i => i.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToList();
        questions["score"] = Q("score", "Rate", levels.Cast<object?>().ToList());
        var answers = new JsonObject();
        foreach (var id in Enumerable.Range(0, 126))
        {
            answers[id.ToString(System.Globalization.CultureInfo.InvariantCulture)] = Fixture()["answers"]!["defect"]!.DeepClone();
        }

        var choiceProbabilities = new JsonObject();
        foreach (var id in Enumerable.Range(0, 255))
        {
            choiceProbabilities[id.ToString(System.Globalization.CultureInfo.InvariantCulture)] = id == 0 ? 1 : 0;
        }

        var scoreProbabilities = new JsonObject();
        foreach (var level in levels)
        {
            scoreProbabilities[level] = level == "0" ? 1 : 0;
        }

        answers["choice"] = new JsonObject { ["type"] = "choice", ["choice"] = "0", ["probabilities"] = choiceProbabilities };
        answers["score"] = new JsonObject { ["type"] = "score", ["score"] = 0, ["probabilities"] = scoreProbabilities };
        var (model, capture) = Setup(new JsonObject { ["answers"] = answers });
        await model.DoDecideAsync(Call(questions));
        Assert.Equal(1, capture.Calls);
        Assert.Equal(128, RequestBody(capture)["questions"]!.AsObject().Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects box questions on direct provider calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_box_questions_on_direct_provider_calls()
    {
        var (model, capture) = Setup();
        await Assert.ThrowsAsync<DecisionUnsupportedQuestionTypeException>(() => model.DoDecideAsync(Call(new Dictionary<string, DecisionQuestion> { ["box"] = Q("box", "Find the product") })));
        Assert.Equal(0, capture.Calls);
    }
}
