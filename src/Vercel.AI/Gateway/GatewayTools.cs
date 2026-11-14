// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Vercel.AI.Gateway;

/// <summary>JSON schemas for the Gateway Tako search tool.</summary>
public static class GatewayTakoSearch
{
    private const string InputSchemaJson = @"{ ""type"": ""object"", ""properties"": { ""query"": { ""type"": ""string"", ""description"": ""Natural-language search query. Include the entity, metric, and time period. Quote a phrase to force it to one entity, for example \""Tesla\"":PRODUCT price."" }, ""effort"": { ""enum"": [ ""deep"", ""fast"", ""instant"" ], ""description"": ""Search effort. fast is the balanced default, instant favors cached results and low latency, and deep broadens retrieval with reranking at higher cost and latency."" }, ""sources"": { ""type"": ""object"", ""description"": ""Sources to search. Omit to search both curated data and the web. When provided, only keys present are searched."", ""properties"": { ""data"": { ""type"": ""object"", ""properties"": { ""count"": { ""type"": ""number"", ""description"": ""Maximum number of data results to return (1-20). When include_contents is true, each additional result adds its own data surcharge."" }, ""include_contents"": { ""type"": ""boolean"", ""description"": ""Inline rows for each data result. This adds a data surcharge based on row count and dataset source. To estimate cost, search with include_contents disabled and inspect cards.content.export_pricing. This applies to every returned card; limit sources.data.count and sources.data.max_rows to control cost."" }, ""mode"": { ""enum"": [ ""inline"", ""url"" ], ""description"": ""Requested data delivery mode. Search card data is always inline."" }, ""content_format"": { ""enum"": [ ""card_json"", ""csv"", ""json_compact"", ""json_records"" ], ""description"": ""Serialization for inlined card data."" }, ""max_rows"": { ""type"": ""number"", ""description"": ""Maximum rows to inline per result. Omit to use the allowance in cards.content.export_pricing. A data surcharge applies per 1,000 exported rows; lower values reduce cost."" }, ""node_ids"": { ""type"": ""array"", ""items"": { ""type"": ""string"" }, ""description"": ""Data Graph node IDs to prioritize. Maximum 20."" }, ""strict"": { ""type"": ""boolean"", ""description"": ""Only return cards matching node_ids. Requires a non-empty node_ids."" } } }, ""web"": { ""type"": ""object"", ""properties"": { ""count"": { ""type"": ""number"", ""description"": ""Maximum number of web results to return (1-20)."" }, ""include_contents"": { ""type"": ""boolean"", ""description"": ""Inline extracted web page text. This can add a data charge."" }, ""category"": { ""enum"": [ ""finance"", ""news"", ""sports"" ], ""description"": ""Optional web-result category filter."" }, ""include_domains"": { ""type"": ""array"", ""items"": { ""type"": ""string"" }, ""description"": ""Only return results from these bare domains."" }, ""exclude_domains"": { ""type"": ""array"", ""items"": { ""type"": ""string"" }, ""description"": ""Exclude results from these bare domains."" }, ""snippet_max_chars"": { ""type"": ""number"", ""description"": ""Maximum characters in each web-result snippet."" }, ""highlights"": { ""type"": ""boolean"", ""description"": ""Include highlighted passages in web results. Defaults to true in AI Gateway."" }, ""article_content_max_chars"": { ""type"": ""number"", ""description"": ""Maximum extracted characters per web page when including contents."" }, ""published_after"": { ""type"": ""string"", ""description"": ""Keep results published on or after this ISO date (YYYY-MM-DD)."" }, ""published_before"": { ""type"": ""string"", ""description"": ""Keep results published on or before this ISO date (YYYY-MM-DD)."" } } } } }, ""location"": { ""type"": ""object"", ""description"": ""End-user coordinates for localized results."", ""properties"": { ""latitude"": { ""type"": ""number"", ""description"": ""Latitude between -90 and 90."" }, ""longitude"": { ""type"": ""number"", ""description"": ""Longitude between -180 and 180."" } }, ""required"": [ ""latitude"", ""longitude"" ] }, ""country_code"": { ""type"": ""string"", ""description"": ""Two-letter ISO 3166-1 country code, such as 'US'."" }, ""locale"": { ""type"": ""string"", ""description"": ""BCP-47 locale, such as 'en-US'."" }, ""timezone"": { ""type"": ""string"", ""description"": ""IANA timezone, such as 'America/New_York'."" }, ""output_settings"": { ""type"": ""object"", ""description"": ""Controls card rendering in the search response."", ""properties"": { ""image_dark_mode"": { ""type"": ""boolean"", ""description"": ""Render card preview images in dark mode."" }, ""force_refresh"": { ""type"": ""boolean"", ""description"": ""Instant-effort only. Request a refreshed instant result."" } } }, ""include_related"": { ""type"": ""number"", ""description"": ""Maximum related search suggestions to include (1-20)."" } }, ""required"": [ ""query"" ] }";

    private const string OutputSchemaJson = @"{
  ""anyOf"": [
    {
      ""type"": ""object"",
      ""properties"": {
        ""request_id"": { ""type"": ""string"" },
        ""cards"": { ""type"": ""array"", ""items"": { ""$ref"": ""#/definitions/card"" } },
        ""web_results"": { ""type"": ""array"", ""items"": { ""$ref"": ""#/definitions/webResult"" } },
        ""usage"": {
          ""type"": [ ""object"", ""null"" ],
          ""properties"": {
            ""total_cost_usd"": { ""type"": ""number"" },
            ""compute"": { ""type"": [ ""object"", ""null"" ], ""properties"": { ""cost_usd"": { ""type"": ""number"" } }, ""required"": [ ""cost_usd"" ] },
            ""data"": {
              ""type"": [ ""object"", ""null"" ],
              ""properties"": {
                ""cost_usd"": { ""type"": ""number"" },
                ""datasets"": { ""type"": ""number"" },
                ""data_points"": { ""type"": [ ""number"", ""null"" ] }
              },
              ""required"": [ ""cost_usd"", ""datasets"" ]
            },
            ""variable_cost"": { ""type"": [ ""object"", ""null"" ], ""properties"": { ""cost_usd"": { ""type"": ""number"" } }, ""required"": [ ""cost_usd"" ] }
          },
          ""required"": [ ""total_cost_usd"" ]
        },
        ""related"": { ""type"": [ ""array"", ""null"" ], ""items"": { ""type"": ""object"" } }
      },
      ""required"": [ ""request_id"" ]
    },
    {
      ""type"": ""object"",
      ""properties"": {
        ""error"": { ""enum"": [ ""api_error"", ""configuration_error"", ""execution_error"", ""gateway_rate_limit"", ""invalid_input"", ""rate_limit"", ""timeout"", ""unknown"", ""unknown_tool"" ] },
        ""message"": { ""type"": ""string"" },
        ""statusCode"": { ""type"": ""number"" },
        ""retryAfter"": { ""type"": ""string"" },
        ""statusText"": { ""type"": ""string"" }
      },
      ""required"": [ ""error"", ""message"" ]
    }
  ],
  ""definitions"": {
    ""content"": {
      ""type"": [ ""object"", ""null"" ],
      ""properties"": {
        ""content_format"": { ""enum"": [ ""card_json"", ""csv"", ""json_compact"", ""json_records"", null ] },
        ""cost"": { ""type"": ""number"" },
        ""data"": { ""type"": [ ""string"", ""null"" ] },
        ""records"": { ""type"": [ ""array"", ""null"" ], ""items"": { ""type"": ""object"" } },
        ""dataset"": {
          ""type"": [ ""object"", ""null"" ],
          ""properties"": {
            ""columns"": {
              ""type"": ""array"",
              ""items"": {
                ""type"": ""object"",
                ""properties"": {
                  ""name"": { ""type"": ""string"" },
                  ""type"": { ""enum"": [ ""boolean"", ""date"", ""datetime"", ""number"", ""string"" ] },
                  ""unit"": { ""type"": [ ""string"", ""null"" ] }
                },
                ""required"": [ ""name"", ""type"" ]
              }
            },
            ""rows"": { ""type"": ""array"", ""items"": { ""type"": ""array"" } },
            ""total_rows"": { ""type"": ""number"" },
            ""truncated"": { ""type"": ""boolean"" },
            ""ref"": { ""type"": ""string"" },
            ""sources"": {
              ""type"": ""array"",
              ""items"": {
                ""type"": ""object"",
                ""properties"": { ""name"": { ""type"": ""string"" }, ""index"": { ""enum"": [ ""data"", ""web"" ] } },
                ""required"": [ ""name"" ]
              }
            },
            ""provenance"": { ""enum"": [ ""query"", ""web_extraction"" ] }
          },
          ""required"": [ ""columns"", ""rows"", ""total_rows"", ""truncated"", ""ref"", ""sources"" ]
        },
        ""card_data"": { ""type"": [ ""object"", ""null"" ] },
        ""card_data_schema"": { ""type"": [ ""object"", ""null"" ] },
        ""url"": { ""type"": [ ""string"", ""null"" ] },
        ""expires_at"": { ""type"": [ ""string"", ""null"" ] },
        ""total_rows"": { ""type"": [ ""number"", ""null"" ] },
        ""truncated"": { ""type"": ""boolean"" },
        ""premium_rows"": { ""type"": [ ""object"", ""null"" ], ""additionalProperties"": { ""type"": ""number"" } },
        ""redacted_cells"": { ""type"": [ ""object"", ""null"" ], ""additionalProperties"": { ""type"": ""array"", ""items"": { ""type"": ""number"" } } },
        ""total_matching"": { ""type"": [ ""number"", ""null"" ] },
        ""export_pricing"": {
          ""type"": [ ""object"", ""null"" ],
          ""properties"": {
            ""baseline_usd"": { ""type"": ""number"" },
            ""free_rows"": { ""type"": ""number"" },
            ""max_rows_ceiling"": { ""type"": ""number"" },
            ""row_cpm_usd"": { ""type"": ""number"" },
            ""premium_row_cpm_usd"": { ""type"": [ ""object"", ""null"" ], ""additionalProperties"": { ""type"": ""number"" } },
            ""variable_cost_max_usd"": { ""type"": [ ""number"", ""null"" ] }
          },
          ""required"": [ ""baseline_usd"", ""max_rows_ceiling"", ""row_cpm_usd"" ]
        },
        ""manifest"": {
          ""type"": [ ""array"", ""null"" ],
          ""items"": {
            ""type"": ""object"",
            ""properties"": {
              ""dtype"": { ""enum"": [ ""boolean"", ""date"", ""datetime"", ""number"", ""string"", null ] },
              ""entity"": { ""type"": [ ""string"", ""null"" ] },
              ""metric"": { ""type"": [ ""string"", ""null"" ] },
              ""name"": { ""type"": [ ""string"", ""null"" ] },
              ""unit"": { ""type"": [ ""string"", ""null"" ] }
            }
          }
        }
      }
    },
    ""card"": {
      ""type"": ""object"",
      ""properties"": {
        ""card_id"": { ""type"": [ ""string"", ""null"" ] },
        ""title"": { ""type"": [ ""string"", ""null"" ] },
        ""description"": { ""type"": [ ""string"", ""null"" ] },
        ""semantic_description"": { ""type"": [ ""string"", ""null"" ] },
        ""webpage_url"": { ""type"": [ ""string"", ""null"" ] },
        ""image_url"": { ""type"": [ ""string"", ""null"" ] },
        ""embed_url"": { ""type"": [ ""string"", ""null"" ] },
        ""sources"": {
          ""type"": [ ""array"", ""null"" ],
          ""items"": {
            ""type"": ""object"",
            ""properties"": {
              ""source_name"": { ""type"": [ ""string"", ""null"" ] },
              ""source_description"": { ""type"": [ ""string"", ""null"" ] },
              ""source_index"": { ""enum"": [ ""data"", ""web"" ] },
              ""source_text"": { ""type"": [ ""string"", ""null"" ] },
              ""url"": { ""type"": [ ""string"", ""null"" ] }
            },
            ""required"": [ ""source_index"" ]
          }
        },
        ""methodologies"": {
          ""type"": [ ""array"", ""null"" ],
          ""items"": {
            ""type"": ""object"",
            ""properties"": {
              ""methodology_name"": { ""type"": [ ""string"", ""null"" ] },
              ""methodology_description"": { ""type"": [ ""string"", ""null"" ] }
            },
            ""required"": [ ""methodology_name"", ""methodology_description"" ]
          }
        },
        ""source_indexes"": { ""type"": [ ""array"", ""null"" ], ""items"": { ""enum"": [ ""data"", ""web"" ] } },
        ""card_type"": { ""type"": [ ""string"", ""null"" ] },
        ""relevance"": { ""enum"": [ ""High"", ""Low"", ""Medium"", null ] },
        ""relevance_score"": { ""type"": [ ""number"", ""null"" ] },
        ""content"": { ""$ref"": ""#/definitions/content"" },
        ""exportable"": { ""type"": ""boolean"" },
        ""nodes"": {
          ""type"": [ ""array"", ""null"" ],
          ""items"": {
            ""type"": ""object"",
            ""properties"": {
              ""id"": { ""type"": ""string"" },
              ""type"": { ""enum"": [ ""entity"", ""metric"" ] },
              ""name"": { ""type"": ""string"" },
              ""description"": { ""type"": [ ""string"", ""null"" ] }
            },
            ""required"": [ ""id"", ""type"", ""name"" ]
          }
        },
        ""metric_definitions"": {
          ""type"": [ ""array"", ""null"" ],
          ""items"": {
            ""type"": ""object"",
            ""properties"": { ""name"": { ""type"": ""string"" }, ""definition"": { ""type"": ""string"" } },
            ""required"": [ ""name"", ""definition"" ]
          }
        },
        ""data_freshness"": {
          ""type"": [ ""object"", ""null"" ],
          ""properties"": {
            ""coverage_end"": { ""type"": [ ""string"", ""null"" ] },
            ""data_as_of"": { ""type"": [ ""string"", ""null"" ] },
            ""last_updated"": { ""type"": [ ""string"", ""null"" ] }
          }
        },
        ""placeholder"": {
          ""type"": [ ""object"", ""null"" ],
          ""properties"": {
            ""withheld_source"": { ""type"": ""string"" },
            ""reason"": { ""enum"": [ ""not_served"", ""over_cap"", null ] }
          },
          ""required"": [ ""withheld_source"" ]
        }
      }
    },
    ""webResult"": {
      ""type"": ""object"",
      ""properties"": {
        ""title"": { ""type"": ""string"" },
        ""url"": { ""type"": ""string"" },
        ""snippet"": { ""type"": [ ""string"", ""null"" ] },
        ""source_name"": { ""type"": [ ""string"", ""null"" ] },
        ""publish_date"": { ""type"": [ ""string"", ""null"" ] },
        ""content"": { ""$ref"": ""#/definitions/content"" }
      },
      ""required"": [ ""title"", ""url"" ]
    }
  }
}";

    /// <summary>Input schema. Data-source descriptions name the surcharge controls.</summary>
    public static JsonElement InputSchema { get; } = Parse(InputSchemaJson);

    /// <summary>Output schema. Internal ranking fields are not part of the contract.</summary>
    public static JsonElement OutputSchema => Resolved.Output;

    /// <summary>Schema of the <c>content</c> of a card or web result, exported for reuse by other Tako tools. Maps to <c>takoResultContentSchema</c>.</summary>
    public static JsonElement ResultContentSchema => Resolved.Content;

    private static readonly (JsonElement Output, JsonElement Content) Resolved = ParseWithReferences(OutputSchemaJson);

    /// <summary>
    /// Creates the Tako search tool. The tool is executed by the AI Gateway, so it has no local execute function.
    /// Maps to <c>gatewayTools.takoSearch</c>.
    /// </summary>
    public static Tool Create(GatewayTakoSearchConfig? config = null)
    {
        return Tool.ProviderDefined("gateway.tako_search", "tako_search", (config ?? new GatewayTakoSearchConfig()).ToArgs(), InputSchema);
    }

    // The validator that checks tool outputs does not follow $ref, so the shared definitions are inlined.
    private static (JsonElement Output, JsonElement Content) ParseWithReferences(string json)
    {
        var root = JsonNode.Parse(json)!.AsObject();
        var definitions = root["definitions"]!.AsObject();
        root.Remove("definitions");
        var resolved = new Dictionary<string, JsonNode>(StringComparer.Ordinal);
        foreach (var name in new[] { "content", "card", "webResult" })
        {
            resolved[name] = Inline(definitions[name]!, resolved);
        }

        var result = Inline(root, resolved);
        return (JsonSerializer.SerializeToElement(result), JsonSerializer.SerializeToElement(resolved["content"]));
    }

    private static JsonNode Inline(JsonNode node, Dictionary<string, JsonNode> resolved)
    {
        switch (node)
        {
            case JsonObject obj:
                if (obj.Count == 1 && obj["$ref"] is JsonNode reference)
                {
                    var name = reference.GetValue<string>().Substring("#/definitions/".Length);
                    return resolved[name].DeepClone();
                }

                var copy = new JsonObject();
                foreach (var property in obj)
                {
                    copy[property.Key] = property.Value == null ? null : Inline(property.Value, resolved);
                }

                return copy;
            case JsonArray array:
                var items = new JsonArray();
                foreach (var item in array)
                {
                    items.Add(item == null ? null : Inline(item, resolved));
                }

                return items;
            default:
                return node.DeepClone();
        }
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}

/// <summary>
/// Developer-only configuration of the Gateway Tako search tool. These options are not part of the tool input,
/// so the model cannot see or set them. The Gateway applies the defaults and validates them. Maps to <c>TakoSearchConfig</c>.
/// </summary>
public sealed class GatewayTakoSearchConfig
{
    /// <summary>Search effort: <c>deep</c>, <c>fast</c> or <c>instant</c>. <c>fast</c> is the default balanced option. Maps to <c>effort</c>.</summary>
    public string? Effort { get; set; }

    /// <summary>Sources to search. Omit to search both curated data and the web. Maps to <c>sources</c>.</summary>
    public TakoSearchSourcesConfig? Sources { get; set; }

    /// <summary>End-user coordinates for localized results. Maps to <c>location</c>.</summary>
    public TakoLocation? Location { get; set; }

    /// <summary>ISO 3166-1 alpha-2 country code. Maps to <c>countryCode</c>.</summary>
    public string? CountryCode { get; set; }

    /// <summary>BCP-47 locale. Maps to <c>locale</c>.</summary>
    public string? Locale { get; set; }

    /// <summary>IANA timezone. Maps to <c>timezone</c>.</summary>
    public string? Timezone { get; set; }

    /// <summary>Settings that control card rendering. Maps to <c>outputSettings</c>.</summary>
    public TakoOutputSettings? OutputSettings { get; set; }

    /// <summary>Maximum related search suggestions to include (1-20). Maps to <c>includeRelated</c>.</summary>
    public int? IncludeRelated { get; set; }

    /// <summary>
    /// Serve per-unit-priced data, such as web traffic and SEO. Each such card adds a charge, capped by
    /// <see cref="MaxVariableCostUsd"/>. On by default; set false to opt out, and those cards come back as placeholders.
    /// Maps to <c>variableCost</c>.
    /// </summary>
    public bool? VariableCost { get; set; }

    /// <summary>
    /// Most one call can be charged in variable cost, in USD. The Gateway defaults to 0.25. Cards that don't fit come back
    /// as placeholders with reason <c>over_cap</c>. Can't be combined with <c>VariableCost = false</c>. To remove the cap
    /// (<c>maxVariableCostUsd: null</c>), set <see cref="RemoveMaxVariableCostCap"/> instead. Maps to <c>maxVariableCostUsd</c>.
    /// </summary>
    public double? MaxVariableCostUsd { get; set; }

    /// <summary>
    /// Removes the variable-cost cap, which sends <c>maxVariableCostUsd: null</c>. Takes precedence over
    /// <see cref="MaxVariableCostUsd"/>.
    /// </summary>
    public bool RemoveMaxVariableCostCap { get; set; }

    /// <summary>
    /// Extra Tako request fields in snake_case, deep-merged into the body last. Can't set fields this tool models;
    /// Tako rejects unknown keys (<c>api_error</c>). Maps to <c>extraBody</c>.
    /// </summary>
    public IDictionary<string, JsonNode?>? ExtraBody { get; set; }

    internal JsonElement ToArgs()
    {
        var args = new JsonObject();
        TakoJson.Set(args, "effort", Effort);
        if (Sources != null)
        {
            args["sources"] = Sources.ToJson();
        }

        if (Location != null)
        {
            args["location"] = new JsonObject { ["latitude"] = Location.Latitude, ["longitude"] = Location.Longitude };
        }

        TakoJson.Set(args, "countryCode", CountryCode);
        TakoJson.Set(args, "locale", Locale);
        TakoJson.Set(args, "timezone", Timezone);
        if (OutputSettings != null)
        {
            var output = new JsonObject();
            TakoJson.Set(output, "imageDarkMode", OutputSettings.ImageDarkMode);
            TakoJson.Set(output, "forceRefresh", OutputSettings.ForceRefresh);
            args["outputSettings"] = output;
        }

        TakoJson.Set(args, "includeRelated", IncludeRelated);
        if (VariableCost is { } variableCost)
        {
            args["variableCost"] = variableCost;
        }

        if (RemoveMaxVariableCostCap)
        {
            args["maxVariableCostUsd"] = null;
        }
        else if (MaxVariableCostUsd is { } max)
        {
            args["maxVariableCostUsd"] = max;
        }

        if (ExtraBody != null)
        {
            var extra = new JsonObject();
            foreach (var pair in ExtraBody)
            {
                extra[pair.Key] = pair.Value?.DeepClone();
            }

            args["extraBody"] = extra;
        }

        return JsonSerializer.SerializeToElement(args);
    }
}

internal static class TakoJson
{
    internal static void Set(JsonObject target, string name, string? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    internal static void Set(JsonObject target, string name, bool? value)
    {
        if (value is { } v)
        {
            target[name] = v;
        }
    }

    internal static void Set(JsonObject target, string name, int? value)
    {
        if (value is { } v)
        {
            target[name] = v;
        }
    }

    internal static void Set(JsonObject target, string name, IEnumerable<string>? value)
    {
        if (value != null)
        {
            var array = new JsonArray();
            foreach (var item in value)
            {
                array.Add(item);
            }

            target[name] = array;
        }
    }
}

/// <summary>Sources to search. Maps to <c>TakoSearchConfig['sources']</c>.</summary>
public sealed class TakoSearchSourcesConfig
{
    /// <summary>Curated data source options. Maps to <c>data</c>.</summary>
    public TakoDataSourceConfig? Data { get; set; }

    /// <summary>Web source options. Maps to <c>web</c>.</summary>
    public TakoWebSourceConfig? Web { get; set; }

    internal JsonObject ToJson()
    {
        var result = new JsonObject();
        if (Data != null)
        {
            result["data"] = Data.ToJson();
        }

        if (Web != null)
        {
            result["web"] = Web.ToJson();
        }

        return result;
    }
}

/// <summary>Options of the curated data source. Maps to <c>TakoDataSourceConfig</c>.</summary>
public sealed class TakoDataSourceConfig
{
    /// <summary>Maximum number of data results to return (1-20). When <see cref="IncludeContents"/> is true, each additional result adds its own data surcharge.</summary>
    public int? Count { get; set; }

    /// <summary>
    /// Inline rows for each data result. This adds a data surcharge based on row count and dataset source. To estimate cost,
    /// search with this disabled and inspect <c>cards.content.export_pricing</c>. This applies to every returned card; limit
    /// <see cref="Count"/> and <see cref="MaxRows"/> to control cost.
    /// </summary>
    public bool? IncludeContents { get; set; }

    /// <summary>Requested delivery mode for card data, <c>inline</c> or <c>url</c>. Search cards are always inlined.</summary>
    public string? Mode { get; set; }

    /// <summary>Serialization for inlined card data: <c>card_json</c>, <c>csv</c>, <c>json_compact</c> or <c>json_records</c>. Maps to <c>contentFormat</c>.</summary>
    public string? ContentFormat { get; set; }

    /// <summary>Maximum rows to inline per result. Omit to use the allowance in <c>cards.content.export_pricing</c>. A data surcharge applies per 1,000 exported rows; lower values reduce cost.</summary>
    public int? MaxRows { get; set; }

    /// <summary>Data Graph node IDs to prioritize. Maps to <c>nodeIds</c>.</summary>
    public IList<string>? NodeIds { get; set; }

    /// <summary>Return only cards matching <see cref="NodeIds"/>. Requires at least one node ID.</summary>
    public bool? Strict { get; set; }

    internal JsonObject ToJson()
    {
        var result = new JsonObject();
        TakoJson.Set(result, "count", Count);
        TakoJson.Set(result, "includeContents", IncludeContents);
        TakoJson.Set(result, "mode", Mode);
        TakoJson.Set(result, "contentFormat", ContentFormat);
        TakoJson.Set(result, "maxRows", MaxRows);
        TakoJson.Set(result, "nodeIds", NodeIds);
        TakoJson.Set(result, "strict", Strict);
        return result;
    }
}

/// <summary>Options of the web source. Maps to <c>TakoWebSourceConfig</c>.</summary>
public sealed class TakoWebSourceConfig
{
    /// <summary>Maximum number of web results to return (1-20).</summary>
    public int? Count { get; set; }

    /// <summary>Inline extracted web page text. This can add a data charge.</summary>
    public bool? IncludeContents { get; set; }

    /// <summary>Optional web-result category filter: <c>finance</c>, <c>news</c> or <c>sports</c>.</summary>
    public string? Category { get; set; }

    /// <summary>Only return results from these bare domains. Maps to <c>includeDomains</c>.</summary>
    public IList<string>? IncludeDomains { get; set; }

    /// <summary>Exclude results from these bare domains. Maps to <c>excludeDomains</c>.</summary>
    public IList<string>? ExcludeDomains { get; set; }

    /// <summary>Maximum characters in each web-result snippet. Maps to <c>snippetMaxChars</c>.</summary>
    public int? SnippetMaxChars { get; set; }

    /// <summary>Include highlighted passages in web results. Defaults to true in AI Gateway.</summary>
    public bool? Highlights { get; set; }

    /// <summary>Maximum extracted characters per web page when including contents. Maps to <c>articleContentMaxChars</c>.</summary>
    public int? ArticleContentMaxChars { get; set; }

    /// <summary>Keep results published on or after this ISO date. Maps to <c>publishedAfter</c>.</summary>
    public string? PublishedAfter { get; set; }

    /// <summary>Keep results published on or before this ISO date. Maps to <c>publishedBefore</c>.</summary>
    public string? PublishedBefore { get; set; }

    internal JsonObject ToJson()
    {
        var result = new JsonObject();
        TakoJson.Set(result, "count", Count);
        TakoJson.Set(result, "includeContents", IncludeContents);
        TakoJson.Set(result, "category", Category);
        TakoJson.Set(result, "includeDomains", IncludeDomains);
        TakoJson.Set(result, "excludeDomains", ExcludeDomains);
        TakoJson.Set(result, "snippetMaxChars", SnippetMaxChars);
        TakoJson.Set(result, "highlights", Highlights);
        TakoJson.Set(result, "articleContentMaxChars", ArticleContentMaxChars);
        TakoJson.Set(result, "publishedAfter", PublishedAfter);
        TakoJson.Set(result, "publishedBefore", PublishedBefore);
        return result;
    }
}

/// <summary>End-user coordinates for localized results. Maps to <c>TakoSearchConfig['location']</c>.</summary>
public sealed class TakoLocation
{
    /// <summary>Latitude between -90 and 90.</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude between -180 and 180.</summary>
    public double Longitude { get; set; }
}

/// <summary>Settings that control card rendering. Maps to <c>TakoSearchConfig['outputSettings']</c>.</summary>
public sealed class TakoOutputSettings
{
    /// <summary>Render card preview images in dark mode. Maps to <c>imageDarkMode</c>.</summary>
    public bool? ImageDarkMode { get; set; }

    /// <summary>Instant-effort only. Request a refreshed instant result. Maps to <c>forceRefresh</c>.</summary>
    public bool? ForceRefresh { get; set; }
}

/// <summary>A dataset cell: a boolean, number, string or null. Maps to <c>TakoDatasetCell</c>.</summary>
[JsonConverter(typeof(TakoDatasetCellConverter))]
public readonly struct TakoDatasetCell
{
    internal TakoDatasetCell(bool? boolean, double? number, string? text)
    {
        Boolean = boolean;
        Number = number;
        String = text;
    }

    /// <summary>The value when the cell is a boolean.</summary>
    public bool? Boolean { get; }

    /// <summary>The value when the cell is a number.</summary>
    public double? Number { get; }

    /// <summary>The value when the cell is a string.</summary>
    public string? String { get; }

    /// <summary>True when the cell is null.</summary>
    public bool IsNull => Boolean is null && Number is null && String is null;
}

internal sealed class TakoDatasetCellConverter : JsonConverter<TakoDatasetCell>
{
    public override TakoDatasetCell Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return default;
            case JsonTokenType.True:
                return new TakoDatasetCell(true, null, null);
            case JsonTokenType.False:
                return new TakoDatasetCell(false, null, null);
            case JsonTokenType.Number:
                return new TakoDatasetCell(null, reader.GetDouble(), null);
            case JsonTokenType.String:
                return new TakoDatasetCell(null, null, reader.GetString());
            default:
                throw new JsonException("A Tako dataset cell must be a boolean, number, string or null.");
        }
    }

    public override void Write(Utf8JsonWriter writer, TakoDatasetCell value, JsonSerializerOptions options)
    {
        if (value.Boolean is { } boolean)
        {
            writer.WriteBooleanValue(boolean);
        }
        else if (value.Number is { } number)
        {
            writer.WriteNumberValue(number);
        }
        else if (value.String is { } text)
        {
            writer.WriteStringValue(text);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

/// <summary>A column of a <see cref="TakoDataset"/>.</summary>
public sealed class TakoDatasetColumn
{
    /// <summary>Column name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary><c>boolean</c>, <c>date</c>, <c>datetime</c>, <c>number</c> or <c>string</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    /// <summary>Unit of the values.</summary>
    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
}

/// <summary>A source of a <see cref="TakoDataset"/>.</summary>
public sealed class TakoDatasetSource
{
    /// <summary>Source name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary><c>data</c> or <c>web</c>.</summary>
    [JsonPropertyName("index")]
    public string? Index { get; set; }
}

/// <summary>Inline dataset of a result content. Maps to <c>TakoResultContent['dataset']</c>.</summary>
public sealed class TakoDataset
{
    /// <summary>Columns.</summary>
    [JsonPropertyName("columns")]
    public IList<TakoDatasetColumn> Columns { get; set; } = [];

    /// <summary>Rows.</summary>
    [JsonPropertyName("rows")]
    public IList<IList<TakoDatasetCell>> Rows { get; set; } = [];

    /// <summary>Total rows.</summary>
    [JsonPropertyName("total_rows")]
    public double TotalRows { get; set; }

    /// <summary>Whether the rows were truncated.</summary>
    [JsonPropertyName("truncated")]
    public bool Truncated { get; set; }

    /// <summary>Dataset reference.</summary>
    [JsonPropertyName("ref")]
    public string Ref { get; set; } = "";

    /// <summary>Sources.</summary>
    [JsonPropertyName("sources")]
    public IList<TakoDatasetSource> Sources { get; set; } = [];

    /// <summary><c>query</c> or <c>web_extraction</c>.</summary>
    [JsonPropertyName("provenance")]
    public string? Provenance { get; set; }

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Pricing of exporting a card's rows. Maps to <c>TakoResultContent['export_pricing']</c>.</summary>
public sealed class TakoExportPricing
{
    /// <summary>Baseline charge in USD.</summary>
    [JsonPropertyName("baseline_usd")]
    public double BaselineUsd { get; set; }

    /// <summary>Deprecated. Always 0, and Tako will stop sending it.</summary>
    [JsonPropertyName("free_rows")]
    public double? FreeRows { get; set; }

    /// <summary>Row ceiling.</summary>
    [JsonPropertyName("max_rows_ceiling")]
    public double MaxRowsCeiling { get; set; }

    /// <summary>USD per 1,000 rows.</summary>
    [JsonPropertyName("row_cpm_usd")]
    public double RowCpmUsd { get; set; }

    /// <summary>USD per 1,000 premium records by class, added to <see cref="RowCpmUsd"/> when premium.</summary>
    [JsonPropertyName("premium_row_cpm_usd")]
    public IDictionary<string, double>? PremiumRowCpmUsd { get; set; }

    /// <summary>Upper bound on this card's variable-cost charge. Set only for per-unit-priced sources.</summary>
    [JsonPropertyName("variable_cost_max_usd")]
    public double? VariableCostMaxUsd { get; set; }

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A column of the manifest of a result content.</summary>
public sealed class TakoManifestColumn
{
    /// <summary>Data type.</summary>
    [JsonPropertyName("dtype")]
    public string? Dtype { get; set; }

    /// <summary>Entity.</summary>
    [JsonPropertyName("entity")]
    public string? Entity { get; set; }

    /// <summary>Metric.</summary>
    [JsonPropertyName("metric")]
    public string? Metric { get; set; }

    /// <summary>Name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Unit.</summary>
    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
}

/// <summary>Content of a card or web result. Maps to <c>TakoResultContent</c>.</summary>
public sealed class TakoResultContent
{
    /// <summary><c>card_json</c>, <c>csv</c>, <c>json_compact</c> or <c>json_records</c>.</summary>
    [JsonPropertyName("content_format")]
    public string? ContentFormat { get; set; }

    /// <summary>Cost.</summary>
    [JsonPropertyName("cost")]
    public double? Cost { get; set; }

    /// <summary>Serialized data.</summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    /// <summary>Records.</summary>
    [JsonPropertyName("records")]
    public IList<IDictionary<string, TakoDatasetCell>>? Records { get; set; }

    /// <summary>Inline dataset.</summary>
    [JsonPropertyName("dataset")]
    public TakoDataset? Dataset { get; set; }

    /// <summary>Card data.</summary>
    [JsonPropertyName("card_data")]
    public IDictionary<string, JsonElement>? CardData { get; set; }

    /// <summary>Card data schema.</summary>
    [JsonPropertyName("card_data_schema")]
    public IDictionary<string, JsonElement>? CardDataSchema { get; set; }

    /// <summary>Url of the data.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>Expiry of <see cref="Url"/>.</summary>
    [JsonPropertyName("expires_at")]
    public string? ExpiresAt { get; set; }

    /// <summary>Total rows.</summary>
    [JsonPropertyName("total_rows")]
    public double? TotalRows { get; set; }

    /// <summary>Whether the rows were truncated.</summary>
    [JsonPropertyName("truncated")]
    public bool? Truncated { get; set; }

    /// <summary>Distinct records per premium class (for example <c>company_record</c>) among delivered rows.</summary>
    [JsonPropertyName("premium_rows")]
    public IDictionary<string, double>? PremiumRows { get; set; }

    /// <summary>Column header to the 0-based delivered-row positions whose premium value was withheld.</summary>
    [JsonPropertyName("redacted_cells")]
    public IDictionary<string, IList<double>>? RedactedCells { get; set; }

    /// <summary>Full set size when the card lists only a capped slice of it.</summary>
    [JsonPropertyName("total_matching")]
    public double? TotalMatching { get; set; }

    /// <summary>Export pricing.</summary>
    [JsonPropertyName("export_pricing")]
    public TakoExportPricing? ExportPricing { get; set; }

    /// <summary>Manifest.</summary>
    [JsonPropertyName("manifest")]
    public IList<TakoManifestColumn>? Manifest { get; set; }

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A source of a <see cref="TakoCard"/>.</summary>
public sealed class TakoCardSource
{
    /// <summary>Source name.</summary>
    [JsonPropertyName("source_name")]
    public string? SourceName { get; set; }

    /// <summary>Source description.</summary>
    [JsonPropertyName("source_description")]
    public string? SourceDescription { get; set; }

    /// <summary><c>data</c> or <c>web</c>.</summary>
    [JsonPropertyName("source_index")]
    public string SourceIndex { get; set; } = "";

    /// <summary>Source text.</summary>
    [JsonPropertyName("source_text")]
    public string? SourceText { get; set; }

    /// <summary>Source url.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>A methodology of a <see cref="TakoCard"/>.</summary>
public sealed class TakoMethodology
{
    /// <summary>Name.</summary>
    [JsonPropertyName("methodology_name")]
    public string? MethodologyName { get; set; }

    /// <summary>Description.</summary>
    [JsonPropertyName("methodology_description")]
    public string? MethodologyDescription { get; set; }
}

/// <summary>A Data Graph node of a <see cref="TakoCard"/>.</summary>
public sealed class TakoNode
{
    /// <summary>Id.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    /// <summary><c>entity</c> or <c>metric</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    /// <summary>Name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>A metric definition of a <see cref="TakoCard"/>.</summary>
public sealed class TakoMetricDefinition
{
    /// <summary>Name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Definition.</summary>
    [JsonPropertyName("definition")]
    public string Definition { get; set; } = "";
}

/// <summary>Freshness of the data of a <see cref="TakoCard"/>.</summary>
public sealed class TakoDataFreshness
{
    /// <summary>Coverage end.</summary>
    [JsonPropertyName("coverage_end")]
    public string? CoverageEnd { get; set; }

    /// <summary>Data as of.</summary>
    [JsonPropertyName("data_as_of")]
    public string? DataAsOf { get; set; }

    /// <summary>Last updated.</summary>
    [JsonPropertyName("last_updated")]
    public string? LastUpdated { get; set; }
}

/// <summary>Set when Tako withholds a card's data. Maps to <c>TakoCard['placeholder']</c>.</summary>
public sealed class TakoPlaceholder
{
    /// <summary>Name of the withheld source.</summary>
    [JsonPropertyName("withheld_source")]
    public string WithheldSource { get; set; } = "";

    /// <summary><c>not_served</c> (variable cost off or not served) or <c>over_cap</c> (over <c>MaxVariableCostUsd</c>).</summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A card of a Tako search response. Maps to <c>TakoCard</c>.</summary>
public sealed class TakoCard
{
    /// <summary>Card id.</summary>
    [JsonPropertyName("card_id")]
    public string? CardId { get; set; }

    /// <summary>Title.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>Description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Semantic description.</summary>
    [JsonPropertyName("semantic_description")]
    public string? SemanticDescription { get; set; }

    /// <summary>Webpage url.</summary>
    [JsonPropertyName("webpage_url")]
    public string? WebpageUrl { get; set; }

    /// <summary>Image url.</summary>
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    /// <summary>Embed url.</summary>
    [JsonPropertyName("embed_url")]
    public string? EmbedUrl { get; set; }

    /// <summary>Sources.</summary>
    [JsonPropertyName("sources")]
    public IList<TakoCardSource>? Sources { get; set; }

    /// <summary>Methodologies.</summary>
    [JsonPropertyName("methodologies")]
    public IList<TakoMethodology>? Methodologies { get; set; }

    /// <summary>Source indexes, <c>data</c> or <c>web</c>.</summary>
    [JsonPropertyName("source_indexes")]
    public IList<string>? SourceIndexes { get; set; }

    /// <summary>Card type.</summary>
    [JsonPropertyName("card_type")]
    public string? CardType { get; set; }

    /// <summary><c>High</c>, <c>Low</c> or <c>Medium</c>.</summary>
    [JsonPropertyName("relevance")]
    public string? Relevance { get; set; }

    /// <summary>Relevance to the query, 1.0-5.0. Null unless the account is entitled.</summary>
    [JsonPropertyName("relevance_score")]
    public double? RelevanceScore { get; set; }

    /// <summary>Content.</summary>
    [JsonPropertyName("content")]
    public TakoResultContent? Content { get; set; }

    /// <summary>Whether the card is exportable.</summary>
    [JsonPropertyName("exportable")]
    public bool? Exportable { get; set; }

    /// <summary>Data Graph nodes.</summary>
    [JsonPropertyName("nodes")]
    public IList<TakoNode>? Nodes { get; set; }

    /// <summary>Metric definitions.</summary>
    [JsonPropertyName("metric_definitions")]
    public IList<TakoMetricDefinition>? MetricDefinitions { get; set; }

    /// <summary>Data freshness.</summary>
    [JsonPropertyName("data_freshness")]
    public TakoDataFreshness? DataFreshness { get; set; }

    /// <summary>Set when Tako withholds the card's data.</summary>
    [JsonPropertyName("placeholder")]
    public TakoPlaceholder? Placeholder { get; set; }

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>A web result of a Tako search response. Maps to <c>TakoWebResult</c>.</summary>
public sealed class TakoWebResult
{
    /// <summary>Title.</summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    /// <summary>Url.</summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    /// <summary>Snippet.</summary>
    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }

    /// <summary>Source name.</summary>
    [JsonPropertyName("source_name")]
    public string? SourceName { get; set; }

    /// <summary>Publish date.</summary>
    [JsonPropertyName("publish_date")]
    public string? PublishDate { get; set; }

    /// <summary>Content.</summary>
    [JsonPropertyName("content")]
    public TakoResultContent? Content { get; set; }

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Cost of one part of a Tako search. Maps to <c>TakoSearchResponse['usage']['compute']</c> and <c>['variable_cost']</c>.</summary>
public sealed class TakoCostUsage
{
    /// <summary>Cost in USD.</summary>
    [JsonPropertyName("cost_usd")]
    public double CostUsd { get; set; }

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Data cost of a Tako search. Maps to <c>TakoSearchResponse['usage']['data']</c>.</summary>
public sealed class TakoDataUsage
{
    /// <summary>Cost in USD.</summary>
    [JsonPropertyName("cost_usd")]
    public double CostUsd { get; set; }

    /// <summary>Datasets.</summary>
    [JsonPropertyName("datasets")]
    public double Datasets { get; set; }

    /// <summary>Data points.</summary>
    [JsonPropertyName("data_points")]
    public double? DataPoints { get; set; }

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Usage of a Tako search. Maps to <c>TakoSearchResponse['usage']</c>.</summary>
public sealed class TakoUsage
{
    /// <summary>Total cost in USD.</summary>
    [JsonPropertyName("total_cost_usd")]
    public double TotalCostUsd { get; set; }

    /// <summary>Compute cost.</summary>
    [JsonPropertyName("compute")]
    public TakoCostUsage? Compute { get; set; }

    /// <summary>Data cost.</summary>
    [JsonPropertyName("data")]
    public TakoDataUsage? Data { get; set; }

    /// <summary>Variable-cost charge for the call.</summary>
    [JsonPropertyName("variable_cost")]
    public TakoCostUsage? VariableCost { get; set; }

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Output of the Tako search tool. Deserialize the tool output into it. Maps to <c>TakoSearchResponse</c>.</summary>
public sealed class TakoSearchResponse
{
    /// <summary>Request id.</summary>
    [JsonPropertyName("request_id")]
    public string RequestId { get; set; } = "";

    /// <summary>Cards.</summary>
    [JsonPropertyName("cards")]
    public IList<TakoCard>? Cards { get; set; }

    /// <summary>Web results.</summary>
    [JsonPropertyName("web_results")]
    public IList<TakoWebResult>? WebResults { get; set; }

    /// <summary>Usage.</summary>
    [JsonPropertyName("usage")]
    public TakoUsage? Usage { get; set; }

    /// <summary>Related search suggestions.</summary>
    [JsonPropertyName("related")]
    public IList<IDictionary<string, JsonElement>>? Related { get; set; }

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
