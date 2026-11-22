// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Parallel.ParallelDecisionModel.ParallelDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.Parallel.ParallelDecisionModel;

/// <summary>Port of <c>parallel-decision-model.test.ts</c> &gt; <c>request limits</c>.</summary>
public sealed class RequestLimitsTests
{
    private const string UpstreamId =
        "packages/parallel/src/parallel-decision-model.test.ts::request limits > ";

    public static IEnumerable<object[]> InvalidQuestions()
    {
        yield return new object[] { new Dictionary<string, DecisionQuestion>() };
        yield return new object[]
        {
            Enumerable.Range(0, 101).ToDictionary(i => "q" + i, _ => HasSearch()),
        };
        yield return new object[]
        {
            new Dictionary<string, DecisionQuestion> { ["bad name"] = HasSearch() },
        };
        yield return new object[]
        {
            new Dictionary<string, DecisionQuestion>
            {
                ["choice"] = Availability(
                    new Dictionary<string, object?> { ["only"] = "One option" }
                ),
            },
        };
        yield return new object[]
        {
            new Dictionary<string, DecisionQuestion>
            {
                ["choice"] = Availability(
                    Enumerable.Range(0, 256).ToDictionary(i => "c" + i, _ => (object?)"Option")
                ),
            },
        };
        yield return new object[]
        {
            new Dictionary<string, DecisionQuestion>
            {
                ["score"] = Fit(new List<object?> { "One level" }),
            },
        };
        yield return new object[]
        {
            new Dictionary<string, DecisionQuestion>
            {
                ["score"] = Fit(Enumerable.Repeat((object?)"Level", 11).ToList()),
            },
        };
    }

    [Theory]
    [MemberData(nameof(InvalidQuestions))]
    [UpstreamTest(
        UpstreamId + "rejects invalid questions before I/O: %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = "6a648d45fffeadc101797685fb76e8b0c19e639a"
    )]
    public async Task Rejects_invalid_questions_before_IO(
        Dictionary<string, DecisionQuestion> questions
    )
    {
        var server = new ParallelServer();
        await Assert.ThrowsAsync<InvalidArgumentError>(() =>
            CreateModel(server).DoDecideAsync(Call(questions), CancellationToken.None)
        );
        Assert.Empty(server.Calls);
    }
}
