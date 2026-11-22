// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Vercel.AI.Gateway;

/// <summary>
/// Declares the Gateway DeepSeek Fetch tool: fetches a page as Markdown through DeepSeek's standalone Fetch API. The AI Gateway executes
/// it for any model, separate from native model web search. Maps to <c>gatewayTools.deepseekFetch</c>.
/// </summary>
public static class GatewayDeepSeekFetch
{
    private const string InputSchemaJson =
        @"{ ""type"": ""object"", ""properties"": { ""url"": { ""type"": ""string"", ""description"": ""URL to fetch, at most 8192 characters; HTTP(S) only, without a username or password."" } }, ""required"": [ ""url"" ] }";

    private const string OutputSchemaJson =
        @"{
  ""anyOf"": [
    {
      ""type"": ""object"",
      ""properties"": {
        ""title"": { ""type"": [ ""string"", ""null"" ] },
        ""content"": { ""type"": ""string"" }
      },
      ""required"": [ ""content"" ]
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

    /// <summary>Input schema the model fills: <c>url</c>.</summary>
    public static JsonElement InputSchema { get; } = Parse(InputSchemaJson);

    /// <summary>Output schema: a fetch response or a <see cref="GatewayToolError"/>.</summary>
    public static JsonElement OutputSchema { get; } = Parse(OutputSchemaJson);

    /// <summary>
    /// Creates the DeepSeek Fetch tool. The tool is executed by the AI Gateway, so it has no local execute function.
    /// Maps to <c>gatewayTools.deepseekFetch</c>.
    /// </summary>
    public static Tool Create(GatewayDeepSeekFetchConfig? config = null)
    {
        return Tool.ProviderDefined(
            "gateway.deepseek_fetch",
            "deepseek_fetch",
            (config ?? new GatewayDeepSeekFetchConfig()).ToArgs(),
            InputSchema
        );
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}

/// <summary>Developer-only configuration of the Gateway DeepSeek Fetch tool. Maps to <c>DeepSeekFetchConfig</c>.</summary>
public sealed class GatewayDeepSeekFetchConfig
{
    /// <summary>Pin the target URL instead of allowing the model to choose it. Maps to <c>url</c>.</summary>
    public string? Url { get; set; }

    internal JsonElement ToArgs()
    {
        var args = new JsonObject();
        if (Url != null)
        {
            args["url"] = Url;
        }

        return JsonSerializer.SerializeToElement(args);
    }
}

/// <summary>Output of the DeepSeek Fetch tool. Maps to <c>DeepSeekFetchResponse</c>.</summary>
public sealed class DeepSeekFetchResponse
{
    /// <summary>Page title.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>Page content as Markdown.</summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    /// <summary>Fields the schema does not declare.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>
/// Output of the DeepSeek Fetch tool: a <see cref="DeepSeekFetchResponse"/> or a <see cref="GatewayToolError"/>. Maps to
/// <c>DeepSeekFetchOutput</c>.
/// </summary>
[JsonConverter(typeof(DeepSeekFetchOutputConverter))]
public sealed class DeepSeekFetchOutput
{
    /// <summary>The fetch response. Null when the tool returned an error.</summary>
    public DeepSeekFetchResponse? Response { get; set; }

    /// <summary>The error. Null when the tool returned a response.</summary>
    public GatewayToolError? Error { get; set; }
}

internal sealed class DeepSeekFetchOutputConverter : JsonConverter<DeepSeekFetchOutput>
{
    public override DeepSeekFetchOutput? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("The DeepSeek Fetch output must be an object.");
        }

        var raw = root.GetRawText();
        if (
            root.TryGetProperty("content", out var content)
            && content.ValueKind == JsonValueKind.String
        )
        {
            return new DeepSeekFetchOutput
            {
                Response = JsonSerializer.Deserialize<DeepSeekFetchResponse>(raw, options),
            };
        }

        if (GatewayToolErrorShape.Matches(root))
        {
            return new DeepSeekFetchOutput
            {
                Error = JsonSerializer.Deserialize<GatewayToolError>(raw, options),
            };
        }

        throw new JsonException(
            "The DeepSeek Fetch output must be a fetch response (content) or a gateway tool error."
        );
    }

    public override void Write(
        Utf8JsonWriter writer,
        DeepSeekFetchOutput value,
        JsonSerializerOptions options
    )
    {
        if (value.Error is not null && value.Response is not null)
        {
            throw new JsonException(
                "A DeepSeek Fetch output holds a Response or an Error, not both."
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
