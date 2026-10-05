// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class GeneratedAudioTests
{
    [Fact]
    public void ShouldThrowAnSdkErrorWhenTheAudioSubtypeIsEmpty()
    {
        var error = Assert.ThrowsAny<AiSdkException>(() => new GeneratedAudio(new byte[] { 0 }, "audio/"));
        var invalid = Assert.IsType<InvalidResponseDataException>(error);
        Assert.Equal("audio/", invalid.Data);
        Assert.Equal("Could not determine audio format from media type: audio/", invalid.Message);
    }

    [Theory]
    [InlineData("audio/mpeg", "mp3")]
    [InlineData("audio/mp3", "mp3")]
    [InlineData("audio/wav", "wav")]
    [InlineData("audio/ogg", "ogg")]
    [InlineData("", "mp3")]
    [InlineData("audio", "mp3")]
    [InlineData("audio/mp3/extra", "mp3")]
    public void ShouldDeriveFormatFromMediaType(string mediaType, string format)
    {
        var data = new byte[] { 0 };
        var audio = new GeneratedAudio(data, mediaType);
        Assert.Equal(format, audio.Format);
        Assert.Equal(mediaType, audio.MediaType);
        Assert.Same(data, audio.Data);
    }
}
