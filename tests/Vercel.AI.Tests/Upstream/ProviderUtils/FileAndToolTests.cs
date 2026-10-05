// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Tests;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class FileAndToolTests
{
    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-inline-file-data-to-uint8-array.test.ts::convertInlineFileDataToUint8Array::converts text data to UTF-8 bytes", Coverage = UpstreamCoverage.Covered)]
    public void Inline_file_converts_text()
    {
        Assert.Equal(Encoding.UTF8.GetBytes("abc"), FileDataConversions.ConvertInlineFileDataToUint8Array(new LanguageModelFilePart("text", null) { Text = "abc" }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-inline-file-data-to-uint8-array.test.ts::convertInlineFileDataToUint8Array::returns Uint8Array data as-is", Coverage = UpstreamCoverage.Covered)]
    public void Inline_file_returns_bytes()
    {
        var bytes = new byte[] { 1, 2, 3 };
        Assert.Same(bytes, FileDataConversions.ConvertInlineFileDataToUint8Array(new LanguageModelFilePart("data", null) { Bytes = bytes }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-inline-file-data-to-uint8-array.test.ts::convertInlineFileDataToUint8Array::decodes base64 string data", Coverage = UpstreamCoverage.Covered)]
    public void Inline_file_decodes_base64()
    {
        Assert.Equal(Encoding.UTF8.GetBytes("abc"), FileDataConversions.ConvertInlineFileDataToUint8Array(new LanguageModelFilePart("data", null) { Base64 = "YWJj" }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-image-model-file-to-data-uri.test.ts::convertImageModelFileToDataUri() > URL files::should return the URL as-is for URL type files", Coverage = UpstreamCoverage.Covered)]
    public void Image_file_returns_a_url()
    {
        Assert.Equal("https://example.com/image.png", FileDataConversions.ConvertImageModelFileToDataUri(ImageModelFile.FromUrl("https://example.com/image.png")));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-image-model-file-to-data-uri.test.ts::convertImageModelFileToDataUri() > URL files::should handle URLs with query parameters", Coverage = UpstreamCoverage.Covered)]
    public void Image_file_keeps_query_parameters()
    {
        Assert.Equal("https://example.com/image.png?width=100&height=200", FileDataConversions.ConvertImageModelFileToDataUri(ImageModelFile.FromUrl("https://example.com/image.png?width=100&height=200")));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-image-model-file-to-data-uri.test.ts::convertImageModelFileToDataUri() > Uint8Array files::should convert Uint8Array to base64 and return a data URI", Coverage = UpstreamCoverage.Covered)]
    public void Image_file_encodes_bytes()
    {
        Assert.Equal("data:image/png;base64,SGVsbG8=", FileDataConversions.ConvertImageModelFileToDataUri(ImageModelFile.FromFile(new byte[] { 72, 101, 108, 108, 111 }, "image/png")));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-image-model-file-to-data-uri.test.ts::convertImageModelFileToDataUri() > Uint8Array files::should handle empty Uint8Array", Coverage = UpstreamCoverage.Covered)]
    public void Image_file_encodes_empty_bytes()
    {
        Assert.Equal("data:image/png;base64,", FileDataConversions.ConvertImageModelFileToDataUri(ImageModelFile.FromFile(Array.Empty<byte>(), "image/png")));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-image-model-file-to-data-uri.test.ts::convertImageModelFileToDataUri() > Uint8Array files::should handle different media types with Uint8Array", Coverage = UpstreamCoverage.Covered)]
    public void Image_file_encodes_webp_bytes()
    {
        Assert.Equal("data:image/webp;base64,SGVsbG8=", FileDataConversions.ConvertImageModelFileToDataUri(ImageModelFile.FromFile(new byte[] { 72, 101, 108, 108, 111 }, "image/webp")));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/create-tool-name-mapping.test.ts::createToolNameMapping::should create mappings for provider-defined tools", Coverage = UpstreamCoverage.Covered)]
    public void Tool_names_map_provider_tools()
    {
        var mapping = ToolNames.CreateToolNameMapping(
            new[]
            {
                new MappedTool("provider", "custom-computer-tool", "anthropic.computer-use"),
                new MappedTool("provider", "custom-code-tool", "openai.code-interpreter"),
            },
            new Dictionary<string, string>
            {
                ["anthropic.computer-use"] = "computer_use",
                ["openai.code-interpreter"] = "code_interpreter",
            });
        Assert.Equal("computer_use", mapping.ToProviderToolName("custom-computer-tool"));
        Assert.Equal("code_interpreter", mapping.ToProviderToolName("custom-code-tool"));
        Assert.Equal("custom-computer-tool", mapping.ToCustomToolName("computer_use"));
        Assert.Equal("custom-code-tool", mapping.ToCustomToolName("code_interpreter"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/create-tool-name-mapping.test.ts::createToolNameMapping::should ignore function tools", Coverage = UpstreamCoverage.Covered)]
    public void Tool_names_ignore_function_tools()
    {
        var mapping = ToolNames.CreateToolNameMapping(
            new[] { new MappedTool("function", "my-function-tool") },
            new Dictionary<string, string>());
        Assert.Equal("my-function-tool", mapping.ToProviderToolName("my-function-tool"));
        Assert.Equal("my-function-tool", mapping.ToCustomToolName("my-function-tool"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/create-tool-name-mapping.test.ts::createToolNameMapping::should return input name when tool is not in providerToolNames", Coverage = UpstreamCoverage.Covered)]
    public void Tool_names_keep_unknown_provider_ids()
    {
        var mapping = ToolNames.CreateToolNameMapping(
            new[] { new MappedTool("provider", "custom-tool", "unknown.tool") },
            new Dictionary<string, string>());
        Assert.Equal("custom-tool", mapping.ToProviderToolName("custom-tool"));
        Assert.Equal("unknown-name", mapping.ToCustomToolName("unknown-name"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/create-tool-name-mapping.test.ts::createToolNameMapping::should return input name when mapping does not exist", Coverage = UpstreamCoverage.Covered)]
    public void Tool_names_keep_unmapped_names()
    {
        var mapping = ToolNames.CreateToolNameMapping(
            new[] { new MappedTool("provider", "custom-computer-tool", "anthropic.computer-use") },
            new Dictionary<string, string> { ["anthropic.computer-use"] = "computer_use" });
        Assert.Equal("non-existent-tool", mapping.ToProviderToolName("non-existent-tool"));
        Assert.Equal("non-existent-provider-tool", mapping.ToCustomToolName("non-existent-provider-tool"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/create-tool-name-mapping.test.ts::createToolNameMapping::should handle empty tools array", Coverage = UpstreamCoverage.Covered)]
    public void Tool_names_handle_an_empty_list()
    {
        var mapping = ToolNames.CreateToolNameMapping(Array.Empty<MappedTool>(), new Dictionary<string, string>());
        Assert.Equal("any-tool", mapping.ToProviderToolName("any-tool"));
        Assert.Equal("any-tool", mapping.ToCustomToolName("any-tool"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/create-tool-name-mapping.test.ts::createToolNameMapping::should handle mixed function and provider-defined tools", Coverage = UpstreamCoverage.Covered)]
    public void Tool_names_mix_function_and_provider_tools()
    {
        var mapping = ToolNames.CreateToolNameMapping(
            new[]
            {
                new MappedTool("function", "function-tool"),
                new MappedTool("provider", "provider-tool", "anthropic.computer-use"),
            },
            new Dictionary<string, string> { ["anthropic.computer-use"] = "computer_use" });
        Assert.Equal("function-tool", mapping.ToProviderToolName("function-tool"));
        Assert.Equal("function-tool", mapping.ToCustomToolName("function-tool"));
        Assert.Equal("computer_use", mapping.ToProviderToolName("provider-tool"));
        Assert.Equal("provider-tool", mapping.ToCustomToolName("computer_use"));
    }
}
