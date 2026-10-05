// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

/// <summary>AWS credentials used to sign Claude Platform requests.</summary>
public sealed class AnthropicAwsCredentials
{
    /// <summary>Creates credentials.</summary>
    public AnthropicAwsCredentials(string region, string accessKeyId, string secretAccessKey, string? sessionToken = null)
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

/// <summary>A request after the Claude Platform fetch wrapper has applied user-agent and SigV4.</summary>
public sealed class AnthropicAwsPreparedCall
{
    /// <summary>Creates a prepared call.</summary>
    public AnthropicAwsPreparedCall(string method, string url, string? body, IReadOnlyDictionary<string, string> headers, bool signed)
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
/// Claude Platform on AWS fetch behavior. GET and POST requests without a body skip signing.
/// POST bodies are signed for service <c>aws-external-anthropic</c>.
/// </summary>
public static class AnthropicAwsFetch
{
    /// <summary>User agent sent on every AWS request. The runtime segment is dotnet.</summary>
    public const string UserAgent = "ai-sdk/anthropic-aws/0.0.0-test runtime/dotnet";

    /// <summary>SigV4 service name.</summary>
    public const string Service = "aws-external-anthropic";

    /// <summary>
    /// Prepares a call. Non-POST requests and POST requests without a body are not signed.
    /// String bodies are left unchanged. Byte bodies are decoded as UTF-8. Other objects are JSON.
    /// </summary>
    public static AnthropicAwsPreparedCall Prepare(
        string url,
        string? method,
        string? body,
        byte[]? binaryBody,
        JsonNode? objectBody,
        IReadOnlyDictionary<string, string?>? headers,
        IReadOnlyDictionary<string, string?>? requestHeaders,
        AnthropicAwsCredentials credentials,
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
            return new AnthropicAwsPreparedCall(verb, url, text, merged, false);
        }

        Sign(url, verb, text, merged, credentials, utcNow ?? DateTimeOffset.UtcNow);
        return new AnthropicAwsPreparedCall(verb, url, text, merged, true);
    }

    /// <summary>
    /// Adds <c>x-api-key</c> and the AWS user-agent without signing.
    /// The API key replaces any caller-supplied <c>x-api-key</c>.
    /// </summary>
    public static AnthropicAwsPreparedCall PrepareApiKey(
        string apiKey,
        string url,
        string? method,
        string? body,
        IReadOnlyDictionary<string, string?>? headers)
    {
        var merged = new Dictionary<string, string>(StringComparer.Ordinal);
        Copy(merged, headers);
        merged["user-agent"] = UserAgent;
        merged["x-api-key"] = apiKey ?? string.Empty;
        return new AnthropicAwsPreparedCall(method ?? string.Empty, url, body, merged, false);
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

    private static void Sign(string url, string method, string body, Dictionary<string, string> headers, AnthropicAwsCredentials credentials, DateTimeOffset utcNow)
    {
        var uri = new Uri(url);
        var amzDate = utcNow.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
        var dateStamp = utcNow.UtcDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        headers["host"] = uri.IsDefaultPort ? uri.Host : uri.Host + ":" + uri.Port.ToString(CultureInfo.InvariantCulture);
        headers["x-amz-date"] = amzDate;
        if (!string.IsNullOrEmpty(credentials.SessionToken))
        {
            headers["x-amz-security-token"] = credentials.SessionToken!;
        }

        var payloadHash = Hex(Sha256(Encoding.UTF8.GetBytes(body)));
        var names = new List<string>(headers.Keys);
        names.Sort(StringComparer.Ordinal);
        var canonicalHeaders = new StringBuilder();
        var signed = new StringBuilder();
        foreach (var name in names)
        {
            canonicalHeaders.Append(name).Append(':').Append(headers[name].Trim()).Append('\n');
            if (signed.Length > 0)
            {
                signed.Append(';');
            }

            signed.Append(name);
        }

        var path = string.IsNullOrEmpty(uri.AbsolutePath) ? "/" : uri.AbsolutePath;
        var canonical = method + "\n" + path + "\n" + uri.Query.TrimStart('?') + "\n" + canonicalHeaders + "\n" + signed + "\n" + payloadHash;
        var scope = dateStamp + "/" + credentials.Region + "/" + Service + "/aws4_request";
        var stringToSign = "AWS4-HMAC-SHA256\n" + amzDate + "\n" + scope + "\n" + Hex(Sha256(Encoding.UTF8.GetBytes(canonical)));
        var key = Hmac(Hmac(Hmac(Hmac(Encoding.UTF8.GetBytes("AWS4" + credentials.SecretAccessKey), dateStamp), credentials.Region), Service), "aws4_request");
        var signature = Hex(Hmac(key, stringToSign));
        headers["authorization"] = "AWS4-HMAC-SHA256 Credential=" + credentials.AccessKeyId + "/" + scope + ", SignedHeaders=" + signed + ", Signature=" + signature;
    }

    private static byte[] Sha256(byte[] data)
    {
        using (var sha = SHA256.Create())
        {
            return sha.ComputeHash(data);
        }
    }

    private static byte[] Hmac(byte[] key, string data)
    {
        using (var hmac = new HMACSHA256(key))
        {
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }

    private static string Hex(byte[] data)
    {
        var builder = new StringBuilder(data.Length * 2);
        foreach (var value in data)
        {
            builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}

/// <summary>
/// API-key fetch wrapper. The transport is read when the call is sent, not when the wrapper is created.
/// </summary>
public sealed class AnthropicAwsApiKeyFetch
{
    private readonly string _apiKey;

    /// <summary>Creates a wrapper for <paramref name="apiKey"/>.</summary>
    public AnthropicAwsApiKeyFetch(string apiKey)
    {
        _apiKey = apiKey ?? string.Empty;
    }

    /// <summary>Optional transport. When unset, the prepared call is returned directly.</summary>
    public Func<AnthropicAwsPreparedCall, AnthropicAwsPreparedCall>? Transport { get; set; }

    /// <summary>Applies the API key and user-agent, then invokes <see cref="Transport"/> if one is set.</summary>
    public AnthropicAwsPreparedCall Send(string url, string? method, string? body, IReadOnlyDictionary<string, string?>? headers)
    {
        var prepared = AnthropicAwsFetch.PrepareApiKey(_apiKey, url, method, body, headers);
        var transport = Transport;
        return transport == null ? prepared : transport(prepared);
    }
}
