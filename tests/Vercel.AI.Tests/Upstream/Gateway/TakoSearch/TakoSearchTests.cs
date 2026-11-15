// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Gateway.TakoSearch;

/// <summary>Port of <c>tako-search.test.ts</c> &gt; <c>takoSearch</c>.</summary>
public sealed class TakoSearchTests
{
    private const string Prefix = "packages/gateway/src/tool/tako-search.test.ts::takoSearch::";

    private static JsonObject VariableCostResponse() => JsonNode.Parse("""
        {
          "request_id": "req_123",
          "cards": [
            {
              "card_id": "card_traffic",
              "title": "vercel.com Traffic Summary",
              "description": "Monthly visits to vercel.com",
              "relevance": "High",
              "relevance_score": 4.2,
              "source_indexes": ["data"],
              "content": {
                "content_format": "json_records",
                "records": [{ "month": "2026-09", "visits": 1000 }],
                "total_rows": 1,
                "truncated": false,
                "premium_rows": { "company_record": 1 },
                "redacted_cells": { "email": [0] },
                "total_matching": 12,
                "export_pricing": {
                  "baseline_usd": 0.01,
                  "max_rows_ceiling": 1000,
                  "row_cpm_usd": 7,
                  "premium_row_cpm_usd": { "company_record": 20 },
                  "variable_cost_max_usd": 0.05
                }
              }
            },
            {
              "card_id": "card_over_cap",
              "title": "vercel.com Backlinks",
              "description": "Withheld",
              "placeholder": { "withheld_source": "Semrush", "reason": "over_cap" }
            },
            {
              "card_id": "card_not_served",
              "title": "vercel.com Keywords",
              "placeholder": { "withheld_source": "Semrush" }
            }
          ],
          "usage": {
            "total_cost_usd": 0.08,
            "compute": { "cost_usd": 0.01 },
            "data": { "cost_usd": 0.02, "datasets": 1, "data_points": 2 },
            "variable_cost": { "cost_usd": 0.05 }
          }
        }
        """)!.AsObject();

    private static IReadOnlyList<string> ValidateOutput(JsonNode value)
    {
        using var document = JsonDocument.Parse(value.ToJsonString());
        return ToolInputSchemaValidator.Validate(GatewayTakoSearch.OutputSchema, document.RootElement);
    }

    [Fact]
    [UpstreamTest(Prefix + "passes developer-only configuration through as camelCase args", Coverage = UpstreamCoverage.Covered, Commit = "0e869b40fabd6891ed58030754de6d2e7c0ea68d")]
    public void Passes_developer_only_configuration_through_as_camelCase_args()
    {
        var config = new GatewayTakoSearchConfig
        {
            VariableCost = false,
            RemoveMaxVariableCostCap = true,
            ExtraBody = new Dictionary<string, JsonNode?>
            {
                ["output_settings"] = JsonNode.Parse("""{ "flat_results": { "max_results": 3 } }"""),
            },
        };
        var tool = GatewayTakoSearch.Create(config);

        Assert.True(tool.IsProviderDefined);
        Assert.Equal("gateway.tako_search", tool.ProviderToolId);
        Assert.Null(tool.Execute);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""
                {
                  "variableCost": false,
                  "maxVariableCostUsd": null,
                  "extraBody": { "output_settings": { "flat_results": { "max_results": 3 } } }
                }
                """),
            JsonNode.Parse(tool.ProviderToolArgs.GetRawText())));
        Assert.True(tool.ProviderToolArgs.TryGetProperty("maxVariableCostUsd", out var max));
        Assert.Equal(JsonValueKind.Null, max.ValueKind);
    }

    [Fact]
    [UpstreamTest(Prefix + "keeps developer-only fields out of the input schema", Coverage = UpstreamCoverage.Covered, Commit = "0e869b40fabd6891ed58030754de6d2e7c0ea68d")]
    public void Keeps_developer_only_fields_out_of_the_input_schema()
    {
        var serializedSchema = GatewayTakoSearch.Create().InputSchema.GetRawText();

        foreach (var field in new[] { "variable_cost", "max_variable_cost_usd", "extra_body", "variableCost", "maxVariableCostUsd", "extraBody" })
        {
            Assert.DoesNotContain(field, serializedSchema);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "describes data surcharge controls in the input schema", Coverage = UpstreamCoverage.Covered)]
    public void Describes_data_surcharge_controls_in_the_input_schema()
    {
        var data = GatewayTakoSearch.Create().InputSchema.GetProperty("properties").GetProperty("sources").GetProperty("properties").GetProperty("data").GetProperty("properties");
        Assert.Contains("data surcharge", data.GetProperty("count").GetProperty("description").GetString());
        Assert.Contains("cards.content.export_pricing", data.GetProperty("include_contents").GetProperty("description").GetString());
        Assert.Contains("per 1,000 exported rows", data.GetProperty("max_rows").GetProperty("description").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts a variable-cost response with placeholder cards", Coverage = UpstreamCoverage.Covered, Commit = "0e869b40fabd6891ed58030754de6d2e7c0ea68d")]
    public void Accepts_a_variable_cost_response_with_placeholder_cards()
    {
        Assert.Empty(ValidateOutput(VariableCostResponse()));
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves additive fields on usage, pricing and placeholders", Coverage = UpstreamCoverage.Covered, Commit = "0e869b40fabd6891ed58030754de6d2e7c0ea68d")]
    public void Preserves_additive_fields_on_usage_pricing_and_placeholders()
    {
        var value = VariableCostResponse();
        value["usage"]!["future_field"] = 1;
        value["usage"]!["data"]!["future_field"] = 2;
        value["usage"]!["variable_cost"]!["future_field"] = 3;
        value["cards"]![0]!["content"]!["export_pricing"]!["future_field"] = 4;
        value["cards"]![1]!["placeholder"]!["future_field"] = 5;
        var expected = value.DeepClone();

        Assert.Empty(ValidateOutput(value));
        Assert.True(JsonNode.DeepEquals(expected, value));
    }

    [Fact]
    [UpstreamTest(Prefix + "still accepts and preserves the deprecated free_rows field", Coverage = UpstreamCoverage.Covered, Commit = "0e869b40fabd6891ed58030754de6d2e7c0ea68d")]
    public void Still_accepts_and_preserves_the_deprecated_free_rows_field()
    {
        var value = VariableCostResponse();
        value["cards"]![0]!["content"]!["export_pricing"]!["free_rows"] = 0;
        var expected = value.DeepClone();

        Assert.Empty(ValidateOutput(value));
        Assert.True(JsonNode.DeepEquals(expected, value));
    }

    [Fact]
    [UpstreamTest(Prefix + "exports the result content schema for reuse", Coverage = UpstreamCoverage.Covered, Commit = "0e869b40fabd6891ed58030754de6d2e7c0ea68d")]
    public void Exports_the_result_content_schema_for_reuse()
    {
        var content = VariableCostResponse()["cards"]![0]!["content"]!.DeepClone();
        content["future_field"] = true;
        var expected = content.DeepClone();
        using var document = JsonDocument.Parse(content.ToJsonString());

        Assert.Empty(ToolInputSchemaValidator.Validate(GatewayTakoSearch.ResultContentSchema, document.RootElement));
        Assert.True(JsonNode.DeepEquals(expected, content));
    }

    [Fact]
    [UpstreamTest(Prefix + "declares relevance_score but not citation_number in the output schema", Coverage = UpstreamCoverage.Covered, Commit = "0e869b40fabd6891ed58030754de6d2e7c0ea68d")]
    public void Declares_relevance_score_but_not_citation_number_in_the_output_schema()
    {
        var serializedSchema = GatewayTakoSearch.OutputSchema.GetRawText();

        Assert.Contains("relevance_score", serializedSchema);
        Assert.DoesNotContain("citation_number", serializedSchema);
    }
}
