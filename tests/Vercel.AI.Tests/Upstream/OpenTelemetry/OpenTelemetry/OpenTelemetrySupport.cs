// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>The event factories and span helpers at the top of <c>open-telemetry.test.ts</c>.</summary>
internal static class OpenTelemetrySupport
{
    public const string CallId = "test-call-1";

    /// <summary>Port of <c>makeOnStartEvent</c>.</summary>
    public static GenerateTextStartEvent MakeOnStartEvent(
        string? functionId = null,
        Vercel.AI.Prompt.Instructions? instructions = null,
        IReadOnlyDictionary<string, object?>? runtimeContext = null,
        IReadOnlyDictionary<string, string?>? headers = null,
        long? timestamp = null,
        DurableTelemetryEventContext? durableContext = null
    ) =>
        new GenerateTextStartEvent
        {
            CallId = CallId,
            OperationId = "ai.generateText",
            Provider = "openai.chat",
            ModelId = "gpt-4",
            Instructions = instructions,
            Messages = [new UserModelMessage("Hello")],
            MaxOutputTokens = 100,
            Temperature = 0.7,
            RuntimeContext = runtimeContext,
            Headers = headers,
            Timestamp = timestamp,
            DurableContext = durableContext,
        }.WithTelemetry(new TelemetryOptions { FunctionId = functionId });

    /// <summary>Port of <c>makeStepStartEvent</c>.</summary>
    public static GenerateTextStepStartEvent MakeStepStartEvent(
        int stepNumber = 0,
        IReadOnlyDictionary<string, object?>? runtimeContext = null,
        ToolChoice? toolChoice = null,
        long? timestamp = null
    ) =>
        new()
        {
            CallId = CallId,
            Provider = "openai.chat",
            ModelId = "gpt-4",
            StepNumber = stepNumber,
            RuntimeContext = runtimeContext,
            ToolChoice = toolChoice,
            Timestamp = timestamp,
        };

    /// <summary>Port of <c>makeLanguageModelCallStartEvent</c>.</summary>
    public static LanguageModelCallStartEvent MakeLanguageModelCallStartEvent(
        Vercel.AI.Prompt.Instructions? instructions = null,
        IReadOnlyList<ModelMessage>? messages = null,
        IReadOnlyList<LanguageModelTool>? tools = null,
        long? timestamp = null,
        DurableTelemetryEventContext? durableContext = null
    ) =>
        new()
        {
            CallId = CallId,
            Provider = "openai.chat",
            ModelId = "gpt-4",
            Instructions = instructions,
            Messages = messages ?? [],
            Tools = tools,
            Timestamp = timestamp,
            DurableContext = durableContext,
        };

    /// <summary>Port of the <c>{ name, status, ended }</c> projection of the mock spans.</summary>
    public static IReadOnlyList<(
        string Name,
        ActivityStatusCode Status,
        string? Message,
        bool Ended
    )> Summarize(MockTracer tracer) =>
        [
            .. tracer.Spans.Select(span =>
                (span.Name, span.Activity.Status, span.Activity.StatusDescription, span.Ended)
            ),
        ];

    /// <summary>Port of <c>makeLanguageModelCallEndEvent</c>.</summary>
    public static LanguageModelCallEndEvent MakeLanguageModelCallEndEvent(
        string provider = "openai.chat",
        string modelId = "gpt-4",
        FinishReason finishReason = FinishReason.Stop,
        LanguageModelUsage? usage = null,
        IReadOnlyList<GeneratedContent>? content = null,
        ModelCallPerformance? performance = null,
        JsonElement? providerMetadata = null,
        long? timestamp = null
    ) =>
        new(
            provider,
            modelId,
            finishReason,
            usage ?? new LanguageModelUsage(10, 20, 30),
            content ?? [new GeneratedText("Hello world")],
            "test-response-id",
            providerMetadata,
            TimeSpan.FromMilliseconds(1000)
        )
        {
            CallId = CallId,
            Timestamp = timestamp,
            Performance = performance
                ?? new ModelCallPerformance
                {
                    ResponseTimeMs = 1000,
                    EffectiveOutputTokensPerSecond = 20,
                    EffectiveTotalTokensPerSecond = 30,
                },
        };

    /// <summary>Port of <c>makeStepFinishEvent</c>.</summary>
    public static StepResult MakeStepFinishEvent(
        string? text = "Hello world",
        IReadOnlyList<GeneratedToolCall>? toolCalls = null,
        FinishReason finishReason = FinishReason.Stop,
        LanguageModelUsage? usage = null,
        int stepNumber = 0,
        JsonElement? providerMetadata = null,
        IReadOnlyList<GeneratedContent>? content = null,
        long? timestamp = null
    ) =>
        new(
            text ?? string.Empty,
            null,
            toolCalls ?? [],
            [],
            finishReason,
            usage ?? new LanguageModelUsage(10, 20, 30),
            [],
            providerMetadata,
            provider: "openai.chat",
            modelId: "gpt-4",
            content: content ?? (text is null ? [] : [new GeneratedText(text)])
        )
        {
            CallId = CallId,
            StepNumber = stepNumber,
            Timestamp = timestamp,
        };

    /// <summary>Port of <c>makeFinishEvent</c>.</summary>
    public static GenerateTextResult MakeFinishEvent(
        LanguageModelUsage? usage = null,
        JsonElement? providerMetadata = null,
        long? timestamp = null
    )
    {
        var step = MakeStepFinishEvent(providerMetadata: providerMetadata);
        return new GenerateTextResult(
            step.Text,
            null,
            [step],
            step.FinishReason,
            usage ?? new LanguageModelUsage(10, 20, 30),
            null,
            [],
            providerMetadata
        )
        {
            Timestamp = timestamp,
        };
    }

    /// <summary>Port of <c>makeToolCallStartEvent</c>.</summary>
    public static ToolExecutionStartEvent MakeToolCallStartEvent(
        GeneratedToolCall? toolCall = null,
        long? timestamp = null
    ) => new(toolCall ?? DefaultToolCall(), []) { CallId = CallId, Timestamp = timestamp };

    /// <summary>Port of <c>makeToolCallFinishEvent</c>.</summary>
    public static ToolExecutionEndEvent MakeToolCallFinishEvent(
        bool success,
        int toolExecutionMs = 42,
        long? timestamp = null
    )
    {
        var toolCall = DefaultToolCall();
        var output = success
            ? new ExecutedTool("tool-call-1", "myTool", "{\"result\":\"ok\"}", false)
            : new ExecutedTool("tool-call-1", "myTool", "null", true)
            {
                Error = new JsError("tool failed"),
            };
        return new ToolExecutionEndEvent(
            toolCall,
            [],
            output,
            TimeSpan.FromMilliseconds(toolExecutionMs)
        )
        {
            CallId = CallId,
            Timestamp = timestamp,
        };
    }

    /// <summary>Port of <c>serializeSpan</c>.</summary>
    public static JsonObject SerializeSpan(MockSpan span)
    {
        var result = new JsonObject
        {
            ["name"] = span.Name,
            ["ended"] = span.Ended,
            ["initAttributes"] = SerializeAttributes(span.InitAttributes),
            ["runtimeAttributes"] = SerializeAttributes(RuntimeAttributesOf(span)),
        };
        return result;
    }

    /// <summary>Port of <c>serializeTrace</c>.</summary>
    public static JsonArray SerializeTrace(MockTracer tracer)
    {
        var result = new JsonArray();
        foreach (var span in tracer.Spans)
        {
            result.Add(SerializeSpan(span));
        }

        return result;
    }

    /// <summary>Port of <c>parseJsonAttributes</c> for one key: the attribute parsed as JSON, or null when it is not a string.</summary>
    public static JsonNode? ParseJsonAttribute(
        IReadOnlyDictionary<string, object?> attributes,
        string key
    )
    {
        if (!attributes.TryGetValue(key, out var value) || value is not string text)
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(text);
        }
        catch (JsonException)
        {
            return JsonValue.Create(text);
        }
    }

    // The integration sets gen_ai.provider.name again on a model call span when the call ends. An Activity cannot tell that from the
    // value it started with, so a span that ended adds it back, like upstream's mock span records every attribute that was set.
    private static IReadOnlyDictionary<string, object?> RuntimeAttributesOf(MockSpan span)
    {
        var attributes = new Dictionary<string, object?>(
            span.RuntimeAttributes,
            StringComparer.Ordinal
        );
        if (
            span.Name.StartsWith("chat ", StringComparison.Ordinal)
            && span.Attributes.TryGetValue("gen_ai.response.model", out _)
            && span.Attributes.TryGetValue("gen_ai.provider.name", out var provider)
        )
        {
            attributes["gen_ai.provider.name"] = provider;
        }

        return attributes;
    }

    private static JsonObject SerializeAttributes(IReadOnlyDictionary<string, object?> attributes)
    {
        var result = new JsonObject();
        foreach (var pair in attributes)
        {
            result[pair.Key] = JsonSerializer.SerializeToNode(pair.Value);
        }

        return result;
    }

    private static GeneratedToolCall DefaultToolCall() =>
        new("tool-call-1", "myTool", "{\"query\":\"test\"}");
}
