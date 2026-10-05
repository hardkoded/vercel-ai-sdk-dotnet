// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using Vercel.AI.Util;

namespace Vercel.AI.ProviderUtils;

/// <summary>Helpers for provider references: maps from provider name to a provider-specific id.</summary>
public static class ProviderReferences
{
    /// <summary>
    /// True for a plain dictionary without a <c>type</c> key. Strings, URLs, bytes, arrays, and primitives are not references.
    /// Maps to <c>isProviderReference</c>.
    /// </summary>
    public static bool IsProviderReference(object? data)
    {
        return data is IDictionary dictionary && !dictionary.Contains("type");
    }

    /// <summary>Returns the id for <paramref name="provider"/>. Maps to <c>resolveProviderReference</c>.</summary>
    /// <exception cref="NoSuchProviderReferenceError">The reference has no entry for <paramref name="provider"/>.</exception>
    public static string ResolveProviderReference(IReadOnlyDictionary<string, string> reference, string provider)
    {
        if (reference is null)
        {
            throw new ArgumentNullException(nameof(reference));
        }

        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        if (reference.TryGetValue(provider, out var id) && id != null)
        {
            return id;
        }

        throw new NoSuchProviderReferenceError(provider, reference);
    }
}

/// <summary>A provider reference has no entry for the requested provider. Maps to <c>NoSuchProviderReferenceError</c>.</summary>
public sealed class NoSuchProviderReferenceError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_NoSuchProviderReferenceError";

    /// <summary>Creates the error.</summary>
    public NoSuchProviderReferenceError(string provider, IReadOnlyDictionary<string, string> reference, string? message = null)
        : base(
            "AI_NoSuchProviderReferenceError",
            message ?? ("No provider reference found for provider '" + provider + "'. Available providers: " + string.Join(", ", reference.Keys)))
    {
        Provider = provider;
        Reference = reference;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Requested provider.</summary>
    public string Provider { get; }

    /// <summary>The reference that was searched.</summary>
    public IReadOnlyDictionary<string, string> Reference { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
