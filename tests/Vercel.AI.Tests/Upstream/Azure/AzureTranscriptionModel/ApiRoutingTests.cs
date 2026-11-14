// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionModel;

/// <summary>Port of <c>azure-transcription-model.test.ts</c> &gt; <c>API routing</c>.</summary>
public sealed class ApiRoutingTests
{
    private const string Prefix = "packages/azure/src/azure-transcription-model.test.ts::API routing::";

    [Theory]
    [InlineData("mai-transcribe-2", null, "speech", "MAI-Transcribe-2")]
    [InlineData("MAI-Transcribe-2", null, "speech", "MAI-Transcribe-2")]
    [InlineData("MaI-TrAnScRiBe-2", null, "speech", "MAI-Transcribe-2")]
    [InlineData("mai-transcribe-1.5", null, "speech", "MAI-Transcribe-1.5")]
    [InlineData("MAI-Transcribe-1.5", null, "speech", "MAI-Transcribe-1.5")]
    [InlineData("mai-transcribe-2-custom", null, "openai", null)]
    [InlineData("mai-transcribe-1.5-custom", null, "openai", null)]
    [InlineData("constructor", null, "openai", null)]
    [InlineData("whisper-1", null, "openai", null)]
    [InlineData("custom-deployment", null, "openai", null)]
    [InlineData("mai-transcribe-2", "openai", "openai", null)]
    [InlineData("mai-transcribe-1.5", "openai", "openai", null)]
    [InlineData("Future-Speech-Model", "speech", "speech", "Future-Speech-Model")]
    [InlineData("mai-transcribe-2", "speech", "speech", "MAI-Transcribe-2")]
    [UpstreamTest(Prefix + "routes %s with api=%s to %s", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Routes_with_api_to(string id, string? api, string expected, string? speechModel)
    {
        var harness = Setup();
        var providerOptions = api == null ? "{\"azure\":{}}" : "{\"azure\":{\"api\":\"" + api + "\"}}";

        await harness.Transcription(id).DoGenerateAsync(Call(providerOptions), CancellationToken.None);

        Assert.Contains(expected == "speech" ? "/speechtotext/" : "/audio/transcriptions", harness.Request.Url);
        if (expected == "speech")
        {
            var enhancedMode = harness.Definition["enhancedMode"]!;
            Assert.True(enhancedMode["enabled"]!.GetValue<bool>());
            Assert.Equal(speechModel, enhancedMode["model"]!.GetValue<string>());
        }
        else
        {
            Assert.Equal(id, harness.Request.Parts["model"].Text);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "resolves the API again for each call to the same model", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Resolves_the_API_again_for_each_call_to_the_same_model()
    {
        var harness = Setup();
        var model = harness.Transcription("mai-transcribe-2");

        await model.DoGenerateAsync(Call(), CancellationToken.None);
        Assert.Contains("/speechtotext/", harness.Request.Url);

        await model.DoGenerateAsync(Call("{\"azure\":{\"api\":\"openai\"}}"), CancellationToken.None);
        Assert.Contains("/audio/transcriptions", harness.Request.Url);

        await model.DoGenerateAsync(Call(), CancellationToken.None);
        Assert.Contains("/speechtotext/", harness.Request.Url);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves OpenAI options and warns about Speech-only options", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Preserves_OpenAI_options_and_warns_about_Speech_only_options()
    {
        var harness = Setup();

        var result = await harness.Transcription("whisper-1").DoGenerateAsync(
            Call("{\"openai\":{\"language\":\"en\",\"prompt\":\"Names\"},\"azure\":{\"api\":\"openai\",\"timestamps\":\"word\"}}"),
            CancellationToken.None);

        Assert.Equal("en", harness.Request.Parts["language"].Text);
        Assert.Equal("Names", harness.Request.Parts["prompt"].Text);
        Assert.Equal(
            new[] { ("unsupported", (string?)"providerOptions.azure.timestamps", (string?)"This option requires the Azure Speech API.") },
            Warnings(result));
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts OpenAI options under the azure key on the OpenAI API", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Accepts_OpenAI_options_under_the_azure_key_on_the_OpenAI_API()
    {
        var harness = Setup();

        var result = await harness.Transcription("whisper-1").DoGenerateAsync(
            Call("{\"azure\":{\"language\":\"en\",\"prompt\":\"Names\"}}"),
            CancellationToken.None);

        Assert.Equal("en", harness.Request.Parts["language"].Text);
        Assert.Equal("Names", harness.Request.Parts["prompt"].Text);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "prefers azure over openai for the same OpenAI option", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Prefers_azure_over_openai_for_the_same_OpenAI_option()
    {
        var harness = Setup();

        await harness.Transcription("whisper-1").DoGenerateAsync(
            Call("{\"openai\":{\"language\":\"de\",\"prompt\":\"Names\"},\"azure\":{\"language\":\"en\"}}"),
            CancellationToken.None);

        Assert.Equal("en", harness.Request.Parts["language"].Text);
        Assert.Equal("Names", harness.Request.Parts["prompt"].Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "warns about OpenAI-only options on the Speech API", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Warns_about_OpenAI_only_options_on_the_Speech_API()
    {
        var harness = Setup();

        var result = await harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Call("{\"azure\":{\"prompt\":\"Names\"}}"),
            CancellationToken.None);

        Assert.Contains("/speechtotext/", harness.Request.Url);
        Assert.Equal(
            new[] { ("unsupported", (string?)"providerOptions.azure.prompt", (string?)"This option requires the OpenAI transcription API.") },
            Warnings(result));
    }

    [Theory]
    [InlineData("{\"api\":\"invalid\"}")]
    [InlineData("{\"timestamps\":\"invalid\"}")]
    [InlineData("{\"locales\":[\"en\",\"de\"]}")]
    [InlineData("{\"transcribeStyle\":\"invalid\"}")]
    [InlineData("{\"diarization\":{\"enabled\":\"yes\"}}")]
    [InlineData("{\"diarization\":{\"enabled\":true,\"maxSpeakers\":2}}")]
    [InlineData("{\"phraseList\":{\"phrases\":[\"Vercel\"],\"biasingWeight\":1.5}}")]
    [InlineData("{\"profanityFilterMode\":\"Masked\"}")]
    [InlineData("{\"timestamp\":\"word\"}")]
    [UpstreamTest(Prefix + "rejects invalid options %j before fetching", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Rejects_invalid_options_before_fetching(string azure)
    {
        var harness = Setup();

        await Assert.ThrowsAsync<InvalidArgumentException>(() => harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Call("{\"azure\":" + azure + "}"),
            CancellationToken.None));

        Assert.Empty(harness.Handler.Requests);
    }
}
