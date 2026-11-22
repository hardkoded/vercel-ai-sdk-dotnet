// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Vercel.AI.Gateway;

/// <summary>
/// Declares the Gateway DeepSeek Search tool: searches the web through DeepSeek's standalone Search API. The AI Gateway executes it for
/// any model, separate from native model web search. Maps to <c>gatewayTools.deepseekSearch</c>.
/// </summary>
public static class GatewayDeepSeekSearch
{
    private const string InputSchemaJson =
        @"{ ""type"": ""object"", ""properties"": { ""query"": { ""type"": ""string"", ""description"": ""Web search query, 1 to 128 Unicode characters. Supports site:, -site:, lang:, after:, before:, and quoted phrases."" }, ""count"": { ""type"": ""integer"", ""description"": ""Number of results, 1 to 50 (default: 10)."" }, ""need_content"": { ""type"": ""boolean"", ""description"": ""Include available page content (default: false)."" }, ""ori_question"": { ""type"": [ ""string"", ""null"" ], ""description"": ""Original user question, 1 to 128 Unicode characters."" } }, ""required"": [ ""query"" ] }";

    private const string OutputSchemaJson =
        @"{
  ""anyOf"": [
    {
      ""type"": ""object"",
      ""properties"": {
        ""results"": {
          ""type"": ""array"",
          ""items"": {
            ""type"": ""object"",
            ""properties"": {
              ""authority"": { ""type"": [ ""number"", ""null"" ] },
              ""content"": { ""type"": [ ""string"", ""null"" ] },
              ""date"": { ""type"": [ ""string"", ""null"" ] },
              ""rerank_score"": { ""type"": [ ""number"", ""null"" ] },
              ""site_icon"": { ""type"": [ ""string"", ""null"" ] },
              ""site_name"": { ""type"": [ ""string"", ""null"" ] },
              ""snippet"": { ""type"": [ ""string"", ""null"" ] },
              ""summary"": { ""type"": [ ""string"", ""null"" ] },
              ""title"": { ""type"": [ ""string"", ""null"" ] },
              ""url"": { ""type"": [ ""string"", ""null"" ] }
            }
          }
        }
      },
      ""required"": [ ""results"" ]
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
  ]
}";

    /// <summary>Input schema the model fills: <c>query</c>, <c>count</c>, <c>need_content</c> and <c>ori_question</c>. <c>localization</c> is developer-only.</summary>
    public static JsonElement InputSchema { get; } = Parse(InputSchemaJson);

    /// <summary>Output schema: a search response or a <see cref="GatewayToolError"/>.</summary>
    public static JsonElement OutputSchema { get; } = Parse(OutputSchemaJson);

    /// <summary>
    /// Creates the DeepSeek Search tool. The tool is executed by the AI Gateway, so it has no local execute function.
    /// Maps to <c>gatewayTools.deepseekSearch</c>.
    /// </summary>
    public static Tool Create(GatewayDeepSeekSearchConfig? config = null)
    {
        return Tool.ProviderDefined(
            "gateway.deepseek_search",
            "deepseek_search",
            (config ?? new GatewayDeepSeekSearchConfig()).ToArgs(),
            InputSchema
        );
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}

/// <summary>Developer-only configuration of the Gateway DeepSeek Search tool. Maps to <c>DeepSeekSearchConfig</c>.</summary>
public sealed class GatewayDeepSeekSearchConfig
{
    /// <summary>Pin the query instead of allowing the model to choose it. Maps to <c>query</c>.</summary>
    public string? Query { get; set; }

    /// <summary>Maximum number of results, 1–50 (default: 10). Maps to <c>count</c>.</summary>
    public int? Count { get; set; }

    /// <summary>Include available page content (default: false). Maps to <c>needContent</c>.</summary>
    public bool? NeedContent { get; set; }

    /// <summary>Original question, 1–128 Unicode characters. Maps to <c>oriQuestion</c>.</summary>
    public string? OriQuestion { get; set; }

    /// <summary>Sends <c>oriQuestion: null</c> (an absent question). Takes precedence over <see cref="OriQuestion"/>.</summary>
    public bool OriQuestionNull { get; set; }

    /// <summary>Developer-configured IP address for result localization. Maps to <c>localization</c>.</summary>
    public string? Localization { get; set; }

    internal JsonElement ToArgs()
    {
        var args = new JsonObject();
        if (Query != null)
        {
            args["query"] = Query;
        }

        if (Count is { } count)
        {
            args["count"] = count;
        }

        if (NeedContent is { } needContent)
        {
            args["needContent"] = needContent;
        }

        if (OriQuestionNull)
        {
            args["oriQuestion"] = null;
        }
        else if (OriQuestion != null)
        {
            args["oriQuestion"] = OriQuestion;
        }

        if (Localization != null)
        {
            args["localization"] = Localization;
        }

        return JsonSerializer.SerializeToElement(args);
    }
}

/// <summary>One DeepSeek search result. Maps to <c>DeepSeekSearchResult</c>.</summary>
public sealed class DeepSeekSearchResult
{
    /// <summary>Source authority.</summary>
    [JsonPropertyName("authority")]
    public double? Authority { get; set; }

    /// <summary>Page content.</summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>Publication date.</summary>
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    /// <summary>Rerank score.</summary>
    [JsonPropertyName("rerank_score")]
    public double? RerankScore { get; set; }

    /// <summary>Site icon URL.</summary>
    [JsonPropertyName("site_icon")]
    public string? SiteIcon { get; set; }

    /// <summary>Site name.</summary>
    [JsonPropertyName("site_name")]
    public string? SiteName { get; set; }

    /// <summary>Snippet.</summary>
    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }

    /// <summary>Summary.</summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>Title.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>Output of the DeepSeek Search tool. Maps to <c>DeepSeekSearchResponse</c>.</summary>
public sealed class DeepSeekSearchResponse
{
    /// <summary>Search results.</summary>
    [JsonPropertyName("results")]
    public IList<DeepSeekSearchResult> Results { get; set; } = [];

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>
/// Output of the DeepSeek Search tool: a <see cref="DeepSeekSearchResponse"/> or a <see cref="GatewayToolError"/>. Maps to
/// <c>DeepSeekSearchOutput</c>.
/// </summary>
[JsonConverter(typeof(DeepSeekSearchOutputConverter))]
public sealed class DeepSeekSearchOutput
{
    /// <summary>The search response. Null when the tool returned an error.</summary>
    public DeepSeekSearchResponse? Response { get; set; }

    /// <summary>The error. Null when the tool returned a response.</summary>
    public GatewayToolError? Error { get; set; }
}

internal sealed class DeepSeekSearchOutputConverter : JsonConverter<DeepSeekSearchOutput>
{
    public override DeepSeekSearchOutput? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("The DeepSeek Search output must be an object.");
        }

        var raw = root.GetRawText();
        if (
            root.TryGetProperty("results", out var results)
            && results.ValueKind == JsonValueKind.Array
        )
        {
            return new DeepSeekSearchOutput
            {
                Response = JsonSerializer.Deserialize<DeepSeekSearchResponse>(raw, options),
            };
        }

        if (GatewayToolErrorShape.Matches(root))
        {
            return new DeepSeekSearchOutput
            {
                Error = JsonSerializer.Deserialize<GatewayToolError>(raw, options),
            };
        }

        throw new JsonException(
            "The DeepSeek Search output must be a search response (results) or a gateway tool error."
        );
    }

    public override void Write(
        Utf8JsonWriter writer,
        DeepSeekSearchOutput value,
        JsonSerializerOptions options
    )
    {
        if (value.Error is not null && value.Response is not null)
        {
            throw new JsonException(
                "A DeepSeek Search output holds a Response or an Error, not both."
            );
        }

        if (value.Error is { } error)
        {
            JsonSerializer.Serialize(writer, error, options);
        }
        else if (value.Response is { } response)
        {
            JsonSerializer.Serialize(writer, response, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
