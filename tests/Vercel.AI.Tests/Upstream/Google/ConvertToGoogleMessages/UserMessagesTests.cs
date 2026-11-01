// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.ConvertToGoogleMessages;

/// <summary>Port of <c>convert-to-google-messages.test.ts</c> &gt; <c>user messages</c>.</summary>
public sealed class UserMessagesTests
{
    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::user messages::should preserve original Google Cloud Storage file URIs", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_original_Google_Cloud_Storage_file_URIs()
    {
        var file = new GoogleFileUserPart("application/pdf", "url")
        {
            Url = "gs://my-bucket/folder/my%20file.pdf",
            OriginalUrl = "gs://my-bucket/folder/My File.pdf",
        };

        var result = Convert(file);

        Assert.Null(result.SystemInstruction);
        GoogleUpstream.JsonEqual(
            result.Contents,
            """[{"role":"user","parts":[{"fileData":{"mimeType":"application/pdf","fileUri":"gs://my-bucket/folder/My File.pdf"}}]}]""");
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::user messages::should add image parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_image_parts()
    {
        var result = Convert(new GoogleFileUserPart("image/png", "data") { Data = new byte[] { 0, 1, 2, 3 } });

        Assert.Null(result.SystemInstruction);
        GoogleUpstream.JsonEqual(
            result.Contents,
            """[{"role":"user","parts":[{"inlineData":{"mimeType":"image/png","data":"AAECAw=="}}]}]""");
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::user messages::should add file parts for base64 encoded files", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_file_parts_for_base64_encoded_files()
    {
        var result = Convert(new GoogleFileUserPart("image/png", "data") { Data = new byte[] { 0, 1, 2, 3 } });

        Assert.Null(result.SystemInstruction);
        GoogleUpstream.JsonEqual(
            result.Contents,
            """[{"role":"user","parts":[{"inlineData":{"mimeType":"image/png","data":"AAECAw=="}}]}]""");
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::user messages::should convert file parts with provider reference to fileData", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_file_parts_with_provider_reference_to_fileData()
    {
        var result = GoogleMessages.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                FileContentPart.FromProviderReference(
                    "image/png",
                    new Dictionary<string, string>
                    {
                        ["google"] = "https://generativelanguage.googleapis.com/v1beta/files/abc123",
                        ["openai"] = "file-xyz789",
                    }),
            }),
        });

        Assert.Null(result.SystemInstruction);
        GoogleUpstream.JsonEqual(
            result.Contents,
            """[{"role":"user","parts":[{"fileData":{"mimeType":"image/png","fileUri":"https://generativelanguage.googleapis.com/v1beta/files/abc123"}}]}]""");
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::user messages::should convert image file parts with provider reference to fileData", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_image_file_parts_with_provider_reference_to_fileData()
    {
        var result = GoogleMessages.Convert(new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                FileContentPart.FromProviderReference(
                    "image/jpeg",
                    new Dictionary<string, string>
                    {
                        ["google"] = "https://generativelanguage.googleapis.com/v1beta/files/img456",
                    }),
            }),
        });

        Assert.Null(result.SystemInstruction);
        GoogleUpstream.JsonEqual(
            result.Contents,
            """[{"role":"user","parts":[{"fileData":{"mimeType":"image/jpeg","fileUri":"https://generativelanguage.googleapis.com/v1beta/files/img456"}}]}]""");
    }

    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::user messages::should throw when provider reference is missing google key in user file part", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_when_provider_reference_is_missing_google_key_in_user_file_part()
    {
        var exception = Assert.Throws<NoSuchProviderReferenceError>(() =>
            GoogleMessages.Convert(new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    FileContentPart.FromProviderReference(
                        "image/png",
                        new Dictionary<string, string> { ["openai"] = "file-xyz789" }),
                }),
            }));

        Assert.Equal("No provider reference found for provider 'google'. Available providers: openai", exception.Message);
    }

    private static GooglePrompt Convert(GoogleFileUserPart file)
    {
        return GoogleMessages.Convert(
            new GoogleTurn[] { new GoogleUserTurn(new GoogleUserPart[] { file }) },
            new GoogleMessageOptions());
    }
}
