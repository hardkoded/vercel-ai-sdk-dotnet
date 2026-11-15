// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using TranscriptionModel = Vercel.AI.OpenAI.OpenAITranscriptionModel;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAITranscriptionModel;

/// <summary>
/// Port of <c>openai-transcription-model.test.ts</c> &gt; <c>doGenerate</c>. The other tests of this <c>describe</c> are in
/// <c>OpenAIMediaUpstreamTests</c>.
/// </summary>
public sealed class DoGenerateTests
{
    private const string Prefix = "packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate::";

    private static OpenAITranscriptionCall Call(string? openai = null)
    {
        var call = new OpenAITranscriptionCall(new AudioInput(new byte[] { 1, 2, 3 }, "audio/mpeg", "audio.mp3"));
        if (openai != null)
        {
            call.ProviderOptions = OpenAIUpstream.Json("{\"openai\":" + openai + "}");
        }

        return call;
    }

    private static string Text(OpenAIMultipartPart part)
    {
        return System.Text.Encoding.UTF8.GetString(part.Data);
    }

    [Theory]
    [InlineData("gpt-live-transcribe")]
    [InlineData("gpt-live-transcribe-2026-01-01")]
    [UpstreamTest(Prefix + "should reject %s for non-streaming transcription", Coverage = UpstreamCoverage.Covered, Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3")]
    public async Task Should_reject_model_for_non_streaming_transcription(string modelId)
    {
        var model = new TranscriptionModel(OpenAIUpstream.Provider(new OpenAICapture()), modelId);

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => model.TranscribeAsync(Call(), CancellationToken.None));
    }

    [Fact]
    [UpstreamTest(Prefix + "should send gpt-transcribe to the REST transcription endpoint", Coverage = UpstreamCoverage.Covered, Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3")]
    public async Task Should_send_gpt_transcribe_to_the_REST_transcription_endpoint()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"text\":\"Hello world\"}" };

        await new TranscriptionModel(OpenAIUpstream.Provider(capture), "gpt-transcribe").TranscribeAsync(Call(), CancellationToken.None);

        Assert.EndsWith("/audio/transcriptions", capture.Uri, StringComparison.Ordinal);
        Assert.Equal("gpt-transcribe", Text(capture.Parts.Single(part => part.Name == "model")));
    }

    [Theory]
    [InlineData("gpt-transcribe")]
    [InlineData("openai/gpt-transcribe")]
    [InlineData("custom-transcription-model")]
    [UpstreamTest(Prefix + "should send language and vocabulary hints for %s", Coverage = UpstreamCoverage.Covered, Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3")]
    public async Task Should_send_language_and_vocabulary_hints_for_model(string modelId)
    {
        var capture = new OpenAICapture { ResponseJson = "{\"text\":\"Hello world\"}" };

        var result = await new TranscriptionModel(OpenAIUpstream.Provider(capture), modelId).TranscribeAsync(
            Call("{\"languages\":[\"en\",\"fr\"],\"keywords\":[\"premium plan\",\"AC-42\"],\"prompt\":\"A customer support call.\"}"),
            CancellationToken.None);

        Assert.Equal(new[] { "en", "fr" }, capture.Parts.Where(part => part.Name == "languages[]").Select(Text).ToArray());
        Assert.Equal(new[] { "premium plan", "AC-42" }, capture.Parts.Where(part => part.Name == "keywords[]").Select(Text).ToArray());
        Assert.Equal("A customer support call.", Text(capture.Parts.Single(part => part.Name == "prompt")));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should omit empty hint arrays while preserving the singular language option", Coverage = UpstreamCoverage.Covered, Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3")]
    public async Task Should_omit_empty_hint_arrays_while_preserving_the_singular_language_option()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"text\":\"Hello world\"}" };

        await new TranscriptionModel(OpenAIUpstream.Provider(capture), "whisper-1").TranscribeAsync(
            Call("{\"language\":\"en\",\"languages\":[],\"keywords\":[]}"),
            CancellationToken.None);

        Assert.Equal("en", Text(capture.Parts.Single(part => part.Name == "language")));
        Assert.DoesNotContain(capture.Parts, part => part.Name == "languages[]");
        Assert.DoesNotContain(capture.Parts, part => part.Name == "keywords[]");
    }
}
