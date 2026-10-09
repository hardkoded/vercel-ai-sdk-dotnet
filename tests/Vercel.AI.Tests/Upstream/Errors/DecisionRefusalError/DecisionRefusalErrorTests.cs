// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;
using RefusalError = Vercel.AI.Util.DecisionRefusalError;

namespace Vercel.AI.Tests.Upstream.Errors.DecisionRefusalError;

public sealed class DecisionRefusalErrorTests
{
    public static TheoryData<object?> UnmarkedValues()
    {
        return new TheoryData<object?>
        {
            null,
            new Exception("Unrelated error"),
            new ErrorMarkerBag(),
        };
    }

    [Fact]
    [UpstreamTest("packages/ai/src/error/decision-refusal-error.test.ts::DecisionRefusalError::identifies the refused questions and model", Coverage = UpstreamCoverage.Covered)]
    public void Identifies_the_refused_questions_and_model()
    {
        var error = new RefusalError(new[] { "precursor" }, "openai.decisions", "gpt-6-luna");
        Assert.IsAssignableFrom<AiSdkError>(error);
        Assert.True(RefusalError.IsInstance(error));
        Assert.Equal("AI_DecisionRefusalError", error.ErrorName);
        Assert.Equal(new[] { "precursor" }, error.QuestionIds);
        Assert.Equal("openai.decisions", error.Provider);
        Assert.Equal("gpt-6-luna", error.ModelId);
        Assert.Equal("Decision model \"gpt-6-luna\" from provider \"openai.decisions\" refused question \"precursor\".", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/error/decision-refusal-error.test.ts::DecisionRefusalError::lists every refused question", Coverage = UpstreamCoverage.Covered)]
    public void Lists_every_refused_question()
    {
        var error = new RefusalError(new[] { "precursor", "yield" }, "openai.decisions", "gpt-6-luna");
        Assert.EndsWith("refused questions \"precursor\", \"yield\".", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/error/decision-refusal-error.test.ts::DecisionRefusalError::recognizes the marker across package copies", Coverage = UpstreamCoverage.Covered)]
    public void Recognizes_the_marker_across_package_copies()
    {
        Assert.True(RefusalError.IsInstance(new ErrorMarkerBag(RefusalError.ErrorMarker, true)));
    }

    [Theory]
    [MemberData(nameof(UnmarkedValues))]
    [UpstreamTest("packages/ai/src/error/decision-refusal-error.test.ts::DecisionRefusalError::rejects values without the marker: %s", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_values_without_the_marker(object? value)
    {
        Assert.False(RefusalError.IsInstance(value));
    }
}
