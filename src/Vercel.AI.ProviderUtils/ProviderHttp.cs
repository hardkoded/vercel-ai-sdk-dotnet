// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

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
        var response = await SendAsync(method, uri, jsonBody, "application/json", headers, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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

    /// <summary>Posts JSON and returns the raw bytes.</summary>
    public async Task<byte[]> SendBytesAsync(
        HttpMethod method,
        Uri uri,
        HttpContent content,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        var response = await SendRawAsync(method, uri, content, headers, cancellationToken).ConfigureAwait(false);
        var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw MapStatus((int)response.StatusCode, Encoding.UTF8.GetString(bytes));
        }

        return bytes;
    }

    /// <summary>Posts JSON and yields SSE data payloads.</summary>
    public async IAsyncEnumerable<string> SendSseAsync(
        Uri uri,
        string jsonBody,
        IReadOnlyDictionary<string, string?>? headers,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var response = await SendAsync(HttpMethod.Post, uri, jsonBody, "application/json", headers, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw MapStatus((int)response.StatusCode, body);
        }

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await foreach (var data in SseParser.ReadDataAsync(stream, cancellationToken).ConfigureAwait(false))
        {
            yield return data;
        }
    }

    /// <summary>Maps an HTTP status onto the SDK exception hierarchy.</summary>
    public static ApiException MapStatus(int statusCode, string? body)
    {
        var message = JsonValues.ExtractErrorMessage(body, statusCode);
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
                await Task.Delay(_retry.GetDelay(attempt, retryAfter), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw new ApiUserAbortException();
            }
            catch (OperationCanceledException exception)
            {
                last = new ApiTimeoutException("The provider call timed out.", exception);
                if (attempt >= _retry.MaxRetries)
                {
                    throw (Exception)last;
                }

                await Task.Delay(_retry.GetDelay(attempt, null), cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException exception)
            {
                last = new ApiConnectionException("The connection to the provider failed.", exception);
                if (attempt >= _retry.MaxRetries)
                {
                    throw (Exception)last;
                }

                await Task.Delay(_retry.GetDelay(attempt, null), cancellationToken).ConfigureAwait(false);
            }
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

    private static TimeSpan? ReadRetryAfter(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("retry-after-ms", out var millisecondsValues))
        {
            var raw = millisecondsValues.FirstOrDefault();
            if (int.TryParse(raw, out var milliseconds))
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
