// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI;

/// <summary>A tool the model can call. Maps to <c>tool()</c>.</summary>
public sealed class Tool
{
    /// <summary>Creates a function tool.</summary>
    public Tool(
        string name,
        string? description,
        JsonElement inputSchema,
        Func<JsonElement, CancellationToken, Task<string>>? execute,
        bool? strict = null
    )
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        InputSchema = inputSchema;
        Execute = execute;
        Strict = strict;
    }

    /// <summary>Tool name.</summary>
    public string Name { get; }

    /// <summary>Tool description.</summary>
    public string? Description { get; }

    /// <summary>
    /// Resolves the description per call from the context of the tool and the sandbox, instead of the fixed <see cref="Description"/>.
    /// Maps to a function-valued <c>description</c>.
    /// </summary>
    public Func<Vercel.AI.Prompt.ToolDescriptionContext, string>? DescriptionFactory { get; set; }

    /// <summary>Display title of the tool. Parsed calls of the tool carry it.</summary>
    public string? Title { get; set; }

    /// <summary>Metadata of the tool, such as the name of the client that created it. Parsed calls of the tool carry it as tool metadata, apart from provider metadata.</summary>
    public IReadOnlyDictionary<string, JsonElement>? Metadata { get; set; }

    /// <summary>True for a tool whose input type is not known at compile time, such as an MCP tool. Parsed calls of the tool are dynamic. Maps to <c>dynamicTool</c>.</summary>
    public bool IsDynamic { get; set; }

    /// <summary>JSON Schema for the arguments. Undefined for a provider-defined tool declared without one.</summary>
    public JsonElement InputSchema { get; }

    /// <summary>JSON Schema for the output. <c>ValidateUIMessages</c> validates the output of a finished call with it. Maps to <c>outputSchema</c>.</summary>
    public JsonElement? OutputSchema { get; set; }

    /// <summary>Executes the tool and returns a JSON string. Null means the call is not executed locally, unless <see cref="ExecuteWithOptions"/> is set.</summary>
    public Func<JsonElement, CancellationToken, Task<string>>? Execute { get; }

    /// <summary>Responses strict mode. Null means unset, which Responses sends as false.</summary>
    public bool? Strict { get; }

    /// <summary>
    /// Executes the tool with the options of the call, such as the messages of the step. When it is set, the loop calls it
    /// instead of <see cref="Execute"/>. Maps to <c>execute(input, { toolCallId, messages, abortSignal })</c>.
    /// </summary>
    public Func<JsonElement, ToolExecutionOptions, Task<string>>? ExecuteWithOptions { get; set; }

    /// <summary>
    /// Executes the tool and streams its outputs, each a JSON string. When it is set, the loop calls it instead of <see cref="Execute"/>
    /// and <see cref="ExecuteWithOptions"/>. <c>streamText</c> emits each output as a preliminary tool result, and the last one is the final result.
    /// Maps to an async-iterable <c>execute</c>.
    /// </summary>
    public Func<
        JsonElement,
        ToolExecutionOptions,
        IAsyncEnumerable<string>
    >? ExecuteStream { get; set; }

    /// <summary>
    /// Converts the JSON output of the tool into the result sent to the model. Null sends the output as it is.
    /// Maps to <c>toModelOutput</c>.
    /// </summary>
    public Func<ToolModelOutputCall, ToolModelOutput>? ToModelOutput { get; set; }

    /// <summary>
    /// When true, a call to this tool needs approval before it runs. The step then holds a <see cref="GeneratedToolApprovalRequest"/>
    /// and the loop ends there, until the caller sends a response. <see cref="GenerateTextOptions.ToolApproval"/> takes precedence.
    /// Maps to <c>needsApproval: true</c>.
    /// </summary>
    public bool NeedsApproval { get; set; }

    /// <summary>
    /// Decides for each call, from its input, whether it needs approval. When it is set, it is used instead of
    /// <see cref="NeedsApproval"/>. Maps to the function form of <c>needsApproval</c>.
    /// </summary>
    public Func<
        JsonElement,
        ToolNeedsApprovalOptions,
        Task<bool>
    >? NeedsApprovalFunction { get; set; }

    /// <summary>
    /// Validates and normalizes the context of this tool, from <see cref="GenerateTextOptions.ToolsContext"/>, before it reaches
    /// an approval function. It returns the validated context, with any defaults applied, and throws when the context is invalid.
    /// Null passes the context through unchanged. Maps to <c>contextSchema</c>.
    /// </summary>
    public Func<JsonElement, JsonElement>? ContextSchema { get; set; }

    /// <summary>
    /// Called when the model starts generating the tool input. In <c>streamText</c> it runs on the <c>tool-input-start</c> part.
    /// Maps to <c>onInputStart</c>.
    /// </summary>
    public Func<ToolExecutionOptions, Task>? OnInputStart { get; set; }

    /// <summary>
    /// Called when a fragment of the streamed tool input is available. Only <c>streamText</c> calls it.
    /// Maps to <c>onInputDelta</c>.
    /// </summary>
    public Func<ToolInputDeltaOptions, Task>? OnInputDelta { get; set; }

    /// <summary>
    /// Called when the tool input is complete and the call can start, even when the tool has no executor. A call that failed
    /// validation does not reach it. Maps to <c>onInputAvailable</c>.
    /// </summary>
    public Func<ToolInputAvailableOptions, Task>? OnInputAvailable { get; set; }

    /// <summary>
    /// Transforms the input of a call after it matches <see cref="InputSchema"/>, as a Zod <c>.transform</c> on the input schema does.
    /// It returns the schema output, which the tool, the callbacks, and the approval check see, and throws when the input is invalid.
    /// Null passes the input through unchanged.
    /// </summary>
    public Func<JsonElement, JsonElement>? InputTransform { get; set; }

    /// <summary>
    /// Provider options of the tool, keyed by provider name, such as <c>{ "openai": { "omitFromRequestTools": true } }</c>.
    /// Maps to the tool's <c>providerOptions</c>.
    /// </summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>
    /// For a provider-defined tool: whether the provider may return the result of a call in a later step, such as code execution in programmatic tool calling.
    /// The loop then continues until the result arrives. Maps to <c>supportsDeferredResults</c>.
    /// </summary>
    public bool SupportsDeferredResults { get; set; }

    /// <summary>
    /// For a provider-defined tool: the provider-owned telemetry view of <see cref="ProviderToolArgs"/>, which hides credentials.
    /// Maps to the <c>redactArgs</c> option of a provider tool factory.
    /// </summary>
    public Func<JsonElement, JsonElement>? RedactArgs { get; set; }

    private Tool(string providerToolId, string name, JsonElement args, JsonElement? inputSchema)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ProviderToolId = providerToolId ?? throw new ArgumentNullException(nameof(providerToolId));
        ProviderToolArgs = args;
        if (inputSchema is { } schema)
        {
            InputSchema = schema;
        }
    }

    /// <summary>Provider-defined tool id (<c>&lt;provider&gt;.&lt;tool&gt;</c>), or null for a function tool.</summary>
    public string? ProviderToolId { get; }

    /// <summary>Arguments of a provider-defined tool. Undefined for a function tool.</summary>
    public JsonElement ProviderToolArgs { get; }

    /// <summary>Whether this is a provider-defined tool, which the provider executes. Maps to <c>{ type: 'provider', id, args }</c>.</summary>
    public bool IsProviderDefined => ProviderToolId != null;

    /// <summary>
    /// Declares a provider-defined tool. The provider executes it, so it has no <see cref="Execute"/>.
    /// <paramref name="args"/> must be a JSON object. Maps to <see cref="LanguageModelTool.ProviderDefined"/>.
    /// </summary>
    public static Tool ProviderDefined(string id, string name, JsonElement args)
    {
        return new Tool(id, name, args.Clone(), null);
    }

    /// <summary>
    /// Declares a provider-defined tool with an input schema. The input of a call the provider ran is parsed with the
    /// schema, so a property the schema does not declare is dropped, as a Zod object does. Maps to a provider tool's <c>inputSchema</c>.
    /// </summary>
    public static Tool ProviderDefined(
        string id,
        string name,
        JsonElement args,
        JsonElement inputSchema
    )
    {
        return new Tool(id, name, args.Clone(), inputSchema.Clone());
    }

    /// <summary>Creates a dynamic tool from a JSON schema string. Maps to <c>dynamicTool</c>.</summary>
    public static Tool DynamicTool(
        string name,
        string? description,
        string jsonSchema,
        Func<JsonElement, CancellationToken, Task<string>>? execute
    )
    {
        var tool = Function(name, description, jsonSchema, execute);
        tool.IsDynamic = true;
        return tool;
    }

    /// <summary>Creates a tool from a JSON schema string.</summary>
    public static Tool Function(
        string name,
        string? description,
        string jsonSchema,
        Func<JsonElement, CancellationToken, Task<string>>? execute,
        bool? strict = null
    )
    {
        using var document = JsonDocument.Parse(
            string.IsNullOrWhiteSpace(jsonSchema) ? "{}" : jsonSchema
        );
        return new Tool(name, description, document.RootElement.Clone(), execute, strict);
    }
}

/// <summary>The options a tool receives when it runs. Maps to the second argument of <c>execute</c>.</summary>
public class ToolExecutionOptions
{
    /// <summary>Creates the options.</summary>
    public ToolExecutionOptions(
        string toolCallId,
        IReadOnlyList<ModelMessage> messages,
        CancellationToken abortSignal
    )
    {
        ToolCallId = toolCallId;
        Messages = messages ?? Array.Empty<ModelMessage>();
        AbortSignal = abortSignal;
    }

    /// <summary>Id of the tool call.</summary>
    public string ToolCallId { get; }

    /// <summary>Messages sent to the model in the step that made the call.</summary>
    public IReadOnlyList<ModelMessage> Messages { get; }

    /// <summary>Cancellation signal of the call.</summary>
    public CancellationToken AbortSignal { get; }

    /// <summary>The sandbox environment that the tool is operating in. Maps to <c>experimental_sandbox</c>.</summary>
    public Vercel.AI.ProviderUtils.ISandboxSession? ExperimentalSandbox { get; init; }

    /// <summary>
    /// The context of this tool from <see cref="GenerateTextOptions.ToolsContext"/>, validated with <see cref="Tool.ContextSchema"/>.
    /// Maps to <c>context</c>.
    /// </summary>
    public JsonElement? Context { get; init; }
}

/// <summary>The options of <see cref="Tool.OnInputDelta"/>. Maps to <c>{ inputTextDelta } &amp; ToolExecutionOptions</c>.</summary>
public sealed class ToolInputDeltaOptions(
    string toolCallId,
    IReadOnlyList<ModelMessage> messages,
    CancellationToken abortSignal,
    string inputTextDelta
) : ToolExecutionOptions(toolCallId, messages, abortSignal)
{
    /// <summary>The fragment of the tool input text. Maps to <c>inputTextDelta</c>.</summary>
    public string InputTextDelta { get; } = inputTextDelta;
}

/// <summary>The options of <see cref="Tool.OnInputAvailable"/>. Maps to <c>{ input } &amp; ToolExecutionOptions</c>.</summary>
public sealed class ToolInputAvailableOptions(
    string toolCallId,
    IReadOnlyList<ModelMessage> messages,
    CancellationToken abortSignal,
    JsonElement input
) : ToolExecutionOptions(toolCallId, messages, abortSignal)
{
    /// <summary>The input of the tool call.</summary>
    public JsonElement Input { get; } = input;
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

    /// <summary>The exception the tool threw, when <see cref="IsError"/> is set by a failed execution. Maps to the error part's <c>error</c>.</summary>
    public Exception? Error { get; init; }

    /// <summary>True when the provider ran the tool. Maps to the result's <c>providerExecuted</c>.</summary>
    public bool ProviderExecuted { get; init; }

    /// <summary>Provider-specific metadata of the result, keyed by provider name.</summary>
    public JsonElement? ProviderMetadata { get; init; }

    /// <summary>The JSON input of the tool call this result answers, after any repair and refinement. Maps to the result's <c>input</c>.</summary>
    public string? InputJson { get; init; }

    /// <summary>True when the tool is dynamic: a dynamic tool, a provider-executed dynamic call, or an invalid call. Maps to the result's <c>dynamic</c>.</summary>
    public bool Dynamic { get; init; }

    /// <summary>Display title of the tool. Maps to the result's <c>title</c>.</summary>
    public string? Title { get; init; }

    /// <summary>Metadata of the tool. It stays apart from <see cref="ProviderMetadata"/>. Maps to the result's <c>toolMetadata</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ToolMetadata { get; init; }

    /// <summary>True when the result is preliminary: a tool streamed it before its final result. Maps to the result's <c>preliminary</c>.</summary>
    public bool? Preliminary { get; init; }
}

/// <summary>Information about the model that produced a step. Maps to the <c>model</c> object of <c>StepResult</c>.</summary>
public sealed class StepModel
{
    /// <summary>Creates model information.</summary>
    public StepModel(string provider, string modelId)
    {
        Provider = provider;
        ModelId = modelId;
    }

    /// <summary>The provider of the model.</summary>
    public string Provider { get; }

    /// <summary>The ID of the model.</summary>
    public string ModelId { get; }
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
        IReadOnlyList<GeneratedSource> sources,
        JsonElement? providerMetadata = null,
        IReadOnlyList<CallWarning>? warnings = null,
        IReadOnlyList<GeneratedFile>? files = null,
        string? provider = null,
        string? modelId = null,
        IReadOnlyList<GeneratedContent>? content = null,
        StepPerformance? performance = null,
        LanguageModelRequestMetadata? request = null,
        StepResponse? response = null,
        string? rawFinishReason = null
    )
    {
        Request = request ?? LanguageModelRequestMetadata.Empty;
        Response = response
            ?? new StepResponse(string.Empty, DateTimeOffset.MinValue, modelId ?? string.Empty);
        RawFinishReason = rawFinishReason;
        Provider = provider;
        ModelId = modelId;
        Performance = performance ?? StepPerformance.Empty;
        Text = text ?? string.Empty;
        ReasoningText = reasoningText;
        Content = content ?? Array.Empty<GeneratedContent>();
        ToolCalls = toolCalls ?? Array.Empty<GeneratedToolCall>();
        ToolResults = toolResults ?? Array.Empty<ExecutedTool>();
        FinishReason = finishReason;
        Usage = usage ?? LanguageModelUsage.Empty;
        Sources = sources ?? Array.Empty<GeneratedSource>();
        ProviderMetadata = providerMetadata;
        Warnings = warnings ?? Array.Empty<CallWarning>();
        Files = files ?? Array.Empty<GeneratedFile>();
    }

    /// <summary>Unique identifier of the generation call this step belongs to. Maps to <c>callId</c>.</summary>
    public string CallId { get; internal init; } = string.Empty;

    /// <summary>Zero-based index of this step. Maps to <c>stepNumber</c>.</summary>
    public int StepNumber { get; internal init; }

    /// <summary>The context of each tool in this step, keyed by tool name. Empty when none was set. Maps to <c>toolsContext</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement> ToolsContext { get; internal set; } =
        EmptyToolsContext;

    /// <summary>The user-defined runtime context of this step. Empty when none was set. Maps to <c>runtimeContext</c>.</summary>
    public IReadOnlyDictionary<string, object?> RuntimeContext { get; internal set; } =
        EmptyRuntimeContext;

    // A copy of the step with other contexts. Telemetry hands integrations the filtered contexts and leaves the original step alone.
    internal StepResult WithContexts(
        IReadOnlyDictionary<string, object?> runtimeContext,
        IReadOnlyDictionary<string, JsonElement> toolsContext
    )
    {
        var copy = (StepResult)MemberwiseClone();
        copy.RuntimeContext = runtimeContext;
        copy.ToolsContext = toolsContext;
        return copy;
    }

    // A copy of the step with other request and response metadata. Telemetry hands integrations the redacted bodies and leaves the original step alone.
    internal StepResult WithRedactedMetadata(
        LanguageModelRequestMetadata request,
        StepResponse response
    )
    {
        var copy = (StepResult)MemberwiseClone();
        copy.Request = request;
        copy.Response = response;
        return copy;
    }

    internal static IReadOnlyDictionary<string, JsonElement> EmptyToolsContext { get; } =
        new Dictionary<string, JsonElement>();

    internal static IReadOnlyDictionary<string, object?> EmptyRuntimeContext { get; } =
        new Dictionary<string, object?>();

    /// <summary>Text generated in this step.</summary>
    public string Text { get; }

    /// <summary>
    /// Experimental. The user messages that were sent with <see cref="GenerateTextOptions.ExperimentalSteering"/> and added to the input of this step.
    /// Null when the step received no steering messages. Maps to <c>experimental_steeringMessages</c>.
    /// </summary>
    public IReadOnlyList<UserModelMessage>? ExperimentalSteeringMessages { get; internal init; }

    // The model call result's StructuredOutputText: the text a JSON output parses instead of Text.
    internal string? StructuredOutputText { get; init; }

    /// <summary>
    /// Content the model generated in this step, in order, followed by the results of the client tools the loop ran. A call that failed to
    /// parse stays here as an <see cref="GeneratedToolCall.Invalid"/> call, followed by its error result.
    /// A <see cref="GeneratedText"/> carries its <see cref="GeneratedText.Citations"/>, separate from the retrieved <see cref="Sources"/>.
    /// </summary>
    public IReadOnlyList<GeneratedContent> Content { get; }

    /// <summary>Reasoning text generated in this step.</summary>
    public string? ReasoningText { get; }

    /// <summary>The reasoning parts and reasoning files generated in this step, in order. Maps to <c>reasoning</c>.</summary>
    public IReadOnlyList<GeneratedContent> Reasoning =>
        Content.Where(part => part is GeneratedReasoning or GeneratedReasoningFile).ToArray();

    /// <summary>Information about the model that produced this step. Maps to <c>model</c>.</summary>
    public StepModel Model => new(Provider ?? string.Empty, ModelId ?? string.Empty);

    /// <summary>Tool calls from this step.</summary>
    public IReadOnlyList<GeneratedToolCall> ToolCalls { get; }

    /// <summary>Tool results from this step.</summary>
    public IReadOnlyList<ExecutedTool> ToolResults { get; }

    /// <summary>Static tool calls from this step. Maps to <c>staticToolCalls</c>.</summary>
    public IReadOnlyList<GeneratedToolCall> StaticToolCalls =>
        ToolCalls.Where(call => !call.Dynamic).ToArray();

    /// <summary>Dynamic tool calls from this step. Maps to <c>dynamicToolCalls</c>.</summary>
    public IReadOnlyList<GeneratedToolCall> DynamicToolCalls =>
        ToolCalls.Where(call => call.Dynamic).ToArray();

    /// <summary>Static tool results from this step. Maps to <c>staticToolResults</c>.</summary>
    public IReadOnlyList<ExecutedTool> StaticToolResults =>
        ToolResults.Where(result => !result.Dynamic).ToArray();

    /// <summary>Dynamic tool results from this step. Maps to <c>dynamicToolResults</c>.</summary>
    public IReadOnlyList<ExecutedTool> DynamicToolResults =>
        ToolResults.Where(result => result.Dynamic).ToArray();

    /// <summary>Finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>The finish reason exactly as the provider reported it, or <see langword="null"/> when the provider reported none. Maps to <c>rawFinishReason</c>.</summary>
    public string? RawFinishReason { get; }

    /// <summary>Usage for this step.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Sources cited in this step.</summary>
    public IReadOnlyList<GeneratedSource> Sources { get; }

    /// <summary>Provider metadata for this step, including cost and native-tool counts when the provider sent them.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Warnings reported for this step.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Files generated in this step.</summary>
    public IReadOnlyList<GeneratedFile> Files { get; }

    /// <summary>Provider id of the model that produced this step, when known.</summary>
    public string? Provider { get; }

    /// <summary>Model id of the model that produced this step, when known.</summary>
    public string? ModelId { get; }

    /// <summary>Performance metrics of this step. Streaming-only metrics are <see langword="null"/> for <c>generateText</c>.</summary>
    public StepPerformance Performance { get; }

    /// <summary>
    /// Additional request information for this step. <see cref="LanguageModelRequestMetadata.Body"/> and <see cref="LanguageModelRequestMetadata.Messages"/>
    /// are set only when <see cref="StreamTextOptions.Include"/> asks for them. Maps to <c>request</c>.
    /// </summary>
    public LanguageModelRequestMetadata Request { get; private set; }

    /// <summary>Additional response information for this step, with the messages this step added to the conversation. Maps to <c>response</c>.</summary>
    public StepResponse Response { get; private set; }
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
    /// <summary>Stops when the number of completed steps equals <paramref name="count"/>.</summary>
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

    /// <summary>
    /// The default stop condition of <see cref="Agent"/>. Stops when the step count equals <paramref name="stepCount"/>
    /// and logs a warning, because the default limit is what ended the tool loop.
    /// </summary>
    internal static StopCondition CreateDefaultStopCondition(int stepCount)
    {
        return new DefaultStopCondition(stepCount);
    }

    /// <summary>A stop condition that never stops the loop. The loop still ends when a step does not call a tool.</summary>
    public static StopCondition IsLoopFinished()
    {
        return new LoopFinishedCondition();
    }

    /// <summary>Returns true when any stop condition returns true. Maps to <c>isStopConditionMet</c>.</summary>
    public static async Task<bool> IsStopConditionMet(
        IReadOnlyList<Func<IReadOnlyList<StepResult>, Task<bool>>> stopConditions,
        IReadOnlyList<StepResult> steps
    )
    {
        if (stopConditions is null)
        {
            throw new ArgumentNullException(nameof(stopConditions));
        }

        if (steps is null)
        {
            throw new ArgumentNullException(nameof(steps));
        }

        var tasks = new Task<bool>[stopConditions.Count];
        for (var i = 0; i < stopConditions.Count; i++)
        {
            tasks[i] = stopConditions[i](steps);
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        foreach (var result in results)
        {
            if (result)
            {
                return true;
            }
        }

        return false;
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
            return steps.Count == _count;
        }
    }

    private sealed class DefaultStopCondition : StopCondition
    {
        private readonly int _stepCount;

        public DefaultStopCondition(int stepCount)
        {
            _stepCount = stepCount;
        }

        public override bool ShouldStop(IReadOnlyList<StepResult> steps)
        {
            if (steps.Count != _stepCount)
            {
                return false;
            }

            var last = steps[steps.Count - 1];
            LogWarnings.Log(
                new LogWarningsOptions(
                    new ModelWarning[]
                    {
                        new OtherWarning(
                            "The tool loop stopped because it reached the default stopWhen condition, isStepCount("
                                + _stepCount
                                + "). To allow more steps, set stopWhen to isStepCount(...) with a higher limit or provide a custom stop condition. Learn more: https://ai-sdk.dev/docs/agents/loop-control"
                        ),
                    },
                    last.Provider,
                    last.ModelId
                )
            );
            return true;
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
            _ = steps;
            return false;
        }
    }
}

/// <summary>Structured output selector. Maps to <c>Output</c>.</summary>
public abstract class OutputSpec
{
    /// <summary>Plain text output. The output type is <see cref="string"/>. Maps to <c>Output.text</c>.</summary>
    public static OutputSpec<string> Text()
    {
        return new TypedOutput<string>(TextOutput.Instance);
    }

    /// <summary>JSON object output validated against <paramref name="schema"/>.</summary>
    public static OutputSpec Object(
        JsonElement schema,
        string? name = null,
        string? description = null
    )
    {
        return new ObjectOutput(schema, name, description);
    }

    /// <summary>JSON object output validated against a schema string.</summary>
    public static OutputSpec Object(
        string jsonSchema,
        string? name = null,
        string? description = null
    )
    {
        using var document = JsonDocument.Parse(jsonSchema);
        return new ObjectOutput(document.RootElement.Clone(), name, description);
    }

    /// <summary>
    /// Array output. Each element is validated against <paramref name="elementSchema"/>. The model answers with
    /// <c>{ "elements": [...] }</c> and the output is the array of elements. Maps to <c>Output.array</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentError"><paramref name="minItems"/> or <paramref name="maxItems"/> is negative, or <paramref name="minItems"/> is greater than <paramref name="maxItems"/>.</exception>
    public static OutputSpec Array(
        JsonElement elementSchema,
        int? minItems = null,
        int? maxItems = null,
        string? name = null,
        string? description = null
    )
    {
        return new ArrayOutput(elementSchema, minItems, maxItems, name, description);
    }

    /// <summary>Array output with the element schema as a string. Maps to <c>Output.array</c>.</summary>
    /// <exception cref="InvalidArgumentError"><paramref name="minItems"/> or <paramref name="maxItems"/> is negative, or <paramref name="minItems"/> is greater than <paramref name="maxItems"/>.</exception>
    public static OutputSpec Array(
        string elementJsonSchema,
        int? minItems = null,
        int? maxItems = null,
        string? name = null,
        string? description = null
    )
    {
        using var document = JsonDocument.Parse(elementJsonSchema);
        return new ArrayOutput(document.RootElement.Clone(), minItems, maxItems, name, description);
    }

    /// <summary>Choice output. The model answers with <c>{ "result": option }</c> and the output is the option. Maps to <c>Output.choice</c>.</summary>
    public static OutputSpec<string> Choice(
        IReadOnlyList<string> options,
        string? name = null,
        string? description = null
    )
    {
        return new TypedOutput<string>(new ChoiceOutput(options, name, description));
    }

    /// <summary>Unstructured JSON output. The output is the parsed JSON value. Maps to <c>Output.json</c>.</summary>
    public static OutputSpec<JsonElement> Json(string? name = null, string? description = null)
    {
        return new TypedOutput<JsonElement>(new JsonOutput(name, description));
    }

    /// <summary>
    /// JSON object output deserialized to <typeparamref name="TOutput"/>. Maps to <c>Output.object</c> with an
    /// inferred output type.
    /// </summary>
    public static OutputSpec<TOutput> Object<TOutput>(
        JsonElement schema,
        string? name = null,
        string? description = null
    )
    {
        return new TypedOutput<TOutput>(Object(schema, name, description));
    }

    /// <summary>JSON object output from a schema string, deserialized to <typeparamref name="TOutput"/>.</summary>
    public static OutputSpec<TOutput> Object<TOutput>(
        string jsonSchema,
        string? name = null,
        string? description = null
    )
    {
        return new TypedOutput<TOutput>(Object(jsonSchema, name, description));
    }

    /// <summary>
    /// Array output whose elements are deserialized to <typeparamref name="TElement"/>. Maps to <c>Output.array</c>
    /// with an inferred element type.
    /// </summary>
    public static OutputSpec<IReadOnlyList<TElement>> Array<TElement>(
        JsonElement elementSchema,
        int? minItems = null,
        int? maxItems = null,
        string? name = null,
        string? description = null
    )
    {
        return new TypedOutput<IReadOnlyList<TElement>>(
            Array(elementSchema, minItems, maxItems, name, description)
        );
    }

    /// <summary>Array output with the element schema as a string, deserialized to <typeparamref name="TElement"/>.</summary>
    public static OutputSpec<IReadOnlyList<TElement>> Array<TElement>(
        string elementJsonSchema,
        int? minItems = null,
        int? maxItems = null,
        string? name = null,
        string? description = null
    )
    {
        return new TypedOutput<IReadOnlyList<TElement>>(
            Array(elementJsonSchema, minItems, maxItems, name, description)
        );
    }

    internal virtual JsonElement? Schema => null;

    internal virtual string? SchemaName => null;

    internal virtual string? SchemaDescription => null;

    // Whether the model is asked for JSON. Text output is not.
    internal virtual bool IsJson => Schema != null;

    // Maps to parseCompleteOutput. Invalid JSON throws JsonParseException, a mismatch throws TypeValidationException.
    internal virtual JsonElement ParseCompleteOutput(string? text)
    {
        return ProviderUtils.JsonParsing.Parse(
            text ?? "undefined",
            JsonNode.Parse(Schema!.Value.GetRawText())
        );
    }

    // Maps to parsePartialOutput. Null means undefined: there is no partial output yet.
    internal virtual JsonElement? ParsePartialOutput(string text)
    {
        if (!IsJson)
        {
            return JsonSerializer.SerializeToElement(text);
        }

        var result = Util.PartialJson.Parse(text);
        return result.State is Util.PartialJson.FailedParse or Util.PartialJson.UndefinedInput
            ? null
            : JsonSerializer.SerializeToElement(result.Value);
    }

    // Maps to createElementStreamTransform. Returns a stateful function from a partial output to the elements that are new.
    internal virtual Func<JsonElement, IEnumerable<JsonElement>>? CreateElementStreamTransform()
    {
        return null;
    }

    // Maps to createArrayStreamParser. Built-in arrays return a parser that scans only the new text of each delta. Other outputs return null.
    internal virtual Util.ArrayOutputStreamParser? CreateArrayStreamParser()
    {
        return null;
    }

    private sealed class TextOutput : OutputSpec
    {
        public static readonly TextOutput Instance = new();

        // Maps to Output.text parseCompleteOutput: the text as is, without a schema.
        internal override JsonElement ParseCompleteOutput(string? text)
        {
            // An undefined text stays undefined, like the upstream cast test expects.
            return text is null ? default : JsonSerializer.SerializeToElement(text);
        }
    }

    private sealed class ObjectOutput : OutputSpec
    {
        public ObjectOutput(JsonElement schema, string? name, string? description)
        {
            SchemaValue = schema;
            Name = name;
            Description = description;
        }

        public JsonElement SchemaValue { get; }

        public string? Name { get; }

        public string? Description { get; }

        internal override JsonElement? Schema => SchemaValue;

        internal override string? SchemaName => Name;

        internal override string? SchemaDescription => Description;
    }

    private sealed class ArrayOutput : OutputSpec
    {
        private readonly JsonElement _elementSchema;
        private readonly JsonNode _elementSchemaNode;
        private readonly int? _minItems;
        private readonly int? _maxItems;

        public ArrayOutput(
            JsonElement elementSchema,
            int? minItems,
            int? maxItems,
            string? name,
            string? description
        )
        {
            ValidateArrayBound("minItems", minItems);
            ValidateArrayBound("maxItems", maxItems);
            if (minItems > maxItems)
            {
                throw new InvalidArgumentError(
                    "minItems",
                    minItems,
                    "minItems must be less than or equal to maxItems"
                );
            }

            _elementSchema = elementSchema;
            _elementSchemaNode = JsonNode.Parse(elementSchema.GetRawText())!;
            _minItems = minItems;
            _maxItems = maxItems;
            SchemaValue = OutputStrategies.WrapArrayOutputSchema(elementSchema, minItems, maxItems);
            Name = name;
            Description = description;
        }

        public JsonElement SchemaValue { get; }

        public string? Name { get; }

        public string? Description { get; }

        internal override JsonElement? Schema => SchemaValue;

        internal override string? SchemaName => Name;

        internal override string? SchemaDescription => Description;

        internal override JsonElement ParseCompleteOutput(string? text)
        {
            var outer = ProviderUtils.JsonParsing.Parse(text ?? "undefined");
            if (
                outer.ValueKind != JsonValueKind.Object
                || !outer.TryGetProperty("elements", out var elements)
                || elements.ValueKind != JsonValueKind.Array
            )
            {
                throw new ProviderUtils.TypeValidationException(
                    "Type validation failed: response must be an object with an elements array",
                    outer
                );
            }

            var lengthError = GetArrayLengthValidationError(
                elements,
                elements.GetArrayLength(),
                _minItems,
                _maxItems
            );
            if (lengthError != null)
            {
                throw lengthError;
            }

            var validated = new List<JsonElement>();
            foreach (var element in elements.EnumerateArray())
            {
                validated.Add(ProviderUtils.JsonParsing.ValidateTypes(element, _elementSchemaNode));
            }

            return JsonSerializer.SerializeToElement(validated);
        }

        internal override JsonElement? ParsePartialOutput(string text)
        {
            var result = Util.PartialJson.Parse(text);
            if (result.State is Util.PartialJson.FailedParse or Util.PartialJson.UndefinedInput)
            {
                return null;
            }

            // No parsable elements array.
            if (
                result.Value is not JsonObject outer
                || !outer.TryGetPropertyValue("elements", out var node)
                || node is not JsonArray elements
            )
            {
                return null;
            }

            // The last element of a repaired parse is incomplete.
            var count =
                result.State == Util.PartialJson.RepairedParse && elements.Count > 0
                    ? elements.Count - 1
                    : elements.Count;
            var parsed = new List<JsonElement>();
            for (var index = 0; index < count; index++)
            {
                var raw = JsonSerializer.SerializeToElement(elements[index]);
                var validation = ProviderUtils.JsonParsing.SafeValidateTypes(
                    raw,
                    _elementSchemaNode
                );
                if (validation.Success)
                {
                    parsed.Add(validation.Value!.Value);
                }
            }

            return JsonSerializer.SerializeToElement(parsed);
        }

        internal override Util.ArrayOutputStreamParser? CreateArrayStreamParser()
        {
            return Util.ArrayOutputStream.CreateArrayOutputStreamParser(_elementSchemaNode, true);
        }

        internal override Func<
            JsonElement,
            IEnumerable<JsonElement>
        >? CreateElementStreamTransform()
        {
            var publishedElements = 0;
            return partial =>
                PublishNewElements(partial, publishedElements, count => publishedElements = count);
        }

        private IEnumerable<JsonElement> PublishNewElements(
            JsonElement partial,
            int published,
            Action<int> setPublished
        )
        {
            var length = partial.GetArrayLength();
            for (; published < length; published++)
            {
                if (_maxItems != null && published >= _maxItems)
                {
                    setPublished(published);
                    throw GetArrayLengthValidationError(partial, length, null, _maxItems)!;
                }

                yield return partial[published];
            }

            setPublished(published);
        }

        private static ProviderUtils.TypeValidationException? GetArrayLengthValidationError(
            JsonElement value,
            int length,
            int? minItems,
            int? maxItems
        )
        {
            if (minItems != null && length < minItems)
            {
                return new ProviderUtils.TypeValidationException(
                    "Type validation failed: elements array must contain at least "
                        + minItems.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        + " items",
                    value
                );
            }

            if (maxItems != null && length > maxItems)
            {
                return new ProviderUtils.TypeValidationException(
                    "Type validation failed: elements array must contain at most "
                        + maxItems.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        + " items",
                    value
                );
            }

            return null;
        }

        private static void ValidateArrayBound(string name, int? value)
        {
            if (value < 0)
            {
                throw new InvalidArgumentError(
                    name,
                    value,
                    name + " must be greater than or equal to 0"
                );
            }
        }
    }

    private sealed class ChoiceOutput : OutputSpec
    {
        private readonly string[] _options;

        public ChoiceOutput(IReadOnlyList<string> options, string? name, string? description)
        {
            _options = options.ToArray();
            Name = name;
            Description = description;
            SchemaValue = JsonSerializer.SerializeToElement(
                new JsonObject
                {
                    ["$schema"] = "http://json-schema.org/draft-07/schema#",
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["result"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["enum"] = new JsonArray(
                                _options
                                    .Select(option => (JsonNode?)JsonValue.Create(option))
                                    .ToArray()
                            ),
                        },
                    },
                    ["required"] = new JsonArray(JsonValue.Create("result")),
                    ["additionalProperties"] = false,
                }
            );
        }

        public JsonElement SchemaValue { get; }

        public string? Name { get; }

        public string? Description { get; }

        internal override JsonElement? Schema => SchemaValue;

        internal override string? SchemaName => Name;

        internal override string? SchemaDescription => Description;

        internal override JsonElement ParseCompleteOutput(string? text)
        {
            var outer = ProviderUtils.JsonParsing.Parse(text ?? "undefined");
            if (
                outer.ValueKind != JsonValueKind.Object
                || !outer.TryGetProperty("result", out var result)
                || result.ValueKind != JsonValueKind.String
                || !_options.Contains(result.GetString())
            )
            {
                throw new ProviderUtils.TypeValidationException(
                    "Type validation failed: response must be an object that contains a choice value.",
                    outer
                );
            }

            return result.Clone();
        }

        internal override JsonElement? ParsePartialOutput(string text)
        {
            var parsed = Util.PartialJson.Parse(text);
            if (parsed.State is Util.PartialJson.FailedParse or Util.PartialJson.UndefinedInput)
            {
                return null;
            }

            if (
                parsed.Value is not JsonObject outer
                || !outer.TryGetPropertyValue("result", out var node)
                || node is not JsonValue value
                || !value.TryGetValue<string>(out var result)
            )
            {
                return null;
            }

            // List of potential matches.
            var potentialMatches = _options
                .Where(option => option.StartsWith(result, StringComparison.Ordinal))
                .ToArray();
            if (parsed.State == Util.PartialJson.SuccessfulParse)
            {
                // Successful parse: exact choice value.
                return potentialMatches.Contains(result)
                    ? JsonSerializer.SerializeToElement(result)
                    : null;
            }

            // Repaired parse: only return if not ambiguous.
            return potentialMatches.Length == 1
                ? JsonSerializer.SerializeToElement(potentialMatches[0])
                : null;
        }
    }

    private sealed class JsonOutput : OutputSpec
    {
        public JsonOutput(string? name, string? description)
        {
            Name = name;
            Description = description;
        }

        public string? Name { get; }

        public string? Description { get; }

        internal override string? SchemaName => Name;

        internal override string? SchemaDescription => Description;

        internal override bool IsJson => true;

        internal override JsonElement ParseCompleteOutput(string? text)
        {
            return ProviderUtils.JsonParsing.Parse(text ?? "undefined");
        }
    }
}

/// <summary>
/// Structured output selector whose complete output is deserialized to <typeparamref name="TOutput"/>.
/// Maps to <c>Output&lt;OUTPUT&gt;</c>.
/// </summary>
/// <typeparam name="TOutput">The type of the parsed output.</typeparam>
public abstract class OutputSpec<TOutput> : OutputSpec
{
    private static readonly JsonSerializerOptions s_options = new(JsonSerializerDefaults.Web);

    internal OutputSpec() { }

    // Maps to the typed result of parseCompleteOutput. A mismatch throws JsonException.
    internal TOutput? Deserialize(JsonElement output)
    {
        return output.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
            ? default
            : output.Deserialize<TOutput>(s_options);
    }
}

// Gives a built-in output specification an output type. Every behavior is the one of the inner specification.
internal sealed class TypedOutput<TOutput>(OutputSpec inner) : OutputSpec<TOutput>
{
    internal override JsonElement? Schema => inner.Schema;

    internal override string? SchemaName => inner.SchemaName;

    internal override string? SchemaDescription => inner.SchemaDescription;

    internal override bool IsJson => inner.IsJson;

    internal override JsonElement ParseCompleteOutput(string? text)
    {
        return inner.ParseCompleteOutput(text);
    }

    internal override JsonElement? ParsePartialOutput(string text)
    {
        return inner.ParsePartialOutput(text);
    }

    internal override Func<JsonElement, IEnumerable<JsonElement>>? CreateElementStreamTransform()
    {
        return inner.CreateElementStreamTransform();
    }

    internal override Util.ArrayOutputStreamParser? CreateArrayStreamParser()
    {
        return inner.CreateArrayStreamParser();
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

    /// <summary>Creates context with the prompt and model the step is about to use.</summary>
    public PrepareStepContext(
        int stepNumber,
        IReadOnlyList<StepResult> steps,
        IReadOnlyList<ModelMessage> messages,
        ILanguageModel model
    )
        : this(stepNumber, steps)
    {
        Messages = messages;
        Model = model;
    }

    /// <summary>Creates context with the instructions and the initial and response messages of the call.</summary>
    public PrepareStepContext(
        int stepNumber,
        IReadOnlyList<StepResult> steps,
        IReadOnlyList<ModelMessage> messages,
        ILanguageModel model,
        Instructions? instructions,
        Instructions? initialInstructions,
        IReadOnlyList<ModelMessage> initialMessages,
        IReadOnlyList<ModelMessage> responseMessages
    )
        : this(stepNumber, steps, messages, model)
    {
        Instructions = instructions;
        InitialInstructions = initialInstructions;
        InitialMessages = initialMessages;
        ResponseMessages = responseMessages;
    }

    /// <summary>Zero-based step index about to run.</summary>
    public int StepNumber { get; }

    /// <summary>The messages the step is about to send, without the instructions: the prompt plus the responses of the earlier steps. A <see cref="PrepareStepUpdate.Messages"/> override carries forward to later steps. Empty when the caller built the context without them.</summary>
    public IReadOnlyList<ModelMessage> Messages { get; } = Array.Empty<ModelMessage>();

    /// <summary>The instructions the step is about to send. A <see cref="PrepareStepUpdate.Instructions"/> override carries forward to later steps. Null when there are none.</summary>
    public Instructions? Instructions { get; }

    /// <summary>The instructions the call started with. Null when there are none.</summary>
    public Instructions? InitialInstructions { get; }

    /// <summary>The messages the call started with, without the instructions. Empty when the caller built the context without them.</summary>
    public IReadOnlyList<ModelMessage> InitialMessages { get; } = Array.Empty<ModelMessage>();

    /// <summary>The response messages the earlier steps added. Empty when the caller built the context without them.</summary>
    public IReadOnlyList<ModelMessage> ResponseMessages { get; } = Array.Empty<ModelMessage>();

    /// <summary>The model the step is about to use, or null when the caller built the context without it.</summary>
    public ILanguageModel? Model { get; }

    /// <summary>Steps already completed.</summary>
    public IReadOnlyList<StepResult> Steps { get; }

    /// <summary>
    /// Experimental. The user messages that were sent with <see cref="GenerateTextOptions.ExperimentalSteering"/> and added to <see cref="Messages"/> for the current step.
    /// Null when the step received no steering messages. <see cref="Messages"/> already contains them. If you return a <see cref="PrepareStepUpdate.Messages"/>
    /// override that is rebuilt from other fields, e.g. <see cref="InitialMessages"/> and <see cref="ResponseMessages"/>, add them back together with the
    /// steering messages of previous steps from <see cref="Steps"/>. Maps to <c>experimental_steeringMessages</c>.
    /// </summary>
    public IReadOnlyList<UserModelMessage>? ExperimentalSteeringMessages { get; init; }

    /// <summary>The sandbox environment that the step is operating in. Maps to <c>experimental_sandbox</c>.</summary>
    public Vercel.AI.ProviderUtils.ISandboxSession? ExperimentalSandbox { get; init; }

    /// <summary>The context of each tool, by tool name. See <see cref="GenerateTextOptions.ToolsContext"/>. Maps to <c>toolsContext</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement> ToolsContext { get; init; } =
        StepResult.EmptyToolsContext;

    /// <summary>The user-defined runtime context. See <see cref="GenerateTextOptions.RuntimeContext"/>. Maps to <c>runtimeContext</c>.</summary>
    public IReadOnlyDictionary<string, object?> RuntimeContext { get; init; } =
        StepResult.EmptyRuntimeContext;
}

/// <summary>Optional changes applied before a step.</summary>
public sealed class PrepareStepUpdate
{
    /// <summary>Replacement instructions. The override carries forward to later steps.</summary>
    public Instructions? Instructions { get; set; }

    /// <summary>Replacement prompt, without the instructions. The override carries forward to later steps.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; set; }

    /// <summary>Replacement model.</summary>
    public ILanguageModel? Model { get; set; }

    /// <summary>Replacement tool choice for this step.</summary>
    public ToolChoice? ToolChoice { get; set; }

    /// <summary>Overrides the active tools for this step. See <see cref="GenerateTextOptions.ActiveTools"/>. Maps to <c>activeTools</c>.</summary>
    public IReadOnlyList<string>? ActiveTools { get; set; }

    /// <summary>Overrides the maximum output tokens for this step. Maps to <c>maxOutputTokens</c>.</summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>Overrides the temperature for this step. Maps to <c>temperature</c>.</summary>
    public double? Temperature { get; set; }

    /// <summary>Overrides top-p for this step. Maps to <c>topP</c>.</summary>
    public double? TopP { get; set; }

    /// <summary>Overrides top-k for this step. Maps to <c>topK</c>.</summary>
    public int? TopK { get; set; }

    /// <summary>Overrides the presence penalty for this step. Maps to <c>presencePenalty</c>.</summary>
    public double? PresencePenalty { get; set; }

    /// <summary>Overrides the frequency penalty for this step. Maps to <c>frequencyPenalty</c>.</summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>Overrides the stop sequences for this step. An empty list clears them. Maps to <c>stopSequences</c>.</summary>
    public IReadOnlyList<string>? StopSequences { get; set; }

    /// <summary>Overrides the seed for this step. Maps to <c>seed</c>.</summary>
    public int? Seed { get; set; }

    /// <summary>Overrides the reasoning effort for this step. Maps to <c>reasoning</c>.</summary>
    public string? Reasoning { get; set; }

    /// <summary>Overrides the order in which tools are sent to the provider for this step. See <see cref="GenerateTextOptions.ToolOrder"/>. Maps to <c>toolOrder</c>.</summary>
    public IReadOnlyList<string>? ToolOrder { get; set; }

    /// <summary>
    /// Provider options for this step, such as an Anthropic container id. They are deep-merged onto the call's provider options.
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>
    /// The sandbox environment that the step is operating in. Changing it affects tool execution in this step only. Maps to <c>experimental_sandbox</c>.
    /// </summary>
    public Vercel.AI.ProviderUtils.ISandboxSession? ExperimentalSandbox { get; set; }

    /// <summary>
    /// Replacement tools context. It affects this step and all later steps, and it is passed into tool execution. Maps to <c>toolsContext</c>.
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement>? ToolsContext { get; set; }

    /// <summary>
    /// Replacement runtime context. It affects this step and all later steps. Maps to <c>runtimeContext</c>.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? RuntimeContext { get; set; }
}

/// <summary>Options for <see cref="IAiClient.GenerateTextAsync"/>. Property names follow <c>generateText</c>.</summary>
public class GenerateTextOptions
{
    /// <summary>The integrations resolved once at the start of a call, so retention and every callback agree.</summary>
    internal TelemetryDispatcher? TelemetrySnapshot { get; set; }

    /// <summary>Telemetry configuration. Maps to <c>telemetry</c>.</summary>
    public TelemetryOptions? Telemetry { get; set; }

    /// <summary>Deprecated alias of <see cref="Telemetry"/>. Maps to <c>experimental_telemetry</c>.</summary>
    public TelemetryOptions? ExperimentalTelemetry { get; set; }

    /// <summary>The clock for step timing in milliseconds. Tests replace it. Maps to <c>_internal.now</c>.</summary>
    internal Func<double>? NowMs { get; set; }

    /// <summary>The clock for the step response timestamp. Tests replace it. Upstream tests freeze the system time instead.</summary>
    internal Func<DateTimeOffset>? CurrentDate { get; set; }

    /// <summary>Generates the id of the call. Maps to <c>_internal.generateCallId</c>.</summary>
    internal Func<string>? GenerateCallId { get; set; }

    /// <summary>Gives the result of the call its typed output. The call stays the same, so callbacks get the same instance.</summary>
    internal Func<GenerateTextResult, GenerateTextResult>? ResultFactory { get; set; }

    internal GenerateTextResult WrapResult(GenerateTextResult result) =>
        ResultFactory is null ? result : ResultFactory(result);

    /// <summary>A shallow copy of these options, so a caller can change one property without changing the original.</summary>
    internal GenerateTextOptions ShallowCopy() => (GenerateTextOptions)MemberwiseClone();

    /// <summary>
    /// Custom download function for URLs in the prompt and for generated file URLs returned by the model. Pass <see cref="Download.CreateDownload"/>
    /// to route them through a custom <see cref="DownloadFetch"/>. By default, prompt files are downloaded when the model does not support
    /// their URL (see <see cref="ILanguageModel.SupportedUrls"/>), and generated file URLs are downloaded with <see cref="Download"/>.
    /// The function receives one <see cref="DownloadRequestItem"/> per prompt URL, with <see cref="DownloadRequestItem.IsUrlSupportedByModel"/> set,
    /// and returns <c>null</c> for a URL the model can read itself. Generated file URLs are always downloaded, so returning <c>null</c>
    /// for one throws a <see cref="DownloadException"/>.
    /// Maps to <c>experimental_download</c>.
    /// </summary>
    public Func<
        IReadOnlyList<Vercel.AI.Util.DownloadRequestItem>,
        Task<IReadOnlyList<Vercel.AI.Util.DownloadResult>>
    >? ExperimentalDownload { get; set; }

    /// <summary>Gateway model id such as <c>openai/gpt-4.1-mini</c>.</summary>
    public string? ModelId { get; set; }

    /// <summary>Explicit language model. Wins over <see cref="ModelId"/>.</summary>
    public ILanguageModel? Model { get; set; }

    /// <summary>System prompt. <see cref="Instructions"/> wins when both are set.</summary>
    public string? System { get; set; }

    /// <summary>System prompt: a string, one <see cref="SystemModelMessage"/>, or an array of them. Maps to <c>instructions</c>.</summary>
    public Instructions? Instructions { get; set; }

    /// <summary>A single user prompt.</summary>
    public string? Prompt { get; set; }

    /// <summary>Full prompt. Maps to <c>messages</c>.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; set; }

    /// <summary>
    /// Whether system messages are allowed in <see cref="Messages"/>. Defaults to false, which rejects them with an <see cref="InvalidPromptException"/>.
    /// Use <see cref="Instructions"/> for the system prompt instead. Maps to <c>allowSystemInMessages</c>.
    /// </summary>
    public bool AllowSystemInMessages { get; set; }

    /// <summary>Tools available to the model.</summary>
    public IReadOnlyList<Tool>? Tools { get; set; }

    /// <summary>Tool names allowed on this call. Null allows every tool.</summary>
    public IReadOnlyList<string>? ActiveTools { get; set; }

    /// <summary>
    /// Controls the order in which tools are sent to the provider. The list can be partial. Tools not listed are sent after the listed tools,
    /// sorted alphabetically. This can improve provider-side caching by keeping tool definitions in a stable order. Maps to <c>toolOrder</c>.
    /// </summary>
    public IReadOnlyList<string>? ToolOrder { get; set; }

    /// <summary>
    /// Maximum number of tool calls that may execute concurrently within a step. Set to 1 to execute them one at a time in model order.
    /// Null runs every tool call at once. Values below 1 throw <see cref="InvalidArgumentError"/>. Maps to <c>toolCallConcurrency</c>.
    /// </summary>
    public int? ToolCallConcurrency { get; set; }

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

    /// <summary>
    /// Reasoning effort: <c>none</c>, <c>minimal</c>, <c>low</c>, <c>medium</c>, <c>high</c>, <c>xhigh</c>, or <c>max</c>.
    /// A provider option with the same setting wins when both are set.
    /// </summary>
    public string? Reasoning { get; set; }

    /// <summary>Retries for provider calls made by this request. Providers that own their HTTP stack use their own policy.</summary>
    public int? MaxRetries { get; set; }

    /// <summary>
    /// Aborts the call with a <see cref="TimeoutError"/> when a timeout expires. A <see cref="TimeSpan"/> sets the total timeout.
    /// <see cref="TimeoutConfiguration.StepMs"/>, <see cref="TimeoutConfiguration.FirstChunkMs"/>, and <see cref="TimeoutConfiguration.ChunkMs"/> set the others. Maps to <c>timeout</c>.
    /// </summary>
    public TimeoutConfiguration? Timeout { get; set; }

    /// <summary>Extra headers forwarded to the language model.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Provider-specific options.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Called before each step. Maps to <c>prepareStep</c>.</summary>
    public Func<PrepareStepContext, PrepareStepUpdate?>? PrepareStep { get; set; }

    /// <summary>
    /// Experimental. A steering object created with <see cref="Vercel.AI.GenerateText.Steering.ExperimentalCreateSteering"/> that adds user messages to the generation
    /// at the next step boundary. A steering object can only be used for one generation. Maps to <c>experimental_steering</c>.
    /// </summary>
    public Vercel.AI.GenerateText.ISteering? ExperimentalSteering { get; set; }

    /// <summary>The sandbox environment that is passed through to tool execution. Maps to <c>experimental_sandbox</c>.</summary>
    public Vercel.AI.ProviderUtils.ISandboxSession? ExperimentalSandbox { get; set; }

    /// <summary>Called when the generation begins, before any model call. An exception it throws is ignored. Maps to <c>onStart</c>.</summary>
    public Func<GenerateTextStartEvent, CancellationToken, Task>? OnStart { get; set; }

    /// <summary>Called when each step begins, before the provider is called. An exception it throws is ignored. Maps to <c>onStepStart</c>.</summary>
    public Func<GenerateTextStepStartEvent, CancellationToken, Task>? OnStepStart { get; set; }

    /// <summary>
    /// Called immediately before each provider model call begins. An exception it throws is ignored.
    /// Maps to <c>onLanguageModelCallStart</c>.
    /// </summary>
    public Func<
        LanguageModelCallStartEvent,
        CancellationToken,
        Task
    >? OnLanguageModelCallStart { get; set; }

    /// <summary>
    /// Called after each provider model call returns, before tools run and before <see cref="OnStepEnd"/>.
    /// Maps to <c>onLanguageModelCallEnd</c>.
    /// </summary>
    public Func<
        LanguageModelCallEndEvent,
        CancellationToken,
        Task
    >? OnLanguageModelCallEnd { get; set; }

    /// <summary>
    /// Called before a tool's <c>Execute</c> runs. It is not called for a tool without <c>Execute</c>, a provider-executed call,
    /// or a call that is invalid or not approved. An exception it throws is ignored. Maps to <c>onToolExecutionStart</c>.
    /// </summary>
    public Func<
        ToolExecutionStartEvent,
        CancellationToken,
        Task
    >? OnToolExecutionStart { get; set; }

    /// <summary>
    /// Called after a tool's <c>Execute</c> returns or throws, before <see cref="OnStepEnd"/>. An exception it throws is ignored.
    /// Maps to <c>onToolExecutionEnd</c>.
    /// </summary>
    public Func<ToolExecutionEndEvent, CancellationToken, Task>? OnToolExecutionEnd { get; set; }

    /// <summary>Called after each step. Maps to <c>onStepEnd</c>.</summary>
    public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }

    /// <summary>Deprecated alias for <see cref="OnStepEnd"/>; <see cref="OnStepEnd"/> wins when both are set. Maps to <c>onStepFinish</c>.</summary>
    [Obsolete("Use OnStepEnd instead.")]
    public Func<StepResult, CancellationToken, Task>? OnStepFinish { get; set; }

    /// <summary>Called when all steps are finished and the response is complete. Maps to <c>onEnd</c>.</summary>
    public Func<GenerateTextResult, CancellationToken, Task>? OnEnd { get; set; }

    /// <summary>Deprecated alias for <see cref="OnEnd"/>; <see cref="OnEnd"/> wins when both are set. Maps to <c>onFinish</c>.</summary>
    [Obsolete("Use OnEnd instead.")]
    public Func<GenerateTextResult, CancellationToken, Task>? OnFinish { get; set; }

    /// <summary>Called when generation fails. Maps to <c>onError</c>.</summary>
    public Func<Exception, CancellationToken, Task>? OnError { get; set; }

    internal Func<string, Exception, CancellationToken, Task<string?>>? RepairOutput { get; set; }

    internal bool GenerateObjectMode { get; set; }

    /// <summary>
    /// Configures whether tools require approval before they run: a generic function for all tool calls, or a status
    /// (or a function) for each tool. It takes precedence over <see cref="Tool.NeedsApproval"/>. Maps to <c>toolApproval</c>.
    /// </summary>
    public ToolApprovalConfiguration? ToolApproval { get; set; }

    /// <summary>
    /// The context of each tool, keyed by tool name. It is validated with the <see cref="Tool.ContextSchema"/> of the tool and then
    /// passed to the approval functions. Maps to <c>toolsContext</c>.
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement>? ToolsContext { get; set; }

    /// <summary>User-defined context that is passed to the approval functions. Maps to <c>runtimeContext</c>.</summary>
    public IReadOnlyDictionary<string, object?>? RuntimeContext { get; set; }

    /// <summary>
    /// Secret for HMAC-signing tool approval requests. When it is set, approval requests are signed when they are issued, and
    /// the signature is verified when the approval is sent back, which prevents forged approvals.
    /// Approval requests of automatically denied tool calls are not signed.
    /// Maps to <c>experimental_toolApprovalSecret</c> given as a string. See also <see cref="ExperimentalToolApprovalSecretBytes"/>.
    /// </summary>
    public string? ExperimentalToolApprovalSecret { get; set; }

    /// <summary>
    /// The same secret as <see cref="ExperimentalToolApprovalSecret"/>, given as bytes. Maps to
    /// <c>experimental_toolApprovalSecret</c> given as a <c>Uint8Array</c>. It is used when the string secret is not set.
    /// </summary>
    public byte[]? ExperimentalToolApprovalSecretBytes { get; set; }

    internal Func<string>? GenerateId { get; set; }

    /// <summary>
    /// Repairs a tool call whose tool is unknown or whose input is invalid, before the call is reported or executed.
    /// Return the repaired call, or null to keep the error. Maps to <c>repairToolCall</c>.
    /// </summary>
    public ToolCallRepairFunction? RepairToolCall { get; set; }

    /// <summary>
    /// Deprecated alias of <see cref="RepairToolCall"/>. <see cref="RepairToolCall"/> wins when both are set.
    /// Maps to <c>experimental_repairToolCall</c>.
    /// </summary>
    [Obsolete("Use RepairToolCall instead.")]
    public ToolCallRepairFunction? ExperimentalRepairToolCall { get; set; }

    /// <summary>
    /// Refines the parsed input of a tool call, keyed by tool name, before the tool runs and before outputs and callbacks see it.
    /// A tool without an entry keeps its parsed input. Maps to <c>experimental_refineToolInput</c>.
    /// </summary>
    public IReadOnlyDictionary<
        string,
        Func<JsonElement, CancellationToken, Task<JsonElement>>
    >? RefineToolInput { get; set; }
}

/// <summary>Passed to <see cref="GenerateTextOptions.OnStart"/> when the generation begins, before any model call. Maps to <c>GenerateTextStartEvent</c>.</summary>
public sealed class GenerateTextStartEvent : IOperationStartEvent
{
    /// <summary>Unique identifier for this generation call, used to correlate events. Maps to <c>callId</c>.</summary>
    public string CallId { get; init; } = string.Empty;

    /// <summary>Identifies the operation type, <c>ai.generateText</c>. Maps to <c>operationId</c>.</summary>
    public string OperationId { get; init; } = "ai.generateText";

    /// <summary>The provider identifier. Maps to <c>provider</c>.</summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>The model identifier. Maps to <c>modelId</c>.</summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>Instructions of the call. Maps to <c>instructions</c>.</summary>
    public Instructions? Instructions { get; init; }

    /// <summary>Messages of the call, without the instructions. Maps to <c>messages</c>.</summary>
    public IReadOnlyList<ModelMessage> Messages { get; init; } = [];

    /// <summary>The tools available for this generation. Maps to <c>tools</c>.</summary>
    public IReadOnlyList<Tool>? Tools { get; init; }

    /// <summary>The tool choice strategy for this generation. Maps to <c>toolChoice</c>.</summary>
    public ToolChoice? ToolChoice { get; init; }

    /// <summary>Limits which tools are available for the model to call. Maps to <c>activeTools</c>.</summary>
    public IReadOnlyList<string>? ActiveTools { get; init; }

    /// <summary>Controls the order in which tools are sent to the provider. Maps to <c>toolOrder</c>.</summary>
    public IReadOnlyList<string>? ToolOrder { get; init; }

    /// <summary>Maximum output tokens. Maps to <c>maxOutputTokens</c>.</summary>
    public int? MaxOutputTokens { get; init; }

    /// <summary>Temperature. Maps to <c>temperature</c>.</summary>
    public double? Temperature { get; init; }

    /// <summary>Top-p. Maps to <c>topP</c>.</summary>
    public double? TopP { get; init; }

    /// <summary>Top-k. Maps to <c>topK</c>.</summary>
    public int? TopK { get; init; }

    /// <summary>Presence penalty. Maps to <c>presencePenalty</c>.</summary>
    public double? PresencePenalty { get; init; }

    /// <summary>Frequency penalty. Maps to <c>frequencyPenalty</c>.</summary>
    public double? FrequencyPenalty { get; init; }

    /// <summary>Stop sequences. Maps to <c>stopSequences</c>.</summary>
    public IReadOnlyList<string>? StopSequences { get; init; }

    /// <summary>Seed. Maps to <c>seed</c>.</summary>
    public int? Seed { get; init; }

    /// <summary>Reasoning effort. Maps to <c>reasoning</c>.</summary>
    public string? Reasoning { get; init; }

    /// <summary>Maximum number of retries for failed requests. Maps to <c>maxRetries</c>.</summary>
    public int MaxRetries { get; init; }

    /// <summary>Timeout configuration for the generation. Maps to <c>timeout</c>.</summary>
    public TimeoutConfiguration? Timeout { get; init; }

    /// <summary>Additional HTTP headers sent with the request. Maps to <c>headers</c>.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; init; }

    /// <summary>Additional provider-specific options. Maps to <c>providerOptions</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }

    /// <summary>The output specification for structured outputs, if configured. Maps to <c>output</c>.</summary>
    public OutputSpec? Output { get; init; }

    /// <summary>Tool context, keyed by tool name. Maps to <c>toolsContext</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ToolsContext
    {
        get => _toolsContext;
        init => _toolsContext = value;
    }

    /// <summary>User-defined runtime context. Maps to <c>runtimeContext</c>.</summary>
    public IReadOnlyDictionary<string, object?>? RuntimeContext
    {
        get => _runtimeContext;
        init => _runtimeContext = value;
    }

    private IReadOnlyDictionary<string, JsonElement>? _toolsContext;
    private IReadOnlyDictionary<string, object?>? _runtimeContext;

    // A copy of the event with other contexts. Telemetry hands integrations the filtered contexts and leaves the original event alone.
    internal GenerateTextStartEvent WithContexts(
        IReadOnlyDictionary<string, object?> runtimeContext,
        IReadOnlyDictionary<string, JsonElement> toolsContext
    )
    {
        var copy = (GenerateTextStartEvent)MemberwiseClone();
        copy._runtimeContext = runtimeContext;
        copy._toolsContext = toolsContext;
        return copy;
    }
}

/// <summary>Passed to <see cref="GenerateTextOptions.OnStepStart"/> when a step begins, before the provider is called. Maps to <c>GenerateTextStepStartEvent</c>.</summary>
public sealed class GenerateTextStepStartEvent
{
    /// <summary>Unique identifier for this generation call, used to correlate events. Maps to <c>callId</c>.</summary>
    public string CallId { get; init; } = string.Empty;

    /// <summary>The provider identifier. Maps to <c>provider</c>.</summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>The model identifier. Maps to <c>modelId</c>.</summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>Zero-based index of the current step. Maps to <c>stepNumber</c>.</summary>
    public int StepNumber { get; init; }

    /// <summary>Instructions of the step. Maps to <c>instructions</c>.</summary>
    public Instructions? Instructions { get; init; }

    /// <summary>Messages of the step, without the instructions. Maps to <c>messages</c>.</summary>
    public IReadOnlyList<ModelMessage> Messages { get; init; } = [];

    /// <summary>The tools available for this generation. Maps to <c>tools</c>.</summary>
    public IReadOnlyList<Tool>? Tools { get; init; }

    /// <summary>The tool choice configuration for this step. Maps to <c>toolChoice</c>.</summary>
    public ToolChoice? ToolChoice { get; init; }

    /// <summary>Limits which tools are available for this step. Maps to <c>activeTools</c>.</summary>
    public IReadOnlyList<string>? ActiveTools { get; init; }

    /// <summary>Controls the order in which tools are sent to the provider for this step. Maps to <c>toolOrder</c>.</summary>
    public IReadOnlyList<string>? ToolOrder { get; init; }

    /// <summary>Results of the previous steps, empty for the first step. Maps to <c>steps</c>.</summary>
    public IReadOnlyList<StepResult> Steps
    {
        get => _steps;
        init => _steps = value;
    }

    private IReadOnlyList<StepResult> _steps = [];

    /// <summary>Additional provider-specific options for this step. Maps to <c>providerOptions</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }

    /// <summary>The output specification for structured outputs, if configured. Maps to <c>output</c>.</summary>
    public OutputSpec? Output { get; init; }

    /// <summary>User-defined runtime context. Maps to <c>runtimeContext</c>.</summary>
    public IReadOnlyDictionary<string, object?>? RuntimeContext
    {
        get => _runtimeContext;
        init => _runtimeContext = value;
    }

    /// <summary>Tool context, keyed by tool name. Maps to <c>toolsContext</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ToolsContext
    {
        get => _toolsContext;
        init => _toolsContext = value;
    }

    private IReadOnlyDictionary<string, object?>? _runtimeContext;
    private IReadOnlyDictionary<string, JsonElement>? _toolsContext;

    // A copy of the event with other contexts. Telemetry hands integrations the filtered contexts and leaves the original event alone.
    internal GenerateTextStepStartEvent WithContexts(
        IReadOnlyDictionary<string, object?> runtimeContext,
        IReadOnlyDictionary<string, JsonElement> toolsContext,
        IReadOnlyList<StepResult> steps
    )
    {
        var copy = (GenerateTextStepStartEvent)MemberwiseClone();
        copy._steps = steps;
        copy._runtimeContext = runtimeContext;
        copy._toolsContext = toolsContext;
        return copy;
    }
}

/// <summary>Passed to <see cref="GenerateTextOptions.OnLanguageModelCallStart"/> before the model is called.</summary>
public sealed class LanguageModelCallStartEvent
{
    /// <summary>Id of the model call.</summary>
    public string CallId { get; init; } = string.Empty;

    /// <summary>Provider id of the model.</summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>Model id.</summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>Instructions of the call.</summary>
    public Instructions? Instructions { get; init; }

    /// <summary>Messages of the call, without the instructions.</summary>
    public IReadOnlyList<ModelMessage> Messages { get; init; } = [];

    /// <summary>Tools offered to the model.</summary>
    public IReadOnlyList<LanguageModelTool>? Tools { get; init; }

    /// <summary>Call settings.</summary>
    public LanguageModelCallOptions CallOptions { get; init; } = new();

    /// <summary>Whether input recording was enabled for the call, as <see cref="TelemetryOptions.RecordInputs"/> set it. Set only on events sent to telemetry integrations. Maps to <c>recordInputs</c>.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Whether output recording was enabled for the call, as <see cref="TelemetryOptions.RecordOutputs"/> set it. Set only on events sent to telemetry integrations. Maps to <c>recordOutputs</c>.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>The <see cref="TelemetryOptions.FunctionId"/> of the call. Set only on events sent to telemetry integrations. Maps to <c>functionId</c>.</summary>
    public string? FunctionId { get; init; }

    /// <summary>The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to <c>aiVersion</c>.</summary>
    public string? AiVersion { get; init; }
}

/// <summary>Passed to <see cref="GenerateTextOptions.OnLanguageModelCallEnd"/> after a provider model call returns.</summary>
public sealed class LanguageModelCallEndEvent
{
    /// <summary>Creates an event.</summary>
    public LanguageModelCallEndEvent(
        string provider,
        string modelId,
        FinishReason finishReason,
        LanguageModelUsage usage,
        IReadOnlyList<GeneratedContent> content,
        string? responseId,
        JsonElement? providerMetadata,
        TimeSpan responseTime
    )
    {
        Provider = provider;
        ModelId = modelId;
        FinishReason = finishReason;
        Usage = usage;
        Content = content;
        ResponseId = responseId;
        ProviderMetadata = providerMetadata;
        ResponseTime = responseTime;
    }

    /// <summary>Provider id of the model that was called.</summary>
    public string Provider { get; }

    /// <summary>Model id the provider reported, or the requested one when it reported none.</summary>
    public string ModelId { get; }

    /// <summary>Finish reason.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Usage of this model call.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Content the model generated, including its tool calls.</summary>
    public IReadOnlyList<GeneratedContent> Content { get; }

    /// <summary>Provider response id.</summary>
    public string? ResponseId { get; }

    /// <summary>Provider metadata of the response, when the provider sent any.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Time the model call took. Maps to <c>performance.responseTimeMs</c>.</summary>
    public TimeSpan ResponseTime { get; }

    /// <summary>Id of the model call, when the call has one. Maps to <c>callId</c>.</summary>
    public string? CallId { get; init; }

    /// <summary>Request of the model call. Only <see cref="LanguageModelStreamRequest.Url"/> is set, reduced to its origin. Maps to <c>request</c>.</summary>
    public LanguageModelStreamRequest? Request { get; init; }

    /// <summary>Performance of the model call. Set for streamed calls. Maps to <c>performance</c>.</summary>
    internal Vercel.AI.GenerateText.ModelCallPerformance? Performance { get; init; }

    /// <summary>Whether input recording was enabled for the call, as <see cref="TelemetryOptions.RecordInputs"/> set it. Set only on events sent to telemetry integrations. Maps to <c>recordInputs</c>.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Whether output recording was enabled for the call, as <see cref="TelemetryOptions.RecordOutputs"/> set it. Set only on events sent to telemetry integrations. Maps to <c>recordOutputs</c>.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>The <see cref="TelemetryOptions.FunctionId"/> of the call. Set only on events sent to telemetry integrations. Maps to <c>functionId</c>.</summary>
    public string? FunctionId { get; init; }

    /// <summary>The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to <c>aiVersion</c>.</summary>
    public string? AiVersion { get; init; }
}

/// <summary>Passed to <see cref="GenerateTextOptions.OnToolExecutionStart"/> before a tool runs.</summary>
public class ToolExecutionStartEvent
{
    /// <summary>Creates an event.</summary>
    public ToolExecutionStartEvent(GeneratedToolCall toolCall, IReadOnlyList<ModelMessage> messages)
    {
        ToolCall = toolCall;
        Messages = messages;
    }

    /// <summary>Unique identifier of the generation this tool call belongs to. Maps to <c>callId</c>.</summary>
    public string? CallId { get; init; }

    /// <summary>The call about to run.</summary>
    public GeneratedToolCall ToolCall { get; }

    /// <summary>The validated context of the tool. Maps to <c>toolContext</c>.</summary>
    public JsonElement? ToolContext { get; init; }

    /// <summary>The messages the model received in the step that made the call.</summary>
    public IReadOnlyList<ModelMessage> Messages { get; }
}

/// <summary>Passed to <see cref="GenerateTextOptions.OnToolExecutionEnd"/> after a tool ran.</summary>
public sealed class ToolExecutionEndEvent : ToolExecutionStartEvent
{
    /// <summary>Creates an event.</summary>
    public ToolExecutionEndEvent(
        GeneratedToolCall toolCall,
        IReadOnlyList<ModelMessage> messages,
        ExecutedTool toolOutput,
        TimeSpan toolExecutionTime
    )
        : base(toolCall, messages)
    {
        ToolOutput = toolOutput;
        ToolExecutionTime = toolExecutionTime;
    }

    /// <summary>The result of the call. <see cref="ExecutedTool.IsError"/> is set when the tool threw.</summary>
    public ExecutedTool ToolOutput { get; }

    /// <summary>Time the tool took. Maps to <c>toolExecutionMs</c>.</summary>
    public TimeSpan ToolExecutionTime { get; }
}

/// <summary>The options a <see cref="StreamTextTransform"/> receives. Maps to the argument of <c>StreamTextTransform</c>.</summary>
public sealed class StreamTextTransformOptions
{
    /// <summary>Creates the options.</summary>
    public StreamTextTransformOptions(IReadOnlyList<Tool>? tools = null, Action? stopStream = null)
    {
        Tools = tools ?? [];
        StopStream = stopStream ?? (() => { });
    }

    /// <summary>The tools that are accessible to and can be called by the model. Maps to <c>tools</c>.</summary>
    public IReadOnlyList<Tool> Tools { get; }

    /// <summary>Stops the source stream. No further part reaches the transform after it is called. Maps to <c>stopStream</c>.</summary>
    public Action StopStream { get; }
}

/// <summary>
/// A transformation that is applied to the stream of parts. It receives the stream and returns the transformed stream.
/// The transformation must maintain the stream structure for <c>StreamTextAsync</c> to work correctly. Maps to <c>StreamTextTransform</c>.
/// </summary>
public delegate Func<
    IAsyncEnumerable<TextStreamPart>,
    CancellationToken,
    IAsyncEnumerable<TextStreamPart>
> StreamTextTransform(StreamTextTransformOptions options);

/// <summary>Options for <see cref="IAiClient.StreamTextAsync"/>.</summary>
public sealed class StreamTextOptions : GenerateTextOptions
{
    /// <summary>
    /// Optional stream transformations, applied in order. They must maintain the stream structure for <c>StreamTextAsync</c> to work correctly.
    /// Use <see cref="GenerateText.SmoothStream.Create"/> to smooth the text output. Maps to <c>experimental_transform</c>.
    /// </summary>
    public IReadOnlyList<StreamTextTransform>? ExperimentalTransform { get; set; }

    /// <summary>What to retain of the request in the <see cref="StartStepPart"/>. Maps to <c>include</c>.</summary>
    public StreamTextInclude? Include { get; set; }

    /// <summary>
    /// Aborts the call and carries the reason. Use it instead of the <see cref="CancellationToken"/> when the reason matters.
    /// A reason that is an <see cref="Exception"/> faults the results as is. Maps to <c>abortSignal</c>.
    /// </summary>
    public Vercel.AI.Util.AbortSignal? AbortSignal { get; set; }

    /// <summary>
    /// Called after the call is aborted, with the steps that finished before it. It does not delay the rejection of the
    /// results. The stream ends with an <see cref="AbortPart"/> after it returns. <see cref="GenerateTextOptions.OnError"/> and
    /// <see cref="GenerateTextOptions.OnEnd"/> are not called. Maps to <c>onAbort</c>.
    /// </summary>
    public Func<StreamAbortContext, CancellationToken, Task>? OnAbort { get; set; }

    /// <summary>
    /// Called for each part of the stream, after the <see cref="ExperimentalTransform"/> transforms. The stream processing pauses until
    /// the callback returns. Maps to <c>onChunk</c>.
    /// </summary>
    public Func<StreamTextChunkEvent, CancellationToken, Task>? OnChunk { get; set; }

    /// <summary>
    /// Maximum number of automatic retries for provider errors received after response streaming has started. Each retry reruns only
    /// the current model step. Completed earlier steps and their tool results are preserved.
    /// Partial output from a failed attempt that was already emitted cannot be retracted and remains in consumer-facing streams. It is
    /// excluded from the recovered step result, structured output parsing, response messages, and subsequent model steps.
    /// Set it to <c>0</c> to disable automatic retries while allowing <see cref="OnErrorRetry"/> to request retries. Leave it
    /// <see langword="null"/> to disable all stream retry behavior. Maps to <c>streamRetries</c>.
    /// </summary>
    public int? StreamRetries { get; set; }

    /// <summary>
    /// Like <see cref="GenerateTextOptions.OnError"/>, and its result can request one retry of the current model step after a provider
    /// error that was received from the response stream, when <see cref="StreamRetries"/> is set. It takes the place of
    /// <see cref="GenerateTextOptions.OnError"/>. C# has no union of callbacks, so the <c>onError</c> that returns <c>{ retry: true }</c> is this property.
    /// </summary>
    public Func<
        Exception,
        CancellationToken,
        Task<StreamTextOnErrorResult?>
    >? OnErrorRetry { get; set; }
}

/// <summary>The result of <see cref="StreamTextOptions.OnErrorRetry"/>. Maps to <c>{ retry?: boolean }</c>.</summary>
public sealed class StreamTextOnErrorResult
{
    /// <summary>Whether to retry the current model step. Maps to <c>retry</c>.</summary>
    public bool Retry { get; init; }
}

/// <summary>Passed to <see cref="StreamTextOptions.OnChunk"/>. Maps to the event of <c>onChunk</c>.</summary>
public sealed class StreamTextChunkEvent
{
    /// <summary>Creates the event.</summary>
    public StreamTextChunkEvent(TextStreamPart chunk)
    {
        Chunk = chunk;
    }

    /// <summary>The stream part.</summary>
    public TextStreamPart Chunk { get; }
}

/// <summary>Passed to <see cref="StreamTextOptions.OnAbort"/>.</summary>
public sealed class StreamAbortContext
{
    /// <summary>Creates a context.</summary>
    public StreamAbortContext(IReadOnlyList<StepResult> steps, Exception exception)
    {
        Steps = steps;
        Exception = exception;
    }

    /// <summary>Steps that finished before the abort.</summary>
    public IReadOnlyList<StepResult> Steps { get; }

    /// <summary>The cancellation exception, or the abort reason when it is an exception.</summary>
    public Exception Exception { get; }
}

/// <summary>Result of <c>generateText</c>.</summary>
public class GenerateTextResult : IOperationEndEvent
{
    // Copies a result. GenerateTextResult<TOutput> uses it to give the same result a typed output.
    private protected GenerateTextResult(GenerateTextResult result)
        : this(
            result.Text,
            result.ReasoningText,
            result.Steps,
            result.FinishReason,
            result.Usage,
            result.Output,
            result.Sources,
            result.ProviderMetadata
        )
    {
        ResponseMessages = result.ResponseMessages;
    }

    /// <summary>Creates a result.</summary>
    public GenerateTextResult(
        string text,
        string? reasoningText,
        IReadOnlyList<StepResult> steps,
        FinishReason finishReason,
        LanguageModelUsage usage,
        JsonElement? output,
        IReadOnlyList<GeneratedSource> sources,
        JsonElement? providerMetadata = null
    )
    {
        Text = text ?? string.Empty;
        ReasoningText = reasoningText;
        Steps = steps ?? Array.Empty<StepResult>();
        FinishReason = finishReason;
        Usage = usage ?? LanguageModelUsage.Empty;
        Output = output;
        Sources = sources ?? Array.Empty<GeneratedSource>();
        ProviderMetadata = providerMetadata;
        var calls = new List<GeneratedToolCall>();
        var results = new List<ExecutedTool>();
        foreach (var step in Steps)
        {
            calls.AddRange(step.ToolCalls);
            results.AddRange(step.ToolResults);
        }

        ToolCalls = calls;
        ToolResults = results;
        var content = new List<GeneratedContent>();
        foreach (var step in Steps)
        {
            content.AddRange(step.Content);
        }

        Content = content;
        var warnings = new List<CallWarning>();
        var files = new List<GeneratedFile>();
        foreach (var step in Steps)
        {
            warnings.AddRange(step.Warnings);
            files.AddRange(step.Files);
        }

        Warnings = warnings;
        Files = files;
    }

    /// <summary>Unique identifier of this generation call. Maps to <c>callId</c>.</summary>
    public string CallId => FinalStep.CallId;

    /// <summary>Zero-based index of the final step. Maps to <c>stepNumber</c>.</summary>
    public int StepNumber => FinalStep.StepNumber;

    /// <summary>Provider id of the model that produced the final step. Maps to <c>model.provider</c>.</summary>
    public string? Provider => FinalStep.Provider;

    /// <summary>Model id of the model that produced the final step. Maps to <c>model.modelId</c>.</summary>
    public string? ModelId => FinalStep.ModelId;

    /// <summary>Information about the model that produced the final step. Maps to <c>model</c>.</summary>
    public StepModel Model => FinalStep.Model;

    /// <summary>The reasoning parts and reasoning files of the final step. Maps to <c>reasoning</c>.</summary>
    public IReadOnlyList<GeneratedContent> Reasoning => FinalStep.Reasoning;

    /// <summary>Tool context from the final step. Maps to <c>toolsContext</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement> ToolsContext => FinalStep.ToolsContext;

    /// <summary>Runtime context from the final step. Maps to <c>runtimeContext</c>.</summary>
    public IReadOnlyDictionary<string, object?> RuntimeContext => FinalStep.RuntimeContext;

    /// <summary>Usage summed across steps. Maps to the deprecated <c>totalUsage</c>; same as <see cref="Usage"/>.</summary>
    public LanguageModelUsage TotalUsage => Usage;

    /// <summary>Static tool calls from every step. Maps to <c>staticToolCalls</c>.</summary>
    public IReadOnlyList<GeneratedToolCall> StaticToolCalls =>
        ToolCalls.Where(call => !call.Dynamic).ToArray();

    /// <summary>Dynamic tool calls from every step. Maps to <c>dynamicToolCalls</c>.</summary>
    public IReadOnlyList<GeneratedToolCall> DynamicToolCalls =>
        ToolCalls.Where(call => call.Dynamic).ToArray();

    /// <summary>Static tool results from every step. Maps to <c>staticToolResults</c>.</summary>
    public IReadOnlyList<ExecutedTool> StaticToolResults =>
        ToolResults.Where(result => !result.Dynamic).ToArray();

    /// <summary>Dynamic tool results from every step. Maps to <c>dynamicToolResults</c>.</summary>
    public IReadOnlyList<ExecutedTool> DynamicToolResults =>
        ToolResults.Where(result => result.Dynamic).ToArray();

    /// <summary>The finish reason of the final step, as the provider reported it. Maps to <c>rawFinishReason</c>.</summary>
    public string? RawFinishReason => FinalStep.RawFinishReason;

    /// <summary>The last completed step.</summary>
    public StepResult FinalStep
    {
        get
        {
            if (Steps.Count == 0)
            {
                throw new InvalidOperationException("GenerateTextResult has no steps.");
            }

            return Steps[Steps.Count - 1];
        }
    }

    // A copy of the result with other steps. Telemetry uses it to send the steps with filtered contexts.
    internal GenerateTextResult WithSteps(IReadOnlyList<StepResult> steps) =>
        new(Text, ReasoningText, steps, FinishReason, Usage, Output, Sources, ProviderMetadata)
        {
            ResponseMessages = ResponseMessages,
            RecordInputs = RecordInputs,
            RecordOutputs = RecordOutputs,
            FunctionId = FunctionId,
            AiVersion = AiVersion,
        };

    /// <summary>Text from the last step.</summary>
    public string Text { get; }

    /// <summary>Reasoning from the last step.</summary>
    public string? ReasoningText { get; }

    /// <summary>Content from every step. See <see cref="StepResult.Content"/>. Maps to <c>content</c>.</summary>
    public IReadOnlyList<GeneratedContent> Content { get; }

    /// <summary>Every step.</summary>
    public IReadOnlyList<StepResult> Steps { get; }

    /// <summary>Finish reason of the last step.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Usage summed across steps.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Parsed structured output, when <see cref="OutputSpec.Object(JsonElement, string?, string?)"/> was used.</summary>
    public JsonElement? Output { get; }

    /// <summary>Tool calls from every step.</summary>
    public IReadOnlyList<GeneratedToolCall> ToolCalls { get; }

    /// <summary>Tool results from every step.</summary>
    public IReadOnlyList<ExecutedTool> ToolResults { get; }

    /// <summary>Sources from every step.</summary>
    public IReadOnlyList<GeneratedSource> Sources { get; }

    /// <summary>Provider metadata from the last step.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Additional request information of the last step. Maps to <c>request</c>. <see cref="GenerateTextOptions"/> has no include option yet, so <c>Body</c> and <c>Messages</c> are <see langword="null"/>.</summary>
    public LanguageModelRequestMetadata Request => FinalStep.Request;

    /// <summary>Additional response information of the last step. Maps to <c>response</c>.</summary>
    public StepResponse Response => FinalStep.Response;

    /// <summary>Warnings from every step.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Files from every step.</summary>
    public IReadOnlyList<GeneratedFile> Files { get; }

    /// <summary>
    /// Messages this call added to the conversation: the assistant message of each step, with any tool approval request, and the
    /// tool results the client ran. Pass them back with the caller's approval responses to continue the conversation.
    /// </summary>
    public IReadOnlyList<ModelMessage> ResponseMessages { get; init; } =
        Array.Empty<ModelMessage>();

    /// <summary>Whether input recording was enabled for the call, as <see cref="TelemetryOptions.RecordInputs"/> set it. Set only on events sent to telemetry integrations. Maps to <c>recordInputs</c>.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Whether output recording was enabled for the call, as <see cref="TelemetryOptions.RecordOutputs"/> set it. Set only on events sent to telemetry integrations. Maps to <c>recordOutputs</c>.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>The <see cref="TelemetryOptions.FunctionId"/> of the call. Set only on events sent to telemetry integrations. Maps to <c>functionId</c>.</summary>
    public string? FunctionId { get; init; }

    /// <summary>The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to <c>aiVersion</c>.</summary>
    public string? AiVersion { get; init; }
}

/// <summary>
/// Result of <c>generateText</c> whose parsed output has the type of the <see cref="OutputSpec{TOutput}"/> of the call.
/// Maps to <c>GenerateTextResult&lt;TOOLS, OUTPUT&gt;</c>.
/// </summary>
/// <typeparam name="TOutput">The type of the parsed output.</typeparam>
public sealed class GenerateTextResult<TOutput> : GenerateTextResult
{
    private readonly Lazy<TOutput?> _output;

    internal GenerateTextResult(GenerateTextResult result, OutputSpec<TOutput> spec)
        : base(result)
    {
        // Deserialized once, so every read gives the same instance.
        _output = new Lazy<TOutput?>(() =>
            base.Output is { } json ? spec.Deserialize(json) : default
        );
    }

    /// <summary>
    /// The parsed output, or <see langword="default"/> when the call generated none, as <see cref="GenerateTextResult.Output"/>.
    /// A mismatch with <typeparamref name="TOutput"/> throws <see cref="JsonException"/>. Maps to <c>output</c>.
    /// </summary>
    public new TOutput? Output => _output.Value;
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

/// <summary>The full stream started. It is the first part of the stream.</summary>
public sealed class StartPart : TextStreamPart
{
    /// <summary>Creates a start part.</summary>
    public StartPart()
        : base("start") { }
}

/// <summary>Metadata for a language model request. Maps to <c>LanguageModelRequestMetadata</c>.</summary>
public sealed class LanguageModelRequestMetadata
{
    /// <summary>Request metadata with no messages and no body.</summary>
    public static LanguageModelRequestMetadata Empty { get; } = new();

    /// <summary>HTTP origin used for this request, without URL credentials, paths, queries or fragments. Maps to <c>url</c>.</summary>
    public string? Url { get; init; }

    /// <summary>The input messages that were sent to the model for this step.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; init; }

    /// <summary>Request HTTP body that was sent to the provider API.</summary>
    public JsonElement? Body { get; init; }

    /// <summary>Provider-owned telemetry view of <see cref="Body"/>, which hides credentials. Maps to the annotation of <c>withTelemetryRedaction</c> on the request body.</summary>
    public Func<JsonElement, JsonElement>? RedactBody { get; init; }
}

/// <summary>
/// Metadata for a language model response, as the <c>finish-step</c> part carries it.
/// Maps to <c>Omit&lt;LanguageModelResponseMetadata, 'messages' | 'body'&gt;</c>.
/// </summary>
public class LanguageModelResponseMetadata
{
    /// <summary>Creates response metadata.</summary>
    public LanguageModelResponseMetadata(
        string id,
        DateTimeOffset timestamp,
        string modelId,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        Id = id;
        Timestamp = timestamp;
        ModelId = modelId;
        Headers = headers;
    }

    /// <summary>ID for the generated response.</summary>
    public string Id { get; }

    /// <summary>Timestamp for the start of the generated response.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>The ID of the response model that was used to generate the response.</summary>
    public string ModelId { get; }

    /// <summary>Response headers. <see langword="null"/> for providers that do not use HTTP.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }
}

/// <summary>
/// Response information of one step. Maps to <c>LanguageModelResponseMetadata &amp; { messages, body? }</c> on <c>StepResult.response</c>.
/// </summary>
public sealed class StepResponse : LanguageModelResponseMetadata
{
    /// <summary>Creates step response information.</summary>
    public StepResponse(
        string id,
        DateTimeOffset timestamp,
        string modelId,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyList<ModelMessage>? messages = null,
        JsonElement? body = null
    )
        : base(id, timestamp, modelId, headers)
    {
        Messages = messages ?? Array.Empty<ModelMessage>();
        Body = body;
    }

    /// <summary>
    /// The response messages that were generated during the call: the assistant message of this step, then the tool message with
    /// the results the client ran. Maps to <c>messages</c>.
    /// </summary>
    public IReadOnlyList<ModelMessage> Messages { get; }

    /// <summary>Response body, for providers that use HTTP and report it. It is not set for streams. Maps to <c>body</c>.</summary>
    public JsonElement? Body { get; }

    /// <summary>Provider-owned telemetry view of <see cref="Body"/>, which hides credentials. Maps to the annotation of <c>withTelemetryRedaction</c> on the response body.</summary>
    public Func<JsonElement, JsonElement>? RedactBody { get; init; }
}

/// <summary>What <c>streamText</c> keeps of the request and the provider stream. Maps to <c>StreamTextInclude</c>.</summary>
public sealed class StreamTextInclude
{
    /// <summary>
    /// Whether to retain the request body in the <c>start-step</c> part. The request body can be large when sending images or files.
    /// Maps to <c>requestBody</c>. The default is <see langword="false"/>.
    /// </summary>
    public bool RequestBody { get; init; }

    /// <summary>
    /// Whether to retain the request messages in the <c>start-step</c> part. The request messages can be large when sending images or files.
    /// Maps to <c>requestMessages</c>. The default is <see langword="false"/>.
    /// </summary>
    public bool RequestMessages { get; init; }

    /// <summary>
    /// Whether to forward the provider's raw chunks as <see cref="RawPart"/> in the full stream. It asks the model to include them.
    /// Maps to <c>rawChunks</c>. The default is <see langword="false"/>.
    /// </summary>
    public bool RawChunks { get; init; }
}

/// <summary>A step started. It comes before the parts of that step.</summary>
public sealed class StartStepPart : TextStreamPart
{
    /// <summary>Creates a start-step part.</summary>
    public StartStepPart(
        IReadOnlyList<CallWarning>? warnings = null,
        LanguageModelRequestMetadata? request = null
    )
        : base("start-step")
    {
        Warnings = warnings ?? Array.Empty<CallWarning>();
        Request = request ?? LanguageModelRequestMetadata.Empty;
    }

    /// <summary>Request metadata of this step. Body and messages are set only when <see cref="StreamTextOptions.Include"/> asks for them.</summary>
    public LanguageModelRequestMetadata Request { get; }

    /// <summary>Warnings from the model call of this step.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }
}

/// <summary>A raw chunk of the provider stream. It appears only when <see cref="StreamTextInclude.RawChunks"/> is set.</summary>
public sealed class RawPart : TextStreamPart
{
    /// <summary>Creates a raw part.</summary>
    public RawPart(JsonElement rawValue)
        : base("raw")
    {
        RawValue = rawValue;
    }

    /// <summary>The raw value the provider sent. Maps to <c>rawValue</c>.</summary>
    public JsonElement RawValue { get; }
}

/// <summary>
/// A transient provider heartbeat. It contains no generated content, does not reset model-output timeouts, and is omitted from UI message streams.
/// Providers may require an explicit opt-in. Maps to <c>TextStreamPingPart</c>.
/// </summary>
public sealed class PingPart : TextStreamPart
{
    /// <summary>Creates a ping part.</summary>
    public PingPart()
        : base("ping") { }
}

/// <summary>A text block started. Maps to <c>text-start</c>.</summary>
public sealed class TextStartPart : TextStreamPart
{
    /// <summary>Creates a text start.</summary>
    public TextStartPart(string id, JsonElement? providerMetadata = null)
        : base("text-start")
    {
        Id = id;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Id of the text block.</summary>
    public string Id { get; }

    /// <summary>Provider metadata of the text block.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Text delta. Maps to <c>text-delta</c>.</summary>
public sealed class TextDeltaPart : TextStreamPart
{
    /// <summary>Creates a text delta.</summary>
    public TextDeltaPart(string id, string text, JsonElement? providerMetadata = null)
        : base("text-delta")
    {
        Id = id;
        Text = text ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Id of the text block.</summary>
    public string Id { get; }

    /// <summary>Delta text.</summary>
    public string Text { get; }

    /// <summary>Provider metadata of the delta.</summary>
    public JsonElement? ProviderMetadata { get; }

    // The partial output after this delta, for the first text block of a step with a structured output.
    internal JsonElement? PartialOutput { get; init; }
}

/// <summary>A text block ended. Maps to <c>text-end</c>.</summary>
public sealed class TextEndPart : TextStreamPart
{
    /// <summary>Creates a text end.</summary>
    public TextEndPart(
        string id,
        IReadOnlyList<Citation>? citations = null,
        JsonElement? providerMetadata = null
    )
        : base("text-end")
    {
        Id = id;
        Citations = citations;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Id of the text block.</summary>
    public string Id { get; }

    /// <summary>References that support the text block, or null when there are none.</summary>
    public IReadOnlyList<Citation>? Citations { get; }

    /// <summary>Provider metadata of the text block.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A reasoning block started. Maps to <c>reasoning-start</c>.</summary>
public sealed class ReasoningStartPart : TextStreamPart
{
    /// <summary>Creates a reasoning start.</summary>
    public ReasoningStartPart(string id, JsonElement? providerMetadata = null)
        : base("reasoning-start")
    {
        Id = id;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Id of the reasoning block.</summary>
    public string Id { get; }

    /// <summary>Provider metadata of the reasoning block.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Reasoning delta. Maps to <c>reasoning-delta</c>.</summary>
public sealed class ReasoningDeltaPart : TextStreamPart
{
    /// <summary>Creates a reasoning delta.</summary>
    public ReasoningDeltaPart(string id, string text, JsonElement? providerMetadata = null)
        : base("reasoning-delta")
    {
        Id = id;
        Text = text ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Id of the reasoning block.</summary>
    public string Id { get; }

    /// <summary>Delta text.</summary>
    public string Text { get; }

    /// <summary>Provider metadata of the delta, such as a reasoning signature.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A reasoning block ended. Maps to <c>reasoning-end</c>.</summary>
public sealed class ReasoningEndPart : TextStreamPart
{
    /// <summary>Creates a reasoning end.</summary>
    public ReasoningEndPart(string id, JsonElement? providerMetadata = null)
        : base("reasoning-end")
    {
        Id = id;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Id of the reasoning block.</summary>
    public string Id { get; }

    /// <summary>Provider metadata of the reasoning block, such as encrypted reasoning content.</summary>
    public JsonElement? ProviderMetadata { get; }
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

/// <summary>A tool call needs approval before it runs. Maps to the <c>tool-approval-request</c> stream part.</summary>
public sealed class ToolApprovalRequestPart : TextStreamPart
{
    /// <summary>Creates a tool-approval-request part.</summary>
    public ToolApprovalRequestPart(GeneratedToolApprovalRequest request)
        : base("tool-approval-request")
    {
        ApprovalId = request.ApprovalId;
        ToolCall = request.ToolCall
            ?? throw new ArgumentException("The request has no tool call.", nameof(request));
        Reason = request.Reason;
        IsAutomatic = request.IsAutomatic;
        Signature = request.Signature;
    }

    /// <summary>Approval id the response must carry.</summary>
    public string ApprovalId { get; }

    /// <summary>The tool call that needs approval.</summary>
    public GeneratedToolCall ToolCall { get; }

    /// <summary>Why the tool call needs approval.</summary>
    public string? Reason { get; }

    /// <summary>True when the request was approved or denied automatically, so nobody needs to answer it.</summary>
    public bool IsAutomatic { get; }

    /// <summary>HMAC-SHA256 signature that binds the request to its tool call.</summary>
    public string? Signature { get; }
}

/// <summary>A tool approval request was answered. Maps to the <c>tool-approval-response</c> stream part.</summary>
public sealed class ToolApprovalResponsePart : TextStreamPart
{
    /// <summary>Creates a tool-approval-response part.</summary>
    public ToolApprovalResponsePart(GeneratedToolApprovalResponse response)
        : base("tool-approval-response")
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        ApprovalId = response.ApprovalId;
        ToolCall = response.ToolCall;
        Approved = response.Approved;
        Reason = response.Reason;
        ProviderExecuted = response.ProviderExecuted;
    }

    /// <summary>Id of the approval request this answers.</summary>
    public string ApprovalId { get; }

    /// <summary>The tool call the response is for.</summary>
    public GeneratedToolCall ToolCall { get; }

    /// <summary>Whether the approval was granted or denied.</summary>
    public bool Approved { get; }

    /// <summary>Reason for the approval or denial.</summary>
    public string? Reason { get; }

    /// <summary>True when the tool call is provider-executed.</summary>
    public bool ProviderExecuted { get; }
}

/// <summary>A tool call was denied, so its tool did not run. Maps to the <c>tool-output-denied</c> stream part.</summary>
public sealed class ToolOutputDeniedPart : TextStreamPart
{
    /// <summary>Creates a tool-output-denied part.</summary>
    public ToolOutputDeniedPart(string toolCallId, string toolName)
        : base("tool-output-denied")
    {
        ToolCallId = toolCallId ?? throw new ArgumentNullException(nameof(toolCallId));
        ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
    }

    /// <summary>Id of the denied tool call.</summary>
    public string ToolCallId { get; }

    /// <summary>Name of the denied tool.</summary>
    public string ToolName { get; }
}

/// <summary>Content that only the provider understands, such as a compaction. Maps to the <c>custom</c> stream part.</summary>
public sealed class CustomPart : TextStreamPart
{
    /// <summary>Creates a custom part.</summary>
    public CustomPart(string kind, JsonElement? providerMetadata = null)
        : base("custom")
    {
        Kind = kind ?? throw new ArgumentNullException(nameof(kind));
        ProviderMetadata = providerMetadata;
    }

    /// <summary>The kind of the content, in the form <c>{provider}.{provider-type}</c>.</summary>
    public string Kind { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>
/// Experimental. A steering message that was added to the run before the step with the given number. Streamed before the output of that step.
/// Maps to the <c>steering-message</c> stream part. <c>StreamText</c> emits it when it delivers a message sent with <see cref="GenerateTextOptions.ExperimentalSteering"/>.
/// </summary>
public sealed class SteeringMessagePart : TextStreamPart
{
    /// <summary>Creates a steering-message part.</summary>
    public SteeringMessagePart(string id, UserModelMessage message, int stepNumber)
        : base("steering-message")
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        StepNumber = stepNumber;
    }

    /// <summary>The id of the steering message.</summary>
    public string Id { get; }

    /// <summary>The user message that was added to the run.</summary>
    public UserModelMessage Message { get; }

    /// <summary>The number of the step whose model call received the message.</summary>
    public int StepNumber { get; }
}

/// <summary>The model started streaming tool input.</summary>
public sealed class ToolInputStartPart : TextStreamPart
{
    /// <summary>Creates a tool-input-start part.</summary>
    public ToolInputStartPart(
        string id,
        string toolName,
        JsonElement? providerMetadata,
        bool? providerExecuted,
        bool dynamic,
        ToolInputEncoding? inputEncoding = null,
        string? title = null
    )
        : base("tool-input-start")
    {
        Id = id;
        ToolName = toolName;
        ProviderMetadata = providerMetadata;
        ProviderExecuted = providerExecuted;
        Dynamic = dynamic;
        InputEncoding = inputEncoding;
        Title = title;
    }

    /// <summary>Display title of the tool. Maps to <c>title</c>.</summary>
    public string? Title { get; }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>Provider metadata for this tool input.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>True when the provider executes the tool.</summary>
    public bool? ProviderExecuted { get; }

    /// <summary>True for a tool the caller did not declare, or a dynamic tool.</summary>
    public bool Dynamic { get; }

    /// <summary>Metadata of the tool, apart from provider metadata. Maps to the part's <c>toolMetadata</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ToolMetadata { get; init; }

    /// <summary>Encoding of the input deltas. Defaults to JSON when <see langword="null"/>.</summary>
    public ToolInputEncoding? InputEncoding { get; }
}

/// <summary>A streamed tool-input fragment.</summary>
public sealed class ToolInputDeltaPart : TextStreamPart
{
    /// <summary>Creates a tool-input-delta part.</summary>
    public ToolInputDeltaPart(string id, string delta, JsonElement? providerMetadata)
        : base("tool-input-delta")
    {
        Id = id;
        Delta = delta;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>JSON fragment.</summary>
    public string Delta { get; }

    /// <summary>Provider metadata for this fragment.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>The model finished streaming tool input.</summary>
public sealed class ToolInputEndPart : TextStreamPart
{
    /// <summary>Creates a tool-input-end part.</summary>
    public ToolInputEndPart(string id, JsonElement? providerMetadata)
        : base("tool-input-end")
    {
        Id = id;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Tool call id.</summary>
    public string Id { get; }

    /// <summary>Provider metadata for this tool input.</summary>
    public JsonElement? ProviderMetadata { get; }
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

/// <summary>A tool call failed or was denied. A successful call is a <see cref="ToolResultPart"/>.</summary>
public sealed class ToolErrorPart : TextStreamPart
{
    /// <summary>Creates a tool-error part.</summary>
    public ToolErrorPart(ExecutedTool result)
        : base("tool-error")
    {
        Result = result;
    }

    /// <summary>The failed tool result. <see cref="ExecutedTool.OutputJson"/> holds the error.</summary>
    public ExecutedTool Result { get; }
}

/// <summary>A file the model generated. A URL has been downloaded by the time this part is emitted. Maps to the <c>file</c> stream part.</summary>
public sealed class FilePart : TextStreamPart
{
    /// <summary>Creates a file part.</summary>
    public FilePart(GeneratedFile file, JsonElement? providerMetadata = null)
        : base("file")
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        ProviderMetadata = providerMetadata;
    }

    /// <summary>The file, with its bytes.</summary>
    public GeneratedFile File { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A file the model generated as part of its reasoning. A URL has been downloaded by the time this part is emitted. Maps to the <c>reasoning-file</c> stream part.</summary>
public sealed class ReasoningFilePart : TextStreamPart
{
    /// <summary>Creates a reasoning-file part.</summary>
    public ReasoningFilePart(GeneratedFile file, JsonElement? providerMetadata = null)
        : base("reasoning-file")
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        ProviderMetadata = providerMetadata;
    }

    /// <summary>The file, with its bytes.</summary>
    public GeneratedFile File { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
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

/// <summary>A cited document. Maps to the <c>source</c> stream part of source type <c>document</c>.</summary>
public sealed class DocumentSourcePart : TextStreamPart
{
    /// <summary>Creates a document source part.</summary>
    public DocumentSourcePart(GeneratedDocumentSource source)
        : base("source")
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>Source.</summary>
    public GeneratedDocumentSource Source { get; }
}

/// <summary>A step finished.</summary>
public sealed class StepFinishPart : TextStreamPart
{
    /// <summary>Creates a step-finish part.</summary>
    public StepFinishPart(
        StepResult step,
        LanguageModelResponseMetadata response,
        string? rawFinishReason = null
    )
        : base("finish-step")
    {
        Step = step;
        Response = response;
        RawFinishReason = rawFinishReason;
    }

    /// <summary>Completed step.</summary>
    public StepResult Step { get; }

    /// <summary>Response metadata of the step: id, model id, timestamp, and headers. Maps to <c>response</c>.</summary>
    public LanguageModelResponseMetadata Response { get; }

    /// <summary>The finish reason exactly as the provider reported it, or <see langword="null"/> when the provider reported none. Maps to <c>rawFinishReason</c>.</summary>
    public string? RawFinishReason { get; }

    /// <summary>Provider metadata of the step. Maps to <c>providerMetadata</c>.</summary>
    public JsonElement? ProviderMetadata => Step.ProviderMetadata;

    /// <summary>Performance metrics of the step. Maps to <c>performance</c>.</summary>
    public StepPerformance Performance => Step.Performance;
}

/// <summary>Timing statistics for the gaps between generated output chunks, in milliseconds.</summary>
public sealed class OutputChunkTimingStats
{
    /// <summary>Creates the statistics.</summary>
    public OutputChunkTimingStats(
        double min,
        double max,
        double avg,
        double median,
        double p10,
        double p90
    )
    {
        Min = min;
        Max = max;
        Avg = avg;
        Median = median;
        P10 = p10;
        P90 = p90;
    }

    /// <summary>Smallest gap.</summary>
    public double Min { get; }

    /// <summary>Largest gap.</summary>
    public double Max { get; }

    /// <summary>Average gap.</summary>
    public double Avg { get; }

    /// <summary>Median gap.</summary>
    public double Median { get; }

    /// <summary>10th percentile gap.</summary>
    public double P10 { get; }

    /// <summary>90th percentile gap.</summary>
    public double P90 { get; }
}

/// <summary>Performance metrics for one step. Maps to <c>StepResultPerformance</c>.</summary>
public sealed class StepPerformance
{
    /// <summary>A step with every metric at zero.</summary>
    public static StepPerformance Empty { get; } = new();

    /// <summary>Effective output tokens per second over the full model response.</summary>
    public double EffectiveOutputTokensPerSecond { get; init; }

    /// <summary>Output tokens per second after the first output chunk. Only set for streaming steps.</summary>
    public double? OutputTokensPerSecond { get; init; }

    /// <summary>Input tokens per second before the first output chunk. Only set for streaming steps.</summary>
    public double? InputTokensPerSecond { get; init; }

    /// <summary>Effective input and output tokens per second over the full model response.</summary>
    public double EffectiveTotalTokensPerSecond { get; init; }

    /// <summary>Total time spent on the step in milliseconds.</summary>
    public double StepTimeMs { get; init; }

    /// <summary>Time spent waiting for the model response in milliseconds.</summary>
    public double ResponseTimeMs { get; init; }

    /// <summary>Time spent executing each client-side tool call in milliseconds, keyed by tool call id.</summary>
    public IReadOnlyDictionary<string, double> ToolExecutionMs { get; init; } =
        new Dictionary<string, double>();

    /// <summary>Time until the first generated output chunk in milliseconds. Only set for streaming steps.</summary>
    public double? TimeToFirstOutputMs { get; init; }

    /// <summary>Gaps between output chunks. Only set for streaming steps with at least two output chunks.</summary>
    public OutputChunkTimingStats? TimeBetweenOutputChunksMs { get; init; }
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

    /// <summary>The error that caused this part, when there is one. Maps to the error part's <c>error</c>.</summary>
    public Exception? Error { get; init; }
}

/// <summary>The call was aborted. It is the last part of the stream.</summary>
public sealed class AbortPart : TextStreamPart
{
    /// <summary>Creates an abort part.</summary>
    public AbortPart(string? reason)
        : base("abort")
    {
        Reason = reason;
    }

    /// <summary>Abort reason.</summary>
    public string? Reason { get; }
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
