// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

/// <summary>Options of <see cref="UIMessageStreamConverter.CreateAgentUIStreamResponse"/>. Maps to the options of <c>createAgentUIStreamResponse</c>.</summary>
public class CreateAgentUIStreamResponseOptions : CreateAgentUIStreamOptions
{
    /// <summary>Additional headers for the response. Maps to <c>headers</c>.</summary>
    public HeaderCollection? Headers { get; set; }

    /// <summary>The status code for the response. Maps to <c>status</c>.</summary>
    public int? Status { get; set; }

    /// <summary>The status text for the response. Maps to <c>statusText</c>.</summary>
    public string? StatusText { get; set; }

    /// <summary>Optional interval in milliseconds for sending SSE keep-alive comments. Maps to <c>keepAliveMs</c>.</summary>
    public int? KeepAliveMs { get; set; }

    /// <summary>Consumes a copy of the SSE stream independently. Maps to <c>consumeSseStream</c>.</summary>
    public Func<ReadableStream<string>, Task?>? ConsumeSseStream { get; set; }

    internal UIMessageStreamResponseInit ToInit() =>
        new()
        {
            Headers = Headers,
            Status = Status,
            StatusText = StatusText,
            KeepAliveMs = KeepAliveMs,
            ConsumeSseStream = ConsumeSseStream,
        };
}

/// <summary>Options of <see cref="UIMessageStreamConverter.CreateAgentUIStreamResponse{TCallOptions}(CreateAgentUIStreamResponseOptions{TCallOptions}, CancellationToken)"/>. Maps to the options of <c>createAgentUIStreamResponse</c> with a <c>CALL_OPTIONS</c> type.</summary>
/// <typeparam name="TCallOptions">The type of the call options of the agent. Maps to <c>CALL_OPTIONS</c>.</typeparam>
public class CreateAgentUIStreamResponseOptions<TCallOptions>
    : CreateAgentUIStreamOptions<TCallOptions>
{
    /// <summary>Additional headers for the response. Maps to <c>headers</c>.</summary>
    public HeaderCollection? Headers { get; set; }

    /// <summary>The status code for the response. Maps to <c>status</c>.</summary>
    public int? Status { get; set; }

    /// <summary>The status text for the response. Maps to <c>statusText</c>.</summary>
    public string? StatusText { get; set; }

    /// <summary>Optional interval in milliseconds for sending SSE keep-alive comments. Maps to <c>keepAliveMs</c>.</summary>
    public int? KeepAliveMs { get; set; }

    /// <summary>Consumes a copy of the SSE stream independently. Maps to <c>consumeSseStream</c>.</summary>
    public Func<ReadableStream<string>, Task?>? ConsumeSseStream { get; set; }

    internal UIMessageStreamResponseInit ToInit() =>
        new()
        {
            Headers = Headers,
            Status = Status,
            StatusText = StatusText,
            KeepAliveMs = KeepAliveMs,
            ConsumeSseStream = ConsumeSseStream,
        };
}

public static partial class UIMessageStreamConverter
{
    /// <summary>Runs an agent with call options and returns a response object with a UI message stream. Maps to <c>createAgentUIStreamResponse</c> with <c>options</c>.</summary>
    /// <typeparam name="TCallOptions">The type of the call options of the agent. Maps to <c>CALL_OPTIONS</c>.</typeparam>
    public static async Task<UIMessageStreamResponse> CreateAgentUIStreamResponse<TCallOptions>(
        CreateAgentUIStreamResponseOptions<TCallOptions> options,
        CancellationToken cancellationToken = default
    )
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var stream = await CreateAgentUIStreamAsync(options, cancellationToken)
            .ConfigureAwait(false);
        return UIMessageStreamResponses.CreateUIMessageStreamResponse(
            ReadableStream<System.Text.Json.Nodes.JsonObject>.From(stream),
            options.ToInit()
        );
    }

    /// <summary>Runs the agent and returns a response object with a UI message stream. Maps to <c>createAgentUIStreamResponse</c>.</summary>
    public static async Task<UIMessageStreamResponse> CreateAgentUIStreamResponse(
        CreateAgentUIStreamResponseOptions options,
        CancellationToken cancellationToken = default
    )
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var stream = await CreateAgentUIStreamAsync(options, cancellationToken)
            .ConfigureAwait(false);
        return UIMessageStreamResponses.CreateUIMessageStreamResponse(
            ReadableStream<System.Text.Json.Nodes.JsonObject>.From(stream),
            options.ToInit()
        );
    }
}
