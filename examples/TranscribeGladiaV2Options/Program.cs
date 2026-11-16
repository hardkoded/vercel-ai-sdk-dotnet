// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gladia;
using Vercel.AI.Provider;

var audioPath = args.Length > 0 ? args[0] : "data/galileo.mp3";
if (
    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GLADIA_API_KEY"))
    || !File.Exists(audioPath)
)
{
    Console.WriteLine(
        "Set GLADIA_API_KEY and pass the path of an audio file (default data/galileo.mp3) to run this example."
    );
    return;
}

#region transcribe-gladia-v2-options
var model = (GladiaProvider.GladiaTranscriptionModel)
    GladiaProvider.Create().TranscriptionModel("solaria-3");
using var options = JsonDocument.Parse(
    """
    {
      "languageConfig": { "languages": ["en"], "codeSwitching": false },
      "piiRedaction": true,
      "piiRedactionConfig": { "processedTextType": "MARKER" }
    }
    """
);
var result = await model.TranscribeAsync(
    new AudioInput(await File.ReadAllBytesAsync(audioPath), "audio/mpeg", null),
    new GladiaTranscriptionRequest
    {
        ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["gladia"] = options.RootElement.Clone(),
        },
    },
    CancellationToken.None
);

Console.WriteLine("Text: " + result.Text);
Console.WriteLine("Language: " + result.Language);
Console.WriteLine("Duration: " + result.DurationInSeconds);
Console.WriteLine("Warnings: " + result.Warnings.Count);
#endregion
