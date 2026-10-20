// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Builds Bedrock service hosts. Partition prefixes select the DNS suffix.</summary>
public static class AmazonBedrockEndpoints
{
    /// <summary>Environment variable that overrides the Bedrock Runtime endpoint.</summary>
    public const string RuntimeEndpointEnvironmentVariable = "AWS_ENDPOINT_URL_BEDROCK_RUNTIME";

    /// <summary>Environment variable that overrides the Bedrock Agent Runtime endpoint.</summary>
    public const string AgentRuntimeEndpointEnvironmentVariable = "AWS_ENDPOINT_URL_BEDROCK_AGENT_RUNTIME";

    /// <summary>Environment variable that overrides every AWS service endpoint.</summary>
    public const string GlobalEndpointEnvironmentVariable = "AWS_ENDPOINT_URL";

    /// <summary>Environment variable that supplies the AWS region.</summary>
    public const string RegionEnvironmentVariable = "AWS_REGION";

    internal static string DefaultRegion() =>
        FirstSet(Environment.GetEnvironmentVariable(RegionEnvironmentVariable)) ?? "us-east-1";

    /// <summary>
    /// Returns <paramref name="baseUrl"/> when it is set, then the service endpoint environment variable, then
    /// <c>AWS_ENDPOINT_URL</c>. Otherwise returns <c>https://{service}.{region}.{suffix}</c>.
    /// </summary>
    public static string Resolve(string? baseUrl, string region, string service)
    {
        return Resolve(baseUrl, () => region, service);
    }

    /// <summary>
    /// Same as <see cref="Resolve(string?, string, string)"/>, but <paramref name="getRegion"/> only runs when no
    /// explicit endpoint is configured.
    /// </summary>
    public static string Resolve(string? baseUrl, Func<string> getRegion, string service)
    {
        var serviceVariable = service == "bedrock-agent-runtime"
            ? AgentRuntimeEndpointEnvironmentVariable
            : RuntimeEndpointEnvironmentVariable;
        var resolved = FirstSet(baseUrl)
            ?? FirstSet(Environment.GetEnvironmentVariable(serviceVariable))
            ?? FirstSet(Environment.GetEnvironmentVariable(GlobalEndpointEnvironmentVariable));
        if (resolved != null)
        {
            return resolved.TrimEnd('/');
        }

        var region = getRegion();
        if (!HostnameParts.IsValidHostnamePart(region))
        {
            throw new ArgumentException("An AWS region must be a single DNS label.", nameof(region));
        }

        return "https://" + service + "." + region + "." + DnsSuffix(region);
    }

    private static string? FirstSet(string? value) => string.IsNullOrEmpty(value) ? null : value;

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
