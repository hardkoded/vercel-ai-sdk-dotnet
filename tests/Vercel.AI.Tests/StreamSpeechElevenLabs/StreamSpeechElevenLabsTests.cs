// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.ElevenLabs;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.StreamSpeechElevenLabs;

/// <summary>Real-model ports of the <c>examples/ai-functions/src/stream-speech/elevenlabs</c> pages.</summary>
[Trait("Category", "RealModel")]
public sealed class StreamSpeechElevenLabsTests
{
    private const string Alice = "21m00Tcm4TlvDq8ikWAM";
    private const string Bob = "pNInz6obpgDQGcFmaJgB";

    [SkippableFact]
    public async Task Basic_streams_text_deltas_to_audio()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELEVENLABS_API_KEY")));
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var result = StreamSpeech.Start(
            new StreamSpeechRequest
            {
                CancellationToken = timeout.Token,
                Model = Model("eleven_flash_v2_5"),
                TextStream = Deltas(
                    "Hello from the AI SDK! ",
                    "This sentence is streamed to ElevenLabs ",
                    "in small pieces, as a language model would produce it. "
                ),
                OutputFormat = new SpeechAudioFormat("audio/mpeg"),
            }
        );

        var total = 0;
        await foreach (var chunk in result.AudioStream)
        {
            total += chunk.Length;
        }

        Assert.True(total > 0);
        Assert.True((await result.Usage).InputCharacters > 0);
    }

    [SkippableTheory]
    [InlineData("eleven_v4")]
    public async Task Dialogue_streams_speaker_turns(string modelId)
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELEVENLABS_API_KEY")));
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var result = StreamSpeech.Start(
            new StreamSpeechRequest
            {
                CancellationToken = timeout.Token,
                Model = Model(modelId),
                TextStream = Script(),
                ProviderOptions = JsonDocument
                    .Parse(
                        "{\"elevenlabs\":{\"dialogue\":{\"voices\":[\""
                            + Alice
                            + "\",\""
                            + Bob
                            + "\"]}}}"
                    )
                    .RootElement.Clone(),
            }
        );

        var chunks = 0;
        await foreach (var chunk in result.AudioStream)
        {
            chunks++;
        }

        Assert.True(chunks > 0);
        Assert.True((await result.Usage).InputCharacters > 0);
    }

    [SkippableFact]
    public async Task StreamText_voices_a_language_model_answer_while_it_is_generated()
    {
        Skip.If(
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELEVENLABS_API_KEY"))
                || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY"))
        );
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
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
                CancellationToken = timeout.Token,
                Model = Model("eleven_flash_v2_5"),
                TextStream = ToParts(textStream.TextStream()),
                OutputFormat = new SpeechAudioFormat("audio/mpeg"),
            }
        );

        var total = 0;
        await foreach (var chunk in result.AudioStream)
        {
            total += chunk.Length;
        }

        Assert.True(total > 0);
        Assert.True((await result.Usage).InputCharacters > 0);
    }

    private static async IAsyncEnumerable<SpeechTextPart> ToParts(IAsyncEnumerable<string> deltas)
    {
        await foreach (var delta in deltas)
        {
            yield return delta;
        }
    }

    private static IStreamingSpeechCaller Model(string modelId) =>
        (IStreamingSpeechCaller)ElevenLabsProvider.Create().SpeechModel(modelId);

    private static async IAsyncEnumerable<SpeechTextPart> Deltas(params string[] deltas)
    {
        foreach (var delta in deltas)
        {
            yield return delta;
            await Task.Yield();
        }
    }

    private static async IAsyncEnumerable<SpeechTextPart> Script()
    {
        var turns = new[]
        {
            (Alice, "Did you hear we can stream a whole dialogue now? "),
            (Bob, "Over a single socket, with two voices? That is neat. "),
            (Alice, "Three turns, and the audio starts right away."),
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
}
