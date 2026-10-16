// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel.KlingAIVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel;

/// <summary>Port of <c>klingai-video-model.test.ts</c> &gt; <c>KlingAIVideoModel</c> &gt; <c>constructor</c>.</summary>
public sealed class ConstructorTests
{
    private const string Prefix = "packages/klingai/src/klingai-video-model.test.ts::KlingAIVideoModel > constructor::";

    [Fact]
    [UpstreamTest(Prefix + "should expose correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public async Task should_expose_correct_provider_and_model_information()
    {
        var model = CreateModel(new Server().Handler);

        Assert.Equal("klingai.video", model.Provider);
        Assert.Equal("kling-v2.6-motion-control", model.ModelId);
        Assert.Equal(1, model.MaxVideosPerCall);
    }

    [Fact]
    [UpstreamTest(Prefix + "should accept custom model IDs in constructor", Coverage = UpstreamCoverage.Covered)]
    public async Task should_accept_custom_model_IDs_in_constructor()
    {
        var model = CreateModel(new Server().Handler, modelId: "kling-v2.6-t2v");

        Assert.Equal("kling-v2.6-t2v", model.ModelId);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw NoSuchModelError for unknown model IDs on doStart", Coverage = UpstreamCoverage.Covered)]
    public async Task should_throw_NoSuchModelError_for_unknown_model_IDs_on_doStart()
    {
        var model = CreateModel(new Server().Handler, modelId: "unknown-model");

        var error = await Assert.ThrowsAsync<NoSuchModelError>(() => model.DoStartAsync(Start(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("No such videoModel: unknown-model", error.Message);
    }
}
