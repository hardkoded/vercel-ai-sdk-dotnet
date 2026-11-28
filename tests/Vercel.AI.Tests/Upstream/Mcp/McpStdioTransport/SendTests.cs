// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.Mcp;

namespace Vercel.AI.Tests.Upstream.Mcp.McpStdioTransport;

public sealed class SendTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/mcp-stdio-transport.test.ts::StdioMCPTransport > send::should successfully send a message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Ndjson_writes_one_json_line()
    {
        using var document = JsonDocument.Parse(
            "{\"jsonrpc\":\"2.0\",\"id\":\"1\",\"method\":\"test\",\"params\":{}}"
        );
        using var stream = new MemoryStream();
        McpNdjson.Write(stream, document.RootElement);
        Assert.Equal(
            document.RootElement.GetRawText() + "\n",
            Encoding.UTF8.GetString(stream.ToArray())
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/mcp-stdio-transport.test.ts::StdioMCPTransport > send::should throw error if transport is not connected",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Ndjson_write_requires_a_stream()
    {
        using var document = JsonDocument.Parse(
            "{\"jsonrpc\":\"2.0\",\"id\":\"1\",\"method\":\"test\",\"params\":{}}"
        );
        var error = Assert.Throws<MCPClientError>(() =>
            McpNdjson.Write(null, document.RootElement)
        );
        Assert.Contains("StdioClientTransport not connected", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/mcp-stdio-transport.test.ts::StdioMCPTransport > send::should handle write backpressure",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldHandleWriteBackpressure()
    {
        var input = new GatedStream();
        using var output = new MemoryStream();
        using var transport = new StdioMcpTransport(input, output);
        using var document = JsonDocument.Parse("{}");

        var send = transport.NotifyAsync("test", document.RootElement, CancellationToken.None);
        Assert.False(send.IsCompleted);

        input.Release();
        await send;
        Assert.Contains("\"method\":\"test\"", Encoding.UTF8.GetString(input.Written.ToArray()));
    }

    private sealed class GatedStream : Stream
    {
        private readonly TaskCompletionSource<bool> _gate = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        public MemoryStream Written { get; } = new();

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public void Release()
        {
            _gate.TrySetResult(true);
        }

        public override async Task WriteAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken
        )
        {
            await _gate.Task.ConfigureAwait(false);
            Written.Write(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count) =>
            Written.Write(buffer, offset, count);

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
