// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicForcedToolStructuredOutput;

public sealed class StructuredOutputsForModelsThatRejectForcedToolUseTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-forced-tool-structured-output.test.ts::structured outputs for models that reject forced tool use::";
    private const string NestedPrefix = Prefix + "%s with native support enabled::fallback tool restrictions for %s::";
    private const string StreamPrefix = Prefix + "%s with native support enabled::";

    private const string Schema = "{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"],\"additionalProperties\":false}";

    private const string CircularSchema = "{\"type\":\"object\",\"properties\":{\"root\":{\"$ref\":\"#/$defs/node\"}},\"required\":[\"root\"],\"additionalProperties\":false,\"$defs\":{\"node\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"children\":{\"type\":\"array\",\"items\":{\"$ref\":\"#/$defs/node\"}}},\"required\":[\"name\",\"children\"],\"additionalProperties\":false}}}";

    public static TheoryData<string> RejectingModels => new() { "claude-sonnet-5-5", "claude-opus-5-5", "claude-fable-5-1", "claude-fable-5-2" };

    [Theory]
    [MemberData(nameof(RejectingModels))]
    [UpstreamTest(Prefix + "uses native structured outputs for %s when the provider disables them by default", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Uses_native_structured_outputs_for_when_the_provider_disables_them_by_default(string modelId)
    {
        var (body, result) = await Generate(modelId, Schema, false, false, TextResponse("{\"answer\":\"42\"}"));

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"json_schema\",\"schema\":" + Schema + "}"), body["output_config"]!["format"]));
        Assert.Null(body["tools"]);
        Assert.Null(body["tool_choice"]);
        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("{\"answer\":\"42\"}", text.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "keeps definitions for a top-level $ref schema", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Keeps_definitions_for_a_top_level_ref_schema()
    {
        var schema = "{\"$ref\":\"#/definitions/Answer\",\"definitions\":{\"Answer\":" + Schema + "}}";

        var (body, _) = await Generate("claude-sonnet-5-5", schema, false, false, TextResponse("{\"answer\":\"42\"}"));

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(schema), body["output_config"]!["format"]!["schema"]), body["output_config"]!.ToJsonString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(Prefix + "keeps the JSON response tool for self-referencing schemas (native support: %s)", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Keeps_the_JSON_response_tool_for_self_referencing_schemas_native_support(bool supportsNativeStructuredOutput)
    {
        var (body, _) = await Generate("claude-sonnet-5-5", CircularSchema, supportsNativeStructuredOutput, false, TextResponse("{\"root\":{}}"));

        Assert.Null(body["output_config"]);
        Assert.Equal(["json"], ToolNames(body));
        Assert.Equal("auto", body["tool_choice"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Prefix + "keeps map values when native structured outputs are enabled", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Keeps_map_values_when_native_structured_outputs_are_enabled()
    {
        var schema = "{\"type\":\"object\",\"properties\":{\"scores\":{\"type\":\"object\",\"additionalProperties\":{\"type\":\"number\"}}},\"required\":[\"scores\"],\"additionalProperties\":false}";

        var (body, _) = await Generate("claude-sonnet-5-5", schema, true, false, TextResponse("{\"scores\":{\"alice\":42}}"));

        Assert.Null(body["output_config"]);
        var tool = Assert.Single(body["tools"]!.AsArray())!;
        Assert.Equal("json", tool["name"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(schema), tool["input_schema"]));
    }

    [Theory]
    [InlineData("a map field", "{\"type\":\"object\",\"properties\":{\"scores\":{\"type\":\"object\",\"additionalProperties\":{\"type\":\"number\"}}},\"required\":[\"scores\"],\"additionalProperties\":false}")]
    [InlineData("more than 10 optional properties", "{\"type\":\"object\",\"properties\":{\"f0\":{\"type\":\"string\"},\"f1\":{\"type\":\"string\"},\"f2\":{\"type\":\"string\"},\"f3\":{\"type\":\"string\"},\"f4\":{\"type\":\"string\"},\"f5\":{\"type\":\"string\"},\"f6\":{\"type\":\"string\"},\"f7\":{\"type\":\"string\"},\"f8\":{\"type\":\"string\"},\"f9\":{\"type\":\"string\"},\"f10\":{\"type\":\"string\"}},\"additionalProperties\":false}")]
    [UpstreamTest(Prefix + "keeps the JSON response tool for a schema with %s", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Keeps_the_JSON_response_tool_for_a_schema_with(string description, string schema)
    {
        Assert.NotEmpty(description);

        var (body, _) = await Generate("claude-sonnet-5-5", schema, false, false, TextResponse("{}"));

        Assert.Null(body["output_config"]);
        Assert.Equal(["json"], ToolNames(body));
    }

    [Fact]
    [UpstreamTest(Prefix + "still uses the JSON response tool for models that accept forced tool use", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Still_uses_the_JSON_response_tool_for_models_that_accept_forced_tool_use()
    {
        var (body, _) = await Generate(
            "claude-sonnet-4-6",
            Schema,
            false,
            false,
            "{\"id\":\"msg_1\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"claude-sonnet-5-5\",\"content\":[{\"type\":\"tool_use\",\"id\":\"toolu_1\",\"name\":\"json\",\"input\":{\"answer\":\"42\"}}],\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":10,\"output_tokens\":20}}");

        Assert.Null(body["output_config"]);
        Assert.Equal("any", body["tool_choice"]!["type"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("doGenerate", "claude-sonnet-4-5", "none", null, "json")]
    [InlineData("doGenerate", "claude-sonnet-4-5", "tool", "lookup", "lookup,json")]
    [InlineData("doGenerate", "claude-sonnet-4-5", "auto", null, "lookup,unused,web_search,json")]
    [InlineData("doGenerate", "claude-sonnet-4-5", "required", null, "lookup,unused,web_search,json")]
    [InlineData("doGenerate", "claude-sonnet-5-5", "none", null, "json")]
    [InlineData("doGenerate", "claude-sonnet-5-5", "tool", "lookup", "lookup,json")]
    [InlineData("doGenerate", "claude-sonnet-5-5", "auto", null, "lookup,unused,web_search,json")]
    [InlineData("doGenerate", "claude-sonnet-5-5", "required", null, "lookup,unused,web_search,json")]
    [InlineData("doStream", "claude-sonnet-4-5", "none", null, "json")]
    [InlineData("doStream", "claude-sonnet-4-5", "tool", "lookup", "lookup,json")]
    [InlineData("doStream", "claude-sonnet-4-5", "auto", null, "lookup,unused,web_search,json")]
    [InlineData("doStream", "claude-sonnet-4-5", "required", null, "lookup,unused,web_search,json")]
    [InlineData("doStream", "claude-sonnet-5-5", "none", null, "json")]
    [InlineData("doStream", "claude-sonnet-5-5", "tool", "lookup", "lookup,json")]
    [InlineData("doStream", "claude-sonnet-5-5", "auto", null, "lookup,unused,web_search,json")]
    [InlineData("doStream", "claude-sonnet-5-5", "required", null, "lookup,unused,web_search,json")]
    [UpstreamTest(NestedPrefix + "preserves caller tool choice %j", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Preserves_caller_tool_choice(string method, string modelId, string choiceType, string? toolName, string expectedNames)
    {
        var options = JsonCall("{\"type\":\"object\",\"additionalProperties\":true}");
        options.ToolChoice = choiceType switch
        {
            "none" => ToolChoice.None,
            "tool" => ToolChoice.Tool(toolName!),
            "required" => ToolChoice.Required,
            _ => ToolChoice.Auto,
        };
        options.Tools =
        [
            new LanguageModelTool("lookup", null, AnthropicParity.Json(Schema)),
            new LanguageModelTool("unused", null, AnthropicParity.Json(Schema)),
            LanguageModelTool.ProviderDefined("anthropic.web_search_20250305", "web_search", AnthropicParity.Json("{}")),
        ];

        var body = await Call(method, modelId, options, true, false, TextResponse("{}"));

        Assert.Null(body["output_config"]);
        Assert.Equal(expectedNames.Split(','), ToolNames(body));
    }

    [Theory]
    [InlineData("doGenerate", "claude-sonnet-4-5")]
    [InlineData("doGenerate", "claude-sonnet-5-5")]
    [InlineData("doStream", "claude-sonnet-4-5")]
    [InlineData("doStream", "claude-sonnet-5-5")]
    [UpstreamTest(StreamPrefix + "preserves an open schema using the JSON tool for %s", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Preserves_an_open_schema_using_the_JSON_tool_for(string method, string modelId)
    {
        var schema = "{\"type\":\"object\",\"additionalProperties\":true}";

        var body = await Call(method, modelId, JsonCall(schema), true, false, TextResponse("{}"));

        Assert.Null(body["output_config"]);
        var tool = Assert.Single(body["tools"]!.AsArray())!;
        Assert.Equal("json", tool["name"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(schema), tool["input_schema"]));
        Assert.Equal(modelId == "claude-sonnet-4-5" ? "any" : "auto", body["tool_choice"]!["type"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("doGenerate", false)]
    [InlineData("doGenerate", true)]
    [InlineData("doStream", false)]
    [InlineData("doStream", true)]
    [UpstreamTest(StreamPrefix + "only sends the strict-tool beta with explicit outputFormat when supported (%s)", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public async Task Only_sends_the_strict_tool_beta_with_explicit_outputFormat_when_supported(string method, bool supportsStrictTools)
    {
        var options = JsonCall("{\"type\":\"object\",\"additionalProperties\":true}");
        options.Tools = [new LanguageModelTool("lookup", null, AnthropicParity.Json(Schema))];
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["anthropic"] = AnthropicParity.Json("{\"structuredOutputMode\":\"outputFormat\"}"),
        };

        var handler = AnthropicParity.Ok(TextResponse("{}"));
        await Run(method, new HostProvider(handler, true, supportsStrictTools), "claude-sonnet-4-5", options);
        var body = JsonNode.Parse(handler.Body)!;

        Assert.NotNull(body["output_config"]!["format"]);
        handler.Headers.TryGetValue("anthropic-beta", out var beta);
        Assert.Equal(supportsStrictTools ? "structured-outputs-2025-11-13" : null, beta);
    }

    private static string TextResponse(string text)
    {
        return "{\"id\":\"msg_1\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"claude-sonnet-5-5\",\"content\":[{\"type\":\"text\",\"text\":" + JsonSerializer.Serialize(text) + "}],\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":10,\"output_tokens\":20}}";
    }

    private static string[] ToolNames(JsonNode body)
    {
        return body["tools"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>()).ToArray();
    }

    private static LanguageModelCallOptions JsonCall(string schema)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            JsonSchema = AnthropicParity.Json(schema),
        };
    }

    private static async Task<(JsonNode Body, LanguageModelGenerateResult Result)> Generate(string modelId, string schema, bool supportsNative, bool supportsStrict, string response)
    {
        var handler = AnthropicParity.Ok(response);
        var result = await new HostProvider(handler, supportsNative, supportsStrict)
            .LanguageModel(modelId)
            .DoGenerateAsync(JsonCall(schema), CancellationToken.None);
        return (JsonNode.Parse(handler.Body)!, result);
    }

    private static async Task<JsonNode> Call(string method, string modelId, LanguageModelCallOptions options, bool supportsNative, bool supportsStrict, string response)
    {
        var handler = AnthropicParity.Ok(response);
        await Run(method, new HostProvider(handler, supportsNative, supportsStrict), modelId, options);
        return JsonNode.Parse(handler.Body)!;
    }

    private static async Task Run(string method, AnthropicProvider provider, string modelId, LanguageModelCallOptions options)
    {
        var model = provider.LanguageModel(modelId);
        if (method == "doStream")
        {
            var stream = await model.DoStreamAsync(options, CancellationToken.None);
            await foreach (var unused in stream.Stream)
            {
            }
        }
        else
        {
            await model.DoGenerateAsync(options, CancellationToken.None);
        }
    }

    // Mirrors provider adapters (e.g. Bedrock, Vertex) that disable native structured outputs by default.
    private sealed class HostProvider(CaptureHandler handler, bool supportsNativeStructuredOutput, bool supportsStrictTools)
        : AnthropicProvider(new HttpClient(handler, disposeHandler: false), new AnthropicOptions { ApiKey = "test-api-key" })
    {
        protected internal override bool SupportsNativeStructuredOutput => supportsNativeStructuredOutput;

        protected internal override bool SupportsStrictTools => supportsStrictTools;
    }
}
