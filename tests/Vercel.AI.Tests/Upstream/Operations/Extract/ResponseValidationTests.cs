// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using static Vercel.AI.Tests.Upstream.Operations.Extract.ExtractSupport;
using ExtractOperation = Vercel.AI.Operations.Extract;

namespace Vercel.AI.Tests.Upstream.Operations.Extract;

/// <summary>Port of <c>extract.test.ts</c> &gt; <c>response validation</c>.</summary>
[Collection("DownloadFetch")]
public sealed class ResponseValidationTests
{
    private const string UpstreamCommit = "8bc7652cdb13fc84722666eb93f45acd0d131eee";

    public static TheoryData<string> InvalidAnswers()
    {
        return new TheoryData<string>
        {
            "a missing answer",
            "an extra answer",
            "a wrong answer type",
            "a string number",
            "a non-finite number",
            "too many decimals",
            "a malformed date",
            "an impossible date",
            "an impossible time",
            "a datetime without a time",
            "a datetime with an impossible date",
            "a negative duration",
            "a list with a wrong item type",
            "a list that is not an array",
            "list item entries that do not match the values",
            "a list with holes",
            "a list with a hole masked by an enumerable property",
            "list item entries with a hole masked by an enumerable property",
            "list item entries without values",
            "a pNull above 1",
            "a negative confidence",
            "an invalid guess",
            "a source past the end of the text",
            "an empty source span",
            "a source part outside the state",
            "a file source on a text part",
        };
    }

    [Theory]
    [MemberData(nameof(InvalidAnswers))]
    [UpstreamTest(
        "packages/ai/src/extract/extract.test.ts::response validation::rejects %s",
        Coverage = UpstreamCoverage.Partial,
        Commit = UpstreamCommit,
        Note = "ExtractionAnswer is typed. 'an undefined value' cannot be told apart from a null value, and 'a sparse list with a huge length' and 'sparse list item entries with a huge length' cannot be built. A C# list has no holes, so a null entry stands in for a hole, and a text list carrying number values stands in for the mixed ['Ana', 2] list.")]
    public async Task Rejects(string name)
    {
        var merged = Answers();
        foreach (var pair in Override(name))
        {
            if (pair.Value is null)
            {
                merged.Remove(pair.Key);
            }
            else
            {
                merged[pair.Key] = pair.Value;
            }
        }

        var model = Setup(new ExtractionModelResult(merged, []));
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => ExtractOperation.ExtractAsync(new ExtractRequest { Model = model, State = State, Fields = Fields() }));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::response validation::enforces integer fields and list item number rules", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Enforces_integer_fields_and_list_item_number_rules()
    {
        Dictionary<string, ExtractionField> IntegerFields() => new()
        {
            ["count"] = new ExtractionField("number") { Integer = true },
            ["scores"] = new ExtractionField("list") { Items = "number", Decimals = 1 },
        };

        Task<ExtractionResult> Run(double count, double[] scores) => ExtractOperation.ExtractAsync(new ExtractRequest
        {
            Model = Setup(new ExtractionModelResult(
                new Dictionary<string, ExtractionAnswer>
                {
                    ["count"] = new ExtractionAnswer("number", number: count),
                    ["scores"] = new ExtractionAnswer("list", numberItems: scores),
                },
                [])),
            State = State,
            Fields = IntegerFields(),
        });

        Assert.NotNull(await Run(3, [1.5, 2]));
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => Run(3.5, [1.5]));
        await Assert.ThrowsAsync<InvalidResponseDataException>(() => Run(3, [1.55]));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::response validation::accepts datetimes with seconds, fractions, and offsets", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Accepts_datetimes_with_seconds_fractions_and_offsets()
    {
        foreach (var value in new[] { "2027-03-03T17:30:00Z", "2027-03-03T17:30:00.123+01:00", "2027-03-03T17:30-08:00" })
        {
            var result = await ExtractOperation.ExtractAsync(new ExtractRequest
            {
                Model = Setup(new ExtractionModelResult(new Dictionary<string, ExtractionAnswer> { ["dueAt"] = new ExtractionAnswer("datetime", text: value) }, [])),
                State = State,
                Fields = new Dictionary<string, ExtractionField> { ["dueAt"] = Fields()["dueAt"] },
            });
            Assert.NotNull(result);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::response validation::accepts sources that point into later text parts and file parts", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Accepts_sources_that_point_into_later_text_parts_and_file_parts()
    {
        var bytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var model = Setup(new ExtractionModelResult(
            new Dictionary<string, ExtractionAnswer>
            {
                ["invoiceNumber"] = new ExtractionAnswer("text", text: "INV-2041", source: new ExtractionSource("text", 2, 0, 8)),
                ["total"] = new ExtractionAnswer("number", number: 1250, source: new ExtractionSource("file", 1)),
            },
            []));
        var fields = Fields();
        var result = await ExtractOperation.ExtractAsync(new ExtractRequest
        {
            Model = model,
            State = new DecisionStatePart[]
            {
                new DecisionTextPart("Header"),
                new DecisionFilePart("application/pdf", FilePartInput.FromBytes(bytes)),
                new DecisionTextPart("INV-2041"),
            },
            Fields = new Dictionary<string, ExtractionField> { ["invoiceNumber"] = fields["invoiceNumber"], ["total"] = fields["total"] },
        });
        Assert.NotNull(result);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::response validation::handles field ids that match object prototype properties", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Handles_field_ids_that_match_object_prototype_properties()
    {
        var specialFields = new Dictionary<string, ExtractionField>
        {
            ["__proto__"] = new ExtractionField("text"),
            ["constructor"] = new ExtractionField("number"),
        };
        var specialAnswers = new Dictionary<string, ExtractionAnswer>
        {
            ["__proto__"] = new ExtractionAnswer("text", text: "x"),
            ["constructor"] = new ExtractionAnswer("number", number: 1),
        };
        var result = await ExtractOperation.ExtractAsync(new ExtractRequest
        {
            Model = Setup(new ExtractionModelResult(specialAnswers, [])),
            State = State,
            Fields = specialFields,
        });
        Assert.Equal(new[] { "__proto__", "constructor" }, result.Answers.Keys.ToArray());
    }

    // Upstream overrides merged over `answers`. A null entry deletes the answer, like upstream's `undefined`.
    private static Dictionary<string, ExtractionAnswer?> Override(string name)
    {
        var invoiceNumber = (ExtractionSource source) => new ExtractionAnswer("text", text: "INV-2041", source: source);
        return name switch
        {
            "a missing answer" => new() { ["total"] = null },
            "an extra answer" => new() { ["extra"] = new ExtractionAnswer("text", text: "x") },
            "a wrong answer type" => new() { ["total"] = new ExtractionAnswer("text", text: "1250") },
            "a string number" => new() { ["total"] = new ExtractionAnswer("number", text: "1250") },
            "a non-finite number" => new() { ["total"] = new ExtractionAnswer("number", number: double.PositiveInfinity) },
            "too many decimals" => new() { ["total"] = new ExtractionAnswer("number", number: 1250.005) },
            "a malformed date" => new() { ["dueDate"] = new ExtractionAnswer("date", text: "03/03/2027") },
            "an impossible date" => new() { ["dueDate"] = new ExtractionAnswer("date", text: "2027-02-29") },
            "an impossible time" => new() { ["dueTime"] = new ExtractionAnswer("time", text: "24:00") },
            "a datetime without a time" => new() { ["dueAt"] = new ExtractionAnswer("datetime", text: "2027-03-03") },
            "a datetime with an impossible date" => new() { ["dueAt"] = new ExtractionAnswer("datetime", text: "2027-13-03T17:30Z") },
            "a negative duration" => new() { ["callWithin"] = new ExtractionAnswer("duration", number: -2) },
            "a list with a wrong item type" => new() { ["contacts"] = new ExtractionAnswer("list", numberItems: [2]) },
            "a list that is not an array" => new() { ["contacts"] = new ExtractionAnswer("list", text: "Ana, Bo") },
            "list item entries that do not match the values" => new() { ["contacts"] = new ExtractionAnswer("list", textItems: ["Ana"], items: [new ExtractionItem(), new ExtractionItem()]) },
            "a list with holes" => new() { ["contacts"] = new ExtractionAnswer("list", textItems: ["Ana", null!]) },
            "a list with a hole masked by an enumerable property" => new() { ["contacts"] = new ExtractionAnswer("list", textItems: [null!]) },
            "list item entries with a hole masked by an enumerable property" => new() { ["contacts"] = new ExtractionAnswer("list", textItems: ["Ana"], items: [null!]) },
            "list item entries without values" => new() { ["contacts"] = new ExtractionAnswer("list", items: []) },
            "a pNull above 1" => new() { ["total"] = new ExtractionAnswer("number", number: 1250, pNull: 1.5) },
            "a negative confidence" => new() { ["total"] = new ExtractionAnswer("number", number: 1250, confidence: -0.1) },
            "an invalid guess" => new() { ["total"] = new ExtractionAnswer("number", guessText: "x") },
            "a source past the end of the text" => new() { ["invoiceNumber"] = invoiceNumber(new ExtractionSource("text", 0, 8, State.Length + 1)) },
            "an empty source span" => new() { ["invoiceNumber"] = invoiceNumber(new ExtractionSource("text", 0, 8, 8)) },
            "a source part outside the state" => new() { ["invoiceNumber"] = invoiceNumber(new ExtractionSource("text", 1, 0, 1)) },
            "a file source on a text part" => new() { ["invoiceNumber"] = invoiceNumber(new ExtractionSource("file", 0)) },
            _ => throw new ArgumentOutOfRangeException(nameof(name)),
        };
    }
}
