// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using System.Threading.Channels;

namespace Vercel.AI.Tests.Streaming;

/// <summary>A response body the test feeds one chunk at a time. It records when the reader closes it.</summary>
internal sealed class GatedStream : Stream
{
    private readonly Channel<byte[]> _chunks = Channel.CreateUnbounded<byte[]>();
    private readonly CancellationTokenSource _closed = new();
    private byte[] _current = Array.Empty<byte>();
    private int _offset;

    public bool Disposed { get; private set; }

    /// <summary>Chunks the reader has not taken. A reader that stopped leaves these behind.</summary>
    public int UnreadChunks => _chunks.Reader.Count;

    public void Write(byte[] chunk) => _chunks.Writer.TryWrite(chunk);

    public void Write(string text) => Write(System.Text.Encoding.UTF8.GetBytes(text));

    public void Complete() => _chunks.Writer.TryComplete();

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _closed.Token);
        while (_offset >= _current.Length)
        {
            if (!await _chunks.Reader.WaitToReadAsync(linked.Token).ConfigureAwait(false))
            {
                return 0;
            }

            if (_chunks.Reader.TryRead(out var next))
            {
                _current = next;
                _offset = 0;
            }
        }

        var size = Math.Min(count, _current.Length - _offset);
        Buffer.BlockCopy(_current, _offset, buffer, offset, size);
        _offset += size;
        return size;
    }

    protected override void Dispose(bool disposing)
    {
        Disposed = true;
        _closed.Cancel();
        base.Dispose(disposing);
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException("Reads must be asynchronous.");

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

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}

/// <summary>Answers every request with a <see cref="GatedStream"/> body, so the test decides when each chunk arrives.</summary>
internal sealed class GatedStreamHandler : HttpMessageHandler
{
    private readonly string _contentType;

    public GatedStreamHandler(string contentType = "text/event-stream")
    {
        _contentType = contentType;
    }

    public GatedStream Body { get; } = new();

    public string RequestBody { get; private set; } = string.Empty;

    public string RequestUri { get; private set; } = string.Empty;

    public Dictionary<string, string> RequestHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestUri = request.RequestUri?.AbsoluteUri ?? string.Empty;
        RequestBody = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        foreach (var header in request.Headers)
        {
            RequestHeaders[header.Key] = string.Join(",", header.Value);
        }

        var content = new StreamContent(Body);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse(_contentType);
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }
}
