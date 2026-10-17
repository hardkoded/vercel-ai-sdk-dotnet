// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Operations.Decide.DecideSupport;
using DecideOperation = Vercel.AI.Operations.Decide;

namespace Vercel.AI.Tests.Upstream.Operations.Decide;

/// <summary>Port of <c>decide.test.ts</c> &gt; <c>input validation</c>.</summary>
[Collection("DownloadFetch")]
public sealed class InputValidationTests
{
    public static TheoryData<string> InstructionFields()
    {
        return new TheoryData<string> { "{}", "{\"instructions\":null}" };
    }

    public static TheoryData<string> BooleanCriterion()
    {
        return new TheoryData<string> { "{\"true\":[\"Refund requested\"]}", "{\"false\":{\"intent\":\"Status inquiry\"}}" };
    }

    [Theory]
    [MemberData(nameof(InstructionFields))]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::input validation::accepts criteria-only questions with %j and forwards them unchanged", Coverage = UpstreamCoverage.Partial, Note = "A missing, undefined, and null instructions field are all a null instruction here, so the undefined case shares the {} case.")]
    public async Task Accepts_criteria_only_questions_and_forwards_them_unchanged(string instructionFields)
    {
        _ = instructionFields;
        var model = new DecideModel();
        var criteriaOnly = new Dictionary<string, DecisionQuestion>
        {
            ["topic"] = Q("choice", null, new Dictionary<string, object?> { ["billing"] = null, ["support"] = null }),
            ["severity"] = Q("score", null, new List<object?> { "Low", "Medium", "High" }),
            ["refund"] = Q("boolean", null, new Dictionary<string, object?> { ["true"] = "Money back", ["false"] = null }),
        };
        var result = await DecideOperation.DecideAsync(new DecideRequest { Model = model, State = "text", Questions = criteriaOnly });
        Assert.Equal(3, result.Answers.Count);
        Assert.Same(criteriaOnly, model.Calls[0].Questions);
    }

    [Theory]
    [MemberData(nameof(BooleanCriterion))]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::input validation::accepts a single Boolean criterion: %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_single_Boolean_criterion(string criteriaJson)
    {
        var criteria = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(criteriaJson)!;
        var model = new DecideModel();
        model.Result = new DecisionModelResult(new Dictionary<string, DecisionAnswer> { ["refund"] = SampleAnswers()["refund"] });
        var result = await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = model,
            State = "text",
            Questions = new Dictionary<string, DecisionQuestion> { ["refund"] = Q("boolean", null, criteria) },
        });
        Assert.NotNull(result);
    }

    [Theory]
    [MemberData(nameof(InstructionFields))]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::input validation::rejects Boolean questions without instructions or a described criterion: %j", Coverage = UpstreamCoverage.Partial, Note = "A missing, undefined, and null instructions field are all a null instruction here, so the undefined case shares the {} case.")]
    public async Task Rejects_Boolean_questions_without_instructions_or_a_described_criterion(string instructionFields)
    {
        _ = instructionFields;
        var model = new DecideModel();
        var cases = new object?[]
        {
            null,
            new Dictionary<string, object?>(),
            new Dictionary<string, object?> { ["true"] = null },
            new Dictionary<string, object?> { ["false"] = null },
            new Dictionary<string, object?> { ["true"] = null, ["false"] = null },
        };
        foreach (var criteria in cases)
        {
            await Assert.ThrowsAsync<InvalidArgumentException>(() => DecideOperation.DecideAsync(new DecideRequest
            {
                Model = model,
                State = "text",
                Questions = new Dictionary<string, DecisionQuestion> { ["refund"] = Q("boolean", null, criteria) },
            }));
        }

        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::input validation::rejects non-JSON state %s before I/O", Coverage = UpstreamCoverage.Partial, Note = "JsonElement holds only JSON. A missing state, true, and 42 are rejected. A Date, undefined, NaN, a function, a cycle, and a sparse array cannot be built.")]
    public async Task Rejects_non_JSON_state_before_I_O()
    {
        var model = new DecideModel();
        await Assert.ThrowsAsync<InvalidArgumentException>(() => DecideOperation.DecideAsync(new DecideRequest { Model = model, State = null, Questions = SampleQuestions() }));
        Assert.Throws<InvalidArgumentException>(() => DecisionState.FromJson(JsonSerializer.SerializeToElement(true)));
        Assert.Throws<InvalidArgumentException>(() => DecisionState.FromJson(JsonSerializer.SerializeToElement(42)));
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::input validation::rejects invalid questions %s before I/O", Coverage = UpstreamCoverage.Partial, Note = "Typed criteria and instructions cannot hold a non-string, non-object, non-array instruction, a non-string description, an undefined level, or boolean keys other than true and false. A box without instructions stands in for the missing and null box instructions.")]
    public async Task Rejects_invalid_questions_before_I_O()
    {
        var cases = new IReadOnlyDictionary<string, DecisionQuestion>?[]
        {
            new Dictionary<string, DecisionQuestion>(),
            null,
            new Dictionary<string, DecisionQuestion> { ["test"] = Q("other", "test") },
            new Dictionary<string, DecisionQuestion> { ["test"] = Q("choice", "test", new Dictionary<string, object?>()) },
            new Dictionary<string, DecisionQuestion> { ["test"] = Q("box", null) },
            new Dictionary<string, DecisionQuestion> { ["test"] = Q("score", "test", new List<object?>()) },
        };
        foreach (var questions in cases)
        {
            var model = new DecideModel();
            await Assert.ThrowsAsync<InvalidArgumentException>(() => DecideOperation.DecideAsync(new DecideRequest { Model = model, State = "text", Questions = questions }));
            Assert.Empty(model.Calls);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::input validation::allows repeated JSON references without treating them as cycles", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_repeated_JSON_references_without_treating_them_as_cycles()
    {
        var shared = new Dictionary<string, object?> { ["text"] = "hello" };
        var result = await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel(),
            State = S(new List<object?> { new Dictionary<string, object?> { ["type"] = "json", ["value"] = new List<object?> { shared, shared } } }),
            Questions = SampleQuestions(),
        });
        Assert.Equal(3, result.Answers.Count);
    }
}
