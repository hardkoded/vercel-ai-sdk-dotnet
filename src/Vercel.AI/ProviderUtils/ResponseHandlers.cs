// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;

namespace Vercel.AI.ProviderUtils;

/// <summary>Inputs shared by response handlers.</summary>
public sealed class ResponseHandlerContext
{
    /// <summary>Creates a context.</summary>
    public ResponseHandlerContext(string url, object? requestBodyValues, FetchResponse response)
    {
        Url = url ?? string.Empty;
        RequestBodyValues = requestBodyValues;
        Response = response ?? throw new ArgumentNullException(nameof(response));
    }

    /// <summary>Request URL.</summary>
    public string Url { get; }

    /// <summary>Request body that was sent.</summary>
    public object? RequestBodyValues { get; }

    /// <summary>Fetch response.</summary>
    public FetchResponse Response { get; }
}

/// <summary>Parsed response plus headers. Maps to the object returned by a <c>ResponseHandler</c>.</summary>
public sealed class HandledResponse<T>
{
    /// <summary>Creates a result.</summary>
    public HandledResponse(T value, Dictionary<string, string> responseHeaders)
    {
        Value = value;
        ResponseHeaders = responseHeaders ?? new Dictionary<string, string>();
    }

    /// <summary>Parsed value.</summary>
    public T Value { get; }

    /// <summary>Value before schema transformation.</summary>
    public JsonNode? RawValue { get; set; }

    /// <summary>True when <see cref="RawValue"/> was captured.</summary>
    public bool HasRawValue { get; set; }

    /// <summary>Response headers with lowercase names.</summary>
    public Dictionary<string, string> ResponseHeaders { get; }
}

/// <summary>Reads and parses provider HTTP responses. Maps to <c>response-handler</c>.</summary>
public static class ResponseHandlers
{
    /// <summary>Default maximum download size: 2 GiB. Maps to <c>DEFAULT_MAX_DOWNLOAD_SIZE</c>.</summary>
    public const long DefaultMaxDownloadSize = 2L * 1024 * 1024 * 1024;

    /// <summary>Cancels <paramref name="response"/>'s body and ignores cancel failures. Maps to <c>cancelResponseBody</c>.</summary>
    public static async Task CancelResponseBody(FetchResponse response)
    {
        if (response is null || response.Body is null)
        {
            return;
        }

        try
        {
            await response.CancelBodyAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// Reads the body, rejecting it when <c>Content-Length</c> or the streamed size exceeds <paramref name="maxBytes"/>.
    /// Maps to <c>readResponseWithSizeLimit</c>.
    /// </summary>
    public static async Task<byte[]> ReadResponseWithSizeLimit(FetchResponse response, string url, long? maxBytes = null)
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var limit = maxBytes ?? DefaultMaxDownloadSize;
        string? contentLength;
        if (response.Headers.TryGetValue("content-length", out contentLength) && contentLength != null)
        {
            long length;
            if (long.TryParse(contentLength, out length) && length > limit)
            {
                await CancelResponseBody(response).ConfigureAwait(false);
                throw new DownloadError(
                    url,
                    "Download of " + url + " exceeded maximum size of " + limit.ToString(System.Globalization.CultureInfo.InvariantCulture) + " bytes (Content-Length: " + length.ToString(System.Globalization.CultureInfo.InvariantCulture) + ").");
            }
        }

        if (response.Body is null)
        {
            return Array.Empty<byte>();
        }

        response.Locked = true;
        var chunks = new List<byte[]>();
        long total = 0;
        var buffer = new byte[8192];
        try
        {
            while (true)
            {
                var read = await response.Body.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                if (read == 0)
                {
                    break;
                }

                total += read;
                if (total > limit)
                {
                    throw new DownloadError(
                        url,
                        "Download of " + url + " exceeded maximum size of " + limit.ToString(System.Globalization.CultureInfo.InvariantCulture) + " bytes.");
                }

                var chunk = new byte[read];
                Buffer.BlockCopy(buffer, 0, chunk, 0, read);
                chunks.Add(chunk);
            }
        }
        finally
        {
            try
            {
                await response.CancelBodyAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
            finally
            {
                response.Locked = false;
            }
        }

        var result = new byte[total];
        var offset = 0;
        foreach (var chunk in chunks)
        {
            Buffer.BlockCopy(chunk, 0, result, offset, chunk.Length);
            offset += chunk.Length;
        }

        return result;
    }

    /// <summary>Parses a JSON body and validates it. Maps to <c>createJsonResponseHandler</c>.</summary>
    public static Func<ResponseHandlerContext, Task<HandledResponse<JsonNode?>>> CreateJsonResponseHandler(object schema)
    {
        return async context =>
        {
            var text = await ReadBodyText(context).ConfigureAwait(false);
            var parsed = JsonParsing.SafeParseJson(text, schema);
            var headers = HeaderFunctions.ExtractResponseHeaders(context.Response);
            if (!parsed.Success)
            {
                throw new APICallError(
                    "Invalid JSON response",
                    context.Url,
                    context.RequestBodyValues,
                    context.Response.Status,
                    headers,
                    text,
                    parsed.Error);
            }

            return new HandledResponse<JsonNode?>(parsed.Value, headers)
            {
                RawValue = parsed.RawValue,
                HasRawValue = parsed.HasRawValue,
            };
        };
    }

    /// <summary>Parses an error JSON body into an <see cref="APICallError"/>. Maps to <c>createJsonErrorResponseHandler</c>.</summary>
    public static Func<ResponseHandlerContext, Task<HandledResponse<APICallError>>> CreateJsonErrorResponseHandler(
        object errorSchema,
        Func<JsonNode?, string> errorToMessage,
        Func<FetchResponse, JsonNode?, bool>? isRetryable = null)
    {
        if (errorToMessage is null)
        {
            throw new ArgumentNullException(nameof(errorToMessage));
        }

        return async context =>
        {
            var text = await ReadBodyText(context).ConfigureAwait(false);
            var headers = HeaderFunctions.ExtractResponseHeaders(context.Response);
            if (text.Trim().Length == 0)
            {
                return new HandledResponse<APICallError>(
                    new APICallError(
                        context.Response.StatusText,
                        context.Url,
                        context.RequestBodyValues,
                        context.Response.Status,
                        headers,
                        text,
                        isRetryable: isRetryable?.Invoke(context.Response, null)),
                    headers);
            }

            try
            {
                var parsed = JsonParsing.ParseJson(text, errorSchema);
                return new HandledResponse<APICallError>(
                    new APICallError(
                        errorToMessage(parsed),
                        context.Url,
                        context.RequestBodyValues,
                        context.Response.Status,
                        headers,
                        text,
                        isRetryable: isRetryable?.Invoke(context.Response, parsed),
                        data: parsed),
                    headers);
            }
            catch (Exception)
            {
                return new HandledResponse<APICallError>(
                    new APICallError(
                        context.Response.StatusText,
                        context.Url,
                        context.RequestBodyValues,
                        context.Response.Status,
                        headers,
                        text,
                        isRetryable: isRetryable?.Invoke(context.Response, null)),
                    headers);
            }
        };
    }

    /// <summary>Parses a Server-Sent Event JSON stream. Maps to <c>createEventSourceResponseHandler</c>.</summary>
    public static Func<ResponseHandlerContext, Task<HandledResponse<IAsyncEnumerable<ParseResult>>>> CreateEventSourceResponseHandler(object schema)
    {
        return context =>
        {
            var headers = HeaderFunctions.ExtractResponseHeaders(context.Response);
            if (context.Response.Body is null)
            {
                throw new EmptyResponseBodyError();
            }

            var stream = ParseJsonEventStream(context, schema);
            return Task.FromResult(new HandledResponse<IAsyncEnumerable<ParseResult>>(stream, headers));
        };
    }

    /// <summary>Parses newline-delimited JSON. Maps to <c>createJsonLinesResponseHandler</c>.</summary>
    public static Func<ResponseHandlerContext, Task<HandledResponse<IAsyncEnumerable<JsonNode?>>>> CreateJsonLinesResponseHandler(object schema)
    {
        return context =>
        {
            var headers = HeaderFunctions.ExtractResponseHeaders(context.Response);
            if (context.Response.Body is null)
            {
                throw new EmptyResponseBodyError();
            }

            return Task.FromResult(new HandledResponse<IAsyncEnumerable<JsonNode?>>(ParseJsonLines(context, schema), headers));
        };
    }

    /// <summary>Reads the body as bytes. Maps to <c>createBinaryResponseHandler</c>.</summary>
    public static Func<ResponseHandlerContext, Task<HandledResponse<byte[]>>> CreateBinaryResponseHandler()
    {
        return async context =>
        {
            var headers = HeaderFunctions.ExtractResponseHeaders(context.Response);
            if (context.Response.Body is null)
            {
                throw new APICallError(
                    "Response body is empty",
                    context.Url,
                    context.RequestBodyValues,
                    context.Response.Status,
                    headers,
                    null);
            }

            try
            {
                var bytes = await ReadResponseWithSizeLimit(context.Response, context.Url).ConfigureAwait(false);
                return new HandledResponse<byte[]>(bytes, headers);
            }
            catch (Exception exception) when (exception is not APICallError && exception is not DownloadError)
            {
                throw new APICallError(
                    "Failed to read response as array buffer",
                    context.Url,
                    context.RequestBodyValues,
                    context.Response.Status,
                    headers,
                    null,
                    exception);
            }
        };
    }

    /// <summary>Returns the response body stream. Maps to <c>createBinaryStreamResponseHandler</c>.</summary>
    public static Func<ResponseHandlerContext, Task<HandledResponse<Stream>>> CreateBinaryStreamResponseHandler()
    {
        return context =>
        {
            var headers = HeaderFunctions.ExtractResponseHeaders(context.Response);
            if (context.Response.Body is null)
            {
                throw new EmptyResponseBodyError();
            }

            return Task.FromResult(new HandledResponse<Stream>(context.Response.Body, headers));
        };
    }

    /// <summary>Builds an <see cref="APICallError"/> from the status line and body. Maps to <c>createStatusCodeErrorResponseHandler</c>.</summary>
    public static Func<ResponseHandlerContext, Task<HandledResponse<APICallError>>> CreateStatusCodeErrorResponseHandler()
    {
        return async context =>
        {
            var text = await ReadBodyText(context).ConfigureAwait(false);
            var headers = HeaderFunctions.ExtractResponseHeaders(context.Response);
            return new HandledResponse<APICallError>(
                new APICallError(
                    context.Response.StatusText,
                    context.Url,
                    context.RequestBodyValues,
                    context.Response.Status,
                    headers,
                    text),
                headers);
        };
    }

    private static async Task<string> ReadBodyText(ResponseHandlerContext context)
    {
        var bytes = await ReadResponseWithSizeLimit(context.Response, context.Url).ConfigureAwait(false);
        return Encoding.UTF8.GetString(bytes);
    }

    private static async IAsyncEnumerable<ParseResult> ParseJsonEventStream(ResponseHandlerContext context, object schema)
    {
        var stream = context.Response.Body ?? Stream.Null;
        var headers = HeaderFunctions.ExtractResponseHeaders(context.Response);
        var decoder = Encoding.UTF8.GetDecoder();
        var parser = new EventSourceParser();
        var buffer = new byte[4096];
        while (true)
        {
            int read;
            try
            {
                read = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                if (AbortErrors.IsAbortError(exception))
                {
                    throw;
                }

                var wrapped = new APICallError(
                    "Failed to process successful response",
                    context.Url,
                    context.RequestBodyValues,
                    context.Response.Status,
                    headers,
                    cause: exception);
                var handled = FetchErrors.HandleFetchError(wrapped, context.Url, context.RequestBodyValues);
                if (handled is Exception handledException)
                {
                    throw handledException;
                }

                throw;
            }

            var finished = read == 0;
            var text = Decode(decoder, buffer, read, finished);
            foreach (var message in parser.Push(text))
            {
                if (message.Data == "[DONE]")
                {
                    continue;
                }

                yield return JsonParsing.SafeParseJson(message.Data, schema);
            }

            if (finished)
            {
                foreach (var message in parser.Flush())
                {
                    if (message.Data == "[DONE]")
                    {
                        continue;
                    }

                    yield return JsonParsing.SafeParseJson(message.Data, schema);
                }

                yield break;
            }
        }
    }

    private static async IAsyncEnumerable<JsonNode?> ParseJsonLines(ResponseHandlerContext context, object schema)
    {
        var stream = context.Response.Body ?? Stream.Null;
        var decoder = Encoding.UTF8.GetDecoder();
        var pending = new StringBuilder();
        var buffer = new byte[4096];
        var finished = false;
        try
        {
            while (true)
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                if (read == 0)
                {
                    finished = true;
                    pending.Append(Decode(decoder, buffer, 0, true));
                    break;
                }

                pending.Append(Decode(decoder, buffer, read, false));
                while (true)
                {
                    var text = pending.ToString();
                    var lineEnd = text.IndexOf('\n');
                    if (lineEnd < 0)
                    {
                        break;
                    }

                    var line = text.Substring(0, lineEnd);
                    if (line.Length > 0 && line[line.Length - 1] == '\r')
                    {
                        line = line.Substring(0, line.Length - 1);
                    }

                    pending.Clear();
                    pending.Append(text.Substring(lineEnd + 1));
                    if (line.Trim().Length > 0)
                    {
                        yield return JsonParsing.ParseJson(line, schema);
                    }
                }
            }

            var finalLine = pending.ToString();
            if (finalLine.Length > 0 && finalLine[finalLine.Length - 1] == '\r')
            {
                finalLine = finalLine.Substring(0, finalLine.Length - 1);
            }

            if (finalLine.Trim().Length > 0)
            {
                yield return JsonParsing.ParseJson(finalLine, schema);
            }
        }
        finally
        {
            if (!finished)
            {
                try
                {
                    await context.Response.CancelBodyAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                }
            }
        }
    }

    private static string Decode(Decoder decoder, byte[] buffer, int count, bool flush)
    {
        var charCount = decoder.GetCharCount(buffer, 0, count, flush);
        if (charCount == 0)
        {
            return string.Empty;
        }

        var chars = new char[charCount];
        decoder.GetChars(buffer, 0, count, chars, 0, flush);
        return new string(chars);
    }
}
