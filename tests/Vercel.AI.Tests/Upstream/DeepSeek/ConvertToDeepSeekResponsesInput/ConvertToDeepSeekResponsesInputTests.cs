// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput.ConvertToDeepSeekResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput;

/// <summary>Port of <c>convert-to-deepseek-responses-input.test.ts</c> &gt; <c>convertToDeepSeekResponsesInput</c>.</summary>
public sealed class ConvertToDeepSeekResponsesInputTests
{
    [Fact]
    [UpstreamTest(TestFile + "::should convert an empty prompt", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_convert_an_empty_prompt()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings);

        JsonAssert.Equal(input, "[]");
        Assert.Empty(warnings);
    }
}
