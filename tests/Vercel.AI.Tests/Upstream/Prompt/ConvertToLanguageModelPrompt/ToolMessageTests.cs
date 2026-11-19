// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Logger;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of <c>convert-to-language-model-prompt.test.ts</c> &gt; <c>convertToLanguageModelMessage &gt; tool message</c>.</summary>
[Collection("LogWarnings")]
public sealed class ToolMessageTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::convertToLanguageModelMessage > tool message::";

    private static readonly IReadOnlyDictionary<string, DownloadResult> NoAssets =
        new Dictionary<string, DownloadResult>();

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [Fact]
    [UpstreamTest(Id + "should convert basic tool result message", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_basic_tool_result_message()
    {
        var result = LanguageModelPrompts.ConvertToLanguageModelMessage(
            new ToolModelMessage("toolCallId", "toolName", "{\"some\":\"result\"}", false),
            NoAssets
        );

        var tool = Assert.IsType<ToolModelMessage>(result);
        Assert.Equal("toolCallId", tool.ToolCallId);
        Assert.Equal("toolName", tool.ToolName);
        Assert.Equal("json", tool.OutputType);
        Assert.Equal("{\"some\":\"result\"}", tool.OutputJson);
        Assert.Null(tool.ProviderOptions);
        Assert.Null(tool.MessageProviderOptions);
    }

    [Fact]
    [UpstreamTest(Id + "should convert tool result with provider metadata", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_tool_result_with_provider_metadata()
    {
        var providerOptions = new Dictionary<string, JsonElement>
        {
            ["test-provider"] = Json("{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}"),
        };
        var result = LanguageModelPrompts.ConvertToLanguageModelMessage(
            new ToolModelMessage("toolCallId", "toolName", "{\"some\":\"result\"}", false)
            {
                ProviderOptions = providerOptions,
            },
            NoAssets
        );

        var tool = Assert.IsType<ToolModelMessage>(result);
        Assert.Equal("json", tool.OutputType);
        Assert.Equal("{\"some\":\"result\"}", tool.OutputJson);
        var options = Assert.IsAssignableFrom<IReadOnlyDictionary<string, JsonElement>>(
            tool.ProviderOptions
        );
        Assert.Equal(
            "{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}",
            options["test-provider"].GetRawText()
        );
        Assert.Null(tool.MessageProviderOptions);
    }

    [Fact]
    [UpstreamTest(Id + "should include error flag", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_error_flag()
    {
        var result = LanguageModelPrompts.ConvertToLanguageModelMessage(
            new ToolModelMessage("toolCallId", "toolName", "{\"some\":\"result\"}", false),
            NoAssets
        );

        var tool = Assert.IsType<ToolModelMessage>(result);
        Assert.Equal("json", tool.OutputType);
        Assert.Equal("{\"some\":\"result\"}", tool.OutputJson);
        Assert.False(tool.IsError);
        Assert.Null(tool.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(Id + "should include multipart content", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_multipart_content()
    {
        using var log = new LogWarningsRecorder();
        var reference = new Dictionary<string, string> { ["test-provider"] = "fileId" };
        var customOptions = new Dictionary<string, JsonElement>
        {
            ["test-provider"] = Json("{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}"),
        };

        var result = LanguageModelPrompts.ConvertToLanguageModelMessage(
            new ToolModelMessage(
                "toolCallId",
                "toolName",
                new UserContentPart[]
                {
                    new FileUrlContentPart("https://example.com/image.png", "image/png"),
                    new FileDataContentPart("dGVzdA==", "image/png"),
                    new FileReferenceContentPart(reference),
                    new ImageDataContentPart("dGVzdA==", "image/png"),
                    new ImageUrlContentPart("https://example.com/image.png"),
                    new ImageFileReferenceContentPart(reference),
                    new CustomContentPart { ProviderOptions = customOptions },
                }
            ),
            NoAssets
        );

        var tool = Assert.IsType<ToolModelMessage>(result);
        Assert.Equal("toolCallId", tool.ToolCallId);
        Assert.Equal("toolName", tool.ToolName);
        Assert.Null(tool.ProviderOptions);
        var content = tool.OutputContent!;
        Assert.Equal(7, content.Count);

        var url = Assert.IsType<FileContentPart>(content[0]);
        Assert.Equal("https://example.com/image.png", url.Url);
        Assert.Equal("image/png", url.MediaType);
        Assert.Null(url.ProviderOptions);

        var data = Assert.IsType<FileContentPart>(content[1]);
        Assert.Equal(new byte[] { 116, 101, 115, 116 }, data.Data);
        Assert.Null(data.FileName);
        Assert.Equal("image/png", data.MediaType);
        Assert.Null(data.ProviderOptions);

        var fileReference = Assert.IsType<FileContentPart>(content[2]);
        Assert.Equal(reference, fileReference.ProviderReference);
        Assert.Equal("application", fileReference.MediaType);
        Assert.Null(fileReference.ProviderOptions);

        var imageData = Assert.IsType<FileContentPart>(content[3]);
        Assert.Equal(new byte[] { 116, 101, 115, 116 }, imageData.Data);
        Assert.Equal("image/png", imageData.MediaType);
        Assert.Null(imageData.ProviderOptions);

        var imageUrl = Assert.IsType<FileContentPart>(content[4]);
        Assert.Equal("https://example.com/image.png", imageUrl.Url);
        Assert.Equal("image", imageUrl.MediaType);
        Assert.Null(imageUrl.ProviderOptions);

        var imageReference = Assert.IsType<FileContentPart>(content[5]);
        Assert.Equal(reference, imageReference.ProviderReference);
        Assert.Equal("image", imageReference.MediaType);
        Assert.Null(imageReference.ProviderOptions);

        var custom = Assert.IsType<CustomContentPart>(content[6]);
        Assert.Equal(
            "{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}",
            custom.ProviderOptions!["test-provider"].GetRawText()
        );
    }
}
