// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Anthropic;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")))
{
    Console.WriteLine("Set ANTHROPIC_API_KEY to run this example.");
    return;
}

#region extract-anthropic-basic
var result = await Extract.ExtractAsync(new ExtractRequest
{
    Model = AnthropicProvider.Create().ExtractionModel("claude-sonnet-5-5"),
    ReferenceDate = "2026-11-11",
    State = "Invoice INV-2041 from Acme. Total due $1,250.00 by March 3 at 5:30 PM. Call Ana or Bo within 2 hours if anything is wrong.",
    Fields = new Dictionary<string, ExtractionField>
    {
        ["invoiceNumber"] = new("text", "The invoice number"),
        ["total"] = new("number", "Total amount due") { Decimals = 2 },
        ["dueDate"] = new("date", "Payment due date"),
        ["dueTime"] = new("time", "Payment due time"),
        ["callWithin"] = new("duration", "How soon to call") { Unit = "minutes" },
        ["contacts"] = new("list", "Contact names") { Items = "text" },
        ["purchaseOrder"] = new("text", "The purchase order number"),
    },
});
#endregion

foreach (var answer in result.Answers)
{
    var value = answer.Value.Text ?? answer.Value.Number?.ToString() ?? (answer.Value.TextItems == null ? "null" : string.Join(", ", answer.Value.TextItems));
    Console.WriteLine($"{answer.Key}: {answer.Value.Type} = {value}");
}

Console.WriteLine("Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out");
Console.WriteLine("Model: " + result.Response.ModelId);
