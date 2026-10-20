// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

// Mirrors createTestModel and MockLanguageModelV4 of the upstream stream-text tests: one scripted stream per call, with request and response info.
internal sealed class StepModel : ILanguageModel
{
    private readonly IReadOnlyList<IReadOnlyList<LanguageModelStreamPart>> _calls;

    public StepModel(IReadOnlyList<LanguageModelStreamPart> parts)
        : this(new[] { parts })
    {
    }

    public StepModel(IReadOnlyList<IReadOnlyList<LanguageModelStreamPart>> calls)
    {
        _calls = calls;
    }

    public LanguageModelStreamRequest? Request { get; init; }

    public LanguageModelStreamResponse? Response { get; init; }

    public List<LanguageModelCallOptions> Calls { get; } = new();

    public string SpecificationVersion => "V4";

    public string Provider => "mock-provider";

    public string ModelId => "mock-model-id";

    public static LanguageModelStreamPart[] HelloStream() => new LanguageModelStreamPart[]
    {
        new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0)),
        new TextStartStreamPart("1"),
        new TextDeltaStreamPart("1", "Hello"),
        new TextEndStreamPart("1"),
        new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
    };

    public static StreamTextResult Stream(ILanguageModel model, StreamTextOptions options)
    {
        options.Model = model;
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(options);
    }

    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var parts = _calls[Math.Min(Calls.Count, _calls.Count - 1)];
        Calls.Add(options);
        return Task.FromResult(new LanguageModelStreamResult(Read(parts), Request, Response));
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> Read(IReadOnlyList<LanguageModelStreamPart> parts, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var part in parts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return part;
            await Task.Yield();
        }
    }
}
