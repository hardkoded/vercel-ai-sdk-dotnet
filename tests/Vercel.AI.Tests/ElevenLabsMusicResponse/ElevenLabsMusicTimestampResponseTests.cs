// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.ElevenLabs;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.MoreProviders;
using Handler = Vercel.AI.ElevenLabs.ElevenLabsMusicResponse;

namespace Vercel.AI.Tests.ElevenLabsMusicResponse;

public sealed class ElevenLabsMusicTimestampResponseTests
{
    private const string Prefix =
        "packages/elevenlabs/src/elevenlabs-music-response.test.ts::ElevenLabs music timestamp response::";

    private const string Commit = "48ab95d62a08b409311c110852e8083910ba438d";

    private const string DefaultMetadata =
        "{\"words_timestamps\":[{\"word\":\"Hello \\uD83C\\uDF0D\",\"start_ms\":120,\"end_ms\":945}]}";

    private static readonly byte[] Audio = new byte[] { 255, 0, 128, 13, 10 }
        .Concat(Encoding.UTF8.GetBytes("--music-boundary-extra"))
        .Concat(new byte[] { 0, 255, 13, 10 })
        .ToArray();

    private static byte[] Multipart(
        string? json = DefaultMetadata,
        byte[]? audioPart = null,
        bool noAudioPart = false,
        bool noJson = false,
        bool reverse = false,
        bool close = true
    )
    {
        var parts = new List<byte[]>();
        if (!noJson)
        {
            parts.Add(
                Encoding.UTF8.GetBytes(
                    "--music-boundary\r\nContent-Type: application/json\r\n\r\n"
                        + (json ?? DefaultMetadata)
                        + "\r\n"
                )
            );
        }

        if (!noAudioPart)
        {
            parts.Add(
                Encoding
                    .UTF8.GetBytes(
                        "--music-boundary\r\nContent-Type: audio/mpeg\r\nContent-Disposition: attachment; filename=\"music.mp3\"\r\n\r\n"
                    )
                    .Concat(audioPart ?? Audio)
                    .Concat(Encoding.UTF8.GetBytes("\r\n"))
                    .ToArray()
            );
        }

        if (reverse)
        {
            parts.Reverse();
        }

        if (close)
        {
            parts.Add(Encoding.UTF8.GetBytes("--music-boundary--\r\n"));
        }

        return parts.SelectMany(part => part).ToArray();
    }

    private static async Task<JsonResponseHandlerResult<ElevenLabsMusicTimestampsResponse>> Parse(
        byte[] body,
        string contentType = "multipart/mixed; boundary=music-boundary"
    )
    {
        using var response = new HttpResponseMessage { Content = new ByteArrayContent(body) };
        response.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
        response.Headers.TryAddWithoutValidation("song-id", "test-song");
        return await Handler.CreateMusicTimestampsResponseHandler()(
            response,
            "https://api.elevenlabs.io/v1/music/detailed",
            new { },
            CancellationToken.None
        );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Prefix + "preserves binary audio and lyric timings with reverse=%s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Preserves_binary_audio_and_lyric_timings(bool reverse)
    {
        var result = await Parse(Multipart(reverse: reverse));

        Assert.Equal(Audio, result.Value.Audio);
        var word = Assert.Single(result.Value.WordTimestamps!);
        Assert.Equal("Hello 🌍", word.Word);
        Assert.Equal(120, word.StartMs);
        Assert.Equal(945, word.EndMs);
        Assert.Equal(
            DefaultMetadata,
            JsonSerializer.Serialize(
                result.RawValue,
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default,
                }
            )
        );
        Assert.Equal("test-song", result.ResponseHeaders["song-id"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts a quoted boundary and case-insensitive content type",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Accepts_a_quoted_boundary_and_case_insensitive_content_type()
    {
        var result = await Parse(
            Multipart(),
            "Multipart/Mixed; boundary=\"music-boundary\"; charset=utf-8"
        );

        Assert.Equal(Audio, result.Value.Audio);
    }

    [Theory]
    [InlineData("{}", false)]
    [InlineData("{\"words_timestamps\":null}", false)]
    [InlineData("{\"words_timestamps\":[]}", true)]
    [UpstreamTest(
        Prefix + "handles optional lyric metadata: %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Handles_optional_lyric_metadata(string json, bool hasList)
    {
        var result = await Parse(Multipart(json: json));

        Assert.Equal(Audio, result.Value.Audio);
        if (hasList)
        {
            Assert.Empty(result.Value.WordTimestamps!);
        }
        else
        {
            Assert.Null(result.Value.WordTimestamps);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves empty audio for core no-audio handling",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Preserves_empty_audio_for_core_no_audio_handling()
    {
        var result = await Parse(Multipart(audioPart: Array.Empty<byte>()));

        Assert.Empty(result.Value.Audio);
    }

    [Theory]
    [InlineData("metadata")]
    [InlineData("audio")]
    [InlineData("closing boundary")]
    [UpstreamTest(
        Prefix + "rejects a response missing %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_a_response_missing(string missing)
    {
        var body = missing switch
        {
            "metadata" => Multipart(noJson: true),
            "audio" => Multipart(noAudioPart: true),
            _ => Multipart(close: false),
        };

        var error = await Assert.ThrowsAnyAsync<Exception>(() => Parse(body));

        Assert.Contains("complete metadata and audio parts", error.Message);
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("multipart/mixed")]
    [InlineData("multipart/mixed; boundary=\"wrong\"")]
    [UpstreamTest(
        Prefix + "rejects invalid multipart content type %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_invalid_multipart_content_type(string contentType)
    {
        await Assert.ThrowsAnyAsync<Exception>(() => Parse(Multipart(), contentType));
    }

    [Theory]
    [InlineData("{")]
    [InlineData(
        "{\"words_timestamps\":[{\"word\":\"Hello\",\"start_ms\":\"invalid\",\"end_ms\":1}]}"
    )]
    [UpstreamTest(
        Prefix + "rejects malformed lyric metadata: %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_malformed_lyric_metadata(string json)
    {
        await Assert.ThrowsAnyAsync<Exception>(() => Parse(Multipart(json: json)));
    }
}
