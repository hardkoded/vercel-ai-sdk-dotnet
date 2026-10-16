// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doGenerate</c> (the tests added for AUDIO response modalities).</summary>
public sealed class DoGenerateTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should pass AUDIO responseModalities in provider options", Coverage = UpstreamCoverage.Covered)]
    public void Should_pass_AUDIO_responseModalities_in_provider_options()
    {
        var options = GoogleUpstream.Hello();
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"responseModalities\":[\"TEXT\",\"AUDIO\"]}}");

        var prepared = GoogleRequest.Prepare("gemini-pro", "google.generative-ai", options, false);

        GoogleUpstream.JsonEqual(prepared.Body["generationConfig"]!["responseModalities"], "[\"TEXT\",\"AUDIO\"]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should include audio inlineData parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_audio_inlineData_parts()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Here is audio:\"},{\"inlineData\":{\"mimeType\":\"audio/wav\",\"data\":\"audiodata\"}}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}]}");

        Assert.Equal(2, result.Content.Count);
        Assert.Equal("Here is audio:", Assert.IsAssignableFrom<GeneratedText>(result.Content[0]).Text);
        var file = Assert.IsAssignableFrom<GeneratedFile>(result.Content[1]);
        Assert.Equal("audio/wav", file.MediaType);
    }
}
