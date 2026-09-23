// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.ProviderUtils;

namespace Vercel.AI.Sdk.Google;

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
}

/// <summary>Google Gemini provider.</summary>
public class GoogleProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderName = "google";

    /// <summary>Creates a provider.</summary>
    public GoogleProvider(HttpClient httpClient, GoogleOptions? options = null)
        : base(ProviderName)
    {
        Options = options ?? new GoogleOptions();
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
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

    internal Dictionary<string, string?> Headers()
    {
        var key = ApiKeys.Require(Options.ApiKey, Options.ApiKeyEnvironmentVariable);
        if (Options.UseBearerToken)
        {
            return new Dictionary<string, string?> { ["Authorization"] = "Bearer " + key };
        }

        return new Dictionary<string, string?> { ["x-goog-api-key"] = key };
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
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "models/" + ModelId + ":generateContent"),
            Build(options).ToJsonString(),
            _provider.Headers(),
            cancellationToken).ConfigureAwait(false);
        return Parse(document.RootElement);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var data in _provider.Http.SendSseAsync(
            ApiKeys.Combine(_provider.Options.BaseUrl, "models/" + ModelId + ":streamGenerateContent?alt=sse"),
            Build(options).ToJsonString(),
            _provider.Headers(),
            cancellationToken).ConfigureAwait(false))
        {
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(data);
            }
            catch (JsonException)
            {
                continue;
            }

            using (document)
            {
                var result = Parse(document.RootElement);
                if (!string.IsNullOrEmpty(result.Text))
                {
                    yield return new TextDeltaStreamPart("text", result.Text);
                }

                foreach (var part in result.Content)
                {
                    if (part is GeneratedToolCall call)
                    {
                        yield return new ToolCallStreamPart(call.ToolCallId, call.ToolName, call.ArgumentsJson);
                    }
                }

                yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason);
            }
        }
    }

    private JsonObject Build(LanguageModelCallOptions options)
    {
        var contents = new JsonArray();
        JsonArray? system = null;
        foreach (var message in options.Prompt)
        {
            if (message is SystemModelMessage systemMessage)
            {
                system ??= new JsonArray();
                system.Add(new JsonObject { ["text"] = systemMessage.Content });
            }
            else if (message is UserModelMessage user)
            {
                var parts = new JsonArray();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart text)
                    {
                        parts.Add(new JsonObject { ["text"] = text.Text });
                    }
                }

                contents.Add(new JsonObject { ["role"] = "user", ["parts"] = parts });
            }
            else if (message is AssistantModelMessage assistant)
            {
                var parts = new JsonArray();
                if (!string.IsNullOrEmpty(assistant.Text))
                {
                    parts.Add(new JsonObject { ["text"] = assistant.Text });
                }

                foreach (var call in assistant.ToolCalls)
                {
                    parts.Add(new JsonObject
                    {
                        ["functionCall"] = new JsonObject
                        {
                            ["name"] = call.ToolName,
                            ["args"] = JsonNode.Parse(string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson),
                        },
                    });
                }

                contents.Add(new JsonObject { ["role"] = "model", ["parts"] = parts });
            }
            else if (message is ToolModelMessage tool)
            {
                contents.Add(new JsonObject
                {
                    ["role"] = "user",
                    ["parts"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["functionResponse"] = new JsonObject
                            {
                                ["name"] = tool.ToolName,
                                ["response"] = new JsonObject { ["result"] = tool.OutputJson },
                            },
                        },
                    },
                });
            }
        }

        var body = new JsonObject { ["contents"] = contents };
        if (system != null)
        {
            body["systemInstruction"] = new JsonObject { ["parts"] = system };
        }

        var config = new JsonObject();
        if (options.Temperature is { } temperature)
        {
            config["temperature"] = temperature;
        }

        if (options.MaxOutputTokens is { } max)
        {
            config["maxOutputTokens"] = max;
        }

        if (options.TopP is { } topP)
        {
            config["topP"] = topP;
        }

        if (options.TopK is { } topK)
        {
            config["topK"] = topK;
        }

        if (options.JsonSchema is { } schema)
        {
            config["responseMimeType"] = "application/json";
            config["responseSchema"] = JsonNode.Parse(schema.GetRawText());
        }

        if (config.Count > 0)
        {
            body["generationConfig"] = config;
        }

        if (options.Tools is { Count: > 0 })
        {
            var declarations = new JsonArray();
            foreach (var tool in options.Tools)
            {
                declarations.Add(new JsonObject
                {
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText()),
                });
            }

            body["tools"] = new JsonArray { new JsonObject { ["functionDeclarations"] = declarations } };
        }

        return body;
    }

    private static LanguageModelGenerateResult Parse(JsonElement root)
    {
        var content = new List<GeneratedContent>();
        var raw = "STOP";
        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var candidate = candidates[0];
            raw = candidate.TryGetProperty("finishReason", out var finish) ? finish.GetString() ?? raw : raw;
            if (candidate.TryGetProperty("content", out var candidateContent) && candidateContent.TryGetProperty("parts", out var parts))
            {
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text))
                    {
                        content.Add(new GeneratedText(text.GetString() ?? string.Empty));
                    }

                    if (part.TryGetProperty("functionCall", out var call))
                    {
                        content.Add(new GeneratedToolCall(
                            "call_" + (call.GetProperty("name").GetString() ?? "tool"),
                            call.GetProperty("name").GetString() ?? string.Empty,
                            call.TryGetProperty("args", out var args) ? args.GetRawText() : "{}"));
                    }
                }
            }
        }

        var usage = LanguageModelUsage.Empty;
        if (root.TryGetProperty("usageMetadata", out var usageElement))
        {
            usage = new LanguageModelUsage(
                usageElement.TryGetProperty("promptTokenCount", out var input) ? input.GetInt32() : null,
                usageElement.TryGetProperty("candidatesTokenCount", out var output) ? output.GetInt32() : null,
                usageElement.TryGetProperty("totalTokenCount", out var total) ? total.GetInt32() : null);
        }

        return new LanguageModelGenerateResult(content, FinishReasons.Parse(raw), usage, raw);
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
    /// <summary>Creates Vertex options.</summary>
    public VertexOptions()
    {
        UseBearerToken = true;
        ApiKeyEnvironmentVariable = "GOOGLE_VERTEX_API_KEY";
    }

    /// <summary>GCP project id.</summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>GCP region.</summary>
    public string Region { get; set; } = "us-central1";
}

/// <summary>Google Vertex AI provider.</summary>
public sealed class GoogleVertexProvider : GoogleProvider
{
    /// <summary>Creates a Vertex provider and points the base URL at the project publisher route.</summary>
    public GoogleVertexProvider(HttpClient httpClient, VertexOptions? options = null)
        : base(httpClient, Prepare(options))
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
