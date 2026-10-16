// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doStream</c> (the tests added for AUDIO response modalities).</summary>
public sealed class DoStreamTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doStream::should only pass valid provider options with AUDIO responseModalities", Coverage = UpstreamCoverage.Covered)]
    public void Should_only_pass_valid_provider_options_with_AUDIO_responseModalities()
    {
        var options = GoogleUpstream.Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"foo\":\"bar\",\"responseModalities\":[\"TEXT\",\"AUDIO\"]}}");

        var prepared = GoogleRequest.Prepare("gemini-pro", "google.generative-ai", options, true);

        GoogleUpstream.JsonEqual(prepared.Body["contents"]!, "[{\"role\":\"user\",\"parts\":[{\"text\":\"Hello\"}]}]");
        GoogleUpstream.JsonEqual(prepared.Body["generationConfig"]!, "{\"responseModalities\":[\"TEXT\",\"AUDIO\"]}");
    }
}
