// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenAICompatibleChatMessages;

/// <summary>Port of <c>convert-to-openai-compatible-chat-messages.test.ts</c> &gt; <c>tool calls</c> (multi-part tool content).</summary>
public sealed class ToolCallsTests
{
    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::tool calls::should stringify multi-part tool content by default", Coverage = UpstreamCoverage.Covered)]
    public void Should_stringify_multi_part_tool_content_by_default()
    {
        var prompt = new ModelMessage[]
        {
            new ToolModelMessage("call-1", "useImage", new UserContentPart[]
            {
                new TextContentPart("image result"),
                new FileContentPart("image/png", null, Convert.FromBase64String("iVBORw0KGgo="), null),
            }),
        };

        var result = OpenAICompatibleChat.ConvertMessages(prompt, "openaiCompatible");

        Assert.Equal("tool", result[0]!["role"]!.GetValue<string>());
        Assert.Equal("call-1", result[0]!["tool_call_id"]!.GetValue<string>());
        Assert.Equal(
            "[{\"type\":\"text\",\"text\":\"image result\"},{\"type\":\"file\",\"data\":{\"type\":\"data\",\"data\":\"iVBORw0KGgo=\"},\"mediaType\":\"image/png\"}]",
            result[0]!["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::tool calls::should convert multi-part tool content when supported", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_multi_part_tool_content_when_supported()
    {
        var reference = new Dictionary<string, string> { ["custom-provider"] = "file-123" };
        var prompt = new ModelMessage[]
        {
            new ToolModelMessage("call-1", "useImage", new UserContentPart[]
            {
                new TextContentPart("image result"),
                FileContentPart.FromProviderReference("application/pdf", reference),
                new FileContentPart("image/png", null, Convert.FromBase64String("iVBORw0KGgo="), null),
                new FileContentPart("image/png", "https://example.com/image.png", null, null),
            }),
        };

        var result = OpenAICompatibleChat.ConvertMessages(prompt, "openaiCompatible", "custom-provider", supportsMultiPartToolContent: true);

        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(
                "[{\"role\":\"tool\",\"tool_call_id\":\"call-1\",\"content\":["
                + "{\"type\":\"text\",\"text\":\"image result\"},"
                + "{\"type\":\"file\",\"file\":{\"file_id\":\"file-123\"}},"
                + "{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,iVBORw0KGgo=\"}},"
                + "{\"type\":\"image_url\",\"image_url\":{\"url\":\"https://example.com/image.png\"}}]}]"),
            result));
    }

    [Fact]
    public void Tool_content_snapshots_file_values_and_omits_provider_options()
    {
        using var options = System.Text.Json.JsonDocument.Parse("{\"custom\":\"value\"}");
        var providerOptions = new Dictionary<string, System.Text.Json.JsonElement> { ["openaiCompatible"] = options.RootElement.Clone() };
        var reference = new Dictionary<string, string> { ["custom-provider"] = "file-123" };
        var bytes = new byte[] { 1, 2, 3 };
        var message = new ToolModelMessage("call-1", "useImage", new UserContentPart[]
        {
            FileContentPart.FromProviderReference("application/pdf", reference, null, providerOptions),
            new FileContentPart("image/png", null, bytes, null),
        });
        var json = message.OutputJson;

        reference["custom-provider"] = "file-999";
        bytes[0] = 9;
        var result = OpenAICompatibleChat.ConvertMessages(new ModelMessage[] { message }, "openaiCompatible", "custom-provider", supportsMultiPartToolContent: true);

        Assert.Equal(json, message.OutputJson);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("[{\"type\":\"file\",\"file\":{\"file_id\":\"file-123\"}},{\"type\":\"image_url\",\"image_url\":{\"url\":\"data:image/png;base64,AQID\"}}]"),
            result[0]!["content"]));
    }
}
