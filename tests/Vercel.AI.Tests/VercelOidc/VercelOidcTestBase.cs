// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Gateway.VercelOidc;

namespace Vercel.AI.Tests.VercelOidc;

/// <summary>Shared setup for the <c>@vercel/oidc</c> tests: a temp project root, a temp user data directory and a scripted HTTP handler.</summary>
[Collection("VercelOidcEnvironment")]
public abstract class VercelOidcTestBase : IDisposable
{
    protected const string ProjectId = "prj_test123";
    protected const string TeamId = "team_test456";

    private readonly string? _previousToken;

    protected VercelOidcTestBase()
    {
        _previousToken = Environment.GetEnvironmentVariable(
            VercelEnvironment.OidcTokenEnvironmentVariable
        );
        Environment.SetEnvironmentVariable(VercelEnvironment.OidcTokenEnvironmentVariable, null);
        RootDir = Path.Combine(Path.GetTempPath(), "test-" + Guid.NewGuid());
        UserDataDir = Path.Combine(RootDir, "data");
        CliDataDir = Path.Combine(UserDataDir, "com.vercel.cli");
        TokenDataDir = Path.Combine(UserDataDir, "com.vercel.token");
        Directory.CreateDirectory(CliDataDir);
        Directory.CreateDirectory(TokenDataDir);
        Directory.CreateDirectory(Path.Combine(RootDir, ".vercel"));
        TokenIo.FindRootDirOverride = () => RootDir;
        TokenIo.GetUserDataDirOverride = () => UserDataDir;
        Handler = new ScriptedHandler();
        OAuth.HandlerOverride = Handler;
        OAuth.ResetTokenEndpointCache();
    }

    protected string RootDir { get; }

    protected string UserDataDir { get; }

    protected string CliDataDir { get; }

    protected string TokenDataDir { get; }

    protected ScriptedHandler Handler { get; }

    public void Dispose()
    {
        TokenIo.FindRootDirOverride = null;
        TokenIo.GetUserDataDirOverride = null;
        OAuth.HandlerOverride = null;
        OAuth.ResetTokenEndpointCache();
        Environment.SetEnvironmentVariable(
            VercelEnvironment.OidcTokenEnvironmentVariable,
            _previousToken
        );
        if (Directory.Exists(RootDir))
        {
            Directory.Delete(RootDir, recursive: true);
        }
    }

    protected void WriteAuthJson(string json) =>
        File.WriteAllText(Path.Combine(CliDataDir, "auth.json"), json);

    protected void WriteProjectJson(string json) =>
        File.WriteAllText(Path.Combine(RootDir, ".vercel", "project.json"), json);

    protected static string CreateExpiredToken() => CreateToken(1, "fake_signature");

    protected static string CreateValidToken(string value = "valid-token") =>
        CreateToken(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 43200, "fake_signature_" + value);

    protected static string CreateToken(long exp, string signature)
    {
        var header = Base64Url("{\"alg\":\"RS256\",\"typ\":\"JWT\"}");
        var payload = Base64Url("{\"sub\":\"test-sub\",\"exp\":" + exp + ",\"iat\":" + exp + "}");
        return header + "." + payload + "." + signature;
    }

    private static string Base64Url(string text) =>
        Convert
            .ToBase64String(Encoding.UTF8.GetBytes(text))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    /// <summary>Answers each request from a script and records it.</summary>
    protected sealed class ScriptedHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        public List<string> Bodies { get; } = [];

        /// <summary>Response for the OIDC refresh call to <c>api.vercel.com</c>.</summary>
        public Func<HttpResponseMessage>? OidcResponse { get; set; }

        /// <summary>Response for the OAuth token endpoint.</summary>
        public Func<HttpResponseMessage>? TokenEndpointResponse { get; set; }

        public static HttpResponseMessage Json(
            string json,
            HttpStatusCode status = HttpStatusCode.OK
        ) => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Requests.Add(request);
            Bodies.Add(
                request.Content is null
                    ? ""
                    : await request.Content.ReadAsStringAsync().ConfigureAwait(false)
            );
            var url = request.RequestUri!.AbsoluteUri;
            if (url == "https://vercel.com/.well-known/openid-configuration")
            {
                return Json("{\"token_endpoint\":\"https://vercel.com/oauth/token\"}");
            }

            if (url == "https://vercel.com/oauth/token" && TokenEndpointResponse is not null)
            {
                return TokenEndpointResponse();
            }

            if (
                url.StartsWith("https://api.vercel.com/v1/projects/", StringComparison.Ordinal)
                && OidcResponse is not null
            )
            {
                return OidcResponse();
            }

            throw new HttpRequestException("Unexpected request: " + url);
        }
    }
}
