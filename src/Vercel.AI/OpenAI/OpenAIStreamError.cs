// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>Classifies OpenAI stream error frames and checks a stream for an error before its first output.</summary>
internal static class OpenAIStreamError
{
    /// <summary>
    /// Reads an OpenAI stream error frame into status and retry metadata.
    /// Returns null when <paramref name="frame"/> is not an error.
    /// </summary>
    public static ProviderStreamError? CreateProviderStreamError(JsonElement frame)
    {
        if (!TryParse(frame, out var message, out var code, out var type))
        {
            return null;
        }

        var statusCode = StatusCode(code, type);
        return new ProviderStreamError(message, frame.Clone(), type, code, statusCode, IsRetryable(code, type, statusCode));
    }

    /// <summary>
    /// Reads <paramref name="source"/> until the first output chunk and throws when an error frame comes first.
    /// After an accepted chunk, each read waits at most <paramref name="acceptedGraceMs"/>.
    /// The returned stream replays every chunk read so far. The caller still owns <paramref name="source"/>.
    /// </summary>
    public static async Task<IAsyncEnumerable<T>> ThrowIfErrorBeforeOutputAsync<T>(
        IAsyncEnumerator<T> source,
        Func<T, JsonElement?> getError,
        Func<T, bool> isOutputChunk,
        Func<T, bool>? isAcceptedChunk = null,
        int acceptedGraceMs = 50)
    {
        var read = new List<T>();
        var accepted = false;
        while (true)
        {
            var next = source.MoveNextAsync().AsTask();
            if (accepted)
            {
                using var grace = new CancellationTokenSource();
                var winner = await Task.WhenAny(next, Task.Delay(acceptedGraceMs, grace.Token)).ConfigureAwait(false);
                grace.Cancel();
                if (winner != next)
                {
                    return Replay(read, next, source);
                }
            }

            if (!await next.ConfigureAwait(false))
            {
                return Replay(read, next, source);
            }

            var chunk = source.Current;
            read.Add(chunk);
            var frame = getError(chunk);
            if (frame.HasValue)
            {
                throw CreateError(frame.Value);
            }

            if (isOutputChunk(chunk))
            {
                return Replay(read, null, source);
            }

            accepted = accepted || isAcceptedChunk?.Invoke(chunk) == true;
        }
    }

    private static async IAsyncEnumerable<T> Replay<T>(List<T> read, Task<bool>? pending, IAsyncEnumerator<T> source)
    {
        foreach (var chunk in read)
        {
            yield return chunk;
        }

        if (pending != null)
        {
            if (!await pending.ConfigureAwait(false))
            {
                yield break;
            }

            yield return source.Current;
        }

        while (await source.MoveNextAsync().ConfigureAwait(false))
        {
            yield return source.Current;
        }
    }

    private static ApiException CreateError(JsonElement frame)
    {
        var streamError = CreateProviderStreamError(frame);
        var error = ProviderHttp.MapStatus(streamError?.StatusCode ?? 500, frame.GetRawText());
        return OpenAICompatibleChat.WithMessage(error, streamError?.Message ?? "OpenAI stream failed before any output was generated");
    }

    private static bool TryParse(JsonElement frame, out string message, out object? code, out string? type)
    {
        message = string.Empty;
        code = null;
        type = null;
        if (frame.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (String(frame, "type") == "response.failed")
        {
            if (!frame.TryGetProperty("response", out var response)
                || response.ValueKind != JsonValueKind.Object
                || !response.TryGetProperty("error", out var responseError)
                || responseError.ValueKind != JsonValueKind.Object
                || String(responseError, "message") is not string responseMessage)
            {
                return false;
            }

            message = responseMessage;
            code = Code(responseError);
            type = "response.failed";
            return true;
        }

        var nested = frame.TryGetProperty("error", out var inner) && inner.ValueKind == JsonValueKind.Object;
        var error = nested ? inner : frame;
        if (String(error, "message") is not string errorMessage
            || !(nested || String(error, "type") != null || error.TryGetProperty("code", out _) || error.TryGetProperty("param", out _)))
        {
            return false;
        }

        message = errorMessage;
        code = Code(error);
        type = String(error, "type");
        return true;
    }

    private static int StatusCode(object? code, string? type)
    {
        var explicitStatus = code switch
        {
            string text when text.Length == 3 && text.All(char.IsDigit) => int.Parse(text, System.Globalization.CultureInfo.InvariantCulture),
            long number when number >= 400 && number <= 599 => (int)number,
            _ => 0,
        };
        if (explicitStatus >= 400 && explicitStatus <= 599)
        {
            return explicitStatus;
        }

        var discriminator = string.Join(" ", new[] { code?.ToString(), type }.Where(value => value != null)).ToLowerInvariant();
        if (discriminator.Contains("insufficient_quota") || discriminator.Contains("rate_limit"))
        {
            return 429;
        }

        if (discriminator.Contains("authentication"))
        {
            return 401;
        }

        if (discriminator.Contains("permission"))
        {
            return 403;
        }

        if (discriminator.Contains("not_found"))
        {
            return 404;
        }

        if (discriminator.Contains("invalid") || discriminator.Contains("bad_request") || discriminator.Contains("context_length"))
        {
            return 400;
        }

        if (discriminator.Contains("overload"))
        {
            return 503;
        }

        return discriminator.Contains("timeout") ? 504 : 500;
    }

    private static bool IsRetryable(object? code, string? type, int statusCode)
    {
        if (code as string == "insufficient_quota" || type == "insufficient_quota")
        {
            return false;
        }

        return statusCode is 408 or 409 or 429 || statusCode >= 500;
    }

    private static object? Code(JsonElement error)
    {
        if (!error.TryGetProperty("code", out var code))
        {
            return null;
        }

        return code.ValueKind switch
        {
            JsonValueKind.String => code.GetString(),
            JsonValueKind.Number => code.TryGetInt64(out var number) ? (object)number : code.GetDouble(),
            _ => null,
        };
    }

    private static string? String(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }
}
