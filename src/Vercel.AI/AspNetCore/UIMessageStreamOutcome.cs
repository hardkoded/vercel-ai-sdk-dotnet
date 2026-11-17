// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Ui;

namespace Vercel.AI.AspNetCore;

/// <summary>The status of a <see cref="UIMessageStreamOutcome"/>.</summary>
public enum UIMessageStreamOutcomeStatus
{
    /// <summary>The stream completed. Maps to <c>'completed'</c>.</summary>
    Completed,

    /// <summary>The stream failed. Maps to <c>'failed'</c>.</summary>
    Failed,

    /// <summary>The stream was aborted. Maps to <c>'aborted'</c>.</summary>
    Aborted,

    /// <summary>No outcome was declared. Maps to <c>'unknown'</c>.</summary>
    Unknown,
}

/// <summary>
/// The operation-level outcome of a UI message stream. This is separate from model finish reasons and individual stream chunks.
/// Fatal stream-processing failures override outcomes declared by the stream owner.
/// Consumer cancellation before an outcome is declared keeps the <see cref="UIMessageStreamOutcomeStatus.Unknown"/> status and is reported
/// separately through <see cref="UIMessageStreamOnEndEvent.IsCancelled"/>. Maps to <c>UIMessageStreamOutcome</c>.
/// </summary>
public sealed class UIMessageStreamOutcome
{
    /// <summary>Creates an outcome.</summary>
    public UIMessageStreamOutcome(UIMessageStreamOutcomeStatus status, Exception? error = null)
    {
        Status = status;
        Error = error;
    }

    /// <summary>The status.</summary>
    public UIMessageStreamOutcomeStatus Status { get; }

    /// <summary>The error of a <see cref="UIMessageStreamOutcomeStatus.Failed"/> outcome, when there is one.</summary>
    public Exception? Error { get; }
}

/// <summary>The event that <see cref="UIMessageStreamOptions.OnEnd"/> receives. Maps to <c>UIMessageStreamOnEndCallback</c>'s event.</summary>
public sealed class UIMessageStreamOnEndEvent
{
    /// <summary>Creates the event.</summary>
    public UIMessageStreamOnEndEvent(
        IReadOnlyList<UIMessage> messages,
        bool isContinuation,
        bool isAborted,
        UIMessageStreamOutcome outcome,
        UIMessage responseMessage
    )
    {
        Messages = messages;
        IsContinuation = isContinuation;
        IsAborted = isAborted;
        Outcome = outcome;
        ResponseMessage = responseMessage;
    }

    /// <summary>The updated list of UI messages.</summary>
    public IReadOnlyList<UIMessage> Messages { get; }

    /// <summary>Whether the response message continues the last original message, or a new message was created.</summary>
    public bool IsContinuation { get; }

    /// <summary>Whether the stream was aborted.</summary>
    public bool IsAborted { get; }

    /// <summary>
    /// True when the consumer cancelled the stream before an outcome was declared, for example because the client disconnected.
    /// Maps to <c>isCancelled</c>, which is only present when it is <c>true</c>.
    /// </summary>
    public bool IsCancelled { get; init; }

    /// <summary>The operation-level outcome of the stream.</summary>
    public UIMessageStreamOutcome Outcome { get; }

    /// <summary>The message that was sent to the client as a response, including the original message when it was extended.</summary>
    public UIMessage ResponseMessage { get; }

    /// <summary>The reason why the generation finished, as the <c>finish</c> chunk wrote it.</summary>
    public string? FinishReason { get; init; }
}

/// <summary>The event that <see cref="UIMessageStreamOptions.OnStepEnd"/> receives. Maps to <c>UIMessageStreamOnStepEndCallback</c>'s event.</summary>
public sealed class UIMessageStreamOnStepEndEvent
{
    /// <summary>Creates the event.</summary>
    public UIMessageStreamOnStepEndEvent(
        IReadOnlyList<UIMessage> messages,
        bool isContinuation,
        UIMessage responseMessage
    )
    {
        Messages = messages;
        IsContinuation = isContinuation;
        ResponseMessage = responseMessage;
    }

    /// <summary>The updated list of UI messages at the end of this step.</summary>
    public IReadOnlyList<UIMessage> Messages { get; }

    /// <summary>Whether the response message continues the last original message, or a new message was created.</summary>
    public bool IsContinuation { get; }

    /// <summary>The message that was sent to the client as a response, including the original message when it was extended.</summary>
    public UIMessage ResponseMessage { get; }
}
