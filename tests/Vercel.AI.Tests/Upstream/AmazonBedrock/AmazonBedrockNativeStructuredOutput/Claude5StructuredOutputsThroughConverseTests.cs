// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockNativeStructuredOutput;

public sealed class Claude5StructuredOutputsThroughConverseTests
{
    private const string File =
        "packages/amazon-bedrock/src/amazon-bedrock-native-structured-output.test.ts::";
    private const string Prefix = File + "Claude 5 structured outputs through Converse::";
    private const string FallbackPrefix =
        File + "Claude 5 structured outputs through Converse > fallback for %s::";
    private const string RestrictionsPrefix =
        File + "Claude 5 structured outputs through Converse > %s fallback tool restrictions::";

    private const string Schema =
        "{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"],\"additionalProperties\":false}";

    private const string OpenObject = "{\"type\":\"object\",\"additionalProperties\":true}";

    private const string RecursiveReference =
        "{\"type\":\"object\",\"properties\":{\"child\":{\"$ref\":\"#\"}},\"required\":[\"child\"],\"additionalProperties\":false}";

    private const string Response =
        "{\"output\":{\"message\":{\"role\":\"assistant\",\"content\":[{\"text\":\"{\\\"answer\\\":\\\"42\\\"}\"}]}},\"usage\":{\"inputTokens\":10,\"outputTokens\":20,\"totalTokens\":30},\"stopReason\":\"end_turn\"}";

    public static TheoryData<string> Methods => new() { "doGenerate", "doStream" };

    public static TheoryData<string, string, string?, string> ToolChoices =>
        new()
        {
            { "doGenerate", "none", null, "json" },
            { "doGenerate", "tool", "lookup", "lookup,json" },
            { "doGenerate", "auto", null, "lookup,unused,json" },
            { "doGenerate", "required", null, "lookup,unused,json" },
            { "doStream", "none", null, "json" },
            { "doStream", "tool", "lookup", "lookup,json" },
            { "doStream", "auto", null, "lookup,unused,json" },
            { "doStream", "required", null, "lookup,unused,json" },
        };

    public static TheoryData<string, string> FallbackSchemas =>
        new()
        {
            { "an open object", OpenObject },
            { "a recursive reference", RecursiveReference },
            { "too many optional properties", ManyOptionalProperties() },
        };

    [Theory]
    [MemberData(nameof(ToolChoices))]
    [UpstreamTest(
        RestrictionsPrefix + "preserves caller tool choice %j",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_caller_tool_choice(
        string method,
        string choiceType,
        string? toolName,
        string expectedNames
    )
    {
        var options = Call(OpenObject);
        options.ToolChoice = choiceType switch
        {
            "none" => ToolChoice.None,
            "tool" => ToolChoice.Tool(toolName!),
            "required" => ToolChoice.Required,
            _ => ToolChoice.Auto,
        };
        options.Tools =
        [
            new LanguageModelTool("lookup", null, Json(Schema)),
            new LanguageModelTool("unused", null, Json(Schema)),
        ];

        var body = await Send("anthropic.claude-opus-5", options, method);

        Assert.Null(body["additionalModelRequestFields"]?["output_config"]);
        Assert.Equal(
            expectedNames.Split(','),
            body["toolConfig"]!["tools"]!
                .AsArray()
                .Select(tool => tool!["toolSpec"]!["name"]!.GetValue<string>())
        );
    }

    [Theory]
    [InlineData("anthropic.claude-opus-5")]
    [InlineData("anthropic.claude-opus-5-5")]
    [InlineData("anthropic.claude-sonnet-5")]
    [InlineData("anthropic.claude-sonnet-5-5")]
    [InlineData("anthropic.claude-haiku-5-5")]
    [UpstreamTest(
        Prefix + "uses native output alongside user tools for %s in auto mode",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_native_output_alongside_user_tools_in_auto_mode(string modelId)
    {
        var options = Call(Schema);
        options.Tools = [new LanguageModelTool("lookup", null, Json(Schema))];

        var body = await Send(modelId, options);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"type\":\"json_schema\",\"schema\":" + Schema + "}"),
                body["additionalModelRequestFields"]!["output_config"]!["format"]
            )
        );
        var tool = Assert.Single(body["toolConfig"]!["tools"]!.AsArray())!;
        Assert.Equal("lookup", tool["toolSpec"]!["name"]!.GetValue<string>());
    }

    [Theory]
    [MemberData(nameof(FallbackSchemas))]
    [UpstreamTest(
        FallbackPrefix + "preserves the JSON tool on models that accept forced tool use",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_the_JSON_tool_on_models_that_accept_forced_tool_use(
        string name,
        string schema
    )
    {
        Assert.NotEmpty(name);

        var body = await Send("anthropic.claude-opus-5", Call(schema));

        Assert.Null(body["additionalModelRequestFields"]?["output_config"]);
        Assert.Equal("{\"any\":{}}", body["toolConfig"]!["toolChoice"]!.ToJsonString());
        var spec = body["toolConfig"]!["tools"]![0]!["toolSpec"]!;
        Assert.Equal("json", spec["name"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(schema), spec["inputSchema"]!["json"]));
    }

    [Theory]
    [MemberData(nameof(FallbackSchemas))]
    [UpstreamTest(
        FallbackPrefix + "preserves JSON instructions on models that reject forced tool use",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_JSON_instructions_on_models_that_reject_forced_tool_use(
        string name,
        string schema
    )
    {
        Assert.NotEmpty(name);

        var body = await Send("anthropic.claude-sonnet-5-5", Call(schema));

        Assert.Null(body["additionalModelRequestFields"]?["output_config"]);
        Assert.Null(body["toolConfig"]);
        Assert.Contains(
            JsonNode.Parse(schema)!.ToJsonString(),
            body["system"]![0]!["text"]!.GetValue<string>()
        );
    }

    private static string ManyOptionalProperties()
    {
        var properties = string.Join(
            ",",
            Enumerable.Range(0, 11).Select(i => $"\"f{i}\":{{\"type\":\"string\"}}")
        );
        return "{\"type\":\"object\",\"properties\":{"
            + properties
            + "},\"additionalProperties\":false}";
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

    private static async Task<JsonNode> Send(
        string modelId,
        LanguageModelCallOptions options,
        string method = "doGenerate"
    )
    {
        var handler = new UpstreamRecordingHandler(method == "doStream" ? "\0" : Response);
        var provider = global::Vercel.AI.AmazonBedrock.AmazonBedrockProvider.Create(
            new AmazonBedrockOptions
            {
                AccessKeyId = "test-access-key",
                SecretAccessKey = "test-secret",
                UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            },
            handler
        );
        var model = provider.LanguageModel(modelId);
        if (method == "doStream")
        {
            var stream = await model.DoStreamAsync(options, CancellationToken.None);
            await using var enumerator = stream.Stream.GetAsyncEnumerator();
        }
        else
        {
            await model.DoGenerateAsync(options, CancellationToken.None);
        }

        return JsonNode.Parse(handler.Body)!;
    }
}
