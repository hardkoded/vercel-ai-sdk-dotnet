// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using XaiProviderType = Vercel.AI.Xai.XaiProvider;
using XaiResponsesLanguageModelType = Vercel.AI.Xai.XaiResponsesLanguageModel;

namespace Vercel.AI.Tests.Upstream.Xai.XaiProvider;

/// <summary>Port of <c>xai-provider.test.ts</c> &gt; <c>xAIProvider &gt; createXAI</c>.</summary>
public sealed class XAIProviderCreateXAITests
{
    private const string Prefix = "packages/xai/src/xai-provider.test.ts::xAIProvider > createXAI::";

    [Fact]
    [UpstreamTest(Prefix + "should return a responses model when called as a function", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_a_responses_model_when_called_as_a_function()
    {
        var provider = XaiProviderType.Create(new OpenAICompatibleOptions { ApiKey = "test-key" });

        var model = provider.LanguageModel("foo-model-id");

        Assert.IsType<XaiResponsesLanguageModelType>(model);
    }
}
