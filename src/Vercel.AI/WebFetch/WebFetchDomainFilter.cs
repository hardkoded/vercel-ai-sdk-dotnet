// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.Util;

namespace Vercel.AI.WebFetch;

/// <summary>Matches hostnames against the allowed and blocked domains. Maps to <c>domain-filter.ts</c>.</summary>
internal static class WebFetchDomainFilter
{
    private static readonly Regex InvalidDomainCharacters = new(@"[\s/\\:*@?#\[\]]", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Normalizes a hostname for domain matching: lowercase, punycode, and without a trailing dot so <c>example.com.</c>
    /// cannot bypass a filter for <c>example.com</c>.
    /// </summary>
    public static string NormalizeHostname(string hostname)
    {
        return hostname.ToLowerInvariant().TrimEnd('.');
    }

    public static IReadOnlyList<string> NormalizeDomains(string parameter, IReadOnlyList<string> domains)
    {
        var normalized = new List<string>();
        foreach (var domain in domains)
        {
            var trimmed = (domain ?? string.Empty).Trim();

            // Entries are bare domains: no scheme, port, path, credentials, or wildcards.
            if (trimmed.Length == 0 || InvalidDomainCharacters.IsMatch(trimmed))
            {
                throw new InvalidArgumentError(parameter, domains, "Invalid domain \"" + domain + "\". Use bare domains such as \"example.com\"; subdomains are included automatically.");
            }

            Uri parsed;
            if (!Uri.TryCreate("https://" + trimmed, UriKind.Absolute, out parsed!))
            {
                throw new InvalidArgumentError(parameter, domains, "Invalid domain \"" + domain + "\".");
            }

            normalized.Add(NormalizeHostname(parsed.IdnHost));
        }

        return normalized;
    }

    public static Func<Uri, bool> Create(IReadOnlyList<string>? allowedDomains, IReadOnlyList<string>? blockedDomains)
    {
        static bool Matches(string hostname, IReadOnlyList<string> domains)
        {
            foreach (var domain in domains)
            {
                if (hostname == domain || hostname.EndsWith("." + domain, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        return url =>
        {
            var hostname = NormalizeHostname(url.IdnHost);

            if (allowedDomains != null)
            {
                return Matches(hostname, allowedDomains);
            }

            if (blockedDomains != null)
            {
                return !Matches(hostname, blockedDomains);
            }

            return true;
        };
    }
}
