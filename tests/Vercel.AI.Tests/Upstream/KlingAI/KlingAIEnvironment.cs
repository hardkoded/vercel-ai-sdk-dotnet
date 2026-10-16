// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.KlingAI;

/// <summary>Serializes tests that mutate the KlingAI environment variables.</summary>
[CollectionDefinition(Name)]
public sealed class KlingAIEnvironmentCollection
{
    /// <summary>Collection name.</summary>
    public const string Name = "KlingAIEnvironment";
}

/// <summary>Like vitest <c>vi.stubEnv</c> and <c>vi.unstubAllEnvs</c>: clears the KlingAI variables, sets stubs, restores on dispose.</summary>
internal sealed class KlingAIEnvironment : IDisposable
{
    private static readonly string[] Names = ["KLINGAI_API_KEY", "KLINGAI_ACCESS_KEY", "KLINGAI_SECRET_KEY"];

    private readonly Dictionary<string, string?> _saved = new(StringComparer.Ordinal);

    internal KlingAIEnvironment()
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

/// <summary>Reads JWT parts like the upstream <c>decodePayload</c>.</summary>
internal static class KlingAIJwt
{
    internal static System.Text.Json.Nodes.JsonObject Decode(string token, int part)
    {
        var text = token.Split('.')[part].Replace('-', '+').Replace('_', '/');
        text = text.PadRight(text.Length + ((4 - (text.Length % 4)) % 4), '=');
        return System.Text.Json.Nodes.JsonNode.Parse(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(text)))!.AsObject();
    }
}
