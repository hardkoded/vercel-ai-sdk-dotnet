// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AssemblyAI;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASSEMBLYAI_API_KEY")))
{
    Console.WriteLine("Set ASSEMBLYAI_API_KEY to run this example.");
    return;
}

#region generate-speech-assemblyai-basic
var model = AssemblyAIProvider.Create().Speech("assemblyai-tts-1");
var result = await model.GenerateAsync(
    new AssemblyAISpeechRequest("Hello from AssemblyAI! This is a test of the text-to-speech API.")
    {
        Voice = "aai-kyla",
    },
    CancellationToken.None
);

Console.WriteLine("Audio bytes: " + result.Audio.Length);
Console.WriteLine("Warnings: " + result.Warnings.Count);
Console.WriteLine("Provider Metadata: " + result.ProviderMetadata);
await File.WriteAllBytesAsync("speech.mp3", result.Audio);
#endregion
