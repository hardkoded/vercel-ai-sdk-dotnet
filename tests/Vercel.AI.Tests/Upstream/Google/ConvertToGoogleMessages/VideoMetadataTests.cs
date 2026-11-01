// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;

namespace Vercel.AI.Tests.ConvertToGoogleMessages;

/// <summary>Port of <c>convert-to-google-messages.test.ts</c> &gt; <c>user messages</c> &gt; <c>videoMetadata</c>.</summary>
public sealed class VideoMetadataTests
{
    private const string FileUri = "https://generativelanguage.googleapis.com/v1beta/files/video";

    public static TheoryData<string, GoogleFileUserPart, string> FileParts => new()
    {
        {
            "YouTube URL",
            File("url", url: "https://www.youtube.com/watch?v=example"),
            """{"fileData":{"mimeType":"video/mp4","fileUri":"https://www.youtube.com/watch?v=example"}}"""
        },
        {
            "Cloud Storage URL with original casing",
            File("url", url: "gs://videobucket/video.mp4", originalUrl: "gs://VideoBucket/video.mp4"),
            """{"fileData":{"mimeType":"video/mp4","fileUri":"gs://VideoBucket/video.mp4"}}"""
        },
        {
            "uploaded file reference",
            File("reference", reference: new Dictionary<string, string> { ["google"] = FileUri }),
            """{"fileData":{"mimeType":"video/mp4","fileUri":"https://generativelanguage.googleapis.com/v1beta/files/video"}}"""
        },
        {
            "base64 data",
            File("data", base64: "AAECAw=="),
            """{"inlineData":{"mimeType":"video/mp4","data":"AAECAw=="}}"""
        },
        {
            "binary data",
            File("data", data: new byte[] { 0, 1, 2, 3 }),
            """{"inlineData":{"mimeType":"video/mp4","data":"AAECAw=="}}"""
        },
    };

    [Theory]
    [MemberData(nameof(FileParts))]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::user messages > videoMetadata::should include videoMetadata for $name", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_videoMetadata_for_name(string name, GoogleFileUserPart file, string expected)
    {
        _ = name;
        file.ProviderOptions = Options("google", """{"startOffset":"0s","endOffset":"10.5s","fps":0.5}""");

        var part = Convert(file);

        var expectedNode = JsonNode.Parse(expected)!.AsObject();
        expectedNode["videoMetadata"] = JsonNode.Parse("""{"startOffset":"0s","endOffset":"10.5s","fps":0.5}""");
        GoogleUpstream.JsonEqual(part, expectedNode.ToJsonString());
    }

    [Theory]
    [InlineData("google", "google")]
    [InlineData("googleVertex,vertex", "googleVertex")]
    [InlineData("googleVertex,vertex", "vertex")]
    [InlineData("googleVertex,vertex", "google")]
    [InlineData("google", "googleVertex")]
    [InlineData("google", "vertex")]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::user messages > videoMetadata::should resolve $namespace metadata with $providerOptionsNames", Coverage = UpstreamCoverage.Covered)]
    public void Should_resolve_namespace_metadata_with_providerOptionsNames(string providerOptionNames, string providerNamespace)
    {
        var file = File("url", url: "https://storage.googleapis.com/bucket/video.mp4");
        file.ProviderOptions = new Dictionary<string, JsonElement>
        {
            [providerNamespace] = JsonSerializer.SerializeToElement(new
            {
                videoMetadata = new GoogleVideoMetadata { Fps = 2 },
            }),
        };

        var part = Convert(file, providerOptionNames.Split(','));

        GoogleUpstream.JsonEqual(
            part,
            """{"fileData":{"mimeType":"video/mp4","fileUri":"https://storage.googleapis.com/bucket/video.mp4"},"videoMetadata":{"fps":2}}""");
    }

    [Theory]
    [InlineData("google", 1)]
    [InlineData("googleVertex,vertex", 2)]
    [InlineData("vertex", 3)]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::user messages > videoMetadata::should prefer the configured namespace $providerOptionsNames", Coverage = UpstreamCoverage.Covered)]
    public void Should_prefer_the_configured_namespace_providerOptionsNames(string providerOptionNames, double expectedFps)
    {
        var file = File("url", url: FileUri);
        file.ProviderOptions = GoogleUpstream.ProviderOptions(
            """{"google":{"videoMetadata":{"fps":1}},"googleVertex":{"videoMetadata":{"fps":2}},"vertex":{"videoMetadata":{"fps":3}}}""");

        var part = Convert(file, providerOptionNames.Split(','));

        GoogleUpstream.JsonEqual(
            part,
            """{"fileData":{"mimeType":"video/mp4","fileUri":"https://generativelanguage.googleapis.com/v1beta/files/video"},"videoMetadata":{"fps":""" + expectedFps.ToString(System.Globalization.CultureInfo.InvariantCulture) + "}}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::user messages > videoMetadata::should omit videoMetadata when it is not provided (%j)", Coverage = UpstreamCoverage.Covered)]
    public void Should_omit_videoMetadata_when_it_is_not_provided(int providerOptions)
    {
        var file = File("data", data: new byte[] { 0, 1, 2, 3 });
        file.ProviderOptions = providerOptions switch
        {
            0 => null,
            1 => new Dictionary<string, JsonElement>(),
            _ => GoogleUpstream.ProviderOptions("""{"google":{"videoMetadata":null}}"""),
        };

        var part = Convert(file);

        GoogleUpstream.JsonEqual(part, """{"inlineData":{"mimeType":"video/mp4","data":"AAECAw=="}}""");
    }

    private static GoogleFileUserPart File(
        string kind,
        string? url = null,
        string? originalUrl = null,
        byte[]? data = null,
        string? base64 = null,
        IReadOnlyDictionary<string, string>? reference = null)
    {
        return new GoogleFileUserPart("video/mp4", kind)
        {
            Url = url,
            OriginalUrl = originalUrl,
            Data = data,
            Base64 = base64,
            Reference = reference,
        };
    }

    private static IReadOnlyDictionary<string, JsonElement> Options(string providerNamespace, string metadata)
    {
        return GoogleUpstream.ProviderOptions(
            "{\"" + providerNamespace + "\":{\"videoMetadata\":" + metadata + "}}");
    }

    private static JsonNode Convert(GoogleFileUserPart file, IReadOnlyList<string>? providerOptionNames = null)
    {
        var prompt = GoogleMessages.Convert(
            new GoogleTurn[] { new GoogleUserTurn(new GoogleUserPart[] { file }) },
            new GoogleMessageOptions { ProviderOptionNames = providerOptionNames ?? new[] { "google" } });
        return prompt.Contents[0]!["parts"]![0]!;
    }
}
