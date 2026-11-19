// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Liquid;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LIQUID_API_KEY")))
{
    Console.WriteLine("Set LIQUID_API_KEY to run this example.");
    return;
}

var path = args.Length > 0 ? args[0] : "data/comic-cat.png";
if (!File.Exists(path))
{
    Console.WriteLine("Pass the path of a PNG image to run this example.");
    return;
}

#region decide-liquid-image
var result = await Decide.DecideAsync(
    new DecideRequest
    {
        Model = LiquidProvider.Create().DecisionModel("d1.2"),
        State = DecisionState.From([
            new DecisionTextPart("An illustration submitted for a picture book."),
            new DecisionFilePart("image/png", FilePartInput.FromBytes(File.ReadAllBytes(path))),
        ]),
        Questions = new Dictionary<string, DecisionQuestion>
        {
            ["animal"] = new(
                "choice",
                "Which animal is shown in the illustration?",
                new DecisionChoiceCriteria(
                    new Dictionary<string, DecisionInput?>
                    {
                        ["cat"] = null,
                        ["dog"] = null,
                        ["bird"] = null,
                    }
                )
            ),
            ["cat"] = new("box", "The cat."),
        },
    }
);
#endregion

Console.WriteLine("Answers: " + JsonSerializer.Serialize(result.Answers));
Console.WriteLine(
    "Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out"
);
Console.WriteLine("Provider metadata: " + result.ProviderMetadata);
