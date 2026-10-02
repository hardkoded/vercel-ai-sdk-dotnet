// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Validates a provider base URL. Maps to <c>validateBaseURL</c>.</summary>
public static class BaseUrls
{
    /// <summary>Returns <paramref name="baseUrl"/>, or throws when it is empty or whitespace.</summary>
    public static string? ValidateBaseUrl(string? baseUrl)
    {
        if (baseUrl != null && baseUrl.Trim().Length == 0)
        {
            throw new InvalidArgumentError("baseURL", "baseURL must be a non-empty string.");
        }

        return baseUrl;
    }
}
