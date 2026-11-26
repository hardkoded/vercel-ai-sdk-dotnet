// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI;
using Vercel.AI.ElevenLabs;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;

if (
    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELEVENLABS_API_KEY"))
    || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY"))
)
{
    Console.WriteLine("Set ELEVENLABS_API_KEY and AI_GATEWAY_API_KEY to run this example.");
    return;
}

#region stream-speech-elevenlabs-stream-text
// Voices a language model's answer while it is still being generated. Real deltas can end mid-word; the
// ElevenLabs provider holds partial words back until a word boundary, so no smoothing is needed here.
var client = new AiClient(GatewayProvider.Create());
var textStream = client.StreamTextAsync(
    new StreamTextOptions
    {
        ModelId = "openai/gpt-5.4-nano",
        Prompt = "In three sentences, explain why the sky is blue.",
    }
);

var result = StreamSpeech.Start(
    new StreamSpeechRequest
    {
        Model = (IStreamingSpeechCaller)
            ElevenLabsProvider.Create().SpeechModel("eleven_flash_v2_5"),
        TextStream = ToParts(textStream.TextStream()),
        OutputFormat = new SpeechAudioFormat("audio/mpeg"),
    }
);

var chunks = new List<byte[]>();
await foreach (var chunk in result.AudioStream)
{
    chunks.Add(chunk);
}

Directory.CreateDirectory("output");
var file =
    "output/stream-speech-stream-text-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ".mp3";
var audio = chunks.SelectMany(chunk => chunk).ToArray();
await File.WriteAllBytesAsync(file, audio);
Console.WriteLine("audio: " + file + " " + audio.Length + " bytes");
Console.WriteLine("usage: " + (await result.Usage).InputCharacters + " characters");

static async IAsyncEnumerable<SpeechTextPart> ToParts(
    IAsyncEnumerable<string> deltas,
    [EnumeratorCancellation] CancellationToken cancellationToken = default
)
{
    await foreach (var delta in deltas.WithCancellation(cancellationToken))
    {
        yield return delta;
    }
}
#endregion
