// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Gemini settings.</summary>
public class GoogleOptions
{
    /// <summary>API origin.</summary>
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";

    /// <summary>Explicit key.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Environment variable.</summary>
    public string ApiKeyEnvironmentVariable { get; set; } = "GOOGLE_GENERATIVE_AI_API_KEY";

    /// <summary>When set, requests use <c>Authorization: Bearer</c> instead of <c>x-goog-api-key</c>.</summary>
    public bool UseBearerToken { get; set; }

    /// <summary>Provider id. Defaults to <c>google</c>. Vertex uses <c>google.vertex.chat</c>.</summary>
    public string? Name { get; set; }

    /// <summary>Extra request headers merged after the API key header.</summary>
    public Dictionary<string, string?> Headers { get; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Id factory for tool calls and grounding sources. Defaults to a random id.</summary>
    public Func<string>? GenerateId { get; set; }
}

/// <summary>Google Gemini provider.</summary>
public class GoogleProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderName = "google";

    /// <summary>Creates a provider.</summary>
    public GoogleProvider(HttpClient httpClient, GoogleOptions? options = null)
        : this(httpClient, options, null)
    {
    }

    /// <summary>Creates a provider with an explicit provider id.</summary>
    public GoogleProvider(HttpClient httpClient, GoogleOptions? options, string? providerName)
        : base(ResolveName(options, providerName))
    {
        Options = options ?? new GoogleOptions();
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
    }

    private static string ResolveName(GoogleOptions? options, string? providerName)
    {
        if (!string.IsNullOrEmpty(providerName))
        {
            return providerName!;
        }

        if (!string.IsNullOrEmpty(options?.Name))
        {
            return options!.Name!;
        }

        return ProviderName;
    }

    /// <summary>Options.</summary>
    public GoogleOptions Options { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Creates a provider.</summary>
    public static GoogleProvider Create(GoogleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new GoogleProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new GoogleLanguageModel(this, modelId);
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        return new GoogleEmbeddingModel(this, modelId);
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return new GoogleTranscriptionModel(this, modelId);
    }

    internal string NextId()
    {
        if (Options.GenerateId != null)
        {
            return Options.GenerateId();
        }

        return Guid.NewGuid().ToString("N");
    }

    internal Dictionary<string, string?> Headers()
    {
        var key = ApiKeys.Require(Options.ApiKey, Options.ApiKeyEnvironmentVariable);
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (Options.UseBearerToken)
        {
            headers["Authorization"] = "Bearer " + key;
        }
        else
        {
            headers["x-goog-api-key"] = key;
        }

        foreach (var pair in Options.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        return headers;
    }
}

/// <summary>Gemini language model.</summary>
public sealed class GoogleLanguageModel : ILanguageModel
{
    private readonly GoogleProvider _provider;

    /// <summary>Creates a model.</summary>
    public GoogleLanguageModel(GoogleProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Name;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var prepared = GoogleGenerate.Prepare(_provider, ModelId, options, streaming: false);
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, GoogleModelPath.Get(ModelId) + ":generateContent"),
            prepared.Body.ToJsonString(),
            prepared.Headers,
            cancellationToken).ConfigureAwait(false);
        return GoogleGenerate.Read(document.RootElement, prepared, _provider.NextId);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var prepared = GoogleGenerate.Prepare(_provider, ModelId, options, streaming: true);
        var session = new GoogleStreamSession(prepared, _provider.NextId);
        yield return session.Start();
        await foreach (var data in _provider.Http.SendSseAsync(
            ApiKeys.Combine(_provider.Options.BaseUrl, GoogleModelPath.Get(ModelId) + ":streamGenerateContent?alt=sse"),
            prepared.Body.ToJsonString(),
            prepared.Headers,
            cancellationToken).ConfigureAwait(false))
        {
            foreach (var part in session.Accept(data, options.IncludeRawChunks))
            {
                yield return part;
            }
        }

        foreach (var part in session.Finish())
        {
            yield return part;
        }
    }
}

/// <summary>Gemini embedding model. Posts to <c>:embedContent</c>.</summary>
public sealed class GoogleEmbeddingModel : IEmbeddingModel
{
    private readonly GoogleProvider _provider;

    /// <summary>Creates an embedding model.</summary>
    public GoogleEmbeddingModel(GoogleProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Name;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, CancellationToken cancellationToken)
    {
        var vectors = new List<float[]>();
        foreach (var value in values)
        {
            var body = new JsonObject
            {
                ["content"] = new JsonObject { ["parts"] = new JsonArray { new JsonObject { ["text"] = value } } },
            };
            using var document = await _provider.Http.SendJsonAsync(
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, "models/" + ModelId + ":embedContent"),
                body.ToJsonString(),
                _provider.Headers(),
                cancellationToken).ConfigureAwait(false);
            var valuesElement = document.RootElement.GetProperty("embedding").GetProperty("values");
            var vector = new float[valuesElement.GetArrayLength()];
            var index = 0;
            foreach (var number in valuesElement.EnumerateArray())
            {
                vector[index++] = number.GetSingle();
            }

            vectors.Add(vector);
        }

        return new EmbeddingResult(vectors, null);
    }
}

/// <summary>
/// Gemini on Vertex AI. Set <see cref="VertexOptions.Project"/> and <see cref="VertexOptions.Region"/>.
/// The key is a bearer access token from <c>GOOGLE_VERTEX_API_KEY</c>.
/// </summary>
public sealed class VertexOptions : GoogleOptions
{
    /// <summary>
    /// Creates Vertex options. <see cref="GoogleOptions.BaseUrl"/> starts empty.
    /// An empty base URL is replaced with the regional publisher host from <see cref="Region"/>. A caller-supplied base URL is kept as-is.
    /// </summary>
    public VertexOptions()
    {
        UseBearerToken = true;
        ApiKeyEnvironmentVariable = "GOOGLE_VERTEX_API_KEY";
        BaseUrl = string.Empty;
    }

    /// <summary>GCP project id.</summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>Vertex location. One DNS label, used only when <see cref="GoogleOptions.BaseUrl"/> is empty.</summary>
    public string Region { get; set; } = "us-central1";
}

/// <summary>Google Vertex AI provider.</summary>
public sealed class GoogleVertexProvider : GoogleProvider
{
    /// <summary>Creates a Vertex provider and points the base URL at the project publisher route.</summary>
    public GoogleVertexProvider(HttpClient httpClient, VertexOptions? options = null)
        : base(httpClient, Prepare(options), "google.vertex.chat")
    {
    }

    /// <summary>Creates a provider.</summary>
    public static GoogleVertexProvider Create(VertexOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new GoogleVertexProvider(client, options);
    }

    private static VertexOptions Prepare(VertexOptions? options)
    {
        options ??= new VertexOptions();
        options.UseBearerToken = true;
        if (string.IsNullOrEmpty(options.Project))
        {
            options.Project = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_PROJECT") ?? string.Empty;
        }

        if (!string.IsNullOrEmpty(options.BaseUrl))
        {
            return options;
        }

        if (!HostnameParts.IsValidHostnamePart(options.Region))
        {
            throw new ArgumentException("A Vertex location must be a single DNS label.", nameof(VertexOptions.Region));
        }

        options.BaseUrl = "https://" + options.Region + "-aiplatform.googleapis.com/v1/projects/" + options.Project
            + "/locations/" + options.Region + "/publishers/google";
        return options;
    }
}

/// <summary>Registers Google and Vertex.</summary>
public static class GoogleServiceCollectionExtensions
{
    /// <summary>Adds <see cref="GoogleProvider"/>.</summary>
    public static IServiceCollection AddGoogle(this IServiceCollection services, Action<GoogleOptions>? configure = null)
    {
        services.AddHttpClient(GoogleProvider.ProviderName);
        services.AddSingleton(sp =>
        {
            var options = new GoogleOptions();
            configure?.Invoke(options);
            return new GoogleProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(GoogleProvider.ProviderName), options);
        });
        return services;
    }

    /// <summary>Adds <see cref="GoogleVertexProvider"/>.</summary>
    public static IServiceCollection AddGoogleVertex(this IServiceCollection services, Action<VertexOptions>? configure = null)
    {
        services.AddHttpClient("google-vertex");
        services.AddSingleton(sp =>
        {
            var options = new VertexOptions();
            configure?.Invoke(options);
            return new GoogleVertexProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient("google-vertex"), options);
        });
        return services;
    }
}
