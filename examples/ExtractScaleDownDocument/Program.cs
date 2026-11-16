// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.ScaleDown;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SCALEDOWN_API_KEY")))
{
    Console.WriteLine("Set SCALEDOWN_API_KEY to run this example.");
    return;
}

var path = args.Length > 0 ? args[0] : "./data/ai.pdf";
if (!File.Exists(path))
{
    Console.WriteLine("Pass the path of a PDF file to run this example.");
    return;
}

#region extract-scaledown-document
var result = await Extract.ExtractAsync(
    new ExtractRequest
    {
        Model = ScaleDownProvider.Default.ExtractionModel("extract-1"),
        State = DecisionState.From([
            new DecisionFilePart(
                "application/pdf",
                FilePartInput.FromBytes(File.ReadAllBytes(path))
            ),
        ]),
        Fields = new Dictionary<string, ExtractionField>
        {
            ["title"] = new("text", DecisionInput.From("The document title")),
            ["topics"] = new("list", DecisionInput.From("Named AI techniques")) { Items = "text" },
        },
        ProviderOptions = JsonDocument
            .Parse("""{"scaledown":{"threshold":0.5}}""")
            .RootElement.Clone(),
    }
);
#endregion

foreach (var answer in result.Answers)
{
    var value = answer.Value.Text
        ?? (answer.Value.TextItems == null ? "null" : string.Join(", ", answer.Value.TextItems));
    Console.WriteLine($"{answer.Key}: {answer.Value.Type} = {value}");
}

Console.WriteLine(
    "OCR text: " + result.ProviderMetadata?.GetProperty("scaledown").GetProperty("ocrText")
);
