// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Operations.Decide.DecideSupport;
using DecideOperation = Vercel.AI.Operations.Decide;

namespace Vercel.AI.Tests.Upstream.Operations.Decide;

/// <summary>Port of <c>decide.test.ts</c> &gt; <c>response validation</c>.</summary>
[Collection("DownloadFetch")]
public sealed class ResponseValidationTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::response validation::accepts native rounded scores without rewriting probabilities or scores", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_native_rounded_scores_without_rewriting_probabilities_or_scores()
    {
        var answers = SampleAnswers();
        answers["severity"] = new DecisionAnswer("score", score: 0.97, probabilities: new Dictionary<string, double> { ["0"] = 0.13, ["1"] = 0.76, ["2"] = 0.11 });
        var rounding = new DecisionRounding(2, 2);
        var result = await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel { Result = new DecisionModelResult(answers, rounding: rounding) },
            State = "text",
            Questions = SampleQuestions(),
        });
        Assert.Equal((double?)0.97, result.Answers["severity"].Score);
        Assert.Equal(0.13, result.Answers["severity"].Probabilities!["0"]);
        Assert.Equal((int?)2, result.Rounding!.ScoreDecimals);
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel { Result = new DecisionModelResult(answers) },
            State = "text",
            Questions = SampleQuestions(),
        }));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::response validation::accepts a rounded distribution sum but rejects errors beyond declared precision", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_rounded_distribution_sum_but_rejects_errors_beyond_declared_precision()
    {
        var answers = SampleAnswers();
        answers["severity"] = new DecisionAnswer("score", score: 1, probabilities: new Dictionary<string, double> { ["0"] = 0.33, ["1"] = 0.33, ["2"] = 0.33 });
        var rounding = new DecisionRounding(2, 2);
        var result = await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel { Result = new DecisionModelResult(answers, rounding: rounding) },
            State = "text",
            Questions = SampleQuestions(),
        });
        Assert.Equal((double?)1, result.Answers["severity"].Score);
        var drifted = SampleAnswers();
        drifted["severity"] = new DecisionAnswer("score", score: 1.5, probabilities: answers["severity"].Probabilities);
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel { Result = new DecisionModelResult(drifted, rounding: rounding) },
            State = "text",
            Questions = SampleQuestions(),
        }));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::response validation::rejects invalid rounding precision %s", Coverage = UpstreamCoverage.Partial, Note = "NaN and 1.5 are JavaScript numbers; DecisionRounding stores integers, so -1 and 16 are the representable cases.")]
    public async Task Rejects_invalid_rounding_precision()
    {
        foreach (var decimals in new int?[] { -1, 16 })
        {
            await Assert.ThrowsAsync<InvalidResponseDataException>(() => DecideOperation.DecideAsync(new DecideRequest
            {
                Model = new DecideModel { Result = new DecisionModelResult(SampleAnswers(), rounding: new DecisionRounding(decimals)) },
                State = "text",
                Questions = SampleQuestions(),
            }));
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::response validation::rejects malformed answers %s without retrying", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_malformed_answers_without_retrying()
    {
        var cases = new IReadOnlyDictionary<string, DecisionAnswer>?[]
        {
            null,
            new Dictionary<string, DecisionAnswer>(),
            With("extra", new DecisionAnswer("boolean", probability: 0.92)),
            With("topic", new DecisionAnswer("score", score: 1)),
            With("topic", new DecisionAnswer("choice", choice: "other")),
            With("topic", new DecisionAnswer("choice", choice: "toString")),
            With("topic", new DecisionAnswer("choice", choice: "billing", probabilities: new Dictionary<string, double> { ["billing"] = 1 })),
            With("topic", new DecisionAnswer("choice", choice: "billing", probabilities: new Dictionary<string, double> { ["billing"] = 0.1, ["support"] = 0.9 })),
            With("severity", new DecisionAnswer("score", score: 3)),
            With("severity", new DecisionAnswer("score", score: double.NaN)),
            With("severity", new DecisionAnswer("score", score: 1, probabilities: new Dictionary<string, double> { ["0"] = 0, ["1"] = 0, ["2"] = 1 })),
            With("severity", new DecisionAnswer("score", score: 1, probabilities: new Dictionary<string, double> { ["0"] = 0.2, ["1"] = 0.2, ["2"] = 0.2 })),
            With("severity", new DecisionAnswer("score", score: 1, probabilities: new Dictionary<string, double> { ["0"] = -0.1, ["1"] = 1.1, ["2"] = 0 })),
            With("severity", new DecisionAnswer("score", score: 1, probabilities: new Dictionary<string, double> { ["0"] = double.NaN, ["1"] = 1, ["2"] = 0 })),
            With("refund", new DecisionAnswer("boolean")),
            With("refund", new DecisionAnswer("boolean", probability: null)),
            With("refund", new DecisionAnswer("boolean", probability: double.PositiveInfinity)),
            With("refund", new DecisionAnswer("boolean", probability: 1.1)),
        };
        foreach (var answers in cases)
        {
            var model = new DecideModel { Result = new DecisionModelResult(answers ?? new Dictionary<string, DecisionAnswer>()) };
            if (answers == null)
            {
                model.Result = new DecisionModelResult(new Dictionary<string, DecisionAnswer>());
                model.ReturnNullAnswers = true;
            }

            await Assert.ThrowsAsync<InvalidResponseDataException>(() => DecideOperation.DecideAsync(new DecideRequest { Model = model, State = "text", Questions = SampleQuestions(), MaxRetries = 2 }));
            Assert.Single(model.Calls);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::response validation::accepts a selected choice within the declared probability rounding", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_selected_choice_within_the_declared_probability_rounding()
    {
        var questions = SampleQuestions();
        questions["topic"] = new DecisionQuestion("choice", questions["topic"].Instructions, new Dictionary<string, object?> { ["billing"] = null, ["support"] = null, ["other"] = null });
        var roundedAnswers = SampleAnswers();
        roundedAnswers["topic"] = new DecisionAnswer("choice", choice: "support", probabilities: new Dictionary<string, double> { ["billing"] = 0.44, ["support"] = 0.43, ["other"] = 0.13 });
        var result = await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel { Result = new DecisionModelResult(roundedAnswers, Array.Empty<OperationWarning>(), rounding: new DecisionRounding(2)) },
            State = "text",
            Questions = questions,
        });
        Assert.Equal(roundedAnswers.Keys, result.Answers.Keys);
        Assert.Equal("support", result.Answers["topic"].Choice);
        Assert.Equal(0.44, result.Answers["topic"].Probabilities!["billing"]);
        Assert.Equal(0.43, result.Answers["topic"].Probabilities!["support"]);
        Assert.Equal(0.13, result.Answers["topic"].Probabilities!["other"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::response validation::accepts rounding within tolerance without modifying the distribution", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_rounding_within_tolerance_without_modifying_the_distribution()
    {
        var probabilities = new Dictionary<string, double> { ["0"] = 0.3333333, ["1"] = 0.3333333, ["2"] = 0.3333333 };
        var answers = SampleAnswers();
        answers["severity"] = new DecisionAnswer("score", score: 1, probabilities: probabilities);
        var result = await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel { Result = new DecisionModelResult(answers) },
            State = "text",
            Questions = SampleQuestions(),
        });
        Assert.Equal(0.3333333, result.Answers["severity"].Probabilities!["0"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::response validation::handles question and option keys that match object prototype properties", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_question_and_option_keys_that_match_object_prototype_properties()
    {
        var questions = new Dictionary<string, DecisionQuestion>
        {
            ["__proto__"] = new DecisionQuestion("choice", "Which?", new Dictionary<string, object?> { ["__proto__"] = null, ["constructor"] = null }),
        };
        var answers = new Dictionary<string, DecisionAnswer>
        {
            ["__proto__"] = new DecisionAnswer("choice", choice: "__proto__"),
        };
        var result = await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel { Result = new DecisionModelResult(answers) },
            State = "text",
            Questions = questions,
        });
        Assert.Equal(new[] { "__proto__" }, result.Answers.Keys.ToArray());
    }
}
