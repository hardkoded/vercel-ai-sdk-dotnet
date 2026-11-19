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

var path = args.Length > 0 ? args[0] : "data/galileo.mp3";
if (!File.Exists(path))
{
    Console.WriteLine("Pass the path of an MP3 file to run this example.");
    return;
}

#region decide-liquid-audio
var result = await Decide.DecideAsync(
    new DecideRequest
    {
        Model = LiquidProvider.Create().DecisionModel("d1.2"),
        State = DecisionState.From([
            new DecisionFilePart("audio/mpeg", FilePartInput.FromBytes(File.ReadAllBytes(path))),
        ]),
        Questions = new Dictionary<string, DecisionQuestion>
        {
            ["science"] = new("boolean", "Does the speaker talk about science or a scientist?"),
            ["language"] = new(
                "choice",
                "Which language does the speaker use?",
                new DecisionChoiceCriteria(
                    new Dictionary<string, DecisionInput?>
                    {
                        ["english"] = "English",
                        ["spanish"] = "Spanish",
                        ["french"] = "French",
                        ["german"] = "German",
                    }
                )
            ),
        },
    }
);
#endregion

Console.WriteLine("Answers: " + JsonSerializer.Serialize(result.Answers));
Console.WriteLine(
    "Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out"
);
