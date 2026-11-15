// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using Download_ = Vercel.AI.Google.DownloadToolResultFiles;

namespace Vercel.AI.Tests.Upstream.Google.DownloadToolResultFiles;

/// <summary>Port of <c>download-tool-result-files.test.ts</c> &gt; <c>downloadToolResultFiles</c>.</summary>
[Collection("DownloadFetch")]
public sealed class DownloadToolResultFilesTests
{
    private const string Id = "packages/google/src/download-tool-result-files.test.ts::downloadToolResultFiles::";

    private static IReadOnlyList<ModelMessage> CreateToolResultPrompt(params string[] urls)
    {
        return new ModelMessage[]
        {
            new ToolModelMessage("tool-call-id", "get-image", urls.Select(url => (UserContentPart)new FileContentPart("image", url, null, null)).ToList()),
        };
    }

    private static Task<T> WithGlobalFetch<T>(DownloadFetch fetch, Func<Task<T>> run)
    {
        return Run();

        async Task<T> Run()
        {
            var previous = Download.Fetch;
            try
            {
                Download.Fetch = fetch;
                return await run().ConfigureAwait(false);
            }
            finally
            {
                Download.Fetch = previous;
            }
        }
    }

    [Fact]
    [UpstreamTest(Id + "downloads remote files in tool results without changing user files", Coverage = UpstreamCoverage.Covered)]
    public async Task DownloadsRemoteFilesInToolResultsWithoutChangingUserFiles()
    {
        var calls = new List<(string Url, string Redirect)>();
        var user = new UserModelMessage(new UserContentPart[] { new FileContentPart("image/png", "https://example.com/user-image.png", null, null) });
        var prompt = new ModelMessage[]
        {
            user,
            new ToolModelMessage("tool-call-id", "get-image", new UserContentPart[] { new FileContentPart("image", "https://example.com/tool-image.jpg", null, null) }),
        };

        var result = await WithGlobalFetch(
            (url, request) =>
            {
                calls.Add((url, request.Redirect));
                return Task.FromResult(DownloadResponse.Bytes(new byte[] { 0xff, 0xd8, 0xff }, "image/jpeg"));
            },
            () => DownloadToolResultFiles_(prompt, 7 * 1024 * 1024));

        Assert.Equal(("https://example.com/tool-image.jpg", "manual"), Assert.Single(calls));
        Assert.Same(user, result[0]);
        var file = Assert.IsType<FileContentPart>(Assert.Single(((ToolModelMessage)result[1]).OutputContent!));
        Assert.Equal("image/jpeg", file.MediaType);
        Assert.Equal(new byte[] { 0xff, 0xd8, 0xff }, file.Data);
        Assert.Null(file.Url);
    }

    [Fact]
    [UpstreamTest(Id + "preserves tool result URLs supported in function responses", Coverage = UpstreamCoverage.Covered)]
    public async Task PreservesToolResultUrlsSupportedInFunctionResponses()
    {
        var fetches = 0;
        var prompt = CreateToolResultPrompt("gs://example-bucket/tool-image.jpg");

        var result = await WithGlobalFetch(
            (url, request) =>
            {
                fetches++;
                return Task.FromResult(DownloadResponse.Bytes(Array.Empty<byte>(), null));
            },
            () => DownloadToolResultFiles_(
                prompt,
                7 * 1024 * 1024,
                new Dictionary<string, IReadOnlyList<Regex>> { ["*"] = new[] { new Regex(@"^gs://.*\z") } }));

        Assert.Equal(0, fetches);
        Assert.Same(prompt[0], result[0]);
    }

    [Fact]
    [UpstreamTest(Id + "limits the size of each download", Coverage = UpstreamCoverage.Covered)]
    public async Task LimitsTheSizeOfEachDownload()
    {
        var error = await Assert.ThrowsAsync<DownloadError>(() => WithGlobalFetch(
            (url, request) => Task.FromResult(DownloadResponse.Bytes(new byte[] { 0xff, 0xd8, 0xff }, null)),
            () => DownloadToolResultFiles_(CreateToolResultPrompt("https://example.com/tool-image.jpg"), 2)));

        Assert.Contains("exceeded maximum size of 2 bytes", error.Message);
    }

    [Fact]
    [UpstreamTest(Id + "downloads files sequentially", Coverage = UpstreamCoverage.Covered)]
    public async Task DownloadsFilesSequentially()
    {
        var active = 0;
        var maxActive = 0;

        await WithGlobalFetch(
            async (url, request) =>
            {
                var now = Interlocked.Increment(ref active);
                maxActive = Math.Max(maxActive, now);
                await Task.Delay(1);
                Interlocked.Decrement(ref active);
                return DownloadResponse.Bytes(new byte[] { 0xff, 0xd8, 0xff }, null);
            },
            () => DownloadToolResultFiles_(CreateToolResultPrompt("https://example.com/tool-image-1.jpg", "https://example.com/tool-image-2.jpg"), 7 * 1024 * 1024));

        Assert.Equal(1, maxActive);
    }

    private static async Task<IReadOnlyList<ModelMessage>> DownloadToolResultFiles_(
        IReadOnlyList<ModelMessage> prompt,
        long maxBytes,
        IReadOnlyDictionary<string, IReadOnlyList<Regex>>? supportedUrls = null)
    {
        return await Download_.DownloadToolResultFilesAsync(prompt, CancellationToken.None, maxBytes, null, supportedUrls);
    }
}
