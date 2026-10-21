// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

/// <summary>HTTP stub and JSON helpers for Google parity tests.</summary>
internal static class GoogleUpstream
{
    public static LanguageModelCallOptions Hello()
    {
        return Prompt(new UserModelMessage("Hello"));
    }

    public static LanguageModelCallOptions Prompt(params ModelMessage[] messages)
    {
        return new LanguageModelCallOptions { Prompt = messages };
    }

    public static Dictionary<string, JsonElement> ProviderOptions(string json)
    {
        using var document = JsonDocument.Parse(json);
        var map = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            map[property.Name] = property.Value.Clone();
        }

        return map;
    }

    public static LanguageModelTool Function(string name, string schema, string? description = "", bool? strict = null)
    {
        using var document = JsonDocument.Parse(schema);
        return new LanguageModelTool(name, description, document.RootElement.Clone(), strict);
    }

    public static GoogleProviderTool Tool(string id, string? args = null, string? name = null)
    {
        JsonElement? element = null;
        if (args != null)
        {
            using var document = JsonDocument.Parse(args);
            element = document.RootElement.Clone();
        }

        return new GoogleProviderTool(id, element, name);
    }

    public static JsonElement Element(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static void JsonEqual(JsonNode? actual, string expected)
    {
        JsonAssert.Equal(actual, expected);
    }

    public static void JsonEqual(JsonElement actual, string expected)
    {
        JsonAssert.Equal(actual, expected);
    }

    public static GooglePreparedRequest Prepare(string modelId, string provider, LanguageModelCallOptions options, bool streaming = false)
    {
        return GoogleRequest.Prepare(modelId, provider, options, streaming);
    }

    public static LanguageModelGenerateResult Parse(string json, string? provider = null, Func<string>? generateId = null)
    {
        using var document = JsonDocument.Parse(json);
        return GoogleResponse.Parse(document.RootElement, new GoogleParseContext(provider, generateId));
    }

    public static GoogleProvider Client(RecordingHandler handler, GoogleOptions? options = null)
    {
        options ??= new GoogleOptions { ApiKey = "test-api-key" };
        if (string.IsNullOrEmpty(options.ApiKey) && !options.UseBearerToken)
        {
            options.ApiKey = "test-api-key";
        }

        return GoogleProvider.Create(options, handler);
    }

    public static async Task<List<LanguageModelStreamPart>> Collect(IAsyncEnumerable<LanguageModelStreamPart> parts)
    {
        var list = new List<LanguageModelStreamPart>();
        await foreach (var part in parts.ConfigureAwait(false))
        {
            list.Add(part);
        }

        return list;
    }
}

/// <summary>Captures one request and returns a scripted body.</summary>
internal sealed class RecordingHandler : HttpMessageHandler
{
    public List<string> Uris { get; } = new();

    public string Method { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public Dictionary<string, string> RequestHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string ResponseText { get; set; } = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}]}";

    public string MediaType { get; set; } = "application/json";

    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    public Dictionary<string, string> ResponseHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    public int Calls
    {
        get { return Uris.Count; }
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Uris.Add(request.RequestUri?.AbsoluteUri ?? string.Empty);
        Method = request.Method.Method;
        Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        RequestHeaders.Clear();
        foreach (var header in request.Headers)
        {
            RequestHeaders[header.Key] = string.Join(",", header.Value);
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                RequestHeaders[header.Key] = string.Join(",", header.Value);
            }
        }

        if (request.Headers.Authorization != null)
        {
            RequestHeaders["Authorization"] = request.Headers.Authorization.ToString();
        }

        var message = new HttpResponseMessage(Status)
        {
            Content = new StringContent(ResponseText, Encoding.UTF8, MediaType),
        };
        foreach (var header in ResponseHeaders)
        {
            message.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return message;
    }
}

/// <summary>Returns scripted responses in order and records every request. The last response repeats.</summary>
internal sealed class SequencedHandler : HttpMessageHandler
{
    private readonly List<(string Body, string MediaType, HttpStatusCode Status, bool Hang, bool Fail)> _responses = new();

    public List<string> Uris { get; } = new();

    public List<string> Methods { get; } = new();

    public List<string> Bodies { get; } = new();

    public SequencedHandler Json(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        _responses.Add((body, "application/json", status, false, false));
        return this;
    }

    public SequencedHandler Sse(string body)
    {
        _responses.Add((body, "text/event-stream", HttpStatusCode.OK, false, false));
        return this;
    }

    /// <summary>An event stream that sends <paramref name="body"/> and then fails with an <see cref="IOException"/>.</summary>
    public SequencedHandler FailingSse(string body)
    {
        _responses.Add((body, "text/event-stream", HttpStatusCode.OK, false, true));
        return this;
    }

    /// <summary>An event stream that sends <paramref name="body"/> and then stays open until the caller cancels.</summary>
    public SequencedHandler HangingSse(string body)
    {
        _responses.Add((body, "text/event-stream", HttpStatusCode.OK, true, false));
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var index = Uris.Count;
        Uris.Add(request.RequestUri?.AbsoluteUri ?? string.Empty);
        Methods.Add(request.Method.Method);
        Bodies.Add(request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false));
        var response = _responses[Math.Min(index, _responses.Count - 1)];
        if (response.Hang || response.Fail)
        {
            var content = new StreamContent(new HangingStream(Encoding.UTF8.GetBytes(response.Body), response.Fail));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(response.MediaType);
            return new HttpResponseMessage(response.Status) { Content = content };
        }

        return new HttpResponseMessage(response.Status)
        {
            Content = new StringContent(response.Body, Encoding.UTF8, response.MediaType),
        };
    }

    /// <summary>Yields its bytes, then waits for the read to be cancelled.</summary>
    private sealed class HangingStream : Stream
    {
        private readonly byte[] _bytes;
        private int _position;

        private readonly bool _fail;

        public HangingStream(byte[] bytes, bool fail = false)
        {
            _bytes = bytes;
            _fail = fail;
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
            if (_position < _bytes.Length)
            {
                var read = Math.Min(count, _bytes.Length - _position);
                Array.Copy(_bytes, _position, buffer, offset, read);
                _position += read;
                return read;
            }

            if (_fail)
            {
                throw new IOException("The connection was reset.");
            }

            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            return 0;
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
