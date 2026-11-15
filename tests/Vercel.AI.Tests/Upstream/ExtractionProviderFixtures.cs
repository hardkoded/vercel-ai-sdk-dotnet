// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream;

/// <summary>Shared call and snapshot of the provider <c>*-extraction.test.ts</c> ports.</summary>
internal static class ExtractionProviderFixtures
{
    internal const string InvoiceText = "Invoice INV-2041. Total due $1,250.00 by March 3. Contacts: Ana, Bo.";

    internal static JsonObject Fixture(string name)
    {
        return (JsonObject)JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name)))!;
    }

    internal static ExtractionModelCall Call(IReadOnlyList<DecisionModelStatePart> state, IReadOnlyDictionary<string, ExtractionField> fields, string? referenceDate = null)
    {
        using var options = JsonDocument.Parse("{}");
        return new ExtractionModelCall(state, fields, options.RootElement.Clone(), new Dictionary<string, string>(), CancellationToken.None, referenceDate);
    }

    /// <summary>The invoice call every provider test sends.</summary>
    internal static ExtractionModelCall InvoiceCall()
    {
        return Call(
            [new DecisionModelTextPart(InvoiceText)],
            new Dictionary<string, ExtractionField>
            {
                ["invoiceNumber"] = new ExtractionField("text"),
                ["total"] = new ExtractionField("number") { Decimals = 2 },
                ["dueDate"] = new ExtractionField("date"),
                ["contacts"] = new ExtractionField("list") { Items = "text" },
                ["purchaseOrder"] = new ExtractionField("text"),
            },
            "2026-11-11");
    }

    /// <summary>Port of the shared inline snapshot of <c>result.answers</c>.</summary>
    internal static void AssertInvoiceAnswers(IReadOnlyDictionary<string, ExtractionAnswer> answers)
    {
        Assert.Equal(["contacts", "dueDate", "invoiceNumber", "purchaseOrder", "total"], answers.Keys.OrderBy(key => key, StringComparer.Ordinal));
        AssertAnswer(answers["contacts"], "list", textItems: ["Ana", "Bo"]);
        AssertAnswer(answers["dueDate"], "date", text: "2027-03-03");
        AssertAnswer(answers["invoiceNumber"], "text", text: "INV-2041");
        AssertAnswer(answers["purchaseOrder"], "text");
        AssertAnswer(answers["total"], "number", number: 1250);
    }

    internal static void AssertAnswer(ExtractionAnswer answer, string type, string? text = null, double? number = null, IReadOnlyList<string>? textItems = null)
    {
        Assert.Equal(type, answer.Type);
        Assert.Equal(text, answer.Text);
        Assert.Equal(number, answer.Number);
        Assert.Equal(textItems, answer.TextItems);
        Assert.Null(answer.NumberItems);
        Assert.Null(answer.GuessText);
        Assert.Null(answer.GuessNumber);
        Assert.Null(answer.PNull);
        Assert.Null(answer.Confidence);
        Assert.Null(answer.Source);
        Assert.Null(answer.Items);
    }
}
