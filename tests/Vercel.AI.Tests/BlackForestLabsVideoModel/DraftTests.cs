// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>draft</c>.</summary>
public sealed class DraftTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > draft::";

    [Fact]
    [UpstreamTest(Id + "should request a draft preview", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldRequestADraftPreview()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(blackForestLabs: "{\"draft\":true}"), CancellationToken.None);

        AssertJson("{\"mode\":\"t2v\",\"prompt\":\"" + PromptText + "\",\"draft\":true}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should send draft when it is explicitly disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendDraftWhenItIsExplicitlyDisabled()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(blackForestLabs: "{\"draft\":false}"), CancellationToken.None);

        AssertMatches("{\"draft\":false}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should omit draft when unset", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldOmitDraftWhenUnset()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None);

        AssertNoProperty("draft", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should report the draft_cache download URL in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReportTheDraftCacheDownloadUrlInProviderMetadata()
    {
        var server = CreateServer();
        server.Json(PollUrl, "{\"status\":\"Ready\",\"result\":{\"sample\":\"" + VideoUrl + "\",\"draft_cache\":\"https://api.example.com/draft/bundle.bin\"}}");

        var result = await CreateModel(server).DoGenerateAsync(Call(blackForestLabs: "{\"draft\":true}"), CancellationToken.None);

        AssertJson(
            "[{\"id\":\"" + RequestId + "\",\"videoUrl\":\"" + VideoUrl + "\",\"draftCache\":\"https://api.example.com/draft/bundle.bin\",\"cost\":0.42,\"inputMegapixels\":1.23,\"outputMegapixels\":4.56}]",
            Videos(result));
    }

    [Fact]
    [UpstreamTest(Id + "should omit draftCache when the result has none", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldOmitDraftCacheWhenTheResultHasNone()
    {
        var result = await CreateModel(CreateServer()).DoGenerateAsync(Call(), CancellationToken.None);

        AssertNoProperty("draftCache", Videos(result)![0]);
    }
}
