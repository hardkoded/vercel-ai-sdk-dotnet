// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; settings and options &gt; responseFormat</c>.</summary>
public sealed class DoGenerateSettingsAndOptionsResponseFormatTests
{
    private const string Prefix = TestFile + " > doGenerate > settings and options > responseFormat::";

    private const string RecipeSchema = """
        {"type":"object","properties":{"name":{"type":"string"},"ingredients":{"type":"array","items":{"type":"string"}}},"required":["name","ingredients"],"additionalProperties":false}
        """;

    [Fact]
    [UpstreamTest(
        Prefix + "should send response format json schema",
        Coverage = UpstreamCoverage.Partial,
        Note = "LanguageModelCallOptions has no response format description, so the case sends no description.")]
    public async Task Should_send_response_format_json_schema()
    {
        var capture = EmptyResponse();
        var options = HelloPrompt();
        options.JsonSchema = UpstreamChat.Json(RecipeSchema);
        options.JsonSchemaName = "recipe";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var expected = "{\"input\":[{\"content\":[{\"text\":\"hello\",\"type\":\"input_text\"}],\"role\":\"user\"}],\"model\":\"grok-4-fast-non-reasoning\","
            + "\"text\":{\"format\":{\"name\":\"recipe\",\"schema\":" + RecipeSchema + ",\"strict\":true,\"type\":\"json_schema\"}}}";
        JsonAssert.Equal(Body(capture), expected);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send response format json object when no schema provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_response_format_json_object_when_no_schema_provided()
    {
        var capture = EmptyResponse();
        var options = HelloPrompt();
        options.JsonResponse = true;

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var expected = "{\"input\":[{\"content\":[{\"text\":\"hello\",\"type\":\"input_text\"}],\"role\":\"user\"}],\"model\":\"grok-4-fast-non-reasoning\","
            + "\"text\":{\"format\":{\"type\":\"json_object\"}}}";
        JsonAssert.Equal(Body(capture), expected);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use default name when responseFormat.name is not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_default_name_when_responseFormat_name_is_not_provided()
    {
        var capture = EmptyResponse();
        var options = HelloPrompt();
        options.JsonSchema = UpstreamChat.Json("""{"type":"object","properties":{"value":{"type":"string"}}}""");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("response", (string?)Body(capture)["text"]!["format"]!["name"]);
    }
}
