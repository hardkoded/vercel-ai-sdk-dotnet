// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput.ConvertToXaiResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput;

/// <summary>Port of <c>convert-to-xai-responses-input.test.ts</c> &gt; <c>convertToXaiResponsesInput &gt; system messages</c>.</summary>
public sealed class SystemMessagesTests
{
    [Fact]
    [UpstreamTest(TestFile + " > system messages::should convert system messages", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_system_messages()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, new SystemModelMessage("you are a helpful assistant"));

        JsonAssert.Equal(input, """[{"content":"you are a helpful assistant","role":"system"}]""");
        Assert.Empty(warnings);
    }
}
