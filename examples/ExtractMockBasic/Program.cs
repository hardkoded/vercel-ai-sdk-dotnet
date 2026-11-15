// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;

var state = "Invoice INV-2041, total due $1,250.00 by March 3. Contacts: Ana, Bo.";

#region extract-mock-basic
var result = await Extract.ExtractAsync(new ExtractRequest
{
    Model = new MockExtractionModel(),
    State = state,
    ReferenceDate = "2026-11-11",
    Fields = new Dictionary<string, ExtractionField>
    {
        ["invoiceNumber"] = new("text", "The invoice number"),
        ["total"] = new("number", "Total due") { Decimals = 2 },
        ["dueDate"] = new("date", "Payment due date"),
        ["contacts"] = new("list", "Contact names") { Items = "text" },
    },
});
#endregion

foreach (var answer in result.Answers)
{
    var value = answer.Value.Text ?? answer.Value.Number?.ToString() ?? string.Join(", ", answer.Value.TextItems ?? Array.Empty<string>());
    Console.WriteLine($"{answer.Key}: {answer.Value.Type} = {value}");
}

/// <summary>An extraction model that returns fixed answers.</summary>
internal sealed class MockExtractionModel : IExtractionCaller
{
    public string Provider => "mock-provider";

    public string ModelId => "mock-model-id";

    public string SpecificationVersion => "v4";

    public IReadOnlyList<string> SupportedFieldTypes => ExtractionField.Types;

    public Task<ExtractionModelResult> DoExtractAsync(ExtractionModelCall call, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ExtractionModelResult(new Dictionary<string, ExtractionAnswer>
        {
            ["invoiceNumber"] = new("text", text: "INV-2041", source: new ExtractionSource("text", 0, 8, 16)),
            ["total"] = new("number", number: 1250),
            ["dueDate"] = new("date", text: "2027-03-03"),
            ["contacts"] = new("list", textItems: new[] { "Ana", "Bo" }),
        }));
    }
}
