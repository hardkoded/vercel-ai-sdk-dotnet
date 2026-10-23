// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ElevenLabs;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.ElevenLabsTranscriptionModel;

public sealed class DurationTests
{
    private const string Words = "[{\"text\":\"Hello\",\"type\":\"word\",\"start\":0,\"end\":2.06}]";

    [Theory]
    [InlineData("includes trailing silence", "4.24", Words, 4.24)]
    [InlineData("does not require word timestamps", "4.24", "null", 4.24)]
    [InlineData("preserves a reported zero", "0", Words, 0.0)]
    [InlineData("falls back when audio duration is absent", null, Words, 2.06)]
    [InlineData("falls back when audio duration is null", "null", Words, 2.06)]
    // Ports upstream 762d96e, which is newer than the parity pin, so the catalog has no id to link yet.
    public async Task Duration(string name, string? audioDuration, string words, double expectedDuration)
    {
        Assert.NotEmpty(name);
        var body = "{"
            + (audioDuration is null ? string.Empty : "\"audio_duration_secs\":" + audioDuration + ",")
            + "\"language_code\":\"en\",\"language_probability\":1,\"text\":\"Hello\",\"words\":" + words + "}";
        var handler = new ParityHandler(_ => ParityHandler.Json(body));
        var options = new ElevenLabsOptions { ApiKey = "test-api-key" };
        var model = (ElevenLabsProvider.ElevenLabsTranscriptionModel)ElevenLabsProvider.Create(options, handler).TranscriptionModel("scribe_v1");

        var result = await model.TranscribeAsync(new AudioInput(new byte[] { 1, 2, 3 }, "audio/wav", null), null, CancellationToken.None);

        Assert.Equal(expectedDuration, result.DurationInSeconds);
    }
}
