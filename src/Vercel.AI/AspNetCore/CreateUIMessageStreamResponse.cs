// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

/// <summary>Options for a UI message stream response. Maps to <c>UIMessageStreamResponseInit</c>.</summary>
public sealed class UIMessageStreamResponseInit
{
    /// <summary>The HTTP status code. Defaults to 200.</summary>
    public int? Status { get; set; }

    /// <summary>The HTTP status text.</summary>
    public string? StatusText { get; set; }

    /// <summary>Additional HTTP headers. They take precedence over the protocol headers.</summary>
    public HeaderCollection? Headers { get; set; }

    /// <summary>
    /// Optional interval in milliseconds for sending SSE keep-alive comments.
    /// When set, an opening comment is sent immediately and additional comments are sent after the stream has been idle for the configured interval.
    /// </summary>
    public int? KeepAliveMs { get; set; }

    /// <summary>
    /// Optional callback to consume a copy of the SSE stream independently.
    /// This is useful for logging, debugging, or processing the stream in parallel.
    /// The callback receives a tee'd copy of the stream and does not block the response.
    /// </summary>
    public Func<ReadableStream<string>, Task?>? ConsumeSseStream { get; set; }
}

/// <summary>Response whose body is a UI message stream encoded as Server-Sent Events.</summary>
public sealed class UIMessageStreamResponse
{
    /// <summary>Creates the response.</summary>
    public UIMessageStreamResponse(int status, string? statusText, HeaderCollection headers, ReadableStream<byte[]> body)
    {
        Status = status;
        StatusText = statusText;
        Headers = headers;
        Body = body;
    }

    /// <summary>HTTP status. Defaults to 200.</summary>
    public int Status { get; }

    /// <summary>HTTP status text.</summary>
    public string? StatusText { get; }

    /// <summary>Response headers, including the protocol headers the caller did not set.</summary>
    public HeaderCollection Headers { get; }

    /// <summary>UTF-8 encoded SSE body chunks.</summary>
    public ReadableStream<byte[]> Body { get; }
}

/// <summary>Builds UI message stream responses. Maps to <c>createUIMessageStreamResponse</c>.</summary>
public static class UIMessageStreamResponses
{
    /// <summary>
    /// Creates a response from a UI message stream. The stream is transformed to Server-Sent Events (SSE) format.
    /// </summary>
    /// <param name="stream">The UI message chunk stream to send.</param>
    /// <param name="init">Status, headers, keep-alive interval, and the optional SSE stream consumer.</param>
    /// <returns>A response with the UI message stream as the body.</returns>
    public static UIMessageStreamResponse CreateUIMessageStreamResponse(ReadableStream<JsonObject> stream, UIMessageStreamResponseInit? init = null)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        init ??= new UIMessageStreamResponseInit();
        UIMessageStreamResult.ValidateKeepAlive(init.KeepAliveMs);

        var sseStream = JsonToSse(stream);
        if (init.KeepAliveMs is not null)
        {
            sseStream = ReadableStream<string>.From(
                UIMessageStreamResult.CreateSseStreamWithKeepAlive(new AsyncIterableStream<string>(sseStream), init.KeepAliveMs));
        }

        // when the consumeSseStream is provided, we need to tee the stream
        // and send the second part to the consumeSseStream function
        // so that it can be consumed by the client independently
        if (init.ConsumeSseStream is { } consumeSseStream)
        {
            var (stream1, stream2) = sseStream.Tee();
            sseStream = stream1;
            Consume(consumeSseStream, stream2); // no await (do not block the response)
        }

        return new UIMessageStreamResponse(
            init.Status ?? 200,
            init.StatusText,
            PrepareHeaders.Apply(init.Headers, UIMessageStreamResult.ProtocolHeaders),
            Encode(sseStream));
    }

    private static void Consume(Func<ReadableStream<string>, Task?> consumeSseStream, ReadableStream<string> stream)
    {
        try
        {
            var task = consumeSseStream(stream);
            if (task is not null)
            {
                // The response does not wait for the consumer. A failing consumer must not break it.
                _ = task.ContinueWith(static completed => _ = completed.Exception, TaskContinuationOptions.OnlyOnFaulted);
            }
        }
        catch (Exception)
        {
            // A consumer that throws synchronously must not break the response either.
        }
    }

    // JsonToSseTransformStream
    private static ReadableStream<string> JsonToSse(ReadableStream<JsonObject> stream)
    {
        var reader = stream.GetReader();
        return new ReadableStream<string>(
            pull: async controller =>
            {
                var read = await reader.ReadAsync().ConfigureAwait(false);
                if (read.Rejected)
                {
                    controller.Error(read.Reason);
                    return;
                }

                if (read.Done)
                {
                    controller.Enqueue("data: [DONE]\n\n");
                    controller.Close();
                    return;
                }

                controller.Enqueue("data: " + read.Value.ToJsonString() + "\n\n");
            },
            cancel: () => reader.CancelAsync());
    }

    // TextEncoderStream
    private static ReadableStream<byte[]> Encode(ReadableStream<string> stream)
    {
        var reader = stream.GetReader();
        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        return new ReadableStream<byte[]>(
            pull: async controller =>
            {
                var read = await reader.ReadAsync().ConfigureAwait(false);
                if (read.Rejected)
                {
                    controller.Error(read.Reason);
                    return;
                }

                if (read.Done)
                {
                    controller.Close();
                    return;
                }

                controller.Enqueue(utf8.GetBytes(read.Value));
            },
            cancel: () => reader.CancelAsync());
    }
}
