// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapLanguageModel;

/// <summary>Port of <c>wrap-language-model.test.ts</c> &gt; <c>wrapLanguageModel &gt; provider property</c>.</summary>
public sealed class ProviderPropertyTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-language-model.test.ts::wrapLanguageModel > provider property::";

    // TestLanguageModel always reports the provider "test". It stands in for the mock's "test-provider".
    private static TestLanguageModel Model() => new("test-model");

    [Fact]
    [UpstreamTest(
        Id + "should pass through by default",
        Coverage = UpstreamCoverage.Covered,
        Note = "TestLanguageModel has the fixed provider 'test' instead of 'test-provider'."
    )]
    public void Should_pass_through_by_default()
    {
        var wrappedModel = Model().WrapLanguageModel(new HookMiddleware());

        Assert.Equal("test", wrappedModel.Provider);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use middleware overrideProvider if provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_middleware_overrideProvider_if_provided()
    {
        var wrappedModel = Model()
            .WrapLanguageModel(
                new HookMiddleware { OnOverrideProvider = _ => "override-provider" }
            );

        Assert.Equal("override-provider", wrappedModel.Provider);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use providerId parameter if provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_providerId_parameter_if_provided()
    {
        var wrappedModel = Model()
            .WrapLanguageModel([new HookMiddleware()], providerId: "override-provider");

        Assert.Equal("override-provider", wrappedModel.Provider);
    }
}
