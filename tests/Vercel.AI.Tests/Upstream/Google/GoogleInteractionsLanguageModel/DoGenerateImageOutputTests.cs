// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>image output</c>.
/// </summary>
public sealed class DoGenerateImageOutputTests
{
    private const string Prefix = DoGenerateSupport.Id + "image output::";

    private const string ImageModelId = "gemini-3-pro-image-preview";

    private static SequencedHandler Fixture(string name)
    {
        return new SequencedHandler().Json(GoogleInteractionsStreamSupport.JsonFixture(name));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits a file content part on the result when the response carries an image block",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_a_file_content_part_on_the_result_when_the_response_carries_an_image_block()
    {
        var options = GoogleUpstream.Prompt(
            new UserModelMessage("Generate an image of a comic cat in a spaceship.")
        );
        options.ProviderOptions = GoogleUpstream.ProviderOptions(
            "{\"google\":{\"responseModalities\":[\"image\"]}}"
        );

        var result = await GoogleInteractionsStreamSupport
            .Model(Fixture("image-output"), ImageModelId)
            .DoGenerateAsync(options, CancellationToken.None);

        var filePart = Assert.IsType<GoogleGeneratedFile>(
            result.Content.First(part => part.Type == "file")
        );
        Assert.Equal("file", filePart.Type);
        Assert.Matches("^image/", filePart.MediaType);
        Assert.Null(filePart.Url);
        Assert.True(filePart.Data.Length > 0);
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
        Prefix + "passes responseModalities into the request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Passes_responseModalities_into_the_request_body()
    {
        var handler = Fixture("image-output");
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.ProviderOptions = GoogleUpstream.ProviderOptions(
            "{\"google\":{\"responseModalities\":[\"image\"]}}"
        );

        await GoogleInteractionsStreamSupport
            .Model(handler, ImageModelId)
            .DoGenerateAsync(options, CancellationToken.None);

        GoogleUpstream.JsonEqual(
            DoGenerateSupport.Body(handler)["response_modalities"],
            "[\"image\"]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "emits a file content part on the modify (turn 2) result with previousInteractionId",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_a_file_content_part_on_the_modify_turn_2_result_with_previousInteractionId()
    {
        var handler = Fixture("image-output-modify");
        var options = GoogleUpstream.Prompt(new UserModelMessage("now make the cat red"));
        options.ProviderOptions = GoogleUpstream.ProviderOptions(
            "{\"google\":{\"responseModalities\":[\"image\"],\"previousInteractionId\":\"v1_prev-turn\"}}"
        );

        var result = await GoogleInteractionsStreamSupport
            .Model(handler, ImageModelId)
            .DoGenerateAsync(options, CancellationToken.None);

        var filePart = Assert.IsAssignableFrom<GeneratedFile>(
            result.Content.First(part => part.Type == "file")
        );
        Assert.Equal("image/jpeg", filePart.MediaType);
        Assert.Equal(
            "v1_prev-turn",
            (string?)DoGenerateSupport.Body(handler)["previous_interaction_id"]
        );
    }
}
