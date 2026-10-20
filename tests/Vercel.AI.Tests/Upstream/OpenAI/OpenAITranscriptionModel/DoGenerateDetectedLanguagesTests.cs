// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using TranscriptionModel = Vercel.AI.OpenAI.OpenAITranscriptionModel;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAITranscriptionModel;

/// <summary>Port of <c>openai-transcription-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>detected languages</c>.</summary>
public sealed class DoGenerateDetectedLanguagesTests
{
    public static TheoryData<string, string?> Cases => new()
    {
        { "{\"language\":\"english\"}", "en" },
        { "{\"language\":\"en\"}", "en" },
        { "{\"language\":\"bn\"}", "bn" },
        { "{\"language\":\"unknown\"}", null },
        { "{\"language\":\"en-US\"}", null },
        { "{\"language\":\"constructor\"}", null },
        { "{\"languages\":[{\"code\":\"en\"}]}", "en" },
        { "{\"languages\":[{\"code\":\"fr\"}]}", "fr" },
        { "{\"languages\":[{\"code\":\"en\"},{\"code\":\"fr\"}]}", null },
        { "{\"languages\":[{\"code\":\"en-US\"}]}", null },
        { "{\"languages\":[{\"code\":\"english\"}]}", null },
        { "{\"languages\":[]}", null },
        { "{\"language\":null,\"languages\":null}", null },
        { "{\"language\":\"unknown\",\"languages\":[{\"code\":\"en\"}]}", "en" },
        { "{\"language\":\"french\",\"languages\":[{\"code\":\"en\"}]}", "fr" },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate > detected languages::normalizes $language / $languages to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Normalizes_language_and_languages(string fields, string? expected)
    {
        var body = "{\"text\":\"Hello world\"," + fields.Substring(1);
        var capture = new OpenAICapture { ResponseJson = body };
        var call = new OpenAITranscriptionCall(new AudioInput(new byte[] { 1, 2, 3 }, "audio/mpeg", "audio.mp3"))
        {
            ProviderOptions = OpenAIUpstream.Json("{\"openai\":{\"responseFormat\":\"json\",\"timestampGranularities\":[]}}"),
        };

        var result = await new TranscriptionModel(OpenAIUpstream.Provider(capture), "gpt-transcribe").TranscribeAsync(call, CancellationToken.None);

        Assert.Equal(expected, result.Language);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(body), JsonNode.Parse(result.Response.Body)));
        var languages = JsonNode.Parse(body)!["languages"];
        if (languages == null)
        {
            Assert.Null(result.ProviderMetadata);
        }
        else
        {
            var expectedMetadata = new JsonObject { ["openai"] = new JsonObject { ["languages"] = languages.DeepClone() } };
            Assert.True(JsonNode.DeepEquals(expectedMetadata, JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
        }
    }

    [Fact]
    [UpstreamTest("packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate > detected languages::preserves detected languages alongside diarized segments", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_detected_languages_alongside_diarized_segments()
    {
        var capture = new OpenAICapture
        {
            ResponseJson = "{\"text\":\"Hello world\",\"languages\":[{\"code\":\"en\"},{\"code\":\"fr-CA\"}],\"segments\":[{\"type\":\"transcript.text.segment\",\"id\":\"segment-0\",\"start\":0,\"end\":1,\"text\":\"Hello world\",\"speaker\":\"A\"}]}",
        };
        var call = new OpenAITranscriptionCall(new AudioInput(new byte[] { 1, 2, 3 }, "audio/mpeg", "audio.mp3"));

        var result = await new TranscriptionModel(OpenAIUpstream.Provider(capture), "gpt-4o-transcribe-diarize").TranscribeAsync(call, CancellationToken.None);

        Assert.Null(result.Language);
        var expected = JsonNode.Parse("{\"openai\":{\"languages\":[{\"code\":\"en\"},{\"code\":\"fr-CA\"}],\"segments\":[{\"text\":\"Hello world\",\"startSecond\":0,\"endSecond\":1,\"speaker\":\"A\"}]}}");
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
    }
}
