// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

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
  ""type"": ""object"",
  ""properties"": {
    ""request_id"": { ""type"": ""string"" },
    ""cards"": { ""type"": ""array"" },
    ""web_results"": { ""type"": ""array"" }
  },
  ""required"": [ ""request_id"" ]
}";

    /// <summary>Input schema. Data-source descriptions name the surcharge controls.</summary>
    public static JsonElement InputSchema { get; } = Parse(InputSchemaJson);

    /// <summary>Output schema. Internal ranking fields are not part of the contract.</summary>
    public static JsonElement OutputSchema { get; } = Parse(OutputSchemaJson);

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
