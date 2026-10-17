// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mcp;

/// <summary>
/// MCP over the legacy HTTP+SSE transport. <see cref="StartAsync"/> opens a <c>GET</c> event stream, waits for the
/// <c>endpoint</c> event, then <see cref="SendAsync"/> posts JSON-RPC messages to that endpoint. Responses arrive on the stream.
/// </summary>
public sealed class SseMcpTransport : IMcpTransport, IDisposable
{
    /// <summary>Protocol version sent in <c>mcp-protocol-version</c> until <see cref="ProtocolVersion"/> is set.</summary>
    public const string LatestLegacyProtocolVersion = "2024-11-05";

    private readonly HttpClient _httpClient;
    private readonly Uri _url;
    private readonly IReadOnlyDictionary<string, string>? _headers;
    private readonly SemaphoreSlim _startGate = new SemaphoreSlim(1, 1);
    private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonElement>> _pending = new ConcurrentDictionary<int, TaskCompletionSource<JsonElement>>();
    private Uri? _endpoint;
    private CancellationTokenSource? _cts;
    private Task? _loop;
    private volatile bool _connected;
    private int _id;

    /// <summary>Creates a transport for the SSE endpoint at <paramref name="url"/>.</summary>
    public SseMcpTransport(HttpClient httpClient, Uri url, IReadOnlyDictionary<string, string>? headers = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _url = url ?? throw new ArgumentNullException(nameof(url));
        _headers = headers;
    }

    /// <summary>Called when the stream closes after <see cref="CloseAsync"/>.</summary>
    public Action? OnClose { get; set; }

    /// <summary>Called with each transport error, such as a failed connection or an unparsable message.</summary>
    public Action<Exception>? OnError { get; set; }

    /// <summary>Called with each JSON-RPC message the server sends.</summary>
    public Action<JsonElement>? OnMessage { get; set; }

    /// <summary>Negotiated protocol version. Defaults to <see cref="LatestLegacyProtocolVersion"/> when unset.</summary>
    public string? ProtocolVersion { get; set; }

    /// <summary>Opens the event stream and completes once the server has sent its <c>endpoint</c> event.</summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_connected)
        {
            return;
        }

        // A stream that ended without CloseAsync must not make the next endpoint event look stale.
        _endpoint = null;
        var cts = new CancellationTokenSource();
        _cts = cts;
        var ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        HttpResponseMessage? response = null;
        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            using var request = new HttpRequestMessage(HttpMethod.Get, _url);
            AddCommonHeaders(request, "text/event-stream");
            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linked.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var text = await TryReadBodyAsync(response, linked.Token).ConfigureAwait(false);
                var reason = response.ReasonPhrase;
                var message = "MCP SSE Transport Error: " + (int)response.StatusCode + " " + reason + (string.IsNullOrEmpty(text) ? string.Empty : ": " + text);
                if ((int)response.StatusCode == 405)
                {
                    message += ". This server does not support SSE transport. Try using `http` transport instead";
                }

                throw new MCPClientError(message, statusCode: (int)response.StatusCode, url: _url.ToString(), responseBody: string.IsNullOrEmpty(text) ? null : text);
            }
        }
        catch (Exception error)
        {
            response?.Dispose();
            if (!(error is OperationCanceledException))
            {
                OnError?.Invoke(error);
            }

            cts.Cancel();
            throw;
        }

        _loop = Task.Run(() => ProcessEventsAsync(response, ready, cts), CancellationToken.None);
        try
        {
            using (cancellationToken.Register(() => ready.TrySetCanceled(cancellationToken)))
            {
                await ready.Task.ConfigureAwait(false);
            }
        }
        catch
        {
            cts.Cancel();
            throw;
        }
    }

    /// <summary>Posts one JSON-RPC message to the endpoint the server announced.</summary>
    public async Task SendAsync(JsonElement message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var endpoint = _endpoint;
        var cts = _cts;
        if (endpoint is null || !_connected || cts is null)
        {
            throw new MCPClientError("MCP SSE Transport Error: Not connected");
        }

        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(message.GetRawText(), Encoding.UTF8, "application/json"),
            };
            AddCommonHeaders(request, null);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linked.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var text = await TryReadBodyAsync(response, linked.Token).ConfigureAwait(false);
                throw new MCPClientError(
                    "MCP SSE Transport Error: POSTing to endpoint (HTTP " + (int)response.StatusCode + "): " + text,
                    statusCode: (int)response.StatusCode,
                    url: endpoint.ToString(),
                    responseBody: string.IsNullOrEmpty(text) ? null : text);
            }
        }
        catch (Exception error)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                OnError?.Invoke(error);
            }

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<JsonElement> CallAsync(string method, JsonElement parameters, CancellationToken cancellationToken)
    {
        if (!_connected)
        {
            await _startGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await StartAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _startGate.Release();
            }
        }

        var id = Interlocked.Increment(ref _id);
        var completion = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[id] = completion;
        try
        {
            var request = new JsonObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = id,
                ["method"] = method,
                ["params"] = JsonNode.Parse(parameters.GetRawText()),
            };
            using var document = JsonDocument.Parse(request.ToJsonString());
            await SendAsync(document.RootElement, cancellationToken).ConfigureAwait(false);
            using (cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken)))
            {
                return await completion.Task.ConfigureAwait(false);
            }
        }
        finally
        {
            _pending.TryRemove(id, out _);
        }
    }

    /// <summary>Closes the event stream and fails pending calls.</summary>
    public async Task CloseAsync()
    {
        _connected = false;
        _endpoint = null;
        var cts = _cts;
        cts?.Cancel();
        FailPending(new MCPClientError("MCP SSE Transport Error: Connection closed"));
        var loop = _loop;
        if (loop != null)
        {
            try
            {
                await loop.ConfigureAwait(false);
            }
            catch (Exception)
            {
                // The loop reports its own errors through OnError.
            }
        }

        OnClose?.Invoke();
    }

    /// <summary>Closes the transport.</summary>
    public void Dispose()
    {
        _connected = false;
        _endpoint = null;
        _cts?.Cancel();
        FailPending(new MCPClientError("MCP SSE Transport Error: Connection closed"));
        _startGate.Dispose();
    }

    /// <summary>Parses one JSON-RPC message. Prototype-pollution keys are rejected.</summary>
    public static JsonElement DeserializeMessage(string line)
    {
        return JsonRpcMessages.Parse(line);
    }

    private void AddCommonHeaders(HttpRequestMessage request, string? accept)
    {
        if (_headers != null)
        {
            foreach (var header in _headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (accept != null)
        {
            request.Headers.Accept.ParseAdd(accept);
        }

        request.Headers.TryAddWithoutValidation("mcp-protocol-version", ProtocolVersion ?? LatestLegacyProtocolVersion);
        request.Headers.UserAgent.ParseAdd("ai-sdk/" + AiSdkVersion.Version);
    }

    private static async Task<string?> TryReadBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await CancellableReads.ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception error) when (!(error is OperationCanceledException))
        {
            return null;
        }
    }

    private async Task ProcessEventsAsync(HttpResponseMessage response, TaskCompletionSource<bool> ready, CancellationTokenSource cts)
    {
        var token = cts.Token;
        try
        {
            using (response)
            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
            {
                string? eventName = null;
                var data = new StringBuilder();
                var hasData = false;
                while (true)
                {
                    var line = await CancellableReads.ReadLineAsync(reader, token).ConfigureAwait(false);
                    if (line is null)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        if (_connected)
                        {
                            _connected = false;
                            throw new MCPClientError("MCP SSE Transport Error: Connection closed unexpectedly");
                        }

                        throw new MCPClientError("MCP SSE Transport Error: Connection closed before endpoint event was received");
                    }

                    if (line.Length == 0)
                    {
                        if (hasData)
                        {
                            Dispatch(eventName, data.ToString(), ready, cts);
                        }

                        eventName = null;
                        data.Clear();
                        hasData = false;
                    }
                    else if (line.StartsWith("event:", StringComparison.Ordinal))
                    {
                        eventName = FieldValue(line, 6);
                    }
                    else if (line.StartsWith("data:", StringComparison.Ordinal))
                    {
                        if (hasData)
                        {
                            data.Append('\n');
                        }

                        data.Append(FieldValue(line, 5));
                        hasData = true;
                    }
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception error)
        {
            _connected = false;
            _endpoint = null;
            OnError?.Invoke(error);
            ready.TrySetException(error);
            FailPending(error);
        }
    }

    private static string FieldValue(string line, int prefixLength)
    {
        return line.Length > prefixLength && line[prefixLength] == ' ' ? line.Substring(prefixLength + 1) : line.Substring(prefixLength);
    }

    private void Dispatch(string? eventName, string data, TaskCompletionSource<bool> ready, CancellationTokenSource cts)
    {
        if (eventName == "endpoint")
        {
            if (_endpoint != null)
            {
                return;
            }

            var endpoint = new Uri(_url, data);
            if (!SameOrigin(endpoint, _url))
            {
                _connected = false;
                _endpoint = null;
                cts.Cancel();
                throw new MCPClientError("MCP SSE Transport Error: Endpoint origin does not match connection origin: " + Origin(endpoint));
            }

            _endpoint = endpoint;
            _connected = true;
            ready.TrySetResult(true);
        }
        else if (eventName is null || eventName == "message")
        {
            JsonElement message;
            try
            {
                message = JsonRpcMessages.Parse(data);
            }
            catch (Exception error) when (!(error is OperationCanceledException))
            {
                OnError?.Invoke(new MCPClientError("MCP SSE Transport Error: Failed to parse message", cause: error));
                return;
            }

            OnMessage?.Invoke(message);
            if (!message.TryGetProperty("method", out _)
                && message.TryGetProperty("id", out var id)
                && id.ValueKind == JsonValueKind.Number
                && id.TryGetInt32(out var number)
                && _pending.TryGetValue(number, out var completion))
            {
                try
                {
                    completion.TrySetResult(StdioMcpTransport.Unwrap(message));
                }
                catch (MCPClientError error)
                {
                    completion.TrySetException(error);
                }
            }
        }
    }

    private void FailPending(Exception error)
    {
        foreach (var entry in _pending)
        {
            entry.Value.TrySetException(error);
        }
    }

    private static bool SameOrigin(Uri left, Uri right)
    {
        return string.Equals(left.Scheme, right.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(left.IdnHost, right.IdnHost, StringComparison.OrdinalIgnoreCase)
            && left.Port == right.Port;
    }

    private static string Origin(Uri uri)
    {
        return uri.GetLeftPart(UriPartial.Authority);
    }
}
