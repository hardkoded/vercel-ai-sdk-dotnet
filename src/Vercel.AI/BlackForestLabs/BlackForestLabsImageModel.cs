// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.GenerateText;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using ProviderGeneratedImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI.BlackForestLabs;

/// <summary>
/// Black Forest Labs image model. It submits the request, polls <c>polling_url</c> until the task is <c>Ready</c>,
/// then downloads the sample. Model ids pick the legacy, FLUX 3, or latest image API.
/// </summary>
public sealed class BlackForestLabsImageModel : IImageModel, IImageCaller
{
    private const int DefaultPollIntervalMillis = 500;
    private const int DefaultPollTimeoutMillis = 60000;

    private static readonly Regex LegacyFamily = new Regex("^flux-[12](?:[.-]|$)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private static readonly string[] Flux3AspectRatios =
    {
        "21:9", "2:1", "16:9", "3:2", "7:5", "4:3", "5:4", "1:1", "4:5", "3:4", "5:7", "2:3", "9:16", "1:2", "9:21",
    };

    private static readonly string[] LatestOptions =
    {
        "height", "outputFormat", "pollIntervalMillis", "pollTimeoutMillis", "resolution", "safetyTolerance", "steps", "width",
    };

    private static readonly string[] Flux3Options =
    {
        "grounding", "pollIntervalMillis", "pollTimeoutMillis", "resolution", "safetyTolerance", "version",
    };

    private static readonly string[] TerminalStatuses =
    {
        "Content Moderated", "Error", "Failed", "Request Moderated", "Task not found",
    };

    private static readonly string[] PollStatuses =
    {
        "Content Moderated", "Generating", "Pending", "Reasoning", "Ready", "Error", "Failed", "Request Moderated", "Task not found",
    };

    private readonly BlackForestLabsProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp.</summary>
    public BlackForestLabsImageModel(BlackForestLabsProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <summary>Called each time <c>GetArgs</c> parses the provider options. Tests count the calls.</summary>
    internal Action? OnProviderOptionsParsed { get; set; }

    /// <inheritdoc />
    public string Provider => "black-forest-labs.image";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Specification version. Always <c>V4</c>.</summary>
    public string SpecificationVersion => "V4";

    /// <summary>Black Forest Labs returns one image per call.</summary>
    public int? MaxImagesPerCall => 1;

    /// <summary>True when the model takes input images, false when it does not, null when unknown.</summary>
    public bool? SupportsFileInputs
    {
        get
        {
            if (ModelApi != BflApi.Legacy
                || ModelId == "flux-kontext-pro" || ModelId == "flux-kontext-max" || ModelId == "flux-pro-1.0-fill")
            {
                return true;
            }

            if (ModelId == "flux-pro-1.1-ultra" || ModelId == "flux-pro-1.1")
            {
                return false;
            }

            return null;
        }
    }

    /// <summary>True when the model takes a mask, false when it does not, null when unknown.</summary>
    public bool? SupportsMaskInputs
    {
        get
        {
            if (ModelId == "flux-pro-1.0-fill")
            {
                return true;
            }

            return SupportsFileInputs == null ? null : false;
        }
    }

    private BflApi ModelApi
    {
        get
        {
            if (ModelId == "flux-pro" || ModelId == "flux-dev"
                || ModelId.StartsWith("flux-pro-", StringComparison.Ordinal)
                || ModelId.StartsWith("flux-kontext-", StringComparison.Ordinal)
                || LegacyFamily.IsMatch(ModelId))
            {
                return BflApi.Legacy;
            }

            if (ModelId == "flux-3-image")
            {
                return BflApi.Flux3Image;
            }

            // Unknown model ids inherit the latest supported image API.
            return BflApi.Latest;
        }
    }

    /// <inheritdoc />
    public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxImagesPerCall);
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var result = await DoGenerateAsync(options.ToModelCall(cancellationToken), cancellationToken).ConfigureAwait(false);
        var data = (byte[])result.Images[0]!;
        var info = new ImageResponseInfo(new DateTimeOffset(result.Response.Timestamp ?? DateTime.UtcNow), result.Response.ModelId, result.Response.Headers);
        return new ImageGenerationResult(
            new[] { new ProviderGeneratedImage(MediaTypes.DetectMediaType(data, "image") ?? "image/png", data, null) },
            result.Warnings.Select(warning => warning.ToCallWarning()).ToList(),
            info);
    }

    /// <inheritdoc />
    public async Task<ImageModelResult> DoGenerateAsync(ImageModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var (body, warnings, options) = GetArgs(call);
        var timestamp = _clock();
        var headers = _provider.CreateHeaders(call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value));
        var baseUrl = _provider.Options.BaseUrl.TrimEnd('/');

        var submitUrl = baseUrl + "/" + ModelId;
        ProviderExchangeResult submitted;
        try
        {
            submitted = await ProviderExchange.SendAsync(_provider.HttpClient, HttpMethod.Post, new Uri(submitUrl), ProviderExchange.Json(body.ToJsonString()), headers, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw Failed(exception, submitUrl);
        }

        using var submit = ParseSubmit(submitted.Body);
        var root = submit.RootElement;
        var pollUrl = root.GetProperty("polling_url").GetString()!;
        var requestId = root.GetProperty("id").GetString()!;

        var polled = await PollForImageUrlAsync(pollUrl, requestId, baseUrl, headers, options, cancellationToken).ConfigureAwait(false);

        var baseHost = new Uri(baseUrl).Host;
        var useCustomDownloadHeaders = ProviderValues.IsSameOrigin(polled.ImageUrl, baseUrl)
            && baseHost != "bfl.ai"
            && !baseHost.EndsWith(".bfl.ai", StringComparison.OrdinalIgnoreCase);
        var userAgentOnly = UserAgentOnly(headers);
        var downloadHeaders = useCustomDownloadHeaders ? headers : userAgentOnly;
        // The sample URL comes from the response body. Signed BFL downloads need no credentials; custom proxies may.
        var image = await ProviderExchange.GetResponseUrlAsync(
            _provider.HttpClient,
            polled.ImageUrl,
            baseUrl,
            DownloadUrls.ValidateDownloadUrl,
            downloadHeaders,
            cancellationToken,
            ProviderExchange.DownloadClient(_provider.Options),
            userAgentOnly).ConfigureAwait(false);

        var metadata = new JsonObject();
        AddNumber(metadata, "seed", polled.Seed);
        AddNumber(metadata, "start_time", polled.StartTime);
        AddNumber(metadata, "end_time", polled.EndTime);
        AddNumber(metadata, "duration", polled.Duration);
        AddNumber(metadata, "cost", Number(root, "cost"));
        AddNumber(metadata, "inputMegapixels", Number(root, "input_mp"));
        AddNumber(metadata, "outputMegapixels", Number(root, "output_mp"));
        var providerMetadata = new JsonObject { ["blackForestLabs"] = new JsonObject { ["images"] = new JsonArray(metadata) } };
        return new ImageModelResult(
            new object?[] { image.Bytes },
            warnings,
            providerMetadata: OperationJson.Parse(providerMetadata.ToJsonString()),
            response: new ProviderResponse(image.Headers, timestamp: timestamp.UtcDateTime, modelId: ModelId));
    }

    private (JsonObject Body, List<OperationWarning> Warnings, BflOptions Options) GetArgs(ImageModelCall call)
    {
        var warnings = new List<OperationWarning>();
        var modelApi = ModelApi;
        var size = call.Size;
        var aspectRatio = call.AspectRatio;
        var finalAspectRatio = aspectRatio ?? (size != null ? ConvertSizeToAspectRatio(size) : null);

        if (size != null && aspectRatio == null && modelApi != BflApi.Latest)
        {
            warnings.Add(OperationWarning.Unsupported(
                "size",
                modelApi == BflApi.Flux3Image
                    ? "Deriving aspect_ratio from size. FLUX 3 uses the resolution provider option to select output resolution."
                    : "Deriving aspect_ratio from size. Use the width and height provider options to specify dimensions for models that support them."));
        }
        else if (size != null && aspectRatio != null && modelApi != BflApi.Latest)
        {
            warnings.Add(OperationWarning.Unsupported(
                "size",
                modelApi == BflApi.Flux3Image
                    ? "FLUX 3 ignores size when aspectRatio is provided. Use the resolution provider option to select output resolution."
                    : "Black Forest Labs ignores size when aspectRatio is provided. Use the width and height provider options to specify dimensions for models that support them"));
        }

        var sizeParts = size?.Split('x') ?? Array.Empty<string>();
        var rawOptions = RawOptions(call.ProviderOptions);
        if (modelApi == BflApi.Latest && size != null)
        {
            var widthFromSize = ParseNumber(sizeParts.Length > 0 ? sizeParts[0] : null);
            var heightFromSize = ParseNumber(sizeParts.Length > 1 ? sizeParts[1] : null);
            var merged = new Dictionary<string, JsonElement>
            {
                ["width"] = NumberElement(widthFromSize),
                ["height"] = NumberElement(heightFromSize),
            };
            foreach (var pair in rawOptions)
            {
                merged[pair.Key] = pair.Value;
            }

            rawOptions = merged;
        }

        var bflOptions = BflOptions.Parse(rawOptions, modelApi);
        OnProviderOptionsParsed?.Invoke();

        var inputImages = new List<string>();
        if (call.Files != null)
        {
            foreach (var file in call.Files)
            {
                inputImages.Add(file.Type == "url" ? file.Url! : Convert.ToBase64String(file.Data ?? Array.Empty<byte>()));
            }
        }

        if (inputImages.Count > 10)
        {
            throw new AiSdkException("Black Forest Labs supports up to 10 input images.");
        }

        if (modelApi != BflApi.Legacy)
        {
            var hasDimensions = modelApi == BflApi.Latest && bflOptions.Width != null;
            // The endpoint uses 21:9 and 9:21 instead of their reduced forms.
            var flux3AspectRatio = hasDimensions
                ? null
                : finalAspectRatio == "7:3" ? "21:9" : finalAspectRatio == "3:7" ? "9:21" : finalAspectRatio;
            if (flux3AspectRatio != null && Array.IndexOf(Flux3AspectRatios, flux3AspectRatio) < 0)
            {
                warnings.Add(OperationWarning.Unsupported(
                    "aspectRatio",
                    "FLUX 3 does not support aspect ratio " + finalAspectRatio + ". Using the endpoint's default auto aspect ratio."));
                flux3AspectRatio = null;
            }

            if (call.Mask != null)
            {
                throw new UnsupportedFunctionalityException("FLUX 3 image masks", "'FLUX 3 image masks' functionality not supported.");
            }

            if (call.Seed != null && modelApi != BflApi.Latest)
            {
                warnings.Add(OperationWarning.Unsupported("seed"));
            }

            var supportedOptions = modelApi == BflApi.Latest ? LatestOptions : Flux3Options;
            foreach (var key in bflOptions.Keys)
            {
                if (Array.IndexOf(supportedOptions, key) < 0)
                {
                    warnings.Add(OperationWarning.Unsupported("blackForestLabs." + key));
                }
            }

            var flux3Body = new JsonObject();
            Set(flux3Body, "prompt", call.Prompt);
            Set(flux3Body, "aspect_ratio", flux3AspectRatio);
            if (inputImages.Count > 0)
            {
                flux3Body["images"] = new JsonArray(inputImages.Select(image => (JsonNode?)JsonValue.Create(image)).ToArray());
            }

            Set(flux3Body, "resolution", hasDimensions ? null : bflOptions.String("resolution"));
            Set(flux3Body, "safety_tolerance", bflOptions.Integer("safetyTolerance"));
            if (modelApi == BflApi.Latest)
            {
                Set(flux3Body, "width", bflOptions.Width);
                Set(flux3Body, "height", bflOptions.Height);
                Set(flux3Body, "seed", call.Seed);
                Set(flux3Body, "steps", bflOptions.Integer("steps"));
                Set(flux3Body, "output_format", bflOptions.String("outputFormat"));
            }
            else
            {
                Set(flux3Body, "grounding", bflOptions.Boolean("grounding"));
                Set(flux3Body, "version", bflOptions.String("version"));
            }

            return (flux3Body, warnings, bflOptions);
        }

        var inputImageField = ModelId == "flux-pro-1.0-fill" ? "image" : "input_image";
        var body = new JsonObject();
        Set(body, "prompt", call.Prompt);
        Set(body, "seed", call.Seed);
        Set(body, "aspect_ratio", finalAspectRatio);
        Set(body, "width", bflOptions.Width ?? (size != null ? SizeInt(sizeParts, 0) : null));
        Set(body, "height", bflOptions.Height ?? (size != null ? SizeInt(sizeParts, 1) : null));
        Set(body, "steps", bflOptions.Integer("steps"));
        Set(body, "guidance", bflOptions.Number("guidance"));
        Set(body, "image_prompt_strength", bflOptions.Number("imagePromptStrength"));
        Set(body, "image_prompt", bflOptions.String("imagePrompt"));
        for (var index = 0; index < inputImages.Count; index++)
        {
            body[inputImageField + (index == 0 ? string.Empty : "_" + (index + 1).ToString(CultureInfo.InvariantCulture))] = inputImages[index];
        }

        if (call.Mask != null)
        {
            Set(body, "mask", call.Mask.Type == "url" ? call.Mask.Url : Convert.ToBase64String(call.Mask.Data ?? Array.Empty<byte>()));
        }

        Set(body, "output_format", bflOptions.String("outputFormat"));
        Set(body, "prompt_upsampling", bflOptions.Boolean("promptUpsampling"));
        Set(body, "raw", bflOptions.Boolean("raw"));
        Set(body, "safety_tolerance", bflOptions.Integer("safetyTolerance"));
        Set(body, "webhook_secret", bflOptions.String("webhookSecret"));
        Set(body, "webhook_url", bflOptions.String("webhookUrl"));
        return (body, warnings, bflOptions);
    }

    private async Task<PolledImage> PollForImageUrlAsync(
        string pollUrl,
        string requestId,
        string baseUrl,
        IReadOnlyDictionary<string, string?> headers,
        BflOptions options,
        CancellationToken cancellationToken)
    {
        var pollIntervalMillis = options.Integer("pollIntervalMillis") ?? _provider.PollIntervalMillis ?? DefaultPollIntervalMillis;
        var pollTimeoutMillis = options.Integer("pollTimeoutMillis") ?? _provider.PollTimeoutMillis ?? DefaultPollTimeoutMillis;

        var builder = new UriBuilder(pollUrl);
        var query = builder.Query.TrimStart('?');
        if (!query.Split('&').Any(part => part == "id" || part.StartsWith("id=", StringComparison.Ordinal)))
        {
            builder.Query = (query.Length == 0 ? string.Empty : query + "&") + "id=" + Uri.EscapeDataString(requestId);
        }

        var url = builder.Uri;
        // Credentials only go to a trusted provider host. For those the poll origin is the trusted origin.
        var trusted = IsTrustedUrl(url.AbsoluteUri, baseUrl);
        var trustedOrigin = trusted ? url.AbsoluteUri : baseUrl;

        var userAgentOnly = UserAgentOnly(headers);
        using var timeout = new CancellationTokenSource(pollTimeoutMillis);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        try
        {
            while (true)
            {
                ProviderExchangeResult response;
                try
                {
                    response = await ProviderExchange.GetResponseUrlAsync(
                        _provider.HttpClient,
                        url.AbsoluteUri,
                        trustedOrigin,
                        DownloadUrls.ValidateDownloadUrl,
                        trusted ? headers : null,
                        linked.Token,
                        ProviderExchange.DownloadClient(_provider.Options),
                        userAgentOnly).ConfigureAwait(false);
                }
                catch (ApiException exception)
                {
                    throw Failed(exception, url.AbsoluteUri);
                }

                using var document = ParsePoll(response.Body, out var status);
                if (status == "Ready")
                {
                    if (document.RootElement.TryGetProperty("result", out var result)
                        && result.ValueKind == JsonValueKind.Object
                        && result.TryGetProperty("sample", out var sample))
                    {
                        return new PolledImage(
                            sample.GetString()!,
                            Number(result, "seed"),
                            Number(result, "start_time"),
                            Number(result, "end_time"),
                            Number(result, "duration"));
                    }

                    throw new AiSdkException("Black Forest Labs poll response is Ready but missing result.sample");
                }

                if (status == "Error" || status == "Failed")
                {
                    throw new AiSdkException("Black Forest Labs generation failed.");
                }

                if (status == "Request Moderated" || status == "Content Moderated" || status == "Task not found")
                {
                    throw new AiSdkException("Black Forest Labs generation failed: " + status + ".");
                }

                await Task.Delay(pollIntervalMillis, linked.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new AiSdkException("Black Forest Labs generation timed out.");
        }
    }

    /// <summary>
    /// Black Forest Labs returns polling URLs on sibling cluster hosts of the API origin (for example <c>api.us1.bfl.ai</c>
    /// for a base URL on <c>api.bfl.ai</c>), so a strict same-origin check is not enough. Credentials may also go to
    /// any https host under the official <c>bfl.ai</c> domain.
    /// </summary>
    internal static bool IsTrustedUrl(string url, string baseUrl)
    {
        if (ProviderValues.IsSameOrigin(url, baseUrl))
        {
            return true;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
        {
            return false;
        }

        return parsed.Scheme == "https"
            && (parsed.Host == "bfl.ai" || parsed.Host.EndsWith(".bfl.ai", StringComparison.OrdinalIgnoreCase));
    }

    private static ApiException Failed(ApiException exception, string url)
    {
        var body = exception.ResponseBody;
        string? message = null;
        string? status = null;
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                status = StringProperty(root, "status") ?? StringProperty(root, "state");
                if (root.TryGetProperty("detail", out var detail) && detail.ValueKind != JsonValueKind.Null)
                {
                    message = detail.ValueKind == JsonValueKind.String ? detail.GetString() : JsonNode.Parse(detail.GetRawText())!.ToJsonString();
                }
                else
                {
                    message = StringProperty(root, "message");
                }

                message ??= status != null ? "Black Forest Labs generation failed: " + status + "." : "Unknown Black Forest Labs error";
            }
        }
        catch (JsonException)
        {
            // Not a Black Forest Labs error body. The HTTP failure message stays.
        }

        var retryable = Array.IndexOf(TerminalStatuses, status ?? string.Empty) < 0
            && (exception.StatusCode == 408 || exception.StatusCode == 409 || exception.StatusCode == 429 || exception.StatusCode >= 500);
        return new ApiException(message ?? exception.Message, exception.StatusCode, body)
        {
            Url = url,
            IsRetryable = retryable,
            ResponseHeaders = exception.ResponseHeaders,
        };
    }

    private static JsonDocument ParseSubmit(string text)
    {
        var document = Parse(text);
        var root = document.RootElement;
        var valid = root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String
            && root.TryGetProperty("polling_url", out var pollingUrl) && IsUrl(pollingUrl)
            && NullableNumber(root, "cost") && NullableNumber(root, "input_mp") && NullableNumber(root, "output_mp");
        if (!valid)
        {
            document.Dispose();
            throw new InvalidResponseDataException(text, "Invalid Black Forest Labs submit response");
        }

        return document;
    }

    private static JsonDocument ParsePoll(string text, out string status)
    {
        var document = Parse(text);
        var root = document.RootElement;
        string? found = null;
        var valid = root.ValueKind == JsonValueKind.Object;
        if (valid)
        {
            foreach (var name in new[] { "status", "state" })
            {
                if (root.TryGetProperty(name, out var value))
                {
                    valid &= value.ValueKind == JsonValueKind.String && Array.IndexOf(PollStatuses, value.GetString()) >= 0;
                    found ??= value.GetString();
                }
            }

            valid &= found != null;
            if (valid && root.TryGetProperty("result", out var result) && result.ValueKind != JsonValueKind.Null)
            {
                valid = result.ValueKind == JsonValueKind.Object
                    && result.TryGetProperty("sample", out var sample) && IsUrl(sample)
                    && OptionalNumber(result, "seed") && OptionalNumber(result, "start_time")
                    && OptionalNumber(result, "end_time") && OptionalNumber(result, "duration");
            }
        }

        if (!valid)
        {
            document.Dispose();
            throw new InvalidResponseDataException(text, found == null ? "Missing status in Black Forest Labs poll response" : "Invalid Black Forest Labs poll response");
        }

        status = found!;
        return document;
    }

    private static JsonDocument Parse(string text)
    {
        try
        {
            return JsonDocument.Parse(string.IsNullOrWhiteSpace(text) ? "{}" : text);
        }
        catch (JsonException exception)
        {
            throw new InvalidResponseDataException(text, "Invalid JSON response: " + exception.Message);
        }
    }

    private static bool IsUrl(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.String && Uri.TryCreate(value.GetString(), UriKind.Absolute, out _);
    }

    private static bool NullableNumber(JsonElement root, string name)
    {
        return !root.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Number;
    }

    private static bool OptionalNumber(JsonElement root, string name)
    {
        return !root.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Number;
    }

    private static string? StringProperty(JsonElement root, string name)
    {
        return root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static JsonElement? Number(JsonElement root, string name)
    {
        return root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number ? value.Clone() : null;
    }

    private static void AddNumber(JsonObject target, string name, JsonElement? value)
    {
        if (value != null)
        {
            target[name] = JsonNode.Parse(value.Value.GetRawText());
        }
    }

    private static Dictionary<string, string?> UserAgentOnly(IReadOnlyDictionary<string, string?> headers)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (headers.TryGetValue("User-Agent", out var userAgent))
        {
            result["User-Agent"] = userAgent;
        }

        return result;
    }

    private static Dictionary<string, JsonElement> RawOptions(JsonElement? providerOptions)
    {
        var result = new Dictionary<string, JsonElement>();
        if (providerOptions is { ValueKind: JsonValueKind.Object } bag
            && bag.TryGetProperty("blackForestLabs", out var options)
            && options.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in options.EnumerateObject())
            {
                result[property.Name] = property.Value.Clone();
            }
        }

        return result;
    }

    private static double ParseNumber(string? text)
    {
        return text != null && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ? number : double.NaN;
    }

    private static JsonElement NumberElement(double value)
    {
        // NaN has no JSON form. A string stands in so validation rejects it as a non-integer.
        return double.IsNaN(value) || double.IsInfinity(value)
            ? JsonSerializer.SerializeToElement("NaN")
            : JsonSerializer.SerializeToElement(value);
    }

    private static int? SizeInt(string[] parts, int index)
    {
        var number = ParseNumber(index < parts.Length ? parts[index] : null);
        return double.IsNaN(number) ? null : (int)number;
    }

    private static string? ConvertSizeToAspectRatio(string size)
    {
        var parts = size.Split('x');
        var width = ParseNumber(parts.Length > 0 ? parts[0] : null);
        var height = ParseNumber(parts.Length > 1 ? parts[1] : null);
        if (double.IsNaN(width) || double.IsInfinity(width) || double.IsNaN(height) || double.IsInfinity(height) || width <= 0 || height <= 0)
        {
            return null;
        }

        var divisor = Gcd(width, height);
        return Math.Round(width / divisor).ToString("0", CultureInfo.InvariantCulture)
            + ":" + Math.Round(height / divisor).ToString("0", CultureInfo.InvariantCulture);
    }

    private static double Gcd(double a, double b)
    {
        var x = Math.Abs(a);
        var y = Math.Abs(b);
        while (y != 0)
        {
            var t = y;
            y = x % y;
            x = t;
        }

        return x;
    }

    private static void Set(JsonObject target, string name, string? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    private static void Set(JsonObject target, string name, int? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    private static void Set(JsonObject target, string name, double? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    private static void Set(JsonObject target, string name, bool? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    private enum BflApi
    {
        Legacy,
        Flux3Image,
        Latest,
    }

    private sealed class PolledImage
    {
        public PolledImage(string imageUrl, JsonElement? seed, JsonElement? startTime, JsonElement? endTime, JsonElement? duration)
        {
            ImageUrl = imageUrl;
            Seed = seed;
            StartTime = startTime;
            EndTime = endTime;
            Duration = duration;
        }

        public string ImageUrl { get; }

        public JsonElement? Seed { get; }

        public JsonElement? StartTime { get; }

        public JsonElement? EndTime { get; }

        public JsonElement? Duration { get; }
    }

    /// <summary>The <c>blackForestLabs</c> provider options after validation. Unknown keys are dropped.</summary>
    private sealed class BflOptions
    {
        private static readonly string[] Schema =
        {
            "resolution", "grounding", "version", "imagePrompt", "imagePromptStrength",
            "inputImage", "inputImage2", "inputImage3", "inputImage4", "inputImage5",
            "inputImage6", "inputImage7", "inputImage8", "inputImage9", "inputImage10",
            "steps", "guidance", "width", "height", "outputFormat", "promptUpsampling", "raw",
            "safetyTolerance", "webhookSecret", "webhookUrl", "pollIntervalMillis", "pollTimeoutMillis",
        };

        private static readonly string[] Resolutions = { "768sq", "1k", "1.5k", "2k", "4k" };

        private readonly Dictionary<string, JsonElement> _values = new Dictionary<string, JsonElement>();

        /// <summary>Set options in schema order.</summary>
        public IEnumerable<string> Keys => Schema.Where(_values.ContainsKey);

        public int? Width => Integer("width");

        public int? Height => Integer("height");

        public static BflOptions Parse(IReadOnlyDictionary<string, JsonElement> raw, BflApi api)
        {
            var options = new BflOptions();
            var latest = api == BflApi.Latest;
            foreach (var key in Schema)
            {
                if (!raw.TryGetValue(key, out var value) || value.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }

                var valid = key switch
                {
                    "resolution" => value.ValueKind == JsonValueKind.String && Array.IndexOf(Resolutions, value.GetString()) >= 0,
                    "grounding" or "promptUpsampling" or "raw" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
                    "version" => value.ValueKind == JsonValueKind.String && value.GetString() == "latest",
                    "imagePrompt" or "webhookSecret" => value.ValueKind == JsonValueKind.String,
                    "webhookUrl" => value.ValueKind == JsonValueKind.String && Uri.TryCreate(value.GetString(), UriKind.Absolute, out _),
                    "imagePromptStrength" => InRange(value, 0, 1, integer: false),
                    "guidance" => InRange(value, 0, double.MaxValue, integer: false),
                    "steps" => latest ? InRange(value, 1, 16, integer: true) : InRange(value, 1, double.MaxValue, integer: true),
                    "width" or "height" => latest ? InRange(value, 64, 6256, integer: true) : InRange(value, 256, 1920, integer: true),
                    "outputFormat" => value.ValueKind == JsonValueKind.String && (value.GetString() == "jpeg" || value.GetString() == "png"),
                    "safetyTolerance" => InRange(value, 0, api == BflApi.Legacy ? 6 : 7, integer: true),
                    "pollIntervalMillis" or "pollTimeoutMillis" => InRange(value, 1, double.MaxValue, integer: true),
                    _ => value.ValueKind == JsonValueKind.String,
                };
                if (!valid)
                {
                    throw new InvalidArgumentException("providerOptions", null, "Invalid Black Forest Labs provider option " + key + ".");
                }

                options._values[key] = value;
            }

            if (latest)
            {
                var width = options.Width;
                var height = options.Height;
                if ((width == null) != (height == null))
                {
                    throw new InvalidArgumentException("providerOptions", null, "width and height must be provided together");
                }

                if (width != null && (long)width.Value * height!.Value > 16822272)
                {
                    throw new InvalidArgumentException("providerOptions", null, "width × height must not exceed 16822272 pixels");
                }
            }

            return options;
        }

        public string? String(string key)
        {
            return _values.TryGetValue(key, out var value) ? value.GetString() : null;
        }

        public bool? Boolean(string key)
        {
            return _values.TryGetValue(key, out var value) ? value.GetBoolean() : null;
        }

        public int? Integer(string key)
        {
            return _values.TryGetValue(key, out var value) ? (int)value.GetDouble() : null;
        }

        public double? Number(string key)
        {
            return _values.TryGetValue(key, out var value) ? value.GetDouble() : null;
        }

        private static bool InRange(JsonElement value, double min, double max, bool integer)
        {
            if (value.ValueKind != JsonValueKind.Number)
            {
                return false;
            }

            var number = value.GetDouble();
            return number >= min && number <= max && (!integer || (Math.Floor(number) == number && number <= int.MaxValue));
        }
    }
}
