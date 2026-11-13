// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;
using RefusalError = Vercel.AI.Util.ExtractionRefusalError;

namespace Vercel.AI.Tests.Upstream.Errors.ExtractionRefusalError;

public sealed class ExtractionRefusalErrorTests
{
    public static TheoryData<object?> UnmarkedValues()
    {
        // Upstream also lists `undefined`; C# has a single null value for it.
        return new TheoryData<object?>
        {
            null,
            new Exception("Unrelated error"),
            new { name = "AI_ExtractionRefusalError" },
            new ErrorMarkerBag(),
        };
    }

    [Fact]
    [UpstreamTest("packages/ai/src/error/extraction-refusal-error.test.ts::ExtractionRefusalError::identifies the refused fields and model", Coverage = UpstreamCoverage.Covered, Commit = "cb4fa523413f23ad36fe18b94b49473bbb0d5655")]
    public void Identifies_the_refused_fields_and_model()
    {
        var error = new RefusalError(["total"], "mock-provider", "mock-model-id");

        Assert.IsAssignableFrom<Exception>(error);
        Assert.True(AiSdkError.IsInstance(error));
        Assert.True(RefusalError.IsInstance(error));
        Assert.Equal(["total"], error.FieldIds);
        Assert.Equal("mock-provider", error.Provider);
        Assert.Equal("mock-model-id", error.ModelId);
        Assert.Equal("AI_ExtractionRefusalError", error.ErrorName);
        Assert.Equal("Extraction model \"mock-model-id\" from provider \"mock-provider\" refused field \"total\".", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/error/extraction-refusal-error.test.ts::ExtractionRefusalError::lists every refused field", Coverage = UpstreamCoverage.Covered, Commit = "cb4fa523413f23ad36fe18b94b49473bbb0d5655")]
    public void Lists_every_refused_field()
    {
        var error = new RefusalError(["total", "dueDate"], "mock-provider", "mock-model-id");

        Assert.Equal("Extraction model \"mock-model-id\" from provider \"mock-provider\" refused fields \"total\", \"dueDate\".", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/error/extraction-refusal-error.test.ts::ExtractionRefusalError::recognizes the marker across package copies", Coverage = UpstreamCoverage.Covered, Commit = "cb4fa523413f23ad36fe18b94b49473bbb0d5655")]
    public void Recognizes_the_marker_across_package_copies()
    {
        Assert.True(RefusalError.IsInstance(new ErrorMarkerBag("vercel.ai.error.AI_ExtractionRefusalError", true)));
    }

    [Theory]
    [MemberData(nameof(UnmarkedValues))]
    [UpstreamTest("packages/ai/src/error/extraction-refusal-error.test.ts::ExtractionRefusalError::rejects values without the marker: %s", Coverage = UpstreamCoverage.Covered, Commit = "cb4fa523413f23ad36fe18b94b49473bbb0d5655")]
    public void Rejects_values_without_the_marker(object? value)
    {
        Assert.False(RefusalError.IsInstance(value));
    }
}
