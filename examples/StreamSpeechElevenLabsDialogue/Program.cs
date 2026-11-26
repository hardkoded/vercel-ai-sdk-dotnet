// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.ElevenLabs;
using Vercel.AI.Operations;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELEVENLABS_API_KEY")))
{
    Console.WriteLine("Set ELEVENLABS_API_KEY to run this example.");
    return;
}

#region stream-speech-elevenlabs-dialogue
const string alice = "21m00Tcm4TlvDq8ikWAM";
const string bob = "pNInz6obpgDQGcFmaJgB";

var modelId = args.Length > 0 ? args[0] : "eleven_v4";
var result = StreamSpeech.Start(
    new StreamSpeechRequest
    {
        Model = (IStreamingSpeechCaller)ElevenLabsProvider.Create().SpeechModel(modelId),
        TextStream = Script(),
        ProviderOptions = JsonDocument
            .Parse(
                "{\"elevenlabs\":{\"dialogue\":{\"voices\":[\"" + alice + "\",\"" + bob + "\"]}}}"
            )
            .RootElement.Clone(),
    }
);

var chunks = new List<byte[]>();
await foreach (var chunk in result.AudioStream)
{
    chunks.Add(chunk);
}

Directory.CreateDirectory("output");
await File.WriteAllBytesAsync(
    "output/dialogue-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ".mp3",
    chunks.SelectMany(chunk => chunk).ToArray()
);
Console.WriteLine("audio chunks: " + chunks.Count);
Console.WriteLine("usage: " + (await result.Usage).InputCharacters + " characters");

// Each text part names its speaker; a change of voice starts a new turn.
// Parts can arrive in small deltas, as from a language model.
static async IAsyncEnumerable<SpeechTextPart> Script(
    [EnumeratorCancellation] CancellationToken cancellationToken = default
)
{
    var turns = new[]
    {
        (alice, "Did you hear we can stream a whole dialogue now? "),
        (bob, "Over a single socket, with two voices? That is neat. "),
        (alice, "Three turns, and the audio starts right away."),
    };
    foreach (var (voice, line) in turns)
    {
        foreach (var word in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            yield return new SpeechTextPart(word + " ", voice);
            await Task.Yield();
        }
    }
}
#endregion
