// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using BedrockProvider = Vercel.AI.AmazonBedrock.AmazonBedrockAnthropicProvider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockAnthropicStructuredOutput;

public sealed class Claude5StructuredOutputsThroughInvokeModelTests
{
    private const string Prefix = "packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-structured-output.test.ts::Claude 5 structured outputs through InvokeModel::";
    private const string Schema = "{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"],\"additionalProperties\":false}";

    [Theory]
    [InlineData("none", null, "json")]
    [InlineData("tool", "lookup", "lookup,json")]
    [UpstreamTest(Prefix + "preserves caller tool choice %j during schema fallback", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Preserves_caller_tool_choice_during_schema_fallback(string choiceType, string? toolName, string expectedNames)
    {
        var options = Call("{\"type\":\"object\",\"additionalProperties\":true}");
        options.ToolChoice = choiceType == "none" ? ToolChoice.None : ToolChoice.Tool(toolName!);
        options.Tools =
        [
            new LanguageModelTool("lookup", null, Json(Schema)),
            new LanguageModelTool("unused", null, Json(Schema)),
        ];

        var (body, _) = await Generate("us.anthropic.claude-sonnet-5-5", options);

        Assert.Null(body["output_config"]);
        Assert.Equal(expectedNames.Split(','), body["tools"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>()));
    }

    [Theory]
    [InlineData("global.anthropic.claude-sonnet-5-5")]
    [InlineData("us.anthropic.claude-opus-5-5")]
    [InlineData("anthropic.claude-fable-5-1")]
    [InlineData("anthropic.claude-fable-5-2")]
    [InlineData("us.anthropic.claude-haiku-5-5")]
    [InlineData("us.anthropic.claude-sonnet-5")]
    [InlineData("us.anthropic.claude-opus-5")]
    [UpstreamTest(Prefix + "sends output_config.format for %s", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Sends_output_config_format_for(string modelId)
    {
        var (body, result) = await Generate(modelId, Call(Schema));

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"format\":{\"type\":\"json_schema\",\"schema\":" + Schema + "}}"), body["output_config"]));
        Assert.Null(body["tools"]);
        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("{\"answer\":\"42\"}", text.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "keeps the json tool when explicitly requested on a model that accepts forced tool use", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Keeps_the_json_tool_when_explicitly_requested_on_a_model_that_accepts_forced_tool_use()
    {
        var options = Call(Schema);
        options.ProviderOptions = new Dictionary<string, JsonElement> { ["anthropic"] = Json("{\"structuredOutputMode\":\"jsonTool\"}") };

        var (body, _) = await Generate("us.anthropic.claude-haiku-5-5", options);

        Assert.Null(body["output_config"]);
        Assert.Equal("{\"type\":\"any\"}", body["tool_choice"]!.ToJsonString());
    }

    [Theory]
    [InlineData("an open object", "{\"type\":\"object\",\"additionalProperties\":true}")]
    [InlineData("a recursive reference", "{\"type\":\"object\",\"properties\":{\"child\":{\"$ref\":\"#\"}},\"required\":[\"child\"],\"additionalProperties\":false}")]
    [UpstreamTest(Prefix + "preserves the JSON tool fallback for %s on the native-capable path", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Preserves_the_JSON_tool_fallback_on_the_native_capable_path(string name, string schema)
    {
        Assert.NotEmpty(name);

        var (body, _) = await Generate("us.anthropic.claude-sonnet-5-5", Call(schema));

        Assert.Null(body["output_config"]);
        var tool = Assert.Single(body["tools"]!.AsArray())!;
        Assert.Equal("json", tool["name"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(schema), tool["input_schema"]));
        Assert.Equal("auto", body["tool_choice"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves strict tool definitions through InvokeModel", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Preserves_strict_tool_definitions_through_InvokeModel()
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            Tools = [new LanguageModelTool("answer", null, Json(Schema), true)],
        };

        var (body, _) = await Generate("us.anthropic.claude-haiku-5-5", options);

        var tool = Assert.Single(body["tools"]!.AsArray())!;
        Assert.Equal("answer", tool["name"]!.GetValue<string>());
        Assert.True(tool["strict"]!.GetValue<bool>());
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static LanguageModelCallOptions Call(string schema)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            JsonSchema = Json(schema),
        };
    }

    private static async Task<(JsonNode Body, LanguageModelGenerateResult Result)> Generate(string modelId, LanguageModelCallOptions options)
    {
        var handler = new UpstreamRecordingHandler("{\"id\":\"msg_1\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"" + modelId + "\",\"content\":[{\"type\":\"text\",\"text\":\"{\\\"answer\\\":\\\"42\\\"}\"}],\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":10,\"output_tokens\":20}}");
        var provider = BedrockProvider.Create(
            new AmazonBedrockAnthropicOptions { Region = "us-east-1", ApiKey = "test-api-key" },
            handler);
        var result = await provider.LanguageModel(modelId).DoGenerateAsync(options, CancellationToken.None);
        return (JsonNode.Parse(handler.Body)!, result);
    }
}
