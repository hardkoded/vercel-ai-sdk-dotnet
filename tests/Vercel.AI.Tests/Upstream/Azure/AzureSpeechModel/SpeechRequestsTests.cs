// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Azure.AzureSpeechModel;

/// <summary>Port of <c>azure-speech-model.test.ts</c> &gt; <c>Speech requests</c>.</summary>
public sealed class SpeechRequestsTests
{
    private static string Ssml(string voice, string content, string locale = "en-US")
    {
        return AzureSpeechModelSetup.Ssml(voice, content, locale);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech requests::sends SSML with the default voice, mp3 format, and subscription key", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsSsmlWithTheDefaultVoiceMp3FormatAndSubscriptionKey()
    {
        var setup = AzureSpeechModelSetup.Create();
        var result = await setup.Provider.Speech("mai-voice-2-flash")
            .GenerateAsync(AzureSpeechModelSetup.Call("Hello world."), CancellationToken.None);

        var (_, headers, body) = setup.Request;
        Assert.Equal(Ssml("en-US-Harper:MAI-Voice-2-Flash", "Hello world."), body);
        Assert.Equal("application/ssml+xml", headers["content-type"]);
        Assert.Equal("audio-24khz-160kbitrate-mono-mp3", headers["x-microsoft-outputformat"]);
        Assert.Equal("test-key", headers["ocp-apim-subscription-key"]);
        Assert.DoesNotContain("api-key", headers.Keys);
        Assert.Contains("ai-sdk-azure/", headers["user-agent"]);
        Assert.Equal(AzureSpeechModelSetup.Audio, result.Audio);
        Assert.Empty(result.Warnings);
        Assert.Equal(body, result.RequestBody);
        Assert.Equal("mai-voice-2-flash", result.ModelId);
        Assert.Equal("audio/mpeg", result.Headers["Content-Type"]);
        Assert.NotEqual(default, result.Timestamp);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech requests::appends the model to the voice and uses its locale", Coverage = UpstreamCoverage.Covered)]
    public async Task AppendsTheModelToTheVoiceAndUsesItsLocale()
    {
        var setup = AzureSpeechModelSetup.Create();
        var call = AzureSpeechModelSetup.Call("Hallo.");
        call.Voice = "de-DE-Mia";
        await setup.Provider.Speech("mai-voice-2").GenerateAsync(call, CancellationToken.None);
        Assert.Equal(Ssml("de-DE-Mia:MAI-Voice-2", "Hallo.", "de-DE"), setup.Request.Body);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech requests::passes a full voice name through unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesAFullVoiceNameThroughUnchanged()
    {
        var setup = AzureSpeechModelSetup.Create();
        var call = AzureSpeechModelSetup.Call("Hi");
        call.Voice = "es-MX-Valeria:MAI-Voice-2-Flash";
        await setup.Provider.Speech("mai-voice-2").GenerateAsync(call, CancellationToken.None);
        Assert.Equal(Ssml("es-MX-Valeria:MAI-Voice-2-Flash", "Hi", "es-MX"), setup.Request.Body);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech requests::escapes text and attribute values", Coverage = UpstreamCoverage.Covered)]
    public async Task EscapesTextAndAttributeValues()
    {
        var setup = AzureSpeechModelSetup.Create();
        var call = AzureSpeechModelSetup.Call("Tom & Jerry <3 \"quotes\" 'apos'", "{\"azure\":{\"style\":\"a\\\"b\"}}");
        call.Voice = "en-US-Harper\"><evil";
        await setup.Provider.Speech("mai-voice-2").GenerateAsync(call, CancellationToken.None);
        Assert.Equal(
            Ssml(
                "en-US-Harper&quot;&gt;&lt;evil:MAI-Voice-2",
                "<mstts:express-as style=\"a&quot;b\">Tom &amp; Jerry &lt;3 &quot;quotes&quot; &apos;apos&apos;</mstts:express-as>"),
            setup.Request.Body);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech requests::maps style, styleDegree, and speed to express-as and prosody", Coverage = UpstreamCoverage.Covered)]
    public async Task MapsStyleStyleDegreeAndSpeedToExpressAsAndProsody()
    {
        var setup = AzureSpeechModelSetup.Create();
        var call = AzureSpeechModelSetup.Call("We won!", "{\"azure\":{\"style\":\"excited\",\"styleDegree\":1.5}}");
        call.Speed = 1.25;
        var result = await setup.Provider.Speech("mai-voice-2").GenerateAsync(call, CancellationToken.None);
        Assert.Equal(
            Ssml(
                "en-US-Harper:MAI-Voice-2",
                "<mstts:express-as style=\"excited\" styledegree=\"1.5\"><prosody rate=\"1.25\">We won!</prosody></mstts:express-as>"),
            setup.Request.Body);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech requests::warns and ignores styleDegree without style", Coverage = UpstreamCoverage.Covered)]
    public async Task WarnsAndIgnoresStyleDegreeWithoutStyle()
    {
        var setup = AzureSpeechModelSetup.Create();
        var result = await setup.Provider.Speech("mai-voice-2")
            .GenerateAsync(AzureSpeechModelSetup.Call("Hi", "{\"azure\":{\"styleDegree\":1.5}}"), CancellationToken.None);
        Assert.Equal(Ssml("en-US-Harper:MAI-Voice-2", "Hi"), setup.Request.Body);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("providerOptions.azure.styleDegree", warning.Feature);
        Assert.Equal("styleDegree requires style.", warning.Details);
    }

    [Theory]
    [InlineData("mp3", "audio-24khz-160kbitrate-mono-mp3")]
    [InlineData("wav", "riff-24khz-16bit-mono-pcm")]
    [InlineData("pcm", "raw-24khz-16bit-mono-pcm")]
    [InlineData("opus", "ogg-24khz-16bit-mono-opus")]
    [InlineData("WAV", "riff-24khz-16bit-mono-pcm")]
    [InlineData("audio-48khz-192kbitrate-mono-mp3", "audio-48khz-192kbitrate-mono-mp3")]
    [InlineData("raw-8khz-8bit-mono-mulaw", "raw-8khz-8bit-mono-mulaw")]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech requests::maps output format %s to %s", Coverage = UpstreamCoverage.Covered)]
    public async Task MapsOutputFormatTo(string outputFormat, string expected)
    {
        var setup = AzureSpeechModelSetup.Create();
        var call = AzureSpeechModelSetup.Call("Hi");
        call.OutputFormat = outputFormat;
        var result = await setup.Provider.Speech("mai-voice-2").GenerateAsync(call, CancellationToken.None);
        Assert.Equal(expected, setup.Request.Headers["x-microsoft-outputformat"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech requests::falls back to mp3 with a warning for unsupported formats", Coverage = UpstreamCoverage.Covered)]
    public async Task FallsBackToMp3WithAWarningForUnsupportedFormats()
    {
        var setup = AzureSpeechModelSetup.Create();
        var call = AzureSpeechModelSetup.Call("Hi");
        call.OutputFormat = "flac";
        var result = await setup.Provider.Speech("mai-voice-2").GenerateAsync(call, CancellationToken.None);
        Assert.Equal("audio-24khz-160kbitrate-mono-mp3", setup.Request.Headers["x-microsoft-outputformat"]);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("outputFormat", warning.Feature);
        Assert.Equal("Unsupported output format: flac. Using mp3 instead.", warning.Details);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech requests::warns about unsupported instructions", Coverage = UpstreamCoverage.Covered)]
    public async Task WarnsAboutUnsupportedInstructions()
    {
        var setup = AzureSpeechModelSetup.Create();
        var call = AzureSpeechModelSetup.Call("Hi");
        call.Instructions = "Speak slowly";
        var result = await setup.Provider.Speech("mai-voice-2").GenerateAsync(call, CancellationToken.None);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("instructions", warning.Feature);
        Assert.Equal("Use providerOptions.azure.style to control speaking style.", warning.Details);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech requests::does not apply Azure OpenAI URL settings to Speech requests", Coverage = UpstreamCoverage.Covered)]
    public async Task DoesNotApplyAzureOpenAIUrlSettingsToSpeechRequests()
    {
        var setup = AzureSpeechModelSetup.Create(options =>
        {
            options.BaseUrl = "https://proxy.example/openai";
            options.ApiVersion = "2025-01-01";
            options.UseDeploymentBasedUrls = true;
        });
        await setup.Provider.Speech("mai-voice-2").GenerateAsync(AzureSpeechModelSetup.Call("Hi"), CancellationToken.None);
        Assert.Equal(AzureSpeechModelSetup.SpeechUrl, setup.Request.Url);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech requests::honors speechBaseURL and per-request headers", Coverage = UpstreamCoverage.Covered)]
    public async Task HonorsSpeechBaseUrlAndPerRequestHeaders()
    {
        var setup = AzureSpeechModelSetup.Create(options =>
        {
            options.ResourceName = null;
            options.SpeechBaseUrl = "https://eastus.api.cognitive.microsoft.com/";
            options.Headers["x-custom"] = "provider";
        });
        var call = AzureSpeechModelSetup.Call("Hi");
        call.Headers = new Dictionary<string, string?> { ["x-custom"] = "call" };
        await setup.Provider.Speech("mai-voice-2").GenerateAsync(call, CancellationToken.None);
        Assert.Equal("https://eastus.api.cognitive.microsoft.com/tts/cognitiveservices/v1", setup.Request.Url);
        Assert.Equal("call", setup.Request.Headers["x-custom"]);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech requests::uses the Entra token instead of the key", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesTheEntraTokenInsteadOfTheKey()
    {
        var setup = AzureSpeechModelSetup.Create(options =>
        {
            options.ApiKey = null;
            options.TokenProvider = () => "entra-token";
        });
        await setup.Provider.Speech("mai-voice-2").GenerateAsync(AzureSpeechModelSetup.Call("Hi"), CancellationToken.None);
        Assert.Equal("Bearer entra-token", setup.Request.Headers["authorization"]);
        Assert.DoesNotContain("ocp-apim-subscription-key", setup.Request.Headers.Keys);
    }
}
