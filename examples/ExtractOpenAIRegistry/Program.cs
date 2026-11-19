// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY")))
{
    Console.WriteLine("Set OPENAI_API_KEY to run this example.");
    return;
}

#region extract-openai-registry
var openai = OpenAIProvider.Create();
var registry = new ProviderRegistry();
registry.Register(
    "invoices",
    new CustomProvider { FallbackProvider = openai }
        .AddExtractionModel("fast", openai.ExtractionModel("gpt-6.1-sol"))
        .AddExtractionModel("careful", openai.ExtractionModel("gpt-6-luna"))
);

DecisionModels.DefaultProvider = new CustomProvider().AddExtractionModel(
    "careful",
    registry.ExtractionModel("invoices:careful")
);

var fields = new Dictionary<string, ExtractionField>
{
    ["invoiceNumber"] = new("text"),
    ["total"] = new("number") { Decimals = 2 },
};
const string state = "Invoice INV-2041, total due $1,250.00 by March 3.";

foreach (
    var request in new[]
    {
        new ExtractRequest { Model = registry.ExtractionModel("invoices:fast") },
        new ExtractRequest { Model = registry.ExtractionModel("invoices:gpt-6.1-sol") },
        new ExtractRequest { ModelId = "careful" },
    }
)
{
    request.State = state;
    request.Fields = fields;
    var result = await Extract.ExtractAsync(request);
    Console.WriteLine(result.Response.ModelId + " " + result.Answers["invoiceNumber"].Text);
}
#endregion
