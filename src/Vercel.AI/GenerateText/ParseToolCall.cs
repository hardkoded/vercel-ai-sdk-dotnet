// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateText;

/// <summary>A tool call after its input was parsed and validated. Maps to a <c>TypedToolCall</c>.</summary>
public sealed class ParsedToolCall
{
    /// <summary>Creates a parsed tool call.</summary>
    public ParsedToolCall(string toolCallId, string toolName, JsonElement input)
    {
        ToolCallId = toolCallId;
        ToolName = toolName;
        Input = input;
    }

    /// <summary>Provider id for this call.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>
    /// The parsed input. When <see cref="Invalid"/> is true this is the raw input, parsed when it is valid JSON
    /// and a JSON string otherwise.
    /// </summary>
    public JsonElement Input { get; private set; }

    /// <summary>
    /// True when the tool is dynamic: a dynamic tool, a provider-executed dynamic call, or an invalid call.
    /// Maps to <c>dynamic</c>.
    /// </summary>
    public bool Dynamic { get; init; }

    /// <summary>
    /// True when the call failed: its tool is unknown, its input did not parse, or the input did not match the tool's
    /// input schema. Such a call is dynamic.
    /// </summary>
    public bool Invalid { get; init; }

    /// <summary>
    /// Why the call is invalid, such as an <see cref="InvalidToolInputError"/>, a <see cref="NoSuchToolError"/>
    /// or a <see cref="ToolCallRepairError"/>.
    /// </summary>
    public Exception? Error { get; init; }

    /// <summary>Display title of the tool.</summary>
    public string? Title { get; init; }

    /// <summary>True when the provider executed the tool.</summary>
    public bool ProviderExecuted { get; init; }

    /// <summary>Provider-specific metadata of the call, as the model returned it.</summary>
    public JsonElement? ProviderMetadata { get; init; }

    /// <summary>Metadata of the tool. It stays apart from <see cref="ProviderMetadata"/>. Maps to <c>toolMetadata</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ToolMetadata { get; init; }

    internal ParsedToolCall WithInput(JsonElement input)
    {
        var copy = (ParsedToolCall)MemberwiseClone();
        copy.Input = input;
        return copy;
    }
}

/// <summary>Context passed to a <see cref="ToolCallRepairFunction"/>. Maps to the options of <c>ToolCallRepairFunction</c>.</summary>
public sealed class ToolCallRepairContext
{
    /// <summary>Creates a repair context.</summary>
    public ToolCallRepairContext(
        GeneratedToolCall toolCall,
        IReadOnlyList<Tool> tools,
        Func<string, Task<JsonElement>> inputSchema,
        AiSdkError error,
        IReadOnlyList<ModelMessage> messages,
        string? instructions,
        CancellationToken cancellationToken)
    {
        ToolCall = toolCall;
        Tools = tools;
        InputSchema = inputSchema;
        Error = error;
        Messages = messages;
        Instructions = instructions;
        CancellationToken = cancellationToken;
    }

    /// <summary>The tool call that failed to parse.</summary>
    public GeneratedToolCall ToolCall { get; }

    /// <summary>The tools that are available.</summary>
    public IReadOnlyList<Tool> Tools { get; }

    /// <summary>Returns the JSON Schema of the input of the named tool.</summary>
    public Func<string, Task<JsonElement>> InputSchema { get; }

    /// <summary>The <see cref="NoSuchToolError"/> or <see cref="InvalidToolInputError"/> raised while parsing the call.</summary>
    public AiSdkError Error { get; }

    /// <summary>The messages of the current generation step.</summary>
    public IReadOnlyList<ModelMessage> Messages { get; }

    /// <summary>The instructions given to the model.</summary>
    public string? Instructions { get; }

    /// <summary>Same as <see cref="Instructions"/>. Deprecated upstream; use <see cref="Instructions"/>.</summary>
    public string? System => Instructions;

    /// <summary>Cancels the repair. Maps to <c>abortSignal</c>.</summary>
    public CancellationToken CancellationToken { get; }
}

/// <summary>
/// Attempts to repair a tool call that failed to parse. Returns the repaired call, or null to keep the error.
/// Maps to <c>ToolCallRepairFunction</c>.
/// </summary>
public delegate Task<GeneratedToolCall?> ToolCallRepairFunction(ToolCallRepairContext context);

/// <summary>Options of <see cref="ToolCallParser.ParseToolCallAsync"/>. Maps to the options of <c>parseToolCall</c>.</summary>
public sealed class ParseToolCallOptions
{
    /// <summary>Creates options for <paramref name="toolCall"/>.</summary>
    public ParseToolCallOptions(GeneratedToolCall toolCall)
    {
        ToolCall = toolCall ?? throw new ArgumentNullException(nameof(toolCall));
    }

    /// <summary>The call to parse.</summary>
    public GeneratedToolCall ToolCall { get; }

    /// <summary>The available tools. Null means no tools were set, which differs from an empty list in the <see cref="NoSuchToolError"/> message.</summary>
    public IReadOnlyList<Tool>? Tools { get; init; }

    /// <summary>Repairs a call whose tool is unknown or whose input is invalid. Maps to <c>repairToolCall</c>.</summary>
    public ToolCallRepairFunction? RepairToolCall { get; init; }

    /// <summary>
    /// Refines the input of a valid call, keyed by tool name. A tool without an entry keeps its parsed input.
    /// Maps to <c>refineToolInput</c>.
    /// </summary>
    public IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>? RefineToolInput { get; init; }

    /// <summary>The messages of the current step. They are passed to <see cref="RepairToolCall"/>.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; init; }

    /// <summary>The instructions given to the model. They are passed to <see cref="RepairToolCall"/>.</summary>
    public string? Instructions { get; init; }
}

/// <summary>Parses the input of a tool call and validates it against the tool's input schema. Maps to <c>parseToolCall</c>.</summary>
public static class ToolCallParser
{
    private static readonly JsonElement EmptyInputSchema = ParseJson("{\"properties\":{},\"additionalProperties\":false}");

    /// <summary>
    /// Parses the call in <paramref name="options"/>. Empty input is validated as an empty object. A call that cannot be parsed
    /// comes back with <see cref="ParsedToolCall.Invalid"/> set and the failure in <see cref="ParsedToolCall.Error"/>: a
    /// <see cref="NoSuchToolError"/>, an <see cref="InvalidToolInputError"/>, or a <see cref="ToolCallRepairError"/> when
    /// <see cref="ParseToolCallOptions.RepairToolCall"/> failed. A failing repair never hides a cancellation:
    /// when <paramref name="cancellationToken"/> is cancelled the task is cancelled.
    /// </summary>
    public static async Task<ParsedToolCall> ParseToolCallAsync(ParseToolCallOptions options, CancellationToken cancellationToken = default)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var toolCall = options.ToolCall;
        try
        {
            return await ParseOrRepairAsync(options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var text = toolCall.ArgumentsJson;
            var tool = FindTool(options.Tools, toolCall.ToolName);
            return new ParsedToolCall(toolCall.ToolCallId, toolCall.ToolName, TryParseJson(text) ?? JsonString(text))
            {
                Dynamic = true,
                Invalid = true,
                Error = error,
                Title = tool?.Title,
                ProviderExecuted = toolCall.ProviderExecuted,
                ProviderMetadata = toolCall.ProviderMetadata,
                ToolMetadata = tool?.Metadata,
            };
        }
    }

    private static async Task<ParsedToolCall> ParseOrRepairAsync(ParseToolCallOptions options, CancellationToken cancellationToken)
    {
        var toolCall = options.ToolCall;
        var tools = options.Tools;

        if (tools == null)
        {
            // provider-executed dynamic tools are not part of our list of tools
            if (toolCall.ProviderExecuted && toolCall.Dynamic)
            {
                return await RefineAsync(ParseProviderExecutedDynamicToolCall(toolCall), options, cancellationToken).ConfigureAwait(false);
            }

            throw new NoSuchToolError(toolCall.ToolName);
        }

        try
        {
            return await RefineAsync(DoParseToolCall(toolCall, tools), options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception error) when (options.RepairToolCall != null && (error is NoSuchToolError || error is InvalidToolInputError))
        {
            GeneratedToolCall? repairedToolCall;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var context = new ToolCallRepairContext(
                    toolCall,
                    tools,
                    toolName =>
                    {
                        var schema = FindTool(tools, toolName)?.InputSchema ?? default;
                        return Task.FromResult(schema.ValueKind == JsonValueKind.Undefined ? EmptyInputSchema : schema);
                    },
                    (AiSdkError)error,
                    options.Messages ?? Array.Empty<ModelMessage>(),
                    options.Instructions,
                    cancellationToken);
                repairedToolCall = await WaitAsync(options.RepairToolCall!(context), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception repairError)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new ToolCallRepairError(repairError, (AiSdkError)error);
            }

            // no repaired tool call returned
            if (repairedToolCall == null)
            {
                throw;
            }

            var parsedRepairedToolCall = await RefineAsync(DoParseToolCall(repairedToolCall, tools), options, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return parsedRepairedToolCall;
        }
    }

    // Stops waiting when the token is cancelled. A repair that fails later is still observed.
    private static async Task<T> WaitAsync<T>(Task<T> task, CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled)
        {
            return await task.ConfigureAwait(false);
        }

        var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using (cancellationToken.Register(() => cancelled.TrySetResult(true)))
        {
            if (await Task.WhenAny(task, cancelled.Task).ConfigureAwait(false) != task)
            {
                _ = task.ContinueWith(
                    failed => { _ = failed.Exception; },
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
                throw new OperationCanceledException(cancellationToken);
            }
        }

        return await task.ConfigureAwait(false);
    }

    private static async Task<ParsedToolCall> RefineAsync(ParsedToolCall toolCall, ParseToolCallOptions options, CancellationToken cancellationToken)
    {
        if (options.RefineToolInput == null || !options.RefineToolInput.TryGetValue(toolCall.ToolName, out var refine) || refine == null)
        {
            return toolCall;
        }

        return toolCall.WithInput(await refine(toolCall.Input, cancellationToken).ConfigureAwait(false));
    }

    private static ParsedToolCall ParseProviderExecutedDynamicToolCall(GeneratedToolCall toolCall)
    {
        var text = toolCall.ArgumentsJson;
        JsonElement input;
        try
        {
            input = ParseJson(string.IsNullOrWhiteSpace(text) ? "{}" : text);
        }
        catch (JsonException exception)
        {
            throw new InvalidToolInputError(toolCall.ToolName, text, exception);
        }

        return new ParsedToolCall(toolCall.ToolCallId, toolCall.ToolName, input)
        {
            ProviderExecuted = true,
            Dynamic = true,
            ProviderMetadata = toolCall.ProviderMetadata,
        };
    }

    private static ParsedToolCall DoParseToolCall(GeneratedToolCall toolCall, IReadOnlyList<Tool> tools)
    {
        var tool = FindTool(tools, toolCall.ToolName);
        if (tool == null)
        {
            // provider-executed dynamic tools are not part of our list of tools
            if (toolCall.ProviderExecuted && toolCall.Dynamic)
            {
                return ParseProviderExecutedDynamicToolCall(toolCall);
            }

            var names = new List<string>(tools.Count);
            foreach (var candidate in tools)
            {
                names.Add(candidate.Name);
            }

            throw new NoSuchToolError(toolCall.ToolName, names);
        }

        // when the tool call has no arguments, we try passing an empty object to the schema
        // (many LLMs generate empty strings for tool calls with no arguments)
        var text = toolCall.ArgumentsJson;
        JsonElement input;
        try
        {
            input = ParseJson(string.IsNullOrWhiteSpace(text) ? "{}" : text);
        }
        catch (JsonException exception)
        {
            throw new InvalidToolInputError(toolCall.ToolName, text, exception);
        }

        // a provider tool's schema is a Zod object upstream, which drops the properties it does not declare before it validates
        if (tool.IsProviderDefined && tool.InputSchema.ValueKind == JsonValueKind.Object)
        {
            input = ParseJson(JsonSchemaProjection.Project(tool.InputSchema, System.Text.Json.Nodes.JsonNode.Parse(input.GetRawText()))?.ToJsonString() ?? "null");
        }

        if (!tool.IsProviderDefined || tool.InputSchema.ValueKind == JsonValueKind.Object)
        {
            var errors = ToolInputSchemaValidator.Validate(tool.InputSchema, input);
            if (errors.Count > 0)
            {
                throw new InvalidToolInputError(
                    toolCall.ToolName,
                    text,
                    new FormatException("Type validation failed: Value: " + input.GetRawText() + ".\nError message: " + string.Join("; ", errors)));
            }
        }

        return new ParsedToolCall(toolCall.ToolCallId, toolCall.ToolName, input)
        {
            Dynamic = tool.IsDynamic,
            Title = tool.Title,
            ProviderExecuted = toolCall.ProviderExecuted,
            ProviderMetadata = toolCall.ProviderMetadata,
            ToolMetadata = tool.Metadata,
        };
    }

    private static Tool? FindTool(IReadOnlyList<Tool>? tools, string toolName)
    {
        if (tools != null)
        {
            foreach (var candidate in tools)
            {
                if (string.Equals(candidate.Name, toolName, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static JsonElement ParseJson(string text)
    {
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    private static JsonElement? TryParseJson(string text)
    {
        try
        {
            return ParseJson(text);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static JsonElement JsonString(string text) => ParseJson(JsonSerializer.Serialize(text));
}
