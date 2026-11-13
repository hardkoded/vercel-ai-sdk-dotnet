// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;
using UnsupportedFieldTypeError = Vercel.AI.Util.ExtractionUnsupportedFieldTypeError;

namespace Vercel.AI.Tests.Upstream.Errors.ExtractionUnsupportedFieldTypeError;

public sealed class ExtractionUnsupportedFieldTypeErrorTests
{
    public static TheoryData<object?> UnmarkedValues()
    {
        // Upstream also lists `undefined`; C# has a single null value for it.
        return new TheoryData<object?>
        {
            null,
            new Exception("Unrelated error"),
            new { name = "AI_ExtractionUnsupportedFieldTypeError" },
            new ErrorMarkerBag(),
        };
    }

    [Fact]
    [UpstreamTest("packages/ai/src/error/extraction-unsupported-field-type-error.test.ts::ExtractionUnsupportedFieldTypeError::identifies the unsupported field and model", Coverage = UpstreamCoverage.Covered, Commit = "cb4fa523413f23ad36fe18b94b49473bbb0d5655")]
    public void Identifies_the_unsupported_field_and_model()
    {
        var error = new UnsupportedFieldTypeError("total", "number", "mock-provider", "mock-model-id");

        Assert.IsAssignableFrom<Exception>(error);
        Assert.True(AiSdkError.IsInstance(error));
        Assert.True(UnsupportedFieldTypeError.IsInstance(error));
        Assert.Equal("total", error.FieldId);
        Assert.Equal("number", error.FieldType);
        Assert.Equal("mock-provider", error.Provider);
        Assert.Equal("mock-model-id", error.ModelId);
        Assert.Equal("AI_ExtractionUnsupportedFieldTypeError", error.ErrorName);
        Assert.Equal("Field \"total\" has type \"number\", which is not supported by provider \"mock-provider\" and model \"mock-model-id\".", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/error/extraction-unsupported-field-type-error.test.ts::ExtractionUnsupportedFieldTypeError::recognizes the marker across package copies", Coverage = UpstreamCoverage.Covered, Commit = "cb4fa523413f23ad36fe18b94b49473bbb0d5655")]
    public void Recognizes_the_marker_across_package_copies()
    {
        Assert.True(UnsupportedFieldTypeError.IsInstance(new ErrorMarkerBag("vercel.ai.error.AI_ExtractionUnsupportedFieldTypeError", true)));
    }

    [Theory]
    [MemberData(nameof(UnmarkedValues))]
    [UpstreamTest("packages/ai/src/error/extraction-unsupported-field-type-error.test.ts::ExtractionUnsupportedFieldTypeError::rejects values without the marker: %s", Coverage = UpstreamCoverage.Covered, Commit = "cb4fa523413f23ad36fe18b94b49473bbb0d5655")]
    public void Rejects_values_without_the_marker(object? value)
    {
        Assert.False(UnsupportedFieldTypeError.IsInstance(value));
    }
}
