// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>Stands in for upstream's <c>dummyEvent</c> (<c>{}</c>), which C# cannot pass for every callback type.</summary>
internal static class DispatcherEvents
{
    internal static readonly JsonElement EmptyJson = JsonDocument.Parse("{}").RootElement.Clone();

    internal static LanguageModelCallStartEvent LanguageModelCallStart() => new();

    internal static GeneratedToolCall ToolCall() => new("tool-1", "weather", "{}");

    internal static StepResult Step() =>
        new("", null, [], [], FinishReason.Stop, LanguageModelUsage.Null, []);

    internal static GenerateTextResult TextResult() =>
        new("", null, [], FinishReason.Stop, LanguageModelUsage.Null, null, []);

    internal static IReadOnlyDictionary<string, object?> Context() =>
        new Dictionary<string, object?>();

    internal static DecideEvent Decide() =>
        new(
            "call-1",
            "ai.decide",
            Context(),
            "provider",
            "model",
            null,
            new Dictionary<string, DecisionQuestion>(),
            0,
            null,
            EmptyJson
        );

    internal static DecisionModelEvent DecisionModel() =>
        new(
            "call-1",
            "ai.decide.doGenerate",
            "provider",
            "model",
            [],
            new Dictionary<string, DecisionQuestion>()
        );

    internal static ExtractStartEvent ExtractStart() =>
        new(
            "call-1",
            Context(),
            "provider",
            "model",
            null,
            new Dictionary<string, ExtractionField>(),
            null,
            null,
            null,
            0,
            null,
            EmptyJson
        );

    internal static ExtractionModelCallStartEvent ExtractionModelCallStart() =>
        new("call-1", "provider", "model", [], new Dictionary<string, ExtractionField>(), null);

    internal static ProviderResponse Response() => new();

    internal static TranscriptionAudioInfo Audio() => new(1, "audio/wav");

    internal static EmbeddingModelCallStartEvent EmbedStart() =>
        new("call-1", "embed-1", "ai.embed.doEmbed", "provider", "model", ["a"]);

    internal static EmbeddingModelCallEndEvent EmbedEnd() =>
        new(
            "call-1",
            "embed-1",
            "ai.embed.doEmbed",
            "provider",
            "model",
            ["a"],
            [
                [1d],
            ],
            new OperationUsage(tokens: 1)
        );

    internal static RerankingModelCallStartEvent RerankStart() =>
        new("call-1", "ai.rerank.doRerank", "provider", "model", ["a"], "text", "q", null);

    internal static RerankingModelCallEndEvent RerankEnd() =>
        new("call-1", "ai.rerank.doRerank", "provider", "model", "text", []);

    internal static StreamTranscriptionStartEvent StreamTranscriptionStart() =>
        new("call-1", "provider", "model", Audio(), null, 0, null, EmptyJson);

    internal static StreamTranscriptionEndEvent StreamTranscriptionEnd() =>
        new("call-1", "provider", "model", Audio(), "", [], null, null, null, [], null, Response());
}
