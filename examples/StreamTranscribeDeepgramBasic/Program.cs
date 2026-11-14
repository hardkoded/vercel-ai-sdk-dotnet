// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI;
using Vercel.AI.Deepgram;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
    || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY")))
{
    Console.WriteLine("Set OPENAI_API_KEY and DEEPGRAM_API_KEY to run this example.");
    return;
}

#region stream-transcribe-deepgram-basic
var speech = await new OpenAISpeechModel(OpenAIProvider.Create(), "gpt-4o-mini-tts").GenerateAsync(
    new OpenAISpeechCall("Hello from the AI SDK! Deepgram can transcribe live audio as it arrives.")
    {
        OutputFormat = "pcm",
    },
    CancellationToken.None);

// 100ms of 24kHz, 16-bit mono PCM, paced like microphone input.
const int chunkSize = 4800;
var audio = new AudioChunkStream();
var feed = Task.Run(async () =>
{
    for (var offset = 0; offset < speech.Audio.Length; offset += chunkSize)
    {
        audio.Write(speech.Audio.AsSpan(offset, Math.Min(chunkSize, speech.Audio.Length - offset)).ToArray());
        await Task.Delay(100);
    }

    audio.Complete();
});

using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
var result = StreamTranscribe.Start(new StreamTranscribeRequest
{
    Model = (DeepgramProvider.DeepgramTranscriptionModel)DeepgramProvider.Create().TranscriptionModel("nova-3"),
    Audio = audio,
    InputAudioFormat = new AudioFormat("audio/pcm", 24000),
    ProviderOptions = OperationJson.Parse("""{"deepgram":{"language":"en","streaming":{"interimResults":true}}}"""),
    CancellationToken = timeout.Token,
});

await foreach (var part in result.FullStream)
{
    if (part.Type == "error")
    {
        throw part.Error!;
    }

    if (part.Type is "transcript-partial" or "transcript-final")
    {
        Console.WriteLine(part.Type + " " + part.Text);
    }
}

await feed;
Console.WriteLine("Text: " + await result.Text);
Console.WriteLine("Segments: " + (await result.Segments).Count);
Console.WriteLine("Duration: " + await result.DurationInSeconds);
#endregion
