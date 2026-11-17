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
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::convertToLanguageModelPrompt > custom download function::";

    [Fact]
    [UpstreamTest(
        Id + "should use custom download function to fetch URL content",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542"
    )]
    public async Task Should_use_custom_download_function_to_fetch_url_content()
    {
        using var abort = new CancellationTokenSource();
        var calls = new List<IReadOnlyList<DownloadRequestItem>>();
        Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>> mockDownload =
            requests =>
            {
                calls.Add(requests);
                return Task.FromResult<IReadOnlyList<DownloadResult>>(
                    new[]
                    {
                        new DownloadResult(new byte[] { 72, 101, 108, 108, 111 }, "text/plain"),
                    }
                );
            };

        var result = await LanguageModelPrompts.ConvertToLanguageModelPromptAsync(
            new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new FileContentPart(
                            "text/plain",
                            "https://example.com/test-file.txt",
                            null,
                            null
                        ),
                    }
                ),
            },
            new Dictionary<string, string>(), // No URL support, so download should be triggered
            mockDownload,
            abort.Token
        );

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

    [Fact]
    [UpstreamTest(
        Id + "should download URL content in tool results",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542"
    )]
    public async Task Should_download_url_content_in_tool_results()
    {
        var calls = new List<IReadOnlyList<DownloadRequestItem>>();
        Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>> mockDownload =
            requests =>
            {
                calls.Add(requests);
                return Task.FromResult<IReadOnlyList<DownloadResult>>(
                    new[] { new DownloadResult(new byte[] { 0, 1, 2, 3 }, "image/png") }
                );
            };

        var result = await LanguageModelPrompts.ConvertToLanguageModelPromptAsync(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantToolCallPart(
                            new GeneratedToolCall("toolCallId", "toolName", "{}")
                        ),
                    }
                ),
                new ToolModelMessage(
                    "toolCallId",
                    "toolName",
                    new UserContentPart[]
                    {
                        new FileContentPart(
                            "image/png",
                            "https://example.com/image.png",
                            null,
                            null
                        ),
                    }
                ),
            },
            new Dictionary<string, string>(),
            mockDownload
        );

        var call = Assert.Single(calls);
        var request = Assert.Single(call);
        Assert.Equal(new Uri("https://example.com/image.png"), request.Url);
        Assert.False(request.IsUrlSupportedByModel);

        Assert.Equal(2, result.Count);
        var tool = Assert.IsType<ToolModelMessage>(result[1]);
        var file = Assert.IsType<FileContentPart>(Assert.Single(tool.OutputContent!));
        Assert.Equal("image/png", file.MediaType);
        Assert.Null(file.Url);
        Assert.Equal(new byte[] { 0, 1, 2, 3 }, file.Data);
    }

    [Fact]
    [UpstreamTest(
        Id + "should download URL content in assistant tool results",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542"
    )]
    public async Task Should_download_url_content_in_assistant_tool_results()
    {
        var calls = new List<IReadOnlyList<DownloadRequestItem>>();
        Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>> mockDownload =
            requests =>
            {
                calls.Add(requests);
                return Task.FromResult<IReadOnlyList<DownloadResult>>(
                    new[] { new DownloadResult(new byte[] { 4, 5, 6, 7 }, "image/png") }
                );
            };

        var output = new ToolModelMessage(
            "toolCallId",
            "toolName",
            new UserContentPart[]
            {
                new FileContentPart(
                    "image/png",
                    "https://example.com/assistant-image.png",
                    null,
                    null
                ),
            }
        );
        var result = await LanguageModelPrompts.ConvertToLanguageModelPromptAsync(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantToolResultPart("toolCallId", "toolName", output),
                    }
                ),
            },
            new Dictionary<string, string>(),
            mockDownload
        );

        var request = Assert.Single(Assert.Single(calls));
        Assert.Equal(new Uri("https://example.com/assistant-image.png"), request.Url);
        Assert.False(request.IsUrlSupportedByModel);

        var assistant = Assert.IsType<AssistantModelMessage>(Assert.Single(result));
        var toolResult = Assert.IsType<AssistantToolResultPart>(Assert.Single(assistant.Parts));
        var file = Assert.IsType<FileContentPart>(Assert.Single(toolResult.Output.OutputContent!));
        Assert.Equal("image/png", file.MediaType);
        Assert.Null(file.Url);
        Assert.Equal(new byte[] { 4, 5, 6, 7 }, file.Data);
    }

    [Fact]
    [UpstreamTest(
        Id + "should download URL content in deprecated file-url and image-url tool results",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542"
    )]
    public async Task Should_download_url_content_in_deprecated_file_url_and_image_url_tool_results()
    {
        var calls = new List<IReadOnlyList<DownloadRequestItem>>();
        Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>> mockDownload =
            requests =>
            {
                calls.Add(requests);
                return Task.FromResult<IReadOnlyList<DownloadResult>>(
                    requests
                        .Select(_ => new DownloadResult(
                            new byte[] { 1, 2, 3 },
                            "application/octet-stream"
                        ))
                        .ToArray()
                );
            };

        var result = await LanguageModelPrompts.ConvertToLanguageModelPromptAsync(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantToolCallPart(
                            new GeneratedToolCall("toolCallId", "toolName", "{}")
                        ),
                    }
                ),
                new ToolModelMessage(
                    "toolCallId",
                    "toolName",
                    new UserContentPart[]
                    {
                        new FileUrlContentPart(
                            "https://example.com/deprecated-file.pdf",
                            "application/pdf"
                        ),
                        new ImageUrlContentPart("https://example.com/deprecated-image.png"),
                    }
                ),
            },
            new Dictionary<string, string>(),
            mockDownload
        );

        var call = Assert.Single(calls);
        Assert.Equal(2, call.Count);
        Assert.Equal(new Uri("https://example.com/deprecated-file.pdf"), call[0].Url);
        Assert.False(call[0].IsUrlSupportedByModel);
        Assert.Equal(new Uri("https://example.com/deprecated-image.png"), call[1].Url);
        Assert.False(call[1].IsUrlSupportedByModel);

        Assert.Equal(2, result.Count);
        var tool = Assert.IsType<ToolModelMessage>(result[1]);
        var parts = tool.OutputContent!;
        Assert.Equal(2, parts.Count);
        var pdf = Assert.IsType<FileContentPart>(parts[0]);
        Assert.Equal("application/pdf", pdf.MediaType);
        Assert.Null(pdf.Url);
        Assert.Equal(new byte[] { 1, 2, 3 }, pdf.Data);
        var image = Assert.IsType<FileContentPart>(parts[1]);
        Assert.Equal("application/octet-stream", image.MediaType);
        Assert.Null(image.Url);
        Assert.Equal(new byte[] { 1, 2, 3 }, image.Data);
    }
}
