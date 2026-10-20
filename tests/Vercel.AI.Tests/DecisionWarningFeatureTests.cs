// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.ProviderUtils.DecisionLanguageModelSuite;
using static Vercel.AI.Tests.Upstream.ProviderUtils.DecisionLanguageModelSuite.DecisionLanguageModelSupport;
using DecisionModel = Vercel.AI.Operations.DecisionLanguageModel;

namespace Vercel.AI.Tests;

public sealed class DecisionWarningFeatureTests
{
    [Fact]
    public async Task Decision_warnings_keep_the_feature_of_the_language_model_warnings()
    {
        var language = new RecordingLanguageModel
        {
            OnGenerate = (_, _) => Task.FromResult(new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("{\"q1\":1.25,\"q0\":\"c1\"}") },
                FinishReason.Stop,
                new LanguageModelUsage(1, 1, 2),
                warnings: new[] { new CallWarning("unsupported", "topK", "topK is ignored"), new CallWarning("other", "plain") })),
        };

        var output = await new DecisionModel(language, "test.decision").DoDecideAsync(Call());

        Assert.Collection(
            output.Warnings,
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("topK", warning.Feature);
                Assert.Equal("topK is ignored", warning.Details);
            },
            warning =>
            {
                Assert.Equal("other", warning.Type);
                Assert.Null(warning.Feature);
                Assert.Equal("plain", warning.Message);
            });
    }
}
