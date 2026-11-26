// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapLanguageModel;

/// <summary>Port of <c>wrap-language-model.test.ts</c> &gt; <c>wrapLanguageModel &gt; model property</c>.</summary>
public sealed class ModelPropertyTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-language-model.test.ts::wrapLanguageModel > model property::";

    [Fact]
    [UpstreamTest(Id + "should pass through by default", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_through_by_default()
    {
        var wrappedModel = new TestLanguageModel("test-model").WrapLanguageModel(
            new HookMiddleware()
        );

        Assert.Equal("test-model", wrappedModel.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use middleware overrideModelId if provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_middleware_overrideModelId_if_provided()
    {
        var wrappedModel = new TestLanguageModel("test-model").WrapLanguageModel(
            new HookMiddleware { OnOverrideModelId = _ => "override-model" }
        );

        Assert.Equal("override-model", wrappedModel.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use modelId parameter if provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_modelId_parameter_if_provided()
    {
        var wrappedModel = new TestLanguageModel("test-model").WrapLanguageModel(
            [new HookMiddleware()],
            modelId: "override-model"
        );

        Assert.Equal("override-model", wrappedModel.ModelId);
    }
}
