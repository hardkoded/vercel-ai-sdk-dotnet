// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>doGenerate</c> (the tests added for AUDIO response modalities and thought signatures on text).</summary>
public sealed class DoGenerateTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should send videoMetadata on video file parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_videoMetadata_on_video_file_parts()
    {
        var handler = new RecordingHandler();
        var model = GoogleUpstream.Client(handler).LanguageModel("gemini-2.5-flash");
        var file = new FileContentPart("video/mp4", "https://www.youtube.com/watch?v=example", null, null)
        {
            ProviderOptions = GoogleUpstream.ProviderOptions(
                """{"google":{"videoMetadata":{"startOffset":"0s","endOffset":"10.5s","fps":0.5}}}"""),
        };

        await model.DoGenerateAsync(
            GoogleUpstream.Prompt(new UserModelMessage(new UserContentPart[] { file })),
            CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Body);
        GoogleUpstream.JsonEqual(
            body.RootElement.GetProperty("contents"),
            """[{"role":"user","parts":[{"fileData":{"mimeType":"video/mp4","fileUri":"https://www.youtube.com/watch?v=example"},"videoMetadata":{"startOffset":"0s","endOffset":"10.5s","fps":0.5}}]}]""");
    }

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
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Here is audio:\"},{\"inlineData\":{\"mimeType\":\"audio/wav\",\"data\":\"YXVkaW9kYXRh\"}}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}]}");

        Assert.Equal(2, result.Content.Count);
        Assert.Equal("Here is audio:", Assert.IsAssignableFrom<GeneratedText>(result.Content[0]).Text);
        var file = Assert.IsAssignableFrom<GeneratedFile>(result.Content[1]);
        Assert.Equal("audio/wav", file.MediaType);
        Assert.Equal("audiodata", System.Text.Encoding.UTF8.GetString(file.Data));
    }

    [Fact]
    public void Should_throw_on_inlineData_that_is_not_valid_base64()
    {
        var ex = Assert.Throws<InvalidResponseDataException>(() => GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"inlineData\":{\"mimeType\":\"audio/wav\",\"data\":\"audiodata\"}}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}]}"));

        Assert.Contains("base64", ex.Message);
    }

    [Fact]
    public void Should_throw_on_inlineData_without_data()
    {
        Assert.Throws<InvalidResponseDataException>(() => GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"inlineData\":{\"mimeType\":\"audio/wav\"}}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}]}"));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-language-model.test.ts::doGenerate::should correctly parse thought signatures with reasoning parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_correctly_parse_thought_signatures_with_reasoning_parts()
    {
        var result = GoogleUpstream.Parse("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Visible text part 1. \",\"thoughtSignature\":\"sig1\"},{\"text\":\"This is a thought process.\",\"thought\":true,\"thoughtSignature\":\"sig2\"},{\"text\":\"Visible text part 2.\",\"thoughtSignature\":\"sig3\"}],\"role\":\"model\"},\"finishReason\":\"STOP\",\"index\":0}],\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":20,\"totalTokenCount\":30}}");

        Assert.Equal(new[] { "text", "reasoning", "text" }, result.Content.Select(part => part.Type).ToArray());
        var first = Assert.IsAssignableFrom<GeneratedText>(result.Content[0]);
        Assert.Equal("Visible text part 1. ", first.Text);
        GoogleUpstream.JsonEqual(first.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"sig1\"}}");
        var reasoning = Assert.IsAssignableFrom<GeneratedReasoning>(result.Content[1]);
        Assert.Equal("This is a thought process.", reasoning.Text);
        GoogleUpstream.JsonEqual(reasoning.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"sig2\"}}");
        var last = Assert.IsAssignableFrom<GeneratedText>(result.Content[2]);
        Assert.Equal("Visible text part 2.", last.Text);
        GoogleUpstream.JsonEqual(last.ProviderMetadata!.Value, "{\"google\":{\"thoughtSignature\":\"sig3\"}}");
    }
}
