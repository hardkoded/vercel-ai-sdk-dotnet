// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// How options found under several <c>providerOptions</c> namespaces are combined. Maps to <c>ProviderOptionsResolution</c>.
/// </summary>
public enum ProviderOptionsResolution
{
    /// <summary>
    /// Namespaces are aliases. The first namespace that is present wins as a whole and later namespaces are
    /// ignored (no merging).
    /// </summary>
    First,

    /// <summary>
    /// Every present namespace is parsed and the results are merged in order, so later namespaces override
    /// earlier ones key by key.
    /// </summary>
    Merge,
}

/// <summary>Result of <see cref="ProviderOptionsResolver.Resolve{T}"/>.</summary>
/// <typeparam name="T">Parsed options type.</typeparam>
/// <param name="Options">The resolved options, or <c>default</c> when no namespace is present.</param>
/// <param name="Namespaces">The namespaces that contributed to <paramref name="Options"/>, in resolution order.</param>
public sealed record ResolvedProviderOptions<T>(T? Options, IReadOnlyList<string> Namespaces);

/// <summary>Resolves options that a model accepts under more than one <c>providerOptions</c> namespace.</summary>
public static class ProviderOptionsResolver
{
    /// <summary>
    /// Parses provider options that a model accepts under more than one <c>providerOptions</c> namespace, for
    /// example a canonical key plus a provider-specific or legacy alias. Maps to <c>resolveProviderOptions</c>.
    /// </summary>
    /// <remarks>
    /// Each present namespace is validated by <paramref name="parse"/>, which stands in for the upstream schema.
    /// A namespace that is missing or <c>null</c> is skipped, and an invalid one makes <paramref name="parse"/> throw.
    /// </remarks>
    /// <param name="providerOptions">Provider options of the call.</param>
    /// <param name="namespaces">Namespaces to look at, in resolution order. Duplicates are ignored.</param>
    /// <param name="resolution">How present namespaces are combined.</param>
    /// <param name="parse">Validates and parses the options of one namespace.</param>
    /// <param name="merge">
    /// Combines two parsed options, the second one winning. Required for <see cref="ProviderOptionsResolution.Merge"/>.
    /// </param>
    /// <returns>The resolved options and the namespaces that contributed to them.</returns>
    public static ResolvedProviderOptions<T> Resolve<T>(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        IEnumerable<string> namespaces,
        ProviderOptionsResolution resolution,
        Func<JsonElement, T> parse,
        Func<T, T, T>? merge = null)
    {
        if (namespaces == null)
        {
            throw new ArgumentNullException(nameof(namespaces));
        }

        if (parse == null)
        {
            throw new ArgumentNullException(nameof(parse));
        }

        if (resolution == ProviderOptionsResolution.Merge && merge == null)
        {
            throw new ArgumentException("A merge function is required to merge namespaces.", nameof(merge));
        }

        var matched = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        T? result = default;
        foreach (var name in namespaces)
        {
            if (!seen.Add(name))
            {
                continue;
            }

            if (providerOptions == null
                || !providerOptions.TryGetValue(name, out var element)
                || element.ValueKind == JsonValueKind.Null
                || element.ValueKind == JsonValueKind.Undefined)
            {
                continue;
            }

            var parsed = parse(element);
            if (resolution == ProviderOptionsResolution.First)
            {
                return new ResolvedProviderOptions<T>(parsed, new[] { name });
            }

            result = matched.Count == 0 ? parsed : merge!(result!, parsed);
            matched.Add(name);
        }

        return new ResolvedProviderOptions<T>(result, matched);
    }
}
