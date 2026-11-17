// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

/// <summary>Options of <see cref="UIMessageStreamConverter.CreateAgentUIStreamAsync"/>. Maps to the options of <c>createAgentUIStream</c>.</summary>
public sealed class CreateAgentUIStreamOptions
{
    /// <summary>The agent to run. Maps to <c>agent</c>.</summary>
    public IAgent? Agent { get; set; }

    /// <summary>The input UI messages. Maps to <c>uiMessages</c>.</summary>
    public IReadOnlyList<UIMessage>? UIMessages { get; set; }

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

    /// <summary>Called when each generation step ends. Maps to <c>onStepEnd</c>.</summary>
    public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; set; }

    /// <summary>Called with the accumulated UI message after each step. Maps to <c>onUIMessageStepEnd</c>.</summary>
    public Func<UIMessageStreamOnStepEndEvent, Task>? OnUIMessageStepEnd { get; set; }

    /// <summary>
    /// The UI message stream options. The tools of the agent and <see cref="OnUIMessageStepEnd"/> replace its
    /// <see cref="ToUIMessageChunkOptions.Tools"/> and <see cref="ToUIMessageChunkOptions.OnStepEnd"/>.
    /// Its <see cref="ToUIMessageChunkOptions.OriginalMessages"/> default to the validated <see cref="UIMessages"/>.
    /// </summary>
    public ToUIMessageChunkOptions? UIMessageStreamOptions { get; set; }
}

public static partial class UIMessageStreamConverter
{
    /// <summary>Runs the agent and streams the output as a UI message stream. Maps to <c>createAgentUIStream</c>.</summary>
    public static async Task<IAsyncEnumerable<JsonObject>> CreateAgentUIStreamAsync(
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
        var uiMessages = options.UIMessages
            ?? throw new ArgumentException("UIMessages is required.", nameof(options));

        // other agents would ignore the steering object and never resolve its messages:
        if (options.ExperimentalSteering != null && agent is not Agent)
        {
            throw new InvalidArgumentException(
                "experimental_steering",
                options.ExperimentalSteering,
                "Steering is only supported by ToolLoopAgent."
            );
        }

        var tools = agent.Tools?.ToDictionary(tool => tool.Name, StringComparer.Ordinal);

        var validatedMessages = await UIMessageValidation
            .ValidateUIMessagesForAgentAsync(
                new ValidateUIMessagesOptions { Messages = uiMessages, Tools = tools }
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

        var result = agent.Stream(
            modelMessages,
            cancellationToken,
            options.AbortSignal,
            options.Timeout,
            options.ExperimentalSteering,
            options.OnStepEnd
        );

        var streamOptions = options.UIMessageStreamOptions;
#pragma warning disable CS0618
        var chunkOptions = new ToUIMessageChunkOptions
        {
            Tools = agent.Tools,
            SendReasoning = streamOptions?.SendReasoning ?? true,
            SendSources = streamOptions?.SendSources ?? false,
            SendStart = streamOptions?.SendStart ?? true,
            SendFinish = streamOptions?.SendFinish ?? true,
            OnError = streamOptions?.OnError,
            RedactToolErrors = streamOptions?.RedactToolErrors,
            MessageMetadata = streamOptions?.MessageMetadata,
            MessageMetadataSelector = streamOptions?.MessageMetadataSelector,
            ResponseMessageId = streamOptions?.ResponseMessageId,
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
