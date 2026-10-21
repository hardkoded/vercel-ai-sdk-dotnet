// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Polls <c>GET /interactions/{id}</c> until the interaction reaches a terminal status.</summary>
internal static class GoogleInteractionsPolling
{
    /// <summary>Starts at 1 s, doubles each tick up to 10 s, and gives up after 30 minutes.</summary>
    internal static readonly TimeSpan DefaultInitialDelay = TimeSpan.FromSeconds(1);

    internal static readonly TimeSpan DefaultMaxDelay = TimeSpan.FromSeconds(10);

    internal static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(30);

    private static readonly TimeSpan CancelTimeout = TimeSpan.FromSeconds(10);

    /// <summary>True for <c>completed</c>, <c>failed</c>, <c>cancelled</c>, and <c>incomplete</c>.</summary>
    internal static bool IsTerminal(string? status)
    {
        return status == "completed" || status == "failed" || status == "cancelled" || status == "incomplete";
    }

    /// <summary>
    /// Polls until the status is terminal. Throws when the loop exceeds <paramref name="timeout"/> or the interaction has no id.
    /// When the caller cancels, sends a best-effort <c>POST /interactions/{id}/cancel</c> and rethrows.
    /// A timeout only stops the wait. It does not cancel the interaction, so the server-side run may continue.
    /// </summary>
    internal static async Task<JsonDocument> PollUntilTerminalAsync(
        ProviderHttp http,
        string baseUrl,
        string? interactionId,
        IReadOnlyDictionary<string, string?> headers,
        TimeSpan? initialDelay,
        TimeSpan? maxDelay,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(interactionId))
        {
            throw new InvalidOperationException("google.interactions: cannot poll a background interaction without an id. The POST response did not include an interaction id.");
        }

        var id = interactionId!;
        var limit = timeout ?? DefaultTimeout;
        var ceiling = maxDelay ?? DefaultMaxDelay;
        var nextDelay = initialDelay ?? DefaultInitialDelay;
        var url = ProviderUtils.ApiKeys.Combine(baseUrl, "interactions/" + Uri.EscapeDataString(id));
        var startedAt = DateTime.UtcNow;
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (DateTime.UtcNow - startedAt > limit)
                {
                    throw new TimeoutException("google.interactions: timed out polling interaction " + id + " after " + (long)limit.TotalMilliseconds + "ms.");
                }

                await Task.Delay(nextDelay, cancellationToken).ConfigureAwait(false);
                var document = await http.SendJsonAsync(HttpMethod.Get, url, null, headers, cancellationToken).ConfigureAwait(false);
                var status = document.RootElement.ValueKind == JsonValueKind.Object
                    && document.RootElement.TryGetProperty("status", out var value)
                    && value.ValueKind == JsonValueKind.String
                    ? value.GetString()
                    : null;
                if (IsTerminal(status))
                {
                    return document;
                }

                document.Dispose();
                nextDelay = TimeSpan.FromTicks(Math.Min(nextDelay.Ticks * 2, ceiling.Ticks));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await CancelOnServerAsync(http, baseUrl, id, headers).ConfigureAwait(false);
            throw;
        }
    }

    // Best effort: stops the run billing on Google's side. Errors are ignored, because the caller is already handling a cancelled run.
    private static async Task CancelOnServerAsync(ProviderHttp http, string baseUrl, string id, IReadOnlyDictionary<string, string?> headers)
    {
        using var timeout = new CancellationTokenSource(CancelTimeout);
        try
        {
            using var response = await http.SendJsonAsync(
                HttpMethod.Post,
                ProviderUtils.ApiKeys.Combine(baseUrl, "interactions/" + Uri.EscapeDataString(id) + "/cancel"),
                "{}",
                headers,
                timeout.Token).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Cancel is best effort.
        }
    }
}
