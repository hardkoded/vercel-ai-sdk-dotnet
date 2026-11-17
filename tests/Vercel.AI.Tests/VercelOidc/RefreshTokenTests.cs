// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway.VercelOidc;

namespace Vercel.AI.Tests.VercelOidc;

/// <summary>Port of <c>token.test.ts</c> &gt; <c>refreshToken</c> (in <c>@vercel/oidc</c>).</summary>
public sealed class RefreshTokenTests : VercelOidcTestBase
{
    public RefreshTokenTests()
    {
        WriteAuthJson("{\"token\":\"test\"}");
        WriteProjectJson("{\"projectId\":\"" + ProjectId + "\"}");
        Handler.OidcResponse = () => ScriptedHandler.Json("{\"token\":\"" + FreshToken + "\"}");
    }

    private static string FreshToken { get; } = CreateValidToken("fresh");

    private static string? Env => Environment.GetEnvironmentVariable("VERCEL_OIDC_TOKEN");

    private string SaveCached(string projectId, string token)
    {
        var path = Path.Combine(TokenDataDir, projectId + ".json");
        File.WriteAllText(path, "{\"token\":\"" + token + "\"}");
        return path;
    }

    private static Task RefreshAsync(VercelOidcTokenOptions? options = null) =>
        Token.RefreshTokenAsync(options, CancellationToken.None);

    [Fact]
    public async Task Should_correctly_load_saved_token_from_file()
    {
        var saved = CreateValidToken("saved");
        SaveCached(ProjectId, saved);

        await RefreshAsync();

        Assert.Equal(saved, Env);
    }

    [Fact]
    public async Task Should_correctly_save_token_to_file()
    {
        await RefreshAsync();

        Assert.Equal(FreshToken, Env);
        var saved = File.ReadAllText(Path.Combine(TokenDataDir, ProjectId + ".json"));
        Assert.Equal("{\"token\":\"" + FreshToken + "\"}", saved);
    }

    [Fact]
    public async Task Should_use_provided_team_and_project_instead_of_reading_project_json()
    {
        var rootDirCalls = 0;
        TokenIo.FindRootDirOverride = () =>
        {
            rootDirCalls++;
            return RootDir;
        };

        await RefreshAsync(
            new VercelOidcTokenOptions { Team = "team_custom456", Project = "prj_custom123" }
        );

        Assert.Equal(0, rootDirCalls);
        Assert.Equal(
            "https://api.vercel.com/v1/projects/prj_custom123/token?source=vercel-oidc-refresh&teamId=team_custom456",
            Handler.Requests.Last().RequestUri!.AbsoluteUri
        );
        Assert.True(File.Exists(Path.Combine(TokenDataDir, "prj_custom123.json")));
    }

    [Fact]
    public async Task Should_merge_provided_project_with_team_from_project_json()
    {
        WriteProjectJson("{\"projectId\":\"" + ProjectId + "\",\"orgId\":\"team_fromproject567\"}");

        await RefreshAsync(new VercelOidcTokenOptions { Project = "prj_custom234" });

        Assert.EndsWith(
            "&teamId=team_fromproject567",
            Handler.Requests.Last().RequestUri!.AbsoluteUri
        );
        Assert.Contains(
            "/projects/prj_custom234/",
            Handler.Requests.Last().RequestUri!.AbsoluteUri
        );
    }

    [Fact]
    public async Task Should_merge_provided_team_with_project_from_project_json()
    {
        WriteProjectJson("{\"projectId\":\"prj_fromjson123\",\"orgId\":\"original-team\"}");

        await RefreshAsync(new VercelOidcTokenOptions { Team = "team_custom789" });

        Assert.EndsWith("&teamId=team_custom789", Handler.Requests.Last().RequestUri!.AbsoluteUri);
        Assert.Contains(
            "/projects/prj_fromjson123/",
            Handler.Requests.Last().RequestUri!.AbsoluteUri
        );
    }

    [Fact]
    public async Task Should_use_cached_token_when_valid_with_custom_project()
    {
        var cached = CreateValidToken("cached-valid-token");
        SaveCached("prj_customcached123", cached);

        await RefreshAsync(
            new VercelOidcTokenOptions
            {
                Team = "team_customcached456",
                Project = "prj_customcached123",
            }
        );

        Assert.Empty(Handler.Requests);
        Assert.Equal(cached, Env);
    }

    [Fact]
    public async Task Should_refresh_token_when_cached_token_expires_within_buffer()
    {
        var expiresIn3Minutes = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 180;
        var path = SaveCached(
            "prj_buffertest123",
            CreateToken(expiresIn3Minutes, "expiring-soon-token")
        );

        await RefreshAsync(
            new VercelOidcTokenOptions
            {
                Team = "team_buffertest456",
                Project = "prj_buffertest123",
                ExpirationBufferMs = 300000,
            }
        );

        Assert.Single(Handler.Requests);
        Assert.Equal(FreshToken, Env);
        Assert.Equal("{\"token\":\"" + FreshToken + "\"}", File.ReadAllText(path));
    }

    [Fact]
    public async Task Should_use_cached_token_when_it_does_not_expire_within_buffer()
    {
        var expiresIn10Minutes = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 600;
        var cached = CreateToken(expiresIn10Minutes, "valid-cached-token");
        SaveCached("prj_buffervalid123", cached);

        await RefreshAsync(
            new VercelOidcTokenOptions
            {
                Team = "team_buffervalid456",
                Project = "prj_buffervalid123",
                ExpirationBufferMs = 300000,
            }
        );

        Assert.Empty(Handler.Requests);
        Assert.Equal(cached, Env);
    }
}
