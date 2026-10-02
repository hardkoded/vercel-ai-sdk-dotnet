// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Resolves Bedrock runtime and agent-runtime base URLs.</summary>
public static class AmazonBedrockEndpoints
{
    private static readonly string[][] PartitionSuffixes =
    {
        new[] { "us-isob-", "sc2s.sgov.gov" },
        new[] { "us-isof-", "csp.hci.ic.gov" },
        new[] { "us-iso-", "c2s.ic.gov" },
        new[] { "eu-isoe-", "cloud.adc-e.uk" },
        new[] { "cn-", "amazonaws.com.cn" },
        new[] { "eusc-", "amazonaws.eu" },
    };

    /// <summary>Resolves the bedrock-runtime base URL for <paramref name="options"/>.</summary>
    public static string ResolveRuntimeBaseUrl(AmazonBedrockOptions options)
    {
        return Resolve(options, "bedrock-runtime", "AWS_ENDPOINT_URL_BEDROCK_RUNTIME");
    }

    /// <summary>Resolves the bedrock-agent-runtime base URL for <paramref name="options"/>.</summary>
    public static string ResolveAgentRuntimeBaseUrl(AmazonBedrockOptions options)
    {
        return Resolve(options, "bedrock-agent-runtime", "AWS_ENDPOINT_URL_BEDROCK_AGENT_RUNTIME");
    }

    /// <summary>DNS suffix for <paramref name="region"/>, using the longest matching partition prefix.</summary>
    public static string DnsSuffix(string region)
    {
        if (region == null)
        {
            return "amazonaws.com";
        }

        foreach (var pair in PartitionSuffixes)
        {
            if (region.IndexOf(pair[0], StringComparison.Ordinal) == 0)
            {
                return pair[1];
            }
        }

        return "amazonaws.com";
    }

    private static string Resolve(AmazonBedrockOptions options, string service, string serviceEnvironmentVariable)
    {
        options = options ?? new AmazonBedrockOptions();
        var explicitBase = TrimTrailingSlash(options.BaseUrl);
        if (!string.IsNullOrEmpty(explicitBase))
        {
            return explicitBase!;
        }

        var serviceEndpoint = TrimTrailingSlash(Environment.GetEnvironmentVariable(serviceEnvironmentVariable));
        if (!string.IsNullOrEmpty(serviceEndpoint))
        {
            return serviceEndpoint!;
        }

        var globalEndpoint = TrimTrailingSlash(Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL"));
        if (!string.IsNullOrEmpty(globalEndpoint))
        {
            return globalEndpoint!;
        }

        var region = options.Region;
        if (!HostnameParts.IsValidHostnamePart(region))
        {
            throw new ArgumentException("An AWS region must be a single DNS label.", nameof(AmazonBedrockOptions.Region));
        }

        return "https://" + service + "." + region + "." + DnsSuffix(region);
    }

    private static string? TrimTrailingSlash(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value!.Trim().TrimEnd('/');
    }
}
