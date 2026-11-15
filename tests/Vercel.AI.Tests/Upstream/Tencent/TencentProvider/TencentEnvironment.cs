// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Tencent.TencentProvider;

/// <summary>Serializes tests that mutate the Tencent environment variables.</summary>
[CollectionDefinition(Name)]
public sealed class TencentEnvironmentCollection
{
    /// <summary>Collection name.</summary>
    public const string Name = "TencentEnvironment";
}

/// <summary>Like vitest <c>vi.stubEnv</c> and <c>vi.unstubAllEnvs</c>: clears the Tencent variables, sets stubs, restores on dispose.</summary>
internal sealed class TencentEnvironment : IDisposable
{
    private static readonly string[] Names =
    [
        "TENCENT_TOKENHUB_API_KEY", "TENCENTCLOUD_SECRET_ID", "TENCENTCLOUD_SECRET_KEY", "TENCENTCLOUD_SESSION_TOKEN", "TENCENTCLOUD_VOD_SUB_APP_ID",
    ];

    private readonly Dictionary<string, string?> _saved = new(StringComparer.Ordinal);

    internal TencentEnvironment()
    {
        foreach (var name in Names)
        {
            _saved[name] = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    /// <summary>The upstream <c>vi.stubEnv</c>.</summary>
    internal void Stub(string name, string? value)
    {
        Environment.SetEnvironmentVariable(name, value);
    }

    public void Dispose()
    {
        foreach (var pair in _saved)
        {
            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }
    }
}
