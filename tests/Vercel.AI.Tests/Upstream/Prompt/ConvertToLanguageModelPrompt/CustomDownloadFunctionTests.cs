// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of <c>convert-to-language-model-prompt.test.ts</c> &gt; <c>convertToLanguageModelPrompt &gt; custom download function</c>.</summary>
public sealed class CustomDownloadFunctionTests
{
    private const string Id = "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::convertToLanguageModelPrompt > custom download function::";

    [Fact]
    [UpstreamTest(Id + "should use custom download function to fetch URL content", Coverage = UpstreamCoverage.Covered, Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Should_use_custom_download_function_to_fetch_url_content()
    {
        using var abort = new CancellationTokenSource();
        var calls = new List<IReadOnlyList<DownloadRequestItem>>();
        Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>> mockDownload = requests =>
        {
            calls.Add(requests);
            return Task.FromResult<IReadOnlyList<DownloadResult>>(new[] { new DownloadResult(new byte[] { 72, 101, 108, 108, 111 }, "text/plain") });
        };

        var result = await LanguageModelPrompts.ConvertToLanguageModelPromptAsync(
            new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[] { new FileContentPart("text/plain", "https://example.com/test-file.txt", null, null) }),
            },
            new Dictionary<string, string>(), // No URL support, so download should be triggered
            mockDownload,
            abort.Token);

        var call = Assert.Single(calls);
        var request = Assert.Single(call);
        Assert.Equal(new Uri("https://example.com/test-file.txt"), request.Url);
        Assert.False(request.IsUrlSupportedByModel);
        Assert.Equal(abort.Token, request.AbortSignal);

        var message = Assert.IsType<UserModelMessage>(Assert.Single(result));
        var file = Assert.IsType<FileContentPart>(Assert.Single(message.Content));
        Assert.Equal("text/plain", file.MediaType);
        Assert.Null(file.Url);
        Assert.Equal(new byte[] { 72, 101, 108, 108, 111 }, file.Data);
    }
}
