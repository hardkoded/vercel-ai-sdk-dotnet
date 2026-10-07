// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Prodia;

/// <summary>
/// Prodia image-to-image job exposed as a language model. The last user message supplies the text and the
/// optional input image. The result holds the returned text and images.
/// </summary>
public sealed class ProdiaLanguageModel : ILanguageModel
{
    private static readonly string[] AspectRatios = { "1:1", "2:3", "3:2", "4:5", "5:4", "4:7", "7:4", "9:16", "16:9", "9:21", "21:9" };

    private readonly ProdiaProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp.</summary>
    public ProdiaLanguageModel(ProdiaProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => "prodia.language";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var warnings = Warnings(options);
        var config = new JsonObject { ["prompt"] = Prompt(options.Prompt), ["include_messages"] = true };
        if (options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue("prodia", out var prodia)
            && prodia.ValueKind == JsonValueKind.Object
            && prodia.TryGetProperty("aspectRatio", out var aspectRatio))
        {
            if (aspectRatio.ValueKind != JsonValueKind.String || Array.IndexOf(AspectRatios, aspectRatio.GetString()) < 0)
            {
                throw new InvalidArgumentException("providerOptions", null, "Invalid Prodia provider option aspectRatio.");
            }

            config["aspect_ratio"] = aspectRatio.GetString();
        }

        var (image, imageMediaType) = await InputImageAsync(options.Prompt, cancellationToken).ConfigureAwait(false);
        var extension = imageMediaType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/webp" => ".webp",
            _ => string.Empty,
        };
        var timestamp = _clock();
        var (parts, headers) = await ProdiaApi.PostJobAsync(
            _provider,
            ProdiaApi.Form(ProdiaApi.Job(ModelId, config), image, imageMediaType, extension),
            "multipart/form-data",
            options.Headers?.Where(pair => pair.Value != null).Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value!)),
            cancellationToken).ConfigureAwait(false);
        var job = ProdiaApi.ReadJob(parts);
        string? text = null;
        var files = new List<GeneratedContent>();
        foreach (var part in parts)
        {
            if (!part.ContentDisposition.Contains("name=\"output\""))
            {
                continue;
            }

            if (part.ContentType.StartsWith("text/", StringComparison.Ordinal) || part.ContentDisposition.Contains(".txt"))
            {
                text = Encoding.UTF8.GetString(part.Body);
            }
            else if (part.ContentType.StartsWith("image/", StringComparison.Ordinal))
            {
                files.Add(new GeneratedFile(part.Body, part.ContentType));
            }
        }

        var content = new List<GeneratedContent>();
        if (text != null)
        {
            content.Add(new GeneratedText(text));
        }

        content.AddRange(files);
        return new LanguageModelGenerateResult(
            content,
            FinishReason.Stop,
            new LanguageModelUsage(null, null, null),
            warnings: warnings,
            providerMetadata: OperationJson.Parse(new JsonObject { ["prodia"] = ProdiaApi.Metadata(job) }.ToJsonString()),
            responseModelId: ModelId,
            responseTimestamp: timestamp,
            responseHeaders: headers);
    }

    /// <summary>Runs <see cref="DoGenerateAsync"/> and replays the result as stream parts. Generated images are not streamed.</summary>
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        yield return new StreamStartStreamPart(result.Warnings);
        yield return new ResponseMetadataStreamPart(null, result.ResponseModelId, result.ResponseTimestamp);
        foreach (var part in result.Content.OfType<GeneratedText>())
        {
            var id = IdGenerator.Generate();
            yield return new TextStartStreamPart(id);
            yield return new TextDeltaStreamPart(id, part.Text);
            yield return new TextEndStreamPart(id);
        }

        yield return new FinishStreamPart(result.FinishReason, result.Usage, providerMetadata: result.ProviderMetadata);
    }

    private static List<CallWarning> Warnings(LanguageModelCallOptions options)
    {
        var unsupported = new (string Feature, bool Present)[]
        {
            ("temperature", options.Temperature != null),
            ("topP", options.TopP != null),
            ("topK", options.TopK != null),
            ("seed", options.Seed != null),
            ("maxOutputTokens", options.MaxOutputTokens != null),
            ("stopSequences", options.StopSequences != null),
            ("presencePenalty", options.PresencePenalty != null),
            ("frequencyPenalty", options.FrequencyPenalty != null),
            ("tools", options.Tools is { Count: > 0 }),
            ("toolChoice", options.ToolChoice != null),
            ("responseFormat", options.JsonSchema != null),
            ("reasoning", ReasoningMap.IsCustomReasoning(options.Reasoning)),
        };
        return unsupported.Where(item => item.Present).Select(item => new CallWarning("unsupported", item.Feature)).ToList();
    }

    /// <summary>The last user message's text, after the system message when there is one.</summary>
    private static string Prompt(IReadOnlyList<ModelMessage> messages)
    {
        var system = messages.OfType<SystemModelMessage>().LastOrDefault()?.Content ?? string.Empty;
        var user = messages.OfType<UserModelMessage>().LastOrDefault();
        var prompt = user == null ? string.Empty : string.Join("\n", user.Content.OfType<TextContentPart>().Select(part => part.Text));
        return system.Length > 0 ? system + "\n" + prompt : prompt;
    }

    /// <summary>The first image in the last user message, with a full media type.</summary>
    private async Task<(byte[]? Data, string MediaType)> InputImageAsync(IReadOnlyList<ModelMessage> messages, CancellationToken cancellationToken)
    {
        var file = messages.OfType<UserModelMessage>().LastOrDefault()?.Content
            .OfType<FileContentPart>()
            .FirstOrDefault(part => MediaTypes.GetTopLevelMediaType(part.MediaType) == "image");
        if (file == null)
        {
            return (null, "image/png");
        }

        var data = file.Data;
        if (data == null)
        {
            Download.ValidateDownloadUrl(file.Url!);
            var response = await ProviderExchange.SendAsync(_provider.HttpClient, HttpMethod.Get, new Uri(file.Url!), null, null, cancellationToken).ConfigureAwait(false);
            data = response.Bytes;
        }

        var mediaType = MediaTypes.IsFullMediaType(file.MediaType)
            ? file.MediaType
            : MediaTypes.DetectMediaType(data, MediaTypes.GetTopLevelMediaType(file.MediaType)) ?? "image/png";
        return (data, mediaType);
    }
}
