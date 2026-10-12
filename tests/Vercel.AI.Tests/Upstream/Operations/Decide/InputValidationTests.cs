// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Operations.Decide.DecideSupport;
using DecideOperation = Vercel.AI.Operations.Decide;

namespace Vercel.AI.Tests.Upstream.Operations.Decide;

/// <summary>Port of <c>decide.test.ts</c> &gt; <c>input validation</c>.</summary>
[Collection("DownloadFetch")]
public sealed class InputValidationTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::input validation::rejects non-JSON state %s before I/O", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_non_JSON_state_before_I_O()
    {
        var cyclic = new Dictionary<string, object?>();
        cyclic["self"] = cyclic;
        object?[] states =
        {
            null,
            true,
            42,
            DecisionValues.Date(),
            new Dictionary<string, object?> { ["value"] = DecisionValues.Undefined },
            new Dictionary<string, object?> { ["value"] = DecisionValues.NotANumber },
            new Dictionary<string, object?> { ["fn"] = DecisionValues.Function },
            cyclic,
            new List<object?> { DecisionValues.Undefined },
            DecisionValues.SparseArray,
        };
        foreach (var state in states)
        {
            var model = new DecideModel();
            await Assert.ThrowsAsync<InvalidArgumentException>(() => DecideOperation.DecideAsync(new DecideRequest { Model = model, State = state, Questions = SampleQuestions() }));
            Assert.Empty(model.Calls);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::input validation::rejects invalid questions %s before I/O", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_questions_before_I_O()
    {
        var cases = new IReadOnlyDictionary<string, DecisionQuestion>?[]
        {
            new Dictionary<string, DecisionQuestion>(),
            null,
            new Dictionary<string, DecisionQuestion> { ["test"] = new DecisionQuestion("other", "test") },
            new Dictionary<string, DecisionQuestion> { ["test"] = new DecisionQuestion("boolean", null) },
            new Dictionary<string, DecisionQuestion> { ["test"] = new DecisionQuestion("boolean", "test", new Dictionary<string, object?> { ["yes"] = "yes" }) },
            new Dictionary<string, DecisionQuestion> { ["test"] = new DecisionQuestion("choice", "test", new Dictionary<string, object?>()) },
            new Dictionary<string, DecisionQuestion> { ["test"] = new DecisionQuestion("choice", "test", new Dictionary<string, object?> { ["a"] = 42 }) },
            new Dictionary<string, DecisionQuestion> { ["test"] = new DecisionQuestion("score", "test", new List<object?> { "Only one" }) },
            new Dictionary<string, DecisionQuestion> { ["test"] = new DecisionQuestion("score", "test", new List<object?> { "Low", DecisionValues.Undefined }) },
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
            State = new List<object?> { new Dictionary<string, object?> { ["type"] = "json", ["value"] = new List<object?> { shared, shared } } },
            Questions = SampleQuestions(),
        });
        Assert.Equal(3, result.Answers.Count);
    }
}
