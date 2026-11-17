// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY")))
{
    Console.WriteLine("Set GOOGLE_GENERATIVE_AI_API_KEY to run this example.");
    return;
}

#region google-speech-language
var model = (GoogleSpeechModel)GoogleProvider.Create().SpeechModel("gemini-3.8-flash-tts");
var result = await model.GenerateAsync(
    new GoogleSpeechCall("Guten Morgen! Willkommen beim AI SDK.") { Language = "de-DE" },
    CancellationToken.None
);
Console.WriteLine("Warnings: " + model.Warnings.Count);
await File.WriteAllBytesAsync("speech.wav", result.Audio);
#endregion
