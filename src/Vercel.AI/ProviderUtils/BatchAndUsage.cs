// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Normalized batch request counts. Maps to <c>normalizeBatchRequestCounts</c>.</summary>
public sealed class BatchRequestCounts
{
    /// <summary>Creates counts.</summary>
    public BatchRequestCounts(long total, long pending, long completed, long failed)
    {
        Total = total;
        Pending = pending;
        Completed = completed;
        Failed = failed;
    }

    /// <summary>Total requests.</summary>
    public long Total { get; }

    /// <summary>Pending requests.</summary>
    public long Pending { get; }

    /// <summary>Completed requests.</summary>
    public long Completed { get; }

    /// <summary>Failed requests.</summary>
    public long Failed { get; }
}

/// <summary>Batch count normalization.</summary>
public static class BatchRequests
{
    /// <summary>Largest integer that can be represented exactly as a JavaScript number.</summary>
    public const long MaxSafeInteger = 9007199254740991L;

    /// <summary>
    /// Returns counts when every value is a non-negative safe integer and the parts add up to the total.
    /// </summary>
    public static BatchRequestCounts? NormalizeBatchRequestCounts(double? total, double? pending, double? completed, double? failed)
    {
        if (IsNonNegativeSafeInteger(total)
            && IsNonNegativeSafeInteger(pending)
            && IsNonNegativeSafeInteger(completed)
            && IsNonNegativeSafeInteger(failed)
            && pending!.Value + completed!.Value + failed!.Value == total!.Value)
        {
            return new BatchRequestCounts((long)total.Value, (long)pending.Value, (long)completed.Value, (long)failed.Value);
        }

        return null;
    }

    private static bool IsNonNegativeSafeInteger(double? value)
    {
        if (value is null || double.IsNaN(value.Value) || double.IsInfinity(value.Value))
        {
            return false;
        }

        var number = value.Value;
        return number >= 0 && number <= MaxSafeInteger && number == Math.Floor(number);
    }
}

/// <summary>Input token buckets for an empty usage object.</summary>
public sealed class LanguageModelInputTokens
{
    /// <summary>Total input tokens.</summary>
    public int? Total { get; set; }

    /// <summary>Input tokens that were not cached.</summary>
    public int? NoCache { get; set; }

    /// <summary>Cache-read tokens.</summary>
    public int? CacheRead { get; set; }

    /// <summary>Cache-write tokens.</summary>
    public int? CacheWrite { get; set; }
}

/// <summary>Output token buckets for an empty usage object.</summary>
public sealed class LanguageModelOutputTokens
{
    /// <summary>Total output tokens.</summary>
    public int? Total { get; set; }

    /// <summary>Text tokens.</summary>
    public int? Text { get; set; }

    /// <summary>Reasoning tokens.</summary>
    public int? Reasoning { get; set; }
}

/// <summary>Empty language-model usage. Maps to <c>LanguageModelV4Usage</c> when usage is unavailable.</summary>
public sealed class NullLanguageModelUsage
{
    /// <summary>Input token buckets.</summary>
    public LanguageModelInputTokens InputTokens { get; set; } = new LanguageModelInputTokens();

    /// <summary>Output token buckets.</summary>
    public LanguageModelOutputTokens OutputTokens { get; set; } = new LanguageModelOutputTokens();

    /// <summary>Raw provider usage.</summary>
    public object? Raw { get; set; }
}

/// <summary>Creates empty usage objects. Maps to <c>createNullLanguageModelUsage</c>.</summary>
public static class LanguageModelUsages
{
    /// <summary>Creates a new usage object whose counts are all unset.</summary>
    public static NullLanguageModelUsage CreateNullLanguageModelUsage()
    {
        return new NullLanguageModelUsage();
    }
}

/// <summary>A provider stream error payload. Maps to <c>ProviderStreamError</c>.</summary>
public sealed class ProviderStreamError
{
    internal ProviderStreamError(string message, string? type, object? code, int? statusCode, bool? isRetryable, object? data)
    {
        Message = message;
        Type = type;
        Code = code;
        StatusCode = statusCode;
        IsRetryable = isRetryable;
        Data = data;
    }

    /// <summary>Error message.</summary>
    public string Message { get; }

    /// <summary>Provider error type.</summary>
    public string? Type { get; }

    /// <summary>Provider error code.</summary>
    public object? Code { get; }

    /// <summary>HTTP status.</summary>
    public int? StatusCode { get; }

    /// <summary>Whether the stream error is retryable.</summary>
    public bool? IsRetryable { get; }

    /// <summary>Raw provider payload.</summary>
    public object? Data { get; }
}

/// <summary>Creates provider stream errors. Maps to <c>createProviderStreamError</c>.</summary>
public static class ProviderStreamErrors
{
    /// <summary>Marks a payload as a provider-owned stream error.</summary>
    public static ProviderStreamError CreateProviderStreamError(
        string message,
        object? data,
        string? type = null,
        object? code = null,
        int? statusCode = null,
        bool? isRetryable = null)
    {
        return new ProviderStreamError(message, type, code, statusCode, isRetryable, data);
    }

    /// <summary>True when <paramref name="error"/> was created by <see cref="CreateProviderStreamError"/>.</summary>
    public static bool IsProviderStreamError(object? error)
    {
        return error is ProviderStreamError;
    }
}
