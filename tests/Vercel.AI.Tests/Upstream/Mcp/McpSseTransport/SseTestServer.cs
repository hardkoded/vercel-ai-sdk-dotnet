// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Channels;

namespace Vercel.AI.Tests.Upstream.Mcp.McpSseTransport;

/// <summary>Stands in for upstream's <c>TestResponseController</c>: a response body the test writes to and closes.</summary>
internal sealed class TestResponseController
{
    private readonly Channel<byte[]> _channel = Channel.CreateUnbounded<byte[]>();

    public Stream Stream => new ControlledStream(_channel.Reader);

    public void Write(string text)
    {
        _channel.Writer.TryWrite(Encoding.UTF8.GetBytes(text));
    }

    public void Close()
    {
        _channel.Writer.TryComplete();
    }

    private sealed class ControlledStream : Stream
    {
        private readonly ChannelReader<byte[]> _reader;
        private byte[] _current = Array.Empty<byte>();
        private int _offset;

        public ControlledStream(ChannelReader<byte[]> reader)
        {
            _reader = reader;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            while (_offset >= _current.Length)
            {
                if (!await _reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    return 0;
                }

                if (_reader.TryRead(out var next))
                {
                    _current = next;
                    _offset = 0;
                }
            }

            var length = Math.Min(count, _current.Length - _offset);
            Array.Copy(_current, _offset, buffer, offset, length);
            _offset += length;
            return length;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var temp = new byte[buffer.Length];
            var count = await ReadAsync(temp, 0, temp.Length, cancellationToken).ConfigureAwait(false);
            temp.AsMemory(0, count).CopyTo(buffer);
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}

internal sealed record RecordedCall(string Method, string Url, Dictionary<string, string> Headers, string Body);

/// <summary>Stands in for upstream's <c>createTestServer</c>: routes the SSE <c>GET</c> and the message <c>POST</c>.</summary>
internal sealed class SseTestServer : HttpMessageHandler
{
    public const string SseUrl = "http://localhost:3000/sse";

    public const string MessagesUrl = "http://localhost:3000/messages";

    public List<RecordedCall> Calls { get; } = new();

    /// <summary>Builds the response to the SSE <c>GET</c>.</summary>
    public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> SseResponse { get; set; } =
        (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

    /// <summary>Builds the response to the message <c>POST</c>.</summary>
    public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> MessagesResponse { get; set; } =
        (_, _) => Task.FromResult(Json("{\"ok\":true,\"message\":\"Created\",\"code\":201}"));

    public static HttpResponseMessage Json(string body)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
    }

    public static HttpResponseMessage Error(HttpStatusCode status, string reason, string body)
    {
        return new HttpResponseMessage(status)
        {
            ReasonPhrase = reason,
            Content = new StringContent(body, Encoding.UTF8, "text/plain"),
        };
    }

    public static HttpResponseMessage EventStream(TestResponseController controller)
    {
        var content = new StreamContent(controller.Stream);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/event-stream");
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    public void Controlled(TestResponseController controller)
    {
        SseResponse = (_, _) => Task.FromResult(EventStream(controller));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in request.Headers)
        {
            headers[header.Key] = string.Join(", ", header.Value);
        }

        var body = string.Empty;
        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key] = string.Join(", ", header.Value);
            }

            body = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }

        lock (Calls)
        {
            Calls.Add(new RecordedCall(request.Method.Method, request.RequestUri!.AbsoluteUri, headers, body));
        }

        return request.Method == HttpMethod.Get
            ? await SseResponse(request, cancellationToken).ConfigureAwait(false)
            : await MessagesResponse(request, cancellationToken).ConfigureAwait(false);
    }
}
