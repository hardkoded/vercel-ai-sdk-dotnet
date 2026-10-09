// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class AmazonBedrockPrepareToolsTests
{
    private const string Anthropic = "anthropic.claude-sonnet-4-5-20250929-v1:0";
    private const string Other = "meta.llama3-70b-instruct-v1:0";

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools::should return empty toolConfig when tools are undefined", Coverage = UpstreamCoverage.Covered)]
    public void Returns_empty_tool_config_when_tools_are_undefined()
    {
        var config = AmazonBedrockTools.Prepare(Anthropic, null, null, out var warnings);
        Assert.Equal("{}", config.ToJsonString());
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools::should return empty toolConfig when tools are empty", Coverage = UpstreamCoverage.Covered)]
    public void Returns_empty_tool_config_when_tools_are_empty()
    {
        var config = AmazonBedrockTools.Prepare(Anthropic, Array.Empty<LanguageModelTool>(), null, out var warnings);
        Assert.Equal("{}", config.ToJsonString());
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools::should correctly prepare function tools", Coverage = UpstreamCoverage.Covered)]
    public void Prepares_function_tools()
    {
        var config = AmazonBedrockTools.Prepare(Anthropic, new[] { Tool("testFunction", "A test function") }, null, out var warnings);
        var spec = config["tools"]![0]!["toolSpec"]!;
        Assert.Equal("testFunction", spec["name"]!.GetValue<string>());
        Assert.Equal("A test function", spec["description"]!.GetValue<string>());
        Assert.Equal("object", spec["inputSchema"]!["json"]!["type"]!.GetValue<string>());
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool description handling::should exclude description when it is empty string", Coverage = UpstreamCoverage.Covered)]
    public void Excludes_an_empty_description()
    {
        var spec = Spec(Tool("testFunction", string.Empty));
        Assert.Null(spec["description"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool description handling::should exclude description when it is whitespace-only", Coverage = UpstreamCoverage.Covered)]
    public void Excludes_a_whitespace_description()
    {
        var spec = Spec(Tool("testFunction", "   "));
        Assert.Null(spec["description"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool description handling::should include description when it has content", Coverage = UpstreamCoverage.Covered)]
    public void Includes_a_description_with_content()
    {
        var spec = Spec(Tool("testFunction", "Valid description"));
        Assert.Equal("Valid description", spec["description"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should handle tool choice \"auto\"", Coverage = UpstreamCoverage.Covered)]
    public void Handles_tool_choice_auto()
    {
        var config = AmazonBedrockTools.Prepare(Other, new[] { Tool("testFunction", "Test") }, ToolChoice.Auto, out _);
        Assert.Equal("{}", config["toolChoice"]!["auto"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should handle tool choice \"required\"", Coverage = UpstreamCoverage.Covered)]
    public void Handles_tool_choice_required()
    {
        var config = AmazonBedrockTools.Prepare(Other, new[] { Tool("testFunction", "Test") }, ToolChoice.Required, out _);
        Assert.Equal("{}", config["toolChoice"]!["any"]!.ToJsonString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should handle tool choice \"none\" by clearing tools", Coverage = UpstreamCoverage.Covered)]
    public void Clears_tools_when_tool_choice_is_none()
    {
        var config = AmazonBedrockTools.Prepare(Other, new[] { Tool("testFunction", "Test") }, ToolChoice.None, out _);
        Assert.Equal("{}", config.ToJsonString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should handle tool choice \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public void Handles_a_named_tool_choice()
    {
        var config = AmazonBedrockTools.Prepare(Other, new[] { Tool("testFunction", "Test") }, ToolChoice.Tool("testFunction"), out _);
        Assert.Equal("testFunction", config["toolChoice"]!["tool"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > tool choice::should filter function tools to only the named tool when tool choice is \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public void Filters_function_tools_to_the_named_tool()
    {
        var tools = new[] { Tool("getWeather", "Get weather"), Tool("getTime", "Get time") };
        var config = AmazonBedrockTools.Prepare(Other, tools, ToolChoice.Tool("getWeather"), out _);
        var prepared = config["tools"]!.AsArray();
        Assert.Single(prepared);
        Assert.Equal("getWeather", prepared[0]!["toolSpec"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should pass through strict mode when strict is true", Coverage = UpstreamCoverage.Covered)]
    public void Passes_through_strict_mode_when_the_schema_is_closed()
    {
        var tool = Tool("testFunction", "A test function", "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}", true);
        var config = AmazonBedrockTools.Prepare(Anthropic, new[] { tool }, null, out var warnings);
        var spec = config["tools"]![0]!["toolSpec"]!;
        Assert.True(spec["strict"]!.GetValue<bool>());
        Assert.Equal("testFunction", spec["name"]!.GetValue<string>());
        Assert.False(spec["inputSchema"]!["json"]!["additionalProperties"]!.GetValue<bool>());
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should keep strict mode enabled for %s", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_strict_mode_enabled_for_supported_models()
    {
        var tool = Tool("testFunction", "A test function", "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}", true);
        foreach (var modelId in new[] { "anthropic.claude-sonnet-4-6-v1", "us.anthropic.claude-haiku-4-5-20251001-v1:0" })
        {
            var config = AmazonBedrockTools.Prepare(modelId, new[] { tool }, null, out var warnings);
            Assert.True(config["tools"]![0]!["toolSpec"]!["strict"]!.GetValue<bool>());
            Assert.Empty(warnings);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should omit strict mode when the top-level object schema is open",
        Coverage = UpstreamCoverage.Partial,
        Note = "Strict is omitted and the warning text matches. Warnings are type plus message rather than feature plus details.")]
    public void Omits_strict_mode_when_the_object_schema_is_open()
    {
        var tool = Tool("testFunction", "A test function", "{\"type\":\"object\",\"properties\":{}}", true);
        var config = AmazonBedrockTools.Prepare(Anthropic, new[] { tool }, null, out var warnings);
        Assert.Null(config["tools"]![0]!["toolSpec"]!["strict"]);
        Assert.Equal("unsupported", warnings[0].Type);
        Assert.Contains("additionalProperties: false", warnings[0].Message);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-prepare-tools.test.ts::prepareTools > strict mode for function tools::should omit strict without warning when strict is false and strict mode is unsupported", Coverage = UpstreamCoverage.Covered)]
    public void Omits_strict_without_warning_when_strict_is_false_and_strict_mode_is_unsupported()
    {
        var tool = Tool("testFunction", "A test function", strict: false);
        var config = AmazonBedrockTools.Prepare("us.anthropic.claude-opus-4-7", new[] { tool }, null, out var warnings);
        Assert.Null(config["tools"]![0]!["toolSpec"]!["strict"]);
        Assert.Empty(warnings);
    }

    private static JsonNode Spec(LanguageModelTool tool)
    {
        var config = AmazonBedrockTools.Prepare(Anthropic, new[] { tool }, null, out _);
        return config["tools"]![0]!["toolSpec"]!;
    }

    private static LanguageModelTool Tool(string name, string? description, string schema = "{\"type\":\"object\",\"properties\":{}}", bool? strict = null)
    {
        using var document = JsonDocument.Parse(schema);
        return new LanguageModelTool(name, description, document.RootElement.Clone(), strict);
    }
}

public sealed class AmazonBedrockMessageConversionTests
{
    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::system messages::should combine multiple leading system messages into a single system message", Coverage = UpstreamCoverage.Covered)]
    public void Combines_leading_system_messages()
    {
        AmazonBedrockMessages.Convert("meta.llama3-70b-instruct-v1:0", new ModelMessage[]
        {
            new SystemModelMessage("Hello"),
            new SystemModelMessage("World"),
        }, out var system, out var messages);
        Assert.Equal("Hello", system![0]!["text"]!.GetValue<string>());
        Assert.Equal("World", system[1]!["text"]!.GetValue<string>());
        Assert.Empty(messages);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::system messages::should throw an error if a system message is provided after a non-system message", Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_a_system_message_follows_another_role()
    {
        var exception = Assert.Throws<AiSdkException>(() => AmazonBedrockMessages.Convert(
            "meta.llama3-70b-instruct-v1:0",
            new ModelMessage[] { new UserModelMessage("Hello"), new SystemModelMessage("World") },
            out _,
            out _));
        Assert.Contains("Multiple system messages that are separated by user/assistant messages", exception.Message);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert messages with image parts", Coverage = UpstreamCoverage.Covered)]
    public void Converts_image_parts()
    {
        var user = new UserModelMessage(new UserContentPart[]
        {
            new TextContentPart("Hello"),
            new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null),
        });
        AmazonBedrockMessages.Convert("meta.llama3-70b-instruct-v1:0", new ModelMessage[] { user }, out _, out var messages);
        var content = messages[0]!["content"]!;
        Assert.Equal("user", messages[0]!["role"]!.GetValue<string>());
        Assert.Equal("Hello", content[0]!["text"]!.GetValue<string>());
        Assert.Equal("png", content[1]!["image"]!["format"]!.GetValue<string>());
        Assert.Equal("AAECAw==", content[1]!["image"]!["source"]!["bytes"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert image parts with S3 URLs", Coverage = UpstreamCoverage.Covered)]
    public void Converts_image_parts_with_s3_urls()
    {
        var user = new UserModelMessage(new UserContentPart[]
        {
            new TextContentPart("Describe the image"),
            new FileContentPart("image/png", "s3://my-test-bucket/path/to/image.png", null, null),
        });
        AmazonBedrockMessages.Convert("meta.llama3-70b-instruct-v1:0", new ModelMessage[] { user }, out _, out var messages);
        var source = messages[0]!["content"]![1]!["image"]!["source"]!;
        Assert.Equal("png", messages[0]!["content"]![1]!["image"]!["format"]!.GetValue<string>());
        Assert.Equal("s3://my-test-bucket/path/to/image.png", source["s3Location"]!["uri"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::top-level-only mediaType resolution::should throw UnsupportedFunctionalityError for URL data (File URL)",
        Coverage = UpstreamCoverage.Partial,
        Note = "HTTP file URLs are rejected. The exception text names File URL data and is not the upstream AI_UnsupportedFunctionalityError snapshot.")]
    public void Rejects_http_file_urls()
    {
        var user = new UserModelMessage(new UserContentPart[]
        {
            new FileContentPart("image", "https://example.com/image.png", null, null),
        });
        var exception = Assert.Throws<AiSdkException>(() => AmazonBedrockMessages.Convert("meta.llama3-70b-instruct-v1:0", new ModelMessage[] { user }, out _, out _));
        Assert.Contains("File URL data", exception.Message);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::Mistral tool call ID normalization::should normalize tool call IDs in tool results when isMistral is true", Coverage = UpstreamCoverage.Covered)]
    public void Normalizes_mistral_tool_result_ids()
    {
        var tool = new ToolModelMessage("tooluse_bpe71yCfRu2b5i-nKGDr5g", "calculator", "The result is 42", false);
        AmazonBedrockMessages.Convert("mistral.mistral-large-2402-v1:0", new ModelMessage[] { tool }, out _, out var messages);
        var result = messages[0]!["content"]![0]!["toolResult"]!;
        Assert.Equal("user", messages[0]!["role"]!.GetValue<string>());
        Assert.Equal("toolusebp", result["toolUseId"]!.GetValue<string>());
        Assert.Equal("The result is 42", result["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::Mistral tool call ID normalization::should normalize tool call IDs in tool calls when isMistral is true", Coverage = UpstreamCoverage.Covered)]
    public void Normalizes_mistral_tool_call_ids()
    {
        var assistant = new AssistantModelMessage(null, new[]
        {
            new GeneratedToolCall("tooluse_xyz123ABC456-def", "test-tool", "{\"query\":\"test\"}"),
        }, null);
        AmazonBedrockMessages.Convert("mistral.mistral-large-2402-v1:0", new ModelMessage[] { assistant }, out _, out var messages);
        var call = messages[0]!["content"]![0]!["toolUse"]!;
        Assert.Equal("assistant", messages[0]!["role"]!.GetValue<string>());
        Assert.Equal("toolusexy", call["toolUseId"]!.GetValue<string>());
        Assert.Equal("test-tool", call["name"]!.GetValue<string>());
        Assert.Equal("test", call["input"]!["query"]!.GetValue<string>());
    }
}

public sealed class AmazonBedrockConverseResponseTests
{
    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should extract finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_the_finish_reason()
    {
        var result = await Generate("{\"output\":{\"message\":{\"content\":[{\"text\":\"Hello, World!\"}]}},\"stopReason\":\"stop_sequence\",\"usage\":{\"inputTokens\":4,\"outputTokens\":34,\"totalTokens\":38}}");
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop_sequence", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should support unknown finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_an_unknown_finish_reason()
    {
        var result = await Generate("{\"output\":{\"message\":{\"content\":[{\"text\":\"Hello, World!\"}]}},\"stopReason\":\"eos\",\"usage\":{\"inputTokens\":4,\"outputTokens\":34,\"totalTokens\":38}}");
        Assert.Equal(FinishReason.Other, result.FinishReason);
        Assert.Equal("eos", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > text::should extract text from citation content", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_text_from_citation_content()
    {
        var result = await Generate("{\"output\":{\"message\":{\"content\":[{\"citationsContent\":{\"content\":[{\"text\":\"Citation \"},{\"text\":\"response\"}],\"citations\":[]}}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":1,\"outputTokens\":2,\"totalTokens\":3}}");
        var text = result.Content.OfType<GeneratedText>().Select(part => part.Text).ToArray();
        Assert.Equal(new[] { "Citation ", "response" }, text);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should extract reasoning text without signature", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_reasoning_text_without_a_signature()
    {
        var result = await Generate("{\"output\":{\"message\":{\"content\":[{\"reasoningContent\":{\"reasoningText\":{\"text\":\"I need to think about this problem carefully...\"}}},{\"text\":\"The answer is 42.\"}]}},\"stopReason\":\"stop_sequence\",\"usage\":{\"inputTokens\":4,\"outputTokens\":34,\"totalTokens\":38}}");
        Assert.Equal("I need to think about this problem carefully...", Assert.IsType<GeneratedReasoning>(result.Content[0]).Text);
        Assert.Equal("The answer is 42.", Assert.IsType<GeneratedText>(result.Content[1]).Text);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > json schema response format with json tool response::should return the json response as text", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_a_json_tool_response_as_text()
    {
        const string json = "{\"elements\":[{\"location\":\"San Francisco\",\"temperature\":-5,\"condition\":\"snowy\"},{\"location\":\"London\",\"temperature\":0,\"condition\":\"snowy\"}]}";
        var body = "{\"output\":{\"message\":{\"content\":[{\"toolUse\":{\"name\":\"json\",\"toolUseId\":\"tool\",\"input\":" + json + "}}]}},\"stopReason\":\"tool_use\"}";
        var result = await Generate(body);
        Assert.Equal(json, result.Text);
        Assert.IsType<GeneratedText>(Assert.Single(result.Content));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate > json schema response format with json tool response::should send stop finish reason when json tool is used", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_stop_when_the_json_tool_is_used()
    {
        var result = await Generate("{\"output\":{\"message\":{\"content\":[{\"toolUse\":{\"name\":\"json\",\"toolUseId\":\"tool\",\"input\":{}}}]}},\"stopReason\":\"tool_use\"}");
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("tool_use", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doGenerate::should surface guardrail intervention as a content-filter finish reason with trace metadata",
        Coverage = UpstreamCoverage.Partial,
        Note = "guardrail_intervened maps to content-filter and the blocked text is kept. Guardrail trace metadata is not copied onto the result.")]
    public async Task Maps_guardrail_intervention_to_content_filter()
    {
        var result = await Generate("{\"output\":{\"message\":{\"content\":[{\"text\":\"Sorry, the model cannot answer this question.\"}]}},\"stopReason\":\"guardrail_intervened\"}");
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
        Assert.Equal("guardrail_intervened", result.RawFinishReason);
        Assert.Equal("Sorry, the model cannot answer this question.", result.Text);
    }

    private static async Task<LanguageModelGenerateResult> Generate(string response)
    {
        var handler = new UpstreamRecordingHandler(response);
        var provider = AmazonBedrockProvider.Create(new AmazonBedrockOptions
        {
            AccessKeyId = "AKIA",
            SecretAccessKey = "secret",
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        }, handler);
        return await provider.LanguageModel("anthropic.claude-sonnet-4-5-20250929-v1:0").DoGenerateAsync(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
        }, CancellationToken.None);
    }
}
