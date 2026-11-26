// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.Middleware.WrapLanguageModel;

/// <summary>Port of <c>wrap-language-model.test.ts</c> &gt; <c>wrapLanguageModel &gt; supportedUrls property</c>.</summary>
public sealed class SupportedUrlsPropertyTests
{
    private const string Id =
        "packages/ai/src/middleware/wrap-language-model.test.ts::wrapLanguageModel > supportedUrls property::";

    [Fact]
    [UpstreamTest(
        Id + "should pass through by default",
        Coverage = UpstreamCoverage.Covered,
        Note = "SupportedUrls maps media types to regular-expression strings instead of RegExp arrays."
    )]
    public void Should_pass_through_by_default()
    {
        var supportedUrls = new Dictionary<string, string> { ["original/*"] = "^https://.*$" };

        var wrappedModel = new TestLanguageModel
        {
            SupportedUrls = supportedUrls,
        }.WrapLanguageModel(new HookMiddleware());

        Assert.Equal(supportedUrls, wrappedModel.SupportedUrls);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use middleware overrideSupportedUrls if provided",
        Coverage = UpstreamCoverage.Covered,
        Note = "SupportedUrls maps media types to regular-expression strings instead of RegExp arrays."
    )]
    public void Should_use_middleware_overrideSupportedUrls_if_provided()
    {
        var wrappedModel = new TestLanguageModel
        {
            SupportedUrls = new Dictionary<string, string> { ["original/*"] = "^https://.*$" },
        }.WrapLanguageModel(
            new HookMiddleware
            {
                OnOverrideSupportedUrls = _ => new Dictionary<string, string>
                {
                    ["override/*"] = "^https://.*$",
                },
            }
        );

        Assert.Equal(
            new Dictionary<string, string> { ["override/*"] = "^https://.*$" },
            wrappedModel.SupportedUrls
        );
    }
}
