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
    public Dictionary<string, string> Headers { get; private set; } = new();

    /// <summary>Query parameters appended to every request URL.</summary>
    public Dictionary<string, string> QueryParameters { get; private set; } = new();

    /// <summary>When true, streaming requests send <c>stream_options.include_usage</c>.</summary>
    public bool IncludeUsage { get; set; }

    /// <summary>When true, a JSON schema is sent as <c>json_schema</c> instead of <c>json_object</c>.</summary>
    public bool SupportsStructuredOutputs { get; set; }

    /// <summary>When true, a tool result with <see cref="ToolModelMessage.OutputContent"/> is sent as an array of content parts. Otherwise the parts are sent as a JSON string.</summary>
    public bool SupportsMultiPartToolContent { get; set; }

    /// <summary>When true, a JSON-mode response that has text drops its tool calls, and a <c>tool_calls</c> finish becomes stop.</summary>
    public bool JsonTextOverridesToolCalls { get; set; }

    /// <summary>
    /// When true, a call that has a JSON schema and function tools sends an internal JSON response tool instead of <c>response_format</c>.
    /// The provider rejects the two together.
    /// </summary>
    internal bool UsesJsonResponseToolWithTools { get; set; }

    /// <summary>
    /// When true, the shared request leaves out <c>tools</c> and <c>tool_choice</c>. <see cref="TransformChatRequest"/> adds them,
    /// so a tool the shared mapping cannot serialize (a provider-defined tool) does not fail the call.
    /// </summary>
    internal bool PreparesOwnTools { get; set; }

    /// <summary>When true, a missing API key fails the call. The generic provider leaves this unset.</summary>
    public bool RequireApiKey { get; set; }

    /// <summary>User-agent suffix. Empty uses <see cref="OpenAICompatibleInfo.UserAgent"/>.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Rewrites a provider error body into the exception message.</summary>
    public Func<string?, string?>? SelectErrorMessage { get; set; }

    /// <summary>Rewrites the JSON body before it is sent. Warnings raised here are returned with the call.</summary>
    public Func<JsonObject, IList<CallWarning>, JsonObject>? TransformRequestBody { get; set; }

    /// <summary>
    /// Rewrites the JSON body from the call options before <see cref="TransformRequestBody"/> runs. Arguments are the model id,
    /// the call options, the body, and the call warnings. Use it when a provider builds fields, such as messages, from the call itself.
    /// </summary>
    internal Func<string, LanguageModelCallOptions, JsonObject, IList<CallWarning>, JsonObject>? TransformChatRequest { get; set; }

    /// <summary>Streams <c>tool-input-start</c>, <c>tool-input-delta</c> and <c>tool-input-end</c> parts for every tool call, not only for the JSON response tool.</summary>
    internal bool StreamToolInput { get; set; }

    /// <summary>Reports <see cref="FinishReason.Interrupted"/> without an error part when a stream ends cleanly without a finish reason. Upstream openai-compatible keeps its own behavior; providers built on it opt in.</summary>
    internal bool InterruptedWithoutFinish { get; set; }

    /// <summary>Id of the streamed text block. Upstream openai-compatible uses <c>txt-0</c>.</summary>
    internal string TextBlockId { get; set; } = "txt-0";

    /// <summary>Creates the id of a streamed reasoning block. Null keeps <c>reasoning-0</c>, like upstream openai-compatible.</summary>
    internal Func<string>? ReasoningBlockId { get; set; }

    /// <summary>
    /// Resolves <c>reasoning_effort</c> for a model. Arguments are the model id, the portable <c>Reasoning</c> level,
    /// the provider option effort, and the call warnings. Null removes the field. Null here keeps the default behavior.
    /// </summary>
    public Func<string, string?, string?, IList<CallWarning>, string?>? ResolveReasoningEffort { get; set; }

    /// <summary>Converts the raw chat usage object. Null uses <see cref="OpenAICompatibleChat.ConvertUsage"/>.</summary>
    public Func<JsonElement, LanguageModelUsage>? ConvertUsage { get; set; }

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

    /// <summary>
    /// Client for hops off the provider origin when a URL from a response body is downloaded (Fal, Replicate, Luma, Prodia).
    /// It must not follow redirects: each hop is validated before it is requested. Null uses a shared client that does not follow them.
    /// </summary>
    public HttpClient? DownloadHttpClient { get; set; }

    /// <summary>
    /// Returns a copy. Providers change the copy, so the options the caller passed in are left as they were.
    /// </summary>
    public OpenAICompatibleOptions Copy()
    {
        var copy = (OpenAICompatibleOptions)MemberwiseClone();
        copy.AdditionalApiKeyEnvironmentVariables = (string[])AdditionalApiKeyEnvironmentVariables.Clone();
        copy.Headers = new Dictionary<string, string>(Headers);
        copy.QueryParameters = new Dictionary<string, string>(QueryParameters);
        return copy;
    }
}
