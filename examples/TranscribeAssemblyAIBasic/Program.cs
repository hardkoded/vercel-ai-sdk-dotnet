// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AssemblyAI;
using Vercel.AI.Provider;

var audioPath = args.Length > 0 ? args[0] : "data/galileo.mp3";
if (
    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASSEMBLYAI_API_KEY"))
    || !File.Exists(audioPath)
)
{
    Console.WriteLine(
        "Set ASSEMBLYAI_API_KEY and pass the path of an audio file (default data/galileo.mp3) to run this example."
    );
    return;
}

#region transcribe-assemblyai-basic
var model = AssemblyAIProvider.Create().Transcription("assemblyai-stt-3-7");
var result = await model.TranscribeAsync(
    new AudioInput(await File.ReadAllBytesAsync(audioPath), "audio/mpeg", null),
    new AssemblyAITranscriptionRequest(),
    CancellationToken.None
);

Console.WriteLine("Text: " + result.Text);
Console.WriteLine("Duration: " + result.DurationInSeconds);
Console.WriteLine("Language: " + result.Language);
Console.WriteLine("Segments: " + result.Segments.Count);
Console.WriteLine("Warnings: " + result.Warnings.Count);
#endregion
