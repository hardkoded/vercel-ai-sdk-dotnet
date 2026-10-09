// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModel;

public sealed class DoGenerateTests
{
    private const string JsonToolResponse = "{\"output\":{\"message\":{\"role\":\"assistant\",\"content\":[{\"toolUse\":{\"toolUseId\":\"json-tool-id\",\"name\":\"json\",\"input\":{\"name\":\"Test\"}}}]}},\"usage\":{\"inputTokens\":4,\"outputTokens\":10,\"totalTokens\":14},\"stopReason\":\"tool_use\"}";

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should still use json tool fallback for structured output without thinking enabled",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_still_use_json_tool_fallback_for_structured_output_without_thinking_enabled()
    {
        var handler = new UpstreamRecordingHandler(JsonToolResponse);
        var result = await Generate(handler, "anthropic.claude-3-haiku-20240307-v1:0");

        using var request = JsonDocument.Parse(handler.Body);
        var toolConfig = request.RootElement.GetProperty("toolConfig");
        Assert.Equal(1, toolConfig.GetProperty("tools").GetArrayLength());
        Assert.Equal("json", toolConfig.GetProperty("tools")[0].GetProperty("toolSpec").GetProperty("name").GetString());
        Assert.Equal("{\"any\":{}}", toolConfig.GetProperty("toolChoice").GetRawText());
        Assert.False(request.RootElement.TryGetProperty("additionalModelRequestFields", out _));

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("{\"name\":\"Test\"}", text.Text);
        Assert.True(result.ProviderMetadata!.Value.GetProperty("bedrock").GetProperty("isJsonResponseFromTool").GetBoolean());
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should use the json tool fallback for claude-opus-5 (Bedrock rejects output_config.format)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_the_json_tool_fallback_for_claude_opus_5_Bedrock_rejects_output_config_format()
    {
        var handler = new UpstreamRecordingHandler(JsonToolResponse);
        await Generate(handler, "us.anthropic.claude-opus-5");
        AssertJsonToolFallback(handler);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should use the json tool fallback for claude-haiku-5-5 (Bedrock rejects output_config.format)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_the_json_tool_fallback_for_claude_haiku_5_5_Bedrock_rejects_output_config_format()
    {
        var handler = new UpstreamRecordingHandler(JsonToolResponse);
        await Generate(handler, "us.anthropic.claude-haiku-5-5");
        AssertJsonToolFallback(handler);
    }

    private static void AssertJsonToolFallback(UpstreamRecordingHandler handler)
    {
        using var request = JsonDocument.Parse(handler.Body);
        Assert.Equal("json", request.RootElement.GetProperty("toolConfig").GetProperty("tools")[0].GetProperty("toolSpec").GetProperty("name").GetString());
        Assert.False(request.RootElement.TryGetProperty("additionalModelRequestFields", out _));
    }

    private static async Task<LanguageModelGenerateResult> Generate(UpstreamRecordingHandler handler, string modelId)
    {
        var provider = AmazonBedrockProvider.Create(new AmazonBedrockOptions
        {
            AccessKeyId = "test-access-key",
            SecretAccessKey = "test-secret",
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        }, handler);
        using var schema = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}");
        return await provider.LanguageModel(modelId).DoGenerateAsync(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Generate a name") },
            JsonSchema = schema.RootElement.Clone(),
        }, CancellationToken.None);
    }
}
