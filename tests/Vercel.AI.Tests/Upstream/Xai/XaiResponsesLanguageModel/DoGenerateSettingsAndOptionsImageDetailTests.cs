// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; settings and options &gt; image detail</c>.</summary>
public sealed class DoGenerateSettingsAndOptionsImageDetailTests
{
    private const string Prefix = TestFile + " > doGenerate > settings and options > image detail::";

    [Fact]
    [UpstreamTest(Prefix + "should pass detail from the imageDetail provider option on image parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_detail_from_the_imageDetail_provider_option_on_image_parts()
    {
        var capture = EmptyResponse("grok-4.3");
        var image = new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null)
        {
            ProviderOptions = UpstreamChat.Bag("xai", """{"imageDetail":"high"}"""),
        };

        await Model(capture, "grok-4.3").DoGenerateAsync(Options(image), CancellationToken.None);

        JsonAssert.Equal(Body(capture)["input"], """
            [{"role":"user","content":[
              {"type":"input_text","text":"What is in this image?"},
              {"type":"input_image","image_url":"data:image/png;base64,AAECAw==","detail":"high"}]}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not set detail when the imageDetail provider option is not set", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_set_detail_when_the_imageDetail_provider_option_is_not_set()
    {
        var capture = EmptyResponse("grok-4.3");
        var image = new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null);

        await Model(capture, "grok-4.3").DoGenerateAsync(Options(image), CancellationToken.None);

        JsonAssert.Equal(Body(capture)["input"], """
            [{"role":"user","content":[
              {"type":"input_text","text":"What is in this image?"},
              {"type":"input_image","image_url":"data:image/png;base64,AAECAw=="}]}]
            """);
    }

    private static LanguageModelCallOptions Options(FileContentPart image)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(new UserContentPart[] { new TextContentPart("What is in this image?"), image }) },
        };
    }
}
