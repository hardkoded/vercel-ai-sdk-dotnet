// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateObject;

/// <summary>Thrown when a model response cannot produce the requested object. Maps to <c>NoObjectGeneratedError</c>.</summary>
public sealed class NoObjectGeneratedException : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_NoObjectGeneratedError";

    /// <summary>Creates the exception and preserves the failed generation details.</summary>
    public NoObjectGeneratedException(
        string message,
        Exception? cause,
        string? text,
        StepResponse response,
        LanguageModelUsage usage,
        FinishReason finishReason)
        : base("AI_NoObjectGeneratedError", message, cause)
    {
        Text = text;
        Response = response ?? throw new ArgumentNullException(nameof(response));
        Usage = usage ?? throw new ArgumentNullException(nameof(usage));
        FinishReason = finishReason;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>The selected text that failed to parse or validate.</summary>
    public string? Text { get; }

    /// <summary>Response metadata from the model call.</summary>
    public StepResponse Response { get; }

    /// <summary>Token usage from the model call.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Why the model finished generating.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error) => HasMarker(error, ErrorMarker);
}

/// <summary>Options for <see cref="IAiClient.GenerateObjectAsync{T}"/>. Maps to <c>generateObject</c>.</summary>
public sealed class GenerateObjectOptions<T>
{
    /// <summary>Gateway model id such as <c>openai/gpt-4.1-mini</c>.</summary>
    public string? ModelId { get; set; }

    /// <summary>Explicit language model. Wins over <see cref="ModelId"/>.</summary>
    public ILanguageModel? Model { get; set; }

    /// <summary>System prompt. <see cref="Instructions"/> wins when both are set.</summary>
    public string? System { get; set; }

    /// <summary>Instructions supplied before the prompt.</summary>
    public Instructions? Instructions { get; set; }

    /// <summary>A single user prompt.</summary>
    public string? Prompt { get; set; }

    /// <summary>Full prompt messages.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; set; }

    /// <summary>JSON Schema for the generated object.</summary>
    public string Schema { get; set; } = string.Empty;

    /// <summary>Optional schema name forwarded to the provider.</summary>
    public string? SchemaName { get; set; }

    /// <summary>Optional schema description forwarded to the provider.</summary>
    public string? SchemaDescription { get; set; }

    /// <summary>Serializer settings used to materialize <typeparamref name="T"/>.</summary>
    public JsonSerializerOptions? SerializerOptions { get; set; }

    /// <summary>Maximum output tokens.</summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>Temperature.</summary>
    public double? Temperature { get; set; }

    /// <summary>Top-p sampling.</summary>
    public double? TopP { get; set; }

    /// <summary>Top-k sampling.</summary>
    public int? TopK { get; set; }

    /// <summary>Presence penalty.</summary>
    public double? PresencePenalty { get; set; }

    /// <summary>Frequency penalty.</summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>Random seed.</summary>
    public int? Seed { get; set; }

    /// <summary>Reasoning effort.</summary>
    public string? Reasoning { get; set; }

    /// <summary>Retries for provider calls made by this request.</summary>
    public int? MaxRetries { get; set; }

    /// <summary>Total or per-step timeout.</summary>
    public TimeoutConfiguration? Timeout { get; set; }

    /// <summary>Extra headers forwarded to the model.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Provider-specific options.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Attempts to repair text that failed JSON parsing or schema validation.</summary>
    public Func<GenerateObjectRepairContext, CancellationToken, Task<string?>>? RepairText { get; set; }

    /// <summary>Called before generation begins.</summary>
    public Func<GenerateObjectStartEvent, CancellationToken, Task>? OnStart { get; set; }

    /// <summary>Called immediately before the model call.</summary>
    public Func<GenerateObjectStepStartEvent, CancellationToken, Task>? OnStepStart { get; set; }

    /// <summary>Called with the raw model result before the selected text is parsed.</summary>
    public Func<GenerateObjectStepEndEvent, CancellationToken, Task>? OnStepEnd { get; set; }

    /// <summary>Called after the object is parsed and validated.</summary>
    public Func<GenerateObjectEndEvent<T>, CancellationToken, Task>? OnFinish { get; set; }
}

/// <summary>Input passed to <see cref="GenerateObjectOptions{T}.RepairText"/>.</summary>
public sealed class GenerateObjectRepairContext
{
    internal GenerateObjectRepairContext(string text, Exception error)
    {
        Text = text;
        Error = error;
    }

    /// <summary>The text that failed parsing or validation.</summary>
    public string Text { get; }

    /// <summary>The parse or validation error.</summary>
    public Exception Error { get; }
}

/// <summary>Base information shared by generate-object callback events.</summary>
public abstract class GenerateObjectEvent
{
    private protected GenerateObjectEvent(string callId, string provider, string modelId)
    {
        CallId = callId;
        Provider = provider;
        ModelId = modelId;
    }

    /// <summary>Identifier shared by every callback from this call.</summary>
    public string CallId { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Requested model id.</summary>
    public string ModelId { get; }
}

/// <summary>Event sent before object generation begins.</summary>
public sealed class GenerateObjectStartEvent : GenerateObjectEvent
{
    internal GenerateObjectStartEvent(string callId, string provider, string modelId, string? prompt, IReadOnlyList<ModelMessage>? messages)
        : base(callId, provider, modelId)
    {
        Prompt = prompt;
        Messages = messages;
    }

    /// <summary>The simple prompt, when supplied.</summary>
    public string? Prompt { get; }

    /// <summary>The prompt messages, when supplied.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; }
}

/// <summary>Event sent immediately before the generate-object model call.</summary>
public sealed class GenerateObjectStepStartEvent : GenerateObjectEvent
{
    internal GenerateObjectStepStartEvent(string callId, string provider, string modelId)
        : base(callId, provider, modelId)
    {
    }

    /// <summary>The only step number made by generateObject.</summary>
    public int StepNumber => 0;
}

/// <summary>The raw result of the one model step made by <c>generateObject</c>.</summary>
public sealed class GenerateObjectStepEndEvent : GenerateObjectEvent
{
    internal GenerateObjectStepEndEvent(string callId, string provider, string modelId, StepResult step)
        : base(callId, provider, modelId)
    {
        ObjectText = step.Text;
        Reasoning = step.ReasoningText;
        FinishReason = step.FinishReason;
        Usage = step.Usage;
        Warnings = step.Warnings;
        Request = step.Request;
        Response = step.Response;
        ProviderMetadata = step.ProviderMetadata;
    }

    /// <summary>The only step number made by generateObject.</summary>
    public int StepNumber => 0;

    /// <summary>All text returned by the model, before provider-selected text is parsed.</summary>
    public string ObjectText { get; }

    /// <summary>Reasoning returned by the model.</summary>
    public string? Reasoning { get; }

    /// <summary>Finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Token usage.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Provider warnings.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Request metadata.</summary>
    public LanguageModelRequestMetadata Request { get; }

    /// <summary>Response metadata, including the raw response body.</summary>
    public StepResponse Response { get; }

    /// <summary>Provider-specific response metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Event sent after the generated object has been parsed and validated.</summary>
public sealed class GenerateObjectEndEvent<T> : GenerateObjectEvent
{
    internal GenerateObjectEndEvent(string callId, string provider, string modelId, GenerateObjectResult<T> result)
        : base(callId, provider, modelId)
    {
        Object = result.Object;
        Reasoning = result.Reasoning;
        FinishReason = result.FinishReason;
        Usage = result.Usage;
        Warnings = result.Warnings;
        Request = result.Request;
        Response = result.Response;
        ProviderMetadata = result.ProviderMetadata;
    }

    /// <summary>The parsed object.</summary>
    public T Object { get; }

    /// <summary>Reasoning returned by the model.</summary>
    public string? Reasoning { get; }

    /// <summary>Finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Token usage.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Provider warnings.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Request metadata.</summary>
    public LanguageModelRequestMetadata Request { get; }

    /// <summary>Response metadata.</summary>
    public StepResponse Response { get; }

    /// <summary>Provider-specific response metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Result of a <c>generateObject</c> call.</summary>
public sealed class GenerateObjectResult<T>
{
    internal GenerateObjectResult(T value, GenerateTextResult result)
    {
        Object = value;
        Reasoning = result.ReasoningText;
        FinishReason = result.FinishReason;
        Usage = result.Usage;
        Warnings = result.Warnings;
        Request = result.Request;
        Response = result.Response;
        ProviderMetadata = result.ProviderMetadata;
    }

    /// <summary>The generated object.</summary>
    public T Object { get; }

    /// <summary>Reasoning returned by the model.</summary>
    public string? Reasoning { get; }

    /// <summary>Finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Token usage.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Provider warnings.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Request metadata.</summary>
    public LanguageModelRequestMetadata Request { get; }

    /// <summary>Response metadata, including the raw response body.</summary>
    public StepResponse Response { get; }

    /// <summary>Provider-specific response metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}
