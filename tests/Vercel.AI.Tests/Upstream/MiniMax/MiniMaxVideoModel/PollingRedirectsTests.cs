// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.MiniMax;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.MiniMaxVideoModel.MiniMaxVideoModelHarness;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>Port of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel &gt; polling redirects</c>.</summary>
public sealed class PollingRedirectsTests
{
    private const string Prefix =
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel > polling redirects::";

    [Fact]
    [UpstreamTest(
        Prefix + "should validate redirects after trusting the configured origin",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_validate_redirects_after_trusting_the_configured_origin()
    {
        var handler = new ParityHandler(call =>
        {
            if (call.Method == HttpMethod.Post)
            {
                return ParityHandler.Json("{\"task_id\":\"task-123\"}");
            }

            var redirect = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Found);
            redirect.Headers.Location = new Uri("http://169.254.169.254/latest/meta-data/");
            return redirect;
        });
        var model = CreateModel(handler, baseUrl: "http://localhost:3000");

        await Assert.ThrowsAsync<Vercel.AI.Util.DownloadError>(() =>
            model.DoGenerateAsync(new MiniMaxCall().ToCall(), CancellationToken.None)
        );

        Assert.Equal(2, handler.Calls.Count);
        Assert.Equal(
            "http://localhost:3000/v2/query/video_generation/" + TaskId,
            handler.Calls[1].Uri.AbsoluteUri
        );
    }
}
