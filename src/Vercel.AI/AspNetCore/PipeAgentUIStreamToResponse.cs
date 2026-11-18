// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

/// <summary>Options of <see cref="UIMessageStreamConverter.PipeAgentUIStreamToResponse"/>. Maps to the options of <c>pipeAgentUIStreamToResponse</c>.</summary>
public sealed class PipeAgentUIStreamToResponseOptions : CreateAgentUIStreamResponseOptions
{
    /// <summary>The response to pipe to. Maps to <c>response</c>.</summary>
    public ServerResponse? Response { get; set; }
}

public static partial class UIMessageStreamConverter
{
    /// <summary>Pipes the agent UI message stream to a <see cref="ServerResponse"/>. Maps to <c>pipeAgentUIStreamToResponse</c>.</summary>
    public static async Task PipeAgentUIStreamToResponse(
        PipeAgentUIStreamToResponseOptions options,
        CancellationToken cancellationToken = default
    )
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var response = options.Response
            ?? throw new ArgumentException("Response is required.", nameof(options));
        var stream = await CreateAgentUIStreamAsync(options, cancellationToken)
            .ConfigureAwait(false);
        await UIMessageStreamResponses
            .PipeUIMessageStreamToResponse(
                response,
                ReadableStream<System.Text.Json.Nodes.JsonObject>.From(stream),
                options.ToInit(),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
