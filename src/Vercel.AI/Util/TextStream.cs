// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;

namespace Vercel.AI.Util;

/// <summary>One part of a model text stream.</summary>
public sealed class TextStreamPart
{
    /// <summary>Creates a part.</summary>
    public TextStreamPart(string type, string? id = null, string? text = null, string? delta = null)
    {
        Type = type;
        Id = id;
        Text = text;
        Delta = delta;
    }

    /// <summary>Part kind, such as <c>text-delta</c>.</summary>
    public string Type { get; }

    /// <summary>Part id.</summary>
    public string? Id { get; }

    /// <summary>Text carried by a <c>text-delta</c> part.</summary>
    public string? Text { get; }

    /// <summary>Delta carried by provider stream parts that use <c>delta</c> instead of <c>text</c>.</summary>
    public string? Delta { get; }
}

/// <summary>HTTP response whose body is a UTF-8 text stream.</summary>
public sealed class TextStreamResponse
{
    /// <summary>Creates the response.</summary>
    public TextStreamResponse(int status, string? statusText, HeaderCollection headers, ReadableStream<byte[]> body)
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

    /// <summary>Response headers, including the default content type when the caller did not set one.</summary>
    public HeaderCollection Headers { get; }

    /// <summary>UTF-8 encoded body chunks.</summary>
    public ReadableStream<byte[]> Body { get; }
}

/// <summary>Text-stream helpers: keep text deltas, build a response, and write it to a server response.</summary>
public static class TextStreams
{
    /// <summary>Yields only <c>text-delta</c> text from <paramref name="stream"/>.</summary>
    public static ReadableStream<string> ToTextStream(ReadableStream<TextStreamPart> stream)
    {
        var reader = stream.GetReader();
        return new ReadableStream<string>(pull: async delegate (ReadableStreamController<string> controller)
        {
            while (true)
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

                if (read.HasValue && read.Value.Type == "text-delta")
                {
                    controller.Enqueue(read.Value.Text ?? string.Empty);
                    return;
                }
            }
        });
    }

    /// <summary>
    /// Builds a response with <c>content-type: text/plain; charset=utf-8</c> unless already set.
    /// Each string chunk is encoded as its own UTF-8 body chunk.
    /// </summary>
    public static TextStreamResponse CreateTextStreamResponse(ReadableStream<string> stream, int? status = null, string? statusText = null, HeaderCollection? headers = null)
    {
        var prepared = PrepareHeaders.Apply(headers, new Dictionary<string, string>
        {
            { "content-type", "text/plain; charset=utf-8" },
        });
        return new TextStreamResponse(status ?? 200, statusText, prepared, Encode(stream));
    }

    /// <summary>Encodes <paramref name="stream"/> as UTF-8 and writes it to <paramref name="response"/>.</summary>
    public static Task PipeTextStreamToResponseAsync(ServerResponse response, ReadableStream<string> stream, int? status = null, string? statusText = null, HeaderCollection? headers = null)
    {
        var prepared = PrepareHeaders.Apply(headers, new Dictionary<string, string>
        {
            { "content-type", "text/plain; charset=utf-8" },
        });
        return ServerResponseWriter.WriteAsync(response, Encode(stream), status, statusText, prepared);
    }

    private static ReadableStream<byte[]> Encode(ReadableStream<string> stream)
    {
        var reader = stream.GetReader();
        return new ReadableStream<byte[]>(pull: async delegate (ReadableStreamController<byte[]> controller)
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

            controller.Enqueue(Encoding.UTF8.GetBytes(read.HasValue ? read.Value : string.Empty));
        });
    }
}
