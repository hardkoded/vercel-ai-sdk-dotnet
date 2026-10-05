// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAICompatible;

/// <summary>How the API key is sent.</summary>
public enum ApiKeyStyle
{
    /// <summary><c>Authorization: Bearer</c>.</summary>
    Bearer,

    /// <summary>A custom header such as <c>api-key</c>.</summary>
    ApiKeyHeader,

    /// <summary><c>Authorization: Token</c>.</summary>
    Token,

    /// <summary><c>Authorization: Key</c>.</summary>
    Key,

    /// <summary>The raw key as the <c>Authorization</c> value.</summary>
    RawAuthorization,

    /// <summary>A named header whose value is the raw key.</summary>
    CustomHeader,
}

/// <summary>Settings for an OpenAI Chat Completions compatible provider.</summary>
public class OpenAICompatibleOptions
{
    /// <summary>Provider id recorded on models.</summary>
    public string ProviderName { get; set; } = "openai-compatible";

    /// <summary>API origin, without a trailing slash. Chat calls append <see cref="ChatCompletionsPath"/>.</summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>Explicit API key. Wins over the environment variable.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Environment variable read when <see cref="ApiKey"/> is empty.</summary>
    public string ApiKeyEnvironmentVariable { get; set; } = "OPENAI_API_KEY";

    /// <summary>Extra environment variables accepted as a fallback.</summary>
    public string[] AdditionalApiKeyEnvironmentVariables { get; set; } = Array.Empty<string>();

    /// <summary>How to send the key.</summary>
    public ApiKeyStyle ApiKeyStyle { get; set; } = ApiKeyStyle.Bearer;

    /// <summary>Header name for <see cref="ApiKeyStyle.ApiKeyHeader"/> and <see cref="ApiKeyStyle.CustomHeader"/>.</summary>
    public string ApiKeyHeaderName { get; set; } = "api-key";

    /// <summary>Relative chat path.</summary>
    public string ChatCompletionsPath { get; set; } = "chat/completions";

    /// <summary>Relative embeddings path.</summary>
    public string EmbeddingsPath { get; set; } = "embeddings";

    /// <summary>Relative images path.</summary>
    public string ImagesPath { get; set; } = "images/generations";

    /// <summary>Whether embedding calls are supported.</summary>
    public bool SupportsEmbeddings { get; set; } = true;

    /// <summary>Whether <see cref="OpenAICompatibleProvider.ImageModel"/> is supported.</summary>
    public bool SupportsImages { get; set; }

    /// <summary>When set, chat and embedding URLs use the Azure deployments route and this API version.</summary>
    public string? AzureApiVersion { get; set; }

    /// <summary>Extra headers sent on every call. These override the API key header.</summary>
    public Dictionary<string, string> Headers { get; } = new();

    /// <summary>Query parameters appended to every request URL.</summary>
    public Dictionary<string, string> QueryParameters { get; } = new();

    /// <summary>When true, streaming requests send <c>stream_options.include_usage</c>.</summary>
    public bool IncludeUsage { get; set; }

    /// <summary>When true, a JSON schema is sent as <c>json_schema</c> instead of <c>json_object</c>.</summary>
    public bool SupportsStructuredOutputs { get; set; }

    /// <summary>When true, a JSON-mode response that has text drops its tool calls, and a <c>tool_calls</c> finish becomes stop.</summary>
    public bool JsonTextOverridesToolCalls { get; set; }

    /// <summary>When true, a missing API key fails the call. The generic provider leaves this unset.</summary>
    public bool RequireApiKey { get; set; }

    /// <summary>User-agent suffix. Empty uses <see cref="OpenAICompatibleInfo.UserAgent"/>.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Rewrites a provider error body into the exception message.</summary>
    public Func<string?, string?>? SelectErrorMessage { get; set; }

    /// <summary>Rewrites the JSON body before it is sent. Warnings raised here are returned with the call.</summary>
    public Func<JsonObject, IList<CallWarning>, JsonObject>? TransformRequestBody { get; set; }

    /// <summary>Rewrites the raw chat usage object before token counts are read.</summary>
    public Func<JsonElement, JsonElement>? TransformUsage { get; set; }

    /// <summary>Maps a raw finish reason before the shared mapping. Null falls through.</summary>
    public Func<string?, FinishReason?>? MapFinishReason { get; set; }

    /// <summary>Relative completions path.</summary>
    public string CompletionsPath { get; set; } = "completions";

    /// <summary>Relative rerank path.</summary>
    public string RerankPath { get; set; } = "rerank";

    /// <summary>Maximum embedding inputs in one request.</summary>
    public int MaxEmbeddingsPerCall { get; set; } = 2048;

    /// <summary>Origin for image calls when it differs from <see cref="BaseUrl"/>.</summary>
    public string? ImageBaseUrl { get; set; }
}
