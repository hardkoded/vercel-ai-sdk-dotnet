// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.ScaleDown;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SCALEDOWN_API_KEY")))
{
    Console.WriteLine("Set SCALEDOWN_API_KEY to run this example.");
    return;
}

#region extract-scaledown-basic
var result = await Extract.ExtractAsync(new ExtractRequest
{
    Model = ScaleDownProvider.Default.ExtractionModel("extract-1"),
    State = "Invoice INV-2041 from Acme Corp. Contacts: Ana, Bo.",
    Fields = new Dictionary<string, ExtractionField>
    {
        ["invoiceNumber"] = new("text", DecisionInput.From("The invoice number")),
        ["vendor"] = new("text", DecisionInput.From("Company that issued the invoice")),
        ["contacts"] = new("list", DecisionInput.From("Contact names")) { Items = "text" },
    },
});
#endregion

foreach (var answer in result.Answers)
{
    var value = answer.Value.Text ?? (answer.Value.TextItems == null ? "null" : string.Join(", ", answer.Value.TextItems));
    Console.WriteLine($"{answer.Key}: {answer.Value.Type} = {value}");
}

Console.WriteLine("Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out");
