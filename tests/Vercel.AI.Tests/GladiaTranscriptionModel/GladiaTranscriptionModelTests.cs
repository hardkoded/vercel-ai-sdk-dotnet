// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gladia;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.GladiaTranscriptionModel;

public sealed class GladiaTranscriptionModelTests
{
    private const string AudioUrl = "https://api.gladia.io/file/7403f025-be30-4335-ae29-20131e0adbd6";

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::warns when explicit language configuration overrides legacy fields", Coverage = UpstreamCoverage.Covered, Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb")]
    public async Task WarnsWhenExplicitLanguageConfigurationOverridesLegacyFields()
    {
        var handler = GladiaFixtures.Handler();
        var request = GladiaFixtures.Options("""{"languageConfig":{"codeSwitching":false},"language":"en"}""");
        var result = await GladiaFixtures.Provider(handler).Transcription().TranscribeAsync(GladiaFixtures.Audio(), request, CancellationToken.None);
        Assert.Equal("""{"code_switching":false}""", GladiaFixtures.InitBody(handler)["language_config"]!.ToJsonString());
        Assert.Contains(
            result.Warnings,
            warning => warning is DeprecatedWarning deprecated
                && deprecated.Setting == "language"
                && deprecated.Message == "Ignored because languageConfig is provided; remove this deprecated option.");
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::validates PII configuration before uploading audio", Coverage = UpstreamCoverage.Covered, Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb")]
    public async Task ValidatesPiiConfigurationBeforeUploadingAudio()
    {
        var handler = GladiaFixtures.Handler();
        var request = GladiaFixtures.Options("""{"piiRedactionConfig":{"processedTextType":"invalid"}}""");
        var error = await Assert.ThrowsAsync<InvalidArgumentError>(
            () => GladiaFixtures.Provider(handler).Transcription().TranscribeAsync(GladiaFixtures.Audio(), request, CancellationToken.None));
        Assert.Contains("invalid gladia provider options", error.Message, StringComparison.Ordinal);
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::warns about missing solaria-3 language without changing the request", Coverage = UpstreamCoverage.Covered, Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb")]
    public async Task WarnsAboutMissingSolaria3LanguageWithoutChangingTheRequest()
    {
        var handler = GladiaFixtures.Handler();
        var provider = GladiaFixtures.Provider(handler);
        var result = await ((GladiaProvider.GladiaTranscriptionModel)provider.TranscriptionModel("solaria-3")).TranscribeAsync(GladiaFixtures.Audio(), null, CancellationToken.None);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"audio_url\":\"" + AudioUrl + "\",\"model\":\"solaria-3\"}"), GladiaFixtures.InitBody(handler)));
        var warning = Assert.IsType<OtherWarning>(Assert.Single(result.Warnings));
        Assert.Equal(
            "The \"solaria-3\" model supports a single language only. Pass exactly one language in languageConfig.languages; Gladia may reject the request.",
            warning.Message);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::warns when PII configuration is set without enabling redaction", Coverage = UpstreamCoverage.Covered, Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb")]
    public async Task WarnsWhenPiiConfigurationIsSetWithoutEnablingRedaction()
    {
        var handler = GladiaFixtures.Handler();
        var request = GladiaFixtures.Options("""{"piiRedactionConfig":{"processedTextType":"MASK"}}""");
        var result = await GladiaFixtures.Provider(handler).Transcription().TranscribeAsync(GladiaFixtures.Audio(), request, CancellationToken.None);
        var warning = Assert.IsType<OtherWarning>(Assert.Single(result.Warnings));
        Assert.Equal("piiRedactionConfig has no effect unless piiRedaction is true.", warning.Message);
        Assert.Equal("""{"processed_text_type":"MASK"}""", GladiaFixtures.InitBody(handler)["pii_redaction_config"]!.ToJsonString());
    }

    [Theory]
    [InlineData("default")]
    [InlineData("solaria-1")]
    [InlineData("future-gladia-model")]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::forwards transcriptionModel alias ID %s", Coverage = UpstreamCoverage.Covered, Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb")]
    public async Task ForwardsTranscriptionModelAliasId(string modelId)
    {
        var handler = GladiaFixtures.Handler();
        var selected = (GladiaProvider.GladiaTranscriptionModel)GladiaFixtures.Provider(handler).TranscriptionModel(modelId);
        var result = await selected.TranscribeAsync(GladiaFixtures.Audio(), null, CancellationToken.None);
        Assert.Equal(modelId, selected.ModelId);
        Assert.Equal(modelId, result.Response.ModelId);
        var body = GladiaFixtures.InitBody(handler);
        if (modelId == "default")
        {
            Assert.False(body.ContainsKey("model"));
        }
        else
        {
            Assert.Equal(modelId, (string?)body["model"]);
        }
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::forwards current translation and audio-to-LLM options", Coverage = UpstreamCoverage.Covered, Commit = "c8875b71a36f921e49b34884edcf8a43a236bdeb")]
    public async Task ForwardsCurrentTranslationAndAudioToLlmOptions()
    {
        var handler = GladiaFixtures.Handler();
        var request = GladiaFixtures.Options("""
            {"translation":true,
             "translationConfig":{"targetLanguages":["fr"],"model":"batch","lipsync":false,"contextAdaptation":true,"context":"Science","informal":false},
             "audioToLlm":true,
             "audioToLlmConfig":{"prompts":["Summarize"],"model":"openai/gpt-5.4-nano"}}
            """);
        await GladiaFixtures.Provider(handler).Transcription().TranscribeAsync(GladiaFixtures.Audio(), request, CancellationToken.None);
        var body = GladiaFixtures.InitBody(handler);
        Assert.Equal(
            """{"target_languages":["fr"],"model":"batch","lipsync":false,"context_adaptation":true,"context":"Science","informal":false}""",
            body["translation_config"]!.ToJsonString());
        Assert.Equal("""{"prompts":["Summarize"],"model":"openai/gpt-5.4-nano"}""", body["audio_to_llm_config"]!.ToJsonString());
    }
}
