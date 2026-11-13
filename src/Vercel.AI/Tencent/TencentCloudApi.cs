// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tencent;

/// <summary>Credentials for Tencent Cloud API 3.0. Upstream <c>TencentCloudCredentials</c>.</summary>
public class TencentCloudCredentials
{
    /// <summary>API SecretId.</summary>
    public string SecretId { get; set; } = string.Empty;

    /// <summary>API SecretKey.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Session token for temporary credentials, sent as <c>X-TC-Token</c>.</summary>
    public string? SessionToken { get; set; }
}

/// <summary>Credentials and application id for VOD. Upstream <c>TencentVodCredentials</c>.</summary>
public sealed class TencentVodCredentials : TencentCloudCredentials
{
    /// <summary>VOD application id (<c>SubAppId</c>).</summary>
    public long SubAppId { get; set; }
}

/// <summary>Tencent Cloud API 3.0 client with TC3-HMAC-SHA256 signing. Upstream <c>tencent-cloud-api.ts</c>.</summary>
public static class TencentCloudApi
{
    private const string Tc3Algorithm = "TC3-HMAC-SHA256";
    private const string Tc3ContentType = "application/json; charset=utf-8";

    /// <summary>Hex SHA-256 of <paramref name="value"/>.</summary>
    public static string Sha256Hex(string value)
    {
        using var sha = SHA256.Create();
        return ToHex(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    /// <summary>
    /// Builds the TC3 canonical request for a POST to <c>/</c> with an empty query.
    /// Header names are lowercased and sorted.
    /// </summary>
    public static (string CanonicalRequest, string SignedHeaders) BuildTc3CanonicalRequest(string payload, IReadOnlyDictionary<string, string> headers)
    {
        var entries = headers
            .Select(pair => (Name: pair.Key.ToLowerInvariant(), Value: pair.Value.Trim()))
            .OrderBy(entry => entry.Name, StringComparer.Ordinal)
            .ToList();
        var canonicalHeaders = string.Concat(entries.Select(entry => entry.Name + ":" + entry.Value + "\n"));
        var signedHeaders = string.Join(";", entries.Select(entry => entry.Name));
        var canonicalRequest = string.Join("\n", "POST", "/", string.Empty, canonicalHeaders, signedHeaders, Sha256Hex(payload));
        return (canonicalRequest, signedHeaders);
    }

    /// <summary>
    /// Computes the <c>Authorization</c> header value for a Tencent Cloud API 3.0 request signed with TC3-HMAC-SHA256.
    /// <paramref name="timestamp"/> is a Unix time in seconds and must match the <c>X-TC-Timestamp</c> header.
    /// </summary>
    public static string CreateTc3Authorization(string secretId, string secretKey, string service, long timestamp, string payload, IReadOnlyDictionary<string, string> headers)
    {
        var date = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var credentialScope = date + "/" + service + "/tc3_request";
        var (canonicalRequest, signedHeaders) = BuildTc3CanonicalRequest(payload, headers);
        var stringToSign = string.Join("\n", Tc3Algorithm, timestamp.ToString(CultureInfo.InvariantCulture), credentialScope, Sha256Hex(canonicalRequest));
        var secretDate = HmacSha256(Encoding.UTF8.GetBytes("TC3" + secretKey), date);
        var secretService = HmacSha256(secretDate, service);
        var secretSigning = HmacSha256(secretService, "tc3_request");
        var signature = ToHex(HmacSha256(secretSigning, stringToSign));
        return Tc3Algorithm + " Credential=" + secretId + "/" + credentialScope + ", SignedHeaders=" + signedHeaders + ", Signature=" + signature;
    }

    /// <summary>
    /// Tencent Cloud API 3.0 reports failures with HTTP 200 and a <c>Response.Error</c> code.
    /// Maps the code family to the HTTP status it stands for so retry logic sees the real failure class.
    /// </summary>
    public static int GetTencentCloudErrorStatusCode(string code)
    {
        var family = code.Split('.')[0];
        switch (family)
        {
            case "AuthFailure":
                return 401;
            case "UnauthorizedOperation":
                return 403;
            case "ResourceNotFound":
            case "InvalidAction":
            case "NoSuchProduct":
            case "NoSuchVersion":
                return 404;
            case "LimitExceeded":
            case "RequestLimitExceeded":
                return 429;
            case "InvalidParameter":
            case "InvalidParameterValue":
            case "InvalidRequest":
            case "MissingParameter":
            case "RequestSizeLimitExceeded":
            case "UnknownParameter":
            case "UnsupportedOperation":
            case "UnsupportedProtocol":
            case "UnsupportedRegion":
                return 400;
            case "ResourceInsufficient":
            case "ResourceUnavailable":
            case "ServiceUnavailable":
                return 503;
            case "FailedOperation":
                return code == "FailedOperation.InvalidVodUser" ? 403 : 500;
            default:
                return 500;
        }
    }

    /// <summary>
    /// Calls a Tencent Cloud API 3.0 action with a TC3-signed JSON POST and returns the validated <c>Response</c> object.
    /// </summary>
    internal static async Task<TencentCloudResponse> PostTencentCloudApiAsync(
        HttpClient httpClient,
        string url,
        string service,
        string action,
        string version,
        string? region,
        TencentCloudCredentials credentials,
        System.Text.Json.Nodes.JsonObject body,
        IReadOnlyDictionary<string, string?>? headers,
        long timestamp,
        CancellationToken cancellationToken)
    {
        var payload = body.ToJsonString();
        var authorization = CreateTc3Authorization(
            credentials.SecretId,
            credentials.SecretKey,
            service,
            timestamp,
            payload,
            new Dictionary<string, string>
            {
                ["content-type"] = Tc3ContentType,
                ["host"] = new Uri(url).Authority,
                ["x-tc-action"] = action.ToLowerInvariant(),
            });
        var protocolHeaders = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["authorization"] = authorization,
            ["x-tc-action"] = action,
            ["x-tc-timestamp"] = timestamp.ToString(CultureInfo.InvariantCulture),
            ["x-tc-version"] = version,
        };
        if (region != null)
        {
            protocolHeaders["x-tc-region"] = region;
        }

        if (credentials.SessionToken != null)
        {
            protocolHeaders["x-tc-token"] = credentials.SessionToken;
        }

        // Caller headers must not shadow the signed protocol headers under a different casing,
        // or the request would carry two conflicting values.
        var requestHeaders = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in headers ?? new Dictionary<string, string?>())
        {
            if (!protocolHeaders.ContainsKey(pair.Key) && !string.Equals(pair.Key, "content-type", StringComparison.OrdinalIgnoreCase))
            {
                requestHeaders[pair.Key] = pair.Value;
            }
        }

        foreach (var pair in protocolHeaders)
        {
            requestHeaders[pair.Key] = pair.Value;
        }

        var content = new StringContent(payload, Encoding.UTF8);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse(Tc3ContentType);
        ProviderExchangeResult exchange;
        try
        {
            exchange = await ProviderExchange.SendAsync(httpClient, HttpMethod.Post, new Uri(url), content, requestHeaders, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception) when (ReadError(exception.ResponseBody) is { } error)
        {
            throw ProviderHttp.MapStatusWithHeaders(exception.StatusCode, error, exception.ResponseBody, exception.ResponseHeaders);
        }

        return ReadResponse(exchange, url, body);
    }

    private static TencentCloudResponse ReadResponse(ProviderExchangeResult exchange, string url, System.Text.Json.Nodes.JsonObject requestBody)
    {
        JsonElement response;
        try
        {
            using var document = JsonDocument.Parse(exchange.Body);
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty("Response", out var inner)
                || inner.ValueKind != JsonValueKind.Object)
            {
                throw Invalid(exchange, url, requestBody);
            }

            response = inner.Clone();
        }
        catch (JsonException)
        {
            throw Invalid(exchange, url, requestBody);
        }

        if (response.TryGetProperty("Error", out var error) && error.ValueKind == JsonValueKind.Object)
        {
            var code = error.TryGetProperty("Code", out var codeValue) && codeValue.ValueKind == JsonValueKind.String ? codeValue.GetString() : null;
            var message = error.TryGetProperty("Message", out var messageValue) && messageValue.ValueKind == JsonValueKind.String ? messageValue.GetString() : null;
            if (code == null || message == null)
            {
                throw Invalid(exchange, url, requestBody);
            }

            throw ProviderHttp.MapStatusWithHeaders(GetTencentCloudErrorStatusCode(code), code + ": " + message, exchange.Body, exchange.Headers);
        }

        return new TencentCloudResponse(response, exchange.Headers);
    }

    private static ApiException Invalid(ProviderExchangeResult exchange, string url, System.Text.Json.Nodes.JsonObject requestBody)
    {
        return new ApiException("Invalid JSON response", 200, exchange.Body) { Url = url, RequestBodyValues = requestBody, ResponseHeaders = exchange.Headers };
    }

    // A failed HTTP response carries { Response: { Error: { Code, Message } } }.
    private static string? ReadError(string? body)
    {
        try
        {
            using var document = JsonDocument.Parse(body ?? string.Empty);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("Response", out var response) && response.ValueKind == JsonValueKind.Object
                && response.TryGetProperty("Error", out var error) && error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("Code", out var code) && code.ValueKind == JsonValueKind.String
                && error.TryGetProperty("Message", out var message) && message.ValueKind == JsonValueKind.String)
            {
                return code.GetString() + ": " + message.GetString();
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    private static byte[] HmacSha256(byte[] key, string value)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
    }

    private static string ToHex(byte[] bytes)
    {
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (var value in bytes)
        {
            builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}

/// <summary>The <c>Response</c> object of a Tencent Cloud API 3.0 call.</summary>
internal sealed class TencentCloudResponse
{
    internal TencentCloudResponse(JsonElement value, IReadOnlyDictionary<string, string> headers)
    {
        Value = value;
        Headers = headers;
    }

    internal JsonElement Value { get; }

    internal IReadOnlyDictionary<string, string> Headers { get; }
}
