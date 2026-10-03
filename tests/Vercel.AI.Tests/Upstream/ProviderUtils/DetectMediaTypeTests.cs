// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class DetectMediaTypeTests
{
    private const string ResolveFull = "packages/provider-utils/src/resolve-full-media-type.test.ts::resolveFullMediaType::";

    private static readonly byte[] Webp = Bytes(0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50, 0x56, 0x50, 0x38, 0x20);
    private static readonly byte[] Wav = Bytes(0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45, 0x66, 0x6d, 0x74, 0x20);
    private static readonly byte[] Bmp = Bytes(0x42, 0x4d, 0x36, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);
    private static readonly byte[] Png = Bytes(0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a);
    private static readonly byte[] Pdf = Bytes(0x25, 0x50, 0x44, 0x46, 0x2d, 0x31, 0x2e, 0x34);

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > GIF::should detect GIF from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_gif_bytes()
    {
        Assert.Equal("image/gif", MediaTypes.DetectMediaType(Bytes(0x47, 0x49, 0x46, 0x38, 0x39, 0x61), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > GIF::should detect GIF from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_gif_base64()
    {
        Assert.Equal("image/gif", MediaTypes.DetectMediaType(ByteEncoding.ToBase64(Bytes(0x47, 0x49, 0x46, 0x38, 0x37, 0x61)), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > GIF::should not detect text that only starts with GIF", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_detect_gif_text()
    {
        Assert.Null(MediaTypes.DetectMediaType(Encoding.UTF8.GetBytes("GIF support notes"), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > PNG::should detect PNG from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_png_bytes()
    {
        Assert.Equal("image/png", MediaTypes.DetectMediaType(Bytes(0x89, 0x50, 0x4e, 0x47, 0xff, 0xff), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > PNG::should detect PNG from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_png_base64()
    {
        Assert.Equal("image/png", MediaTypes.DetectMediaType("iVBORwabc123", "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > JPEG::should detect JPEG from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_jpeg_bytes()
    {
        Assert.Equal("image/jpeg", MediaTypes.DetectMediaType(Bytes(0xff, 0xd8, 0xff, 0xff), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > JPEG::should detect JPEG from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_jpeg_base64()
    {
        Assert.Equal("image/jpeg", MediaTypes.DetectMediaType("/9j/abc123", "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > WebP::should detect WebP from bytes (positive webp image uint8)", Coverage = UpstreamCoverage.Covered)]
    public void Detects_webp_bytes()
    {
        Assert.Equal("image/webp", MediaTypes.DetectMediaType(Webp, "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > WebP::should detect WebP from base64 (positive webp image base64)", Coverage = UpstreamCoverage.Covered)]
    public void Detects_webp_base64()
    {
        Assert.Equal("image/webp", MediaTypes.DetectMediaType(ByteEncoding.ToBase64(Webp), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > WebP::should NOT detect RIFF audio as WebP from bytes (negative riff audio uint8)", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_detect_wav_as_webp()
    {
        Assert.Null(MediaTypes.DetectMediaType(Wav, "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > WebP::should NOT detect RIFF audio as WebP from base64 (negative riff audio base64)", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_detect_wav_base64_as_webp()
    {
        Assert.Null(MediaTypes.DetectMediaType(ByteEncoding.ToBase64(Wav), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > BMP::should detect BMP from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_bmp_bytes()
    {
        Assert.Equal("image/bmp", MediaTypes.DetectMediaType(Bmp, "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > BMP::should detect BMP from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_bmp_base64()
    {
        Assert.Equal("image/bmp", MediaTypes.DetectMediaType(ByteEncoding.ToBase64(Bmp), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > BMP::should not detect text that only starts with BM", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_detect_bm_text()
    {
        Assert.Null(MediaTypes.DetectMediaType(Encoding.UTF8.GetBytes("BM25 ranking notes"), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > TIFF::should detect TIFF (little endian) from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_tiff_le_bytes()
    {
        Assert.Equal("image/tiff", MediaTypes.DetectMediaType(Bytes(0x49, 0x49, 0x2a, 0x00, 0xff), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > TIFF::should detect TIFF (little endian) from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_tiff_le_base64()
    {
        Assert.Equal("image/tiff", MediaTypes.DetectMediaType("SUkqAAabc123", "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > TIFF::should detect TIFF (big endian) from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_tiff_be_bytes()
    {
        Assert.Equal("image/tiff", MediaTypes.DetectMediaType(Bytes(0x4d, 0x4d, 0x00, 0x2a, 0xff), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > TIFF::should detect TIFF (big endian) from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_tiff_be_base64()
    {
        Assert.Equal("image/tiff", MediaTypes.DetectMediaType("TU0AKgabc123", "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > AVIF::should detect AVIF with a %s ftyp box from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_avif_box_sizes()
    {
        foreach (var size in new[] { 0x1c, 0x20 })
        {
            var bytes = Bytes(0x00, 0x00, 0x00, size, 0x66, 0x74, 0x79, 0x70, 0x61, 0x76, 0x69, 0x66);
            Assert.Equal("image/avif", MediaTypes.DetectMediaType(bytes, "image"));
            Assert.Equal("image/avif", MediaTypes.DetectMediaType(bytes));
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > AVIF::should detect AVIF with a 28-byte ftyp box from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_avif_base64()
    {
        var encoded = ByteEncoding.ToBase64(Bytes(0x00, 0x00, 0x00, 0x1c, 0x66, 0x74, 0x79, 0x70, 0x61, 0x76, 0x69, 0x66));
        Assert.Equal("image/avif", MediaTypes.DetectMediaType(encoded, "image"));
        Assert.Equal("image/avif", MediaTypes.DetectMediaType(encoded));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > HEIC::should detect HEIC with a %s ftyp box from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_heic_box_sizes()
    {
        foreach (var size in new[] { 0x1c, 0x20 })
        {
            var bytes = Bytes(0x00, 0x00, 0x00, size, 0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63);
            Assert.Equal("image/heic", MediaTypes.DetectMediaType(bytes, "image"));
            Assert.Equal("image/heic", MediaTypes.DetectMediaType(bytes));
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > HEIC::should detect HEIC with a 28-byte ftyp box from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_heic_base64()
    {
        var encoded = ByteEncoding.ToBase64(Bytes(0x00, 0x00, 0x00, 0x1c, 0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63));
        Assert.Equal("image/heic", MediaTypes.DetectMediaType(encoded, "image"));
        Assert.Equal("image/heic", MediaTypes.DetectMediaType(encoded));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > MP3::should detect MP3 from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_mp3_bytes()
    {
        Assert.Equal("audio/mpeg", MediaTypes.DetectMediaType(Bytes(0xff, 0xfb), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > MP3::should detect MP3 from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_mp3_base64()
    {
        Assert.Equal("audio/mpeg", MediaTypes.DetectMediaType("//s=", "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > MP3::should detect MP3 with ID3v2 tags from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_id3_mp3_bytes()
    {
        Assert.Equal("audio/mpeg", MediaTypes.DetectMediaType(Id3Mp3(10), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > MP3::should detect MP3 with ID3v2 tags from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_id3_mp3_base64()
    {
        Assert.Equal("audio/mpeg", MediaTypes.DetectMediaType(ByteEncoding.ToBase64(Id3Mp3(10)), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > MP3::detects an ID3-tagged MP3 whose tag is at the scan limit", Coverage = UpstreamCoverage.Covered)]
    public void Detects_an_id3_tag_at_the_scan_limit()
    {
        var bytes = Id3Mp3(MediaTypes.MaxId3TagBytes);
        Assert.Equal("audio/mpeg", MediaTypes.DetectMediaType(bytes, "audio"));
        Assert.Equal("audio/mpeg", MediaTypes.DetectMediaType(ByteEncoding.ToBase64(bytes), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > MP3::does not detect an ID3-tagged MP3 whose tag exceeds the scan limit", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_detect_an_id3_tag_past_the_scan_limit()
    {
        var bytes = Id3Mp3(MediaTypes.MaxId3TagBytes + 1);
        Assert.Null(MediaTypes.DetectMediaType(bytes, "audio"));
        Assert.Null(MediaTypes.DetectMediaType(ByteEncoding.ToBase64(bytes), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > WAV::should detect WAV from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_wav_bytes()
    {
        Assert.Equal("audio/wav", MediaTypes.DetectMediaType(Wav, "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > WAV::should detect WAV from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_wav_base64()
    {
        Assert.Equal("audio/wav", MediaTypes.DetectMediaType(ByteEncoding.ToBase64(Wav), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > WAV::should NOT detect WebP as WAV from bytes (negative webp image uint8)", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_detect_webp_as_wav()
    {
        Assert.Null(MediaTypes.DetectMediaType(Webp, "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > WAV::should NOT detect WebP as WAV from base64 (negative webp image base64)", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_detect_webp_base64_as_wav()
    {
        Assert.Null(MediaTypes.DetectMediaType(ByteEncoding.ToBase64(Webp), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > OGG::should detect OGG from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_ogg_bytes()
    {
        Assert.Equal("audio/ogg", MediaTypes.DetectMediaType(Bytes(0x4f, 0x67, 0x67, 0x53), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > OGG::should detect OGG from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_ogg_base64()
    {
        Assert.Equal("audio/ogg", MediaTypes.DetectMediaType("T2dnUw", "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > FLAC::should detect FLAC from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_flac_bytes()
    {
        Assert.Equal("audio/flac", MediaTypes.DetectMediaType(Bytes(0x66, 0x4c, 0x61, 0x43), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > FLAC::should detect FLAC from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_flac_base64()
    {
        Assert.Equal("audio/flac", MediaTypes.DetectMediaType("ZkxhQw", "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > AAC::should detect $name from bytes and base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_aac_adts_headers()
    {
        foreach (var header in new[] { new[] { 0xff, 0xf0 }, new[] { 0xff, 0xf1 }, new[] { 0xff, 0xf8 }, new[] { 0xff, 0xf9 } })
        {
            var bytes = Bytes(header[0], header[1], 0x50, 0x40);
            Assert.Equal("audio/aac", MediaTypes.DetectMediaType(bytes, "audio"));
            Assert.Equal("audio/aac", MediaTypes.DetectMediaType(ByteEncoding.ToBase64(bytes), "audio"));
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > AAC::should detect ID3-tagged ADTS AAC from bytes and base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_id3_aac()
    {
        var bytes = Bytes(0x49, 0x44, 0x33, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xf1, 0x50, 0x40);
        Assert.Equal("audio/aac", MediaTypes.DetectMediaType(bytes, "audio"));
        Assert.Equal("audio/aac", MediaTypes.DetectMediaType(ByteEncoding.ToBase64(bytes), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > AAC::should detect AAC from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_aac_bytes()
    {
        Assert.Equal("audio/aac", MediaTypes.DetectMediaType(Bytes(0x40, 0x15, 0x00, 0x00), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > AAC::should detect AAC from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_aac_base64()
    {
        Assert.Equal("audio/aac", MediaTypes.DetectMediaType(ByteEncoding.ToBase64(Bytes(0x40, 0x15, 0x00, 0x00)), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > MP4::should detect MP4 from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_mp4_audio_bytes()
    {
        Assert.Equal("audio/mp4", MediaTypes.DetectMediaType(Bytes(0x00, 0x00, 0x00, 0x1c, 0x66, 0x74, 0x79, 0x70, 0x4d, 0x34, 0x41, 0x20), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > MP4::should detect MP4 from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_mp4_audio_base64()
    {
        Assert.Equal("audio/mp4", MediaTypes.DetectMediaType(ByteEncoding.ToBase64(Bytes(0x00, 0x00, 0x00, 0x1c, 0x66, 0x74, 0x79, 0x70, 0x4d, 0x34, 0x41, 0x20)), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > WEBM::should detect WEBM from bytes", Coverage = UpstreamCoverage.Covered)]
    public void Detects_webm_bytes()
    {
        Assert.Equal("audio/webm", MediaTypes.DetectMediaType(Bytes(0x1a, 0x45, 0xdf, 0xa3), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > WEBM::should detect WEBM from base64", Coverage = UpstreamCoverage.Covered)]
    public void Detects_webm_base64()
    {
        Assert.Equal("audio/webm", MediaTypes.DetectMediaType("GkXfow==", "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > error cases::should return undefined for unknown image formats", Coverage = UpstreamCoverage.Covered)]
    public void Unknown_image_is_undefined()
    {
        Assert.Null(MediaTypes.DetectMediaType(Bytes(0x00, 0x01, 0x02, 0x03), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > error cases::should return undefined for unknown audio formats", Coverage = UpstreamCoverage.Covered)]
    public void Unknown_audio_is_undefined()
    {
        Assert.Null(MediaTypes.DetectMediaType(Bytes(0x00, 0x01, 0x02, 0x03), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > error cases::should return undefined for empty arrays for image", Coverage = UpstreamCoverage.Covered)]
    public void Empty_image_is_undefined()
    {
        Assert.Null(MediaTypes.DetectMediaType(Array.Empty<byte>(), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > error cases::should return undefined for empty arrays for audio", Coverage = UpstreamCoverage.Covered)]
    public void Empty_audio_is_undefined()
    {
        Assert.Null(MediaTypes.DetectMediaType(Array.Empty<byte>(), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > error cases::should return undefined for arrays shorter than signature length for image", Coverage = UpstreamCoverage.Covered)]
    public void Short_png_is_undefined()
    {
        Assert.Null(MediaTypes.DetectMediaType(Bytes(0x89, 0x50), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > error cases::should return undefined for arrays shorter than signature length for audio", Coverage = UpstreamCoverage.Covered)]
    public void Short_ogg_is_undefined()
    {
        Assert.Null(MediaTypes.DetectMediaType(Bytes(0x4f, 0x67), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > error cases::should return undefined for invalid base64 strings for image", Coverage = UpstreamCoverage.Covered)]
    public void Invalid_image_base64_is_undefined()
    {
        Assert.Null(MediaTypes.DetectMediaType("invalid123", "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType signature matching > error cases::should return undefined for invalid base64 strings for audio", Coverage = UpstreamCoverage.Covered)]
    public void Invalid_audio_base64_is_undefined()
    {
        Assert.Null(MediaTypes.DetectMediaType("invalid123", "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::getTopLevelMediaType::returns the top-level segment for a full media type", Coverage = UpstreamCoverage.Covered)]
    public void Top_level_media_type_returns_the_segment()
    {
        Assert.Equal("image", MediaTypes.GetTopLevelMediaType("image/png"));
        Assert.Equal("audio", MediaTypes.GetTopLevelMediaType("audio/mpeg"));
        Assert.Equal("video", MediaTypes.GetTopLevelMediaType("video/mp4"));
        Assert.Equal("application", MediaTypes.GetTopLevelMediaType("application/pdf"));
        Assert.Equal("text", MediaTypes.GetTopLevelMediaType("text/plain"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::getTopLevelMediaType::returns the input when it is already just a top-level segment", Coverage = UpstreamCoverage.Covered)]
    public void Top_level_media_type_keeps_a_segment()
    {
        Assert.Equal("image", MediaTypes.GetTopLevelMediaType("image"));
        Assert.Equal("audio", MediaTypes.GetTopLevelMediaType("audio"));
        Assert.Equal("video", MediaTypes.GetTopLevelMediaType("video"));
        Assert.Equal("application", MediaTypes.GetTopLevelMediaType("application"));
        Assert.Equal("text", MediaTypes.GetTopLevelMediaType("text"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::getTopLevelMediaType::normalizes *-subtype wildcards to the top-level segment", Coverage = UpstreamCoverage.Covered)]
    public void Top_level_media_type_strips_a_wildcard()
    {
        Assert.Equal("image", MediaTypes.GetTopLevelMediaType("image/*"));
        Assert.Equal("audio", MediaTypes.GetTopLevelMediaType("audio/*"));
        Assert.Equal("video", MediaTypes.GetTopLevelMediaType("video/*"));
        Assert.Equal("application", MediaTypes.GetTopLevelMediaType("application/*"));
        Assert.Equal("text", MediaTypes.GetTopLevelMediaType("text/*"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::getTopLevelMediaType::handles edge cases", Coverage = UpstreamCoverage.Covered)]
    public void Top_level_media_type_handles_edges()
    {
        Assert.Equal(string.Empty, MediaTypes.GetTopLevelMediaType(string.Empty));
        Assert.Equal(string.Empty, MediaTypes.GetTopLevelMediaType("/"));
        Assert.Equal("image", MediaTypes.GetTopLevelMediaType("image/"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::isFullMediaType::returns true for media types with a concrete subtype", Coverage = UpstreamCoverage.Covered)]
    public void Full_media_type_accepts_a_subtype()
    {
        Assert.True(MediaTypes.IsFullMediaType("image/png"));
        Assert.True(MediaTypes.IsFullMediaType("audio/mpeg"));
        Assert.True(MediaTypes.IsFullMediaType("video/mp4"));
        Assert.True(MediaTypes.IsFullMediaType("application/pdf"));
        Assert.True(MediaTypes.IsFullMediaType("text/plain"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::isFullMediaType::returns false for top-level-only media types", Coverage = UpstreamCoverage.Covered)]
    public void Full_media_type_rejects_a_segment()
    {
        Assert.False(MediaTypes.IsFullMediaType("image"));
        Assert.False(MediaTypes.IsFullMediaType("audio"));
        Assert.False(MediaTypes.IsFullMediaType("video"));
        Assert.False(MediaTypes.IsFullMediaType("application"));
        Assert.False(MediaTypes.IsFullMediaType("text"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::isFullMediaType::returns false for *-subtype wildcards", Coverage = UpstreamCoverage.Covered)]
    public void Full_media_type_rejects_a_wildcard()
    {
        Assert.False(MediaTypes.IsFullMediaType("image/*"));
        Assert.False(MediaTypes.IsFullMediaType("audio/*"));
        Assert.False(MediaTypes.IsFullMediaType("video/*"));
        Assert.False(MediaTypes.IsFullMediaType("application/*"));
        Assert.False(MediaTypes.IsFullMediaType("text/*"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::isFullMediaType::returns false for edge cases", Coverage = UpstreamCoverage.Covered)]
    public void Full_media_type_rejects_edges()
    {
        Assert.False(MediaTypes.IsFullMediaType(string.Empty));
        Assert.False(MediaTypes.IsFullMediaType("/"));
        Assert.False(MediaTypes.IsFullMediaType("image/"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType::detects image types when topLevelType is \"image\"", Coverage = UpstreamCoverage.Covered)]
    public void Detects_an_image_segment()
    {
        Assert.Equal("image/png", MediaTypes.DetectMediaType(Bytes(0x89, 0x50, 0x4e, 0x47, 0xff, 0xff), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType::detects audio types when topLevelType is \"audio\"", Coverage = UpstreamCoverage.Covered)]
    public void Detects_an_audio_segment()
    {
        Assert.Equal("audio/mpeg", MediaTypes.DetectMediaType(Bytes(0xff, 0xfb), "audio"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType::detects video types when topLevelType is \"video\"", Coverage = UpstreamCoverage.Covered)]
    public void Detects_a_video_segment()
    {
        Assert.Equal("video/webm", MediaTypes.DetectMediaType(Bytes(0x1a, 0x45, 0xdf, 0xa3), "video"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType::detects document types when topLevelType is \"application\"", Coverage = UpstreamCoverage.Covered)]
    public void Detects_a_document_segment()
    {
        Assert.Equal("application/pdf", MediaTypes.DetectMediaType(Bytes(0x25, 0x50, 0x44, 0x46, 0x00), "application"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType::returns undefined for the \"text\" top-level segment", Coverage = UpstreamCoverage.Covered)]
    public void Text_segment_is_undefined()
    {
        Assert.Null(MediaTypes.DetectMediaType(Bytes(0x48, 0x65, 0x6c, 0x6c, 0x6f), "text"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType::returns undefined for unknown top-level segments", Coverage = UpstreamCoverage.Covered)]
    public void Unknown_segment_is_undefined()
    {
        Assert.Null(MediaTypes.DetectMediaType(Bytes(0x89, 0x50, 0x4e, 0x47, 0xff, 0xff), "not-a-real-segment"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType::returns undefined when data does not match any signature in the segment table", Coverage = UpstreamCoverage.Covered)]
    public void Unmatched_segment_data_is_undefined()
    {
        Assert.Null(MediaTypes.DetectMediaType(Bytes(0x00, 0x01, 0x02, 0x03), "image"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType > without topLevelType::detects image types", Coverage = UpstreamCoverage.Covered)]
    public void Unscoped_detection_finds_images()
    {
        Assert.Equal("image/png", MediaTypes.DetectMediaType(Bytes(0x89, 0x50, 0x4e, 0x47, 0xff, 0xff)));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType > without topLevelType::detects audio types", Coverage = UpstreamCoverage.Covered)]
    public void Unscoped_detection_finds_audio()
    {
        Assert.Equal("audio/mpeg", MediaTypes.DetectMediaType(Bytes(0xff, 0xfb)));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType > without topLevelType::detects video types", Coverage = UpstreamCoverage.Covered)]
    public void Unscoped_detection_finds_video()
    {
        Assert.Equal("video/mp4", MediaTypes.DetectMediaType(Bytes(0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70)));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType > without topLevelType::detects document types", Coverage = UpstreamCoverage.Covered)]
    public void Unscoped_detection_finds_documents()
    {
        Assert.Equal("application/pdf", MediaTypes.DetectMediaType(Bytes(0x25, 0x50, 0x44, 0x46, 0x00)));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/detect-media-type.test.ts::detectMediaType > without topLevelType::returns undefined when data does not match any signature", Coverage = UpstreamCoverage.Covered)]
    public void Unscoped_detection_rejects_garbage()
    {
        Assert.Null(MediaTypes.DetectMediaType(Bytes(0x00, 0x01, 0x02, 0x03)));
    }

    [Fact]
    [UpstreamTest(ResolveFull + "returns full media type as-is", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_full_media_type_keeps_a_concrete_type()
    {
        Assert.Equal("image/jpeg", MediaTypes.ResolveFullMediaType("image/jpeg", Png));
    }

    [Fact]
    [UpstreamTest(ResolveFull + "detects image subtype from inline bytes for top-level-only media type", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_full_media_type_sniffs_an_image()
    {
        Assert.Equal("image/png", MediaTypes.ResolveFullMediaType("image", Png));
    }

    [Fact]
    [UpstreamTest(ResolveFull + "treats image/* wildcard as top-level and runs detection", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_full_media_type_sniffs_a_wildcard()
    {
        Assert.Equal("image/png", MediaTypes.ResolveFullMediaType("image/*", Png));
    }

    [Fact]
    [UpstreamTest(ResolveFull + "detects application subtype (PDF)", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_full_media_type_sniffs_a_pdf()
    {
        Assert.Equal("application/pdf", MediaTypes.ResolveFullMediaType("application", Pdf));
    }

    [Fact]
    [UpstreamTest(ResolveFull + "throws the \"not passed as inline bytes\" message when URL source with top-level-only media type", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_full_media_type_rejects_a_url()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() => MediaTypes.ResolveFullMediaType("image", null));
        Assert.Contains("not passed as inline bytes", error.Message);
    }

    [Fact]
    [UpstreamTest(ResolveFull + "throws the \"could not be auto-detected\" message when bytes are present but unrecognised", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_full_media_type_rejects_unknown_bytes()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() => MediaTypes.ResolveFullMediaType("image", Bytes(0x00, 0x01, 0x02)));
        Assert.Contains("could not be auto-detected", error.Message);
    }

    [Fact]
    [UpstreamTest(ResolveFull + "throws the \"could not be auto-detected\" message when top-level segment is unsupported (e.g. text)", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_full_media_type_rejects_text()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() => MediaTypes.ResolveFullMediaType("text", "hello"));
        Assert.Contains("could not be auto-detected", error.Message);
    }

    [Fact]
    [UpstreamTest(ResolveFull + "accepts base64 string data", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_full_media_type_accepts_base64()
    {
        Assert.Equal("image/png", MediaTypes.ResolveFullMediaType("image", ByteEncoding.ToBase64(Png)));
    }

    private static byte[] Bytes(params int[] values)
    {
        var bytes = new byte[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            bytes[i] = (byte)values[i];
        }

        return bytes;
    }

    private static byte[] Id3Mp3(int tagBody)
    {
        var bytes = new byte[10 + tagBody + 2];
        bytes[0] = 0x49;
        bytes[1] = 0x44;
        bytes[2] = 0x33;
        bytes[6] = (byte)((tagBody >> 21) & 0x7f);
        bytes[7] = (byte)((tagBody >> 14) & 0x7f);
        bytes[8] = (byte)((tagBody >> 7) & 0x7f);
        bytes[9] = (byte)(tagBody & 0x7f);
        bytes[10 + tagBody] = 0xff;
        bytes[11 + tagBody] = 0xfb;
        return bytes;
    }
}
