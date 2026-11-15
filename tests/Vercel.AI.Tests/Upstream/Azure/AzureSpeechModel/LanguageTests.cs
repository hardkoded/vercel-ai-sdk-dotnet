// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Azure.AzureSpeechModel;

/// <summary>Port of <c>azure-speech-model.test.ts</c> &gt; <c>language</c>.</summary>
public sealed class LanguageTests
{
    [Theory]
    [InlineData("mai-voice-2", "de", "de-DE-Mia:MAI-Voice-2", "de-DE")]
    [InlineData("mai-voice-2-flash", "es", "es-MX-Valeria:MAI-Voice-2-Flash", "es-MX")]
    [InlineData("mai-voice-2", "pt", "pt-BR-Luana:MAI-Voice-2", "pt-BR")]
    [InlineData("mai-voice-2", "zh", "zh-CN-Mei:MAI-Voice-2", "zh-CN")]
    [InlineData("mai-voice-2", "en", "en-US-Harper:MAI-Voice-2", "en-US")]
    [InlineData("mai-voice-2", "FR", "fr-FR-Soleil:MAI-Voice-2", "fr-FR")]
    [InlineData("mai-voice-2", "ko-KR", "ko-KR-Haena:MAI-Voice-2", "ko-KR")]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::language::uses the %s default voice for language %s", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesTheDefaultVoiceForLanguage(string id, string language, string voice, string locale)
    {
        var setup = AzureSpeechModelSetup.Create();
        var call = AzureSpeechModelSetup.Call("Hi");
        call.Language = language;
        var result = await setup.Provider.Speech(id).GenerateAsync(call, CancellationToken.None);
        Assert.Equal(AzureSpeechModelSetup.Ssml(voice, "Hi", locale), setup.Request.Body);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("ja", "No default MAI voice for language \"ja\". en-US-Harper was used.")]
    [InlineData("constructor", "No default MAI voice for language \"constructor\". en-US-Harper was used.")]
    [InlineData("auto", "Automatic language detection is not supported. en-US-Harper was used.")]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::language::falls back to en-US-Harper with a warning for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task FallsBackToEnUsHarperWithAWarningFor(string language, string details)
    {
        var setup = AzureSpeechModelSetup.Create();
        var call = AzureSpeechModelSetup.Call("Hi");
        call.Language = language;
        var result = await setup.Provider.Speech("mai-voice-2").GenerateAsync(call, CancellationToken.None);
        Assert.Equal(AzureSpeechModelSetup.Ssml("en-US-Harper:MAI-Voice-2", "Hi"), setup.Request.Body);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("language", warning.Feature);
        Assert.Equal(details, warning.Details);
    }

    [Theory]
    [InlineData("de")]
    [InlineData("de-AT")]
    [InlineData("auto")]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::language::keeps an explicit voice without a warning for language %s", Coverage = UpstreamCoverage.Covered)]
    public async Task KeepsAnExplicitVoiceWithoutAWarningForLanguage(string language)
    {
        var setup = AzureSpeechModelSetup.Create();
        var call = AzureSpeechModelSetup.Call("Hallo");
        call.Voice = "de-DE-Klaus";
        call.Language = language;
        var result = await setup.Provider.Speech("mai-voice-2").GenerateAsync(call, CancellationToken.None);
        Assert.Equal(AzureSpeechModelSetup.Ssml("de-DE-Klaus:MAI-Voice-2", "Hallo", "de-DE"), setup.Request.Body);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::language::keeps an explicit voice and warns when the language conflicts", Coverage = UpstreamCoverage.Covered)]
    public async Task KeepsAnExplicitVoiceAndWarnsWhenTheLanguageConflicts()
    {
        var setup = AzureSpeechModelSetup.Create();
        var call = AzureSpeechModelSetup.Call("Hi");
        call.Voice = "en-US-Ethan";
        call.Language = "de";
        var result = await setup.Provider.Speech("mai-voice-2").GenerateAsync(call, CancellationToken.None);
        Assert.Equal(AzureSpeechModelSetup.Ssml("en-US-Ethan:MAI-Voice-2", "Hi"), setup.Request.Body);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("language", warning.Feature);
        Assert.Equal("The voice en-US-Ethan selects the language. Language \"de\" was ignored.", warning.Details);
    }
}
