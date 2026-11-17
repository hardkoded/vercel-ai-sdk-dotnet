// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway.VercelOidc;
using Vercel.AI.Tests.VercelOidc;
using GatewayTokenUtil = Vercel.AI.Gateway.VercelOidc.TokenUtil;

namespace Vercel.AI.Tests.VercelOidc.TokenUtil;

/// <summary>Port of <c>token-util.test.ts</c> &gt; <c>getVercelToken</c> (in <c>@vercel/oidc</c>).</summary>
public sealed class GetVercelTokenTests : VercelOidcTestBase
{
    private static long Now => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private string AuthPath => Path.Combine(CliDataDir, "auth.json");

    private static Task<string> GetAsync() =>
        GatewayTokenUtil.GetVercelTokenAsync(null, CancellationToken.None);

    private static HttpResponseMessage TokenResponse(string refreshToken = "") =>
        ScriptedHandler.Json(
            "{\"access_token\":\"new-access-token\",\"token_type\":\"Bearer\",\"expires_in\":3600"
                + refreshToken
                + "}"
        );

    private JsonObject WrittenAuth() => (JsonObject)JsonNode.Parse(File.ReadAllText(AuthPath))!;

    [Fact]
    public async Task Should_return_token_if_valid_and_not_expired()
    {
        var json =
            "{\"token\":\"valid-access-token\",\"refreshToken\":\"refresh-token\",\"expiresAt\":"
            + (Now + 3600)
            + "}";
        WriteAuthJson(json);

        Assert.Equal("valid-access-token", await GetAsync());
        Assert.Equal(json, File.ReadAllText(AuthPath));
    }

    [Fact]
    public async Task Should_throw_AccessTokenMissingError_if_auth_config_does_not_exist()
    {
        await Assert.ThrowsAsync<AccessTokenMissingError>(() => GetAsync());
    }

    [Fact]
    public async Task Should_refresh_token_if_expired_and_refresh_token_exists()
    {
        WriteAuthJson(
            "{\"token\":\"expired-access-token\",\"refreshToken\":\"valid-refresh-token\",\"expiresAt\":"
                + (Now - 3600)
                + "}"
        );
        Handler.TokenEndpointResponse = () =>
            TokenResponse(",\"refresh_token\":\"new-refresh-token\"");

        Assert.Equal("new-access-token", await GetAsync());

        Assert.Contains("refresh_token=valid-refresh-token", Handler.Bodies.Last());
        Assert.Contains("grant_type=refresh_token", Handler.Bodies.Last());
        var written = WrittenAuth();
        Assert.Equal("new-access-token", (string?)written["token"]);
        Assert.Equal("new-refresh-token", (string?)written["refreshToken"]);
        Assert.NotNull(written["expiresAt"]);
    }

    [Fact]
    public async Task Should_clear_auth_and_throw_RefreshAccessTokenFailedError_if_token_expired_and_no_refresh_token()
    {
        WriteAuthJson("{\"token\":\"expired-access-token\",\"expiresAt\":" + (Now - 3600) + "}");

        await Assert.ThrowsAsync<RefreshAccessTokenFailedError>(() => GetAsync());

        Assert.Equal("{}", File.ReadAllText(AuthPath));
    }

    [Fact]
    public async Task Should_clear_auth_and_throw_RefreshAccessTokenFailedError_if_refresh_fails_with_OAuth_error()
    {
        WriteAuthJson(
            "{\"token\":\"expired-access-token\",\"refreshToken\":\"invalid-refresh-token\",\"expiresAt\":"
                + (Now - 3600)
                + "}"
        );
        Handler.TokenEndpointResponse = () =>
            ScriptedHandler.Json(
                "{\"error\":\"invalid_grant\",\"error_description\":\"Refresh token expired\"}",
                HttpStatusCode.BadRequest
            );

        await Assert.ThrowsAsync<RefreshAccessTokenFailedError>(() => GetAsync());

        Assert.Equal("{}", File.ReadAllText(AuthPath));
    }

    [Fact]
    public async Task Should_clear_auth_and_throw_RefreshAccessTokenFailedError_if_refresh_fails_with_network_error()
    {
        WriteAuthJson(
            "{\"token\":\"expired-access-token\",\"refreshToken\":\"valid-refresh-token\",\"expiresAt\":"
                + (Now - 3600)
                + "}"
        );
        Handler.TokenEndpointResponse = () => throw new HttpRequestException("Network error");

        await Assert.ThrowsAsync<RefreshAccessTokenFailedError>(() => GetAsync());

        Assert.Equal("{}", File.ReadAllText(AuthPath));
    }

    [Fact]
    public async Task Should_treat_token_as_valid_if_expiresAt_is_missing_token_case()
    {
        var json = "{\"token\":\"cli-provided-token\"}";
        WriteAuthJson(json);

        Assert.Equal("cli-provided-token", await GetAsync());
        Assert.Equal(json, File.ReadAllText(AuthPath));
    }

    [Fact]
    public async Task Should_preserve_new_refresh_token_if_provided_in_response()
    {
        WriteAuthJson(
            "{\"token\":\"expired-access-token\",\"refreshToken\":\"old-refresh-token\",\"expiresAt\":"
                + (Now - 3600)
                + "}"
        );
        Handler.TokenEndpointResponse = () =>
            TokenResponse(",\"refresh_token\":\"new-refresh-token\"");

        await GetAsync();

        Assert.Equal("new-refresh-token", (string?)WrittenAuth()["refreshToken"]);
    }

    [Fact]
    public async Task Should_not_overwrite_refresh_token_if_not_provided_in_response()
    {
        WriteAuthJson(
            "{\"token\":\"expired-access-token\",\"refreshToken\":\"existing-refresh-token\",\"expiresAt\":"
                + (Now - 3600)
                + "}"
        );
        Handler.TokenEndpointResponse = () => TokenResponse();

        await GetAsync();

        Assert.False(WrittenAuth().ContainsKey("refreshToken"));
    }

    [Fact]
    public async Task Should_calculate_expiresAt_correctly_from_expires_in()
    {
        WriteAuthJson(
            "{\"token\":\"expired-access-token\",\"refreshToken\":\"valid-refresh-token\",\"expiresAt\":"
                + (Now - 3600)
                + "}"
        );
        Handler.TokenEndpointResponse = () =>
            ScriptedHandler.Json(
                "{\"access_token\":\"new-access-token\",\"token_type\":\"Bearer\",\"expires_in\":7200}"
            );

        var before = Now;
        await GetAsync();
        var after = Now;

        var expiresAt = (double)WrittenAuth()["expiresAt"]!;
        Assert.InRange(expiresAt, before + 7200, after + 7200);
    }
}
