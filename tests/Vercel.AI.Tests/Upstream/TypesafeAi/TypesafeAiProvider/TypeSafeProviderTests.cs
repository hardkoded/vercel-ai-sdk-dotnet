// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.TypeSafeAi;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.TypesafeAi.TypesafeAiProvider;

/// <summary>Port of <c>typesafe-ai-provider.test.ts</c> &gt; <c>TypeSafe provider</c>.</summary>
public sealed class TypeSafeProviderTests
{
    private const string Prefix = "packages/typesafe-ai/src/typesafe-ai-provider.test.ts::TypeSafe provider::";

    [Fact]
    [UpstreamTest(Prefix + "provides the experimental decision capability", Coverage = UpstreamCoverage.Covered)]
    public void Provides_the_experimental_decision_capability()
    {
        var model = TypeSafeAiProvider.Default.DecisionModel("jev-latest");
        Assert.Equal("v4", TypeSafeAiProvider.Default.SpecificationVersion);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal("typesafe.decision", model.Provider);
        Assert.Equal("jev-latest", model.ModelId);
        Assert.Equal(new[] { "choice", "score", "boolean" }, model.SupportedQuestionTypes);
    }

    [Fact]
    [UpstreamTest(Prefix + "retains the deprecated factory alias", Coverage = UpstreamCoverage.Covered)]
    public void Retains_the_deprecated_factory_alias()
    {
#pragma warning disable CS0618
        var model = TypeSafeAiProvider.Default.EvaluationModel("jev-latest");
#pragma warning restore CS0618
        Assert.Equal("typesafe.decision", model.Provider);
        Assert.Equal("jev-latest", model.ModelId);
    }

    [Theory]
    [InlineData("languageModel")]
    [InlineData("embeddingModel")]
    [InlineData("imageModel")]
    [UpstreamTest(Prefix + "rejects unsupported %s factories", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_unsupported_factories(string factory)
    {
        var provider = new TypeSafeAiProvider();
        Assert.Throws<NoSuchModelError>(() =>
        {
            switch (factory)
            {
                case "languageModel":
                    provider.LanguageModel("unknown");
                    break;
                case "embeddingModel":
                    provider.EmbeddingModel("unknown");
                    break;
                default:
                    provider.ImageModel("unknown");
                    break;
            }
        });
    }
}
