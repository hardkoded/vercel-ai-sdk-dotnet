// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Helpers that stand in for <c>createDefaultDownloadFunction</c> and the prompt builders of <c>convert-to-language-model-prompt.test.ts</c>.</summary>
internal static class PromptTestSupport
{
    public static readonly IReadOnlyDictionary<string, string> NoUrls =
        new Dictionary<string, string>();

    /// <summary>Download function that checks each requested URL and returns the same result for it.</summary>
    public static Func<
        IReadOnlyList<DownloadRequestItem>,
        Task<IReadOnlyList<DownloadResult>>
    > DownloadOf(string expectedUrl, byte[] data, string mediaType)
    {
        return requests =>
        {
            var request = Assert.Single(requests);
            Assert.Equal(new Uri(expectedUrl), request.Url);
            return Task.FromResult<IReadOnlyList<DownloadResult>>(
                new[] { new DownloadResult(data, mediaType) }
            );
        };
    }

    public static Task<IReadOnlyList<ModelMessage>> ConvertAsync(
        IReadOnlyList<ModelMessage> messages,
        IReadOnlyDictionary<string, string>? supportedUrls = null,
        Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>? download =
            null
    )
    {
        return LanguageModelPrompts.ConvertToLanguageModelPromptAsync(
            messages,
            supportedUrls ?? NoUrls,
            download
        );
    }

    public static FileContentPart OnlyFile(IReadOnlyList<ModelMessage> result)
    {
        var user = Assert.IsType<UserModelMessage>(Assert.Single(result));
        return Assert.IsType<FileContentPart>(Assert.Single(user.Content));
    }
}
