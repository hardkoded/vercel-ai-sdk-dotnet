// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Operations.Extract.ExtractSupport;
using ExtractOperation = Vercel.AI.Operations.Extract;

namespace Vercel.AI.Tests.Upstream.Operations.Extract;

/// <summary>Port of <c>extract.test.ts</c> &gt; <c>input validation</c>.</summary>
[Collection("DownloadFetch")]
public sealed class InputValidationTests
{
    private const string UpstreamCommit = "8bc7652cdb13fc84722666eb93f45acd0d131eee";

    public static TheoryData<string> InvalidInputs()
    {
        return new TheoryData<string>
        {
            "an empty field map",
            "decimals above 6",
            "an unknown duration unit",
            "an unknown list item type",
            "a malformed reference date",
            "an impossible reference date",
        };
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    [UpstreamTest(
        "packages/ai/src/extract/extract.test.ts::input validation::rejects %s before any provider I/O",
        Coverage = UpstreamCoverage.Partial,
        Commit = UpstreamCommit,
        Note = "ExtractionField.Decimals is an int and Instructions is a JSON value, so the 'fractional decimals' (1.5) and 'non-finite instructions' ({ max: Infinity }) cases cannot be built.")]
    public async Task Rejects_before_any_provider_IO(string name)
    {
        var (input, referenceDate) = name switch
        {
            "an empty field map" => (new Dictionary<string, ExtractionField>(), (string?)null),
            "decimals above 6" => (new Dictionary<string, ExtractionField> { ["total"] = new ExtractionField("number") { Decimals = 7 } }, null),
            "an unknown duration unit" => (new Dictionary<string, ExtractionField> { ["wait"] = new ExtractionField("duration") { Unit = "fortnights" } }, null),
            "an unknown list item type" => (new Dictionary<string, ExtractionField> { ["names"] = new ExtractionField("list") { Items = "time" } }, null),
            "a malformed reference date" => (new Dictionary<string, ExtractionField> { ["name"] = new ExtractionField("text") }, "11/11/2026"),
            "an impossible reference date" => (new Dictionary<string, ExtractionField> { ["name"] = new ExtractionField("text") }, "2026-02-30"),
            _ => throw new ArgumentOutOfRangeException(nameof(name)),
        };
        var model = Setup();
        await Assert.ThrowsAsync<InvalidArgumentException>(() => ExtractOperation.ExtractAsync(new ExtractRequest { Model = model, State = State, Fields = input, ReferenceDate = referenceDate }));
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/extract/extract.test.ts::input validation::rejects state with array holes",
        Coverage = UpstreamCoverage.Partial,
        Commit = UpstreamCommit,
        Note = "A C# array has no holes. A null part stands in for the hole.")]
    public async Task Rejects_state_with_array_holes()
    {
        var model = Setup();
        var holes = new DecisionStatePart[2];
        holes[1] = new DecisionTextPart("b");
        await Assert.ThrowsAsync<InvalidArgumentException>(() => ExtractOperation.ExtractAsync(new ExtractRequest { Model = model, State = holes, Fields = Fields() }));
        Assert.Empty(model.Calls);
    }
}
