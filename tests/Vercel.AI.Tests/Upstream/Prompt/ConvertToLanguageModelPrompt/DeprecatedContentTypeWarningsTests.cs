// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Logger;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of <c>convert-to-language-model-prompt.test.ts</c> &gt; <c>convertToLanguageModelMessage &gt; tool message &gt; deprecated content type warnings</c>.</summary>
[Collection("LogWarnings")]
public sealed class DeprecatedContentTypeWarningsTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::convertToLanguageModelMessage > tool message > deprecated content type warnings::";

    private static readonly IReadOnlyDictionary<string, string> TestProviderReference =
        new Dictionary<string, string> { ["test-provider"] = "fileId" };

    private static IReadOnlyList<UserContentPart> ConvertTool(params UserContentPart[] parts)
    {
        var result = LanguageModelPrompts.ConvertToLanguageModelMessage(
            new ToolModelMessage("toolCallId", "toolName", parts),
            new Dictionary<string, DownloadResult>()
        );
        return Assert.IsType<ToolModelMessage>(result).OutputContent!;
    }

    private static void AssertWarning(
        LogWarningsRecorder log,
        string message,
        string code,
        int count = 2
    )
    {
        Assert.Equal(count, log.Process.Count);
        Assert.Contains(log.Process, entry =>
            entry.Message == "AI SDK Warning: Deprecated: \"" + message
            && entry.Type == "DeprecationWarning"
            && entry.Code == code
        );
    }

    private static FileContentPart AssertReference(
        UserContentPart part,
        string mediaType,
        string provider = "test-provider",
        string fileId = "fileId"
    )
    {
        var file = Assert.IsType<FileContentPart>(part);
        Assert.Equal(mediaType, file.MediaType);
        Assert.Equal(
            new Dictionary<string, string> { [provider] = fileId },
            file.ProviderReference
        );
        Assert.Null(file.ProviderOptions);
        return file;
    }

    [Fact]
    [UpstreamTest(
        Id + "should emit DeprecationWarning for file-data and emit type: \"file\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_DeprecationWarning_for_file_data_and_emit_type_file()
    {
        using var log = new LogWarningsRecorder();

        var parts = ConvertTool(new FileDataContentPart("dGVzdA==", "image/png"));

        AssertWarning(
            log,
            "\"tool-result\" content of type \"file-data\"\". The \"file-data\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'data', data } instead.",
            "AISDK_DEP_TOOL_RESULT_FILE_DATA"
        );
        var file = Assert.IsType<FileContentPart>(Assert.Single(parts));
        Assert.Equal("image/png", file.MediaType);
        Assert.Equal(new byte[] { 116, 101, 115, 116 }, file.Data);
        Assert.Null(file.FileName);
        Assert.Null(file.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should emit DeprecationWarning for file-reference and emit type: \"file\" with application default",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_DeprecationWarning_for_file_reference_and_emit_type_file_with_application_default()
    {
        using var log = new LogWarningsRecorder();

        var parts = ConvertTool(new FileReferenceContentPart(TestProviderReference));

        AssertWarning(
            log,
            "\"tool-result\" content of type \"file-reference\"\". The \"file-reference\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'reference', reference } instead.",
            "AISDK_DEP_TOOL_RESULT_FILE_REFERENCE"
        );
        AssertReference(Assert.Single(parts), "application");
    }

    [Fact]
    [UpstreamTest(
        Id + "should emit DeprecationWarning for image-data and emit type: \"file\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_DeprecationWarning_for_image_data_and_emit_type_file()
    {
        using var log = new LogWarningsRecorder();

        var parts = ConvertTool(new ImageDataContentPart("dGVzdA==", "image/png"));

        AssertWarning(
            log,
            "\"tool-result\" content of type \"image-data\"\". The \"image-data\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'data', data } instead.",
            "AISDK_DEP_TOOL_RESULT_IMAGE_DATA"
        );
        var file = Assert.IsType<FileContentPart>(Assert.Single(parts));
        Assert.Equal("image/png", file.MediaType);
        Assert.Equal(new byte[] { 116, 101, 115, 116 }, file.Data);
        Assert.Null(file.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should emit DeprecationWarning for image-url and emit type: \"file\" with mediaType \"image\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_DeprecationWarning_for_image_url_and_emit_type_file_with_mediaType_image()
    {
        using var log = new LogWarningsRecorder();

        var parts = ConvertTool(new ImageUrlContentPart("https://example.com/image.png"));

        AssertWarning(
            log,
            "\"tool-result\" content of type \"image-url\"\". The \"image-url\" type for tool result content is deprecated. Use the \"file\" type with mediaType 'image' (or a specific image/* subtype) and { type: 'url', url } instead.",
            "AISDK_DEP_TOOL_RESULT_IMAGE_URL"
        );
        var file = Assert.IsType<FileContentPart>(Assert.Single(parts));
        Assert.Equal("image", file.MediaType);
        Assert.Equal("https://example.com/image.png", file.Url);
        Assert.Null(file.ProviderOptions);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should emit DeprecationWarning for image-file-reference and emit type: \"file\" with mediaType \"image\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_DeprecationWarning_for_image_file_reference_and_emit_type_file_with_mediaType_image()
    {
        using var log = new LogWarningsRecorder();

        var parts = ConvertTool(new ImageFileReferenceContentPart(TestProviderReference));

        AssertWarning(
            log,
            "\"tool-result\" content of type \"image-file-reference\"\". The \"image-file-reference\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'reference', reference } instead.",
            "AISDK_DEP_TOOL_RESULT_IMAGE_FILE_REFERENCE"
        );
        AssertReference(Assert.Single(parts), "image");
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should emit DeprecationWarning for image-file-id with object fileId and emit type: \"file\" with mediaType \"image\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_DeprecationWarning_for_image_file_id_with_object_fileId_and_emit_type_file_with_mediaType_image()
    {
        using var log = new LogWarningsRecorder();

        var parts = ConvertTool(new ImageFileIdContentPart(TestProviderReference));

        AssertWarning(
            log,
            "\"tool-result\" content of type \"image-file-id\"\". The \"image-file-id\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'reference', reference } instead.",
            "AISDK_DEP_TOOL_RESULT_IMAGE_FILE_ID"
        );
        AssertReference(Assert.Single(parts), "image");
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should emit DeprecationWarning for file-id with object fileId and emit type: \"file\" with application default",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_DeprecationWarning_for_file_id_with_object_fileId_and_emit_type_file_with_application_default()
    {
        using var log = new LogWarningsRecorder();

        var parts = ConvertTool(new FileIdContentPart(TestProviderReference));

        AssertWarning(
            log,
            "\"tool-result\" content of type \"file-id\"\". The \"file-id\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'reference', reference } instead.",
            "AISDK_DEP_TOOL_RESULT_FILE_ID"
        );
        AssertReference(Assert.Single(parts), "application");
    }

    [Fact]
    [UpstreamTest(
        Id + "should emit DeprecationWarning for file-url with mediaType",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_DeprecationWarning_for_file_url_with_mediaType()
    {
        using var log = new LogWarningsRecorder();

        ConvertTool(new FileUrlContentPart("https://example.com/image.png", "image/png"));

        AssertWarning(
            log,
            "\"tool-result\" content of type \"file-url\"\". The \"file-url\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'url', url } instead.",
            "AISDK_DEP_TOOL_RESULT_FILE_URL"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should emit DeprecationWarning for file-url without mediaType (inferred)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_DeprecationWarning_for_file_url_without_mediaType_inferred()
    {
        using var log = new LogWarningsRecorder();

        ConvertTool(new FileUrlContentPart("https://example.com/image.png"));

        AssertWarning(
            log,
            "\"tool-result\" content of type \"file-url\"\". The \"file-url\" tool result content part with URL \"https://example.com/image.png\" is missing a \"mediaType\". Inferred media type 'image/png' from URL. The \"file-url\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'url', url } instead.",
            "AISDK_DEP_TOOL_RESULT_FILE_URL"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should emit DeprecationWarning for file-url without mediaType (not inferable)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_DeprecationWarning_for_file_url_without_mediaType_not_inferable()
    {
        using var log = new LogWarningsRecorder();

        ConvertTool(new FileUrlContentPart("https://example.com/file"));

        AssertWarning(
            log,
            "\"tool-result\" content of type \"file-url\"\". The \"file-url\" tool result content part with URL \"https://example.com/file\" is missing a \"mediaType\". Unable to infer media type from URL. Defaulting to 'application/octet-stream'. The \"file-url\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'url', url } instead.",
            "AISDK_DEP_TOOL_RESULT_FILE_URL"
        );
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should fall back when file-url extension collides with Object.prototype (e.g. `.constructor`)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_fall_back_when_file_url_extension_collides_with_Object_prototype()
    {
        using var log = new LogWarningsRecorder();

        ConvertTool(new FileUrlContentPart("https://example.com/foo.constructor"));

        AssertWarning(
            log,
            "\"tool-result\" content of type \"file-url\"\". The \"file-url\" tool result content part with URL \"https://example.com/foo.constructor\" is missing a \"mediaType\". Unable to infer media type from URL. Defaulting to 'application/octet-stream'. The \"file-url\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'url', url } instead.",
            "AISDK_DEP_TOOL_RESULT_FILE_URL"
        );
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should emit DeprecationWarning for deprecated types in assistant message tool-result content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_DeprecationWarning_for_deprecated_types_in_assistant_message_tool_result_content()
    {
        using var log = new LogWarningsRecorder();

        LanguageModelPrompts.ConvertToLanguageModelMessage(
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolResultPart(
                        "toolCallId",
                        "toolName",
                        new ToolModelMessage(
                            "toolCallId",
                            "toolName",
                            new UserContentPart[]
                            {
                                new ImageDataContentPart("dGVzdA==", "image/png"),
                            }
                        ),
                        null
                    ),
                }
            ),
            new Dictionary<string, DownloadResult>()
        );

        AssertWarning(
            log,
            "\"tool-result\" content of type \"image-data\"\". The \"image-data\" type for tool result content is deprecated. Use the \"file\" type with mediaType and { type: 'data', data } instead.",
            "AISDK_DEP_TOOL_RESULT_IMAGE_DATA"
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should emit one DeprecationWarning per deprecated item",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_emit_one_DeprecationWarning_per_deprecated_item()
    {
        using var log = new LogWarningsRecorder();

        ConvertTool(
            new ImageDataContentPart("dGVzdA==", "image/png"),
            new ImageUrlContentPart("https://example.com/image.png")
        );

        Assert.Equal(3, log.Process.Count);
    }

    [Fact]
    [UpstreamTest(
        Id + "should not emit warnings for non-deprecated content types",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_emit_warnings_for_non_deprecated_content_types()
    {
        using var log = new LogWarningsRecorder();

        ConvertTool(
            new FileContentPart("image/png", null, new byte[] { 116, 101, 115, 116 }, null),
            new FileContentPart("image/png", "https://example.com/image.png", null, null),
            FileContentPart.FromProviderReference("application/octet-stream", TestProviderReference)
        );

        Assert.Empty(log.Process);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass the new \"file\" shape through unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_the_new_file_shape_through_unchanged()
    {
        using var log = new LogWarningsRecorder();

        var parts = ConvertTool(
            new FileContentPart("image/png", null, new byte[] { 116, 101, 115, 116 }, "image.png"),
            new FileContentPart("image/png", "https://example.com/image.png", null, null),
            FileContentPart.FromProviderReference("application/pdf", TestProviderReference),
            FileContentPart.FromText("text/plain", "inline text")
        );

        Assert.Empty(log.Process);
        Assert.Equal(4, parts.Count);
        var data = Assert.IsType<FileContentPart>(parts[0]);
        Assert.Equal("image/png", data.MediaType);
        Assert.Equal(new byte[] { 116, 101, 115, 116 }, data.Data);
        Assert.Equal("image.png", data.FileName);
        Assert.Null(data.ProviderOptions);
        var url = Assert.IsType<FileContentPart>(parts[1]);
        Assert.Equal("image/png", url.MediaType);
        Assert.Equal("https://example.com/image.png", url.Url);
        Assert.Null(url.FileName);
        Assert.Null(url.ProviderOptions);
        var reference = AssertReference(parts[2], "application/pdf");
        Assert.Null(reference.FileName);
        var text = Assert.IsType<FileContentPart>(parts[3]);
        Assert.Equal("text/plain", text.MediaType);
        Assert.Equal("inline text", text.Text);
        Assert.Null(text.FileName);
        Assert.Null(text.ProviderOptions);
    }
}
