// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Vercel.AI.Sdk.AmazonBedrock;

/// <summary>Signs an HTTP request with AWS Signature Version 4.</summary>
public static class AwsSigV4
{
    /// <summary>Adds the Authorization header for <paramref name="service"/> in <paramref name="region"/>.</summary>
    public static void Sign(HttpRequestMessage request, byte[] payload, string region, string service, string accessKey, string secretKey, string? sessionToken, DateTimeOffset utcNow)
    {
        if (request.RequestUri is null)
        {
            throw new ArgumentException("Request URI is required.", nameof(request));
        }

        var amzDate = utcNow.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
        var dateStamp = utcNow.UtcDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var payloadHash = ToHex(Sha256(payload));
        request.Headers.TryAddWithoutValidation("x-amz-date", amzDate);
        request.Headers.TryAddWithoutValidation("host", request.RequestUri.Host);
        if (!string.IsNullOrEmpty(sessionToken))
        {
            request.Headers.TryAddWithoutValidation("x-amz-security-token", sessionToken);
        }

        var headerNames = new SortedDictionary<string, string>(StringComparer.Ordinal);
        headerNames["host"] = request.RequestUri.Host;
        headerNames["x-amz-date"] = amzDate;
        if (!string.IsNullOrEmpty(sessionToken))
        {
            headerNames["x-amz-security-token"] = sessionToken!;
        }

        var canonicalHeaders = new StringBuilder();
        var signed = new StringBuilder();
        foreach (var pair in headerNames)
        {
            canonicalHeaders.Append(pair.Key).Append(':').Append(pair.Value).Append('\n');
            if (signed.Length > 0)
            {
                signed.Append(';');
            }

            signed.Append(pair.Key);
        }

        var path = string.IsNullOrEmpty(request.RequestUri.AbsolutePath) ? "/" : request.RequestUri.AbsolutePath;
        var canonical = request.Method.Method + "\n" + path + "\n" + request.RequestUri.Query.TrimStart('?') + "\n"
            + canonicalHeaders + "\n" + signed + "\n" + payloadHash;
        var scope = dateStamp + "/" + region + "/" + service + "/aws4_request";
        var stringToSign = "AWS4-HMAC-SHA256\n" + amzDate + "\n" + scope + "\n" + ToHex(Sha256(Encoding.UTF8.GetBytes(canonical)));
        var signingKey = Hmac(Hmac(Hmac(Hmac(Encoding.UTF8.GetBytes("AWS4" + secretKey), dateStamp), region), service), "aws4_request");
        var signature = ToHex(Hmac(signingKey, stringToSign));
        request.Headers.TryAddWithoutValidation("Authorization", "AWS4-HMAC-SHA256 Credential=" + accessKey + "/" + scope + ", SignedHeaders=" + signed + ", Signature=" + signature);
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
