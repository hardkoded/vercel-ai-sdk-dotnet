// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.ProviderUtils;

/// <summary>Sends JSON and SSE requests with the SDK retry policy.</summary>
public sealed class ProviderHttp
{
    private readonly HttpClient _httpClient;
    private readonly RetryPolicy _retry;
    private readonly TimeSpan? _timeout;

    /// <summary>Creates a provider HTTP client.</summary>
    public ProviderHttp(HttpClient httpClient, RetryPolicy? retry = null, TimeSpan? timeout = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _retry = retry ?? RetryPolicy.Default;
        _timeout = timeout;
    }

    /// <summary>Posts JSON and returns the parsed body.</summary>
    public async Task<JsonDocument> SendJsonAsync(
        HttpMethod method,
        Uri uri,
        string? jsonBody,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(method, uri, jsonBody, "application/json", headers, cancellationToken).ConfigureAwait(false);
        var body = await CancellableReads.ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw MapStatus((int)response.StatusCode, body);
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return JsonDocument.Parse("{}");
        }

        return JsonDocument.Parse(body);
    }

    /// <summary>Posts JSON and returns the response body text and headers.</summary>
    public async Task<ProviderTextResponse> SendJsonStringAsync(
        HttpMethod method,
        Uri uri,
        string? jsonBody,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        var response = await SendAsync(method, uri, jsonBody, "application/json", headers, cancellationToken).ConfigureAwait(false);
        try
        {
            var body = await CancellableReads.ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw MapStatus((int)response.StatusCode, body);
            }

            return new ProviderTextResponse(body ?? string.Empty, CopyHeaders(response));
        }
        finally
        {
            response.Dispose();
        }
    }

    /// <summary>Sends a request and returns the raw bytes.</summary>
    public async Task<byte[]> SendBytesAsync(
        HttpMethod method,
        Uri uri,
        HttpContent content,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        return (await SendBinaryAsync(method, uri, content, headers, cancellationToken).ConfigureAwait(false)).Body;
    }

    /// <summary>Sends a request, with retries, and returns the raw bytes, headers, and media type. A null <paramref name="content"/> sends no body.</summary>
    internal async Task<ProviderBinaryResponse> SendBinaryAsync(
        HttpMethod method,
        Uri uri,
        HttpContent? content,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        // A retry needs a fresh body, so keep the bytes and rebuild the content for each attempt.
        Func<HttpContent>? rebuild = null;
        if (content != null && _retry.MaxRetries > 0)
        {
            var body = await CancellableReads.ReadAsByteArrayAsync(content, cancellationToken).ConfigureAwait(false);
            var contentHeaders = content.Headers.ToList();
            rebuild = () =>
            {
                var copy = new ByteArrayContent(body);
                foreach (var header in contentHeaders)
                {
                    copy.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                return copy;
            };
        }

        using var response = await SendRawAsync(method, uri, content, headers, cancellationToken, rebuild).ConfigureAwait(false);
        var bytes = await CancellableReads.ReadAsByteArrayAsync(response.Content, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw MapStatus((int)response.StatusCode, Encoding.UTF8.GetString(bytes));
        }

        return new ProviderBinaryResponse(bytes, CopyHeaders(response), response.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>Posts JSON and yields SSE data payloads.</summary>
    public async IAsyncEnumerable<string> SendSseAsync(
        Uri uri,
        string jsonBody,
        IReadOnlyDictionary<string, string?>? headers,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var events = await OpenSseAsync(uri, jsonBody, headers, cancellationToken).ConfigureAwait(false);
        await foreach (var data in events.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return data;
        }
    }

    /// <summary>
    /// Posts JSON and returns once the response status is known. A failed response throws here.
    /// The returned sequence yields SSE data payloads as they arrive. Enumerate it once: it disposes the response when it ends or the consumer stops.
    /// </summary>
    public async Task<IAsyncEnumerable<string>> OpenSseAsync(
        Uri uri,
        string jsonBody,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        return (await OpenSseWithResourceAsync(uri, jsonBody, headers, cancellationToken).ConfigureAwait(false)).Events;
    }

    /// <summary>
    /// Like <see cref="OpenSseAsync"/>, and also returns the response so a caller that never enumerates the events can release it.
    /// </summary>
    public async Task<(IAsyncEnumerable<string> Events, IDisposable Resource)> OpenSseWithResourceAsync(
        Uri uri,
        string jsonBody,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        var opened = await OpenSseResponseAsync(uri, jsonBody, headers, cancellationToken).ConfigureAwait(false);
        return (opened.Events, opened.Resource);
    }

    /// <summary>
    /// Like <see cref="OpenSseWithResourceAsync"/>, and also returns the response headers.
    /// A failed response throws here.
    /// </summary>
    internal async Task<ProviderSseResponse> OpenSseResponseAsync(
        Uri uri,
        string jsonBody,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        var response = await SendAsync(HttpMethod.Post, uri, jsonBody, "application/json", headers, cancellationToken).ConfigureAwait(false);
        try
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await CancellableReads.ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
                throw MapStatus((int)response.StatusCode, body);
            }

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return new ProviderSseResponse(ReadSseAsync(response, stream, cancellationToken), response, CopyHeaders(response));
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    private static async IAsyncEnumerable<string> ReadSseAsync(
        HttpResponseMessage response,
        Stream stream,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using (response)
        using (stream)
        {
            await foreach (var data in SseParser.ReadDataAsync(stream, cancellationToken).ConfigureAwait(false))
            {
                yield return data;
            }
        }
    }

    /// <summary>
    /// Sends a request and yields one JSON value per line of the response body.
    /// A row above <paramref name="maxLineBytes"/> UTF-8 bytes throws a <see cref="Util.DownloadError"/> and cancels the download.
    /// </summary>
    public async IAsyncEnumerable<JsonElement> SendJsonLinesAsync(
        HttpMethod method,
        Uri uri,
        string? jsonBody,
        IReadOnlyDictionary<string, string?>? headers,
        int? maxLineBytes,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var lines = await OpenJsonLinesAsync(method, uri, jsonBody, headers, maxLineBytes, cancellationToken).ConfigureAwait(false);
        await foreach (var value in lines.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return value;
        }
    }

    /// <summary>
    /// Sends a request and returns once the response status is known. Failed responses throw here.
    /// The returned sequence reads one JSON value per line of the body. Enumerate it once: it disposes the response when it ends.
    /// A row above <paramref name="maxLineBytes"/> UTF-8 bytes throws a <see cref="Util.DownloadError"/> and cancels the download.
    /// </summary>
    public async Task<IAsyncEnumerable<JsonElement>> OpenJsonLinesAsync(
        HttpMethod method,
        Uri uri,
        string? jsonBody,
        IReadOnlyDictionary<string, string?>? headers,
        int? maxLineBytes,
        CancellationToken cancellationToken)
    {
        var limit = maxLineBytes ?? JsonStreams.DefaultMaxLineBytes;
        if (limit <= 0)
        {
            throw new Util.InvalidArgumentError("maxLineBytes", limit, "maxLineBytes must be a positive safe integer.");
        }

        var response = await SendAsync(method, uri, jsonBody, "application/json", headers, cancellationToken).ConfigureAwait(false);
        try
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await CancellableReads.ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
                throw MapStatus((int)response.StatusCode, body);
            }

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return ReadJsonLinesAsync(response, stream, limit, uri, cancellationToken);
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    private static async IAsyncEnumerable<JsonElement> ReadJsonLinesAsync(
        HttpResponseMessage response,
        Stream stream,
        int maxLineBytes,
        Uri uri,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using (response)
        using (stream)
        {
            await foreach (var value in JsonStreams.ReadJsonLinesAsync(stream, null, null, cancellationToken, maxLineBytes, uri.ToString()).ConfigureAwait(false))
            {
                yield return value;
            }
        }
    }

    /// <summary>Maps an HTTP status onto the SDK exception hierarchy.</summary>
    public static ApiException MapStatus(int statusCode, string? body)
    {
        return MapStatus(statusCode, JsonValues.ExtractErrorMessage(body, statusCode), body);
    }

    /// <summary>Maps an HTTP status onto the SDK exception hierarchy with a provider-specific <paramref name="message"/>.</summary>
    public static ApiException MapStatus(int statusCode, string message, string? body)
    {
        return statusCode switch
        {
            400 => new BadRequestException(message, body),
            401 => new AuthenticationException(message, body),
            403 => new PermissionDeniedException(message, body),
            404 => new NotFoundException(message, body),
            422 => new UnprocessableEntityException(message, body),
            429 => new RateLimitException(message, body),
            >= 500 and <= 599 => new InternalServerException(message, statusCode, body),
            _ => new ApiException(message, statusCode, body),
        };
    }

    /// <summary>True for 408, 429, and 500–599.</summary>
    public static bool IsRetryable(int statusCode)
    {
        return statusCode == 408 || statusCode == 429 || (statusCode >= 500 && statusCode <= 599);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        Uri uri,
        string? jsonBody,
        string mediaType,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        HttpContent Content()
        {
            return new StringContent(jsonBody ?? string.Empty, Encoding.UTF8, mediaType);
        }

        return await SendRawAsync(method, uri, jsonBody is null && method == HttpMethod.Get ? null : Content(), headers, cancellationToken, Content).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendRawAsync(
        HttpMethod method,
        Uri uri,
        HttpContent? content,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken,
        Func<HttpContent>? rebuild = null)
    {
        Exception? last = null;
        for (var attempt = 0; ; attempt++)
        {
            using var timeout = CreateTimeout(cancellationToken);
            var token = timeout?.Token ?? cancellationToken;
            try
            {
                using var request = new HttpRequestMessage(method, uri);
                ApplyHeaders(request, headers);
                request.Content = attempt == 0 ? content : rebuild?.Invoke();
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
                if (response.IsSuccessStatusCode || !IsRetryable((int)response.StatusCode) || attempt >= _retry.MaxRetries)
                {
                    return response;
                }

                var retryAfter = ReadRetryAfter(response);
                response.Dispose();
                await DelayAsync(_retry.GetDelay(attempt, retryAfter), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
            {
                throw new ApiUserAbortException(exception, cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                last = new ApiTimeoutException("The provider call timed out.", exception);
                if (attempt >= _retry.MaxRetries)
                {
                    throw (Exception)last;
                }

                await DelayAsync(_retry.GetDelay(attempt, null), cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException exception)
            {
                last = new ApiConnectionException("The connection to the provider failed.", exception);
                if (attempt >= _retry.MaxRetries)
                {
                    throw (Exception)last;
                }

                await DelayAsync(_retry.GetDelay(attempt, null), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
        {
            throw new ApiUserAbortException(exception, cancellationToken);
        }
    }

    private CancellationTokenSource? CreateTimeout(CancellationToken cancellationToken)
    {
        if (_timeout is not { } timeout || timeout <= TimeSpan.Zero)
        {
            return null;
        }

        var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        source.CancelAfter(timeout);
        return source;
    }

    private static void ApplyHeaders(HttpRequestMessage request, IReadOnlyDictionary<string, string?>? headers)
    {
        if (headers is null)
        {
            return;
        }

        foreach (var pair in headers)
        {
            if (string.IsNullOrEmpty(pair.Value))
            {
                continue;
            }

            if (pair.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (pair.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                var value = pair.Value!;
                var space = value.IndexOf(' ');
                if (space > 0)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue(
                        value.Substring(0, space),
                        value.Substring(space + 1));
                }
                else
                {
                    request.Headers.TryAddWithoutValidation("Authorization", value);
                }

                continue;
            }

            request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
        }
    }

    private static IReadOnlyDictionary<string, string> CopyHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        return headers;
    }

    private static TimeSpan? ReadRetryAfter(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("retry-after-ms", out var millisecondsValues))
        {
            var raw = millisecondsValues.FirstOrDefault();
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var milliseconds))
            {
                return TimeSpan.FromMilliseconds(milliseconds);
            }
        }

        if (response.Headers.RetryAfter?.Delta is { } delta)
        {
            return delta;
        }

        return null;
    }
}

/// <summary>A successful JSON response body and its headers.</summary>
public sealed class ProviderTextResponse
{
    /// <summary>Creates a text response.</summary>
    public ProviderTextResponse(string body, IReadOnlyDictionary<string, string> headers)
    {
        Body = body ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
    }

    /// <summary>Response body.</summary>
    public string Body { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>An open SSE response: the data payloads, the response headers, and the response to release.</summary>
internal sealed class ProviderSseResponse
{
    public ProviderSseResponse(IAsyncEnumerable<string> events, IDisposable resource, IReadOnlyDictionary<string, string> headers)
    {
        Events = events;
        Resource = resource;
        Headers = headers;
    }

    public IAsyncEnumerable<string> Events { get; }

    public IDisposable Resource { get; }

    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Wraps the parts read from this response into a stream result with the request body and the response headers.
    /// The result releases the response when it is disposed or when the stream ends. The response is released here if building the result throws.
    /// </summary>
    public LanguageModelStreamResult ToStreamResult(IAsyncEnumerable<LanguageModelStreamPart> stream, string requestJson)
    {
        try
        {
            return new LanguageModelStreamResult(
                new ReleasingAsyncEnumerable<LanguageModelStreamPart>(stream, Resource),
                LanguageModelStreamRequest.FromJson(requestJson),
                new LanguageModelStreamResponse(Headers),
                Resource);
        }
        catch
        {
            Resource.Dispose();
            throw;
        }
    }
}

/// <summary>Response body bytes, headers, and media type.</summary>
internal sealed class ProviderBinaryResponse
{
    public ProviderBinaryResponse(byte[] body, IReadOnlyDictionary<string, string> headers, string? mediaType)
    {
        Body = body;
        Headers = headers;
        MediaType = mediaType;
    }

    public byte[] Body { get; }

    public IReadOnlyDictionary<string, string> Headers { get; }

    public string? MediaType { get; }
}
