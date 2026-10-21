// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.RevAI;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Upstream.RevAI.RevAITranscriptionModel;

/// <summary>Port of <c>revai-transcription-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>transcription</c>.</summary>
public sealed class DoGenerateTranscriptionTests
{
    private const string Prefix = "packages/revai/src/revai-transcription-model.test.ts::doGenerate > transcription::";
    private const string Transcript = "{\"monologues\":[{\"speaker\":0,\"elements\":[{\"type\":\"text\",\"value\":\"Hello\",\"ts\":0,\"end_ts\":1},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"world\",\"ts\":1,\"end_ts\":2}]}]}";

    [Fact]
    [UpstreamTest(Prefix + "should encode the job ID as a single URL path segment in the poll and transcript URLs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_encode_the_job_ID_as_a_single_URL_path_segment_in_the_poll_and_transcript_URLs()
    {
        var jobId = "abc/../../internal";
        var encodedJobId = Uri.EscapeDataString(jobId);
        var handler = new ParityHandler(call =>
        {
            if (call.Uri.AbsolutePath.EndsWith("/transcript", StringComparison.Ordinal))
            {
                return ParityHandler.Json(Transcript);
            }

            return call.Method == HttpMethod.Get
                ? ParityHandler.Json("{\"id\":\"" + jobId + "\",\"status\":\"transcribed\",\"language\":\"en\"}")
                : ParityHandler.Json("{\"id\":\"" + jobId + "\",\"status\":\"in_progress\",\"type\":\"async\",\"language\":\"en\"}");
        });
        var provider = RevAIProvider.Create(new RevAIOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        var audio = new AudioInput(new byte[] { 1, 2, 3, 4 }, "audio/mp3", "audio.mp3");

        await ((RevAIProvider.RevAITranscriptionModel)provider.TranscriptionModel("machine")).TranscribeAsync(audio, null, CancellationToken.None);

        Assert.Equal("https://api.rev.ai/speechtotext/v1/jobs/" + encodedJobId, handler.Calls[1].Uri.AbsoluteUri);
        Assert.Equal("https://api.rev.ai/speechtotext/v1/jobs/" + encodedJobId + "/transcript", handler.Calls[2].Uri.AbsoluteUri);
    }
}
