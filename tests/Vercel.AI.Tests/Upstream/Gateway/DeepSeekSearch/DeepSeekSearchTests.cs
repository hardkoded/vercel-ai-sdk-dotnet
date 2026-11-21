// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Gateway.DeepSeekSearch;

/// <summary>Port of <c>deepseek-search.test.ts</c> &gt; <c>deepseekSearch</c>.</summary>
public sealed class DeepSeekSearchTests
{
    private const string Prefix =
        "packages/gateway/src/tool/deepseek-search.test.ts::deepseekSearch::";
    private const string Commit = "2099d455e449b77778c9648a754e8c27b1a74829";

    private static IReadOnlyList<string> ValidateOutput(string json)
    {
        using var document = JsonDocument.Parse(json);
        return ToolInputSchemaValidator.Validate(
            GatewayDeepSeekSearch.OutputSchema,
            document.RootElement
        );
    }

    private static IReadOnlyList<string> ValidateInput(string json)
    {
        using var document = JsonDocument.Parse(json);
        return ToolInputSchemaValidator.Validate(
            GatewayDeepSeekSearch.Create().InputSchema,
            document.RootElement
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "registers a provider-executed tool with camelCase developer configuration",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Registers_a_provider_executed_tool_with_camelCase_developer_configuration()
    {
        var tool = GatewayDeepSeekSearch.Create(
            new GatewayDeepSeekSearchConfig
            {
                Count = 5,
                NeedContent = true,
                OriQuestion = "Question",
                Localization = "8.8.8.8",
            }
        );

        Assert.True(tool.IsProviderDefined);
        Assert.Equal("gateway.deepseek_search", tool.ProviderToolId);
        Assert.Null(tool.Execute);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """
                    { "count": 5, "needContent": true, "oriQuestion": "Question", "localization": "8.8.8.8" }
                    """
                ),
                JsonNode.Parse(tool.ProviderToolArgs.GetRawText())
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "exposes wire input names but keeps localization developer-only",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Exposes_wire_input_names_but_keeps_localization_developer_only()
    {
        var schema = GatewayDeepSeekSearch.Create().InputSchema;
        var properties = schema.GetProperty("properties");

        Assert.Equal("query", schema.GetProperty("required")[0].GetString());
        Assert.Equal("string", properties.GetProperty("query").GetProperty("type").GetString());
        Assert.Equal("integer", properties.GetProperty("count").GetProperty("type").GetString());
        Assert.Equal(
            "boolean",
            properties.GetProperty("need_content").GetProperty("type").GetString()
        );
        Assert.False(properties.TryGetProperty("localization", out _));
    }

    public static TheoryData<string> QueryValidationCases =>
        ["", " ", new string('x', 129), string.Concat(Enumerable.Repeat("😀", 129))];

    [Theory]
    [MemberData(nameof(QueryValidationCases))]
    [UpstreamTest(
        Prefix + "leaves query validation to the gateway (%j)",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Leaves_query_validation_to_the_gateway(string query)
    {
        Assert.Empty(ValidateInput(new JsonObject { ["query"] = query }.ToJsonString()));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts nullable result metadata and preserves additive response fields",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Accepts_nullable_result_metadata_and_preserves_additive_response_fields()
    {
        const string json = """
            {
              "results": [
                {
                  "authority": -1,
                  "content": "",
                  "date": null,
                  "rerank_score": 2.4,
                  "site_icon": null,
                  "site_name": null,
                  "snippet": "Snippet",
                  "summary": "Summary",
                  "title": "Title",
                  "url": "https://example.com",
                  "extra": true
                }
              ]
            }
            """;

        Assert.Empty(ValidateOutput(json));
        var output = JsonSerializer.Deserialize<DeepSeekSearchOutput>(json)!;
        var result = Assert.Single(output.Response!.Results);
        Assert.Equal(-1, result.Authority);
        Assert.Null(result.Date);
        Assert.Equal("Summary", result.Summary);
        Assert.True(result.AdditionalProperties!["extra"].GetBoolean());
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(json),
                JsonNode.Parse(JsonSerializer.Serialize(output))
            )
        );
    }

    [Theory]
    [InlineData("""{ "results": [] }""")]
    [InlineData(
        """{ "error": "gateway_rate_limit", "message": "Retry later", "statusCode": 429, "retryAfter": "2" }"""
    )]
    [UpstreamTest(
        Prefix + "accepts result %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Accepts_result(string json)
    {
        Assert.Empty(ValidateOutput(json));
        var output = JsonSerializer.Deserialize<DeepSeekSearchOutput>(json)!;
        Assert.True((output.Response is null) != (output.Error is null));
    }

    [Theory]
    [InlineData("""{ "results": [{}] }""")]
    [InlineData("""{ "results": [{ "url": "https://example.com" }] }""")]
    [InlineData(
        """{ "results": [{ "authority": null, "summary": null, "future_field": true }] }"""
    )]
    [UpstreamTest(
        Prefix + "accepts partial and evolving result metadata (%j)",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Accepts_partial_and_evolving_result_metadata(string json)
    {
        Assert.Empty(ValidateOutput(json));
        Assert.NotNull(JsonSerializer.Deserialize<DeepSeekSearchOutput>(json)!.Response);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects malformed result bodies",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Rejects_malformed_result_bodies()
    {
        const string json = """{ "results": null }""";

        Assert.NotEmpty(ValidateOutput(json));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DeepSeekSearchOutput>(json));
    }
}
