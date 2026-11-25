// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Operations.Transcribe;

/// <summary>Fakes and helpers shared by the transcribe and streamTranscribe tests.</summary>
internal static class TranscribeSupport
{
    internal static ModelStreamPart[] Finish(string text)
    {
        return new[]
        {
            new ModelStreamPart { Type = "finish", Text = text },
        };
    }

    internal static async Task<List<ModelStreamPart>> Read(IAsyncEnumerable<ModelStreamPart> stream)
    {
        var parts = new List<ModelStreamPart>();
        await foreach (var part in stream)
        {
            parts.Add(part);
        }

        return parts;
    }

    internal static List<WarningLogContext> Watch()
    {
        var seen = new List<WarningLogContext>();
        WarningLog.Observer = seen.Add;
        return seen;
    }

    internal sealed class TranscriptFake : ITranscriptionUrlCaller
    {
        public Func<
            TranscriptionModelUrlCall,
            Task<TranscriptionModelResult?>
        >? FromUrl { get; set; }

        public TranscriptionModelUrlCall? UrlGenerated { get; private set; }

        public int UrlCalls { get; private set; }

        public int GenerateCalls { get; private set; }

        public Func<TranscriptionModelCall, Task<TranscriptionModelResult>>? Generate { get; set; }

        public Task<TranscriptionModelResult?> DoGenerateFromUrlAsync(
            TranscriptionModelUrlCall call,
            CancellationToken cancellationToken
        )
        {
            UrlGenerated = call;
            UrlCalls++;
            return FromUrl?.Invoke(call)
                ?? throw new InvalidOperationException("The fake has no native URL handler.");
        }

        public string Provider { get; set; } = "test-provider";

        public string ModelId { get; set; } = "test-model";

        public string SpecificationVersion => "v4";

        public bool CanStream { get; set; } = true;

        public string? Text { get; set; } = "hello";

        public string? Language { get; set; }

        public IReadOnlyList<TranscriptSegment>? Segments { get; set; }

        public IReadOnlyList<TranscriptSegment>? Words { get; set; }

        public double? Duration { get; set; }

        public JsonElement? Usage { get; set; }

        public IReadOnlyList<OperationWarning>? Warnings { get; set; }

        public JsonElement? Metadata { get; set; }

        public ProviderResponse? Response { get; set; }

        public TranscriptionModelCall? Generated { get; private set; }

        public TranscriptionStreamCall? Call { get; private set; }

        public ModelPartStream? Stream { get; private set; }

        public IReadOnlyList<ModelStreamPart> Parts { get; set; } = Array.Empty<ModelStreamPart>();

        public bool Completes { get; set; } = true;

        public Exception? Failure { get; set; }

        public Exception? SetupError { get; set; }

        public bool HoldSetup { get; set; }

        public bool LockAudio { get; set; }

        public Func<TranscriptionStreamCall, Task>? BeforeStream { get; set; }

        public TaskCompletionSource<bool> Started { get; } = new TaskCompletionSource<bool>();

        public TaskCompletionSource<bool> SetupCancelled { get; } =
            new TaskCompletionSource<bool>();

        public Task<TranscriptionModelResult> DoGenerateAsync(
            TranscriptionModelCall call,
            CancellationToken cancellationToken
        )
        {
            Generated = call;
            GenerateCalls++;
            if (Generate != null)
            {
                return Generate(call);
            }

            return Task.FromResult(
                new TranscriptionModelResult(
                    Text,
                    Segments,
                    Language,
                    Duration,
                    Warnings,
                    Metadata,
                    Response,
                    Words,
                    Usage
                )
            );
        }

        public async Task<TranscriptionStreamStart?> DoStreamAsync(
            TranscriptionStreamCall call,
            CancellationToken cancellationToken
        )
        {
            Call = call;
            Started.TrySetResult(true);
            if (LockAudio)
            {
                call.Audio.Lock();
            }

            if (BeforeStream != null)
            {
                await BeforeStream(call);
            }

            if (SetupError != null)
            {
                throw SetupError;
            }

            if (HoldSetup)
            {
                try
                {
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    SetupCancelled.TrySetResult(true);
                    throw;
                }
            }

            Stream = new ModelPartStream(Parts, Completes, Failure);
            return new TranscriptionStreamStart(Stream);
        }
    }
}
