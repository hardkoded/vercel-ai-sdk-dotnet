// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>The event builders of <c>open-telemetry.test.ts</c> (<c>makeOnStartEvent</c> and the others).</summary>
internal static class OpenTelemetryEvents
{
    public const string CallId = "call-1";

    public static GenerateTextStartEvent MakeOnStartEvent(
        Instructions? instructions = null,
        string? functionId = null
    ) =>
        new GenerateTextStartEvent
        {
            CallId = CallId,
            Provider = "openai.chat",
            ModelId = "gpt-4",
            Instructions = instructions,
            Messages = [new UserModelMessage("Hello")],
            MaxOutputTokens = 100,
            Temperature = 0.7,
            MaxRetries = 2,
        }.WithTelemetry(new TelemetryOptions { FunctionId = functionId });

    public static GenerateTextStepStartEvent MakeStepStartEvent() =>
        new()
        {
            CallId = CallId,
            Provider = "openai.chat",
            ModelId = "gpt-4",
            StepNumber = 0,
        };

    public static LanguageModelCallStartEvent MakeLanguageModelCallStartEvent(
        Instructions? instructions = null,
        IReadOnlyList<ModelMessage>? messages = null,
        IReadOnlyList<LanguageModelTool>? tools = null
    ) =>
        new()
        {
            CallId = CallId,
            Provider = "openai.chat",
            ModelId = "gpt-4",
            Instructions = instructions,
            Messages = messages ?? [],
            Tools = tools,
        };

    public static LanguageModelCallEndEvent MakeLanguageModelCallEndEvent(
        IReadOnlyList<GeneratedContent>? content = null,
        FinishReason finishReason = FinishReason.Stop
    ) =>
        new(
            "openai.chat",
            "gpt-4",
            finishReason,
            new LanguageModelUsage(10, 20, 30),
            content ?? [new GeneratedText("Hello world")],
            "test-response-id",
            null,
            TimeSpan.FromMilliseconds(1000)
        )
        {
            CallId = CallId,
        };

    public static GenerateTextResult MakeFinishEvent()
    {
        var step = new StepResult(
            "Hello world",
            null,
            [],
            [],
            FinishReason.Stop,
            new LanguageModelUsage(10, 20, 30),
            [],
            content: [new GeneratedText("Hello world")],
            provider: "openai.chat",
            modelId: "gpt-4"
        )
        {
            CallId = CallId,
        };
        return new GenerateTextResult(
            "Hello world",
            null,
            [step],
            FinishReason.Stop,
            new LanguageModelUsage(10, 20, 30),
            null,
            []
        );
    }

    /// <summary>Maps to <c>parseJsonAttributes</c>.</summary>
    public static JsonNode? ParseJsonAttribute(
        IReadOnlyDictionary<string, object?> attributes,
        string key
    ) =>
        attributes.TryGetValue(key, out var value) && value is string text
            ? JsonNode.Parse(text)
            : null;
}
