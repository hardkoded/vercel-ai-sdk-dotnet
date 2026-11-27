// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Ui;

namespace Vercel.AI.AspNetCore;

public static partial class UIMessageStreamConverter
{
    /// <summary>
    /// Determines the message id to use for a response message.
    /// If the last message is an assistant message, its id is reused (continuation).
    /// Otherwise, a new id is generated or the provided id is used. Maps to <c>getResponseUIMessageId</c>.
    /// </summary>
    /// <param name="originalMessages">
    /// The original messages. When <c>null</c>, the result is <c>null</c>, since client-side id generation is used in non-persistence mode.
    /// </param>
    /// <param name="responseMessageId">The response message id.</param>
    /// <returns>The message id to use, or <c>null</c> when there is no persistence mode.</returns>
    public static string? GetResponseUIMessageId(
        IReadOnlyList<UIMessage>? originalMessages,
        string responseMessageId
    )
    {
        return GetResponseUIMessageId(originalMessages, () => responseMessageId);
    }

    /// <summary>
    /// Determines the message id to use for a response message.
    /// If the last message is an assistant message, its id is reused (continuation).
    /// Otherwise, a new id is generated or the provided id is used. Maps to <c>getResponseUIMessageId</c>.
    /// </summary>
    /// <param name="originalMessages">
    /// The original messages. When <c>null</c>, the result is <c>null</c>, since client-side id generation is used in non-persistence mode.
    /// </param>
    /// <param name="responseMessageId">An id generator function.</param>
    /// <returns>The message id to use, or <c>null</c> when there is no persistence mode.</returns>
    public static string? GetResponseUIMessageId(
        IReadOnlyList<UIMessage>? originalMessages,
        Func<string> responseMessageId
    )
    {
        // when there are no original messages (i.e. no persistence),
        // the assistant message id generation is handled on the client side.
        if (originalMessages is null)
        {
            return null;
        }

        var lastMessage =
            originalMessages.Count > 0 ? originalMessages[originalMessages.Count - 1] : null;

        return lastMessage?.Role == UIMessageRole.Assistant ? lastMessage.Id : responseMessageId();
    }
}
