// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Sdk.OpenAICompatible;

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

    /// <summary>Extra headers sent on every call.</summary>
    public Dictionary<string, string> Headers { get; } = new();
}
