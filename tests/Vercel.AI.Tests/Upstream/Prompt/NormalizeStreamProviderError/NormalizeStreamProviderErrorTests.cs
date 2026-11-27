// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;

namespace Vercel.AI.Tests.Upstream.Prompt.NormalizeStreamProviderError;

/// <summary>Port of <c>normalize-stream-provider-error.test.ts</c> &gt; <c>normalizeStreamProviderError</c>.</summary>
public sealed class NormalizeStreamProviderErrorTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/prompt/normalize-stream-provider-error.test.ts::normalizeStreamProviderError::preserves non-AI SDK cross-realm Error instances",
        Coverage = UpstreamCoverage.Covered,
        Note = "A cross-realm Error is an error that is not an instance of this realm's Error. .NET models it as PreservedStreamError with isAiSdkError false."
    )]
    public void Preserves_non_AI_SDK_cross_realm_Error_instances()
    {
        var error = new PreservedStreamError("cross-realm error", false);

        Assert.False(error.IsAiSdkError);
        Assert.Same(error, StreamErrors.NormalizeStreamProviderError(error));
    }
}
