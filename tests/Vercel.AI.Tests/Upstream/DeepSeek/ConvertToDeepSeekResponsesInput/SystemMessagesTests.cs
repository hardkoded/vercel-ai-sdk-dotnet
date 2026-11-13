// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput.ConvertToDeepSeekResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput;

/// <summary>Port of <c>convert-to-deepseek-responses-input.test.ts</c> &gt; <c>convertToDeepSeekResponsesInput &gt; system messages</c>.</summary>
public sealed class SystemMessagesTests
{
    private const string Prefix = TestFile + " > system messages::";

    [Fact]
    [UpstreamTest(Prefix + "should preserve multiple system messages in order", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_preserve_multiple_system_messages_in_order()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, new SystemModelMessage("You are a helpful assistant."), new SystemModelMessage("Be concise."));

        JsonAssert.Equal(input, """[{"role":"system","content":"You are a helpful assistant."},{"role":"system","content":"Be concise."}]""");
        Assert.Empty(warnings);
    }
}
