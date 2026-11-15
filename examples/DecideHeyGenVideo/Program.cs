// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.HeyGen;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HEYGEN_API_KEY")))
{
    Console.WriteLine("Set HEYGEN_API_KEY to run this example.");
    return;
}

var path = args.Length > 0 ? args[0] : "./data/prudence.mp4";
if (!File.Exists(path))
{
    Console.WriteLine("Pass the path of an MP4 file to run this example.");
    return;
}

#region decide-heygen-video
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = HeyGenProvider.Create().DecisionModel("heygen-decision-1"),
    State = DecisionState.From(
    [
        new DecisionTextPart("Review this clip before it is published."),
        new DecisionFilePart("video/mp4", FilePartInput.FromBytes(File.ReadAllBytes(path))),
    ]),
    Questions = new Dictionary<string, DecisionQuestion>
    {
        ["person"] = new("boolean", "Does the video show a person?"),
        ["setting"] = new(
            "choice",
            "Where does the video take place?",
            new DecisionChoiceCriteria(new Dictionary<string, DecisionInput?> { ["indoors"] = null, ["outdoors"] = null })),
    },
});
#endregion

Console.WriteLine("Answers: " + JsonSerializer.Serialize(result.Answers));
Console.WriteLine("Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out");
