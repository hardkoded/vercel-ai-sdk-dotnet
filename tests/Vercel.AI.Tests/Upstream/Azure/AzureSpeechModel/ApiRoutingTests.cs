// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Azure.AzureSpeechModel;

/// <summary>Port of <c>azure-speech-model.test.ts</c> &gt; <c>API routing</c>.</summary>
public sealed class ApiRoutingTests
{
    private const string OpenAIUrl = "https://test-resource.openai.azure.com/openai/v1/audio/speech?api-version=v1";

    [Theory]
    [InlineData("mai-voice-2-flash", null, "speech", "MAI-Voice-2-Flash")]
    [InlineData("MAI-Voice-2-Flash", null, "speech", "MAI-Voice-2-Flash")]
    [InlineData("mai-voice-2", null, "speech", "MAI-Voice-2")]
    [InlineData("MaI-vOiCe-2", null, "speech", "MAI-Voice-2")]
    [InlineData("mai-voice-2.1-flash", null, "speech", "MAI-Voice-2.1-Flash")]
    [InlineData("MAI-Voice-2.1", null, "speech", "MAI-Voice-2.1")]
    [InlineData("tts-1", null, "openai", null)]
    [InlineData("mai-voice-2-custom", null, "openai", null)]
    [InlineData("constructor", null, "openai", null)]
    [InlineData("mai-voice-2", "openai", "openai", null)]
    [InlineData("custom-voice-model", "speech", "speech", "custom-voice-model")]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::API routing::routes %s with api=%s to %s", Coverage = UpstreamCoverage.Covered)]
    public async Task RoutesWithApiTo(string id, string? api, string expected, string? suffix)
    {
        var setup = AzureSpeechModelSetup.Create();
        var providerOptions = api == null ? "{\"azure\":{}}" : "{\"azure\":{\"api\":\"" + api + "\"}}";
        await setup.Provider.Speech(id).GenerateAsync(AzureSpeechModelSetup.Call("Hello", providerOptions), CancellationToken.None);
        var request = setup.Request;
        if (expected == "speech")
        {
            Assert.Equal(AzureSpeechModelSetup.SpeechUrl, request.Url);
            Assert.Equal(AzureSpeechModelSetup.Ssml("en-US-Harper:" + suffix, "Hello"), request.Body);
        }
        else
        {
            Assert.Equal(OpenAIUrl, request.Url);
            Assert.Equal(id, JsonNode.Parse(request.Body)!["model"]!.GetValue<string>());
        }
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::API routing::resolves the API again for each call to the same model", Coverage = UpstreamCoverage.Covered)]
    public async Task ResolvesTheApiAgainForEachCallToTheSameModel()
    {
        var setup = AzureSpeechModelSetup.Create();
        var model = setup.Provider.Speech("mai-voice-2");
        await model.GenerateAsync(AzureSpeechModelSetup.Call("Hi"), CancellationToken.None);
        Assert.Equal(AzureSpeechModelSetup.SpeechUrl, setup.Request.Url);
        await model.GenerateAsync(AzureSpeechModelSetup.Call("Hi", "{\"azure\":{\"api\":\"openai\"}}"), CancellationToken.None);
        Assert.Contains("/audio/speech", setup.Request.Url);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::API routing::exposes speechModel as an alias of speech", Coverage = UpstreamCoverage.Covered)]
    public async Task ExposesSpeechModelAsAnAliasOfSpeech()
    {
        var setup = AzureSpeechModelSetup.Create();
        await setup.Provider.SpeechModel("mai-voice-2").DoGenerateAsync(new SpeechCallOptions("Hello"), CancellationToken.None);
        Assert.Equal(AzureSpeechModelSetup.SpeechUrl, setup.Request.Url);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::API routing::warns about Speech-only options on the OpenAI API", Coverage = UpstreamCoverage.Covered)]
    public async Task WarnsAboutSpeechOnlyOptionsOnTheOpenAIApi()
    {
        var setup = AzureSpeechModelSetup.Create();
        var result = await setup.Provider.Speech("tts-1").GenerateAsync(
            AzureSpeechModelSetup.Call("Hello", "{\"azure\":{\"style\":\"excited\",\"styleDegree\":1.5}}"),
            CancellationToken.None);
        Assert.Collection(
            result.Warnings,
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("providerOptions.azure.style", warning.Feature);
                Assert.Equal("This option requires the Azure Speech API.", warning.Details);
            },
            warning =>
            {
                Assert.Equal("unsupported", warning.Type);
                Assert.Equal("providerOptions.azure.styleDegree", warning.Feature);
                Assert.Equal("This option requires the Azure Speech API.", warning.Details);
            });
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::API routing::accepts OpenAI options under the azure key on the OpenAI API", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task AcceptsOpenAIOptionsUnderTheAzureKeyOnTheOpenAIApi()
    {
        var setup = AzureSpeechModelSetup.Create();
        var result = await setup.Provider.Speech("tts-1").GenerateAsync(
            AzureSpeechModelSetup.Call(
                "Hello",
                "{\"openai\":{\"speed\":2},\"azure\":{\"instructions\":\"Speak calmly\",\"speed\":1.5}}"),
            CancellationToken.None);
        var body = JsonNode.Parse(setup.Request.Body)!;
        Assert.Equal("Speak calmly", body["instructions"]!.GetValue<string>());
        Assert.Equal(1.5, body["speed"]!.GetValue<double>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::API routing::warns about OpenAI-only options on the Speech API", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task WarnsAboutOpenAIOnlyOptionsOnTheSpeechApi()
    {
        var setup = AzureSpeechModelSetup.Create();
        var result = await setup.Provider.Speech("mai-voice-2").GenerateAsync(
            AzureSpeechModelSetup.Call("Hello", "{\"azure\":{\"speed\":1.5}}"),
            CancellationToken.None);
        Assert.Equal(AzureSpeechModelSetup.SpeechUrl, setup.Request.Url);
        Assert.Contains(
            result.Warnings,
            warning => warning.Type == "unsupported"
                && warning.Feature == "providerOptions.azure.speed"
                && warning.Details == "This option requires the OpenAI speech API.");
    }

    [Theory]
    [InlineData("{\"emotion\":\"happy\"}")]
    [InlineData("{\"api\":\"invalid\"}")]
    [InlineData("{\"style\":\"\"}")]
    [InlineData("{\"styleDegree\":0}")]
    [InlineData("{\"styleDegree\":2.5}")]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::API routing::rejects invalid options %j before fetching", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsInvalidOptionsBeforeFetching(string azure)
    {
        var setup = AzureSpeechModelSetup.Create();
        await Assert.ThrowsAsync<InvalidArgumentError>(() => setup.Provider.Speech("mai-voice-2").GenerateAsync(
            AzureSpeechModelSetup.Call("Hello", "{\"azure\":" + azure + "}"),
            CancellationToken.None));
        Assert.Equal(0, setup.Handler.Calls);
    }

    // Not ported: "restores request-time routing after a workflow serialization round trip". The port has no workflow
    // serialization (WORKFLOW_SERIALIZE / WORKFLOW_DESERIALIZE), so there is nothing to round trip.
}
