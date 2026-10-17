// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Sends each request through an <see cref="HttpClient"/> without disposing it.</summary>
internal sealed class HttpClientMessageHandler : HttpMessageHandler
{
    private readonly HttpClient _client;

    public HttpClientMessageHandler(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // A request can be sent once, so the inner client gets a copy that shares the content.
        var copy = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Content = request.Content,
            Version = request.Version,
        };
        foreach (var header in request.Headers)
        {
            copy.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return _client.SendAsync(copy, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }
}

/// <summary>
/// Authenticates a Bedrock request: a bearer token when <c>apiKey</c> is set, otherwise SigV4.
/// Maps to <c>createApiKeyFetchFunction</c> and <c>createSigV4FetchFunction</c>.
/// </summary>
internal sealed class AmazonBedrockSigningHandler : DelegatingHandler
{
    private readonly string _region;
    private readonly string _service;
    private readonly string? _apiKey;
    private readonly string? _accessKeyId;
    private readonly string? _secretAccessKey;
    private readonly string? _sessionToken;
    private readonly Func<DateTimeOffset>? _utcNow;
    private readonly Func<CancellationToken, Task<AmazonBedrockCredentials>>? _credentialProvider;

    public AmazonBedrockSigningHandler(
        HttpMessageHandler inner,
        string region,
        string service,
        string? apiKey,
        string? accessKeyId,
        string? secretAccessKey,
        string? sessionToken,
        Func<DateTimeOffset>? utcNow,
        Func<CancellationToken, Task<AmazonBedrockCredentials>>? credentialProvider = null)
        : base(inner)
    {
        _credentialProvider = credentialProvider;
        _region = region;
        _service = service;
        _apiKey = apiKey;
        _accessKeyId = accessKeyId;
        _secretAccessKey = secretAccessKey;
        _sessionToken = sessionToken;
        _utcNow = utcNow;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var payload = request.Content == null
            ? Array.Empty<byte>()
            : await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        request.Headers.Remove("Authorization");
        var accessKeyId = _accessKeyId;
        var secretAccessKey = _secretAccessKey;
        var sessionToken = _sessionToken;
        if (_credentialProvider != null && string.IsNullOrWhiteSpace(_apiKey))
        {
            // The region always comes from the options. The provider supplies only the keys.
            AmazonBedrockCredentials credentials;
            try
            {
                credentials = await _credentialProvider(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                throw new AiSdkException("AWS credential provider failed: " + exception.Message + ". Please ensure your credential provider returns valid AWS credentials with accessKeyId and secretAccessKey properties.", exception);
            }

            accessKeyId = credentials.AccessKeyId;
            secretAccessKey = credentials.SecretAccessKey;
            sessionToken = credentials.SessionToken;
        }

        AmazonBedrockSigner.Apply(
            request,
            payload,
            _region,
            _service,
            _apiKey,
            accessKeyId,
            secretAccessKey,
            sessionToken,
            _utcNow?.Invoke() ?? DateTimeOffset.UtcNow);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Wraps a handler so the Anthropic Messages model can read Bedrock responses. Failed responses become
/// Anthropic error bodies, and <c>application/vnd.amazon.eventstream</c> bodies become server-sent events.
/// Maps to <c>createAmazonBedrockAnthropicFetch</c>.
/// </summary>
public static class AmazonBedrockAnthropicFetch
{
    /// <summary>Wraps <paramref name="baseHandler"/>. Disposing the result disposes <paramref name="baseHandler"/>.</summary>
    public static DelegatingHandler Create(HttpMessageHandler baseHandler)
    {
        return new AmazonBedrockAnthropicResponseHandler(baseHandler ?? throw new ArgumentNullException(nameof(baseHandler)));
    }
}

internal sealed class AmazonBedrockAnthropicResponseHandler : DelegatingHandler
{
    private static readonly JsonSerializerOptions Compact = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public AmazonBedrockAnthropicResponseHandler(HttpMessageHandler inner)
        : base(inner)
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        try
        {
            // Bedrock errors become the Anthropic { type: error, error } shape so the model can read the message.
            if (!response.IsSuccessStatusCode)
            {
                var text = response.Content == null
                    ? string.Empty
                    : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var failed = CopyStatus(response);
                failed.Content = new StringContent(AmazonBedrockAnthropicRequests.ToAnthropicErrorBody(text), Encoding.UTF8, "application/json");
                response.Dispose();
                return failed;
            }

            var contentType = response.Content?.Headers.ContentType?.ToString();
            if (contentType != null
                && contentType.IndexOf("application/vnd.amazon.eventstream", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var source = await response.Content!.ReadAsStreamAsync().ConfigureAwait(false);
                var transformed = CopyStatus(response);
                transformed.Content = new StreamContent(new EventStreamSseStream(source, response, ProcessEvent));
                transformed.Content.Headers.ContentType = new MediaTypeHeaderValue("text/event-stream");
                return transformed;
            }

            return response;
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    private static HttpResponseMessage CopyStatus(HttpResponseMessage response)
    {
        var copy = new HttpResponseMessage(response.StatusCode)
        {
            ReasonPhrase = response.ReasonPhrase,
            Version = response.Version,
            RequestMessage = response.RequestMessage,
        };
        foreach (var header in response.Headers)
        {
            copy.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return copy;
    }

    private static void ProcessEvent(DecodedEvent decoded, Action<byte[]> enqueue)
    {
        if (decoded.MessageType == "event")
        {
            if (decoded.EventType == "chunk")
            {
                enqueue(Data(ChunkData(decoded.Data)));
            }
            else if (decoded.EventType == "messageStop")
            {
                enqueue(Data("[DONE]"));
            }
        }
        else if (decoded.MessageType == "exception")
        {
            enqueue(Data(ExceptionData(decoded)));
        }
    }

    private static string ChunkData(string data)
    {
        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(data);
        }
        catch (JsonException)
        {
            return data;
        }

        if (parsed is JsonObject chunk
            && chunk["bytes"] is JsonValue bytes
            && bytes.TryGetValue<string>(out var encoded)
            && encoded.Length > 0)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }

        return data;
    }

    private static string ExceptionData(DecodedEvent decoded)
    {
        JsonNode? data;
        try
        {
            data = JsonNode.Parse(decoded.Data);
        }
        catch (JsonException)
        {
            data = JsonValue.Create(decoded.Data);
        }

        var message = decoded.Data;
        if (data is JsonObject fields && fields["message"] is JsonValue value && value.TryGetValue<string>(out var text))
        {
            message = text;
        }

        var type = decoded.EventType ?? decoded.ExceptionType ?? "error";
        var error = new JsonObject
        {
            ["type"] = type,
            ["message"] = message,
        };
        var (statusCode, isRetryable) = AmazonBedrockStreamError.Metadata(type);
        if (statusCode != null)
        {
            error["statusCode"] = statusCode;
        }

        if (isRetryable != null)
        {
            error["isRetryable"] = isRetryable;
        }

        error["data"] = data;
        return new JsonObject { ["type"] = "error", ["error"] = error }.ToJsonString(Compact);
    }

    private static byte[] Data(string payload)
    {
        return Encoding.UTF8.GetBytes("data: " + payload + "\n\n");
    }
}

/// <summary>Reads an AWS event stream and exposes it as server-sent events.</summary>
internal sealed class EventStreamSseStream : Stream
{
    private readonly Stream _source;
    private readonly HttpResponseMessage _response;
    private readonly Action<DecodedEvent, Action<byte[]>> _process;
    private IAsyncEnumerator<byte[]>? _events;
    private byte[] _pending = Array.Empty<byte>();
    private int _offset;

    public EventStreamSseStream(Stream source, HttpResponseMessage response, Action<DecodedEvent, Action<byte[]>> process)
    {
        _source = source;
        _response = response;
        _process = process;
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

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_offset >= _pending.Length)
        {
            _events ??= AmazonBedrockEventStreamDecoder.Create(_source, _process, cancellationToken).GetAsyncEnumerator(cancellationToken);
            if (!await _events.MoveNextAsync().ConfigureAwait(false))
            {
                return 0;
            }

            _pending = _events.Current;
            _offset = 0;
        }

        var length = Math.Min(count, _pending.Length - _offset);
        Buffer.BlockCopy(_pending, _offset, buffer, offset, length);
        _offset += length;
        return length;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("Read the Bedrock event stream asynchronously.");
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                _source.Dispose();
            }
            finally
            {
                _response.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
