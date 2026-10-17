// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using KlingAIProviderType = Vercel.AI.KlingAI.KlingAIProvider;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIProvider;

/// <summary>Port of <c>klingai-provider.test.ts</c> &gt; <c>createKlingAI &gt; specificationVersion</c>.</summary>
public sealed class CreateKlingAISpecificationVersionTests
{
    private const string Prefix = "packages/klingai/src/klingai-provider.test.ts::createKlingAI > specificationVersion::";

    [Fact]
    [UpstreamTest(Prefix + "should have specificationVersion v4", Coverage = UpstreamCoverage.Covered)]
    public void Should_have_specificationVersion_v4()
    {
        var provider = KlingAIProviderType.Create();

        Assert.Equal("v4", provider.SpecificationVersion);
    }
}
