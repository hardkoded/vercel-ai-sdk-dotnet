// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using S = Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput.ConvertToGoogleInteractionsInputMessageSupport;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Port of <c>convert-to-google-interactions-input.test.ts</c> &gt; <c>convertToGoogleInteractionsInput</c> &gt; <c>assistant file parts</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputAssistantFilePartsTests
{
    private const string Prefix =
        ConvertToGoogleInteractionsInputSupport.Id + "assistant file parts::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "emits an image content block from an assistant inline-data file part (round-trips a generated image)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_an_image_content_block_from_an_assistant_inline_data_file_part_round_trips_a_generated_image()
    {
        // The C# part holds bytes, so the data is a valid base64 string ("base64da" instead of upstream's "base64data").
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage("generate a cat"),
            S.Assistant(
                new AssistantFilePart("image/png", null, Convert.FromBase64String("base64da"))
            ),
            new UserModelMessage("now make it red")
        );

        Assert.Empty(result.Warnings);
        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"generate a cat\"}]},{\"type\":\"model_output\",\"content\":[{\"type\":\"image\",\"data\":\"base64da\",\"mime_type\":\"image/png\"}]},{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"now make it red\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits an image content block from an assistant url-data file part",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_an_image_content_block_from_an_assistant_url_data_file_part()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            S.Assistant(new AssistantFilePart("image/png", "https://example.com/cat.png", null))
        );

        GoogleUpstream.JsonEqual(
            result.Input[0]!["content"]![0],
            "{\"type\":\"image\",\"uri\":\"https://example.com/cat.png\",\"mime_type\":\"image/png\"}"
        );
    }
}
