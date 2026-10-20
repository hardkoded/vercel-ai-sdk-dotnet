// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.AmazonBedrock;

/// <summary>Clears the endpoint and region variables for a test and restores them on dispose.</summary>
public class BedrockEndpointEnvironment : IDisposable
{
    private static readonly string[] Names =
    [
        "AWS_REGION",
        "AWS_ENDPOINT_URL",
        "AWS_ENDPOINT_URL_BEDROCK_RUNTIME",
        "AWS_ENDPOINT_URL_BEDROCK_AGENT_RUNTIME",
    ];

    private readonly Dictionary<string, string?> _original = new();

    public BedrockEndpointEnvironment()
    {
        foreach (var name in Names)
        {
            _original[name] = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    public void Dispose()
    {
        foreach (var pair in _original)
        {
            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }

        GC.SuppressFinalize(this);
    }
}

[CollectionDefinition("BedrockEndpointEnvironment", DisableParallelization = true)]
public sealed class BedrockEndpointEnvironmentCollection;
