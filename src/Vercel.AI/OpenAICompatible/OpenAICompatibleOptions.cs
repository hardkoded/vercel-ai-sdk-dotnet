// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

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
    public Resolvable<string>? ApiKey { get; set; }

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

    /// <summary>When set, chat, completion, embedding, and Responses URLs use the Azure routes and this API version.</summary>
    public string? AzureApiVersion { get; set; }

    /// <summary>With <see cref="AzureApiVersion"/>, uses <c>/openai/deployments/{id}</c> routes instead of <c>/openai/v1</c>.</summary>
    public bool AzureUseDeploymentBasedUrls { get; set; }

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

    /// <summary>
    /// .NET-only hook. Rewrites each tool input schema and the response-format schema before it is sent.
    /// Moonshot uses it for <c>normalizeJsonSchemaForMFJS</c>, which upstream calls from its own chat model.
    /// </summary>
    internal Func<JsonNode?, JsonNode?>? NormalizeRequestSchema { get; set; }

    /// <summary>
    /// .NET-only hook. Rewrites the response-format schema before it is sent.
    /// When unset, <see cref="NormalizeRequestSchema"/> is used for both tool and response schemas.
    /// </summary>
    internal Func<JsonNode?, JsonNode?>? NormalizeResponseSchema { get; set; }

    /// <summary>When true, a missing API key fails the call. The generic provider leaves this unset.</summary>
    public bool RequireApiKey { get; set; }

    /// <summary>User-agent suffix. Empty uses <see cref="OpenAICompatibleInfo.UserAgent"/>.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Extracts provider-specific metadata from the response body and stream chunks. Maps to the <c>metadataExtractor</c> setting.</summary>
    public OpenAICompatibleMetadataExtractor? MetadataExtractor { get; set; }

    /// <summary>
    /// .NET-only hook. Returns the call headers to send in place of <see cref="LanguageModelCallOptions.Headers"/>.
    /// Cerebras uses it for <c>applyQueueThresholdHeader</c>, which upstream runs in its own chat model subclass.
    /// </summary>
    internal Func<
        LanguageModelCallOptions,
        IReadOnlyDictionary<string, string?>?
    >? PrepareCallHeaders { get; set; }

    /// <summary>Rewrites a provider error body into the exception message.</summary>
    public Func<string?, string?>? SelectErrorMessage { get; set; }

    /// <summary>Rewrites the JSON body before it is sent. Warnings raised here are returned with the call.</summary>
    public Func<JsonObject, IList<CallWarning>, JsonObject>? TransformRequestBody { get; set; }

    /// <summary>
    /// Rewrites the JSON body from the call options before <see cref="TransformRequestBody"/> runs. Arguments are the model id,
    /// the call options, the body, and the call warnings. Use it when a provider builds fields, such as messages, from the call itself.
    /// </summary>
    internal Func<
        string,
        LanguageModelCallOptions,
        JsonObject,
        IList<CallWarning>,
        JsonObject
    >? TransformChatRequest { get; set; }

    /// <summary>
    /// .NET-only hook. Converts the prompt to the chat <c>messages</c> in place of the shared conversion. Arguments are the model id,
    /// the call options, the provider options key, the <c>response_format</c> type (or null), and the call warnings.
    /// Moonshot uses it for <c>convertToMoonshotAIChatMessages</c>, which upstream calls from its own chat model.
    /// </summary>
    internal Func<
        string,
        LanguageModelCallOptions,
        string,
        string?,
        IList<CallWarning>,
        JsonArray
    >? ConvertChatMessages { get; set; }

    /// <summary>
    /// .NET-only hook. Validates a chat response body before it is converted and throws <see cref="InvalidResponseDataException"/>
    /// when it does not match. Moonshot uses it for <c>moonshotAIChatResponseSchema</c>.
    /// </summary>
    internal Action<JsonElement>? ValidateChatResponse { get; set; }

    /// <summary>
    /// When true, the <c>response_format</c> JSON schema carries the call's schema description. Groq sends it
    /// (<c>description: responseFormat.description</c>); Moonshot AI does not.
    /// </summary>
    internal bool SendsCallSchemaDescription { get; set; }

    /// <summary>
    /// When true, a streamed chunk carries its usage only in <c>x_groq.usage</c>, and a top-level <c>usage</c> is ignored.
    /// Maps to the Groq chunk schema in <c>groq-chat-language-model.ts</c>.
    /// </summary>
    internal bool StreamUsageFromXGroqOnly { get; set; }

    /// <summary>
    /// .NET-only hook. Maps a streamed <c>error</c> chunk to a structured provider error. Null keeps the message-only error part.
    /// Groq uses it for <c>createGroqStreamError</c>, which upstream calls from its own chat model.
    /// </summary>
    internal Func<JsonObject, ProviderStreamError?>? CreateStreamError { get; set; }

    /// <summary>
    /// .NET-only switch. When true, a streamed tool-call delta without an <c>index</c> takes its position in the
    /// <c>tool_calls</c> array, and a finish chunk's choice-level <c>usage</c> is used when no top-level usage arrives.
    /// Moonshot sets it for <c>toolCallDelta.index ?? index</c> and <c>topLevelUsage ?? choiceUsage</c>.
    /// </summary>
    internal bool StreamToolIndexAndChoiceUsage { get; set; }

    /// <summary>
    /// .NET-only hook. Validates a streamed chat chunk and returns the error message when it does not match, or null.
    /// Moonshot uses it for <c>moonshotAIChatChunkSchema</c>.
    /// </summary>
    internal Func<JsonObject, string?>? ValidateChatChunk { get; set; }

    /// <summary>
    /// Reads provider metadata from the response headers. The entries are added to the provider metadata of the
    /// result and of the stream finish part. Maps to <c>extractCacheWriteTokensByTtl(responseHeaders)</c> in the Moonshot AI chat model.
    /// </summary>
    internal Func<
        IReadOnlyDictionary<string, string>?,
        JsonObject?
    >? ExtractResponseHeaderMetadata { get; set; }

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
    public Func<
        string,
        string?,
        string?,
        IList<CallWarning>,
        string?
    >? ResolveReasoningEffort { get; set; }

    /// <summary>
    /// Maps the top-level <c>Reasoning</c> option to the <c>reasoning_effort</c> request value. Arguments are the model id,
    /// the portable reasoning level, and the call warnings. It is not applied to a <c>reasoningEffort</c> from provider
    /// options, which is always sent unchanged. Null sends the reasoning value directly. Maps to <c>mapReasoning</c>.
    /// </summary>
    public Func<string, string, IList<CallWarning>, string?>? MapReasoning { get; set; }

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

    /// <summary>Origin for video calls when it differs from <see cref="BaseUrl"/>. Alibaba reads it as <c>videoBaseURL</c>.</summary>
    public string? VideoBaseUrl { get; set; }

    /// <summary>
    /// Client for hops off the provider origin when a URL from a response body is downloaded (Fal, Replicate, Luma, Prodia).
    /// It must not follow redirects: each hop is validated before it is requested. Null uses the HttpClient the provider was created with when <c>Create</c> was given a handler, else a shared client that does not follow them.
    /// </summary>
    public HttpClient? DownloadHttpClient { get; set; }

    /// <summary>
    /// Fetch used to download URLs from a response body (Fal). Maps to the provider setting <c>fetch</c> that upstream also uses for downloads.
    /// Pass a <see cref="Vercel.AI.Util.GuardedFetch"/> for connect-time DNS validation. It takes precedence over <see cref="DownloadHttpClient"/>.
    /// </summary>
    public Vercel.AI.Util.DownloadFetch? DownloadFetch { get; set; }

    /// <summary>Generates ids for sources and for streamed tool calls that have none. Maps to the provider setting <c>generateId</c>. Used by the Mistral Conversations model.</summary>
    public Func<string>? GenerateId { get; set; }

    /// <summary>
    /// URLs the chat model reads itself, as regular expression text per media type. Maps to the <c>supportedUrls</c> config.
    /// When it is null, no URL is supported and prompt file URLs are downloaded.
    /// </summary>
    public IReadOnlyDictionary<string, string>? SupportedUrls { get; set; }

    /// <summary>
    /// Returns a copy. Providers change the copy, so the options the caller passed in are left as they were.
    /// </summary>
    public OpenAICompatibleOptions Copy()
    {
        var copy = (OpenAICompatibleOptions)MemberwiseClone();
        copy.AdditionalApiKeyEnvironmentVariables = (string[])
            AdditionalApiKeyEnvironmentVariables.Clone();
        copy.Headers = new Dictionary<string, string>(Headers);
        copy.QueryParameters = new Dictionary<string, string>(QueryParameters);
        return copy;
    }
}
