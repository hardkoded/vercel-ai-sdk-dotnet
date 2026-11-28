// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportA;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>
/// Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>structured outputs</c>.
/// </summary>
public sealed class DoGenerateStructuredOutputsTests
{
    private const string Prefix = MoonshotLanguageModelSupportA.Id + "structured outputs::";

    private const string City =
        "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}";

    private const string Coords =
        "{\"properties\":{\"lat\":{\"type\":\"number\"}},\"required\":[\"lat\"]}";

    private static LanguageModelCallOptions Json(string? schema, string? name = null)
    {
        var call = TestPrompt();
        if (schema == null)
        {
            call.JsonResponse = true;
            return call;
        }

        call.JsonSchema = GoogleUpstream.Element(schema);
        call.JsonSchemaName = name;
        return call;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should normalize json schemas and enable strict validation by default",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_normalize_json_schemas_and_enable_strict_validation_by_default()
    {
        var (model, handler) = Model("kimi-k3");
        var call = Json(
            "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"ingredients\":{\"type\":\"array\",\"items\":[{\"type\":\"string\"},{\"type\":\"number\"}]}}}",
            "recipe"
        );
        call.JsonSchemaDescription = "A recipe response.";

        await model.DoGenerateAsync(call, CancellationToken.None);

        Strict(
            Body(handler)["response_format"],
            "{\"type\":\"json_schema\",\"json_schema\":{\"name\":\"recipe\",\"strict\":true,\"schema\":{\"type\":\"object\",\"properties\":{\"ingredients\":{\"type\":\"array\",\"prefixItems\":[{\"type\":\"string\"},{\"type\":\"number\"}]}}}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send root unions of objects with type object in response_format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_root_unions_of_objects_with_type_object_in_response_format()
    {
        var (model, handler) = Model("kimi-k3");

        await model.DoGenerateAsync(
            Json("{\"anyOf\":[" + City + "," + Coords + "]}", "location"),
            CancellationToken.None
        );

        Strict(
            Body(handler)["response_format"]!["json_schema"]!["schema"],
            "{\"type\":\"object\",\"anyOf\":[" + City + "," + Coords + "]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should allow strict json schema validation to be disabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_allow_strict_json_schema_validation_to_be_disabled()
    {
        var (model, handler) = Model("kimi-k3");
        var call = Json("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}}");
        call.ProviderOptions = Opts("{\"moonshotai\":{\"strictJsonSchema\":false}}");

        await model.DoGenerateAsync(call, CancellationToken.None);

        Strict(
            Body(handler)["response_format"],
            "{\"type\":\"json_schema\",\"json_schema\":{\"name\":\"response\",\"strict\":false,\"schema\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fall back to json_object without a schema",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_fall_back_to_json_object_without_a_schema()
    {
        var (model, handler) = Model("kimi-k3");

        await model.DoGenerateAsync(Json(null), CancellationToken.None);

        Strict(Body(handler)["response_format"], "{\"type\":\"json_object\"}");
    }

    [Theory]
    [InlineData("moonshot-v1-8k")]
    [InlineData("moonshot-v1-32k")]
    [InlineData("moonshot-v1-128k")]
    [InlineData("moonshot-v1-auto")]
    [InlineData("moonshot-v1-8k-vision-preview")]
    [InlineData("moonshot-v1-32k-vision-preview")]
    [InlineData("moonshot-v1-128k-vision-preview")]
    [UpstreamTest(Prefix + "should use json_schema for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_json_schema_for(string modelId)
    {
        var (model, handler) = Model(modelId);

        await model.DoGenerateAsync(
            Json("{\"type\":\"object\",\"properties\":{}}", "response"),
            CancellationToken.None
        );

        Strict(
            Body(handler)["response_format"],
            "{\"type\":\"json_schema\",\"json_schema\":{\"name\":\"response\",\"schema\":{\"type\":\"object\",\"properties\":{}},\"strict\":true}}"
        );
    }

    [Theory]
    [InlineData("custom-model-id")]
    [InlineData("moonshot-v2")]
    [InlineData("kimi-next")]
    [UpstreamTest(
        Prefix + "should preserve JSON schema output for unknown model %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_JSON_schema_output_for_unknown_model(string modelId)
    {
        var (model, handler) = Model(modelId);

        var result = await model.DoGenerateAsync(
            Json("{\"type\":\"object\",\"properties\":{}}", "response"),
            CancellationToken.None
        );

        Strict(
            Body(handler)["response_format"],
            "{\"type\":\"json_schema\",\"json_schema\":{\"name\":\"response\",\"schema\":{\"type\":\"object\",\"properties\":{}},\"strict\":true}}"
        );
        Assert.Empty(result.Warnings);
    }
}
