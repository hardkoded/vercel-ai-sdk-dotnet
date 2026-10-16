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
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::input validation::rejects invalid questions %s before I/O", Coverage = UpstreamCoverage.Partial, Note = "Typed criteria cannot hold a missing instruction, a non-string description, an undefined level, or boolean keys other than true and false.")]
    public async Task Rejects_invalid_questions_before_I_O()
    {
        var cases = new IReadOnlyDictionary<string, DecisionQuestion>?[]
        {
            new Dictionary<string, DecisionQuestion>(),
            null,
            new Dictionary<string, DecisionQuestion> { ["test"] = Q("other", "test") },
            new Dictionary<string, DecisionQuestion> { ["test"] = Q("choice", "test", new Dictionary<string, object?>()) },
            new Dictionary<string, DecisionQuestion> { ["test"] = Q("score", "test", new List<object?> { "Only one" }) },
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
