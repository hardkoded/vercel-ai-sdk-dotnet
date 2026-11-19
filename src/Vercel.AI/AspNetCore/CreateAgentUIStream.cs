// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

/// <summary>The options of <c>createAgentUIStream</c> that do not depend on the agent. The .NET split of the generic <c>CALL_OPTIONS</c> parameter.</summary>
public abstract class CreateAgentUIStreamOptionsBase
{
    internal CreateAgentUIStreamOptionsBase() { }

    /// <summary>The input UI messages. Maps to <c>uiMessages</c>. Set <see cref="UIMessagesJson"/> instead for messages that are not typed yet.</summary>
    public IReadOnlyList<UIMessage>? UIMessages { get; set; }

    /// <summary>The input UI messages as a JSON array that the call validates, like the <c>unknown[]</c> of <c>uiMessages</c>. Set either this or <see cref="UIMessages"/>.</summary>
    public JsonElement? UIMessagesJson { get; set; }

    /// <summary>Converts custom UI data parts to text or file model message parts. Maps to <c>convertDataPart</c>.</summary>
    public Func<DataUIPart, UserContentPart?>? ConvertDataPart { get; set; }

    /// <summary>Aborts the stream. Maps to <c>abortSignal</c>.</summary>
    public AbortSignal? AbortSignal { get; set; }

    /// <summary>The timeout of the call. Maps to <c>timeout</c>.</summary>
    public TimeoutConfiguration? Timeout { get; set; }

    /// <summary>
    /// Adds user messages to the run at the next step boundary. Only <see cref="Vercel.AI.Agent"/> supports it, any other
    /// <see cref="IAgent"/> makes the call throw <see cref="InvalidArgumentException"/>. Maps to <c>experimental_steering</c>.
    /// </summary>
    public ISteering? ExperimentalSteering { get; set; }

    /// <summary>The sandbox environment that is passed through to tool execution. Maps to <c>experimental_sandbox</c>.</summary>
    public ISandboxSession? ExperimentalSandbox { get; set; }

    /// <summary>Called when each generation step ends. Maps to <c>onStepEnd</c>.</summary>
    public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }

    /// <summary>Deprecated alias for <see cref="OnStepEnd"/>. Maps to <c>onStepFinish</c>.</summary>
    [Obsolete("Use OnStepEnd instead.")]
    public Func<StepResult, CancellationToken, Task>? OnStepFinish { get; set; }

    /// <summary>Called with the accumulated UI message after each step. Maps to <c>onUIMessageStepEnd</c>.</summary>
    public Func<UIMessageStreamOnStepEndEvent, Task>? OnUIMessageStepEnd { get; set; }

    /// <summary>
    /// The UI message stream options. The tools of the agent and <see cref="OnUIMessageStepEnd"/> replace its
    /// <see cref="UIMessageStreamOptions.Tools"/> and <see cref="UIMessageStreamOptions.OnStepEnd"/>.
    /// Its <see cref="UIMessageStreamOptions.OriginalMessages"/> default to the validated <see cref="UIMessages"/>.
    /// </summary>
    public UIMessageStreamOptions? UIMessageStreamOptions { get; set; }
}

/// <summary>Options of <see cref="UIMessageStreamConverter.CreateAgentUIStreamAsync(CreateAgentUIStreamOptions, CancellationToken)"/>. Maps to the options of <c>createAgentUIStream</c>.</summary>
public class CreateAgentUIStreamOptions : CreateAgentUIStreamOptionsBase
{
    /// <summary>The agent to run. Maps to <c>agent</c>.</summary>
    public IAgent? Agent { get; set; }
}

/// <summary>Options of <see cref="UIMessageStreamConverter.CreateAgentUIStreamAsync{TCallOptions}(CreateAgentUIStreamOptions{TCallOptions}, CancellationToken)"/>. Maps to the options of <c>createAgentUIStream</c> with a <c>CALL_OPTIONS</c> type.</summary>
/// <typeparam name="TCallOptions">The type of the call options of the agent. Maps to <c>CALL_OPTIONS</c>.</typeparam>
public class CreateAgentUIStreamOptions<TCallOptions> : CreateAgentUIStreamOptionsBase
{
    /// <summary>The agent to run. Maps to <c>agent</c>.</summary>
    public IAgent<TCallOptions>? Agent { get; set; }

    /// <summary>The options of the agent call. Maps to <c>options</c>.</summary>
    public TCallOptions? Options { get; set; }
}

public static partial class UIMessageStreamConverter
{
    /// <summary>Runs the agent and streams the output as a UI message stream. Maps to <c>createAgentUIStream</c>.</summary>
    public static Task<IAsyncEnumerable<JsonObject>> CreateAgentUIStreamAsync(
        CreateAgentUIStreamOptions options,
        CancellationToken cancellationToken = default
    )
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var agent = options.Agent
            ?? throw new ArgumentException("Agent is required.", nameof(options));
        return CreateAgentUIStreamCoreAsync(
            options,
            agent.Tools,
            agent is Agent,
            (messages, onStepEnd) =>
                agent.Stream(
                    messages,
                    cancellationToken,
                    options.AbortSignal,
                    options.Timeout,
                    options.ExperimentalSteering,
                    onStepEnd,
                    options.ExperimentalSandbox
                ),
            cancellationToken
        );
    }

    /// <summary>Runs an agent with call options and streams the output as a UI message stream. Maps to <c>createAgentUIStream</c> with <c>options</c>.</summary>
    /// <typeparam name="TCallOptions">The type of the call options of the agent. Maps to <c>CALL_OPTIONS</c>.</typeparam>
    public static Task<IAsyncEnumerable<JsonObject>> CreateAgentUIStreamAsync<TCallOptions>(
        CreateAgentUIStreamOptions<TCallOptions> options,
        CancellationToken cancellationToken = default
    )
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var agent = options.Agent
            ?? throw new ArgumentException("Agent is required.", nameof(options));
        return CreateAgentUIStreamCoreAsync(
            options,
            agent.Tools,
            agent is Agent<TCallOptions>,
            (messages, onStepEnd) =>
                agent.Stream(
                    messages,
                    options.Options!,
                    cancellationToken,
                    options.AbortSignal,
                    options.Timeout,
                    options.ExperimentalSteering,
                    onStepEnd,
                    options.ExperimentalSandbox
                ),
            cancellationToken
        );
    }

    private static async Task<IAsyncEnumerable<JsonObject>> CreateAgentUIStreamCoreAsync(
        CreateAgentUIStreamOptionsBase options,
        IReadOnlyList<Tool>? agentTools,
        bool supportsSteering,
        Func<
            IReadOnlyList<ModelMessage>,
            Func<StepResult, CancellationToken, Task>?,
            StreamTextResult
        > stream,
        CancellationToken cancellationToken
    )
    {
        if (options.UIMessages is null && options.UIMessagesJson is null)
        {
            throw new ArgumentException("UIMessages is required.", nameof(options));
        }

        // other agents would ignore the steering object and never resolve its messages:
        if (options.ExperimentalSteering != null && !supportsSteering)
        {
            throw new InvalidArgumentException(
                "experimental_steering",
                options.ExperimentalSteering,
                "Steering is only supported by ToolLoopAgent."
            );
        }

        var tools = agentTools?.ToDictionary(tool => tool.Name, StringComparer.Ordinal);

        var validatedMessages = await UIMessageValidation
            .ValidateUIMessagesForAgentAsync(
                new ValidateUIMessagesOptions
                {
                    Messages = options.UIMessages,
                    MessagesJson = options.UIMessagesJson,
                    Tools = tools,
                }
            )
            .ConfigureAwait(false);

        var modelMessages = UIMessageConversion.ConvertToModelMessages(
            validatedMessages,
            new ConvertToModelMessagesOptions
            {
                Tools = tools,
                ConvertDataPart = options.ConvertDataPart,
            }
        );

#pragma warning disable CS0618
        var result = stream(modelMessages, options.OnStepEnd ?? options.OnStepFinish);
#pragma warning restore CS0618

        var streamOptions = options.UIMessageStreamOptions;
#pragma warning disable CS0618
        var chunkOptions = new UIMessageStreamOptions
        {
            Tools = agentTools,
            SendReasoning = streamOptions?.SendReasoning ?? true,
            SendSources = streamOptions?.SendSources ?? false,
            SendStart = streamOptions?.SendStart ?? true,
            SendFinish = streamOptions?.SendFinish ?? true,
            OnError = streamOptions?.OnError,
            RedactToolErrors = streamOptions?.RedactToolErrors,
            MessageMetadata = streamOptions?.MessageMetadata,
            GenerateMessageId = streamOptions?.GenerateMessageId,
            OriginalMessages = streamOptions?.OriginalMessages ?? validatedMessages,
            OnStepEnd = options.OnUIMessageStepEnd,
            OnEnd = streamOptions?.OnEnd,
            OnFinish = streamOptions?.OnFinish,
        };
#pragma warning restore CS0618

        return ToUIMessageStream(result.Stream(cancellationToken), chunkOptions, cancellationToken);
    }
}
