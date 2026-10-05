// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Builds Bedrock service hosts. Partition prefixes select the DNS suffix.</summary>
public static class AmazonBedrockEndpoints
{
    /// <summary>
    /// Returns <paramref name="baseUrl"/> when it is set.
    /// Otherwise returns <c>https://{service}.{region}.{suffix}</c>.
    /// </summary>
    public static string Resolve(string? baseUrl, string region, string service)
    {
        if (!string.IsNullOrEmpty(baseUrl))
        {
            return baseUrl!.TrimEnd('/');
        }

        if (!HostnameParts.IsValidHostnamePart(region))
        {
            throw new ArgumentException("An AWS region must be a single DNS label.", nameof(region));
        }

        return "https://" + service + "." + region + "." + DnsSuffix(region);
    }

    /// <summary>DNS suffix for an AWS partition. The first matching prefix wins, matching the upstream list order.</summary>
    public static string DnsSuffix(string region)
    {
        if (region.StartsWith("cn-", StringComparison.Ordinal))
        {
            return "amazonaws.com.cn";
        }

        if (region.StartsWith("us-iso-", StringComparison.Ordinal))
        {
            return "c2s.ic.gov";
        }

        if (region.StartsWith("us-isob-", StringComparison.Ordinal))
        {
            return "sc2s.sgov.gov";
        }

        if (region.StartsWith("eu-isoe-", StringComparison.Ordinal))
        {
            return "cloud.adc-e.uk";
        }

        if (region.StartsWith("us-isof-", StringComparison.Ordinal))
        {
            return "csp.hci.ic.gov";
        }

        if (region.StartsWith("eusc-", StringComparison.Ordinal))
        {
            return "amazonaws.eu";
        }

        return "amazonaws.com";
    }
}
