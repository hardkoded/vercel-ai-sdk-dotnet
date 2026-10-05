// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>Request counts derived from an Anthropic message batch.</summary>
public sealed class AnthropicBatchCounts
{
    /// <summary>Creates counts.</summary>
    public AnthropicBatchCounts(int total, int pending, int completed, int failed)
    {
        Total = total;
        Pending = pending;
        Completed = completed;
        Failed = failed;
    }

    /// <summary>processing + succeeded + errored + canceled + expired.</summary>
    public int Total { get; }

    /// <summary>Requests still processing.</summary>
    public int Pending { get; }

    /// <summary>Succeeded requests.</summary>
    public int Completed { get; }

    /// <summary>errored + canceled + expired.</summary>
    public int Failed { get; }
}

/// <summary>Validation and status mapping for the Anthropic Message Batches API.</summary>
public static class AnthropicBatch
{
    private static readonly Regex IdPattern = new Regex("^[A-Za-z0-9_-]{1,64}$", RegexOptions.CultureInvariant);

    /// <summary>Throws when <paramref name="id"/> is not 1–64 letters, digits, underscores, or hyphens.</summary>
    public static void ValidateRequestId(string id)
    {
        if (id == null || !IdPattern.IsMatch(id))
        {
            throw new AiSdkException("Anthropic batch request ID \"" + id + "\" must match ^[A-Za-z0-9_-]{1,64}$.");
        }
    }

    /// <summary>Validates ids and rejects duplicates before any HTTP call.</summary>
    public static void ValidateRequestIds(IEnumerable<string> ids)
    {
        var seen = new HashSet<string>();
        foreach (var id in ids)
        {
            ValidateRequestId(id);
            if (!seen.Add(id))
            {
                throw new AiSdkException("Anthropic batch request IDs must be unique; duplicate ID \"" + id + "\".");
            }
        }
    }

    /// <summary>Rejects request types the batches API does not accept, such as image.</summary>
    public static void ValidateRequestType(string? type)
    {
        if (type == "image")
        {
            throw new AiSdkException("Unsupported batch request type 'image'.");
        }
    }

    /// <summary>Rejects speed and fallback speed, which batches do not support.</summary>
    public static void RejectSpeed(string? speed, bool fallbackSpeed)
    {
        if (!string.IsNullOrEmpty(speed))
        {
            throw new AiSdkException("Anthropic Message Batches do not support speed (request \"request-1\").");
        }

        if (fallbackSpeed)
        {
            throw new AiSdkException("Anthropic Message Batches do not support fallback speed (request \"request-1\").");
        }
    }

    /// <summary>Maps processing status. <c>ended</c> is completed. Other known states are pending.</summary>
    public static string MapStatus(string? processingStatus)
    {
        return processingStatus == "ended" ? "completed" : "pending";
    }

    /// <summary>Builds SDK counts from the provider request_counts object.</summary>
    public static AnthropicBatchCounts Count(int processing, int succeeded, int errored, int canceled, int expired)
    {
        return new AnthropicBatchCounts(
            processing + succeeded + errored + canceled + expired,
            processing,
            succeeded,
            errored + canceled + expired);
    }

    /// <summary>Reads counts from a batch JSON object.</summary>
    public static AnthropicBatchCounts Count(JsonElement batch)
    {
        if (!batch.TryGetProperty("request_counts", out var counts))
        {
            return new AnthropicBatchCounts(0, 0, 0, 0);
        }

        return Count(
            Number(counts, "processing"),
            Number(counts, "succeeded"),
            Number(counts, "errored"),
            Number(counts, "canceled"),
            Number(counts, "expired"));
    }

    /// <summary>True when a completed batch has no results URL.</summary>
    public static bool MissingResults(JsonElement batch)
    {
        var status = AnthropicJson.String(batch, "processing_status");
        var url = AnthropicJson.String(batch, "results_url");
        return status == "ended" && string.IsNullOrEmpty(url);
    }

    /// <summary>True when the batch is still pending, so results cannot be read.</summary>
    public static bool ResultsUnavailable(JsonElement batch)
    {
        return MapStatus(AnthropicJson.String(batch, "processing_status")) != "completed";
    }

    private static int Number(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.TryGetInt32(out var number) ? number : 0;
    }
}
