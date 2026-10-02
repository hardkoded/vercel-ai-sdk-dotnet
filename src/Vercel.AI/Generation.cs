// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI;

internal static class Generation
{
    private const int HardStepCap = 50;

    public static List<ModelMessage> BuildPrompt(GenerateTextOptions options)
    {
        var messages = new List<ModelMessage>();
        var instructions = options.Instructions ?? options.System;
        if (!string.IsNullOrEmpty(instructions))
        {
            messages.Add(new SystemModelMessage(instructions!));
        }

        if (options.Messages != null)
        {
            messages.AddRange(options.Messages);
        }

        if (!string.IsNullOrEmpty(options.Prompt))
        {
            messages.Add(new UserModelMessage(options.Prompt!));
        }

        if (messages.Count == 0)
        {
            throw new AiSdkException("Prompt or messages are required.");
        }

        return messages;
    }

    public static LanguageModelCallOptions CallOptions(GenerateTextOptions options, IReadOnlyList<ModelMessage> prompt)
    {
        return new LanguageModelCallOptions
        {
            Prompt = prompt.ToArray(),
            MaxOutputTokens = options.MaxOutputTokens,
            Temperature = options.Temperature,
            TopP = options.TopP,
            TopK = options.TopK,
            PresencePenalty = options.PresencePenalty,
            FrequencyPenalty = options.FrequencyPenalty,
            StopSequences = options.StopSequences,
            Seed = options.Seed,
            JsonSchema = options.Output?.Schema,
            JsonSchemaName = options.Output?.SchemaName,
            Tools = SelectTools(options),
            ToolChoice = options.ToolChoice,
            Headers = options.Headers,
            ProviderOptions = options.ProviderOptions,
            Reasoning = options.Reasoning,
        };
    }

    public static async Task<GenerateTextResult> GenerateAsync(
        ILanguageModel model,
        GenerateTextOptions options,
        IAiTelemetry? telemetry,
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = telemetry?.Begin("generateText", model.ModelId);
            var messages = BuildPrompt(options);
            var steps = new List<StepResult>();
            var stop = options.StopWhen ?? StopWhen.IsStepCount(1);
            var current = model;
            while (steps.Count < HardStepCap)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var update = options.PrepareStep?.Invoke(new PrepareStepContext(steps.Count, steps));
                if (update?.Messages != null)
                {
                    messages = new List<ModelMessage>(update.Messages);
                }

                if (update?.Model != null)
                {
                    current = update.Model;
                }

                if (options.OnStepStart != null)
                {
                    await options.OnStepStart(new PrepareStepContext(steps.Count, steps), cancellationToken).ConfigureAwait(false);
                }

                var generated = await current.DoGenerateAsync(CallOptions(options, messages), cancellationToken).ConfigureAwait(false);
                var step = await FinishStepAsync(generated, options, messages, steps.Count, cancellationToken).ConfigureAwait(false);
                steps.Add(step);
                await NotifyStepAsync(options, step, cancellationToken).ConfigureAwait(false);

                if (step.ToolCalls.Count == 0 || await stop.ShouldStopAsync(steps, cancellationToken).ConfigureAwait(false))
                {
                    break;
                }
            }

            var result = ToResult(steps, options);
            if (telemetry != null && scope != null)
            {
                telemetry.OnUsage(scope, result.Usage);
                telemetry.OnFinish(scope, result.FinishReason);
            }

            if (options.OnFinish != null)
            {
                await options.OnFinish(result, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }
        catch (Exception exception) when (!IsAbort(exception))
        {
            if (options.OnError != null)
            {
                await options.OnError(exception, cancellationToken).ConfigureAwait(false);
            }

            throw;
        }
    }

    public static StreamTextResult Stream(
        ILanguageModel model,
        StreamTextOptions options,
        IAiTelemetry? telemetry,
        CancellationToken cancellationToken)
    {
        var buffer = new PartBuffer();
        var textSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reasoningSource = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var finishSource = new TaskCompletionSource<FinishReason>(TaskCreationOptions.RunContinuationsAsynchronously);
        var usageSource = new TaskCompletionSource<LanguageModelUsage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var stepsSource = new TaskCompletionSource<IReadOnlyList<StepResult>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sourcesSource = new TaskCompletionSource<IReadOnlyList<GeneratedSource>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var finalStepSource = new TaskCompletionSource<StepResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = Task.Run(() => ProduceAsync(model, options, telemetry, buffer, textSource, reasoningSource, finishSource, usageSource, stepsSource, sourcesSource, finalStepSource, cancellationToken));
        return new StreamTextResult(buffer, textSource.Task, reasoningSource.Task, finishSource.Task, usageSource.Task, stepsSource.Task, sourcesSource.Task, finalStepSource.Task);
    }

    private static async Task ProduceAsync(
        ILanguageModel model,
        StreamTextOptions options,
        IAiTelemetry? telemetry,
        PartBuffer buffer,
        TaskCompletionSource<string> textSource,
        TaskCompletionSource<string?> reasoningSource,
        TaskCompletionSource<FinishReason> finishSource,
        TaskCompletionSource<LanguageModelUsage> usageSource,
        TaskCompletionSource<IReadOnlyList<StepResult>> stepsSource,
        TaskCompletionSource<IReadOnlyList<GeneratedSource>> sourcesSource,
        TaskCompletionSource<StepResult> finalStepSource,
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = telemetry?.Begin("streamText", model.ModelId);
            var messages = BuildPrompt(options);
            var steps = new List<StepResult>();
            var stop = options.StopWhen ?? StopWhen.IsStepCount(1);
            var current = model;
            while (steps.Count < HardStepCap)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var update = options.PrepareStep?.Invoke(new PrepareStepContext(steps.Count, steps));
                if (update?.Messages != null)
                {
                    messages = new List<ModelMessage>(update.Messages);
                }

                if (update?.Model != null)
                {
                    current = update.Model;
                }

                if (options.OnStepStart != null)
                {
                    await options.OnStepStart(new PrepareStepContext(steps.Count, steps), cancellationToken).ConfigureAwait(false);
                }

                var text = new StringBuilder();
                var reasoningBlocks = new List<StringBuilder>();
                var reasoningIndex = new Dictionary<string, int>(StringComparer.Ordinal);
                var toolCalls = new List<GeneratedToolCall>();
                var sources = new List<GeneratedSource>();
                FinishReason? finish = null;
                string? rawFinish = null;
                JsonElement? providerMetadata = null;
                var usage = LanguageModelUsage.Empty;
                await foreach (var part in current.DoStreamAsync(CallOptions(options, messages), cancellationToken).ConfigureAwait(false))
                {
                    switch (part)
                    {
                        case TextDeltaStreamPart delta:
                            text.Append(delta.Delta);
                            buffer.Add(new TextDeltaPart(delta.Delta));
                            break;
                        case ReasoningDeltaStreamPart reasoningDelta:
                            if (!reasoningIndex.TryGetValue(reasoningDelta.Id, out var blockIndex))
                            {
                                blockIndex = reasoningBlocks.Count;
                                reasoningIndex[reasoningDelta.Id] = blockIndex;
                                reasoningBlocks.Add(new StringBuilder());
                            }

                            reasoningBlocks[blockIndex].Append(reasoningDelta.Delta);
                            buffer.Add(new ReasoningDeltaPart(reasoningDelta.Delta));
                            break;
                        case ToolCallStreamPart toolCall:
                            var call = new GeneratedToolCall(toolCall.ToolCallId, toolCall.ToolName, toolCall.ArgumentsJson, toolCall.ProviderMetadata);
                            toolCalls.Add(call);
                            buffer.Add(new ToolCallPart(call));
                            break;
                        case SourceStreamPart source:
                            var generatedSource = new GeneratedSource(source.Id, source.Url, source.Title, source.ProviderMetadata);
                            sources.Add(generatedSource);
                            buffer.Add(new SourcePart(generatedSource));
                            break;
                        case FinishStreamPart done:
                            finish = done.FinishReason;
                            rawFinish = done.RawFinishReason;
                            usage = done.Usage;
                            providerMetadata = done.ProviderMetadata;
                            break;
                        case ErrorStreamPart error:
                            buffer.Add(new ErrorPart(error.Message));
                            throw new AiSdkException(error.Message);
                    }
                }

                var reasoning = new StringBuilder();
                foreach (var block in reasoningBlocks)
                {
                    reasoning.Append(block);
                }

                var generated = new LanguageModelGenerateResult(
                    BuildContent(text.ToString(), reasoning.ToString(), toolCalls, sources),
                    finish ?? (toolCalls.Count > 0 ? FinishReason.ToolCalls : FinishReason.Stop),
                    usage,
                    rawFinish,
                    providerMetadata: providerMetadata);
                var step = await FinishStepAsync(generated, options, messages, steps.Count, cancellationToken).ConfigureAwait(false);
                foreach (var toolResult in step.ToolResults)
                {
                    buffer.Add(new ToolResultPart(toolResult));
                }

                buffer.Add(new StepFinishPart(step));
                steps.Add(step);
                await NotifyStepAsync(options, step, cancellationToken).ConfigureAwait(false);

                if (step.ToolCalls.Count == 0 || await stop.ShouldStopAsync(steps, cancellationToken).ConfigureAwait(false))
                {
                    break;
                }
            }

            var result = ToResult(steps, options);
            if (telemetry != null && scope != null)
            {
                telemetry.OnUsage(scope, result.Usage);
                telemetry.OnFinish(scope, result.FinishReason);
            }

            buffer.Add(new FinishPart(result.FinishReason, result.Usage));
            buffer.Complete();
            textSource.TrySetResult(result.Text);
            reasoningSource.TrySetResult(result.ReasoningText);
            finishSource.TrySetResult(result.FinishReason);
            usageSource.TrySetResult(result.Usage);
            stepsSource.TrySetResult(result.Steps);
            sourcesSource.TrySetResult(result.Sources);
            finalStepSource.TrySetResult(result.FinalStep);
            if (options.OnFinish != null)
            {
                await options.OnFinish(result, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            buffer.Fail(exception);
            textSource.TrySetException(exception);
            reasoningSource.TrySetException(exception);
            finishSource.TrySetException(exception);
            usageSource.TrySetException(exception);
            stepsSource.TrySetException(exception);
            sourcesSource.TrySetException(exception);
            finalStepSource.TrySetException(exception);
            if (options.OnError != null && !IsAbort(exception))
            {
                try
                {
                    await options.OnError(exception, CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception callback)
                {
                    buffer.Fail(callback);
                }
            }
        }
    }

    private static List<GeneratedContent> BuildContent(string text, string reasoning, List<GeneratedToolCall> toolCalls, List<GeneratedSource> sources)
    {
        var content = new List<GeneratedContent>();
        if (reasoning.Length > 0)
        {
            content.Add(new GeneratedReasoning(reasoning));
        }

        if (text.Length > 0)
        {
            content.Add(new GeneratedText(text));
        }

        content.AddRange(toolCalls);
        content.AddRange(sources);
        return content;
    }

    private static async Task NotifyStepAsync(GenerateTextOptions options, StepResult step, CancellationToken cancellationToken)
    {
        var callback = options.OnStepEnd ?? options.OnStepFinish;
        if (callback != null)
        {
            await callback(step, cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool IsAbort(Exception exception)
    {
        return exception is OperationCanceledException || exception is ApiUserAbortException;
    }

    private static async Task<StepResult> FinishStepAsync(
        LanguageModelGenerateResult generated,
        GenerateTextOptions options,
        List<ModelMessage> messages,
        int stepNumber,
        CancellationToken cancellationToken)
    {
        var toolCalls = new List<GeneratedToolCall>();
        var sources = new List<GeneratedSource>();
        string? reasoning = null;
        foreach (var part in generated.Content)
        {
            switch (part)
            {
                case GeneratedToolCall call:
                    toolCalls.Add(call);
                    break;
                case GeneratedSource source:
                    sources.Add(source);
                    break;
                case GeneratedReasoning generatedReasoning:
                    reasoning = generatedReasoning.Text;
                    break;
            }
        }

        messages.Add(new AssistantModelMessage(generated.Text, toolCalls, reasoning));
        var toolResults = new List<ExecutedTool>();
        foreach (var call in toolCalls)
        {
            var executed = await ExecuteToolAsync(call, options, cancellationToken).ConfigureAwait(false);
            toolResults.Add(executed);
            messages.Add(new ToolModelMessage(executed.ToolCallId, executed.ToolName, executed.OutputJson, executed.IsError));
        }

        return new StepResult(generated.Text, reasoning, toolCalls, toolResults, generated.FinishReason, generated.Usage, sources, generated.ProviderMetadata, stepNumber);
    }

    private static async Task<ExecutedTool> ExecuteToolAsync(GeneratedToolCall call, GenerateTextOptions options, CancellationToken cancellationToken)
    {
        Tool? tool = null;
        if (options.Tools != null)
        {
            foreach (var candidate in options.Tools)
            {
                if (string.Equals(candidate.Name, call.ToolName, StringComparison.Ordinal))
                {
                    tool = candidate;
                    break;
                }
            }
        }

        if (tool?.Execute == null)
        {
            return new ExecutedTool(call.ToolCallId, call.ToolName, "{\"error\":\"Tool has no execute function.\"}", true);
        }

        if (options.ApproveTool != null)
        {
            var approved = await options.ApproveTool(new ToolApprovalRequest(call.ToolCallId, call.ToolName, call.ArgumentsJson), cancellationToken).ConfigureAwait(false);
            if (!approved)
            {
                return new ExecutedTool(call.ToolCallId, call.ToolName, "{\"error\":\"Tool call was not approved.\"}", true);
            }
        }

        try
        {
            using var arguments = JsonDocument.Parse(string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson);
            var output = await tool.Execute(arguments.RootElement.Clone(), cancellationToken).ConfigureAwait(false);
            return new ExecutedTool(call.ToolCallId, call.ToolName, string.IsNullOrEmpty(output) ? "null" : output, false);
        }
        catch (Exception exception) when (IsAbort(exception))
        {
            throw;
        }
        catch (Exception exception)
        {
            var error = JsonSerializer.Serialize(new { error = exception.Message });
            return new ExecutedTool(call.ToolCallId, call.ToolName, error, true);
        }
    }

    private static IReadOnlyList<LanguageModelTool>? SelectTools(GenerateTextOptions options)
    {
        if (options.Tools == null || options.Tools.Count == 0)
        {
            return null;
        }

        var tools = new List<LanguageModelTool>();
        foreach (var tool in options.Tools)
        {
            if (options.ActiveTools != null && !Contains(options.ActiveTools, tool.Name))
            {
                continue;
            }

            tools.Add(new LanguageModelTool(tool.Name, tool.Description, tool.InputSchema, tool.Strict));
        }

        return tools;
    }

    private static bool Contains(IReadOnlyList<string> names, string name)
    {
        foreach (var candidate in names)
        {
            if (string.Equals(candidate, name, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static GenerateTextResult ToResult(List<StepResult> steps, GenerateTextOptions options)
    {
        var last = steps.Count == 0
            ? new StepResult(string.Empty, null, Array.Empty<GeneratedToolCall>(), Array.Empty<ExecutedTool>(), FinishReason.Other, LanguageModelUsage.Empty, Array.Empty<GeneratedSource>())
            : steps[steps.Count - 1];
        var usage = LanguageModelUsage.Empty;
        var sources = new List<GeneratedSource>();
        foreach (var step in steps)
        {
            usage = LanguageModelUsage.Add(usage, step.Usage);
            sources.AddRange(step.Sources);
        }

        JsonElement? output = null;
        if (options.Output?.Schema != null)
        {
            if (string.IsNullOrWhiteSpace(last.Text))
            {
                throw new NoObjectGeneratedException(last.Text, last.FinishReason, usage);
            }

            try
            {
                using var document = JsonDocument.Parse(last.Text);
                output = document.RootElement.Clone();
            }
            catch (JsonException exception)
            {
                throw new NoObjectGeneratedException(last.Text, last.FinishReason, usage, exception);
            }
        }

        return new GenerateTextResult(last.Text, last.ReasoningText, steps, last.FinishReason, usage, output, sources, last.ProviderMetadata, ResponseMessages(steps));
    }

    private static List<ModelMessage> ResponseMessages(List<StepResult> steps)
    {
        var messages = new List<ModelMessage>();
        foreach (var step in steps)
        {
            messages.Add(new AssistantModelMessage(step.Text, step.ToolCalls, step.ReasoningText));
            foreach (var toolResult in step.ToolResults)
            {
                messages.Add(new ToolModelMessage(toolResult.ToolCallId, toolResult.ToolName, toolResult.OutputJson, toolResult.IsError));
            }
        }

        return messages;
    }
}

internal sealed class PartBuffer
{
    private readonly List<TextStreamPart> _parts = new();
    private readonly object _gate = new();
    private readonly SemaphoreSlim _signal = new(0);
    private Exception? _error;
    private bool _completed;

    public void Add(TextStreamPart part)
    {
        lock (_gate)
        {
            _parts.Add(part);
        }

        _signal.Release();
    }

    public void Complete()
    {
        lock (_gate)
        {
            _completed = true;
        }

        _signal.Release();
    }

    public void Fail(Exception exception)
    {
        lock (_gate)
        {
            _error ??= exception;
            _completed = true;
        }

        _signal.Release();
    }

    public async IAsyncEnumerable<TextStreamPart> Read([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var index = 0;
        while (true)
        {
            TextStreamPart? part = null;
            var ready = false;
            Exception? error = null;
            var done = false;
            lock (_gate)
            {
                if (index < _parts.Count)
                {
                    part = _parts[index++];
                    ready = true;
                }
                else if (_completed)
                {
                    done = true;
                    error = _error;
                }
            }

            if (ready)
            {
                yield return part!;
                continue;
            }

            if (done)
            {
                if (error != null)
                {
                    throw error;
                }

                yield break;
            }

            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

/// <summary>Result of <c>streamText</c>. Enumerating <see cref="Stream"/> or awaiting <see cref="Text"/> consumes the same buffered events.</summary>
public sealed class StreamTextResult
{
    private readonly PartBuffer _buffer;

    internal StreamTextResult(
        PartBuffer buffer,
        Task<string> text,
        Task<string?> reasoningText,
        Task<FinishReason> finishReason,
        Task<LanguageModelUsage> usage,
        Task<IReadOnlyList<StepResult>> steps,
        Task<IReadOnlyList<GeneratedSource>> sources,
        Task<StepResult> finalStep)
    {
        _buffer = buffer;
        Text = text;
        ReasoningText = reasoningText;
        FinishReason = finishReason;
        Usage = usage;
        Steps = steps;
        Sources = sources;
        FinalStep = finalStep;
    }

    /// <summary>Text deltas only.</summary>
    public async IAsyncEnumerable<string> TextStream([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var part in _buffer.Read(cancellationToken).ConfigureAwait(false))
        {
            if (part is TextDeltaPart delta && delta.Text.Length > 0)
            {
                yield return delta.Text;
            }
        }
    }

    /// <summary>Every stream part, including tool calls, tool results, and finish.</summary>
    public IAsyncEnumerable<TextStreamPart> Stream(CancellationToken cancellationToken = default)
    {
        return _buffer.Read(cancellationToken);
    }

    /// <summary>Full text from the last step. Completes when the stream finishes.</summary>
    public Task<string> Text { get; }

    /// <summary>Reasoning from the last step. Completes when the stream finishes.</summary>
    public Task<string?> ReasoningText { get; }

    /// <summary>Finish reason. Completes when the stream finishes.</summary>
    public Task<FinishReason> FinishReason { get; }

    /// <summary>Total usage. Completes when the stream finishes.</summary>
    public Task<LanguageModelUsage> Usage { get; }

    /// <summary>Completed steps. Completes when the stream finishes.</summary>
    public Task<IReadOnlyList<StepResult>> Steps { get; }

    /// <summary>Sources from every step. Completes when the stream finishes.</summary>
    public Task<IReadOnlyList<GeneratedSource>> Sources { get; }

    /// <summary>The last completed step. Completes when the stream finishes.</summary>
    public Task<StepResult> FinalStep { get; }
}
