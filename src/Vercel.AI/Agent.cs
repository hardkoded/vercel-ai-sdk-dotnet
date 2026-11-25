// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI;

/// <summary>An agent that streams a reply to model messages. Maps to the <c>Agent</c> interface.</summary>
public interface IAgent
{
    /// <summary>The tools of the agent. Maps to <c>tools</c>.</summary>
    IReadOnlyList<Tool>? Tools { get; }

    /// <summary>Streams a reply to <paramref name="messages"/>. Maps to <c>stream</c>.</summary>
    StreamTextResult Stream(
        IReadOnlyList<ModelMessage> messages,
        CancellationToken cancellationToken = default,
        Vercel.AI.Util.AbortSignal? abortSignal = null,
        TimeoutConfiguration? timeout = null,
        ISteering? experimentalSteering = null,
        Func<StepResult, CancellationToken, Task>? onStepEnd = null,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox = null,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null
    );
}

/// <summary>An agent that streams a reply to model messages and takes call options. Maps to the <c>Agent</c> interface with a <c>CALL_OPTIONS</c> type.</summary>
/// <typeparam name="TCallOptions">The type of the per-call options. Maps to <c>CALL_OPTIONS</c>.</typeparam>
public interface IAgent<TCallOptions>
{
    /// <summary>The tools of the agent. Maps to <c>tools</c>.</summary>
    IReadOnlyList<Tool>? Tools { get; }

    /// <summary>Streams a reply to <paramref name="messages"/> with <paramref name="options"/>. Maps to <c>stream</c>.</summary>
    StreamTextResult Stream(
        IReadOnlyList<ModelMessage> messages,
        TCallOptions options,
        CancellationToken cancellationToken = default,
        Vercel.AI.Util.AbortSignal? abortSignal = null,
        TimeoutConfiguration? timeout = null,
        ISteering? experimentalSteering = null,
        Func<StepResult, CancellationToken, Task>? onStepEnd = null,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox = null,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null
    );
}

/// <summary>An agent that calls tools until <see cref="StopWhen"/> matches. Maps to <c>ToolLoopAgent</c>.</summary>
public sealed class Agent : IAgent
{
    private readonly IAiClient _client;

    /// <summary>Creates an agent. The default stop condition is 10 steps.</summary>
    public Agent(AgentOptions options, IAiClient? client = null)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        Options = options;
        _client = client ?? new AiClient(Vercel.AI.Gateway.GatewayProvider.Create());
    }

    /// <summary>Agent options.</summary>
    public AgentOptions Options { get; }

    /// <inheritdoc />
    public IReadOnlyList<Tool>? Tools => Options.Tools;

    /// <summary>
    /// Generates a reply to <paramref name="prompt"/>.
    /// <paramref name="timeout"/> replaces <see cref="AgentOptions.Timeout"/> and the timeout that <see cref="AgentOptions.PrepareCall"/> returns for this call. Maps to <c>timeout</c> of <c>generate</c>.
    /// <paramref name="experimentalSteering"/> adds user messages to the run at the next step boundary and is not passed to <see cref="AgentOptions.PrepareCall"/>. Maps to <c>experimental_steering</c>.
    /// </summary>
    public Task<GenerateTextResult> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken = default,
        TimeoutConfiguration? timeout = null,
        ISteering? experimentalSteering = null,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox = null,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null
    )
    {
        return GenerateAsync(
            CreateOptions(prompt, null, experimentalSandbox),
            cancellationToken,
            timeout,
            experimentalSteering,
            onStepStart,
            onToolExecutionStart,
            onToolExecutionEnd
        );
    }

    /// <summary>
    /// Generates a reply to <paramref name="messages"/>. System messages in <paramref name="messages"/> are rejected unless
    /// <see cref="AgentOptions.AllowSystemInMessages"/> is set. Maps to <c>messages</c> of <c>generate</c>.
    /// </summary>
    public Task<GenerateTextResult> GenerateAsync(
        IReadOnlyList<ModelMessage> messages,
        CancellationToken cancellationToken = default,
        TimeoutConfiguration? timeout = null,
        ISteering? experimentalSteering = null,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox = null,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null
    )
    {
        return GenerateAsync(
            CreateOptions(
                null,
                messages ?? throw new ArgumentNullException(nameof(messages)),
                experimentalSandbox
            ),
            cancellationToken,
            timeout,
            experimentalSteering,
            onStepStart,
            onToolExecutionStart,
            onToolExecutionEnd
        );
    }

    private Task<GenerateTextResult> GenerateAsync(
        GenerateTextOptions call,
        CancellationToken cancellationToken,
        TimeoutConfiguration? timeout,
        ISteering? experimentalSteering,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd
    ) =>
        GenerateCoreAsync(
            call,
            cancellationToken,
            timeout,
            experimentalSteering,
            null,
            onStepStart,
            onToolExecutionStart,
            onToolExecutionEnd
        );

    internal async Task<GenerateTextResult> GenerateCoreAsync(
        GenerateTextOptions call,
        CancellationToken cancellationToken,
        TimeoutConfiguration? timeout,
        ISteering? experimentalSteering,
        Func<GenerateTextOptions, GenerateTextOptions?>? prepareCall,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd
    )
    {
        var prepared = Prepare(call, prepareCall).ShallowCopy();
        prepared.Headers = AgentHeaders(prepared.Headers);
        prepared.Timeout = timeout ?? prepared.Timeout;
        prepared.ExperimentalSteering = experimentalSteering ?? prepared.ExperimentalSteering;
        prepared.OnStepEnd = MergeCallbacks(Options.OnStepEnd, prepared.OnStepEnd);
        prepared.OnStepError = Options.OnStepError;
        prepared.OnStart = MergeCallbacks(Options.OnStart, prepared.OnStart);
        prepared.OnStepStart = MergeCallbacks(
            Options.OnStepStart,
            MergeCallbacks(prepared.OnStepStart, onStepStart)
        );
        prepared.OnToolExecutionStart = MergeCallbacks(
            Options.OnToolExecutionStart,
            MergeCallbacks(prepared.OnToolExecutionStart, onToolExecutionStart)
        );
        prepared.OnToolExecutionEnd = MergeCallbacks(
            Options.OnToolExecutionEnd,
            MergeCallbacks(prepared.OnToolExecutionEnd, onToolExecutionEnd)
        );
        prepared.GenerateCallId = Options.GenerateCallId ?? prepared.GenerateCallId;
        prepared.NowMs = Options.NowMs ?? prepared.NowMs;
        prepared.OnLanguageModelCallStart = MergeCallbacks(
            Options.OnLanguageModelCallStart,
            prepared.OnLanguageModelCallStart
        );
        prepared.OnLanguageModelCallEnd = MergeCallbacks(
            Options.OnLanguageModelCallEnd,
            prepared.OnLanguageModelCallEnd
        );
#pragma warning disable CS0618 // OnFinish is the deprecated alias of OnEnd
        prepared.OnEnd = MergeCallbacks(Options.OnEnd, prepared.OnEnd ?? prepared.OnFinish);
#pragma warning restore CS0618

        return await _client.GenerateTextAsync(prepared, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Streams a reply to <paramref name="prompt"/>. <paramref name="abortSignal"/> aborts the stream and carries the reason.
    /// <paramref name="timeout"/> replaces <see cref="AgentOptions.Timeout"/> and the timeout that <see cref="AgentOptions.PrepareCall"/> returns for this call. Maps to <c>timeout</c> of <c>stream</c>.
    /// <paramref name="experimentalSteering"/> adds user messages to the run at the next step boundary and is not passed to <see cref="AgentOptions.PrepareCall"/>. Maps to <c>experimental_steering</c>.
    /// </summary>
    public StreamTextResult Stream(
        string prompt,
        CancellationToken cancellationToken = default,
        Vercel.AI.Util.AbortSignal? abortSignal = null,
        TimeoutConfiguration? timeout = null,
        ISteering? experimentalSteering = null,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox = null,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null
    )
    {
        return Stream(
            CreateOptions(prompt, null, experimentalSandbox),
            cancellationToken,
            abortSignal,
            timeout,
            experimentalSteering,
            null,
            onStepStart,
            onToolExecutionStart,
            onToolExecutionEnd
        );
    }

    /// <summary>
    /// Streams a reply to <paramref name="messages"/>. System messages in <paramref name="messages"/> are rejected unless
    /// <see cref="AgentOptions.AllowSystemInMessages"/> is set. <paramref name="onStepEnd"/> replaces the callback that
    /// <see cref="AgentOptions.PrepareCall"/> returns for this call. Maps to <c>messages</c> of <c>stream</c>.
    /// </summary>
    public StreamTextResult Stream(
        IReadOnlyList<ModelMessage> messages,
        CancellationToken cancellationToken = default,
        Vercel.AI.Util.AbortSignal? abortSignal = null,
        TimeoutConfiguration? timeout = null,
        ISteering? experimentalSteering = null,
        Func<StepResult, CancellationToken, Task>? onStepEnd = null,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox = null,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null
    )
    {
        return Stream(
            CreateOptions(
                null,
                messages ?? throw new ArgumentNullException(nameof(messages)),
                experimentalSandbox
            ),
            cancellationToken,
            abortSignal,
            timeout,
            experimentalSteering,
            onStepEnd,
            onStepStart,
            onToolExecutionStart,
            onToolExecutionEnd
        );
    }

    /// <inheritdoc />
    StreamTextResult IAgent.Stream(
        IReadOnlyList<ModelMessage> messages,
        CancellationToken cancellationToken,
        Vercel.AI.Util.AbortSignal? abortSignal,
        TimeoutConfiguration? timeout,
        ISteering? experimentalSteering,
        Func<StepResult, CancellationToken, Task>? onStepEnd,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd
    )
    {
        return Stream(
            messages,
            cancellationToken,
            abortSignal,
            timeout,
            experimentalSteering,
            onStepEnd,
            experimentalSandbox,
            onStepStart,
            onToolExecutionStart,
            onToolExecutionEnd
        );
    }

    private StreamTextResult Stream(
        GenerateTextOptions call,
        CancellationToken cancellationToken,
        Vercel.AI.Util.AbortSignal? abortSignal,
        TimeoutConfiguration? timeout,
        ISteering? experimentalSteering,
        Func<StepResult, CancellationToken, Task>? onStepEnd,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd
    ) =>
        StreamCore(
            call,
            cancellationToken,
            abortSignal,
            timeout,
            experimentalSteering,
            onStepEnd,
            null,
            onStepStart,
            onToolExecutionStart,
            onToolExecutionEnd
        );

    internal StreamTextResult StreamCore(
        GenerateTextOptions call,
        CancellationToken cancellationToken,
        Vercel.AI.Util.AbortSignal? abortSignal,
        TimeoutConfiguration? timeout,
        ISteering? experimentalSteering,
        Func<StepResult, CancellationToken, Task>? onStepEnd,
        Func<GenerateTextOptions, GenerateTextOptions?>? prepareCall,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd
    )
    {
        var prepared = Prepare(call, prepareCall);
        return _client.StreamTextAsync(
            new StreamTextOptions
            {
                Model = prepared.Model,
                ModelId = prepared.ModelId,
                System = prepared.System,
                Instructions = prepared.Instructions,
                Prompt = prepared.Prompt,
                Messages = prepared.Messages,
                AllowSystemInMessages = prepared.AllowSystemInMessages,
                Tools = prepared.Tools,
                ActiveTools = prepared.ActiveTools,
                ToolOrder = prepared.ToolOrder,
                ToolCallConcurrency = prepared.ToolCallConcurrency,
                ToolChoice = prepared.ToolChoice,
                StopWhen = prepared.StopWhen,
                Output = prepared.Output,
                MaxOutputTokens = prepared.MaxOutputTokens,
                Temperature = prepared.Temperature,
                TopP = prepared.TopP,
                TopK = prepared.TopK,
                PresencePenalty = prepared.PresencePenalty,
                FrequencyPenalty = prepared.FrequencyPenalty,
                StopSequences = prepared.StopSequences,
                Seed = prepared.Seed,
                Reasoning = prepared.Reasoning,
                MaxRetries = prepared.MaxRetries,
                Timeout = timeout ?? prepared.Timeout,
                Headers = AgentHeaders(prepared.Headers),
                ProviderOptions = prepared.ProviderOptions,
                PrepareStep = prepared.PrepareStep,
                ExperimentalSteering = experimentalSteering ?? prepared.ExperimentalSteering,
                ExperimentalSandbox = prepared.ExperimentalSandbox,
                OnStepEnd = MergeCallbacks(Options.OnStepEnd, onStepEnd ?? prepared.OnStepEnd),
                OnStepError = Options.OnStepError,
                OnStart = MergeCallbacks(Options.OnStart, prepared.OnStart),
                OnStepStart = MergeCallbacks(
                    Options.OnStepStart,
                    MergeCallbacks(prepared.OnStepStart, onStepStart)
                ),
                OnToolExecutionStart = MergeCallbacks(
                    Options.OnToolExecutionStart,
                    MergeCallbacks(prepared.OnToolExecutionStart, onToolExecutionStart)
                ),
                OnToolExecutionEnd = MergeCallbacks(
                    Options.OnToolExecutionEnd,
                    MergeCallbacks(prepared.OnToolExecutionEnd, onToolExecutionEnd)
                ),
                GenerateCallId = Options.GenerateCallId ?? prepared.GenerateCallId,
                NowMs = Options.NowMs ?? prepared.NowMs,
                OnLanguageModelCallStart = MergeCallbacks(
                    Options.OnLanguageModelCallStart,
                    prepared.OnLanguageModelCallStart
                ),
                OnLanguageModelCallEnd = MergeCallbacks(
                    Options.OnLanguageModelCallEnd,
                    prepared.OnLanguageModelCallEnd
                ),
#pragma warning disable CS0618 // OnFinish is the deprecated alias of OnEnd
                OnEnd = MergeCallbacks(Options.OnEnd, prepared.OnEnd ?? prepared.OnFinish),
#pragma warning restore CS0618
                Telemetry = prepared.Telemetry,
                ExperimentalTelemetry = prepared.ExperimentalTelemetry,
#pragma warning disable CS0618 // ExperimentalInclude is the deprecated alias of Include
                Include = (prepared.Include ?? prepared.ExperimentalInclude) is { } include
                    ? new StreamTextInclude
                    {
                        RequestBody = include.RequestBody,
                        RequestMessages = include.RequestMessages,
                    }
                    : null,
#pragma warning restore CS0618
#pragma warning disable CS0618 // OnFinish is the deprecated alias of OnEnd
                OnFinish = prepared.OnFinish,
#pragma warning restore CS0618
                OnError = prepared.OnError,
                ToolApproval = prepared.ToolApproval,
                ToolsContext = prepared.ToolsContext,
                RuntimeContext = prepared.RuntimeContext,
                ExperimentalToolApprovalSecret = prepared.ExperimentalToolApprovalSecret,
                ExperimentalToolApprovalSecretBytes = prepared.ExperimentalToolApprovalSecretBytes,
#pragma warning disable CS0618
                RepairToolCall = prepared.RepairToolCall ?? prepared.ExperimentalRepairToolCall,
#pragma warning restore CS0618
                RefineToolInput = prepared.RefineToolInput,
                AbortSignal = abortSignal,
                OnAbort = Options.OnAbort,
            },
            cancellationToken
        );
    }

    // mergeCallbacks: both callbacks run, the settings callback first, and an error of one does not stop the other
    // Maps to ToolLoopAgent.agentHeaders: tags outgoing requests so usage can be attributed to the agent.
    private static IReadOnlyDictionary<string, string?> AgentHeaders(
        IReadOnlyDictionary<string, string?>? headers
    ) =>
        ProviderValues
            .WithUserAgentSuffix(headers, "ai-sdk-agent/tool-loop")
            .ToDictionary(pair => pair.Key, pair => (string?)pair.Value);

    private static Func<TEvent, CancellationToken, Task>? MergeCallbacks<TEvent>(
        Func<TEvent, CancellationToken, Task>? settings,
        Func<TEvent, CancellationToken, Task>? call
    )
    {
        if (settings is null || call is null)
        {
            return settings ?? call;
        }

        return async (result, cancellationToken) =>
        {
            await Task.WhenAll(
                    Settle(settings, result, cancellationToken),
                    Settle(call, result, cancellationToken)
                )
                .ConfigureAwait(false);
        };

        static async Task Settle(
            Func<TEvent, CancellationToken, Task> callback,
            TEvent result,
            CancellationToken cancellationToken
        )
        {
            try
            {
                await callback(result, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Promise.allSettled
            }
        }
    }

    private GenerateTextOptions Prepare(
        GenerateTextOptions call,
        Func<GenerateTextOptions, GenerateTextOptions?>? prepareCall
    )
    {
        return (prepareCall ?? Options.PrepareCall)?.Invoke(call) ?? call;
    }

    internal GenerateTextOptions CreateOptions(
        string? prompt,
        IReadOnlyList<ModelMessage>? messages,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox
    )
    {
        return new GenerateTextOptions
        {
            Model = Options.Model,
            ModelId = Options.ModelId,
            Instructions = Options.Instructions,
            AllowSystemInMessages = Options.AllowSystemInMessages,
            Prompt = prompt,
            Messages = messages,
            Tools = Options.Tools,
            StopWhen = Options.StopWhen ?? StopWhen.CreateDefaultStopCondition(10),
            Temperature = Options.Temperature,
            MaxOutputTokens = Options.MaxOutputTokens,
            Headers = Options.Headers,
            ToolOrder = Options.ToolOrder,
            ToolCallConcurrency = Options.ToolCallConcurrency,
            RepairToolCall = Options.RepairToolCall,
            RefineToolInput = Options.RefineToolInput,
            Timeout = Options.Timeout,
            ToolApproval = Options.ToolApproval,
            ToolsContext = Options.ToolsContext,
            RuntimeContext = Options.RuntimeContext,
            Telemetry = Options.Telemetry,
            ExperimentalToolApprovalSecret = Options.ExperimentalToolApprovalSecret,
            ExperimentalToolApprovalSecretBytes = Options.ExperimentalToolApprovalSecretBytes,
            ExperimentalSandbox = experimentalSandbox,
        };
    }
}

/// <summary>Agent settings.</summary>
public class AgentOptions
{
    /// <summary>Explicit model.</summary>
    public ILanguageModel? Model { get; set; }

    /// <summary>Gateway model id.</summary>
    public string? ModelId { get; set; }

    /// <summary>System instructions: a string, one <see cref="SystemModelMessage"/>, or an array of them.</summary>
    public Instructions? Instructions { get; set; }

    /// <summary>
    /// Whether system messages are allowed in the messages passed to <see cref="Agent.GenerateAsync(IReadOnlyList{ModelMessage}, CancellationToken, TimeoutConfiguration?, ISteering?, Vercel.AI.ProviderUtils.ISandboxSession?, Func{GenerateTextStepStartEvent, CancellationToken, Task}?, Func{ToolExecutionStartEvent, CancellationToken, Task}?, Func{ToolExecutionEndEvent, CancellationToken, Task}?)"/>
    /// and <see cref="Agent.Stream(IReadOnlyList{ModelMessage}, CancellationToken, Vercel.AI.Util.AbortSignal?, TimeoutConfiguration?, ISteering?, Func{StepResult, CancellationToken, Task}?, Vercel.AI.ProviderUtils.ISandboxSession?, Func{GenerateTextStepStartEvent, CancellationToken, Task}?, Func{ToolExecutionStartEvent, CancellationToken, Task}?, Func{ToolExecutionEndEvent, CancellationToken, Task}?)"/>.
    /// Defaults to false, which rejects them with an <see cref="InvalidPromptException"/>. Use <see cref="Instructions"/> for the system prompt instead.
    /// <see cref="PrepareCall"/> can replace it. Maps to <c>allowSystemInMessages</c>.
    /// </summary>
    public bool AllowSystemInMessages { get; set; }

    /// <summary>Tools.</summary>
    public IReadOnlyList<Tool>? Tools { get; set; }

    /// <summary>
    /// Called when each generation step ends. It runs together with the callback of the call, and a failure of one does not stop the other.
    /// Maps to <c>onStepEnd</c> of the <c>ToolLoopAgent</c> settings.
    /// </summary>
    public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }

    /// <summary>
    /// Called when a step that started fails before it produces a step result. <see cref="OnStepEnd"/> is not called for such a step.
    /// Unlike <see cref="OnStepEnd"/>, it can only be set here and not per call. Maps to <c>onStepError</c> of the <c>ToolLoopAgent</c> settings.
    /// </summary>
    public Func<GenerateTextStepErrorEvent, CancellationToken, Task>? OnStepError { get; set; }

    /// <summary>Telemetry configuration of every call. See <see cref="GenerateTextOptions.Telemetry"/>. <see cref="PrepareCall"/> can replace it. Maps to <c>telemetry</c>.</summary>
    public TelemetryOptions? Telemetry { get; set; }

    /// <summary>
    /// Called when a generation begins. It runs together with the callback of the call, and a failure of one does not stop the other.
    /// Maps to <c>onStart</c> of the <c>ToolLoopAgent</c> settings.
    /// </summary>
    public Func<GenerateTextStartEvent, CancellationToken, Task>? OnStart { get; set; }

    /// <summary>Called when each step begins. It runs together with the callback of the call. Maps to <c>onStepStart</c> of the <c>ToolLoopAgent</c> settings.</summary>
    public Func<GenerateTextStepStartEvent, CancellationToken, Task>? OnStepStart { get; set; }

    /// <summary>Called before a tool runs. It runs together with the callback of the call. Maps to <c>onToolExecutionStart</c> of the <c>ToolLoopAgent</c> settings.</summary>
    public Func<
        ToolExecutionStartEvent,
        CancellationToken,
        Task
    >? OnToolExecutionStart { get; set; }

    /// <summary>Called after a tool ran. It runs together with the callback of the call. Maps to <c>onToolExecutionEnd</c> of the <c>ToolLoopAgent</c> settings.</summary>
    public Func<ToolExecutionEndEvent, CancellationToken, Task>? OnToolExecutionEnd { get; set; }

    /// <summary>Generates the id of each call. Tests replace it. Maps to <c>_internal.generateCallId</c>.</summary>
    internal Func<string>? GenerateCallId { get; set; }

    /// <summary>The clock for step timing in milliseconds. Tests replace it. Maps to the mocked <c>now</c>.</summary>
    internal Func<double>? NowMs { get; set; }

    /// <summary>Called before each provider model call. It runs together with the callback of the call. Maps to <c>onLanguageModelCallStart</c> of the <c>ToolLoopAgent</c> settings.</summary>
    public Func<
        LanguageModelCallStartEvent,
        CancellationToken,
        Task
    >? OnLanguageModelCallStart { get; set; }

    /// <summary>Called after each provider model call. It runs together with the callback of the call. Maps to <c>onLanguageModelCallEnd</c> of the <c>ToolLoopAgent</c> settings.</summary>
    public Func<
        LanguageModelCallEndEvent,
        CancellationToken,
        Task
    >? OnLanguageModelCallEnd { get; set; }

    /// <summary>
    /// Called when a generation completes. It runs together with the callback of the call, and a failure of one does not stop the other.
    /// Maps to <c>onFinish</c> of the <c>ToolLoopAgent</c> settings.
    /// </summary>
    public Func<GenerateTextResult, CancellationToken, Task>? OnEnd { get; set; }

    /// <summary>
    /// Configures whether tools require approval before they run. See <see cref="GenerateTextOptions.ToolApproval"/>.
    /// <see cref="PrepareCall"/> can replace it. Maps to <c>toolApproval</c>.
    /// </summary>
    public ToolApprovalConfiguration? ToolApproval { get; set; }

    /// <summary>The context of each tool, keyed by tool name. See <see cref="GenerateTextOptions.ToolsContext"/>. <see cref="PrepareCall"/> can replace it. Maps to <c>toolsContext</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ToolsContext { get; set; }

    /// <summary>User-defined context that is passed to the approval functions. See <see cref="GenerateTextOptions.RuntimeContext"/>. <see cref="PrepareCall"/> can replace it. Maps to <c>runtimeContext</c>.</summary>
    public IReadOnlyDictionary<string, object?>? RuntimeContext { get; set; }

    /// <summary>Secret for HMAC-signing tool approval requests. Approval requests of automatically denied tool calls are not signed. See <see cref="GenerateTextOptions.ExperimentalToolApprovalSecret"/>. Maps to <c>experimental_toolApprovalSecret</c>.</summary>
    public string? ExperimentalToolApprovalSecret { get; set; }

    /// <summary>The same secret as <see cref="ExperimentalToolApprovalSecret"/>, given as bytes. See <see cref="GenerateTextOptions.ExperimentalToolApprovalSecretBytes"/>.</summary>
    public byte[]? ExperimentalToolApprovalSecretBytes { get; set; }

    /// <summary>Stop condition. Defaults to 10 steps.</summary>
    public StopCondition? StopWhen { get; set; }

    /// <summary>Temperature.</summary>
    public double? Temperature { get; set; }

    /// <summary>Maximum output tokens.</summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>Additional HTTP headers sent with every request. The agent appends <c>ai-sdk-agent/tool-loop</c> to the <c>user-agent</c> header.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Controls the order in which tools are sent to the provider. See <see cref="GenerateTextOptions.ToolOrder"/>. <see cref="PrepareCall"/> can replace it.</summary>
    public IReadOnlyList<string>? ToolOrder { get; set; }

    /// <summary>
    /// Maximum number of tool calls that may execute concurrently within a step. See <see cref="GenerateTextOptions.ToolCallConcurrency"/>.
    /// <see cref="PrepareCall"/> can replace it.
    /// </summary>
    public int? ToolCallConcurrency { get; set; }

    /// <summary>Repairs a tool call that failed to parse. See <see cref="GenerateTextOptions.RepairToolCall"/>. <see cref="PrepareCall"/> can replace it.</summary>
    public ToolCallRepairFunction? RepairToolCall { get; set; }

    /// <summary>Refines the parsed input of tool calls. See <see cref="GenerateTextOptions.RefineToolInput"/>. <see cref="PrepareCall"/> can replace it.</summary>
    public IReadOnlyDictionary<
        string,
        Func<JsonElement, CancellationToken, Task<JsonElement>>
    >? RefineToolInput { get; set; }

    /// <summary>
    /// Timeout of each <see cref="Agent.GenerateAsync(string, CancellationToken, TimeoutConfiguration?, ISteering?, Vercel.AI.ProviderUtils.ISandboxSession?, Func{GenerateTextStepStartEvent, CancellationToken, Task}?, Func{ToolExecutionStartEvent, CancellationToken, Task}?, Func{ToolExecutionEndEvent, CancellationToken, Task}?)"/> and <see cref="Agent.Stream(string, CancellationToken, Vercel.AI.Util.AbortSignal?, TimeoutConfiguration?, ISteering?, Vercel.AI.ProviderUtils.ISandboxSession?, Func{GenerateTextStepStartEvent, CancellationToken, Task}?, Func{ToolExecutionStartEvent, CancellationToken, Task}?, Func{ToolExecutionEndEvent, CancellationToken, Task}?)"/> call. See <see cref="GenerateTextOptions.Timeout"/>.
    /// <see cref="PrepareCall"/> can replace it. Maps to <c>timeout</c> of <c>ToolLoopAgentSettings</c>.
    /// </summary>
    public TimeoutConfiguration? Timeout { get; set; }

    /// <summary>
    /// Called before each <see cref="Agent.GenerateAsync(string, CancellationToken, TimeoutConfiguration?, ISteering?, Vercel.AI.ProviderUtils.ISandboxSession?, Func{GenerateTextStepStartEvent, CancellationToken, Task}?, Func{ToolExecutionStartEvent, CancellationToken, Task}?, Func{ToolExecutionEndEvent, CancellationToken, Task}?)"/> and <see cref="Agent.Stream(string, CancellationToken, Vercel.AI.Util.AbortSignal?, TimeoutConfiguration?, ISteering?, Vercel.AI.ProviderUtils.ISandboxSession?, Func{GenerateTextStepStartEvent, CancellationToken, Task}?, Func{ToolExecutionStartEvent, CancellationToken, Task}?, Func{ToolExecutionEndEvent, CancellationToken, Task}?)"/> call with the options the agent would use,
    /// including the default <see cref="GenerateTextOptions.StopWhen"/>. Return replacement options, or null to keep them.
    /// A returned options object that sets <see cref="GenerateTextOptions.StopWhen"/> replaces the default limit. Maps to <c>prepareCall</c>.
    /// </summary>
    public Func<GenerateTextOptions, GenerateTextOptions?>? PrepareCall { get; set; }

    /// <summary>Called after <see cref="Agent.Stream(string, CancellationToken, Vercel.AI.Util.AbortSignal?, TimeoutConfiguration?, ISteering?, Vercel.AI.ProviderUtils.ISandboxSession?, Func{GenerateTextStepStartEvent, CancellationToken, Task}?, Func{ToolExecutionStartEvent, CancellationToken, Task}?, Func{ToolExecutionEndEvent, CancellationToken, Task}?)"/> is aborted. See <see cref="StreamTextOptions.OnAbort"/>.</summary>
    public Func<StreamAbortContext, CancellationToken, Task>? OnAbort { get; set; }
}

/// <summary>
/// Looks up provider models by <c>provider:model</c> ids. Maps to <c>createProviderRegistry</c>.
/// Everything after the first separator is the model id, so model ids may contain the separator.
/// </summary>
public sealed class ProviderRegistry
{
    private readonly Dictionary<string, ProviderBase> _providers = new(
        StringComparer.OrdinalIgnoreCase
    );
    private readonly string _separator;

    /// <summary>Creates a registry. <paramref name="separator"/> splits the provider id from the model id.</summary>
    public ProviderRegistry(string separator = ":")
    {
        if (string.IsNullOrEmpty(separator))
        {
            throw new ArgumentException("A separator is required.", nameof(separator));
        }

        _separator = separator;
    }

    /// <summary>Middleware applied to every language model the registry returns. The first one sees the call first.</summary>
    public IReadOnlyList<ILanguageModelMiddleware>? LanguageModelMiddleware { get; set; }

    /// <summary>Registers a provider under <paramref name="id"/>.</summary>
    public void Register(string id, ProviderBase provider)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Provider id is required.", nameof(id));
        }

        _providers[id] = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>Resolves <c>provider:model</c> to a language model.</summary>
    public ILanguageModel LanguageModel(string providerAndModel)
    {
        var model = Model(providerAndModel, "languageModel", (provider, modelId) =>
            provider.LanguageModel(modelId)
        );
        return LanguageModelMiddleware is { Count: > 0 } middleware
            ? model.WrapLanguageModel(middleware.ToArray())
            : model;
    }

    /// <summary>Resolves <c>provider:model</c> to an embedding model.</summary>
    public IEmbeddingModel EmbeddingModel(string providerAndModel)
    {
        return Model(providerAndModel, "embeddingModel", (provider, modelId) =>
            provider.EmbeddingModel(modelId)
        );
    }

    /// <summary>Resolves <c>provider:model</c> to an image model.</summary>
    public IImageModel ImageModel(string providerAndModel)
    {
        return Model(providerAndModel, "imageModel", (provider, modelId) =>
            provider.ImageModel(modelId)
        );
    }

    /// <summary>Resolves <c>provider:model</c> to a transcription model.</summary>
    public ITranscriptionModel TranscriptionModel(string providerAndModel)
    {
        return Model(providerAndModel, "transcriptionModel", (provider, modelId) =>
            provider.TranscriptionModel(modelId)
        );
    }

    /// <summary>Resolves <c>provider:model</c> to a speech model.</summary>
    public ISpeechModel SpeechModel(string providerAndModel)
    {
        return Model(providerAndModel, "speechModel", (provider, modelId) =>
            provider.SpeechModel(modelId)
        );
    }

    /// <summary>Resolves <c>provider:model</c> to a reranking model.</summary>
    public IRerankingModel RerankingModel(string providerAndModel)
    {
        return Model(providerAndModel, "rerankingModel", (provider, modelId) =>
            provider.RerankingModel(modelId)
        );
    }

    /// <summary>Resolves <c>provider:model</c> to a video model.</summary>
    public IVideoModel VideoModel(string providerAndModel)
    {
        return Model(providerAndModel, "videoModel", (provider, modelId) =>
            provider.VideoModel(modelId)
        );
    }

    /// <summary>Resolves <c>provider:model</c> to a model that scores a candidate against a rubric.</summary>
    public IRubricModel RubricModel(string providerAndModel)
    {
        return Model(providerAndModel, "rubricModel", (provider, modelId) =>
            provider.RubricModel(modelId)
        );
    }

    /// <summary>
    /// Resolves <c>provider:model</c> to a decision model. The provider must implement <see cref="Operations.IDecisionProvider"/>
    /// or the deprecated <see cref="Operations.IEvaluationProvider"/>, or <see cref="NoSuchModelError"/> is thrown.
    /// </summary>
    public Operations.IDecisionCaller DecisionModel(string providerAndModel)
    {
        return Operations.DecisionModels.Validate(
            Model(providerAndModel, "decisionModel", Operations.DecisionModels.FromProvider)
        );
    }

    /// <summary>
    /// Deprecated alias of <see cref="DecisionModel"/>. Returns the same model, which also has the deprecated
    /// <see cref="Operations.IEvaluationCaller.DoEvaluateAsync"/>. Maps to <c>evaluationModel</c>.
    /// </summary>
    [Obsolete("Use DecisionModel instead.")]
    public Operations.IEvaluationDecisionCaller EvaluationModel(string providerAndModel)
    {
        return Operations.DecisionModels.AsEvaluationModel(DecisionModel(providerAndModel));
    }

    /// <summary>
    /// Resolves <c>provider:model</c> to an extraction model. The provider must implement <see cref="Operations.IExtractionProvider"/>,
    /// or <see cref="NoSuchModelError"/> is thrown. Maps to <c>extractionModel</c>.
    /// </summary>
    public Operations.IExtractionCaller ExtractionModel(string providerAndModel)
    {
        return Operations.DecisionModels.ResolveExtractionModel(
            Model<Operations.IExtractionCaller>(
                providerAndModel,
                "extractionModel",
                (provider, modelId) =>
                    (provider as Operations.IExtractionProvider)?.ExtractionModel(modelId)
            )
        );
    }

    /// <summary>Returns the file store of the provider registered under <paramref name="providerId"/>.</summary>
    public IFileStore FileStore(string providerId)
    {
        return Provider(providerId, "languageModel").FileStore();
    }

    /// <summary>Returns the skill store of the provider registered under <paramref name="providerId"/>.</summary>
    public ISkillStore SkillStore(string providerId)
    {
        return Provider(providerId, "languageModel").SkillStore();
    }

    // A provider that returns null has no such model.
    private T Model<T>(string id, string modelType, Func<ProviderBase, string, T?> create)
        where T : class
    {
        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        var index = id.IndexOf(_separator, StringComparison.Ordinal);
        if (index < 0)
        {
            throw new NoSuchModelError(
                id,
                modelType,
                "Invalid "
                    + modelType
                    + " id for registry: "
                    + id
                    + " (must be in the format \"providerId"
                    + _separator
                    + "modelId\")"
            );
        }

        var provider = Provider(id.Substring(0, index), modelType);
        return create(provider, id.Substring(index + _separator.Length))
            ?? throw new NoSuchModelError(id, modelType);
    }

    private ProviderBase Provider(string providerId, string modelType)
    {
        return _providers.TryGetValue(providerId, out var provider)
            ? provider
            : throw new NoSuchProviderError(
                providerId,
                modelType,
                providerId,
                _providers.Keys.ToList()
            );
    }
}

/// <summary>
/// A provider backed by caller-supplied models. Maps to <c>customProvider</c>. A model id that is not registered goes to
/// <see cref="FallbackProvider"/>, or throws <see cref="NoSuchModelError"/> when there is none.
/// </summary>
public sealed class CustomProvider
    : ProviderBase,
        Operations.IDecisionProvider,
        Operations.IExtractionProvider
{
    private readonly Dictionary<string, ILanguageModel> _language = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IEmbeddingModel> _embedding = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IImageModel> _image = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ITranscriptionModel> _transcription = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<string, ISpeechModel> _speech = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IRerankingModel> _reranking = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IVideoModel> _video = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IRubricModel> _rubric = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<Operations.IDecisionCaller>> _decision = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<string, Func<Operations.IDecisionCaller>> _legacyDecision = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<string, Func<Operations.IExtractionCaller>> _extraction = new(
        StringComparer.Ordinal
    );

    /// <summary>Creates a custom provider.</summary>
    public CustomProvider(string name = "custom")
        : base(name) { }

    /// <summary>Provider asked for model ids that are not registered here.</summary>
    public ProviderBase? FallbackProvider { get; set; }

    /// <summary>File store. When null, the fallback provider's file store is used.</summary>
    public IFileStore? Files { get; set; }

    /// <summary>Skill store. When null, the fallback provider's skill store is used.</summary>
    public ISkillStore? Skills { get; set; }

    /// <summary>Registers a language model under its own model id.</summary>
    public CustomProvider AddLanguageModel(ILanguageModel model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        return AddLanguageModel(model.ModelId, model);
    }

    /// <summary>Registers a language model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddLanguageModel(string modelId, ILanguageModel model)
    {
        return Add(_language, modelId, model);
    }

    /// <summary>Registers an embedding model under its own model id.</summary>
    public CustomProvider AddEmbeddingModel(IEmbeddingModel model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        return AddEmbeddingModel(model.ModelId, model);
    }

    /// <summary>Registers an embedding model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddEmbeddingModel(string modelId, IEmbeddingModel model)
    {
        return Add(_embedding, modelId, model);
    }

    /// <summary>Registers an image model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddImageModel(string modelId, IImageModel model)
    {
        return Add(_image, modelId, model);
    }

    /// <summary>Registers a transcription model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddTranscriptionModel(string modelId, ITranscriptionModel model)
    {
        return Add(_transcription, modelId, model);
    }

    /// <summary>Registers a speech model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddSpeechModel(string modelId, ISpeechModel model)
    {
        return Add(_speech, modelId, model);
    }

    /// <summary>Registers a reranking model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddRerankingModel(string modelId, IRerankingModel model)
    {
        return Add(_reranking, modelId, model);
    }

    /// <summary>Registers a video model under <paramref name="modelId"/>.</summary>
    public CustomProvider AddVideoModel(string modelId, IVideoModel model)
    {
        return Add(_video, modelId, model);
    }

    /// <summary>Registers a model that scores a candidate against a rubric under <paramref name="modelId"/>.</summary>
    public CustomProvider AddRubricModel(string modelId, IRubricModel model)
    {
        return Add(_rubric, modelId, model);
    }

    /// <summary>Deprecated name of <see cref="AddRubricModel"/>. Registers a rubric model, not a decision model.</summary>
    [Obsolete("Use AddRubricModel instead.")]
    public CustomProvider AddEvaluationModel(string modelId, IEvaluationModel model)
    {
        return AddRubricModel(modelId, RubricModels.FromLegacy(model));
    }

    /// <summary>Registers a decision model under <paramref name="modelId"/>. Maps to <c>decisionModels</c>.</summary>
    public CustomProvider AddDecisionModel(string modelId, Operations.IDecisionCaller model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        return Add(_decision, modelId, () => model);
    }

    /// <summary>
    /// Registers a string alias for a decision model. The id is resolved through
    /// <see cref="Operations.DecisionModels.DefaultProvider"/>, or Gateway when none is set, when <see cref="DecisionModel"/> is called.
    /// </summary>
    public CustomProvider AddDecisionModel(string modelId, string providerModelId)
    {
        if (providerModelId is null)
        {
            throw new ArgumentNullException(nameof(providerModelId));
        }

        return Add(_decision, modelId, () =>
            Operations.DecisionModels.Resolve(null, providerModelId)
        );
    }

    /// <summary>
    /// Registers a deprecated decision model under <paramref name="modelId"/>. Maps to <c>evaluationModels</c>.
    /// These models are ignored once any model is added with <c>AddDecisionModel</c>.
    /// </summary>
    [Obsolete("Use AddDecisionModel instead.")]
    public CustomProvider AddEvaluationModel(string modelId, Operations.IEvaluationCaller model)
    {
        var legacy = Operations.DecisionCaller.FromLegacy(model);
        return Add(_legacyDecision, modelId, () => legacy);
    }

    /// <summary>Registers an extraction model under <paramref name="modelId"/>. Maps to <c>extractionModels</c>.</summary>
    public CustomProvider AddExtractionModel(string modelId, Operations.IExtractionCaller model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        return Add(_extraction, modelId, () => model);
    }

    /// <summary>
    /// Registers a string alias for an extraction model. The id is resolved through
    /// <see cref="Operations.DecisionModels.DefaultProvider"/>, or Gateway when none is set, when <see cref="ExtractionModel"/> is called.
    /// </summary>
    public CustomProvider AddExtractionModel(string modelId, string providerModelId)
    {
        if (providerModelId is null)
        {
            throw new ArgumentNullException(nameof(providerModelId));
        }

        return Add(_extraction, modelId, () =>
            Operations.DecisionModels.ResolveExtractionModel(null, providerModelId)
        );
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return Find(_language, modelId, "languageModel", fallback =>
            fallback.LanguageModel(modelId)
        );
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return Find(_embedding, modelId, "embeddingModel", fallback =>
            fallback.EmbeddingModel(modelId)
        );
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        return Find(_image, modelId, "imageModel", fallback => fallback.ImageModel(modelId));
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return Find(_transcription, modelId, "transcriptionModel", fallback =>
            fallback.TranscriptionModel(modelId)
        );
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId)
    {
        return Find(_speech, modelId, "speechModel", fallback => fallback.SpeechModel(modelId));
    }

    /// <inheritdoc />
    public override IRerankingModel RerankingModel(string modelId)
    {
        return Find(_reranking, modelId, "rerankingModel", fallback =>
            fallback.RerankingModel(modelId)
        );
    }

    /// <inheritdoc />
    public override IVideoModel VideoModel(string modelId)
    {
        return Find(_video, modelId, "videoModel", fallback => fallback.VideoModel(modelId));
    }

    /// <inheritdoc />
    public override IRubricModel RubricModel(string modelId)
    {
        return Find(_rubric, modelId, "rubricModel", fallback => fallback.RubricModel(modelId));
    }

    /// <summary>
    /// Returns the decision model registered under <paramref name="modelId"/>, or asks <see cref="FallbackProvider"/>
    /// when it implements <see cref="Operations.IDecisionProvider"/> or the deprecated <see cref="Operations.IEvaluationProvider"/>.
    /// </summary>
    public Operations.IDecisionCaller DecisionModel(string modelId)
    {
        var models = _decision.Count > 0 ? _decision : _legacyDecision;
        var factory = Find(models, modelId, "decisionModel", fallback =>
            Operations.DecisionModels.FromProvider(fallback, modelId) is { } found
                ? () => found
                : null
        );
        return Operations.DecisionModels.Validate(factory());
    }

    /// <summary>
    /// Deprecated alias of <see cref="DecisionModel"/>. Returns the same model, which also has the deprecated
    /// <see cref="Operations.IEvaluationCaller.DoEvaluateAsync"/>. Maps to <c>evaluationModel</c>.
    /// </summary>
    [Obsolete("Use DecisionModel instead.")]
    public Operations.IEvaluationDecisionCaller EvaluationModel(string modelId)
    {
        return Operations.DecisionModels.AsEvaluationModel(DecisionModel(modelId));
    }

    /// <summary>
    /// Returns the extraction model registered under <paramref name="modelId"/>, or asks <see cref="FallbackProvider"/>
    /// when it implements <see cref="Operations.IExtractionProvider"/>.
    /// </summary>
    public Operations.IExtractionCaller ExtractionModel(string modelId)
    {
        var factory = Find(_extraction, modelId, "extractionModel", fallback =>
            (fallback as Operations.IExtractionProvider)?.ExtractionModel(modelId) is { } found
                ? () => found
                : null
        );
        return Operations.DecisionModels.ResolveExtractionModel(factory());
    }

    Operations.IExtractionCaller? Operations.IExtractionProvider.ExtractionModel(string modelId)
    {
        return ExtractionModel(modelId);
    }

    /// <inheritdoc />
    public override IFileStore FileStore()
    {
        return Files ?? FallbackProvider?.FileStore() ?? base.FileStore();
    }

    /// <inheritdoc />
    public override ISkillStore SkillStore()
    {
        return Skills ?? FallbackProvider?.SkillStore() ?? base.SkillStore();
    }

    private CustomProvider Add<T>(Dictionary<string, T> models, string modelId, T model)
    {
        if (modelId is null)
        {
            throw new ArgumentNullException(nameof(modelId));
        }

        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        models[modelId] = model;
        return this;
    }

    // A fallback that returns null has no such model.
    private T Find<T>(
        Dictionary<string, T> models,
        string modelId,
        string modelType,
        Func<ProviderBase, T?> fallback
    )
        where T : class
    {
        if (models.TryGetValue(modelId, out var model))
        {
            return model;
        }

        return (FallbackProvider == null ? null : fallback(FallbackProvider))
            ?? throw new NoSuchModelError(modelId, modelType);
    }
}

/// <summary>Agent settings with call options. Maps to <c>ToolLoopAgentSettings</c> with a <c>CALL_OPTIONS</c> type.</summary>
/// <typeparam name="TCallOptions">The type of the per-call options. Maps to <c>CALL_OPTIONS</c>.</typeparam>
public sealed class AgentOptions<TCallOptions> : AgentOptions
{
    /// <summary>
    /// The JSON schema of the call options. Each call validates its options against it before <see cref="PrepareCall"/> runs
    /// and throws a <see cref="Vercel.AI.ProviderUtils.TypeValidationException"/> (context field <c>options</c>) on a mismatch. Maps to <c>callOptionsSchema</c>.
    /// </summary>
    public System.Text.Json.Nodes.JsonNode? CallOptionsSchema { get; set; }

    /// <summary>
    /// Called before each call with the options the agent would use and the validated call options. Return replacement options, or null to keep them.
    /// It replaces <see cref="AgentOptions.PrepareCall"/>, which <see cref="Agent{TCallOptions}"/> ignores. Maps to <c>prepareCall</c>.
    /// </summary>
    public new Func<
        GenerateTextOptions,
        TCallOptions,
        GenerateTextOptions?
    >? PrepareCall { get; set; }
}

/// <summary>An agent that takes options on each call. Maps to <c>ToolLoopAgent</c> with a <c>CALL_OPTIONS</c> type.</summary>
/// <typeparam name="TCallOptions">The type of the per-call options. Maps to <c>CALL_OPTIONS</c>.</typeparam>
public sealed class Agent<TCallOptions> : IAgent<TCallOptions>
{
    private static readonly System.Text.Json.JsonSerializerOptions s_json = new(
        System.Text.Json.JsonSerializerDefaults.Web
    );

    private readonly Agent _agent;

    /// <summary>Creates an agent. The default stop condition is 10 steps.</summary>
    public Agent(AgentOptions<TCallOptions> options, IAiClient? client = null)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        Options = options;
        _agent = new Agent(options, client);
    }

    /// <summary>Agent options.</summary>
    public AgentOptions<TCallOptions> Options { get; }

    /// <summary>The tools of the agent. Maps to <c>tools</c>.</summary>
    public IReadOnlyList<Tool>? Tools => Options.Tools;

    /// <summary>
    /// Generates a reply to <paramref name="prompt"/> with <paramref name="options"/>, which are validated against
    /// <see cref="AgentOptions{TCallOptions}.CallOptionsSchema"/> first. Maps to <c>generate</c>.
    /// </summary>
    public Task<GenerateTextResult> GenerateAsync(
        string prompt,
        TCallOptions options,
        CancellationToken cancellationToken = default,
        TimeoutConfiguration? timeout = null,
        ISteering? experimentalSteering = null,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox = null,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null
    )
    {
        var prepare = PrepareFor(options);
        return _agent.GenerateCoreAsync(
            _agent.CreateOptions(prompt, null, experimentalSandbox),
            cancellationToken,
            timeout,
            experimentalSteering,
            prepare,
            onStepStart,
            onToolExecutionStart,
            onToolExecutionEnd
        );
    }

    /// <summary>Generates a reply to <paramref name="messages"/> with <paramref name="options"/>. Maps to <c>generate</c>.</summary>
    public Task<GenerateTextResult> GenerateAsync(
        IReadOnlyList<ModelMessage> messages,
        TCallOptions options,
        CancellationToken cancellationToken = default,
        TimeoutConfiguration? timeout = null,
        ISteering? experimentalSteering = null,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox = null,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null
    )
    {
        var prepare = PrepareFor(options);
        return _agent.GenerateCoreAsync(
            _agent.CreateOptions(
                null,
                messages ?? throw new ArgumentNullException(nameof(messages)),
                experimentalSandbox
            ),
            cancellationToken,
            timeout,
            experimentalSteering,
            prepare,
            onStepStart,
            onToolExecutionStart,
            onToolExecutionEnd
        );
    }

    /// <summary>Streams a reply to <paramref name="prompt"/> with <paramref name="options"/>. Maps to <c>stream</c>.</summary>
    public StreamTextResult Stream(
        string prompt,
        TCallOptions options,
        CancellationToken cancellationToken = default,
        Vercel.AI.Util.AbortSignal? abortSignal = null,
        TimeoutConfiguration? timeout = null,
        ISteering? experimentalSteering = null,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox = null,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null
    )
    {
        var prepare = PrepareFor(options);
        return _agent.StreamCore(
            _agent.CreateOptions(prompt, null, experimentalSandbox),
            cancellationToken,
            abortSignal,
            timeout,
            experimentalSteering,
            null,
            prepare,
            onStepStart,
            onToolExecutionStart,
            onToolExecutionEnd
        );
    }

    /// <summary>Streams a reply to <paramref name="messages"/> with <paramref name="options"/>. Maps to <c>stream</c>.</summary>
    public StreamTextResult Stream(
        IReadOnlyList<ModelMessage> messages,
        TCallOptions options,
        CancellationToken cancellationToken = default,
        Vercel.AI.Util.AbortSignal? abortSignal = null,
        TimeoutConfiguration? timeout = null,
        ISteering? experimentalSteering = null,
        Func<StepResult, CancellationToken, Task>? onStepEnd = null,
        Vercel.AI.ProviderUtils.ISandboxSession? experimentalSandbox = null,
        Func<GenerateTextStepStartEvent, CancellationToken, Task>? onStepStart = null,
        Func<ToolExecutionStartEvent, CancellationToken, Task>? onToolExecutionStart = null,
        Func<ToolExecutionEndEvent, CancellationToken, Task>? onToolExecutionEnd = null
    )
    {
        var prepare = PrepareFor(options);
        return _agent.StreamCore(
            _agent.CreateOptions(
                null,
                messages ?? throw new ArgumentNullException(nameof(messages)),
                experimentalSandbox
            ),
            cancellationToken,
            abortSignal,
            timeout,
            experimentalSteering,
            onStepEnd,
            prepare,
            onStepStart,
            onToolExecutionStart,
            onToolExecutionEnd
        );
    }

    // prepareCall: validateTypes({ value: options, schema: callOptionsSchema, context: { field: 'options' } })
    private Func<GenerateTextOptions, GenerateTextOptions?> PrepareFor(TCallOptions options)
    {
        var validated = options;
        if (Options.CallOptionsSchema is not null && options is not null)
        {
            var element = System.Text.Json.JsonSerializer.SerializeToElement(options, s_json);
            var projected = Vercel.AI.ProviderUtils.JsonParsing.ValidateTypes(
                element,
                Options.CallOptionsSchema,
                new Vercel.AI.ProviderUtils.TypeValidationContext { Field = "options" }
            );
            validated = projected.Deserialize<TCallOptions>(s_json)!;
        }

        return call => Options.PrepareCall?.Invoke(call, validated);
    }
}
