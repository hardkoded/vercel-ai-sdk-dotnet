// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using ProviderGeneratedImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI.QuiverAI;

/// <summary>
/// QuiverAI SVG model. <c>providerOptions.quiverai.operation</c> selects <c>generate</c> (default),
/// <c>vectorize</c>, <c>animate</c>, or <c>edit</c>. Invalid requests throw before any HTTP call.
/// </summary>
public sealed class QuiverAIImageModel : IImageModel, IImageCaller
{
    private const int MaxAnimationSourceBase64Length = 1_066_668;
    private const int MaxEditSvgBytes = 200_000;

    private static readonly Regex AnimationSvgStart = new Regex("^(?:(?:<\\?xml[\\s\\S]*?\\?>|<!--[\\s\\S]*?-->|<!DOCTYPE[\\s\\S]*?>)\\s*)*<svg(?:\\s|>)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex DecimalReference = new Regex("^#\\d+$", RegexOptions.CultureInvariant);
    private static readonly Regex HexReference = new Regex("^#x[\\dA-Fa-f]+$", RegexOptions.CultureInvariant);
    private static readonly string[] ReasoningEfforts = { "low", "medium", "high", "xhigh" };

    private readonly QuiverAIProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the timestamp when the response has none.</summary>
    public QuiverAIImageModel(QuiverAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string Provider => "quiverai.image";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>At most 16 images per call.</summary>
    public int? MaxImagesPerCall => 16;

    /// <inheritdoc />
    public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxImagesPerCall);
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var call = new ImageModelCall(options.Prompt, null, null, options.Count, options.Size, options.AspectRatio, null, null, new Dictionary<string, string>(), cancellationToken);
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        return new ImageGenerationResult(result.Images.Select(image => new ProviderGeneratedImage("image/svg+xml", (byte[])image!, null)).ToArray());
    }

    /// <inheritdoc />
    public async Task<ImageModelResult> DoGenerateAsync(ImageModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var options = ParseOptions(call.ProviderOptions);
        var warnings = CollectWarnings(call);
        var body = BuildRequestBody(call, options);
        var timestamp = _clock();
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, OperationPath(options.Operation)),
            body.ToJsonString(),
            _provider.CreateHeaders(headers),
            cancellationToken).ConfigureAwait(false);
        return ParseResponse(response, timestamp, warnings);
    }

    private ImageModelResult ParseResponse(ProviderTextResponse response, DateTimeOffset timestamp, IReadOnlyList<OperationWarning> warnings)
    {
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array || data.GetArrayLength() == 0)
        {
            throw new InvalidResponseDataException(response.Body, "QuiverAI response did not contain any SVG documents.");
        }

        var images = new List<object?>();
        var imageMetadata = new JsonArray();
        var index = 0;
        foreach (var item in data.EnumerateArray())
        {
            var svg = item.TryGetProperty("svg", out var svgElement) && svgElement.ValueKind == JsonValueKind.String ? svgElement.GetString() : null;
            if (string.IsNullOrEmpty(svg))
            {
                throw new InvalidResponseDataException(response.Body, "QuiverAI response contained an SVG document without markup.");
            }

            images.Add(Encoding.UTF8.GetBytes(svg));
            var metadata = new JsonObject { ["index"] = index++, ["mimeType"] = item.TryGetProperty("mime_type", out var mimeType) ? mimeType.GetString() : null };
            if (item.TryGetProperty("loop_period_ms", out var loop))
            {
                metadata["loopPeriodMs"] = JsonNode.Parse(loop.GetRawText());
            }

            if (item.TryGetProperty("opening_animation_ms", out var opening))
            {
                metadata["openingAnimationMs"] = JsonNode.Parse(opening.GetRawText());
            }

            imageMetadata.Add(metadata);
        }

        var quiverai = new JsonObject();
        if (root.TryGetProperty("credits", out var credits) && credits.ValueKind == JsonValueKind.Number)
        {
            quiverai["credits"] = credits.GetInt32();
        }

        quiverai["images"] = imageMetadata;
        OperationUsage? usage = null;
        if (root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object)
        {
            usage = new OperationUsage(
                usageElement.GetProperty("input_tokens").GetInt32(),
                usageElement.GetProperty("output_tokens").GetInt32(),
                usageElement.GetProperty("total_tokens").GetInt32());
        }

        var created = root.TryGetProperty("created", out var createdElement) && createdElement.ValueKind == JsonValueKind.Number ? createdElement.GetInt64() : 0;
        var responseTimestamp = created != 0 ? DateTimeOffset.FromUnixTimeSeconds(created) : timestamp;
        var providerMetadata = OperationJson.Parse(new JsonObject { ["quiverai"] = quiverai }.ToJsonString());
        return new ImageModelResult(
            images,
            warnings,
            usage,
            providerMetadata,
            new ProviderResponse(response.Headers, timestamp: responseTimestamp.UtcDateTime, modelId: ModelId));
    }

    private static string OperationPath(string operation)
    {
        return operation switch
        {
            "vectorize" => "/svgs/vectorizations",
            "edit" => "/svgs/edits",
            "animate" => "/svgs/animations",
            _ => "/svgs/generations",
        };
    }

    private static List<OperationWarning> CollectWarnings(ImageModelCall call)
    {
        var warnings = new List<OperationWarning>();
        if (call.Size != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "size", details: "QuiverAI SVG generation does not support the `size` option. The setting was ignored."));
        }

        if (call.AspectRatio != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "aspectRatio", details: "QuiverAI SVG generation does not support the `aspectRatio` option. The setting was ignored."));
        }

        if (call.Seed != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "seed", details: "QuiverAI SVG generation does not support the `seed` option. The setting was ignored."));
        }

        if (call.Mask != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "mask", details: "QuiverAI SVG generation does not support masks. The mask was ignored."));
        }

        return warnings;
    }

    private JsonObject BuildRequestBody(ImageModelCall call, QuiverAIOptions options)
    {
        if ((ModelId == "arrow-2" || ModelId == "arrow-2-telos") && options.MaxOutputTokens > 65536)
        {
            throw new InvalidArgumentException("maxOutputTokens", options.MaxOutputTokens, "QuiverAI model \"" + ModelId + "\" supports at most 65536 output tokens.");
        }

        if (options.Operation != "edit")
        {
            RejectEditOnlyOptions(options);
        }

        switch (options.Operation)
        {
            case "generate":
                return BuildGenerateBody(call, options);
            case "edit":
                return BuildEditBody(call, options);
            case "animate":
                return BuildAnimationBody(call, options);
        }

        var files = call.Files;
        if (files == null || files.Count == 0)
        {
            throw new InvalidArgumentException("files", null, "QuiverAI vectorize requires an input image. Pass an image in the generateImage prompt and set providerOptions.quiverai.operation to \"vectorize\".");
        }

        if (files.Count > 1)
        {
            throw new InvalidArgumentException("files", files.Count, "QuiverAI vectorize accepts a single input image.");
        }

        if (call.N != 1)
        {
            throw new InvalidArgumentException("n", call.N, "QuiverAI vectorize returns one SVG per request. Set maxImagesPerCall to 1 in generateImage to vectorize multiple times.");
        }

        var body = new JsonObject { ["model"] = ModelId, ["image"] = Reference(files[0]) };
        AddSharedOptions(body, options);
        Add(body, "auto_crop", options.AutoCrop);
        Add(body, "target_size", options.TargetSize);
        return body;
    }

    private JsonObject BuildGenerateBody(ImageModelCall call, QuiverAIOptions options)
    {
        if (string.IsNullOrWhiteSpace(call.Prompt))
        {
            throw new InvalidArgumentException("prompt", call.Prompt, "QuiverAI image generation requires a non-empty prompt for generateImage.");
        }

        // Only documented Arrow 1.x models get the lower limit. The API enforces model-specific limits within its 16-reference request limit.
        var maxReferences = ModelId is "arrow-1" or "arrow-1.0" or "arrow-1.1" ? 4 : 16;
        if (call.Files != null && call.Files.Count > maxReferences)
        {
            throw new InvalidArgumentException("files", call.Files.Count, "QuiverAI generate supports up to " + maxReferences + " reference images for model \"" + ModelId + "\".");
        }

        var body = new JsonObject { ["model"] = ModelId, ["n"] = call.N, ["prompt"] = call.Prompt };
        AddSharedOptions(body, options);
        Add(body, "instructions", options.Instructions);
        if (call.Files != null)
        {
            body["references"] = new JsonArray(call.Files.Select(file => (JsonNode)Reference(file)).ToArray());
        }

        return body;
    }

    private JsonObject BuildAnimationBody(ImageModelCall call, QuiverAIOptions options)
    {
        if (ModelId != "arrow-2" && ModelId != "arrow-2-telos")
        {
            throw new InvalidArgumentException("modelId", ModelId, "QuiverAI animate is supported by the \"arrow-2\" and \"arrow-2-telos\" models.");
        }

        if (call.Files == null || call.Files.Count == 0)
        {
            throw new InvalidArgumentException("files", null, "QuiverAI animate requires exactly one source SVG in prompt.images.");
        }

        if (call.Files.Count != 1)
        {
            throw new InvalidArgumentException("files", call.Files.Count, "QuiverAI animate accepts exactly one source SVG in prompt.images.");
        }

        if (call.N != 1)
        {
            throw new InvalidArgumentException("n", call.N, "QuiverAI animate returns one SVG per request. Set maxImagesPerCall to 1 in generateImage to animate multiple times.");
        }

        if (call.Mask != null)
        {
            throw new InvalidArgumentException("mask", null, "QuiverAI animate does not support masks.");
        }

        if (call.Prompt != null && call.Prompt.Trim().Length == 0)
        {
            throw new InvalidArgumentException("prompt", call.Prompt, "QuiverAI animate requires a non-empty prompt when an animation instruction is provided.");
        }

        var unsupported = new (string Name, bool Present)[]
        {
            ("instructions", options.Instructions != null),
            ("topP", options.TopP != null),
            ("presencePenalty", options.HasPresencePenalty),
            ("attributes", options.Attributes != null),
            ("autoCrop", options.AutoCrop != null),
            ("targetSize", options.TargetSize != null),
        }.FirstOrDefault(option => option.Present);
        if (unsupported.Name != null)
        {
            throw new InvalidArgumentException("providerOptions.quiverai." + unsupported.Name, null, "QuiverAI animate does not support providerOptions.quiverai." + unsupported.Name + ".");
        }

        var body = new JsonObject { ["model"] = ModelId, ["svg_source"] = AnimationSource(call.Files[0]) };
        Add(body, "prompt", call.Prompt);
        Add(body, "temperature", options.Temperature);
        Add(body, "max_output_tokens", options.MaxOutputTokens);
        Add(body, "reasoning_effort", options.ReasoningEffort);
        body["stream"] = false;
        return body;
    }

    private JsonObject BuildEditBody(ImageModelCall call, QuiverAIOptions options)
    {
        if (ModelId != "arrow-2" && ModelId != "arrow-2-telos")
        {
            throw new InvalidArgumentException("modelId", ModelId, "QuiverAI SVG editing is supported by the \"arrow-2\" and \"arrow-2-telos\" models.");
        }

        if (string.IsNullOrWhiteSpace(call.Prompt))
        {
            throw new InvalidArgumentException("prompt", call.Prompt, "QuiverAI SVG editing requires a non-empty instruction in generateImage prompt.text.");
        }

        if (call.Prompt!.Length > 4000)
        {
            throw new InvalidArgumentException("prompt", call.Prompt, "QuiverAI SVG editing instructions must contain at most 4000 characters.");
        }

        if (call.Files == null || call.Files.Count == 0)
        {
            throw new InvalidArgumentException("files", null, "QuiverAI SVG editing requires one source SVG in generateImage prompt.images.");
        }

        if (call.Files.Count != 1)
        {
            throw new InvalidArgumentException("files", call.Files.Count, "QuiverAI SVG editing accepts exactly one source SVG.");
        }

        if (call.N != 1)
        {
            throw new InvalidArgumentException("n", call.N, "QuiverAI SVG editing returns exactly one SVG per request. Set maxImagesPerCall to 1 in generateImage to edit multiple times.");
        }

        if (call.Mask != null)
        {
            throw new InvalidArgumentException("mask", null, "QuiverAI SVG editing does not support masks.");
        }

        var unsupported = new (string Name, bool Present)[]
        {
            ("instructions", options.Instructions != null),
            ("attributes", options.Attributes != null),
            ("topP", options.TopP != null),
            ("presencePenalty", options.PresencePenalty != null),
            ("autoCrop", options.AutoCrop != null),
            ("targetSize", options.TargetSize != null),
        }.Where(option => option.Present).Select(option => option.Name).ToArray();
        if (unsupported.Length > 0)
        {
            throw new InvalidArgumentException("providerOptions", null, "QuiverAI SVG editing does not support these provider options: " + string.Join(", ", unsupported) + ".");
        }

        var body = new JsonObject { ["model"] = ModelId, ["prompt"] = call.Prompt, ["svg_source"] = EditSource(call.Files[0]) };
        if (options.ReferenceImages != null)
        {
            var references = new JsonArray();
            for (var index = 0; index < options.ReferenceImages.Count; index++)
            {
                var reference = options.ReferenceImages[index];
                if (reference.Url != null)
                {
                    references.Add(new JsonObject { ["url"] = QuiverAIImageReference.ValidateImageUrl(reference.Url) });
                    continue;
                }

                QuiverAIImageReference.ValidateReferenceBase64(reference.Base64!, "providerOptions.quiverai.referenceImages[" + index + "]");
                references.Add(new JsonObject { ["base64"] = reference.Base64 });
            }

            body["reference_images"] = references;
        }

        Add(body, "max_review_steps", options.MaxReviewSteps);
        Add(body, "reasoning_effort", options.ReasoningEffort);
        var settings = new JsonObject();
        Add(settings, "max_output_tokens", options.MaxOutputTokens);
        Add(settings, "orchestrator_max_output_tokens", options.OrchestratorMaxOutputTokens);
        Add(settings, "shallow_max_output_tokens", options.ShallowMaxOutputTokens);
        Add(settings, "temperature", options.Temperature);
        if (settings.Count > 0)
        {
            body["settings"] = settings;
        }

        body["stream"] = false;
        return body;
    }

    private static void RejectEditOnlyOptions(QuiverAIOptions options)
    {
        var editOnly = new (string Name, bool Present)[]
        {
            ("referenceImages", options.ReferenceImages != null),
            ("maxReviewSteps", options.MaxReviewSteps != null),
            ("orchestratorMaxOutputTokens", options.OrchestratorMaxOutputTokens != null),
            ("shallowMaxOutputTokens", options.ShallowMaxOutputTokens != null),
        }.Where(option => option.Present).Select(option => option.Name).ToArray();
        if (editOnly.Length > 0)
        {
            throw new InvalidArgumentException("providerOptions", null, "QuiverAI " + options.Operation + " does not support these edit-only provider options: " + string.Join(", ", editOnly) + ".");
        }
    }

    private static void AddSharedOptions(JsonObject body, QuiverAIOptions options)
    {
        Add(body, "temperature", options.Temperature);
        Add(body, "top_p", options.TopP);
        if (options.HasPresencePenalty)
        {
            body["presence_penalty"] = options.PresencePenalty;
        }

        Add(body, "max_output_tokens", options.MaxOutputTokens);
        Add(body, "reasoning_effort", options.ReasoningEffort);
        if (options.Attributes != null)
        {
            body["attributes"] = JsonNode.Parse(options.Attributes.Value.GetRawText());
        }

        body["stream"] = false;
    }

    private static void Add<T>(JsonObject body, string name, T? value)
        where T : struct
    {
        if (value != null)
        {
            body[name] = JsonValue.Create(value.Value);
        }
    }

    private static void Add(JsonObject body, string name, string? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static JsonObject Reference(ImageModelFile file)
    {
        return file.Type == "url"
            ? new JsonObject { ["url"] = file.Url }
            : new JsonObject { ["base64"] = Convert.ToBase64String(file.Data!) };
    }

    private static JsonObject AnimationSource(ImageModelFile file)
    {
        if (file.Type == "url")
        {
            if (!Uri.TryCreate(file.Url, UriKind.Absolute, out var url))
            {
                throw new InvalidArgumentException("files", file.Url, "QuiverAI animate requires a valid HTTP or HTTPS SVG URL.");
            }

            if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
            {
                throw new InvalidArgumentException("files", file.Url, "QuiverAI animate requires an HTTP or HTTPS SVG URL.");
            }

            return new JsonObject { ["url"] = file.Url };
        }

        var bytes = file.Data!;
        var head = Encoding.UTF8.GetString(bytes, 0, Math.Min(bytes.Length, 4096)).TrimStart();
        if (!AnimationSvgStart.IsMatch(head))
        {
            throw new InvalidArgumentException("files", null, "QuiverAI animate requires the input file to contain SVG data.");
        }

        var base64 = Convert.ToBase64String(bytes);
        if (base64.Length > MaxAnimationSourceBase64Length)
        {
            throw new InvalidArgumentException("files", null, "QuiverAI animate accepts at most " + MaxAnimationSourceBase64Length + " base64 characters for the source SVG.");
        }

        return new JsonObject { ["base64"] = base64 };
    }

    private static JsonObject EditSource(ImageModelFile file)
    {
        if (file.Type == "url")
        {
            return new JsonObject { ["url"] = QuiverAIImageReference.ValidateImageUrl(file.Url!) };
        }

        var data = file.Data!;
        if (data.Length == 0 || data.Length > MaxEditSvgBytes)
        {
            throw new InvalidArgumentException("files", null, "QuiverAI SVG source data must contain 1-" + MaxEditSvgBytes + " bytes.");
        }

        string svg;
        try
        {
            svg = new UTF8Encoding(false, true).GetString(data);
        }
        catch (DecoderFallbackException)
        {
            throw new InvalidArgumentException("files", null, "QuiverAI SVG source data must be valid UTF-8.");
        }

        if (svg.Length > MaxEditSvgBytes || !IsSvgMarkup(svg))
        {
            throw new InvalidArgumentException("files", null, "QuiverAI SVG source data must contain a complete SVG document.");
        }

        return new JsonObject { ["base64"] = Convert.ToBase64String(data) };
    }

    /// <summary>Checks that <paramref name="svg"/> is one well-formed XML document whose root element is <c>svg</c>.</summary>
    private static bool IsSvgMarkup(string svg)
    {
        var document = svg.Length > 0 && svg[0] == (char)0xFEFF ? svg.Substring(1) : svg;
        var elements = new Stack<string>();
        var position = 0;
        var rootSeen = false;
        var rootClosed = false;
        var doctypeSeen = false;

        while (position < document.Length)
        {
            if (document[position] != '<')
            {
                var nextTag = document.IndexOf('<', position);
                var end = nextTag == -1 ? document.Length : nextTag;
                var text = document.Substring(position, end - position);
                if ((elements.Count == 0 && text.Trim().Length > 0) || text.Contains("]]>") || !HasValidXmlReferences(text))
                {
                    return false;
                }

                position = end;
                continue;
            }

            if (StartsAt(document, "<!--", position))
            {
                var commentEnd = document.IndexOf("-->", position + 4, StringComparison.Ordinal);
                if (commentEnd == -1 || document.Substring(position + 4, commentEnd - position - 4).Contains("--"))
                {
                    return false;
                }

                position = commentEnd + 3;
                continue;
            }

            if (StartsAt(document, "<?", position))
            {
                var instructionEnd = document.IndexOf("?>", position + 2, StringComparison.Ordinal);
                if (instructionEnd == -1)
                {
                    return false;
                }

                position = instructionEnd + 2;
                continue;
            }

            if (StartsAt(document, "<![CDATA[", position))
            {
                if (elements.Count == 0)
                {
                    return false;
                }

                var cdataEnd = document.IndexOf("]]>", position + 9, StringComparison.Ordinal);
                if (cdataEnd == -1)
                {
                    return false;
                }

                position = cdataEnd + 3;
                continue;
            }

            if (position + 9 <= document.Length && string.Equals(document.Substring(position, 9), "<!DOCTYPE", StringComparison.OrdinalIgnoreCase))
            {
                if (rootSeen || doctypeSeen || elements.Count > 0 || !IsXmlWhitespace(At(document, position + 9)))
                {
                    return false;
                }

                var doctypeName = ReadXmlName(document, SkipXmlWhitespace(document, position + 9));
                if (doctypeName == null || !string.Equals(doctypeName.Value.Name, "svg", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var doctypeEnd = FindDoctypeEnd(document, doctypeName.Value.End);
                if (doctypeEnd == -1)
                {
                    return false;
                }

                doctypeSeen = true;
                position = doctypeEnd;
                continue;
            }

            if (StartsAt(document, "<!", position))
            {
                return false;
            }

            if (StartsAt(document, "</", position))
            {
                var closingTag = ReadXmlName(document, position + 2);
                if (closingTag == null)
                {
                    return false;
                }

                var tagEnd = SkipXmlWhitespace(document, closingTag.Value.End);
                if (At(document, tagEnd) != '>' || elements.Count == 0 || elements.Pop() != closingTag.Value.Name)
                {
                    return false;
                }

                if (elements.Count == 0)
                {
                    rootClosed = true;
                }

                position = tagEnd + 1;
                continue;
            }

            if (rootClosed)
            {
                return false;
            }

            var openingTag = ReadXmlName(document, position + 1);
            if (openingTag == null)
            {
                return false;
            }

            if (!rootSeen)
            {
                if (!string.Equals(openingTag.Value.Name, "svg", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                rootSeen = true;
            }

            var attributes = new HashSet<string>(StringComparer.Ordinal);
            var tagPosition = openingTag.Value.End;
            while (tagPosition < document.Length)
            {
                var beforeWhitespace = tagPosition;
                tagPosition = SkipXmlWhitespace(document, tagPosition);
                if (StartsAt(document, "/>", tagPosition))
                {
                    tagPosition += 2;
                    if (elements.Count == 0)
                    {
                        rootClosed = true;
                    }

                    position = tagPosition;
                    break;
                }

                if (At(document, tagPosition) == '>')
                {
                    elements.Push(openingTag.Value.Name);
                    position = tagPosition + 1;
                    break;
                }

                if (tagPosition == beforeWhitespace)
                {
                    return false;
                }

                var attribute = ReadXmlName(document, tagPosition);
                if (attribute == null || !attributes.Add(attribute.Value.Name))
                {
                    return false;
                }

                tagPosition = SkipXmlWhitespace(document, attribute.Value.End);
                if (At(document, tagPosition) != '=')
                {
                    return false;
                }

                tagPosition = SkipXmlWhitespace(document, tagPosition + 1);
                var quote = At(document, tagPosition);
                if (quote != '"' && quote != '\'')
                {
                    return false;
                }

                var valueEnd = document.IndexOf(quote, tagPosition + 1);
                if (valueEnd == -1)
                {
                    return false;
                }

                var value = document.Substring(tagPosition + 1, valueEnd - tagPosition - 1);
                if (value.Contains("<") || !HasValidXmlReferences(value))
                {
                    return false;
                }

                tagPosition = valueEnd + 1;
            }

            if (tagPosition >= document.Length && position != document.Length)
            {
                return false;
            }
        }

        return rootSeen && rootClosed && elements.Count == 0;
    }

    private static bool StartsAt(string value, string prefix, int position)
    {
        return string.CompareOrdinal(value, position, prefix, 0, prefix.Length) == 0 && position + prefix.Length <= value.Length;
    }

    private static char At(string value, int position)
    {
        return position < value.Length ? value[position] : '\0';
    }

    private static bool IsXmlWhitespace(char character)
    {
        return character is '\t' or '\n' or '\r' or ' ';
    }

    private static bool IsXmlNameStart(char character)
    {
        return character is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_' or ':' || character >= (char)0x80;
    }

    private static bool IsXmlNameCharacter(char character)
    {
        return IsXmlNameStart(character) || character is '-' or '.' or (>= '0' and <= '9');
    }

    private static (string Name, int End)? ReadXmlName(string value, int position)
    {
        if (!IsXmlNameStart(At(value, position)))
        {
            return null;
        }

        var start = position;
        position++;
        while (IsXmlNameCharacter(At(value, position)))
        {
            position++;
        }

        return (value.Substring(start, position - start), position);
    }

    private static int SkipXmlWhitespace(string value, int position)
    {
        while (IsXmlWhitespace(At(value, position)))
        {
            position++;
        }

        return position;
    }

    private static bool HasValidXmlReferences(string value)
    {
        var position = value.IndexOf('&');
        while (position != -1)
        {
            var end = value.IndexOf(';', position + 1);
            if (end == -1)
            {
                return false;
            }

            var reference = value.Substring(position + 1, end - position - 1);
            if (!DecimalReference.IsMatch(reference) && !HexReference.IsMatch(reference) && !IsXmlName(reference))
            {
                return false;
            }

            position = value.IndexOf('&', end + 1);
        }

        return true;
    }

    private static bool IsXmlName(string value)
    {
        return value.Length > 0 && IsXmlNameStart(value[0]) && value.Skip(1).All(IsXmlNameCharacter);
    }

    private static int FindDoctypeEnd(string value, int position)
    {
        var subsetDepth = 0;
        char? quote = null;
        while (position < value.Length)
        {
            var character = value[position];
            if (quote != null)
            {
                if (character == quote)
                {
                    quote = null;
                }
            }
            else if (character is '"' or '\'')
            {
                quote = character;
            }
            else if (character == '[')
            {
                subsetDepth++;
            }
            else if (character == ']')
            {
                if (subsetDepth == 0)
                {
                    return -1;
                }

                subsetDepth--;
            }
            else if (character == '>' && subsetDepth == 0)
            {
                return position + 1;
            }

            position++;
        }

        return -1;
    }

    private static QuiverAIOptions ParseOptions(JsonElement? providerOptions)
    {
        var options = new QuiverAIOptions();
        if (providerOptions is not { ValueKind: JsonValueKind.Object } bag
            || !bag.TryGetProperty("quiverai", out var quiverai)
            || quiverai.ValueKind != JsonValueKind.Object)
        {
            return options;
        }

        foreach (var property in quiverai.EnumerateObject())
        {
            var value = property.Value;
            switch (property.Name)
            {
                case "operation":
                    options.Operation = OneOf(value, property.Name, "generate", "vectorize", "animate", "edit");
                    break;
                case "instructions":
                    options.Instructions = NonEmptyString(value, property.Name, int.MaxValue);
                    break;
                case "reasoningEffort":
                    options.ReasoningEffort = OneOf(value, property.Name, ReasoningEfforts);
                    break;
                case "referenceImages":
                    options.ReferenceImages = ReferenceImages(value);
                    break;
                case "maxReviewSteps":
                    options.MaxReviewSteps = Integer(value, property.Name, 0, 5);
                    break;
                case "attributes":
                    options.Attributes = Attributes(value);
                    break;
                case "temperature":
                    options.Temperature = Number(value, property.Name, 0, 2);
                    break;
                case "topP":
                    options.TopP = Number(value, property.Name, 0, 1);
                    break;
                case "presencePenalty":
                    options.HasPresencePenalty = true;
                    options.PresencePenalty = value.ValueKind == JsonValueKind.Null ? null : Number(value, property.Name, -2, 2);
                    break;
                case "maxOutputTokens":
                    options.MaxOutputTokens = Integer(value, property.Name, 1, 131072);
                    break;
                case "orchestratorMaxOutputTokens":
                    options.OrchestratorMaxOutputTokens = Integer(value, property.Name, 1, 65536);
                    break;
                case "shallowMaxOutputTokens":
                    options.ShallowMaxOutputTokens = Integer(value, property.Name, 1, 65536);
                    break;
                case "autoCrop":
                    options.AutoCrop = value.ValueKind switch
                    {
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => throw InvalidOption(property.Name, "must be a boolean"),
                    };
                    break;
                case "targetSize":
                    options.TargetSize = Integer(value, property.Name, 128, 4096);
                    break;
            }
        }

        return options;
    }

    private static List<QuiverAIReferenceOption> ReferenceImages(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() > 4)
        {
            throw InvalidOption("referenceImages", "must be an array of at most 4 references");
        }

        var references = new List<QuiverAIReferenceOption>();
        foreach (var item in value.EnumerateArray())
        {
            var properties = item.ValueKind == JsonValueKind.Object ? item.EnumerateObject().ToArray() : Array.Empty<JsonProperty>();
            if (properties.Length != 1 || (properties[0].Name != "url" && properties[0].Name != "base64"))
            {
                throw InvalidOption("referenceImages", "must contain objects with exactly one of url or base64");
            }

            var text = NonEmptyString(properties[0].Value, "referenceImages", properties[0].Name == "base64" ? 16_777_216 : int.MaxValue);
            references.Add(properties[0].Name == "url" ? new QuiverAIReferenceOption(text, null) : new QuiverAIReferenceOption(null, text));
        }

        return references;
    }

    private static JsonElement Attributes(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw InvalidOption("attributes", "must be an object");
        }

        if (value.TryGetProperty("viewBox", out var viewBox))
        {
            if (viewBox.ValueKind != JsonValueKind.Object)
            {
                throw InvalidOption("attributes.viewBox", "must be an object");
            }

            Number(Required(viewBox, "minX"), "attributes.viewBox.minX", double.MinValue, double.MaxValue);
            Number(Required(viewBox, "minY"), "attributes.viewBox.minY", double.MinValue, double.MaxValue);
            if (Number(Required(viewBox, "width"), "attributes.viewBox.width", double.MinValue, double.MaxValue) <= 0
                || Number(Required(viewBox, "height"), "attributes.viewBox.height", double.MinValue, double.MaxValue) <= 0)
            {
                throw InvalidOption("attributes.viewBox", "must have a positive width and height");
            }
        }

        return value.Clone();
    }

    private static JsonElement Required(JsonElement value, string name)
    {
        return value.TryGetProperty(name, out var property) ? property : default;
    }

    private static string OneOf(JsonElement value, string name, params string[] allowed)
    {
        var text = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        if (text == null || Array.IndexOf(allowed, text) < 0)
        {
            throw InvalidOption(name, "must be one of " + string.Join(", ", allowed));
        }

        return text;
    }

    private static string NonEmptyString(JsonElement value, string name, int maxLength)
    {
        var text = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        if (string.IsNullOrEmpty(text) || text!.Length > maxLength)
        {
            throw InvalidOption(name, "must be a non-empty string");
        }

        return text;
    }

    private static double Number(JsonElement value, string name, double min, double max)
    {
        if (value.ValueKind != JsonValueKind.Number || value.GetDouble() < min || value.GetDouble() > max)
        {
            throw InvalidOption(name, "must be a number from " + min + " to " + max);
        }

        return value.GetDouble();
    }

    private static int Integer(JsonElement value, string name, int min, int max)
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number) || number < min || number > max)
        {
            throw InvalidOption(name, "must be an integer from " + min + " to " + max);
        }

        return number;
    }

    private static InvalidArgumentException InvalidOption(string name, string rule)
    {
        return new InvalidArgumentException("providerOptions", null, "Invalid QuiverAI provider option " + name + ": " + rule + ".");
    }

    private sealed class QuiverAIOptions
    {
        public string Operation { get; set; } = "generate";

        public string? Instructions { get; set; }

        public string? ReasoningEffort { get; set; }

        public List<QuiverAIReferenceOption>? ReferenceImages { get; set; }

        public int? MaxReviewSteps { get; set; }

        public JsonElement? Attributes { get; set; }

        public double? Temperature { get; set; }

        public double? TopP { get; set; }

        public bool HasPresencePenalty { get; set; }

        public double? PresencePenalty { get; set; }

        public int? MaxOutputTokens { get; set; }

        public int? OrchestratorMaxOutputTokens { get; set; }

        public int? ShallowMaxOutputTokens { get; set; }

        public bool? AutoCrop { get; set; }

        public int? TargetSize { get; set; }
    }

    private sealed class QuiverAIReferenceOption
    {
        public QuiverAIReferenceOption(string? url, string? base64)
        {
            Url = url;
            Base64 = base64;
        }

        public string? Url { get; }

        public string? Base64 { get; }
    }
}
