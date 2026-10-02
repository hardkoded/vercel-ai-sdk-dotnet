// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class FileAndToolParityTests
{
    [Fact]
    [UpstreamTest("packages/provider-utils/src/uint8-utils.test.ts::convertUint8ArrayToBase64::converts a byte array to base64", Coverage = UpstreamCoverage.Covered)]
    public void Uint8_encodes_hello()
    {
        Assert.Equal("SGVsbG8=", Uint8Utils.ConvertUint8ArrayToBase64(new byte[] { 72, 101, 108, 108, 111 }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/uint8-utils.test.ts::convertUint8ArrayToBase64::handles an empty array", Coverage = UpstreamCoverage.Covered)]
    public void Uint8_encodes_an_empty_array()
    {
        Assert.Equal(string.Empty, Uint8Utils.ConvertUint8ArrayToBase64(Array.Empty<byte>()));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/uint8-utils.test.ts::convertUint8ArrayToBase64::round-trips arrays larger than a single conversion chunk", Coverage = UpstreamCoverage.Covered)]
    public void Uint8_round_trips_a_large_array()
    {
        var bytes = new byte[100000];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(i % 256);
        }

        var decoded = Uint8Utils.ConvertBase64ToUint8Array(Uint8Utils.ConvertUint8ArrayToBase64(bytes));
        Assert.Equal(bytes, decoded);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/uint8-utils.test.ts::convertBase64ToUint8Array::converts base64 to a byte array", Coverage = UpstreamCoverage.Covered)]
    public void Uint8_decodes_hello()
    {
        Assert.Equal(new byte[] { 72, 101, 108, 108, 111 }, Uint8Utils.ConvertBase64ToUint8Array("SGVsbG8="));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/uint8-utils.test.ts::convertBase64ToUint8Array::supports base64url characters", Coverage = UpstreamCoverage.Covered)]
    public void Uint8_decodes_base64url()
    {
        Assert.Equal(new byte[] { 251, 255 }, Uint8Utils.ConvertBase64ToUint8Array("-_8="));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/uint8-utils.test.ts::convertBase64ToUint8Array::handles an empty string", Coverage = UpstreamCoverage.Covered)]
    public void Uint8_decodes_an_empty_string()
    {
        Assert.Empty(Uint8Utils.ConvertBase64ToUint8Array(string.Empty));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/uint8-utils.test.ts::convertToBase64::returns base64 strings unchanged", Coverage = UpstreamCoverage.Covered)]
    public void Convert_to_base64_keeps_text()
    {
        Assert.Equal("SGVsbG8=", Uint8Utils.ConvertToBase64("SGVsbG8="));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/uint8-utils.test.ts::convertToBase64::converts byte arrays to base64", Coverage = UpstreamCoverage.Covered)]
    public void Convert_to_base64_encodes_bytes()
    {
        Assert.Equal("SGVsbG8=", Uint8Utils.ConvertToBase64(new byte[] { 72, 101, 108, 108, 111 }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-inline-file-data-to-uint8-array.test.ts::convertInlineFileDataToUint8Array::converts text data to UTF-8 bytes", Coverage = UpstreamCoverage.Covered)]
    public void Inline_file_converts_text()
    {
        Assert.Equal(Encoding.UTF8.GetBytes("abc"), FileDataConversions.ConvertInlineFileDataToUint8Array(new InlineFileData("text", text: "abc")));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-inline-file-data-to-uint8-array.test.ts::convertInlineFileDataToUint8Array::returns Uint8Array data as-is", Coverage = UpstreamCoverage.Covered)]
    public void Inline_file_returns_bytes()
    {
        var bytes = new byte[] { 1, 2, 3 };
        Assert.Same(bytes, FileDataConversions.ConvertInlineFileDataToUint8Array(new InlineFileData("data", data: bytes)));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-inline-file-data-to-uint8-array.test.ts::convertInlineFileDataToUint8Array::decodes base64 string data", Coverage = UpstreamCoverage.Covered)]
    public void Inline_file_decodes_base64()
    {
        Assert.Equal(Encoding.UTF8.GetBytes("abc"), FileDataConversions.ConvertInlineFileDataToUint8Array(new InlineFileData("data", data: "YWJj")));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-inline-file-data-to-uint8-array.test.ts::convertInlineFileDataToUint8Array::rejects stream data with UnsupportedFunctionalityError and cancels the stream", Coverage = UpstreamCoverage.Covered)]
    public void Inline_file_rejects_a_stream_and_cancels_it()
    {
        var cancelled = false;
        var error = Assert.Throws<UnsupportedFunctionalityError>(() => FileDataConversions.ConvertInlineFileDataToUint8Array(new InlineFileData("stream", stream: Stream.Null, onCancel: _ => cancelled = true)));
        Assert.True(UnsupportedFunctionalityError.IsInstance(error));
        Assert.True(cancelled);
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
    [UpstreamTest("packages/provider-utils/src/convert-image-model-file-to-data-uri.test.ts::convertImageModelFileToDataUri() > base64 string files::should return a data URI for base64 string data", Coverage = UpstreamCoverage.Covered)]
    public void Image_file_wraps_base64()
    {
        const string data = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
        Assert.Equal("data:image/png;base64," + data, FileDataConversions.ConvertImageModelFileToDataUri(ImageModelFile.File("image/png", data)));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-image-model-file-to-data-uri.test.ts::convertImageModelFileToDataUri() > base64 string files::should handle different media types", Coverage = UpstreamCoverage.Covered)]
    public void Image_file_uses_the_media_type()
    {
        Assert.Equal("data:image/jpeg;base64,base64data", FileDataConversions.ConvertImageModelFileToDataUri(ImageModelFile.File("image/jpeg", "base64data")));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-image-model-file-to-data-uri.test.ts::convertImageModelFileToDataUri() > Uint8Array files::should convert Uint8Array to base64 and return a data URI", Coverage = UpstreamCoverage.Covered)]
    public void Image_file_encodes_bytes()
    {
        Assert.Equal("data:image/png;base64,SGVsbG8=", FileDataConversions.ConvertImageModelFileToDataUri(ImageModelFile.File("image/png", new byte[] { 72, 101, 108, 108, 111 })));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-image-model-file-to-data-uri.test.ts::convertImageModelFileToDataUri() > Uint8Array files::should handle empty Uint8Array", Coverage = UpstreamCoverage.Covered)]
    public void Image_file_encodes_empty_bytes()
    {
        Assert.Equal("data:image/png;base64,", FileDataConversions.ConvertImageModelFileToDataUri(ImageModelFile.File("image/png", Array.Empty<byte>())));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/convert-image-model-file-to-data-uri.test.ts::convertImageModelFileToDataUri() > Uint8Array files::should handle different media types with Uint8Array", Coverage = UpstreamCoverage.Covered)]
    public void Image_file_encodes_webp_bytes()
    {
        Assert.Equal("data:image/webp;base64,SGVsbG8=", FileDataConversions.ConvertImageModelFileToDataUri(ImageModelFile.File("image/webp", new byte[] { 72, 101, 108, 108, 111 })));
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
