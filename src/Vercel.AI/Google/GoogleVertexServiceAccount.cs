// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>Google Cloud service account credentials.</summary>
public sealed class GoogleCredentials
{
    /// <summary>Service account email. Falls back to <c>GOOGLE_CLIENT_EMAIL</c>.</summary>
    public string ClientEmail { get; set; } = string.Empty;

    /// <summary>PKCS #8 RSA private key in PEM form. Falls back to <c>GOOGLE_PRIVATE_KEY</c>.</summary>
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>Optional key id, sent as the JWT <c>kid</c>. Falls back to <c>GOOGLE_PRIVATE_KEY_ID</c>.</summary>
    public string? PrivateKeyId { get; set; }
}

/// <summary>Exchanges a signed service account JWT for a Google OAuth access token.</summary>
public static class GoogleVertexServiceAccount
{
    /// <summary>OAuth token endpoint. It is also the JWT audience.</summary>
    public const string TokenUrl = "https://oauth2.googleapis.com/token";

    /// <summary>User agent sent to the token endpoint.</summary>
    public const string UserAgent = "ai-sdk/google-vertex/0.0.0-test runtime/dotnet";

    private const string Scope = "https://www.googleapis.com/auth/cloud-platform";

    /// <summary>
    /// Returns an access token. Without <paramref name="credentials"/>, they are read from
    /// <c>GOOGLE_CLIENT_EMAIL</c>, <c>GOOGLE_PRIVATE_KEY</c>, and <c>GOOGLE_PRIVATE_KEY_ID</c>.
    /// </summary>
    public static async Task<string> GenerateAuthTokenAsync(HttpClient httpClient, GoogleCredentials? credentials, CancellationToken cancellationToken)
    {
        var jwt = BuildJwt(credentials ?? LoadCredentials(), DateTimeOffset.UtcNow);
        using var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["assertion"] = jwt,
            }),
        };
        request.Headers.TryAddWithoutValidation("user-agent", UserAgent);
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new AiSdkException("Token request failed: " + response.ReasonPhrase);
        }

        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("access_token").GetString()!;
    }

    /// <summary>RS256 JWT that is valid for one hour from <paramref name="now"/>.</summary>
    internal static string BuildJwt(GoogleCredentials credentials, DateTimeOffset now)
    {
        var header = new JsonObject { ["alg"] = "RS256", ["typ"] = "JWT" };
        if (!string.IsNullOrEmpty(credentials.PrivateKeyId))
        {
            header["kid"] = credentials.PrivateKeyId;
        }

        var issuedAt = now.ToUnixTimeSeconds();
        var payload = new JsonObject
        {
            ["iss"] = credentials.ClientEmail,
            ["scope"] = Scope,
            ["aud"] = TokenUrl,
            ["exp"] = issuedAt + 3600,
            ["iat"] = issuedAt,
        };
        var signingInput = Base64Url(Encoding.UTF8.GetBytes(header.ToJsonString())) + "." + Base64Url(Encoding.UTF8.GetBytes(payload.ToJsonString()));
        using var rsa = RSA.Create();
        rsa.ImportParameters(ReadPkcs8(credentials.PrivateKey));
        var signature = rsa.SignData(Encoding.ASCII.GetBytes(signingInput), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return signingInput + "." + Base64Url(signature);
    }

    private static GoogleCredentials LoadCredentials()
    {
        return new GoogleCredentials
        {
            ClientEmail = Require("GOOGLE_CLIENT_EMAIL", "Google client email", "clientEmail"),
            PrivateKey = Require("GOOGLE_PRIVATE_KEY", "Google private key", "privateKey"),
            PrivateKeyId = Environment.GetEnvironmentVariable("GOOGLE_PRIVATE_KEY_ID"),
        };
    }

    private static string Require(string variable, string description, string parameter)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrEmpty(value))
        {
            throw new AiSdkException(
                "Failed to load Google credentials: " + description + " setting is missing. Pass it using the '" + parameter + "' parameter or the " + variable + " environment variable.");
        }

        return value!;
    }

    private static string Base64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>
    /// Reads a PEM PKCS #8 RSA key. Escaped <c>\n</c> sequences from environment variables are treated as line breaks.
    /// Parsed by hand because netstandard2.0 has no PKCS #8 import.
    /// </summary>
    private static RSAParameters ReadPkcs8(string pem)
    {
        var base64 = new StringBuilder();
        foreach (var line in pem.Replace("\\n", "\n").Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 0 && !trimmed.StartsWith("-----", StringComparison.Ordinal))
            {
                base64.Append(trimmed);
            }
        }

        try
        {
            var der = new DerReader(Convert.FromBase64String(base64.ToString()));
            var info = der.Sequence();
            info.Integer();
            info.Sequence();
            var key = new DerReader(info.Read(0x04)).Sequence();
            key.Integer();
            var modulus = key.Integer();
            var exponent = key.Integer();
            var half = (modulus.Length + 1) / 2;
            return new RSAParameters
            {
                Modulus = modulus,
                Exponent = exponent,
                D = Pad(key.Integer(), modulus.Length),
                P = Pad(key.Integer(), half),
                Q = Pad(key.Integer(), half),
                DP = Pad(key.Integer(), half),
                DQ = Pad(key.Integer(), half),
                InverseQ = Pad(key.Integer(), half),
            };
        }
        catch (Exception exception) when (exception is FormatException || exception is IndexOutOfRangeException || exception is ArgumentException)
        {
            throw new AiSdkException("Google private key is not a PEM PKCS #8 RSA key.", exception);
        }
    }

    private static byte[] Pad(byte[] value, int length)
    {
        if (value.Length >= length)
        {
            return value;
        }

        var padded = new byte[length];
        Buffer.BlockCopy(value, 0, padded, length - value.Length, value.Length);
        return padded;
    }

    private sealed class DerReader
    {
        private readonly byte[] _data;
        private int _offset;

        public DerReader(byte[] data)
        {
            _data = data;
        }

        public DerReader Sequence()
        {
            return new DerReader(Read(0x30));
        }

        /// <summary>Unsigned big-endian integer without the sign byte.</summary>
        public byte[] Integer()
        {
            var value = Read(0x02);
            var start = 0;
            while (start < value.Length - 1 && value[start] == 0)
            {
                start++;
            }

            var result = new byte[value.Length - start];
            Buffer.BlockCopy(value, start, result, 0, result.Length);
            return result;
        }

        public byte[] Read(byte tag)
        {
            if (_data[_offset++] != tag)
            {
                throw new FormatException("Unexpected DER tag.");
            }

            int length = _data[_offset++];
            if (length > 0x80)
            {
                var count = length & 0x7F;
                length = 0;
                for (var i = 0; i < count; i++)
                {
                    length = (length << 8) | _data[_offset++];
                }
            }

            var value = new byte[length];
            Buffer.BlockCopy(_data, _offset, value, 0, length);
            _offset += length;
            return value;
        }
    }
}
