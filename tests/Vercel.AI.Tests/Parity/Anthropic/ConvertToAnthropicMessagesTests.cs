// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Covers convert-to-anthropic-prompt message, cache, citation, and toolset cases.</summary>
public sealed class ConvertToAnthropicMessagesTests
{

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::system messages::should convert a single system message into an anthropic system message", Coverage = UpstreamCoverage.Covered)]
    public void Single_system_message_becomes_a_system_block()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""system"",""content"":""This is a system message""}]");
        AnthropicParity.PromptEqual(result, @"{""messages"":[],""system"":[{""type"":""text"",""text"":""This is a system message""}]}");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::system messages::should convert multiple system messages into an anthropic system message", Coverage = UpstreamCoverage.Covered)]
    public void Consecutive_system_messages_stay_in_one_system_array()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""system"",""content"":""This is a system message""},
            {""role"":""system"",""content"":""This is another system message""}]");
        AnthropicParity.PromptEqual(result, @"{""messages"":[],""system"":[
            {""type"":""text"",""text"":""This is a system message""},
            {""type"":""text"",""text"":""This is another system message""}]}");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::system messages::should emit a mid-conversation system message inline and add the beta", Coverage = UpstreamCoverage.Covered)]
    public void Mid_conversation_system_message_is_inline()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""system"",""content"":""initial""},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""hi""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""hello""}]},
            {""role"":""system"",""content"":""switch tone""},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""go""}]}]");
        AnthropicParity.JsonEqual(result.Prompt["system"], @"[{""type"":""text"",""text"":""initial""}]");
        Assert.Contains(result.Prompt["messages"]!.AsArray(), message =>
            JsonNodeEquals(message, @"{""role"":""system"",""content"":[{""type"":""text"",""text"":""switch tone""}]}"));
        Assert.Contains("mid-conversation-system-2026-04-07", result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::system messages::should serialize clearAt and effort on individual mid-conversation system messages", Coverage = UpstreamCoverage.Covered)]
    public void Mid_conversation_clear_at_and_effort_are_serialized()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""system"",""content"":""initial""},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""hi""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""hello""}]},
            {""role"":""system"",""content"":"""",""providerOptions"":{""anthropic"":{""clearAt"":""next_user_message"",""effort"":""high""}}},
            {""role"":""system"",""content"":""this instruction persists""},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""go""}]}]");
        Assert.Contains(result.Prompt["messages"]!.AsArray(), message =>
            JsonNodeEquals(message, @"{""role"":""system"",""content"":[],""clear_at"":""next_user_message"",""output_config"":{""effort"":""high""}}"));
        Assert.Contains(result.Prompt["messages"]!.AsArray(), message =>
            JsonNodeEquals(message, @"{""role"":""system"",""content"":[{""type"":""text"",""text"":""this instruction persists""}]}"));
        Assert.Contains("mid-conversation-system-clear-at-2026-08-21", result.Betas);
        Assert.Contains("mid-conversation-output-config-2026-07-01", result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::system messages::should emit tool change blocks on a mid-conversation system message and add the beta", Coverage = UpstreamCoverage.Covered)]
    public void Mid_conversation_tool_changes_add_the_beta()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""system"",""content"":""initial""},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""hi""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""hello""}]},
            {""role"":""system"",""content"":""tools have changed"",""providerOptions"":{""anthropic"":{""toolChanges"":[
                {""type"":""tool_addition"",""toolName"":""get_forecast""},
                {""type"":""tool_removal"",""toolName"":""get_weather""}]}}},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""go""}]}]");
        Assert.Contains(result.Prompt["messages"]!.AsArray(), message => JsonNodeEquals(message, @"{
            ""role"":""system"",
            ""content"":[
                {""type"":""text"",""text"":""tools have changed""},
                {""type"":""tool_addition"",""tool"":{""type"":""tool_reference"",""name"":""get_forecast""}},
                {""type"":""tool_removal"",""tool"":{""type"":""tool_reference"",""name"":""get_weather""}}]}"));
        Assert.Contains("mid-conversation-system-2026-04-07", result.Betas);
        Assert.Contains("mid-conversation-tool-changes-2026-07-01", result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::system messages::should not emit an empty text block for a system message that only carries tool changes", Coverage = UpstreamCoverage.Covered)]
    public void Tool_change_only_system_message_omits_empty_text()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""system"",""content"":""initial""},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""hi""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""hello""}]},
            {""role"":""system"",""content"":"""",""providerOptions"":{""anthropic"":{""toolChanges"":[{""type"":""tool_removal"",""toolName"":""get_weather""}]}}} ]");
        Assert.Contains(result.Prompt["messages"]!.AsArray(), message => JsonNodeEquals(message, @"{
            ""role"":""system"",
            ""content"":[{""type"":""tool_removal"",""tool"":{""type"":""tool_reference"",""name"":""get_weather""}}]}"));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::system messages::should warn and drop tool changes on the initial system message", Coverage = UpstreamCoverage.Covered)]
    public void Initial_system_tool_changes_are_dropped()
    {
        var warnings = new List<AnthropicWarning>();
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""system"",""content"":""initial"",""providerOptions"":{""anthropic"":{""toolChanges"":[{""type"":""tool_addition"",""toolName"":""get_forecast""}]}}},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""hi""}]}]", warnings: warnings);
        AnthropicParity.JsonEqual(result.Prompt["system"], @"[{""type"":""text"",""text"":""initial""}]");
        Assert.DoesNotContain("mid-conversation-tool-changes-2026-07-01", result.Betas);
        Assert.Contains(warnings, warning => warning.Type == "other" && warning.Details != null && warning.Details.Contains("initial system message"));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should add image parts for UInt8Array images", Coverage = UpstreamCoverage.Covered)]
    public void Inline_png_bytes_become_a_base64_image()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""data"",""data"":""AAECAw==""},""mediaType"":""image/png""}]}]");
        AnthropicParity.PromptEqual(result, @"{""messages"":[{""role"":""user"",""content"":[{""type"":""image"",""source"":{""type"":""base64"",""media_type"":""image/png"",""data"":""AAECAw==""}}]}]}");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should add image parts for URL images", Coverage = UpstreamCoverage.Covered)]
    public void Image_urls_stay_url_sources()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""url"",""url"":""https://example.com/image.png""},""mediaType"":""image/*""}]}]");
        AnthropicParity.PromptEqual(result, @"{""messages"":[{""role"":""user"",""content"":[{""type"":""image"",""source"":{""type"":""url"",""url"":""https://example.com/image.png""}}]}]}");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages > top-level-only media type resolution::detects image subtype from inline bytes for top-level \"image\"", Coverage = UpstreamCoverage.Covered)]
    public void Top_level_image_detects_png()
    {
        var png = Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""data"",""data"":""" + png + @"""},""mediaType"":""image""}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0], @"{""type"":""image"",""source"":{""type"":""base64"",""media_type"":""image/png"",""data"":""" + png + @"""}}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages > top-level-only media type resolution::normalizes image/* via detection", Coverage = UpstreamCoverage.Covered)]
    public void Image_star_is_detected_as_png()
    {
        var png = Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""data"",""data"":""" + png + @"""},""mediaType"":""image/*""}]}]");
        Assert.Equal("image/png", result.Prompt["messages"]![0]!["content"]![0]!["source"]!["media_type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages > top-level-only media type resolution::passes through URL for top-level-only image (Anthropic accepts URL source)", Coverage = UpstreamCoverage.Covered)]
    public void Top_level_image_url_is_passed_through()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""url"",""url"":""https://example.com/x.png""},""mediaType"":""image""}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0], @"{""type"":""image"",""source"":{""type"":""url"",""url"":""https://example.com/x.png""}}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages > top-level-only media type resolution::detects PDF subtype from inline bytes for top-level \"application\"", Coverage = UpstreamCoverage.Covered)]
    public void Top_level_application_detects_pdf()
    {
        var pdf = Convert.ToBase64String(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D });
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""data"",""data"":""" + pdf + @"""},""mediaType"":""application""}]}]");
        Assert.Equal("application/pdf", result.Prompt["messages"]![0]!["content"]![0]!["source"]!["media_type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages > top-level-only media type resolution::preserves full image/png pass-through", Coverage = UpstreamCoverage.Covered)]
    public void Full_image_png_is_passed_through()
    {
        var png = Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""data"",""data"":""" + png + @"""},""mediaType"":""image/png""}]}]");
        Assert.Equal("image/png", result.Prompt["messages"]![0]!["content"]![0]!["source"]!["media_type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages > top-level-only media type resolution::still routes text/plain inline text through document source", Coverage = UpstreamCoverage.Covered)]
    public void Inline_text_file_is_a_text_document()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""text"",""text"":""hello""},""mediaType"":""text/plain""}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0]!["source"], @"{""type"":""text"",""media_type"":""text/plain"",""data"":""hello""}");
        Assert.Equal("document", result.Prompt["messages"]![0]!["content"]![0]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should treat URL strings in image file data as URLs, not base64)", Coverage = UpstreamCoverage.Covered)]
    public void Image_url_data_is_not_base64()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""url"",""url"":""https://example.com/image.png""},""mediaType"":""image/png""}]}]");
        AnthropicParity.PromptEqual(result, @"{""messages"":[{""role"":""user"",""content"":[{""type"":""image"",""source"":{""type"":""url"",""url"":""https://example.com/image.png""}}]}]}");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should treat URL strings in PDF file data as URLs, not base64)", Coverage = UpstreamCoverage.Covered)]
    public void Pdf_url_data_is_a_document_url()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""url"",""url"":""https://example.com/document.pdf""},""mediaType"":""application/pdf""}]}]");
        AnthropicParity.PromptEqual(result, @"{""messages"":[{""role"":""user"",""content"":[{""type"":""document"",""source"":{""type"":""url"",""url"":""https://example.com/document.pdf""}}]}]}");
        Assert.Equal(new[] { "pdfs-2024-09-25" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should add PDF file parts for base64 PDFs", Coverage = UpstreamCoverage.Covered)]
    public void Base64_pdf_is_a_document()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""data"",""data"":""base64PDFdata""},""mediaType"":""application/pdf""}]}]");
        AnthropicParity.PromptEqual(result, @"{""messages"":[{""role"":""user"",""content"":[{""type"":""document"",""source"":{""type"":""base64"",""media_type"":""application/pdf"",""data"":""base64PDFdata""}}]}]}");
        Assert.Equal(new[] { "pdfs-2024-09-25" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should add PDF file parts for URL PDFs", Coverage = UpstreamCoverage.Covered)]
    public void Url_pdf_is_a_document()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""url"",""url"":""https://example.com/document.pdf""},""mediaType"":""application/pdf""}]}]");
        Assert.Equal("url", result.Prompt["messages"]![0]!["content"]![0]!["source"]!["type"]!.GetValue<string>());
        Assert.Equal(new[] { "pdfs-2024-09-25" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should add text file parts for text/plain documents", Coverage = UpstreamCoverage.Covered)]
    public void Text_plain_bytes_are_decoded_into_a_document()
    {
        var data = Convert.ToBase64String(Encoding.UTF8.GetBytes("sample text content"));
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""filename"":""sample.txt"",""mediaType"":""text/plain"",""data"":{""type"":""data"",""data"":""" + data + @"""}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0], @"{
            ""type"":""document"",
            ""source"":{""type"":""text"",""media_type"":""text/plain"",""data"":""sample text content""},
            ""title"":""sample.txt""}");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should map inline text file parts to inline text document source", Coverage = UpstreamCoverage.Covered)]
    public void Inline_text_part_keeps_its_text()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""filename"":""inline.txt"",""mediaType"":""text/plain"",""data"":{""type"":""text"",""text"":""inline text content""}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0], @"{
            ""type"":""document"",
            ""source"":{""type"":""text"",""media_type"":""text/plain"",""data"":""inline text content""},
            ""title"":""inline.txt""}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should throw error for unsupported file types", Coverage = UpstreamCoverage.Covered)]
    public void Unsupported_media_type_throws()
    {
        var error = Assert.Throws<AiSdkException>(() => AnthropicParity.ConvertPrompt(
            @"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""data"",""data"":""base64data""},""mediaType"":""video/mp4""}]}]"));
        Assert.Contains("media type: video/mp4", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should convert messages with image file parts using provider reference", Coverage = UpstreamCoverage.Covered)]
    public void Image_provider_reference_uses_a_file_id()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""mediaType"":""image/png"",""data"":{""type"":""reference"",""reference"":{""anthropic"":""file-img-12345""}}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0], @"{""type"":""image"",""source"":{""type"":""file"",""file_id"":""file-img-12345""}}");
        Assert.Equal(new[] { "files-api-2025-04-14" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should convert messages with PDF file parts using provider reference", Coverage = UpstreamCoverage.Covered)]
    public void Pdf_provider_reference_uses_a_file_id()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""mediaType"":""application/pdf"",""data"":{""type"":""reference"",""reference"":{""anthropic"":""file-pdf-12345""}}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0], @"{""type"":""document"",""source"":{""type"":""file"",""file_id"":""file-pdf-12345""}}");
        Assert.Contains("files-api-2025-04-14", result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should convert messages with text/plain file parts using provider reference", Coverage = UpstreamCoverage.Covered)]
    public void Text_provider_reference_is_a_document_file()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""mediaType"":""text/plain"",""data"":{""type"":""reference"",""reference"":{""anthropic"":""file-txt-12345""}}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0], @"{""type"":""document"",""source"":{""type"":""file"",""file_id"":""file-txt-12345""}}");
        Assert.Contains("files-api-2025-04-14", result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should convert provider referenced file parts to container uploads when requested", Coverage = UpstreamCoverage.Covered)]
    public void Container_upload_uses_the_file_id()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""mediaType"":""text/plain"",""providerOptions"":{""anthropic"":{""containerUpload"":true}},""data"":{""type"":""reference"",""reference"":{""anthropic"":""file-upload-1""}}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0], @"{""type"":""container_upload"",""file_id"":""file-upload-1""}");
        Assert.Contains("files-api-2025-04-14", result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::user messages::should throw when provider reference does not contain anthropic key", Coverage = UpstreamCoverage.Covered)]
    public void Missing_anthropic_reference_names_the_available_provider()
    {
        var error = Assert.Throws<AiSdkException>(() => AnthropicParity.ConvertPrompt(
            @"[{""role"":""user"",""content"":[{""type"":""file"",""mediaType"":""application/pdf"",""data"":{""type"":""reference"",""reference"":{""openai"":""file-xyz""}}}]}]"));
        Assert.Equal("No provider reference found for provider 'anthropic'. Available providers: openai", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::tool messages::should convert a single tool result into an anthropic user message", Coverage = UpstreamCoverage.Covered)]
    public void Single_tool_result_is_a_user_message()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""tool"",""content"":[{""type"":""tool-result"",""toolName"":""tool-1"",""toolCallId"":""tool-call-1"",""output"":{""type"":""json"",""value"":{""test"":""This is a tool message""}}}]}]");
        AnthropicParity.PromptEqual(result, @"{""messages"":[{""role"":""user"",""content"":[{""type"":""tool_result"",""tool_use_id"":""tool-call-1"",""content"":""{\""test\"":\""This is a tool message\""}""}]}]}");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::tool messages::should convert multiple tool results into an anthropic user message", Coverage = UpstreamCoverage.Covered)]
    public void Multiple_tool_results_share_one_user_message()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""tool"",""content"":[
            {""type"":""tool-result"",""toolName"":""tool-1"",""toolCallId"":""tool-call-1"",""output"":{""type"":""json"",""value"":{""test"":""This is a tool message""}}},
            {""type"":""tool-result"",""toolName"":""tool-2"",""toolCallId"":""tool-call-2"",""output"":{""type"":""json"",""value"":{""something"":""else""}}}]}]");
        AnthropicParity.MessagesEqual(result, @"[{""role"":""user"",""content"":[
            {""type"":""tool_result"",""tool_use_id"":""tool-call-1"",""content"":""{\""test\"":\""This is a tool message\""}""},
            {""type"":""tool_result"",""tool_use_id"":""tool-call-2"",""content"":""{\""something\"":\""else\""}""}]}]");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::tool messages::should combine user and tool messages", Coverage = UpstreamCoverage.Covered)]
    public void Tool_results_and_following_user_text_are_combined()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""tool"",""content"":[{""type"":""tool-result"",""toolName"":""tool-1"",""toolCallId"":""tool-call-1"",""output"":{""type"":""json"",""value"":{""test"":""This is a tool message""}}}]},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""This is a user message""}]}]");
        AnthropicParity.MessagesEqual(result, @"[{""role"":""user"",""content"":[
            {""type"":""tool_result"",""tool_use_id"":""tool-call-1"",""content"":""{\""test\"":\""This is a tool message\""}""},
            {""type"":""text"",""text"":""This is a user message""}]}]");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::tool messages::should handle tool result with content parts", Coverage = UpstreamCoverage.Covered)]
    public void Tool_content_can_include_text_and_an_image()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""tool"",""content"":[{""type"":""tool-result"",""toolName"":""image-generator"",""toolCallId"":""image-gen-1"",""output"":{""type"":""content"",""value"":[
            {""type"":""text"",""text"":""Image generated successfully""},
            {""type"":""file"",""data"":{""type"":""data"",""data"":""AAECAw==""},""mediaType"":""image/png""}]}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0], @"{
            ""type"":""tool_result"",""tool_use_id"":""image-gen-1"",
            ""content"":[
                {""type"":""text"",""text"":""Image generated successfully""},
                {""type"":""image"",""source"":{""type"":""base64"",""media_type"":""image/png"",""data"":""AAECAw==""}}]}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::tool messages::should handle tool result with PDF content", Coverage = UpstreamCoverage.Covered)]
    public void Tool_pdf_content_adds_the_pdf_beta()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""tool"",""content"":[{""type"":""tool-result"",""toolName"":""pdf-generator"",""toolCallId"":""pdf-gen-1"",""output"":{""type"":""content"",""value"":[
            {""type"":""text"",""text"":""PDF generated successfully""},
            {""type"":""file"",""mediaType"":""application/pdf"",""data"":{""type"":""data"",""data"":""JVBERi0xLjQKJeLjz9MKNCAwIG9iago=""}}]}}]}]");
        Assert.Equal(new[] { "pdfs-2024-09-25" }, result.Betas);
        Assert.Equal("document", result.Prompt["messages"]![0]!["content"]![0]!["content"]![1]!["type"]!.GetValue<string>());
        Assert.Equal("application/pdf", result.Prompt["messages"]![0]!["content"]![0]!["content"]![1]!["source"]!["media_type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::tool messages::should handle tool result with custom tool-reference content for custom tool search", Coverage = UpstreamCoverage.Covered)]
    public void Custom_tool_references_are_tool_reference_blocks()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""tool"",""content"":[{""type"":""tool-result"",""toolName"":""searchTools"",""toolCallId"":""search-1"",""output"":{""type"":""content"",""value"":[
            {""type"":""custom"",""providerOptions"":{""anthropic"":{""type"":""tool-reference"",""toolName"":""get_weather""}}},
            {""type"":""custom"",""providerOptions"":{""anthropic"":{""type"":""tool-reference"",""toolName"":""get_forecast""}}}]}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0]!["content"], @"[
            {""type"":""tool_reference"",""tool_name"":""get_weather""},
            {""type"":""tool_reference"",""tool_name"":""get_forecast""}]");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::tool messages::should handle tool result with url-based PDF content", Coverage = UpstreamCoverage.Covered)]
    public void Tool_pdf_url_content_is_a_document_url()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""tool"",""content"":[{""type"":""tool-result"",""toolName"":""get-pdf"",""toolCallId"":""get-pdf-1"",""output"":{""type"":""content"",""value"":[
            {""type"":""file"",""mediaType"":""application/pdf"",""data"":{""type"":""url"",""url"":""https://example.com/document.pdf""}}]}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0]!["content"]![0], @"{""type"":""document"",""source"":{""type"":""url"",""url"":""https://example.com/document.pdf""}}");
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::tool messages::should handle tool result with url-based image content", Coverage = UpstreamCoverage.Covered)]
    public void Tool_image_url_content_is_an_image_url()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""tool"",""content"":[{""type"":""tool-result"",""toolName"":""image-generator"",""toolCallId"":""image-gen-1"",""output"":{""type"":""content"",""value"":[
            {""type"":""file"",""mediaType"":""image/png"",""data"":{""type"":""url"",""url"":""https://example.com/image.png""}}]}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0]!["content"]![0], @"{""type"":""image"",""source"":{""type"":""url"",""url"":""https://example.com/image.png""}}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should omit empty compaction blocks", Coverage = UpstreamCoverage.Covered)]
    public void Empty_compaction_blocks_are_omitted()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""user content""}]},
            {""role"":""assistant"",""content"":[
                {""type"":""text"",""text"":"""",""providerOptions"":{""anthropic"":{""type"":""compaction""}}},
                {""type"":""text"",""text"":""assistant content""}]}]");
        AnthropicParity.MessagesEqual(result, @"[
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""user content""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""assistant content""}]}]");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should omit an assistant message that only contains an empty compaction block", Coverage = UpstreamCoverage.Covered)]
    public void Assistant_message_of_only_empty_compaction_is_omitted()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""first user message""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":"""",""providerOptions"":{""anthropic"":{""type"":""compaction""}}}]},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""second user message""}]}]");
        AnthropicParity.MessagesEqual(result, @"[
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""first user message""}]},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""second user message""}]}]");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should preserve non-empty compaction blocks", Coverage = UpstreamCoverage.Covered)]
    public void Non_empty_compaction_is_preserved()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""assistant"",""content"":[{""type"":""text"",""text"":""Summary of the conversation"",""providerOptions"":{""anthropic"":{""type"":""compaction""}}}]}]");
        AnthropicParity.MessagesEqual(result, @"[{""role"":""assistant"",""content"":[{""type"":""compaction"",""content"":""Summary of the conversation""}]}]");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should preserve citations on assistant text", Coverage = UpstreamCoverage.Covered)]
    public void Assistant_text_citations_are_passed_through()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""The Federal Reserve held rates steady."",""providerOptions"":{""anthropic"":{""citations"":[
                {""type"":""web_search_result_location"",""cited_text"":""The Committee decided to maintain the rate."",""url"":""https://example.com/fed-decision"",""title"":""Federal Reserve decision"",""encrypted_index"":""encrypted-index""}]}}}]},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""What happened before that?""}]}]");
        Assert.Equal("web_search_result_location", result.Prompt["messages"]![0]!["content"]![0]!["citations"]![0]!["type"]!.GetValue<string>());
        Assert.Equal("What happened before that?", result.Prompt["messages"]![1]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should remove trailing whitespace from last assistant message when there is no further user message", Coverage = UpstreamCoverage.Covered)]
    public void Trailing_whitespace_is_trimmed_on_the_final_assistant_text()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""user content""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""assistant content  ""}]}]");
        Assert.Equal("assistant content", result.Prompt["messages"]![1]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should remove trailing whitespace from last assistant message with multi-part content when there is no further user message", Coverage = UpstreamCoverage.Covered)]
    public void Only_the_last_assistant_text_part_is_trimmed()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""user content""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""assistant ""},{""type"":""text"",""text"":""content  ""}]}]");
        Assert.Equal("assistant ", result.Prompt["messages"]![1]!["content"]![0]!["text"]!.GetValue<string>());
        Assert.Equal("content", result.Prompt["messages"]![1]!["content"]![1]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should keep trailing whitespace from assistant message when there is a further user message", Coverage = UpstreamCoverage.Covered)]
    public void Trailing_whitespace_is_kept_when_a_user_message_follows()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""user content""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""assistant content  ""}]},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""user content 2""}]}]");
        Assert.Equal("assistant content  ", result.Prompt["messages"]![1]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should combine multiple sequential assistant messages into a single message", Coverage = UpstreamCoverage.Covered)]
    public void Sequential_assistant_messages_are_one_message()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""Hi!""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""Hello""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""World""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""!""}]}]");
        AnthropicParity.MessagesEqual(result, @"[
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""Hi!""}]},
            {""role"":""assistant"",""content"":[
                {""type"":""text"",""text"":""Hello""},
                {""type"":""text"",""text"":""World""},
                {""type"":""text"",""text"":""!""}]}]");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should convert assistant message reasoning parts with signature into thinking parts when sendReasoning is true", Coverage = UpstreamCoverage.Covered)]
    public void Signed_reasoning_becomes_thinking()
    {
        var warnings = new List<AnthropicWarning>();
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""assistant"",""content"":[
            {""type"":""reasoning"",""text"":""I need to count the number of \""r\""s in the word \""strawberry\""."",""providerOptions"":{""anthropic"":{""signature"":""test-signature""}}},
            {""type"":""text"",""text"":""The word \""strawberry\"" has 2 \""r\""s.""}]}]", warnings: warnings);
        AnthropicParity.MessagesEqual(result, @"[{""role"":""assistant"",""content"":[
            {""type"":""thinking"",""thinking"":""I need to count the number of \""r\""s in the word \""strawberry\""."",""signature"":""test-signature""},
            {""type"":""text"",""text"":""The word \""strawberry\"" has 2 \""r\""s.""}]}]");
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should ignore reasoning parts without signature into thinking parts when sendReasoning is true", Coverage = UpstreamCoverage.Covered)]
    public void Unsigned_reasoning_warns_and_is_dropped()
    {
        var warnings = new List<AnthropicWarning>();
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""assistant"",""content"":[
            {""type"":""reasoning"",""text"":""I need to count the number of \""r\""s in the word \""strawberry\"".""},
            {""type"":""text"",""text"":""The word \""strawberry\"" has 2 \""r\""s.""}]}]", warnings: warnings);
        Assert.Equal("text", result.Prompt["messages"]![0]!["content"]![0]!["type"]!.GetValue<string>());
        Assert.Equal("unsupported reasoning metadata", warnings[0].Details);
        Assert.Equal("other", warnings[0].Type);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should omit assistant message reasoning parts with signature when sendReasoning is false", Coverage = UpstreamCoverage.Covered)]
    public void Signed_reasoning_is_omitted_when_sending_is_disabled()
    {
        var warnings = new List<AnthropicWarning>();
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""assistant"",""content"":[
            {""type"":""reasoning"",""text"":""hidden"",""providerOptions"":{""anthropic"":{""signature"":""test-signature""}}},
            {""type"":""text"",""text"":""The word \""strawberry\"" has 2 \""r\""s.""}]}]", sendReasoning: false, warnings: warnings);
        Assert.Single(result.Prompt["messages"]![0]!["content"]!.AsArray());
        Assert.Equal("sending reasoning content is disabled for this model", warnings[0].Details);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::should omit reasoning parts without signature when sendReasoning is false", Coverage = UpstreamCoverage.Covered)]
    public void Unsigned_reasoning_is_omitted_when_sending_is_disabled()
    {
        var warnings = new List<AnthropicWarning>();
        AnthropicParity.ConvertPrompt(@"[{""role"":""assistant"",""content"":[
            {""type"":""reasoning"",""text"":""hidden""},
            {""type"":""text"",""text"":""visible""}]}]", sendReasoning: false, warnings: warnings);
        Assert.Equal("sending reasoning content is disabled for this model", warnings[0].Details);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > system message::should set cache_control on system message with message cache control", Coverage = UpstreamCoverage.Covered)]
    public void System_cache_control_is_copied()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""system"",""content"":""system message"",""providerOptions"":{""anthropic"":{""cacheControl"":{""type"":""ephemeral""}}}}]");
        AnthropicParity.JsonEqual(result.Prompt["system"], @"[{""type"":""text"",""text"":""system message"",""cache_control"":{""type"":""ephemeral""}}]");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > user message::should set cache_control on user message part with part cache control", Coverage = UpstreamCoverage.Covered)]
    public void User_part_cache_control_is_copied()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""text"",""text"":""test"",""providerOptions"":{""anthropic"":{""cacheControl"":{""type"":""ephemeral""}}}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0], @"{""type"":""text"",""text"":""test"",""cache_control"":{""type"":""ephemeral""}}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > user message::should set cache_control on last user message part with message cache control", Coverage = UpstreamCoverage.Covered)]
    public void Message_cache_control_lands_on_the_last_user_part()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""text"",""text"":""part1""},{""type"":""text"",""text"":""part2""}],""providerOptions"":{""anthropic"":{""cacheControl"":{""type"":""ephemeral""}}}}]");
        Assert.Null(result.Prompt["messages"]![0]!["content"]![0]!["cache_control"]);
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![1]!["cache_control"], @"{""type"":""ephemeral""}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > assistant message::should set cache_control on assistant message text part with part cache control", Coverage = UpstreamCoverage.Covered)]
    public void Assistant_text_cache_control_is_copied()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""user-content""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""test"",""providerOptions"":{""anthropic"":{""cacheControl"":{""type"":""ephemeral""}}}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![1]!["content"]![0]!["cache_control"], @"{""type"":""ephemeral""}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > assistant message::should set cache_control on assistant tool call part with part cache control", Coverage = UpstreamCoverage.Covered)]
    public void Assistant_tool_call_cache_control_is_copied()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""user-content""}]},
            {""role"":""assistant"",""content"":[{""type"":""tool-call"",""toolCallId"":""test-id"",""toolName"":""test-tool"",""input"":{""some"":""arg""},""providerOptions"":{""anthropic"":{""cacheControl"":{""type"":""ephemeral""}}}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![1]!["content"]![0], @"{
            ""type"":""tool_use"",""id"":""test-id"",""name"":""test-tool"",""input"":{""some"":""arg""},""cache_control"":{""type"":""ephemeral""}}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > assistant message::should wrap non-object (invalid) tool call input in an object", Coverage = UpstreamCoverage.Covered)]
    public void Invalid_tool_input_is_wrapped()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""assistant"",""content"":[{""type"":""tool-call"",""toolCallId"":""call-1"",""toolName"":""cityAttractions"",""input"":""{ \""city\"": \""San Francisco\"", }""}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0]!["input"], @"{""rawInvalidInput"":""{ \""city\"": \""San Francisco\"", }""}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > assistant message::should set cache_control on last assistant message part with message cache control", Coverage = UpstreamCoverage.Covered)]
    public void Assistant_message_cache_control_lands_on_the_last_part()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""user-content""}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""part1""},{""type"":""text"",""text"":""part2""}],""providerOptions"":{""anthropic"":{""cacheControl"":{""type"":""ephemeral""}}}}]");
        Assert.Null(result.Prompt["messages"]![1]!["content"]![0]!["cache_control"]);
        AnthropicParity.JsonEqual(result.Prompt["messages"]![1]!["content"]![1]!["cache_control"], @"{""type"":""ephemeral""}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > tool message::should set cache_control on tool result message part with part cache control", Coverage = UpstreamCoverage.Covered)]
    public void Tool_result_part_cache_control_is_copied()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""tool"",""content"":[{""type"":""tool-result"",""toolName"":""test"",""toolCallId"":""test"",""output"":{""type"":""json"",""value"":{""test"":""test""}},""providerOptions"":{""anthropic"":{""cacheControl"":{""type"":""ephemeral""}}}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0]!["cache_control"], @"{""type"":""ephemeral""}");
        Assert.Equal(@"{""test"":""test""}", result.Prompt["messages"]![0]!["content"]![0]!["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > tool message::should set cache_control on tool result with output cache control", Coverage = UpstreamCoverage.Covered)]
    public void Tool_output_cache_control_is_copied()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""tool"",""content"":[{""type"":""tool-result"",""toolName"":""test"",""toolCallId"":""test"",""output"":{""type"":""text"",""value"":""test"",""providerOptions"":{""anthropic"":{""cacheControl"":{""type"":""ephemeral""}}}}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0]!["cache_control"], @"{""type"":""ephemeral""}");
        Assert.Equal("test", result.Prompt["messages"]![0]!["content"]![0]!["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > tool message::should set cache_control on tool result with content output cache control", Coverage = UpstreamCoverage.Covered)]
    public void Tool_content_output_cache_control_is_copied()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""tool"",""content"":[{""type"":""tool-result"",""toolName"":""test"",""toolCallId"":""test"",""output"":{""type"":""content"",""value"":[{""type"":""text"",""text"":""test"",""providerOptions"":{""anthropic"":{""cacheControl"":{""type"":""ephemeral""}}}}]}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0]!["cache_control"], @"{""type"":""ephemeral""}");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0]!["content"], @"[{""type"":""text"",""text"":""test""}]");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > tool message::should set cache_control on last tool result message part with message cache control", Coverage = UpstreamCoverage.Covered)]
    public void Tool_message_cache_control_lands_on_the_last_result()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""tool"",""content"":[
            {""type"":""tool-result"",""toolName"":""test"",""toolCallId"":""part1"",""output"":{""type"":""json"",""value"":{""test"":""part1""}}},
            {""type"":""tool-result"",""toolName"":""test"",""toolCallId"":""part2"",""output"":{""type"":""json"",""value"":{""test"":""part2""}}}],
            ""providerOptions"":{""anthropic"":{""cacheControl"":{""type"":""ephemeral""}}}}]");
        Assert.Null(result.Prompt["messages"]![0]!["content"]![0]!["cache_control"]);
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![1]!["cache_control"], @"{""type"":""ephemeral""}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > cache control validation::should reject cache_control on thinking blocks", Coverage = UpstreamCoverage.Covered)]
    public void Thinking_cache_control_is_ignored()
    {
        var validator = new AnthropicCacheControlValidator();
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""assistant"",""content"":[{""type"":""reasoning"",""text"":""thinking content"",""providerOptions"":{""anthropic"":{""signature"":""test-sig"",""cacheControl"":{""type"":""ephemeral""}}}}]}]", cacheControl: validator);
        Assert.Null(result.Prompt["messages"]![0]!["content"]![0]!["cache_control"]);
        Assert.Equal("thinking", result.Prompt["messages"]![0]!["content"]![0]!["type"]!.GetValue<string>());
        Assert.Contains(validator.Warnings, warning => warning.Feature == "cache_control on non-cacheable context" && warning.Details == "cache_control cannot be set on thinking block. It will be ignored.");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > cache control validation::should reject cache_control on redacted thinking blocks", Coverage = UpstreamCoverage.Covered)]
    public void Redacted_thinking_cache_control_is_ignored()
    {
        var validator = new AnthropicCacheControlValidator();
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""assistant"",""content"":[{""type"":""reasoning"",""text"":""redacted"",""providerOptions"":{""anthropic"":{""redactedData"":""abc123"",""cacheControl"":{""type"":""ephemeral""}}}}]}]", cacheControl: validator);
        Assert.Null(result.Prompt["messages"]![0]!["content"]![0]!["cache_control"]);
        Assert.Equal("redacted_thinking", result.Prompt["messages"]![0]!["content"]![0]!["type"]!.GetValue<string>());
        Assert.Contains(validator.Warnings, warning => warning.Details == "cache_control cannot be set on redacted thinking block. It will be ignored.");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control::should limit cache breakpoints to 4", Coverage = UpstreamCoverage.Covered)]
    public void Fifth_cache_breakpoint_is_dropped()
    {
        var validator = new AnthropicCacheControlValidator();
        var cache = @"""providerOptions"":{""anthropic"":{""cacheControl"":{""type"":""ephemeral""}}}";
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""system"",""content"":""system 1""," + cache + @"},
            {""role"":""system"",""content"":""system 2""," + cache + @"},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""user 1""," + cache + @"}]},
            {""role"":""assistant"",""content"":[{""type"":""text"",""text"":""assistant 1""," + cache + @"}]},
            {""role"":""user"",""content"":[{""type"":""text"",""text"":""user 2 (should be rejected)""," + cache + @"}]}]", cacheControl: validator);
        Assert.Equal("ephemeral", result.Prompt["system"]![0]!["cache_control"]!["type"]!.GetValue<string>());
        Assert.Equal("ephemeral", result.Prompt["system"]![1]!["cache_control"]!["type"]!.GetValue<string>());
        Assert.Equal("ephemeral", result.Prompt["messages"]![0]!["content"]![0]!["cache_control"]!["type"]!.GetValue<string>());
        Assert.Equal("ephemeral", result.Prompt["messages"]![1]!["content"]![0]!["cache_control"]!["type"]!.GetValue<string>());
        Assert.Null(result.Prompt["messages"]![2]!["content"]![0]!["cache_control"]);
        Assert.Contains(validator.Warnings, warning => warning.Details == "Maximum 4 cache breakpoints exceeded (found 5). This breakpoint will be ignored.");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::citations::should not include citations by default", Coverage = UpstreamCoverage.Covered)]
    public void Documents_omit_citations_by_default()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""data"":{""type"":""data"",""data"":""base64PDFdata""},""mediaType"":""application/pdf""}]}]");
        Assert.Null(result.Prompt["messages"]![0]!["content"]![0]!["citations"]);
        Assert.Equal(new[] { "pdfs-2024-09-25" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::citations::should include citations when enabled on file part", Coverage = UpstreamCoverage.Covered)]
    public void Enabled_citations_are_sent()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""mediaType"":""application/pdf"",""data"":{""type"":""data"",""data"":""base64PDFdata""},""providerOptions"":{""anthropic"":{""citations"":{""enabled"":true}}}}]}]");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0]!["citations"], @"{""enabled"":true}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::citations::should include custom title and context when provided", Coverage = UpstreamCoverage.Covered)]
    public void Document_title_and_context_override_the_filename()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[{""type"":""file"",""filename"":""original-name.pdf"",""mediaType"":""application/pdf"",""data"":{""type"":""data"",""data"":""base64PDFdata""},""providerOptions"":{""anthropic"":{""title"":""Custom Document Title"",""context"":""This is metadata about the document"",""citations"":{""enabled"":true}}}}]}]");
        var document = result.Prompt["messages"]![0]!["content"]![0]!;
        Assert.Equal("Custom Document Title", document["title"]!.GetValue<string>());
        Assert.Equal("This is metadata about the document", document["context"]!.GetValue<string>());
        Assert.True(document["citations"]!["enabled"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::citations::should handle multiple documents with consistent citation settings", Coverage = UpstreamCoverage.Covered)]
    public void Multiple_documents_keep_their_own_citation_metadata()
    {
        var result = AnthropicParity.ConvertPrompt(@"[{""role"":""user"",""content"":[
            {""type"":""file"",""filename"":""doc1.pdf"",""mediaType"":""application/pdf"",""data"":{""type"":""data"",""data"":""base64PDFdata1""},""providerOptions"":{""anthropic"":{""citations"":{""enabled"":true},""title"":""Custom Title 1""}}},
            {""type"":""file"",""filename"":""doc2.pdf"",""mediaType"":""application/pdf"",""data"":{""type"":""data"",""data"":""base64PDFdata2""},""providerOptions"":{""anthropic"":{""citations"":{""enabled"":true},""title"":""Custom Title 2"",""context"":""Additional context for document 2""}}},
            {""type"":""text"",""text"":""Analyze both documents""}]}]");
        var content = result.Prompt["messages"]![0]!["content"]!;
        Assert.Equal("Custom Title 1", content[0]!["title"]!.GetValue<string>());
        Assert.Equal("Custom Title 2", content[1]!["title"]!.GetValue<string>());
        Assert.Equal("Additional context for document 2", content[1]!["context"]!.GetValue<string>());
        Assert.Equal("Analyze both documents", content[2]!["text"]!.GetValue<string>());
        Assert.Equal(new[] { "pdfs-2024-09-25" }, result.Betas);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::toolsets::should serialize toolset tool calls and results with toolset_name", Coverage = UpstreamCoverage.Covered)]
    public void Toolset_calls_use_the_action_name()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""assistant"",""content"":[{""type"":""tool-call"",""toolCallId"":""toolu_click"",""toolName"":""computer"",""input"":{""action"":""left_click"",""coordinate"":[640,60]}}]},
            {""role"":""tool"",""content"":[{""type"":""tool-result"",""toolCallId"":""toolu_click"",""toolName"":""computer"",""output"":{""type"":""text"",""value"":""OK""}}]}]",
            toolsetNames: new Dictionary<string, string> { ["computer"] = "computer" });
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0], @"{""type"":""tool_use"",""id"":""toolu_click"",""name"":""left_click"",""toolset_name"":""computer"",""input"":{""coordinate"":[640,60]}}");
        AnthropicParity.JsonEqual(result.Prompt["messages"]![1]!["content"]![0], @"{""type"":""tool_result"",""tool_use_id"":""toolu_click"",""toolset_name"":""computer"",""content"":""OK""}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::toolsets::should detect toolset tool calls through provider metadata when the tool is not passed", Coverage = UpstreamCoverage.Covered)]
    public void Toolset_name_can_come_from_provider_metadata()
    {
        var result = AnthropicParity.ConvertPrompt(@"[
            {""role"":""assistant"",""content"":[{""type"":""tool-call"",""toolCallId"":""toolu_screenshot"",""toolName"":""desktop"",""input"":{""action"":""screenshot""},""providerOptions"":{""anthropic"":{""toolsetName"":""computer""}}}]},
            {""role"":""tool"",""content"":[{""type"":""tool-result"",""toolCallId"":""toolu_screenshot"",""toolName"":""desktop"",""output"":{""type"":""text"",""value"":""OK""},""providerOptions"":{""anthropic"":{""toolsetName"":""computer""}}}]}]");
        Assert.Equal("screenshot", result.Prompt["messages"]![0]!["content"]![0]!["name"]!.GetValue<string>());
        Assert.Equal("computer", result.Prompt["messages"]![0]!["content"]![0]!["toolset_name"]!.GetValue<string>());
        Assert.Equal("computer", result.Prompt["messages"]![1]!["content"]![0]!["toolset_name"]!.GetValue<string>());
        AnthropicParity.JsonEqual(result.Prompt["messages"]![0]!["content"]![0]!["input"], "{}");
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/convert-to-anthropic-prompt.test.ts::toolsets::should warn and skip toolset tool calls without an action", Coverage = UpstreamCoverage.Covered)]
    public void Toolset_call_without_an_action_is_skipped()
    {
        var warnings = new List<AnthropicWarning>();
        var result = AnthropicParity.ConvertPrompt(
            @"[{""role"":""assistant"",""content"":[{""type"":""tool-call"",""toolCallId"":""toolu_bad"",""toolName"":""computer"",""input"":{""coordinate"":[1,2]}}]}]",
            warnings: warnings,
            toolsetNames: new Dictionary<string, string> { ["computer"] = "computer" });
        Assert.Empty(result.Prompt["messages"]!.AsArray());
        Assert.Equal("toolset tool call for tool computer is missing the action", warnings[0].Details);
    }

    private static bool JsonNodeEquals(JsonNode? actual, string expected)
    {
        return JsonNode.DeepEquals(JsonNode.Parse(expected), actual);
    }
}
