// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResponseHandler;

public sealed class CreateEventSourceResponseHandlerTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/response-handler.test.ts::createEventSourceResponseHandler::should preserve context and mark response body socket errors as retryable",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_context_and_mark_response_body_socket_errors_as_retryable()
    {
        var socketError = new IOException("other side closed");
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new FailingContent(Encoding.UTF8.GetBytes("data: {\"value\":\"partial\"}\n\n"), socketError),
        };
        response.Headers.TryAddWithoutValidation("x-request-id", "request-id");
        var schema = JsonSchemas.Object(
            new[] { new KeyValuePair<string, JsonNode>("value", JsonSchemas.String()) },
            new[] { "value" });
        var requestBodyValues = new Dictionary<string, string> { ["prompt"] = "test" };

        var result = await ResponseHandlers.CreateEventSourceResponseAsync(response, "test-url", requestBodyValues, schema);
        var enumerator = result.Value.GetAsyncEnumerator();

        Assert.True(await enumerator.MoveNextAsync());
        Assert.True(enumerator.Current.Success);
        Assert.Equal("partial", enumerator.Current.Value!.Value.GetProperty("value").GetString());

        var error = await Assert.ThrowsAsync<ApiException>(async () => await enumerator.MoveNextAsync());
        Assert.Equal("Failed to process successful response", error.Message);
        Assert.True(error.IsRetryable);
        Assert.Equal(200, error.StatusCode);
        Assert.Equal("request-id", error.ResponseHeaders!["x-request-id"]);
        Assert.Equal("test-url", error.Url);
        Assert.Equal(requestBodyValues, error.RequestBodyValues);
        Assert.Same(socketError, error.InnerException);
        await enumerator.DisposeAsync();
    }

    private sealed class FailingContent : HttpContent
    {
        private readonly byte[] _first;
        private readonly Exception _error;

        public FailingContent(byte[] first, Exception error)
        {
            _first = first;
            _error = error;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => throw new NotSupportedException();

        protected override Task<Stream> CreateContentReadStreamAsync() => Task.FromResult<Stream>(new FailingStream(_first, _error));

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }

    private sealed class FailingStream : Stream
    {
        private readonly byte[] _first;
        private readonly Exception _error;
        private int _reads;

        public FailingStream(byte[] first, Exception error)
        {
            _first = first;
            _error = error;
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_reads++ == 0)
            {
                Array.Copy(_first, 0, buffer, offset, _first.Length);
                return _first.Length;
            }

            throw _error;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
