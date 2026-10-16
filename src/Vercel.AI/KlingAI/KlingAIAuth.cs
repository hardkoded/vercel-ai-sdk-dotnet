// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Vercel.AI.Provider;

namespace Vercel.AI.KlingAI;

/// <summary>
/// Resolves the bearer token for KlingAI API requests.
/// KlingAI accepts a single API key sent as a bearer token, or a legacy access key / secret key pair that signs a short-lived JWT.
/// </summary>
public static class KlingAIAuth
{
    /// <summary>Environment variable that holds the API key.</summary>
    public const string ApiKeyEnvironmentVariable = "KLINGAI_API_KEY";

    /// <summary>Environment variable that holds the legacy access key.</summary>
    public const string AccessKeyEnvironmentVariable = "KLINGAI_ACCESS_KEY";

    /// <summary>Environment variable that holds the legacy secret key.</summary>
    public const string SecretKeyEnvironmentVariable = "KLINGAI_SECRET_KEY";

    /// <summary>
    /// Resolves the bearer token. Explicit values win over environment variables, and the API key wins over the access key / secret key pair:
    /// <c>apiKey</c>, then <c>accessKey</c> with <c>secretKey</c>, then <c>KLINGAI_API_KEY</c>, then <c>KLINGAI_ACCESS_KEY</c> with <c>KLINGAI_SECRET_KEY</c>.
    /// </summary>
    public static string ResolveAuthToken(string? apiKey = null, string? accessKey = null, string? secretKey = null)
    {
        var explicitApiKey = apiKey?.Trim();
        if (!string.IsNullOrEmpty(explicitApiKey))
        {
            return explicitApiKey!;
        }

        if (accessKey != null && secretKey != null)
        {
            return GenerateAuthToken(accessKey, secretKey);
        }

        var environmentApiKey = LoadTrimmed(null, ApiKeyEnvironmentVariable);
        if (environmentApiKey != null)
        {
            return environmentApiKey;
        }

        var hasLegacyCredentials = LoadTrimmed(accessKey, AccessKeyEnvironmentVariable) != null
            || LoadTrimmed(secretKey, SecretKeyEnvironmentVariable) != null;
        if (!hasLegacyCredentials)
        {
            throw new AiSdkException(
                "KlingAI API key is missing. Pass it using the 'apiKey' parameter "
                + "or the KLINGAI_API_KEY environment variable. Alternatively, pass the "
                + "legacy 'accessKey' and 'secretKey' parameters or the "
                + "KLINGAI_ACCESS_KEY and KLINGAI_SECRET_KEY environment variables.");
        }

        return GenerateAuthToken(accessKey, secretKey);
    }

    /// <summary>
    /// Signs a JWT (HS256) from a legacy access key / secret key pair. The token is valid for 30 minutes.
    /// A missing key falls back to its environment variable.
    /// </summary>
    public static string GenerateAuthToken(string? accessKey = null, string? secretKey = null)
    {
        var ak = LoadSetting(accessKey, "accessKey", AccessKeyEnvironmentVariable, "KlingAI access key");
        var sk = LoadSetting(secretKey, "secretKey", SecretKeyEnvironmentVariable, "KlingAI secret key");
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var header = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";
        var payload = "{\"iss\":" + System.Text.Json.JsonSerializer.Serialize(ak)
            + ",\"exp\":" + (now + 1800).ToString(CultureInfo.InvariantCulture)
            + ",\"nbf\":" + (now - 5).ToString(CultureInfo.InvariantCulture) + "}";
        var signingInput = Base64Url(Encoding.UTF8.GetBytes(header)) + "." + Base64Url(Encoding.UTF8.GetBytes(payload));
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sk));
        var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
        return signingInput + "." + Base64Url(signature);
    }

    private static string Base64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static string? LoadTrimmed(string? value, string environmentVariable)
    {
        var resolved = value ?? Environment.GetEnvironmentVariable(environmentVariable);
        resolved = resolved?.Trim();
        return string.IsNullOrEmpty(resolved) ? null : resolved;
    }

    private static string LoadSetting(string? value, string name, string environmentVariable, string description)
    {
        if (value != null)
        {
            return value;
        }

        var fromEnvironment = Environment.GetEnvironmentVariable(environmentVariable);
        if (fromEnvironment == null)
        {
            throw new AiSdkException(description + " is missing. Pass it using the '" + name + "' parameter or the " + environmentVariable + " environment variable.");
        }

        return fromEnvironment;
    }
}
