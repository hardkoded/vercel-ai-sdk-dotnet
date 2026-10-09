// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Signs an HTTP request with AWS Signature Version 4.</summary>
public static class AwsSigV4
{
    // The transport can change these after signing, so they are never signed.
    private static readonly HashSet<string> UnsignedHeaders = new HashSet<string>(StringComparer.Ordinal)
    {
        "authorization",
        "connection",
        "expect",
        "x-amzn-trace-id",
    };

    /// <summary>
    /// Adds the Authorization header for <paramref name="service"/> in <paramref name="region"/>.
    /// The headers already on <paramref name="request"/> are signed too, except values with non-ASCII characters.
    /// </summary>
    public static void Sign(HttpRequestMessage request, byte[] payload, string region, string service, string accessKey, string secretKey, string? sessionToken, DateTimeOffset utcNow)
    {
        if (request.RequestUri is null)
        {
            throw new ArgumentException("Request URI is required.", nameof(request));
        }

        var amzDate = utcNow.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var header in request.Headers)
        {
            var name = header.Key.ToLowerInvariant();
            if (!UnsignedHeaders.Contains(name))
            {
                headers[name] = string.Join(", ", header.Value);
            }
        }

        headers["host"] = request.RequestUri.Host;
        headers["x-amz-date"] = amzDate;
        if (!string.IsNullOrEmpty(sessionToken))
        {
            headers["x-amz-security-token"] = sessionToken!;
        }

        request.Headers.TryAddWithoutValidation("x-amz-date", amzDate);
        request.Headers.TryAddWithoutValidation("host", request.RequestUri.Host);
        if (!string.IsNullOrEmpty(sessionToken))
        {
            request.Headers.TryAddWithoutValidation("x-amz-security-token", sessionToken);
        }

        request.Headers.TryAddWithoutValidation("Authorization", Authorize(request.Method.Method, request.RequestUri, headers, payload, region, service, accessKey, secretKey, utcNow));
    }

    /// <summary>
    /// Returns the <c>Authorization</c> value for <paramref name="headers"/>, which holds lower-case names
    /// and includes <c>host</c> and <c>x-amz-date</c>. A header whose value has a non-ASCII character is not signed.
    /// </summary>
    internal static string Authorize(string method, Uri uri, IReadOnlyDictionary<string, string> headers, byte[] payload, string region, string service, string accessKey, string secretKey, DateTimeOffset utcNow)
    {
        var amzDate = utcNow.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
        var dateStamp = utcNow.UtcDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var payloadHash = ToHex(Sha256(payload));
        var names = new List<string>(headers.Keys);
        names.Sort(StringComparer.Ordinal);
        var canonicalHeaders = new StringBuilder();
        var signed = new StringBuilder();
        foreach (var name in names)
        {
            if (!IsAscii(headers[name]))
            {
                continue;
            }

            canonicalHeaders.Append(name).Append(':').Append(headers[name].Trim()).Append('\n');
            if (signed.Length > 0)
            {
                signed.Append(';');
            }

            signed.Append(name);
        }

        var path = string.IsNullOrEmpty(uri.AbsolutePath) ? "/" : uri.AbsolutePath;
        var canonical = method + "\n" + path + "\n" + uri.Query.TrimStart('?') + "\n"
            + canonicalHeaders + "\n" + signed + "\n" + payloadHash;
        var scope = dateStamp + "/" + region + "/" + service + "/aws4_request";
        var stringToSign = "AWS4-HMAC-SHA256\n" + amzDate + "\n" + scope + "\n" + ToHex(Sha256(Encoding.UTF8.GetBytes(canonical)));
        var signingKey = Hmac(Hmac(Hmac(Hmac(Encoding.UTF8.GetBytes("AWS4" + secretKey), dateStamp), region), service), "aws4_request");
        var signature = ToHex(Hmac(signingKey, stringToSign));
        return "AWS4-HMAC-SHA256 Credential=" + accessKey + "/" + scope + ", SignedHeaders=" + signed + ", Signature=" + signature;
    }

    // SigV4 header values must be ASCII. Other values are sent but left out of the signature.
    private static bool IsAscii(string value)
    {
        foreach (var c in value)
        {
            if (c > 0x7F)
            {
                return false;
            }
        }

        return true;
    }

    private static byte[] Sha256(byte[] data)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(data);
    }

    private static byte[] Hmac(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static string ToHex(byte[] data)
    {
        var builder = new StringBuilder(data.Length * 2);
        foreach (var value in data)
        {
            builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}
