// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>A registry has no provider with the requested id. Maps to <c>NoSuchProviderError</c>.</summary>
public sealed class NoSuchProviderError : NoSuchModelError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public new const string ErrorMarker = "vercel.ai.error.AI_NoSuchProviderError";

    /// <summary>Creates the error.</summary>
    public NoSuchProviderError(string modelId, string modelType, string providerId, IReadOnlyList<string> availableProviders, string? message = null)
        : base(
            "AI_NoSuchProviderError",
            modelId,
            modelType,
            message ?? "No such provider: " + providerId + " (available providers: " + string.Join(",", availableProviders) + ")")
    {
        ProviderId = providerId;
        AvailableProviders = availableProviders;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Requested provider id.</summary>
    public string ProviderId { get; }

    /// <summary>Registered provider ids.</summary>
    public IReadOnlyList<string> AvailableProviders { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
