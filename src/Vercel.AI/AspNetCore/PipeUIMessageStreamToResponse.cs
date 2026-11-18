// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

public static partial class UIMessageStreamResponses
{
    /// <summary>
    /// Pipes a UI message stream, encoded as Server-Sent Events, to a <see cref="ServerResponse"/>. Maps to <c>pipeUIMessageStreamToResponse</c>.
    /// </summary>
    /// <param name="response">The response to write to.</param>
    /// <param name="stream">The UI message chunk stream to send.</param>
    /// <param name="init">Status, headers, keep-alive interval, and the optional SSE stream consumer.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    public static Task PipeUIMessageStreamToResponse(
        ServerResponse response,
        ReadableStream<JsonObject> stream,
        UIMessageStreamResponseInit? init = null,
        CancellationToken cancellationToken = default
    )
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var built = CreateUIMessageStreamResponse(stream, init);
        return ServerResponseWriter.WriteAsync(
            response,
            built.Body,
            built.Status,
            built.StatusText,
            built.Headers,
            cancellationToken
        );
    }
}
