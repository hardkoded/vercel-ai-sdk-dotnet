// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.ProviderUtils;

/// <summary>Resolves an API key from an explicit value or an environment variable.</summary>
public static class ApiKeys
{
    /// <summary>Returns the first non-empty key, or throws.</summary>
    public static string Require(string? apiKey, string environmentVariable, params string[] additionalEnvironmentVariables)
    {
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return apiKey!;
        }

        var fromEnvironment = Environment.GetEnvironmentVariable(environmentVariable);
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment!;
        }

        if (additionalEnvironmentVariables != null)
        {
            foreach (var name in additionalEnvironmentVariables)
            {
                var value = Environment.GetEnvironmentVariable(name);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value!;
                }
            }
        }

        throw new AiSdkException(
            "API key is required. Pass it explicitly or set the " + environmentVariable + " environment variable.");
    }

    /// <summary>Joins a base URL and a relative path.</summary>
    public static Uri Combine(string baseUrl, string relativePath)
    {
        var trimmed = baseUrl.TrimEnd('/');
        var path = relativePath.TrimStart('/');
        return new Uri(trimmed + "/" + path, UriKind.Absolute);
    }
}
