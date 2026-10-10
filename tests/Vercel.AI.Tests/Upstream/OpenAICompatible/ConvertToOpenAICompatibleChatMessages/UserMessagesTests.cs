// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.ConvertToOpenAICompatibleChatMessages;

/// <summary>Port of <c>convert-to-openai-compatible-chat-messages.test.ts</c> &gt; <c>user messages</c>.</summary>
public sealed class UserMessagesTests
{
    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/png")]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should convert %s provider references to file IDs", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_provider_references_to_file_IDs(string mediaType)
    {
        using var options = JsonDocument.Parse("{\"openaiCompatible\":{\"customOption\":\"value\"}}");
        var providerOptions = new Dictionary<string, JsonElement> { ["openaiCompatible"] = options.RootElement.GetProperty("openaiCompatible").Clone() };
        var reference = new Dictionary<string, string> { ["custom-provider"] = "file-123", ["openai"] = "file-other" };
        var prompt = new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[] { FileContentPart.FromProviderReference(mediaType, reference, "document.pdf", providerOptions) }),
        };

        var result = OpenAICompatibleChat.ConvertMessages(prompt, "openaiCompatible", "custom-provider");

        var content = result[0]!["content"]!.AsArray();
        Assert.Single(content);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"type\":\"file\",\"file\":{\"file_id\":\"file-123\"},\"customOption\":\"value\"}"),
            content[0]));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/chat/convert-to-openai-compatible-chat-messages.test.ts::user messages::should reject a reference without an ID for the configured provider", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_a_reference_without_an_ID_for_the_configured_provider()
    {
        var reference = new Dictionary<string, string> { ["openai"] = "file-other" };
        var prompt = new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[] { FileContentPart.FromProviderReference("application/pdf", reference) }),
        };

        var error = Assert.Throws<NoSuchProviderReferenceError>(() => OpenAICompatibleChat.ConvertMessages(prompt, "openaiCompatible", "custom-provider"));

        Assert.Equal("custom-provider", error.Provider);
    }
}
