// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.MoonshotChatLanguageModelTests;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; structured outputs</c>.</summary>
public sealed class DoGenerateStructuredOutputsTests
{
    [Fact]
    [UpstreamTest(
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate > structured outputs::should normalize json schemas and enable strict validation by default",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_normalize_json_schemas_and_enable_strict_validation_by_default()
    {
        var capture = new UpstreamCapture();
        var provider = MoonshotProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture);
        var options = UpstreamChat.Prompt();
        options.JsonSchemaName = "recipe";
        options.JsonSchemaDescription = "A recipe response.";
        options.JsonSchema = UpstreamChat.Json(
            """
            {
              "$schema": "http://json-schema.org/draft-07/schema#",
              "type": "object",
              "properties": {
                "ingredients": {
                  "type": "array",
                  "items": [{ "type": "string" }, { "type": "number" }]
                }
              }
            }
            """);

        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("kimi-k3")).DoGenerateAsync(options, CancellationToken.None);

        var format = JsonNode.Parse(capture.Requests[0].Body)!["response_format"];
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(
                """
                {
                  "type": "json_schema",
                  "json_schema": {
                    "name": "recipe",
                    "strict": true,
                    "schema": {
                      "type": "object",
                      "properties": {
                        "ingredients": {
                          "type": "array",
                          "prefixItems": [{ "type": "string" }, { "type": "number" }]
                        }
                      }
                    }
                  }
                }
                """),
            format));
    }

    [Theory]
    [InlineData("custom-model-id")]
    [InlineData("moonshot-v2")]
    [InlineData("kimi-next")]
    [UpstreamTest(
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate > structured outputs::should preserve JSON schema output for unknown model %s",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_JSON_schema_output_for_unknown_model(string modelId)
    {
        var capture = new UpstreamCapture();
        var provider = MoonshotProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture);
        var options = UpstreamChat.Prompt();
        options.JsonSchema = UpstreamChat.Json("{\"type\":\"object\",\"properties\":{}}");
        options.JsonSchemaName = "response";

        var result = await ((OpenAICompatibleLanguageModel)provider.LanguageModel(modelId)).DoGenerateAsync(options, CancellationToken.None);

        var format = JsonNode.Parse(capture.Requests[0].Body)!["response_format"]!;
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("{\"type\":\"json_schema\",\"json_schema\":{\"name\":\"response\",\"schema\":{\"type\":\"object\",\"properties\":{}},\"strict\":true}}"),
            format));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate > structured outputs::should send root unions of objects with type object in response_format",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_root_unions_of_objects_with_type_object_in_response_format()
    {
        const string city = """{"type":"object","properties":{"city":{"type":"string"}},"required":["city"]}""";
        const string coords = """{"properties":{"lat":{"type":"number"}},"required":["lat"]}""";
        var capture = new UpstreamCapture();
        var provider = MoonshotProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture);
        var options = UpstreamChat.Prompt();
        options.JsonSchema = UpstreamChat.Json("{\"anyOf\":[" + city + "," + coords + "]}");
        options.JsonSchemaName = "location";

        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("kimi-k3")).DoGenerateAsync(options, CancellationToken.None);

        var schema = JsonNode.Parse(capture.Requests[0].Body)!["response_format"]!["json_schema"]!["schema"];
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"object\",\"anyOf\":[" + city + "," + coords + "]}"), schema));
    }
}
