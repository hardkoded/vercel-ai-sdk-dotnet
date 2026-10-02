// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Collections;

namespace Vercel.AI.ProviderUtils;

/// <summary>Provider reference checks. Maps to <c>isProviderReference</c> and <c>resolveProviderReference</c>.</summary>
public static class ProviderReferences
{
    /// <summary>True for a plain object that is not bytes, a URL, or a tagged <c>type</c> object.</summary>
    public static bool IsProviderReference(object? data)
    {
        if (data is null || data is JsUndefined || data is string || data is Uri || data is byte[] || data is Array)
        {
            return false;
        }

        if (data is ValueType)
        {
            return false;
        }

        if (IsBuffer(data))
        {
            return false;
        }

        if (data is IDictionary dictionary && dictionary.Contains("type"))
        {
            return false;
        }

        return data is IDictionary;
    }

    /// <summary>Node <c>Buffer.isBuffer</c>. Always false on .NET.</summary>
    public static bool IsBuffer(object? value)
    {
        return value is JsBuffer;
    }

    /// <summary>Placeholder for a Node.js <c>Buffer</c>. The runtime never produces one.</summary>
    public sealed class JsBuffer
    {
    }

    /// <summary>Returns the id stored for <paramref name="provider"/>.</summary>
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

        string? id;
        if (reference.TryGetValue(provider, out id) && id != null)
        {
            return id;
        }

        throw new NoSuchProviderReferenceError(provider, reference);
    }
}
