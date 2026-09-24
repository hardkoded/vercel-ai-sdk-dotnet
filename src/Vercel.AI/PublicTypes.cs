// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI;

/// <summary>A tool the model can call. Maps to <c>tool()</c>.</summary>
public sealed class Tool
{
    /// <summary>Creates a function tool.</summary>
    public Tool(string name, string? description, JsonElement inputSchema, Func<JsonElement, CancellationToken, Task<string>>? execute)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        InputSchema = inputSchema;
        Execute = execute;
    }

    /// <summary>Tool name.</summary>
    public string Name { get; }

    /// <summary>Tool description.</summary>
    public string? Description { get; }

    /// <summary>JSON Schema for the arguments.</summary>
    public JsonElement InputSchema { get; }

    /// <summary>Executes the tool and returns a JSON string. Null means the call is not executed locally.</summary>
    public Func<JsonElement, CancellationToken, Task<string>>? Execute { get; }

    /// <summary>Creates a tool from a JSON schema string.</summary>
    public static Tool Function(string name, string? description, string jsonSchema, Func<JsonElement, CancellationToken, Task<string>>? execute)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(jsonSchema) ? "{}" : jsonSchema);
        return new Tool(name, description, document.RootElement.Clone(), execute);
    }
}

/// <summary>A tool call the caller may approve before it runs.</summary>
public sealed class ToolApprovalRequest
{
    /// <summary>Creates an approval request.</summary>
    public ToolApprovalRequest(string toolCallId, string toolName, string argumentsJson)
    {
        ToolCallId = toolCallId;
        ToolName = toolName;
        ArgumentsJson = argumentsJson;
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON arguments.</summary>
    public string ArgumentsJson { get; }
}

/// <summary>The result of executing a tool.</summary>
public sealed class ExecutedTool
{
    /// <summary>Creates a tool result.</summary>
    public ExecutedTool(string toolCallId, string toolName, string outputJson, bool isError)
    {
        ToolCallId = toolCallId;
        ToolName = toolName;
        OutputJson = outputJson;
        IsError = isError;
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON output.</summary>
    public string OutputJson { get; }

    /// <summary>Whether execution failed or was denied.</summary>
    public bool IsError { get; }
}

/// <summary>One language-model step, including any tool results from that step.</summary>
public sealed class StepResult
{
    /// <summary>Creates a step result.</summary>
    public StepResult(
        string text,
        string? reasoningText,
        IReadOnlyList<GeneratedToolCall> toolCalls,
        IReadOnlyList<ExecutedTool> toolResults,
        FinishReason finishReason,
        LanguageModelUsage usage,
        IReadOnlyList<GeneratedSource> sources)
    {
        Text = text ?? string.Empty;
        ReasoningText = reasoningText;
        ToolCalls = toolCalls ?? Array.Empty<GeneratedToolCall>();
        ToolResults = toolResults ?? Array.Empty<ExecutedTool>();
        FinishReason = finishReason;
        Usage = usage ?? LanguageModelUsage.Empty;
        Sources = sources ?? Array.Empty<GeneratedSource>();
    }

    /// <summary>Text generated in this step.</summary>
    public string Text { get; }

    /// <summary>Reasoning text generated in this step.</summary>
    public string? ReasoningText { get; }

    /// <summary>Tool calls from this step.</summary>
    public IReadOnlyList<GeneratedToolCall> ToolCalls { get; }

    /// <summary>Tool results from this step.</summary>
    public IReadOnlyList<ExecutedTool> ToolResults { get; }

    /// <summary>Finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Usage for this step.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Sources cited in this step.</summary>
    public IReadOnlyList<GeneratedSource> Sources { get; }
}

/// <summary>Stops the tool loop. The default for <c>generateText</c> is <see cref="StopWhen.IsStepCount"/> of 1.</summary>
public abstract class StopCondition
{
    /// <summary>Returns true when the loop should stop after <paramref name="steps"/>.</summary>
    public abstract bool ShouldStop(IReadOnlyList<StepResult> steps);
}

/// <summary>Stop-condition factories. Maps to <c>isStepCount</c>, <c>hasToolCall</c>, and <c>isLoopFinished</c>.</summary>
public static class StopWhen
{
    /// <summary>Stops after <paramref name="count"/> steps.</summary>
    public static StopCondition IsStepCount(int count)
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        return new StepCountCondition(count);
    }

    /// <summary>Stops when any of <paramref name="toolNames"/> was called on the latest step.</summary>
    public static StopCondition HasToolCall(params string[] toolNames)
    {
        return new HasToolCallCondition(toolNames ?? Array.Empty<string>());
    }

    /// <summary>Stops when the latest step did not call a tool.</summary>
    public static StopCondition IsLoopFinished()
    {
        return new LoopFinishedCondition();
    }

    private sealed class StepCountCondition : StopCondition
    {
        private readonly int _count;

        public StepCountCondition(int count)
        {
            _count = count;
        }

        public override bool ShouldStop(IReadOnlyList<StepResult> steps)
        {
            return steps.Count >= _count;
        }
    }

    private sealed class HasToolCallCondition : StopCondition
    {
        private readonly string[] _names;

        public HasToolCallCondition(string[] names)
        {
            _names = names;
        }

        public override bool ShouldStop(IReadOnlyList<StepResult> steps)
        {
            if (steps.Count == 0)
            {
                return false;
            }

            var latest = steps[steps.Count - 1];
            foreach (var call in latest.ToolCalls)
            {
                foreach (var name in _names)
                {
                    if (string.Equals(call.ToolName, name, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    private sealed class LoopFinishedCondition : StopCondition
    {
        public override bool ShouldStop(IReadOnlyList<StepResult> steps)
        {
            return steps.Count > 0 && steps[steps.Count - 1].ToolCalls.Count == 0;
        }
    }
}

/// <summary>Structured output selector. Maps to <c>Output</c>.</summary>
public abstract class OutputSpec
{
    /// <summary>Plain text output.</summary>
    public static OutputSpec Text()
    {
        return TextOutput.Instance;
    }

    /// <summary>JSON object output validated against <paramref name="schema"/>.</summary>
    public static OutputSpec Object(JsonElement schema, string? name = null)
    {
        return new ObjectOutput(schema, name);
    }

    /// <summary>JSON object output validated against a schema string.</summary>
    public static OutputSpec Object(string jsonSchema, string? name = null)
    {
        using var document = JsonDocument.Parse(jsonSchema);
        return new ObjectOutput(document.RootElement.Clone(), name);
    }

    internal virtual JsonElement? Schema => null;

    internal virtual string? SchemaName => null;

    private sealed class TextOutput : OutputSpec
    {
        public static readonly TextOutput Instance = new();
    }

    private sealed class ObjectOutput : OutputSpec
    {
        public ObjectOutput(JsonElement schema, string? name)
        {
            SchemaValue = schema;
            Name = name;
        }

        public JsonElement SchemaValue { get; }

        public string? Name { get; }

        internal override JsonElement? Schema => SchemaValue;

        internal override string? SchemaName => Name;
    }
}

/// <summary>Context passed to <c>prepareStep</c>.</summary>
public sealed class PrepareStepContext
{
    /// <summary>Creates context.</summary>
    public PrepareStepContext(int stepNumber, IReadOnlyList<StepResult> steps)
    {
        StepNumber = stepNumber;
        Steps = steps;
    }

    /// <summary>Zero-based step index about to run.</summary>
    public int StepNumber { get; }

    /// <summary>Steps already completed.</summary>
    public IReadOnlyList<StepResult> Steps { get; }
}

/// <summary>Optional changes applied before a step.</summary>
public sealed class PrepareStepUpdate
{
    /// <summary>Replacement prompt.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; set; }

    /// <summary>Replacement model.</summary>
    public ILanguageModel? Model { get; set; }
}

/// <summary>Options for <see cref="IAiClient.GenerateTextAsync"/>. Property names follow <c>generateText</c>.</summary>
public class GenerateTextOptions
{
    /// <summary>Gateway model id such as <c>openai/gpt-4.1-mini</c>.</summary>
    public string? ModelId { get; set; }

    /// <summary>Explicit language model. Wins over <see cref="ModelId"/>.</summary>
    public ILanguageModel? Model { get; set; }

    /// <summary>System prompt. <see cref="Instructions"/> wins when both are set.</summary>
    public string? System { get; set; }

    /// <summary>System prompt. Maps to <c>instructions</c>.</summary>
    public string? Instructions { get; set; }

    /// <summary>A single user prompt.</summary>
    public string? Prompt { get; set; }

    /// <summary>Full prompt. Maps to <c>messages</c>.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; set; }

    /// <summary>Tools available to the model.</summary>
    public IReadOnlyList<Tool>? Tools { get; set; }

    /// <summary>Tool names allowed on this call. Null allows every tool.</summary>
    public IReadOnlyList<string>? ActiveTools { get; set; }

    /// <summary>Tool selection mode.</summary>
    public ToolChoice? ToolChoice { get; set; }

    /// <summary>When to stop the tool loop. Defaults to one step.</summary>
    public StopCondition? StopWhen { get; set; }

    /// <summary>Structured output.</summary>
    public OutputSpec? Output { get; set; }

    /// <summary>Maximum output tokens.</summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>Temperature.</summary>
    public double? Temperature { get; set; }

    /// <summary>Top-p.</summary>
    public double? TopP { get; set; }

    /// <summary>Top-k.</summary>
    public int? TopK { get; set; }

    /// <summary>Presence penalty.</summary>
    public double? PresencePenalty { get; set; }

    /// <summary>Frequency penalty.</summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>Stop sequences.</summary>
    public IReadOnlyList<string>? StopSequences { get; set; }

    /// <summary>Seed.</summary>
    public int? Seed { get; set; }

    /// <summary>Retries for provider calls made by this request. Providers that own their HTTP stack use their own policy.</summary>
    public int? MaxRetries { get; set; }

    /// <summary>Per-call timeout.</summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>Extra headers forwarded to the language model.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Provider-specific options.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Called before each step. Maps to <c>prepareStep</c>.</summary>
    public Func<PrepareStepContext, PrepareStepUpdate?>? PrepareStep { get; set; }

    /// <summary>Called after each step. Maps to <c>onStepEnd</c>.</summary>
    public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }

    /// <summary>Called when generation finishes. Maps to <c>onFinish</c>.</summary>
    public Func<GenerateTextResult, CancellationToken, Task>? OnFinish { get; set; }

    /// <summary>Called when generation fails. Maps to <c>onError</c>.</summary>
    public Func<Exception, CancellationToken, Task>? OnError { get; set; }

    /// <summary>Approves a tool call before it runs. A false result records an error tool result.</summary>
    public Func<ToolApprovalRequest, CancellationToken, Task<bool>>? ApproveTool { get; set; }
}

/// <summary>Options for <see cref="IAiClient.StreamTextAsync"/>.</summary>
public sealed class StreamTextOptions : GenerateTextOptions
{
}

/// <summary>Result of <c>generateText</c>.</summary>
public sealed class GenerateTextResult
{
    /// <summary>Creates a result.</summary>
    public GenerateTextResult(
        string text,
        string? reasoningText,
        IReadOnlyList<StepResult> steps,
        FinishReason finishReason,
        LanguageModelUsage usage,
        JsonElement? output,
        IReadOnlyList<GeneratedSource> sources)
    {
        Text = text ?? string.Empty;
        ReasoningText = reasoningText;
        Steps = steps ?? Array.Empty<StepResult>();
        FinishReason = finishReason;
        Usage = usage ?? LanguageModelUsage.Empty;
        Output = output;
        Sources = sources ?? Array.Empty<GeneratedSource>();
        var calls = new List<GeneratedToolCall>();
        var results = new List<ExecutedTool>();
        foreach (var step in Steps)
        {
            calls.AddRange(step.ToolCalls);
            results.AddRange(step.ToolResults);
        }

        ToolCalls = calls;
        ToolResults = results;
    }

    /// <summary>Text from the last step.</summary>
    public string Text { get; }

    /// <summary>Reasoning from the last step.</summary>
    public string? ReasoningText { get; }

    /// <summary>Every step.</summary>
    public IReadOnlyList<StepResult> Steps { get; }

    /// <summary>Finish reason of the last step.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Usage summed across steps.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Parsed structured output, when <see cref="OutputSpec.Object(JsonElement, string?)"/> was used.</summary>
    public JsonElement? Output { get; }

    /// <summary>Tool calls from every step.</summary>
    public IReadOnlyList<GeneratedToolCall> ToolCalls { get; }

    /// <summary>Tool results from every step.</summary>
    public IReadOnlyList<ExecutedTool> ToolResults { get; }

    /// <summary>Sources from every step.</summary>
    public IReadOnlyList<GeneratedSource> Sources { get; }
}

/// <summary>A part of the <c>streamText</c> full stream.</summary>
public abstract class TextStreamPart
{
    /// <summary>Creates a part.</summary>
    protected TextStreamPart(string type)
    {
        Type = type;
    }

    /// <summary>Part type.</summary>
    public string Type { get; }
}

/// <summary>Text delta.</summary>
public sealed class TextDeltaPart : TextStreamPart
{
    /// <summary>Creates a text delta.</summary>
    public TextDeltaPart(string text)
        : base("text-delta")
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Delta text.</summary>
    public string Text { get; }
}

/// <summary>Reasoning delta.</summary>
public sealed class ReasoningDeltaPart : TextStreamPart
{
    /// <summary>Creates a reasoning delta.</summary>
    public ReasoningDeltaPart(string text)
        : base("reasoning-delta")
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Delta text.</summary>
    public string Text { get; }
}

/// <summary>A tool call.</summary>
public sealed class ToolCallPart : TextStreamPart
{
    /// <summary>Creates a tool-call part.</summary>
    public ToolCallPart(GeneratedToolCall call)
        : base("tool-call")
    {
        ToolCall = call;
    }

    /// <summary>Tool call.</summary>
    public GeneratedToolCall ToolCall { get; }
}

/// <summary>A tool result.</summary>
public sealed class ToolResultPart : TextStreamPart
{
    /// <summary>Creates a tool-result part.</summary>
    public ToolResultPart(ExecutedTool result)
        : base("tool-result")
    {
        Result = result;
    }

    /// <summary>Tool result.</summary>
    public ExecutedTool Result { get; }
}

/// <summary>A cited source.</summary>
public sealed class SourcePart : TextStreamPart
{
    /// <summary>Creates a source part.</summary>
    public SourcePart(GeneratedSource source)
        : base("source")
    {
        Source = source;
    }

    /// <summary>Source.</summary>
    public GeneratedSource Source { get; }
}

/// <summary>A step finished.</summary>
public sealed class StepFinishPart : TextStreamPart
{
    /// <summary>Creates a step-finish part.</summary>
    public StepFinishPart(StepResult step)
        : base("finish-step")
    {
        Step = step;
    }

    /// <summary>Completed step.</summary>
    public StepResult Step { get; }
}

/// <summary>The stream failed.</summary>
public sealed class ErrorPart : TextStreamPart
{
    /// <summary>Creates an error part.</summary>
    public ErrorPart(string message)
        : base("error")
    {
        Message = message;
    }

    /// <summary>Error message.</summary>
    public string Message { get; }
}

/// <summary>The full stream finished.</summary>
public sealed class FinishPart : TextStreamPart
{
    /// <summary>Creates a finish part.</summary>
    public FinishPart(FinishReason finishReason, LanguageModelUsage usage)
        : base("finish")
    {
        FinishReason = finishReason;
        Usage = usage;
    }

    /// <summary>Finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Total usage.</summary>
    public LanguageModelUsage Usage { get; }
}
