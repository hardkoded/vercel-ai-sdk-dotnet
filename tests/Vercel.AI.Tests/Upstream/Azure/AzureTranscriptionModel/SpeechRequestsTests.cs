// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionModel;

/// <summary>Port of <c>azure-transcription-model.test.ts</c> &gt; <c>Speech requests</c>.</summary>
public sealed class SpeechRequestsTests
{
    private const string Prefix = "packages/azure/src/azure-transcription-model.test.ts::Speech requests::";
    private const string SpeechUrl = "https://test-resource.cognitiveservices.azure.com/speechtotext/transcriptions:transcribe?api-version=2025-10-15";

    [Theory]
    [InlineData("bytes")]
    [InlineData("AQID")]
    [UpstreamTest(Prefix + "sends audio and the required model definition", Coverage = UpstreamCoverage.Covered, Note = "A base64 string is decoded before it reaches the model in this port, so the second case decodes it first.", Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Sends_audio_and_the_required_model_definition(string source)
    {
        var harness = Setup();
        var audio = source == "bytes" ? Input : Convert.FromBase64String(source);

        await harness.Transcription("mai-transcribe-2").DoGenerateAsync(Call(audio: audio), CancellationToken.None);

        Assert.Equal(SpeechUrl, harness.Request.Url);
        Assert.Equal("test-key", harness.Request.Headers["Ocp-Apim-Subscription-Key"]);
        Assert.False(harness.Request.Headers.ContainsKey("api-key"));
        Assert.Contains("ai-sdk-azure/", harness.Request.Headers["user-agent"]);
        AssertJson("{\"enhancedMode\":{\"enabled\":true,\"model\":\"MAI-Transcribe-2\",\"modelOptions\":{\"timestamps\":\"segment\"}}}", harness.Definition);
        var file = harness.Request.Parts["audio"];
        Assert.Equal("audio.wav", file.FileName);
        Assert.Equal("audio/wav", file.ContentType);
        Assert.Equal(Input, file.Bytes);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps shared timestampGranularities onto Azure timestamps", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Maps_shared_timestampGranularities_onto_Azure_timestamps()
    {
        var harness = Setup();

        var result = await harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Call(timestampGranularities: new[] { "word", "segment" }),
            CancellationToken.None);

        Assert.Equal("word", harness.Definition["enhancedMode"]!["modelOptions"]!["timestamps"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps segment-only shared timings onto Azure timestamps", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Maps_segment_only_shared_timings_onto_Azure_timestamps()
    {
        var harness = Setup();

        var result = await harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Call(timestampGranularities: new[] { "segment" }),
            CancellationToken.None);

        Assert.Equal("segment", harness.Definition["enhancedMode"]!["modelOptions"]!["timestamps"]!.GetValue<string>());
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("word", "none")]
    [InlineData("word", "segment")]
    [InlineData("segment", "none")]
    [UpstreamTest(Prefix + "warns when Azure timestamps override requested %s timings with %s", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Warns_when_Azure_timestamps_override_requested_timings(string granularity, string timestamps)
    {
        var harness = Setup();

        var result = await harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Call("{\"azure\":{\"timestamps\":\"" + timestamps + "\"}}", timestampGranularities: new[] { granularity }),
            CancellationToken.None);

        Assert.Equal(timestamps, harness.Definition["enhancedMode"]!["modelOptions"]!["timestamps"]!.GetValue<string>());
        Assert.Equal(
            new[]
            {
                ("unsupported", (string?)"timestampGranularities", (string?)("providerOptions.azure.timestamps is \"" + timestamps + "\" and overrides the requested timestamp granularities.")),
            },
            Warnings(result));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("word")]
    [InlineData("segment")]
    [InlineData("none")]
    [UpstreamTest(Prefix + "warns about unsupported shared timings with Azure timestamps=%s", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Warns_about_unsupported_shared_timings_with_Azure_timestamps(string? timestamps)
    {
        var harness = Setup();

        var result = await harness.Transcription("mai-transcribe-1.5").DoGenerateAsync(
            Call(timestamps != null ? "{\"azure\":{\"timestamps\":\"" + timestamps + "\"}}" : "{\"azure\":{}}", timestampGranularities: new[] { "word", "segment" }),
            CancellationToken.None);

        Assert.Equal(timestamps, harness.Definition["enhancedMode"]!["modelOptions"]!["timestamps"]?.GetValue<string>());
        Assert.Equal(
            new[] { ("unsupported", (string?)"timestampGranularities", (string?)"MAI-Transcribe-1.5 cannot return word or segment timings.") },
            Warnings(result));
    }

    [Theory]
    [InlineData("word")]
    [InlineData("segment")]
    [InlineData("none")]
    [UpstreamTest(Prefix + "serializes all options with %s timestamps", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Serializes_all_options_with_timestamps(string timestamps)
    {
        var harness = Setup();
        var azure = "{\"azure\":{\"timestamps\":\"" + timestamps + "\",\"transcribeStyle\":\"clean\",\"locales\":[\"en\"],\"diarization\":{\"enabled\":true},\"phraseList\":{\"phrases\":[\"Vercel\"]}}}";

        var result = await harness.Transcription("mai-transcribe-2").DoGenerateAsync(Call(azure), CancellationToken.None);

        Assert.Empty(result.Warnings);
        AssertJson(
            "{\"enhancedMode\":{\"enabled\":true,\"model\":\"MAI-Transcribe-2\",\"modelOptions\":{\"timestamps\":\"" + timestamps + "\",\"transcribeStyle\":\"clean\"}},"
            + "\"locales\":[\"en\"],\"diarization\":{\"enabled\":true},\"phraseList\":{\"phrases\":[\"Vercel\"]}}",
            harness.Definition);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects a resourceName that would rewrite the Speech host", Coverage = UpstreamCoverage.Covered, Note = "The provider rejects the resource name when it is created, before any model call. No request is sent.", Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public void Rejects_a_resourceName_that_would_rewrite_the_Speech_host()
    {
        var handler = new AzureTranscriptionHandler(_ => Json("{}"));

        var error = Assert.Throws<ArgumentException>(() => AzureOpenAIProvider.Create(
            new AzureOpenAIOptions { ApiKey = "test-key", ResourceName = "user@internal:8080/#" },
            handler));

        Assert.Equal("ResourceName", error.ParamName);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    [UpstreamTest(Prefix + "loads resource and key from the existing environment variables", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Loads_resource_and_key_from_the_existing_environment_variables()
    {
        var resource = Environment.GetEnvironmentVariable("AZURE_RESOURCE_NAME");
        var key = Environment.GetEnvironmentVariable("AZURE_API_KEY");
        Environment.SetEnvironmentVariable("AZURE_RESOURCE_NAME", "environment-resource");
        Environment.SetEnvironmentVariable("AZURE_API_KEY", "environment-key");
        try
        {
            var harness = Setup(options => options.ApiKey = null, defaultResource: false);

            await harness.Transcription("mai-transcribe-2").DoGenerateAsync(Call(), CancellationToken.None);

            Assert.Contains("https://environment-resource.cognitiveservices.azure.com/", harness.Request.Url);
            Assert.Equal("environment-key", harness.Request.Headers["Ocp-Apim-Subscription-Key"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("AZURE_RESOURCE_NAME", resource);
            Environment.SetEnvironmentVariable("AZURE_API_KEY", key);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "honors speechBaseURL, headers, and abort overrides", Coverage = UpstreamCoverage.Covered, Note = "The abort signal is a CancellationToken, so the test observes it through a cancelled call instead of comparing signal identity.", Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Honors_speechBaseURL_headers_and_abort_overrides()
    {
        var harness = Setup(options =>
        {
            options.SpeechBaseUrl = "https://eastus.api.cognitive.microsoft.com/";
            options.Headers["x-custom"] = "provider";
        });
        using var cancellation = new CancellationTokenSource();

        await harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Call(headers: new Dictionary<string, string> { ["x-custom"] = "call" }, cancellationToken: cancellation.Token),
            cancellation.Token);

        Assert.Equal("https://eastus.api.cognitive.microsoft.com/speechtotext/transcriptions:transcribe?api-version=2025-10-15", harness.Request.Url);
        Assert.Equal("call", harness.Request.Headers["x-custom"]);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Call(cancellationToken: cancellation.Token),
            cancellation.Token));
        Assert.Single(harness.Handler.Requests);
    }

    [Fact]
    [UpstreamTest(Prefix + "does not require resourceName when speechBaseURL is set", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Does_not_require_resourceName_when_speechBaseURL_is_set()
    {
        var harness = Setup(options => options.SpeechBaseUrl = "https://eastus.api.cognitive.microsoft.com", defaultResource: false);

        await harness.Transcription("mai-transcribe-2").DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Contains("https://eastus.api.cognitive.microsoft.com/speechtotext/", harness.Request.Url);
    }

    [Fact]
    [UpstreamTest(Prefix + "does not apply Azure OpenAI baseURL or apiVersion to Speech requests", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Does_not_apply_Azure_OpenAI_baseURL_or_apiVersion_to_Speech_requests()
    {
        var harness = Setup(options =>
        {
            options.BaseUrl = "https://test-resource.openai.azure.com";
            options.ApiVersion = "2025-04-01-preview";
            options.UseDeploymentBasedUrls = true;
        });

        await harness.Transcription("mai-transcribe-2").DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(SpeechUrl, harness.Request.Url);
    }

    [Fact]
    [UpstreamTest(Prefix + "gets a fresh bearer token for each request", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Gets_a_fresh_bearer_token_for_each_request()
    {
        var tokens = new Queue<string>(new[] { "first", "second" });
        var harness = Setup(options =>
        {
            options.ApiKey = null;
            options.TokenProvider = () => tokens.Dequeue();
        });
        var model = harness.Transcription("mai-transcribe-2");

        await model.DoGenerateAsync(Call(), CancellationToken.None);
        Assert.Equal("Bearer first", harness.Request.Headers["authorization"]);
        Assert.False(harness.Request.Headers.ContainsKey("Ocp-Apim-Subscription-Key"));

        await model.DoGenerateAsync(Call(), CancellationToken.None);
        Assert.Equal("Bearer second", harness.Request.Headers["authorization"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "does not replace an explicitly supplied bearer token", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Does_not_replace_an_explicitly_supplied_bearer_token()
    {
        var calls = 0;
        var harness = Setup(options =>
        {
            options.ApiKey = null;
            options.TokenProvider = () =>
            {
                calls++;
                return "unused";
            };
        });

        await harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Call(headers: new Dictionary<string, string> { ["Authorization"] = "Bearer custom" }),
            CancellationToken.None);

        Assert.Equal("Bearer custom", harness.Request.Headers["authorization"]);
        Assert.Equal(0, calls);
    }
}
