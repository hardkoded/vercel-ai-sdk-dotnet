// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Gateway;

/// <summary>JSON schemas for the Gateway Tako search tool.</summary>
public static class GatewayTakoSearch
{
    private const string InputSchemaJson = @"{
  ""type"": ""object"",
  ""properties"": {
    ""query"": { ""type"": ""string"", ""description"": ""Natural-language search query."" },
    ""sources"": {
      ""type"": ""object"",
      ""description"": ""Sources to search. Omit to search both curated data and the web."",
      ""properties"": {
        ""data"": {
          ""type"": ""object"",
          ""properties"": {
            ""count"": { ""type"": ""number"", ""description"": ""Maximum number of data results to return (1-20). When include_contents is true, each additional result adds its own data surcharge."" },
            ""include_contents"": { ""type"": ""boolean"", ""description"": ""Inline rows for each data result. This adds a data surcharge based on row count and dataset source. To estimate cost, search with include_contents disabled and inspect cards.content.export_pricing. This applies to every returned card; limit sources.data.count and sources.data.max_rows to control cost."" },
            ""max_rows"": { ""type"": ""number"", ""description"": ""Maximum rows to inline per result. Omit to use the allowance in cards.content.export_pricing. A data surcharge applies per 1,000 exported rows; lower values reduce cost."" }
          }
        }
      }
    }
  },
  ""required"": [ ""query"" ]
}";

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
        ""source_indexes"": { ""type"": [ ""array"", ""null"" ], ""items"": { ""enum"": [ ""data"", ""web"" ] } },
        ""card_type"": { ""type"": [ ""string"", ""null"" ] },
        ""relevance"": { ""enum"": [ ""High"", ""Low"", ""Medium"", null ] },
        ""relevance_score"": { ""type"": [ ""number"", ""null"" ] },
        ""content"": { ""$ref"": ""#/definitions/content"" },
        ""exportable"": { ""type"": ""boolean"" },
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
