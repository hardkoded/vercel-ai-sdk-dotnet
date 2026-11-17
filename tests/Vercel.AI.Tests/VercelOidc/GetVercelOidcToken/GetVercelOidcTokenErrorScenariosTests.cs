// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Gateway.VercelOidc;
using Vercel.AI.Tests.VercelOidc;

namespace Vercel.AI.Tests.VercelOidc.GetVercelOidcToken;

/// <summary>Port of <c>get-vercel-oidc-token.test.ts</c> &gt; <c>getVercelOidcToken - Error Scenarios</c> (in <c>@vercel/oidc</c>).</summary>
public sealed class GetVercelOidcTokenErrorScenariosTests : VercelOidcTestBase
{
    private static Task<string> GetAsync(VercelOidcTokenOptions? options = null) =>
        VercelOidcToken.GetVercelOidcTokenAsync(options, CancellationToken.None);

    private void SetEnv(string? token) =>
        Environment.SetEnvironmentVariable("VERCEL_OIDC_TOKEN", token);

    private static string OidcJson(string token) => "{\"token\":\"" + token + "\"}";

    [Fact]
    public async Task Should_throw_AccessTokenMissingError_when_CLI_auth_file_is_missing()
    {
        WriteProjectJson("{\"projectId\":\"" + ProjectId + "\",\"orgId\":\"" + TeamId + "\"}");
        SetEnv(CreateExpiredToken());

        var error = await Assert.ThrowsAsync<VercelOidcTokenError>(() => GetAsync());

        Assert.Matches("No authentication found", error.Message);
    }

    [Fact]
    public async Task Should_throw_helpful_error_when_project_json_is_missing()
    {
        WriteAuthJson("{\"token\":\"test-auth-token\"}");
        SetEnv(CreateExpiredToken());

        var error = await Assert.ThrowsAsync<VercelOidcTokenError>(() => GetAsync());

        Assert.Contains(
            "project.json not found, have you linked your project with `vc link?`",
            error.Message
        );
    }

    [Fact]
    public async Task Should_throw_helpful_error_when_root_directory_cannot_be_found()
    {
        TokenIo.FindRootDirOverride = () => null;
        SetEnv(CreateExpiredToken());

        var error = await Assert.ThrowsAsync<VercelOidcTokenError>(() => GetAsync());

        Assert.Contains(
            "Unable to find project root directory. Have you linked your project with `vc link?`",
            error.Message
        );
    }

    [Fact]
    public async Task Should_throw_helpful_error_when_user_data_directory_cannot_be_found()
    {
        WriteProjectJson("{\"projectId\":\"" + ProjectId + "\",\"orgId\":\"" + TeamId + "\"}");
        TokenIo.GetUserDataDirOverride = () => null;
        SetEnv(CreateExpiredToken());

        var error = await Assert.ThrowsAsync<VercelOidcTokenError>(() => GetAsync());

        Assert.Contains(
            "Unable to find user data directory. Please reach out to Vercel support.",
            error.Message
        );
    }

    [Fact]
    public async Task Should_throw_helpful_error_when_API_returns_non_200()
    {
        WriteAuthJson("{\"token\":\"test-auth-token\"}");
        WriteProjectJson("{\"projectId\":\"" + ProjectId + "\",\"orgId\":\"" + TeamId + "\"}");
        Handler.OidcResponse = () =>
            new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "Unauthorized" };
        SetEnv(CreateExpiredToken());

        var error = await Assert.ThrowsAsync<VercelOidcTokenError>(() => GetAsync());

        Assert.Contains("Failed to refresh OIDC token: Unauthorized", error.Message);
    }

    [Fact]
    public async Task Should_throw_helpful_error_when_token_response_is_malformed()
    {
        WriteAuthJson("{\"token\":\"test-auth-token\"}");
        WriteProjectJson("{\"projectId\":\"" + ProjectId + "\",\"orgId\":\"" + TeamId + "\"}");
        Handler.OidcResponse = () => ScriptedHandler.Json("{\"token\":5}");
        SetEnv(CreateExpiredToken());

        var error = await Assert.ThrowsAsync<VercelOidcTokenError>(() => GetAsync());

        Assert.Contains(
            "Vercel OIDC token is malformed. Expected a string-valued token property. Please run `vc env pull` and try again",
            error.Message
        );
    }

    [Fact]
    public async Task Should_throw_helpful_error_when_token_has_invalid_format()
    {
        SetEnv("not-a-valid-jwt-token");

        var error = await Assert.ThrowsAsync<VercelOidcTokenError>(() => GetAsync());

        Assert.Contains("Invalid token. Please run `vc env pull` and try again", error.Message);
    }

    [Fact]
    public async Task Should_throw_error_when_token_expiry_check_fails()
    {
        SetEnv("not-a-jwt-token");

        var error = await Assert.ThrowsAsync<VercelOidcTokenError>(() => GetAsync());

        Assert.Contains("Invalid token. Please run `vc env pull` and try again", error.Message);
    }

    [Fact]
    public async Task Should_fail_when_no_token_exists_and_no_CLI_credentials_available()
    {
        // Upstream assigns undefined to process.env, which stores the string "undefined".
        SetEnv("undefined");

        var error = await Assert.ThrowsAsync<VercelOidcTokenError>(() => GetAsync());

        Assert.Contains("Invalid token", error.Message);
    }

    [Fact]
    public async Task Should_propagate_filesystem_errors_when_saving_token_fails()
    {
        WriteAuthJson("{\"token\":\"test-auth-token\"}");
        WriteProjectJson("{\"projectId\":\"" + ProjectId + "\",\"orgId\":\"" + TeamId + "\"}");
        Handler.OidcResponse = () => ScriptedHandler.Json(OidcJson("new-valid-token"));
        // A file where the token directory should be makes saving fail.
        Directory.Delete(TokenDataDir);
        File.WriteAllText(TokenDataDir, "blocked");
        SetEnv(CreateExpiredToken());

        var error = await Assert.ThrowsAsync<VercelOidcTokenError>(() => GetAsync());

        Assert.NotEmpty(error.Message);
    }

    [Fact]
    public async Task Should_succeed_when_valid_token_exists_in_env()
    {
        var validToken = CreateValidToken();
        SetEnv(validToken);

        Assert.Equal(validToken, await GetAsync());
    }

    [Fact]
    public async Task Should_refresh_when_token_is_expired_but_all_configs_are_valid()
    {
        WriteAuthJson("{\"token\":\"test-auth-token\"}");
        WriteProjectJson("{\"projectId\":\"" + ProjectId + "\",\"orgId\":\"" + TeamId + "\"}");
        var newToken = CreateValidToken("new-token");
        Handler.OidcResponse = () => ScriptedHandler.Json(OidcJson(newToken));
        SetEnv(CreateExpiredToken());

        Assert.Equal(newToken, await GetAsync());
    }

    [Fact]
    public async Task Should_use_provided_team_and_project_for_refresh_instead_of_reading_project_json()
    {
        WriteAuthJson("{\"token\":\"test-auth-token\"}");
        var newToken = CreateValidToken("custom-token");
        Handler.OidcResponse = () => ScriptedHandler.Json(OidcJson(newToken));
        SetEnv(CreateExpiredToken());

        var token = await GetAsync(
            new VercelOidcTokenOptions { Team = "team_custom456", Project = "prj_custom123" }
        );

        Assert.Equal(newToken, token);
        Assert.Equal(
            "https://api.vercel.com/v1/projects/prj_custom123/token?source=vercel-oidc-refresh&teamId=team_custom456",
            Handler.Requests.Last().RequestUri!.AbsoluteUri
        );
        Assert.Equal(
            "Bearer test-auth-token",
            Handler.Requests.Last().Headers.GetValues("Authorization").Single()
        );
    }

    [Fact]
    public async Task Should_use_provided_project_and_read_team_from_project_json()
    {
        WriteAuthJson("{\"token\":\"test-auth-token\"}");
        WriteProjectJson("{\"projectId\":\"original-project-id\",\"orgId\":\"" + TeamId + "\"}");
        Handler.OidcResponse = () =>
            ScriptedHandler.Json(OidcJson(CreateValidToken("partial-custom-token")));
        SetEnv(CreateExpiredToken());

        await GetAsync(new VercelOidcTokenOptions { Project = "prj_custom234" });

        Assert.Equal(
            "https://api.vercel.com/v1/projects/prj_custom234/token?source=vercel-oidc-refresh&teamId="
                + TeamId,
            Handler.Requests.Last().RequestUri!.AbsoluteUri
        );
    }

    [Fact]
    public async Task Should_use_provided_team_and_read_project_from_project_json()
    {
        WriteAuthJson("{\"token\":\"test-auth-token\"}");
        WriteProjectJson("{\"projectId\":\"" + ProjectId + "\",\"orgId\":\"original-team-id\"}");
        Handler.OidcResponse = () =>
            ScriptedHandler.Json(OidcJson(CreateValidToken("partial-custom-token-2")));
        SetEnv(CreateExpiredToken());

        await GetAsync(new VercelOidcTokenOptions { Team = "team_custom789" });

        Assert.Equal(
            "https://api.vercel.com/v1/projects/"
                + ProjectId
                + "/token?source=vercel-oidc-refresh&teamId=team_custom789",
            Handler.Requests.Last().RequestUri!.AbsoluteUri
        );
    }

    [Fact]
    public async Task Should_not_refresh_when_token_is_valid_even_with_options_provided()
    {
        var validToken = CreateValidToken();
        SetEnv(validToken);

        var token = await GetAsync(
            new VercelOidcTokenOptions { Team = "custom-team", Project = "custom-project" }
        );

        Assert.Equal(validToken, token);
        Assert.Empty(Handler.Requests);
    }
}
