// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; prepareStep with model switch and image URLs</c>.</summary>
public sealed class PrepareStepWithModelSwitchAndImageUrlsTests
{
    private const string Id =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > prepareStep with model switch and image URLs::";

    [Fact]
    [UpstreamTest(
        Id + "should use the prepareStep model supportedUrls for download decision",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542"
    )]
    public async Task Should_use_the_prepareStep_model_supportedUrls_for_download_decision()
    {
        var downloadCalls = new List<DownloadRequestItem>();
        var withSupport = Model(
            "with-image-url-support",
            new Dictionary<string, string> { ["image/*"] = "^https?://.*\\z" }
        );
        var withoutSupport = Model("without-image-url-support", new Dictionary<string, string>());

        Task<IReadOnlyList<DownloadResult>> CustomDownload(
            IReadOnlyList<DownloadRequestItem> requested
        )
        {
            downloadCalls.AddRange(requested);
            return Task.FromResult<IReadOnlyList<DownloadResult>>(
                requested
                    .Select(item =>
                        item.IsUrlSupportedByModel
                            ? null!
                            : new DownloadResult(new byte[] { 1, 2, 3, 4 }, "image/png")
                    )
                    .ToArray()
            );
        }

        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        var messages = new ModelMessage[]
        {
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("Describe this image"),
                    new FileContentPart("image/*", "https://example.com/test.jpg", null, null),
                }
            ),
        };
        var generated = await client.GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = withSupport,
                Messages = messages,
                PrepareStep = _ => new PrepareStepUpdate { Model = withoutSupport }, // model switch
                ExperimentalDownload = CustomDownload,
            }
        );
        var text = generated.Text;

        var download = Assert.Single(downloadCalls);
        Assert.Equal(new Uri("https://example.com/test.jpg"), download.Url);
        Assert.False(download.IsUrlSupportedByModel);

        Assert.Empty(withSupport.Calls);
        var call = Assert.Single(withoutSupport.Calls);
        var user = Assert.IsType<UserModelMessage>(Assert.Single(call.Prompt));
        Assert.Equal(2, user.Content.Count);
        Assert.Equal("Describe this image", Assert.IsType<TextContentPart>(user.Content[0]).Text);
        var file = Assert.IsType<FileContentPart>(user.Content[1]);
        Assert.Equal("image/png", file.MediaType);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, file.Data);
        Assert.Null(file.Url);
        Assert.Equal("response from without-image-url-support", text);
    }

    private static TestLanguageModel Model(
        string id,
        IReadOnlyDictionary<string, string> supportedUrls
    )
    {
        return new TestLanguageModel(id)
        {
            SupportedUrls = supportedUrls,
            OnGenerate = _ => TestLanguageModel.Text("response from " + id),
            StreamParts = new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(Array.Empty<CallWarning>()),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "response from " + id),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, LanguageModelUsage.Empty, "stop"),
            },
        };
    }
}
