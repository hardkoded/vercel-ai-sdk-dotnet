// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAICompatible;

/// <summary>Provider that speaks the OpenAI Chat Completions, embeddings, and images APIs.</summary>
public class OpenAICompatibleProvider : ProviderBase
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider. The caller owns <paramref name="httpClient"/>.</summary>
    public OpenAICompatibleProvider(OpenAICompatibleOptions options, HttpClient httpClient)
        : base(options?.ProviderName ?? throw new ArgumentNullException(nameof(options)))
    {
        Options = options;
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Http = new ProviderHttp(httpClient);
    }

    /// <summary>Options used to build requests.</summary>
    public OpenAICompatibleOptions Options { get; }

    /// <summary>HTTP helper used by the models.</summary>
    public ProviderHttp Http { get; }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return CreateChatModel(modelId);
    }

    /// <summary>Creates a chat model. Providers override this when support differs by model id.</summary>
    public virtual OpenAICompatibleLanguageModel CreateChatModel(string modelId)
    {
        return new OpenAICompatibleLanguageModel(this, modelId);
    }

    /// <summary>Creates a legacy Completions model.</summary>
    public virtual OpenAICompatibleCompletionLanguageModel CompletionModel(string modelId)
    {
        return new OpenAICompatibleCompletionLanguageModel(this, modelId);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        if (!Options.SupportsEmbeddings)
        {
            return base.EmbeddingModel(modelId);
        }

        return new OpenAICompatibleEmbeddingModel(this, modelId);
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        if (!Options.SupportsImages)
        {
            return base.ImageModel(modelId);
        }

        return new OpenAICompatibleImageModel(this, modelId);
    }

    /// <summary>Creates a provider with an <see cref="HttpClient"/> that uses <paramref name="handler"/>.</summary>
    public static OpenAICompatibleProvider Create(OpenAICompatibleOptions options, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new OpenAICompatibleProvider(options, client);
    }

    /// <summary>Resolves the API key and builds request headers.</summary>
    public Dictionary<string, string?> CreateHeaders()
    {
        return CreateHeaders(null);
    }

    /// <summary>Builds headers and lets <paramref name="callHeaders"/> override them.</summary>
    public Dictionary<string, string?> CreateHeaders(IReadOnlyDictionary<string, string?>? callHeaders)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var key = ResolveApiKey();
        if (!string.IsNullOrEmpty(key))
        {
            switch (Options.ApiKeyStyle)
            {
                case ApiKeyStyle.Bearer:
                    headers["Authorization"] = "Bearer " + key;
                    break;
                case ApiKeyStyle.Token:
                    headers["Authorization"] = "Token " + key;
                    break;
                case ApiKeyStyle.Key:
                    headers["Authorization"] = "Key " + key;
                    break;
                case ApiKeyStyle.RawAuthorization:
                    headers["Authorization"] = key;
                    break;
                case ApiKeyStyle.ApiKeyHeader:
                case ApiKeyStyle.CustomHeader:
                    headers[Options.ApiKeyHeaderName] = key;
                    break;
            }
        }

        foreach (var pair in Options.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        var suffix = string.IsNullOrEmpty(Options.UserAgent)
            ? OpenAICompatibleInfo.UserAgent("openai-compatible")
            : Options.UserAgent!;
        headers.TryGetValue("User-Agent", out var current);
        if (string.IsNullOrEmpty(current))
        {
            headers["User-Agent"] = suffix;
        }
        else if (current!.IndexOf(suffix, StringComparison.Ordinal) < 0)
        {
            headers["User-Agent"] = current + " " + suffix;
        }

        if (callHeaders != null)
        {
            foreach (var pair in callHeaders)
            {
                if (pair.Value != null)
                {
                    headers[pair.Key] = pair.Value;
                }
            }
        }

        return headers;
    }

    /// <summary>Resolves the API key. A missing key is omitted unless <see cref="OpenAICompatibleOptions.RequireApiKey"/> is set.</summary>
    protected virtual string? ResolveApiKey()
    {
        if (!string.IsNullOrWhiteSpace(Options.ApiKey))
        {
            return Options.ApiKey;
        }

        var fromEnvironment = Environment.GetEnvironmentVariable(Options.ApiKeyEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment;
        }

        if (Options.AdditionalApiKeyEnvironmentVariables != null)
        {
            foreach (var name in Options.AdditionalApiKeyEnvironmentVariables)
            {
                var value = Environment.GetEnvironmentVariable(name);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        if (Options.RequireApiKey)
        {
            throw new AiSdkException(
                "API key is required. Pass it explicitly or set the " + Options.ApiKeyEnvironmentVariable + " environment variable.");
        }

        return null;
    }

    /// <summary>Posts JSON and rewrites the provider error message when one is available.</summary>
    public async Task<ProviderTextResponse> PostJsonAsync(
        Uri uri,
        string json,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        try
        {
            return await Http.SendJsonStringAsync(HttpMethod.Post, uri, json, headers, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            var message = Options.SelectErrorMessage?.Invoke(exception.ResponseBody) ?? OpenAICompatibleChat.ReadErrorMessage(exception.ResponseBody);
            if (string.IsNullOrEmpty(message) || message == exception.Message)
            {
                throw;
            }

            throw OpenAICompatibleChat.WithMessage(exception, message!);
        }
    }

    /// <summary>Posts JSON and opens an SSE response. The caller disposes the result.</summary>
    public async Task<OpenAICompatibleSseResponse> PostSseAsync(
        Uri uri,
        string json,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(json ?? string.Empty, Encoding.UTF8, "application/json"),
        };
        ApplyHeaders(request, headers);
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new ApiUserAbortException();
        }
        catch (HttpRequestException exception)
        {
            throw new ApiConnectionException("The connection to the provider failed.", exception);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            response.Dispose();
            var error = ProviderHttp.MapStatus((int)response.StatusCode, body);
            var message = Options.SelectErrorMessage?.Invoke(body) ?? OpenAICompatibleChat.ReadErrorMessage(body);
            if (!string.IsNullOrEmpty(message) && message != error.Message)
            {
                throw OpenAICompatibleChat.WithMessage(error, message!);
            }

            throw error;
        }

        return new OpenAICompatibleSseResponse(response);
    }

    /// <summary>Posts multipart form data and rewrites the provider error message when one is available.</summary>
    public async Task<ProviderTextResponse> PostMultipartAsync(
        Uri uri,
        MultipartFormDataContent content,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
        ApplyHeaders(request, headers);
        return await ReadTextAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Downloads raw bytes.</summary>
    public Task<OpenAICompatibleBinaryResponse> GetBinaryAsync(
        Uri uri,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        ApplyHeaders(request, headers);
        return ReadBinaryAsync(request, cancellationToken);
    }

    /// <summary>Posts JSON and returns the raw response bytes.</summary>
    public async Task<OpenAICompatibleBinaryResponse> PostBinaryAsync(
        Uri uri,
        string json,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(json ?? string.Empty, Encoding.UTF8, "application/json"),
        };
        ApplyHeaders(request, headers);
        return await ReadBinaryAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task<OpenAICompatibleBinaryResponse> ReadBinaryAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new ApiUserAbortException();
        }
        catch (HttpRequestException exception)
        {
            throw new ApiConnectionException("The connection to the provider failed.", exception);
        }

        var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        var copied = CopyHeaders(response);
        var status = (int)response.StatusCode;
        var success = response.IsSuccessStatusCode;
        response.Dispose();
        if (!success)
        {
            var body = Encoding.UTF8.GetString(bytes);
            var error = ProviderHttp.MapStatus(status, body);
            var message = Options.SelectErrorMessage?.Invoke(body) ?? OpenAICompatibleChat.ReadErrorMessage(body);
            if (!string.IsNullOrEmpty(message) && message != error.Message)
            {
                throw OpenAICompatibleChat.WithMessage(error, message!);
            }

            throw error;
        }

        return new OpenAICompatibleBinaryResponse(bytes, copied, status);
    }

    private async Task<ProviderTextResponse> ReadTextAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new ApiUserAbortException();
        }
        catch (HttpRequestException exception)
        {
            throw new ApiConnectionException("The connection to the provider failed.", exception);
        }

        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var copied = CopyHeaders(response);
        var status = (int)response.StatusCode;
        var success = response.IsSuccessStatusCode;
        response.Dispose();
        if (!success)
        {
            var error = ProviderHttp.MapStatus(status, body);
            var message = Options.SelectErrorMessage?.Invoke(body) ?? OpenAICompatibleChat.ReadErrorMessage(body);
            if (!string.IsNullOrEmpty(message) && message != error.Message)
            {
                throw OpenAICompatibleChat.WithMessage(error, message!);
            }

            throw error;
        }

        return new ProviderTextResponse(body, copied);
    }

    /// <summary>Chat Completions URL for a model. Azure deployment routes include the model id.</summary>
    public Uri ChatUri(string modelId)
    {
        if (!string.IsNullOrEmpty(Options.AzureApiVersion))
        {
            return WithQuery(ApiKeys.Combine(
                Options.BaseUrl,
                "openai/deployments/" + Uri.EscapeDataString(modelId) + "/chat/completions?api-version=" + Options.AzureApiVersion));
        }

        return WithQuery(ApiKeys.Combine(Options.BaseUrl, Options.ChatCompletionsPath));
    }

    /// <summary>Completions URL.</summary>
    public Uri CompletionsUri(string modelId)
    {
        if (!string.IsNullOrEmpty(Options.AzureApiVersion))
        {
            return WithQuery(ApiKeys.Combine(
                Options.BaseUrl,
                "openai/deployments/" + Uri.EscapeDataString(modelId) + "/completions?api-version=" + Options.AzureApiVersion));
        }

        return WithQuery(ApiKeys.Combine(Options.BaseUrl, Options.CompletionsPath));
    }

    /// <summary>Embeddings URL.</summary>
    public Uri EmbeddingsUri(string modelId)
    {
        if (!string.IsNullOrEmpty(Options.AzureApiVersion))
        {
            return WithQuery(ApiKeys.Combine(
                Options.BaseUrl,
                "openai/deployments/" + Uri.EscapeDataString(modelId) + "/embeddings?api-version=" + Options.AzureApiVersion));
        }

        return WithQuery(ApiKeys.Combine(Options.BaseUrl, Options.EmbeddingsPath));
    }

    /// <summary>Images URL.</summary>
    public Uri ImagesUri()
    {
        var origin = string.IsNullOrEmpty(Options.ImageBaseUrl) ? Options.BaseUrl : Options.ImageBaseUrl!;
        return WithQuery(ApiKeys.Combine(origin, Options.ImagesPath));
    }

    /// <summary>Image edits URL.</summary>
    public Uri ImagesEditsUri()
    {
        var origin = string.IsNullOrEmpty(Options.ImageBaseUrl) ? Options.BaseUrl : Options.ImageBaseUrl!;
        return WithQuery(ApiKeys.Combine(origin, "images/edits"));
    }

    /// <summary>Rerank URL.</summary>
    public Uri RerankUri()
    {
        return WithQuery(ApiKeys.Combine(Options.BaseUrl, Options.RerankPath));
    }

    private Uri WithQuery(Uri uri)
    {
        if (Options.QueryParameters.Count == 0)
        {
            return uri;
        }

        var builder = new UriBuilder(uri);
        var query = builder.Query;
        if (query.StartsWith("?", StringComparison.Ordinal))
        {
            query = query.Substring(1);
        }

        var parts = new List<string>();
        if (query.Length > 0)
        {
            parts.Add(query);
        }

        foreach (var pair in Options.QueryParameters)
        {
            parts.Add(Uri.EscapeDataString(pair.Key) + "=" + Uri.EscapeDataString(pair.Value));
        }

        builder.Query = string.Join("&", parts);
        return builder.Uri;
    }

    private static void ApplyHeaders(HttpRequestMessage request, IReadOnlyDictionary<string, string?>? headers)
    {
        if (headers == null)
        {
            return;
        }

        foreach (var pair in headers)
        {
            if (string.IsNullOrEmpty(pair.Value) || pair.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (pair.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                var value = pair.Value!;
                var space = value.IndexOf(' ');
                if (space > 0)
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                        value.Substring(0, space),
                        value.Substring(space + 1));
                }
                else
                {
                    request.Headers.TryAddWithoutValidation("Authorization", value);
                }

                continue;
            }

            request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
        }
    }

    private static Dictionary<string, string> CopyHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        return headers;
    }
}

/// <summary>OpenAI-compatible embeddings model.</summary>
public sealed class OpenAICompatibleEmbeddingModel : IEmbeddingModel
{
    private readonly OpenAICompatibleProvider _provider;

    /// <summary>Creates an embedding model.</summary>
    public OpenAICompatibleEmbeddingModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        MaxEmbeddingsPerCall = provider.Options.MaxEmbeddingsPerCall;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => OpenAICompatibleChat.Qualify(_provider.Options.ProviderName, "embedding");

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Requested embedding width. Omitted from the body when unset.</summary>
    public int? Dimensions { get; set; }

    /// <summary>End-user identifier sent as <c>user</c>.</summary>
    public string? User { get; set; }

    /// <summary>Maximum inputs accepted by one request.</summary>
    public int MaxEmbeddingsPerCall { get; set; }

    /// <summary>When set, the embedding call uses this URL instead of the provider embeddings route.</summary>
    public Uri? Endpoint { get; set; }

    /// <summary>Provider options for the next call. <c>dimensions</c> and <c>user</c> are copied into the body.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Headers for the next call. They override the provider headers.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Warnings from the most recent call.</summary>
    public IReadOnlyList<CallWarning> LastWarnings { get; private set; } = Array.Empty<CallWarning>();

    /// <summary>HTTP response headers from the most recent call.</summary>
    public IReadOnlyDictionary<string, string> LastResponseHeaders { get; private set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, CancellationToken cancellationToken)
    {
        return DoEmbedAsync(values, null, null, cancellationToken);
    }

    /// <summary>Embeds values, sending <paramref name="dimensions"/> and <paramref name="user"/> when they are set.</summary>
    public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, int? dimensions, string? user, CancellationToken cancellationToken)
    {
        if (values == null)
        {
            values = Array.Empty<string>();
        }

        var limit = MaxEmbeddingsPerCall > 0 ? MaxEmbeddingsPerCall : _provider.Options.MaxEmbeddingsPerCall;
        if (limit > 0 && values.Count > limit)
        {
            throw new AiSdkException(
                "Too many embedding values for a single call to '" + Provider + "'. The maximum is " + limit.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".");
        }

        var input = new JsonArray();
        foreach (var value in values)
        {
            input.Add(value);
        }

        var warnings = new List<CallWarning>();
        var fromOptions = ReadEmbeddingOptions(warnings);
        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = input,
            ["encoding_format"] = "float",
        };
        var width = dimensions ?? fromOptions.Dimensions ?? Dimensions;
        if (width != null)
        {
            body["dimensions"] = width.Value;
        }

        var endUser = user ?? fromOptions.User ?? User;
        if (!string.IsNullOrEmpty(endUser))
        {
            body["user"] = endUser;
        }

        var response = await _provider.PostJsonAsync(
            Endpoint ?? _provider.EmbeddingsUri(ModelId),
            body.ToJsonString(),
            _provider.CreateHeaders(Headers),
            cancellationToken).ConfigureAwait(false);
        LastWarnings = warnings;
        LastResponseHeaders = response.Headers;
        using var parsed = System.Text.Json.JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var vectors = new List<float[]>();
        if (parsed.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                var embedding = item.GetProperty("embedding");
                var vector = new float[embedding.GetArrayLength()];
                var index = 0;
                foreach (var number in embedding.EnumerateArray())
                {
                    vector[index++] = number.GetSingle();
                }

                vectors.Add(vector);
            }
        }

        int? tokens = null;
        if (parsed.RootElement.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
        {
            if (usage.TryGetProperty("prompt_tokens", out var prompt) && prompt.ValueKind == JsonValueKind.Number && prompt.TryGetInt32(out var promptTokens))
            {
                tokens = promptTokens;
            }
            else if (usage.TryGetProperty("total_tokens", out var total) && total.ValueKind == JsonValueKind.Number && total.TryGetInt32(out var totalTokens))
            {
                tokens = totalTokens;
            }
        }

        return new EmbeddingResult(vectors, tokens);
    }

    private EmbeddingCallOptions ReadEmbeddingOptions(List<CallWarning> warnings)
    {
        var rawName = OpenAICompatibleChat.BaseProviderName(Provider);
        if (OpenAICompatibleChat.HasOptions(ProviderOptions, "openai-compatible"))
        {
            warnings.Add(new CallWarning(
                "deprecated",
                "providerOptions key 'openai-compatible'. Use 'openaiCompatible' instead."));
        }

        var camel = OpenAICompatibleChat.ToCamelCase(rawName);
        OpenAICompatibleChat.WarnIfDeprecated(rawName, ProviderOptions, warnings);
        int? width = null;
        string? endUser = null;
        ApplyEmbeddingOptions(ProviderOptions, "openai-compatible", ref width, ref endUser);
        ApplyEmbeddingOptions(ProviderOptions, "openaiCompatible", ref width, ref endUser);
        ApplyEmbeddingOptions(ProviderOptions, rawName, ref width, ref endUser);
        if (!string.Equals(camel, rawName, StringComparison.Ordinal))
        {
            ApplyEmbeddingOptions(ProviderOptions, camel, ref width, ref endUser);
        }

        return new EmbeddingCallOptions(width, endUser);
    }

    private static void ApplyEmbeddingOptions(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string key,
        ref int? dimensions,
        ref string? user)
    {
        if (!OpenAICompatibleChat.HasOptions(providerOptions, key))
        {
            return;
        }

        var bag = providerOptions![key];
        if (bag.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (bag.TryGetProperty("dimensions", out var width) && width.ValueKind == JsonValueKind.Number && width.TryGetInt32(out var parsed))
        {
            dimensions = parsed;
        }

        if (bag.TryGetProperty("user", out var endUser) && endUser.ValueKind == JsonValueKind.String)
        {
            user = endUser.GetString();
        }
    }

    private readonly struct EmbeddingCallOptions
    {
        public EmbeddingCallOptions(int? dimensions, string? user)
        {
            Dimensions = dimensions;
            User = user;
        }

        public int? Dimensions { get; }

        public string? User { get; }
    }
}

/// <summary>An image file sent to an edits request.</summary>
public sealed class OpenAICompatibleImageFile
{
    /// <summary>Creates an image file.</summary>
    public OpenAICompatibleImageFile(string mediaType, byte[]? data, string? url, string? fileName)
    {
        MediaType = mediaType ?? "application/octet-stream";
        Data = data;
        Url = url;
        FileName = fileName;
    }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }

    /// <summary>Inline bytes.</summary>
    public byte[]? Data { get; }

    /// <summary>Remote URL.</summary>
    public string? Url { get; }

    /// <summary>File name used in multipart uploads.</summary>
    public string? FileName { get; }
}

/// <summary>OpenAI-compatible image model.</summary>
public sealed class OpenAICompatibleImageModel : IImageModel
{
    private readonly OpenAICompatibleProvider _provider;

    /// <summary>Creates an image model.</summary>
    public OpenAICompatibleImageModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => OpenAICompatibleChat.Qualify(_provider.Options.ProviderName, "image");

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Seed. The shared images API does not accept it, so a set value becomes a warning.</summary>
    public int? Seed { get; set; }

    /// <summary>Provider-specific fields merged into the JSON body.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Headers for this call. They override the provider headers.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Input images. When set, the call uses <c>/images/edits</c>.</summary>
    public IReadOnlyList<OpenAICompatibleImageFile>? Files { get; set; }

    /// <summary>Optional edit mask.</summary>
    public OpenAICompatibleImageFile? Mask { get; set; }

    /// <summary>Warnings from the most recent call.</summary>
    public IReadOnlyList<CallWarning> LastWarnings { get; private set; } = Array.Empty<CallWarning>();

    /// <summary>Usage from the most recent call, when the response included it.</summary>
    public LanguageModelUsage? LastUsage { get; private set; }

    /// <summary>HTTP response headers from the most recent call.</summary>
    public IReadOnlyDictionary<string, string> LastResponseHeaders { get; private set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var warnings = new List<CallWarning>();
        if (!string.IsNullOrEmpty(options.AspectRatio))
        {
            warnings.Add(new CallWarning("unsupported", "This model does not support aspect ratio. Use `size` instead."));
        }

        if (Seed != null)
        {
            warnings.Add(new CallWarning("unsupported", "seed"));
        }

        var headers = _provider.CreateHeaders(Headers);
        ProviderTextResponse response;
        if (Files != null && Files.Count > 0)
        {
            response = await _provider.PostMultipartAsync(
                _provider.ImagesEditsUri(),
                BuildEditForm(options),
                headers,
                cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var body = BuildGenerationBody(options, warnings);
            response = await _provider.PostJsonAsync(
                _provider.ImagesUri(),
                body.ToJsonString(),
                headers,
                cancellationToken).ConfigureAwait(false);
        }

        LastWarnings = warnings;
        LastResponseHeaders = response.Headers;
        return ReadImages(response.Body);
    }

    private JsonObject BuildGenerationBody(ImageCallOptions options, List<CallWarning> warnings)
    {
        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["prompt"] = options.Prompt,
            ["n"] = options.Count,
        };
        if (!string.IsNullOrEmpty(options.Size))
        {
            body["size"] = options.Size;
        }

        OpenAICompatibleImages.MergeOptions(body, Provider, ProviderOptions, warnings);
        return body;
    }

    private MultipartFormDataContent BuildEditForm(ImageCallOptions options)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(ModelId), "model");
        form.Add(new StringContent(options.Prompt ?? string.Empty), "prompt");
        form.Add(new StringContent(options.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)), "n");
        if (!string.IsNullOrEmpty(options.Size))
        {
            form.Add(new StringContent(options.Size!), "size");
        }

        var index = 0;
        foreach (var file in Files!)
        {
            form.Add(OpenAICompatibleImages.FileContent(file), "image", file.FileName ?? ("image-" + index.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".png"));
            index++;
        }

        if (Mask != null)
        {
            form.Add(OpenAICompatibleImages.FileContent(Mask), "mask", Mask.FileName ?? "mask.png");
        }

        return form;
    }

    private ImageGenerationResult ReadImages(string? json)
    {
        using var document = System.Text.Json.JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json!);
        var root = document.RootElement;
        LastUsage = null;
        if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
        {
            LastUsage = new LanguageModelUsage(
                OpenAICompatibleImages.ReadInt(usage, "input_tokens"),
                OpenAICompatibleImages.ReadInt(usage, "output_tokens"),
                OpenAICompatibleImages.ReadInt(usage, "total_tokens"));
        }

        var images = new List<GeneratedImage>();
        if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                string? url = item.TryGetProperty("url", out var urlElement) && urlElement.ValueKind == JsonValueKind.String
                    ? urlElement.GetString()
                    : null;
                byte[]? bytes = null;
                if (item.TryGetProperty("b64_json", out var b64) && b64.ValueKind == JsonValueKind.String)
                {
                    bytes = Convert.FromBase64String(b64.GetString() ?? string.Empty);
                }

                images.Add(new GeneratedImage(bytes != null ? "image/png" : "image/*", bytes, url));
            }
        }

        return new ImageGenerationResult(images);
    }
}

/// <summary>Shared image request helpers.</summary>
public static class OpenAICompatibleImages
{
    /// <summary>Merges provider options into an image body. Camel-case keys win.</summary>
    public static void MergeOptions(JsonObject body, string providerId, IReadOnlyDictionary<string, JsonElement>? providerOptions, IList<CallWarning> warnings)
    {
        var raw = OpenAICompatibleChat.BaseProviderName(providerId);
        var camel = OpenAICompatibleChat.ToCamelCase(raw);
        if (!string.Equals(camel, raw, StringComparison.Ordinal) && OpenAICompatibleChat.HasOptions(providerOptions, raw))
        {
            warnings.Add(new CallWarning("deprecated", "providerOptions key '" + raw + "'. Use '" + camel + "' instead."));
        }

        Copy(body, providerOptions, raw);
        if (!string.Equals(camel, raw, StringComparison.Ordinal))
        {
            Copy(body, providerOptions, camel);
        }
    }

    /// <summary>Multipart content for one image file.</summary>
    public static ByteArrayContent FileContent(OpenAICompatibleImageFile file)
    {
        var bytes = file.Data ?? Array.Empty<byte>();
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(file.MediaType) ? "application/octet-stream" : file.MediaType);
        return content;
    }

    /// <summary>Reads an optional integer field.</summary>
    public static int? ReadInt(JsonElement obj, string name)
    {
        if (!obj.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number))
        {
            return null;
        }

        return number;
    }

    private static void Copy(JsonObject body, IReadOnlyDictionary<string, JsonElement>? providerOptions, string key)
    {
        if (!OpenAICompatibleChat.HasOptions(providerOptions, key))
        {
            return;
        }

        var bag = providerOptions![key];
        if (bag.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in bag.EnumerateObject())
        {
            body[property.Name] = JsonNode.Parse(property.Value.GetRawText());
        }
    }
}

/// <summary>Raw bytes returned by a provider.</summary>
public sealed class OpenAICompatibleBinaryResponse
{
    /// <summary>Creates a binary response.</summary>
    public OpenAICompatibleBinaryResponse(byte[] body, IReadOnlyDictionary<string, string> headers, int statusCode)
    {
        Body = body ?? Array.Empty<byte>();
        Headers = headers ?? new Dictionary<string, string>();
        StatusCode = statusCode;
    }

    /// <summary>Response bytes.</summary>
    public byte[] Body { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>HTTP status code.</summary>
    public int StatusCode { get; }
}

/// <summary>An open SSE response. The caller disposes it.</summary>
public sealed class OpenAICompatibleSseResponse : IDisposable
{
    private readonly HttpResponseMessage _response;

    /// <summary>Creates a response wrapper.</summary>
    public OpenAICompatibleSseResponse(HttpResponseMessage response)
    {
        _response = response ?? throw new ArgumentNullException(nameof(response));
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        Headers = headers;
    }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Reads SSE <c>data</c> payloads until <c>[DONE]</c>.</summary>
    public async IAsyncEnumerable<string> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stream = await _response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await foreach (var data in SseParser.ReadDataAsync(stream, cancellationToken).ConfigureAwait(false))
        {
            yield return data;
        }
    }

    /// <summary>Disposes the HTTP response.</summary>
    public void Dispose()
    {
        _response.Dispose();
    }
}
