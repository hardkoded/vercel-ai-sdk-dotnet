// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.ProviderUtils;

/// <summary>A handled response value with the response headers.</summary>
public sealed class ResponseHandlerResult<T>
{
    /// <summary>Creates a result.</summary>
    public ResponseHandlerResult(T value, IReadOnlyDictionary<string, string> responseHeaders)
    {
        Value = value;
        ResponseHeaders = responseHeaders;
    }

    /// <summary>Handled value.</summary>
    public T Value { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; }
}

/// <summary>A handled JSON value, the parsed body before validation, and the response headers.</summary>
public sealed class JsonResponseHandlerResult<T>
{
    /// <summary>Creates a result.</summary>
    public JsonResponseHandlerResult(
        T value,
        JsonElement rawValue,
        IReadOnlyDictionary<string, string> responseHeaders
    )
    {
        Value = value;
        RawValue = rawValue;
        ResponseHeaders = responseHeaders;
    }

    /// <summary>The validated value. It holds the decoded <see cref="byte"/> arrays at the binary fields.</summary>
    public T Value { get; }

    /// <summary>The parsed body. It holds a placeholder string in place of each decoded binary field.</summary>
    public JsonElement RawValue { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; }
}

/// <summary>Handles a JSON response. Maps to the <c>ResponseHandler</c> that <c>createJsonResponseHandler</c> returns.</summary>
public delegate Task<JsonResponseHandlerResult<T>> JsonResponseHandler<T>(
    HttpResponseMessage response,
    string url,
    object? requestBodyValues,
    CancellationToken cancellationToken
);

/// <summary>
/// Response handlers for binary, event stream, and error responses.
/// Maps to <c>createBinaryResponseHandler</c>, <c>createBinaryStreamResponseHandler</c>,
/// <c>createEventSourceResponseHandler</c>, <c>createJsonErrorResponseHandler</c>,
/// and <c>createStatusCodeErrorResponseHandler</c>.
/// </summary>
public static class ResponseHandlers
{
    /// <summary>
    /// Creates a handler that parses a JSON body and validates it with <paramref name="validate"/>. Maps to <c>createJsonResponseHandler</c>.
    /// <paramref name="validate"/> receives the parsed body as <see cref="bool"/>, <see cref="double"/>, <see cref="string"/>,
    /// <see cref="List{T}"/> of objects, <see cref="Dictionary{TKey, TValue}"/> of objects, or <c>null</c>. With
    /// <paramref name="experimentalBinaryFields"/> it also holds the decoded <see cref="byte"/> arrays in place of their placeholders.
    /// A <paramref name="validate"/> of <c>null</c> returns that value as it is. When the validation fails, the error holds the placeholders, never the bytes.
    /// See <see cref="JsonStreams.ReadJsonAsync"/> for the paths.
    /// A body that is not valid JSON, or a validation that fails, throws an <see cref="ApiException"/> with the message
    /// <c>Invalid JSON response</c>, whose cause is a <see cref="JsonParseException"/> or a <see cref="TypeValidationException"/>.
    /// </summary>
    /// <exception cref="Util.InvalidArgumentError">A path in <paramref name="experimentalBinaryFields"/> is not valid.</exception>
    public static JsonResponseHandler<T> CreateJsonResponseHandler<T>(
        Func<object?, T>? validate = null,
        IReadOnlyList<string>? experimentalBinaryFields = null
    )
    {
        var binaryFieldPaths = experimentalBinaryFields is { Count: > 0 }
            ? JsonBinaryFieldScanner.CompileBinaryFieldPaths(experimentalBinaryFields)
            : null;
        return async (response, url, requestBodyValues, cancellationToken) =>
        {
            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            string text;
            IReadOnlyDictionary<string, byte[]> binaries = new Dictionary<string, byte[]>();
            if (binaryFieldPaths is null)
            {
                var bytes = await ResponseBodies
                    .ReadResponseWithSizeLimitAsync(
                        response,
                        url,
                        cancellationToken: cancellationToken
                    )
                    .ConfigureAwait(false);
                text = JsonStreams.ResponseEncoding(response).GetString(bytes).TrimStart('\uFEFF');
            }
            else
            {
                var scanner = new JsonBinaryFieldScanner(binaryFieldPaths);
                await ResponseBodies
                    .ReadResponseWithSizeLimitAsync(
                        response,
                        url,
                        cancellationToken: cancellationToken,
                        onChunk: scanner.Push
                    )
                    .ConfigureAwait(false);
                var scanned = scanner.Finish();
                text = scanned.Text.TrimStart('\uFEFF');
                binaries = scanned.Binaries;
            }

            var headers = JsonStreams.ExtractResponseHeaders(response);
            ApiException Invalid(Exception cause) =>
                new("Invalid JSON response", (int)response.StatusCode, text, cause)
                {
                    Url = url,
                    RequestBodyValues = requestBodyValues,
                    ResponseHeaders = headers,
                };

            var parsed = JsonParsing.SafeParse(text);
            if (!parsed.Success || parsed.RawValue is not { } rawValue)
            {
                throw Invalid(
                    parsed.Error
                        ?? new JsonParseException(text, new JsonException("Invalid JSON response"))
                );
            }

            var replaced = JsonBinaryFieldScanner.ReplaceBinaryFieldPlaceholders(
                rawValue,
                binaries
            );
            T value;
            try
            {
                value = validate is null ? (T)replaced! : validate(replaced);
            }
            catch (Exception error)
                when (error is not OperationCanceledException and not OutOfMemoryException)
            {
                // The error holds the parsed body with placeholders: the bytes never reach a message.
                throw Invalid(
                    new TypeValidationException(
                        rawValue,
                        JsonBinaryFieldScanner.RedactBinaryFields(error, binaries),
                        null
                    )
                );
            }

            return new JsonResponseHandlerResult<T>(value, rawValue, headers);
        };
    }

    /// <summary>Reads the body as bytes. Maps to <c>createBinaryResponseHandler</c>.</summary>
    /// <exception cref="ApiException">The response has no body.</exception>
    /// <exception cref="Util.DownloadError">The body is larger than <see cref="Util.Download.DefaultMaxBytes"/>.</exception>
    public static async Task<ResponseHandlerResult<byte[]>> CreateBinaryResponseAsync(
        HttpResponseMessage response,
        string url,
        object? requestBodyValues = null,
        CancellationToken cancellationToken = default
    )
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var headers = JsonStreams.ExtractResponseHeaders(response);
        if (IsEmptyBody(response))
        {
            throw new ApiException("Response body is empty", (int)response.StatusCode, null)
            {
                Url = url,
                RequestBodyValues = requestBodyValues,
                ResponseHeaders = headers,
            };
        }

        var bytes = await ResponseBodies
            .ReadResponseWithSizeLimitAsync(response, url, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return new ResponseHandlerResult<byte[]>(bytes, headers);
    }

    /// <summary>Passes the body through as a stream. Maps to <c>createBinaryStreamResponseHandler</c>.</summary>
    /// <exception cref="InvalidOperationException">The response has no body.</exception>
    public static async Task<ResponseHandlerResult<Stream>> CreateBinaryStreamResponseAsync(
        HttpResponseMessage response
    )
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (IsEmptyBody(response))
        {
            throw new InvalidOperationException("Empty response body");
        }

        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        return new ResponseHandlerResult<Stream>(
            stream,
            JsonStreams.ExtractResponseHeaders(response)
        );
    }

    /// <summary>
    /// Parses a Server-Sent Event body into JSON values. A read failure on the successful response is
    /// rethrown as a retryable <see cref="ApiException"/> that keeps the status, URL, headers, and cause.
    /// Maps to <c>createEventSourceResponseHandler</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">The response has no body.</exception>
    public static async Task<
        ResponseHandlerResult<IAsyncEnumerable<JsonParseResult>>
    > CreateEventSourceResponseAsync(
        HttpResponseMessage response,
        string url,
        object? requestBodyValues,
        JsonNode? schema,
        CancellationToken cancellationToken = default
    )
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (IsEmptyBody(response))
        {
            throw new InvalidOperationException("Empty response body");
        }

        var headers = JsonStreams.ExtractResponseHeaders(response);
        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        return new ResponseHandlerResult<IAsyncEnumerable<JsonParseResult>>(
            ReadEvents(
                stream,
                schema,
                (int)response.StatusCode,
                url,
                requestBodyValues,
                headers,
                cancellationToken
            ),
            headers
        );
    }

    /// <summary>
    /// Builds an <see cref="ApiException"/> from an error response whose body is JSON.
    /// The message comes from <paramref name="errorToMessage"/>. When the body is not valid for the schema, the status text is used.
    /// Maps to <c>createJsonErrorResponseHandler</c>.
    /// </summary>
    /// <exception cref="Util.DownloadError">The body is larger than <see cref="Util.Download.DefaultMaxBytes"/>.</exception>
    public static async Task<ApiException> CreateJsonErrorResponseAsync(
        HttpResponseMessage response,
        string url,
        object? requestBodyValues,
        JsonNode? errorSchema,
        Func<JsonElement, string> errorToMessage,
        CancellationToken cancellationToken = default
    )
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (errorToMessage is null)
        {
            throw new ArgumentNullException(nameof(errorToMessage));
        }

        var body = await ReadTextAsync(response, url, cancellationToken).ConfigureAwait(false);
        var headers = JsonStreams.ExtractResponseHeaders(response);
        var statusCode = (int)response.StatusCode;
        var statusText = response.ReasonPhrase ?? string.Empty;
        if (string.IsNullOrWhiteSpace(body))
        {
            return Create(statusText, statusCode, body, url, requestBodyValues, headers);
        }

        var parsed = JsonParsing.SafeParse(body, errorSchema);
        if (!parsed.Success || parsed.Value is null)
        {
            return Create(statusText, statusCode, body, url, requestBodyValues, headers);
        }

        return Create(
            errorToMessage(parsed.Value.Value),
            statusCode,
            body,
            url,
            requestBodyValues,
            headers,
            parsed.Value.Value.Clone()
        );
    }

    /// <summary>
    /// Builds an <see cref="ApiException"/> whose message is the status text.
    /// Maps to <c>createStatusCodeErrorResponseHandler</c>.
    /// </summary>
    /// <exception cref="Util.DownloadError">The body is larger than <see cref="Util.Download.DefaultMaxBytes"/>.</exception>
    public static async Task<ApiException> CreateStatusCodeErrorResponseAsync(
        HttpResponseMessage response,
        string url,
        object? requestBodyValues = null,
        CancellationToken cancellationToken = default
    )
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var body = await ReadTextAsync(response, url, cancellationToken).ConfigureAwait(false);
        return Create(
            response.ReasonPhrase ?? string.Empty,
            (int)response.StatusCode,
            body,
            url,
            requestBodyValues,
            JsonStreams.ExtractResponseHeaders(response)
        );
    }

    private static async IAsyncEnumerable<JsonParseResult> ReadEvents(
        Stream stream,
        JsonNode? schema,
        int statusCode,
        string url,
        object? requestBodyValues,
        IReadOnlyDictionary<string, string> headers,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        using (stream)
        {
            var enumerator = JsonStreams
                .ReadJsonEventsAsync(stream, schema, cancellationToken)
                .GetAsyncEnumerator(cancellationToken);
            try
            {
                while (true)
                {
                    bool hasNext;
                    try
                    {
                        hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                    }
                    catch (Exception exception)
                        when (exception is IOException || exception is HttpRequestException)
                    {
                        throw new ApiException(
                            "Failed to process successful response",
                            statusCode,
                            null,
                            exception
                        )
                        {
                            Url = url,
                            RequestBodyValues = requestBodyValues,
                            ResponseHeaders = headers,
                            IsRetryable = true,
                        };
                    }

                    if (!hasNext)
                    {
                        yield break;
                    }

                    yield return enumerator.Current;
                }
            }
            finally
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private static ApiException Create(
        string message,
        int statusCode,
        string? body,
        string url,
        object? requestBodyValues,
        IReadOnlyDictionary<string, string> headers,
        object? errorData = null
    )
    {
        return new ApiException(message, statusCode, body)
        {
            ErrorData = errorData,
            Url = url,
            RequestBodyValues = requestBodyValues,
            ResponseHeaders = headers,
        };
    }

    private static async Task<string> ReadTextAsync(
        HttpResponseMessage response,
        string url,
        CancellationToken cancellationToken
    )
    {
        var bytes = await ResponseBodies
            .ReadResponseWithSizeLimitAsync(response, url, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return Encoding.UTF8.GetString(bytes);
    }

    private static bool IsEmptyBody(HttpResponseMessage response)
    {
        return response.Content is null || response.Content.Headers.ContentLength == 0;
    }
}
