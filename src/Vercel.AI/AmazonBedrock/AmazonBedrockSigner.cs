// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Applies either a Bedrock bearer token or SigV4.</summary>
internal static class AmazonBedrockSigner
{
    /// <summary>
    /// Sends <c>Authorization: Bearer</c> when <paramref name="apiKey"/> is non-blank.
    /// Otherwise signs <paramref name="request"/> for <paramref name="service"/>.
    /// </summary>
    public static void Apply(HttpRequestMessage request, byte[] payload, string region, string service, string? apiKey, string? accessKeyId, string? secretAccessKey, string? sessionToken, DateTimeOffset utcNow)
    {
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + apiKey!.Trim());
            return;
        }

        var accessKey = accessKeyId ?? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secret = secretAccessKey ?? Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secret))
        {
            throw new AiSdkException("AWS credentials are required. Set AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY.");
        }

        var token = sessionToken ?? Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");
        AwsSigV4.Sign(request, payload, region, service, accessKey!, secret!, token, utcNow);
    }
}
