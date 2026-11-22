// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AssemblyAI;
using Vercel.AI.Provider;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASSEMBLYAI_API_KEY")))
{
    Console.WriteLine("Set ASSEMBLYAI_API_KEY to run this example.");
    return;
}

var audioUrl =
    "https://github.com/vercel/ai/raw/refs/heads/main/examples/ai-functions/data/galileo.mp3";

#region transcribe-assemblyai-url
using var audio = new HttpClient();
var model = AssemblyAIProvider.Create().Transcription("assemblyai-stt-3-7");
var result = await model.TranscribeAsync(
    new AudioInput(await audio.GetByteArrayAsync(audioUrl), "audio/mpeg", null),
    new AssemblyAITranscriptionRequest(),
    CancellationToken.None
);

Console.WriteLine("Text: " + result.Text);
Console.WriteLine("Duration: " + result.DurationInSeconds);
Console.WriteLine("Language: " + result.Language);
Console.WriteLine("Segments: " + result.Segments.Count);
Console.WriteLine("Warnings: " + result.Warnings.Count);
#endregion
