// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Operations.StreamSpeech;

/// <summary>Collects the console error writer so a test can assert what was logged.</summary>
[CollectionDefinition("ConsoleError", DisableParallelization = true)]
public sealed class ConsoleErrorCollection
{
}

/// <summary>Port of <c>MockStreamingSpeechModelV4</c> and <c>MockSpeechModelV4</c>.</summary>
internal sealed class MockStreamingSpeechModel : IStreamingSpeechCaller
{
    public string Provider { get; init; } = "mock-provider";

    public string ModelId { get; init; } = "mock-model-id";

    public Func<SpeechModelCall, CancellationToken, Task<SpeechModelResult>> DoGenerate { get; init; } = (_, _) => throw new NotSupportedException();

    public Func<SpeechModelStreamCall, CancellationToken, Task<SpeechModelStreamResult>> DoStream { get; init; } = (_, _) => throw new NotSupportedException();

    public Task<SpeechModelResult> DoGenerateAsync(SpeechModelCall call, CancellationToken cancellationToken)
    {
        return DoGenerate(call, cancellationToken);
    }

    public Task<SpeechModelStreamResult> DoStreamAsync(SpeechModelStreamCall call, CancellationToken cancellationToken)
    {
        return DoStream(call, cancellationToken);
    }
}

internal sealed class MockSpeechModel : ISpeechCaller
{
    public string Provider { get { return "mock-provider"; } }

    public string ModelId { get { return "mock-model-id"; } }

    public Task<SpeechModelResult> DoGenerateAsync(SpeechModelCall call, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}

/// <summary>A text source that records whether its enumerator was released.</summary>
internal sealed class TextProbe : IAsyncEnumerable<SpeechTextPart>
{
    public bool Cancelled { get; private set; }

    public IAsyncEnumerator<SpeechTextPart> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new Enumerator(this);
    }

    private sealed class Enumerator : IAsyncEnumerator<SpeechTextPart>
    {
        private readonly TextProbe _owner;

        public Enumerator(TextProbe owner)
        {
            _owner = owner;
        }

        public SpeechTextPart Current { get { return null!; } }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(false);
        }

        public ValueTask DisposeAsync()
        {
            _owner.Cancelled = true;
            return default;
        }
    }
}

internal static class StreamSpeechSupport
{
    public static readonly DateTime TestDate = new DateTime(2024, 1, 1);

    public static readonly SpeechAudioFormat Mp3 = new SpeechAudioFormat("audio/mpeg");

    public static SpeechModelStreamPart StreamStart() => new SpeechModelStreamPart { Type = "stream-start", Warnings = Array.Empty<OperationWarning>() };

    public static SpeechModelStreamPart Audio(params byte[] audio) => new SpeechModelStreamPart { Type = "audio", Audio = audio, AudioFormat = Mp3 };

    public static SpeechModelStreamPart AudioBase64(string audio) => new SpeechModelStreamPart { Type = "audio", AudioBase64 = audio, AudioFormat = Mp3 };

    public static SpeechModelStreamPart Finish(SpeechStreamUsage? usage = null, System.Text.Json.JsonElement? providerMetadata = null) => new SpeechModelStreamPart { Type = "finish", Usage = usage, ProviderMetadata = providerMetadata };

    public static SpeechModelStreamPart Error(Exception error) => new SpeechModelStreamPart { Type = "error", Error = error };

    public static SpeechModelStreamPart[] BasicParts()
    {
        return new[] { StreamStart(), Audio(1, 2), AudioBase64("AwQ="), Finish(new SpeechStreamUsage { InputCharacters = 11 }) };
    }

    public static SpeechModelStreamResult CreateStreamResponse(params SpeechModelStreamPart[] parts)
    {
        return new SpeechModelStreamResult(Enumerate(parts), new SpeechModelStreamResponse { Timestamp = TestDate, ModelId = "test-model-id", Headers = new Dictionary<string, string> { ["x-test"] = "value" } });
    }

    /// <summary>A model stream that stays open after its parts until it is cancelled.</summary>
    public static IAsyncEnumerable<SpeechModelStreamPart> OpenStream(Action onCancel, params SpeechModelStreamPart[] parts)
    {
        return Open(onCancel, parts);
    }

    public static async IAsyncEnumerable<SpeechModelStreamPart> Enumerate(IEnumerable<SpeechModelStreamPart> parts, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var part in parts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return part;
        }

        await Task.CompletedTask;
    }

    public static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
    {
        var items = new List<T>();
        await foreach (var item in source)
        {
            items.Add(item);
        }

        return items;
    }

    public static async IAsyncEnumerable<SpeechTextPart> TextStream(params SpeechTextPart[] parts)
    {
        foreach (var part in parts)
        {
            yield return part;
        }

        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<SpeechModelStreamPart> Open(Action onCancel, IEnumerable<SpeechModelStreamPart> parts, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var part in parts)
            {
                yield return part;
            }

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        finally
        {
            onCancel();
        }
    }
}
