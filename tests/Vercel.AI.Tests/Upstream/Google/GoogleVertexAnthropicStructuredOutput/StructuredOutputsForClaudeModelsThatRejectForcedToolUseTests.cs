// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicStructuredOutput;

public sealed class StructuredOutputsForClaudeModelsThatRejectForcedToolUseTests
{
    private const string Prefix = "packages/google-vertex/src/anthropic/google-vertex-anthropic-structured-output.test.ts::structured outputs for Claude models that reject forced tool use::";
    private const string Schema = "{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"],\"additionalProperties\":false}";

    [Theory]
    [InlineData("claude-sonnet-5-5")]
    [InlineData("claude-opus-5-5")]
    [InlineData("claude-fable-5-1")]
    [InlineData("claude-fable-5-2")]
    [UpstreamTest(Prefix + "sends output_config.format for %s", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Sends_output_config_format_for(string modelId)
    {
        var (body, result) = await Generate(modelId);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"format\":{\"type\":\"json_schema\",\"schema\":" + Schema + "}}"), body["output_config"]));
        Assert.Null(body["tools"]);
        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("{\"answer\":\"42\"}", text.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "keeps the json tool for models that accept forced tool use", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Keeps_the_json_tool_for_models_that_accept_forced_tool_use()
    {
        var (body, _) = await Generate("claude-haiku-5-5");

        Assert.Null(body["output_config"]);
        Assert.Equal("any", body["tool_choice"]!["type"]!.GetValue<string>());
    }

    private static async Task<(JsonNode Body, LanguageModelGenerateResult Result)> Generate(string modelId)
    {
        var handler = new CaptureHandler
        {
            ResponseBody = "{\"id\":\"msg_1\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"" + modelId + "\",\"content\":[{\"type\":\"text\",\"text\":\"{\\\"answer\\\":\\\"42\\\"}\"}],\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":10,\"output_tokens\":20}}",
        };
        var provider = GoogleVertexAnthropicProvider.Create(
            new GoogleVertexAnthropicOptions { Project = "test-project", Location = "global" },
            handler);
        using var schema = JsonDocument.Parse(Schema);
        var result = await provider.LanguageModel(modelId).DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
                JsonSchema = schema.RootElement.Clone(),
            },
            CancellationToken.None);
        return (JsonNode.Parse(handler.Body)!, result);
    }
}
