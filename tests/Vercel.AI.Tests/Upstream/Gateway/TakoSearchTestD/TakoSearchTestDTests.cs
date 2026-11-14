// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.TakoSearchTestD;

/// <summary>
/// Port of the type assertions of <c>tako-search.test-d.ts</c>, which has no <c>it</c>. They are checked with
/// reflection and by deserializing a response into the typed output.
/// </summary>
public sealed class TakoSearchTestDTests
{
    private const string Response = """
        {
          "request_id": "req_1",
          "cards": [
            {
              "card_id": "c1",
              "relevance_score": 4.2,
              "content": {
                "premium_rows": { "company_record": 1 },
                "redacted_cells": { "email": [0, 2] },
                "total_matching": 12,
                "export_pricing": {
                  "baseline_usd": 0.01,
                  "free_rows": 0,
                  "max_rows_ceiling": 1000,
                  "row_cpm_usd": 7,
                  "premium_row_cpm_usd": { "company_record": 20 },
                  "variable_cost_max_usd": 0.05
                }
              }
            },
            { "card_id": "c2", "placeholder": { "withheld_source": "Semrush", "reason": "over_cap" } }
          ],
          "web_results": [{ "title": "t", "url": "https://example.com", "future_field": 1 }],
          "usage": {
            "total_cost_usd": 0.08,
            "data": { "cost_usd": 0.02, "datasets": 1 },
            "variable_cost": { "cost_usd": 0.05 }
          }
        }
        """;

    [Fact]
    public void Config_has_variable_cost_max_variable_cost_and_extra_body_and_no_snake_case_names()
    {
        Assert.Equal(typeof(bool?), typeof(GatewayTakoSearchConfig).GetProperty("VariableCost")!.PropertyType);
        Assert.Equal(typeof(double?), typeof(GatewayTakoSearchConfig).GetProperty("MaxVariableCostUsd")!.PropertyType);
        Assert.Equal(typeof(IDictionary<string, JsonNode?>), typeof(GatewayTakoSearchConfig).GetProperty("ExtraBody")!.PropertyType);

        var input = GatewayTakoSearch.InputSchema.GetProperty("properties");
        foreach (var field in new[] { "variable_cost", "max_variable_cost_usd", "extra_body", "variableCost", "maxVariableCostUsd", "extraBody" })
        {
            Assert.False(input.TryGetProperty(field, out _), field);
        }
    }

    [Fact]
    public void Passes_every_config_option_through_as_camelCase_args()
    {
        var tool = GatewayTakoSearch.Create(new GatewayTakoSearchConfig
        {
            Effort = "deep",
            Sources = new TakoSearchSourcesConfig
            {
                Data = new TakoDataSourceConfig
                {
                    Count = 3,
                    IncludeContents = true,
                    Mode = "inline",
                    ContentFormat = "csv",
                    MaxRows = 50,
                    NodeIds = ["n1"],
                    Strict = true,
                },
                Web = new TakoWebSourceConfig
                {
                    Count = 2,
                    IncludeContents = false,
                    Category = "news",
                    IncludeDomains = ["a.com"],
                    ExcludeDomains = ["b.com"],
                    SnippetMaxChars = 100,
                    Highlights = true,
                    ArticleContentMaxChars = 2000,
                    PublishedAfter = "2026-01-01",
                    PublishedBefore = "2026-02-01",
                },
            },
            Location = new TakoLocation { Latitude = 1.5, Longitude = -2.5 },
            CountryCode = "US",
            Locale = "en-US",
            Timezone = "America/New_York",
            OutputSettings = new TakoOutputSettings { ImageDarkMode = true, ForceRefresh = false },
            IncludeRelated = 5,
        });

        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""
                {
                  "effort": "deep",
                  "sources": {
                    "data": { "count": 3, "includeContents": true, "mode": "inline", "contentFormat": "csv", "maxRows": 50, "nodeIds": ["n1"], "strict": true },
                    "web": { "count": 2, "includeContents": false, "category": "news", "includeDomains": ["a.com"], "excludeDomains": ["b.com"], "snippetMaxChars": 100, "highlights": true, "articleContentMaxChars": 2000, "publishedAfter": "2026-01-01", "publishedBefore": "2026-02-01" }
                  },
                  "location": { "latitude": 1.5, "longitude": -2.5 },
                  "countryCode": "US",
                  "locale": "en-US",
                  "timezone": "America/New_York",
                  "outputSettings": { "imageDarkMode": true, "forceRefresh": false },
                  "includeRelated": 5
                }
                """),
            JsonNode.Parse(tool.ProviderToolArgs.GetRawText())));
    }

    [Fact]
    public void Input_schema_declares_every_input_field()
    {
        var properties = GatewayTakoSearch.InputSchema.GetProperty("properties");
        foreach (var field in new[] { "query", "effort", "sources", "location", "country_code", "locale", "timezone", "output_settings", "include_related" })
        {
            Assert.True(properties.TryGetProperty(field, out _), field);
        }

        var sources = properties.GetProperty("sources").GetProperty("properties");
        foreach (var field in new[] { "count", "include_contents", "mode", "content_format", "max_rows", "node_ids", "strict" })
        {
            Assert.True(sources.GetProperty("data").GetProperty("properties").TryGetProperty(field, out _), field);
        }

        foreach (var field in new[] { "count", "include_contents", "category", "include_domains", "exclude_domains", "snippet_max_chars", "highlights", "article_content_max_chars", "published_after", "published_before" })
        {
            Assert.True(sources.GetProperty("web").GetProperty("properties").TryGetProperty(field, out _), field);
        }
    }

    [Fact]
    public void Deserializes_the_response_into_the_typed_output()
    {
        var response = JsonSerializer.Deserialize<TakoSearchResponse>(Response)!;

        Assert.Equal(4.2, response.Cards![0].RelevanceScore);
        Assert.Equal("Semrush", response.Cards[1].Placeholder!.WithheldSource);
        Assert.Equal("over_cap", response.Cards[1].Placeholder!.Reason);
        var content = response.Cards[0].Content!;
        Assert.Equal(1, content.PremiumRows!["company_record"]);
        Assert.Equal([0d, 2d], content.RedactedCells!["email"]);
        Assert.Equal(12, content.TotalMatching);
        Assert.Equal(0, content.ExportPricing!.FreeRows);
        Assert.Equal(0.05, content.ExportPricing.VariableCostMaxUsd);
        Assert.Equal(20, content.ExportPricing.PremiumRowCpmUsd!["company_record"]);
        Assert.Equal(0.05, response.Usage!.VariableCost!.CostUsd);
        Assert.Null(response.Usage.Data!.DataPoints);
        Assert.True(response.WebResults![0].AdditionalProperties!.ContainsKey("future_field"));
    }

    [Fact]
    public void Web_result_has_no_citation_number()
    {
        Assert.Null(typeof(TakoWebResult).GetProperty("CitationNumber"));
        Assert.DoesNotContain(
            typeof(TakoWebResult).GetProperties(),
            property => property.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), false)
                .Cast<System.Text.Json.Serialization.JsonPropertyNameAttribute>()
                .Any(attribute => attribute.Name == "citation_number"));
    }
}
