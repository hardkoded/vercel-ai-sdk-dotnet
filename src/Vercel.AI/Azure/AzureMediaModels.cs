// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Azure;

/// <summary>Legacy completions model on the deployments URL.</summary>
public sealed class AzureCompletionLanguageModel : ILanguageModel
{
    private readonly AzureOpenAIProvider _provider;

    /// <summary>Creates a completions model.</summary>
    public AzureCompletionLanguageModel(AzureOpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => AzureOpenAIProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var prompt = new System.Text.StringBuilder();
        foreach (var message in options.Prompt)
        {
            if (message is UserModelMessage user)
            {
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart textPart)
                    {
                        prompt.Append(textPart.Text);
                    }
                }
            }
        }

        var body = new JsonObject { ["model"] = ModelId, ["prompt"] = prompt.ToString() };
        var response = await _provider.PostJsonAsync(
            _provider.DeploymentUri(ModelId, "completions"),
            body.ToJsonString(),
            options.Headers,
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var text = string.Empty;
        string? finish = null;
        if (document.RootElement.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
        {
            text = AzureJson.String(choices[0], "text") ?? string.Empty;
            finish = AzureJson.String(choices[0], "finish_reason");
        }

        var content = string.IsNullOrEmpty(text)
            ? (IReadOnlyList<GeneratedContent>)Array.Empty<GeneratedContent>()
            : new GeneratedContent[] { new GeneratedText(text) };
        return new LanguageModelGenerateResult(content, FinishReasons.Parse(finish), LanguageModelUsage.Empty, finish, responseHeaders: response.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(result.Text))
        {
            yield return new TextDeltaStreamPart("text", result.Text);
        }

        yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason);
    }
}

/// <summary>Azure embeddings model.</summary>
public sealed class AzureEmbeddingModel : IEmbeddingModel
{
    private readonly AzureOpenAIProvider _provider;

    /// <summary>Creates an embedding model.</summary>
    public AzureEmbeddingModel(AzureOpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => AzureOpenAIProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Aggregate input budget exposed to callers that batch embeddings.</summary>
    public int MaxInputBytesPerCall => 300000;

    /// <inheritdoc />
    public async Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, CancellationToken cancellationToken)
    {
        var input = new JsonArray();
        foreach (var value in values)
        {
            input.Add(value);
        }

        var body = new JsonObject { ["model"] = ModelId, ["input"] = input };
        var response = await _provider.PostJsonAsync(
            _provider.DeploymentUri(ModelId, "embeddings"),
            body.ToJsonString(),
            null,
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var vectors = new List<float[]>();
        if (document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                if (!item.TryGetProperty("embedding", out var embedding) || embedding.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

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
        if (document.RootElement.TryGetProperty("usage", out var usage) && usage.TryGetProperty("total_tokens", out var total) && total.ValueKind == JsonValueKind.Number)
        {
            tokens = total.GetInt32();
        }

        return new EmbeddingResult(vectors, tokens);
    }

    /// <summary>Embeds values and forwards caller headers.</summary>
    public Task<EmbeddingResult> EmbedAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
    {
        return EmbedCoreAsync(values, headers, cancellationToken);
    }

    private async Task<EmbeddingResult> EmbedCoreAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
    {
        var input = new JsonArray();
        foreach (var value in values)
        {
            input.Add(value);
        }

        var body = new JsonObject { ["model"] = ModelId, ["input"] = input };
        var response = await _provider.PostJsonAsync(
            _provider.DeploymentUri(ModelId, "embeddings"),
            body.ToJsonString(),
            headers,
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var vectors = new List<float[]>();
        if (document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                if (!item.TryGetProperty("embedding", out var embedding) || embedding.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var vector = new float[embedding.GetArrayLength()];
                var index = 0;
                foreach (var number in embedding.EnumerateArray())
                {
                    vector[index++] = number.GetSingle();
                }

                vectors.Add(vector);
            }
        }

        return new EmbeddingResult(vectors, null);
    }
}

/// <summary>Azure image model. <see cref="AzureOpenAIProvider.Image"/> and <see cref="AzureOpenAIProvider.ImageModel"/> return this type.</summary>
public sealed class AzureImageModel : IImageModel
{
    private readonly AzureOpenAIProvider _provider;

    /// <summary>Creates an image model.</summary>
    public AzureImageModel(AzureOpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => AzureOpenAIProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        var result = await GenerateAsync(new AzureImageCallOptions(options.Prompt) { Count = options.Count, Size = options.Size }, cancellationToken).ConfigureAwait(false);
        var images = new List<GeneratedImage>();
        foreach (var item in result.Base64Images)
        {
            images.Add(new GeneratedImage("image/png", Decode(item), null));
        }

        return new ImageGenerationResult(images);
    }

    /// <summary>Generates images and returns the <c>b64_json</c> strings.</summary>
    public async Task<AzureImageResult> GenerateAsync(AzureImageCallOptions options, CancellationToken cancellationToken)
    {
        options ??= new AzureImageCallOptions(string.Empty);
        var style = options.Style;
        if (string.IsNullOrEmpty(style) && AzureJson.ProviderOptions(options.ProviderOptions, "openai", out var openai))
        {
            style = AzureJson.String(openai, "style");
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["prompt"] = options.Prompt,
            ["n"] = options.Count,
            ["response_format"] = "b64_json",
        };
        AzureJson.Set(body, "size", options.Size);
        AzureJson.Set(body, "style", style);
        var response = await _provider.PostJsonAsync(
            _provider.DeploymentUri(ModelId, "images/generations"),
            body.ToJsonString(),
            options.Headers,
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var images = new List<string>();
        if (document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                var b64 = AzureJson.String(item, "b64_json");
                if (b64 != null)
                {
                    images.Add(b64);
                }
            }
        }

        return new AzureImageResult(images);
    }

    private static byte[] Decode(string value)
    {
        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            return System.Text.Encoding.UTF8.GetBytes(value);
        }
    }
}

/// <summary>Azure speech model on the deployments URL.</summary>
public sealed class AzureSpeechModel : ISpeechModel
{
    private readonly AzureOpenAIProvider _provider;

    /// <summary>Creates a speech model.</summary>
    public AzureSpeechModel(AzureOpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => AzureOpenAIProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
    {
        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = options.Text,
            ["voice"] = options.Voice ?? "alloy",
        };
        var response = await _provider.PostJsonAsync(
            _provider.DeploymentUri(ModelId, "audio/speech"),
            body.ToJsonString(),
            null,
            cancellationToken).ConfigureAwait(false);
        return new SpeechResult(System.Text.Encoding.UTF8.GetBytes(response.Body), "audio/mpeg");
    }
}

/// <summary>Azure transcription model on the deployments URL.</summary>
public sealed class AzureTranscriptionModel : ITranscriptionModel
{
    private readonly AzureOpenAIProvider _provider;

    /// <summary>Creates a transcription model.</summary>
    public AzureTranscriptionModel(AzureOpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => AzureOpenAIProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(audio.Data ?? Array.Empty<byte>());
        file.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(audio.MediaType) ? "application/octet-stream" : audio.MediaType);
        content.Add(file, "file", string.IsNullOrEmpty(audio.FileName) ? "audio.wav" : audio.FileName);
        content.Add(new StringContent(ModelId), "model");
        var headers = await _provider.CreateHeadersAsync(null, cancellationToken).ConfigureAwait(false);
        var bytes = await _provider.Http.SendBytesAsync(
            HttpMethod.Post,
            _provider.DeploymentUri(ModelId, "audio/transcriptions"),
            content,
            headers,
            cancellationToken).ConfigureAwait(false);
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        try
        {
            using var document = JsonDocument.Parse(text);
            return new TranscriptionResult(AzureJson.String(document.RootElement, "text") ?? string.Empty);
        }
        catch (JsonException)
        {
            return new TranscriptionResult(text);
        }
    }
}
