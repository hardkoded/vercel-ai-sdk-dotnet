// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doStream</c> &gt; <c>image output</c>.
/// </summary>
public sealed class DoStreamImageOutputTests
{
    private const string Prefix =
        "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doStream > image output::";

    [Fact]
    [UpstreamTest(
        Prefix + "emits a file stream part at content.stop for image blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_a_file_stream_part_at_content_stop_for_image_blocks()
    {
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("image-output");
        var options = GoogleUpstream.Prompt(
            new UserModelMessage("Generate an image of a comic cat in a spaceship.")
        );
        options.ProviderOptions = GoogleUpstream.ProviderOptions(
            "{\"google\":{\"responseModalities\":[\"image\"]}}"
        );
        options.IncludeRawChunks = false;

        var parts = await GoogleInteractionsStreamSupport.Stream(
            GoogleInteractionsStreamSupport.Model(handler, "gemini-3-pro-image-preview"),
            options
        );

        var filePart = Assert.IsType<FileStreamPart>(parts.First(part => part is FileStreamPart));
        Assert.Equal("file", filePart.Type);
        Assert.Matches("^image/", filePart.MediaType);
        Assert.NotNull(filePart.Data);
        Assert.True(filePart.Data!.Length > 0);
        Assert.Matches(
            "^v1_",
            filePart
                .ProviderMetadata!.Value.GetProperty("google")
                .GetProperty("interactionId")
                .GetString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits a file stream part for the modify (turn 2) stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_a_file_stream_part_for_the_modify_turn_2_stream()
    {
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("image-output-modify");
        var options = GoogleUpstream.Prompt(new UserModelMessage("now make the cat red"));
        options.ProviderOptions = GoogleUpstream.ProviderOptions(
            "{\"google\":{\"responseModalities\":[\"image\"],\"previousInteractionId\":\"v1_prev-turn\"}}"
        );
        options.IncludeRawChunks = false;

        var parts = await GoogleInteractionsStreamSupport.Stream(
            GoogleInteractionsStreamSupport.Model(handler, "gemini-3-pro-image-preview"),
            options
        );

        var filePart = Assert.IsType<FileStreamPart>(parts.First(part => part is FileStreamPart));
        Assert.Equal("image/jpeg", filePart.MediaType);
        Assert.NotNull(filePart.Data);
    }
}
