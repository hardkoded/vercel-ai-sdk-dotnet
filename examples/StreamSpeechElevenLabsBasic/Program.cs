// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI.ElevenLabs;
using Vercel.AI.Operations;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELEVENLABS_API_KEY")))
{
    Console.WriteLine("Set ELEVENLABS_API_KEY to run this example.");
    return;
}

#region stream-speech-elevenlabs-basic
var result = StreamSpeech.Start(
    new StreamSpeechRequest
    {
        Model = (IStreamingSpeechCaller)
            ElevenLabsProvider.Create().SpeechModel("eleven_flash_v2_5"),
        TextStream = TextDeltas(),
        OutputFormat = new SpeechAudioFormat("audio/mpeg"),
    }
);

var chunks = new List<byte[]>();
await foreach (var chunk in result.AudioStream)
{
    chunks.Add(chunk);
    Console.WriteLine("audio chunk: " + chunk.Length + " bytes");
}

Directory.CreateDirectory("output");
await File.WriteAllBytesAsync(
    "output/stream-speech-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ".mp3",
    chunks.SelectMany(chunk => chunk).ToArray()
);
Console.WriteLine("usage: " + (await result.Usage).InputCharacters + " characters");

static async IAsyncEnumerable<SpeechTextPart> TextDeltas(
    [EnumeratorCancellation] CancellationToken cancellationToken = default
)
{
    foreach (
        var delta in new[]
        {
            "Hello from the AI SDK! ",
            "This sentence is streamed to ElevenLabs ",
            "in small pieces, as a language model would produce it. ",
        }
    )
    {
        yield return delta;
        await Task.Yield();
    }
}
#endregion
