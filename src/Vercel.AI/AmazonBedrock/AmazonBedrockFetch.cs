// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;

namespace Vercel.AI.AmazonBedrock;

/// <summary>AWS credentials used to sign Amazon Bedrock requests.</summary>
public sealed class AmazonBedrockCredentials
{
    /// <summary>Creates credentials.</summary>
    public AmazonBedrockCredentials(string region, string accessKeyId, string secretAccessKey, string? sessionToken = null)
    {
        Region = region ?? string.Empty;
        AccessKeyId = accessKeyId ?? string.Empty;
        SecretAccessKey = secretAccessKey ?? string.Empty;
        SessionToken = sessionToken;
    }

    /// <summary>AWS region.</summary>
    public string Region { get; }

    /// <summary>Access key id.</summary>
    public string AccessKeyId { get; }

    /// <summary>Secret access key.</summary>
    public string SecretAccessKey { get; }

    /// <summary>Optional session token.</summary>
    public string? SessionToken { get; }
}

/// <summary>A request after the Bedrock fetch wrapper has applied user-agent and authentication.</summary>
public sealed class AmazonBedrockPreparedCall
{
    /// <summary>Creates a prepared call.</summary>
    public AmazonBedrockPreparedCall(string method, string url, string? body, IReadOnlyDictionary<string, string> headers, bool signed)
    {
        Method = method;
        Url = url;
        Body = body;
        Headers = headers;
        Signed = signed;
    }

    /// <summary>HTTP method.</summary>
    public string Method { get; }

    /// <summary>Request URL.</summary>
    public string Url { get; }

    /// <summary>Body. Unchanged when the caller passed a string.</summary>
    public string? Body { get; }

    /// <summary>Lower-case headers, including signing headers when <see cref="Signed"/> is true.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>True when the body was signed.</summary>
    public bool Signed { get; }
}

/// <summary>
/// Amazon Bedrock fetch behavior. GET requests and POST requests without a body skip signing.
/// POST bodies are signed with SigV4. Caller headers are signed, except values with non-ASCII characters.
/// </summary>
public static class AmazonBedrockFetch
{
    /// <summary>SigV4 service name used when none is given.</summary>
    public const string DefaultService = "bedrock";

    /// <summary>User agent sent on every request. The runtime segment is dotnet.</summary>
    public static readonly string UserAgent = "ai-sdk-amazon-bedrock/" + AiSdkVersion.Version + " runtime/dotnet";

    /// <summary>
    /// Prepares a call. Non-POST requests and POST requests without a body are not signed.
    /// String bodies are left unchanged. Byte bodies are decoded as UTF-8. Other objects are JSON.
    /// </summary>
    public static AmazonBedrockPreparedCall Prepare(
        string url,
        string? method,
        string? body,
        byte[]? binaryBody,
        JsonNode? objectBody,
        IReadOnlyDictionary<string, string?>? headers,
        IReadOnlyDictionary<string, string?>? requestHeaders,
        AmazonBedrockCredentials credentials,
        string service = DefaultService,
        DateTimeOffset? utcNow = null)
    {
        var verb = string.IsNullOrEmpty(method) ? "GET" : method!.ToUpperInvariant();
        var merged = Merge(headers, requestHeaders);
        string? text = body;
        if (text == null && binaryBody != null)
        {
            text = Encoding.UTF8.GetString(binaryBody);
        }
        else if (text == null && objectBody != null)
        {
            text = objectBody.ToJsonString();
        }

        merged["user-agent"] = UserAgent;
        if (verb != "POST" || string.IsNullOrEmpty(text))
        {
            return new AmazonBedrockPreparedCall(verb, url, text, merged, false);
        }

        Sign(url, text!, merged, credentials, service, utcNow ?? DateTimeOffset.UtcNow);
        return new AmazonBedrockPreparedCall(verb, url, text, merged, true);
    }

    /// <summary>
    /// Adds <c>Authorization: Bearer</c> and the user-agent without signing.
    /// The API key replaces any caller-supplied <c>Authorization</c>.
    /// </summary>
    public static AmazonBedrockPreparedCall PrepareApiKey(
        string apiKey,
        string url,
        string? method,
        string? body,
        IReadOnlyDictionary<string, string?>? headers)
    {
        var merged = new Dictionary<string, string>(StringComparer.Ordinal);
        Copy(merged, headers);
        merged["user-agent"] = UserAgent;
        merged["authorization"] = "Bearer " + (apiKey ?? string.Empty);
        return new AmazonBedrockPreparedCall(method ?? string.Empty, url, body, merged, false);
    }

    private static Dictionary<string, string> Merge(IReadOnlyDictionary<string, string?>? headers, IReadOnlyDictionary<string, string?>? requestHeaders)
    {
        var merged = new Dictionary<string, string>(StringComparer.Ordinal);
        Copy(merged, requestHeaders);
        Copy(merged, headers);
        return merged;
    }

    private static void Copy(Dictionary<string, string> target, IReadOnlyDictionary<string, string?>? headers)
    {
        if (headers == null)
        {
            return;
        }

        foreach (var pair in headers)
        {
            if (string.IsNullOrEmpty(pair.Key) || pair.Value == null || pair.Value.Length == 0)
            {
                continue;
            }

            target[pair.Key.ToLowerInvariant()] = pair.Value;
        }
    }

    private static void Sign(string url, string body, Dictionary<string, string> headers, AmazonBedrockCredentials credentials, string service, DateTimeOffset utcNow)
    {
        var uri = new Uri(url);
        headers["host"] = uri.IsDefaultPort ? uri.Host : uri.Host + ":" + uri.Port.ToString(CultureInfo.InvariantCulture);
        headers["x-amz-date"] = utcNow.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
        if (!string.IsNullOrEmpty(credentials.SessionToken))
        {
            headers["x-amz-security-token"] = credentials.SessionToken!;
        }

        headers["authorization"] = AwsSigV4.Authorize("POST", uri, headers, Encoding.UTF8.GetBytes(body), credentials.Region, service, credentials.AccessKeyId, credentials.SecretAccessKey, utcNow);
    }
}

/// <summary>
/// SigV4 fetch wrapper. The credentials are read when each call is sent.
/// </summary>
public sealed class AmazonBedrockSigV4Fetch
{
    private readonly Func<CancellationToken, Task<AmazonBedrockCredentials>> _getCredentials;
    private readonly string _service;

    /// <summary>Creates a wrapper that signs for <paramref name="service"/>.</summary>
    public AmazonBedrockSigV4Fetch(Func<CancellationToken, Task<AmazonBedrockCredentials>> getCredentials, string service = AmazonBedrockFetch.DefaultService)
    {
        _getCredentials = getCredentials ?? throw new ArgumentNullException(nameof(getCredentials));
        _service = service;
    }

    /// <summary>Optional transport. When unset, the prepared call is returned directly.</summary>
    public Func<AmazonBedrockPreparedCall, AmazonBedrockPreparedCall>? Transport { get; set; }

    /// <summary>Clock used for signing. Tests can pin this.</summary>
    public Func<DateTimeOffset>? UtcNow { get; set; }

    /// <summary>
    /// Resolves the credentials, signs the call, then invokes <see cref="Transport"/> if one is set.
    /// Calls that are not signed never read the credentials. A credential failure propagates and the transport is not called.
    /// </summary>
    public async Task<AmazonBedrockPreparedCall> SendAsync(
        string url,
        string? method,
        string? body,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken = default)
    {
        var verb = string.IsNullOrEmpty(method) ? "GET" : method!.ToUpperInvariant();
        var signs = verb == "POST" && !string.IsNullOrEmpty(body);
        var credentials = signs
            ? await _getCredentials(cancellationToken).ConfigureAwait(false)
            : new AmazonBedrockCredentials(string.Empty, string.Empty, string.Empty);
        var prepared = AmazonBedrockFetch.Prepare(url, verb, body, null, null, headers, null, credentials, _service, UtcNow?.Invoke());
        var transport = Transport;
        return transport == null ? prepared : transport(prepared);
    }
}

/// <summary>
/// API-key fetch wrapper. The transport is read when the call is sent, not when the wrapper is created.
/// </summary>
public sealed class AmazonBedrockApiKeyFetch
{
    private readonly string _apiKey;

    /// <summary>Creates a wrapper for <paramref name="apiKey"/>.</summary>
    public AmazonBedrockApiKeyFetch(string apiKey)
    {
        _apiKey = apiKey ?? string.Empty;
    }

    /// <summary>Optional transport. When unset, the prepared call is returned directly.</summary>
    public Func<AmazonBedrockPreparedCall, AmazonBedrockPreparedCall>? Transport { get; set; }

    /// <summary>Applies the API key and user-agent, then invokes <see cref="Transport"/> if one is set.</summary>
    public AmazonBedrockPreparedCall Send(string url, string? method, string? body, IReadOnlyDictionary<string, string?>? headers)
    {
        var prepared = AmazonBedrockFetch.PrepareApiKey(_apiKey, url, method, body, headers);
        var transport = Transport;
        return transport == null ? prepared : transport(prepared);
    }
}
