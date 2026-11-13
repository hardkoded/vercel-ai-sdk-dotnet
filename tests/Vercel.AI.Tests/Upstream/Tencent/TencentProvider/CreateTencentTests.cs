// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Tencent.TencentProvider.TencentProviderSupport;
using TencentProviderType = Vercel.AI.Tencent.TencentProvider;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentProvider;

/// <summary>Port of <c>tencent-provider.test.ts</c> &gt; <c>createTencent</c>.</summary>
[Collection(TencentEnvironmentCollection.Name)]
public sealed class CreateTencentTests : IDisposable
{
    private const string Prefix = UpstreamFile + "::createTencent::";

    private readonly TencentEnvironment _env = new();

    public void Dispose()
    {
        _env.Dispose();
    }

    [Fact]
    [UpstreamTest(Prefix + "throws NoSuchModelError for embedding and image models", Coverage = UpstreamCoverage.Covered, Commit = Commit, Note = "The port has no textEmbeddingModel alias.")]
    public void Throws_NoSuchModelError_for_embedding_and_image_models()
    {
        var provider = TencentProviderType.Create();

        Assert.Throws<NoSuchModelError>(() => provider.EmbeddingModel("model"));
        Assert.Throws<NoSuchModelError>(() => provider.ImageModel("model"));
    }
}
