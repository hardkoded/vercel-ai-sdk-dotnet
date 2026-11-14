// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Azure;

/// <summary>Azure Speech file transcription (MAI-Transcribe). Maps to <c>AzureSpeechTranscriptionModel</c>.</summary>
internal sealed class AzureSpeechTranscriptionModel
{
    private static readonly Dictionary<string, OptionValidator> WordShape = new(StringComparer.Ordinal)
    {
        ["text"] = ProviderOptionShape.String(),
        ["offsetMilliseconds"] = ProviderOptionShape.Nullish(ProviderOptionShape.Number()),
        ["durationMilliseconds"] = ProviderOptionShape.Nullish(ProviderOptionShape.Number()),
    };

    private static readonly Dictionary<string, OptionValidator> PhraseShape = new(StringComparer.Ordinal)
    {
        ["text"] = ProviderOptionShape.String(),
        ["offsetMilliseconds"] = ProviderOptionShape.Nullish(ProviderOptionShape.Number()),
        ["durationMilliseconds"] = ProviderOptionShape.Nullish(ProviderOptionShape.Number()),
        ["locale"] = ProviderOptionShape.Nullish(ProviderOptionShape.String()),
        ["speaker"] = ProviderOptionShape.Nullish(ProviderOptionShape.Number()),
        ["confidence"] = ProviderOptionShape.Nullish(ProviderOptionShape.Number()),
        ["words"] = ProviderOptionShape.Nullish(ProviderOptionShape.ArrayOf(ProviderOptionShape.ObjectOf(WordShape, false, "text"))),
    };

    private static readonly Dictionary<string, OptionValidator> ResponseShape = new(StringComparer.Ordinal)
    {
        ["combinedPhrases"] = ProviderOptionShape.ArrayOf(ProviderOptionShape.ObjectOf(
            new Dictionary<string, OptionValidator>(StringComparer.Ordinal) { ["text"] = ProviderOptionShape.String() },
            false,
            "text")),
        ["durationMilliseconds"] = ProviderOptionShape.Nullish(ProviderOptionShape.Number()),
        ["phrases"] = ProviderOptionShape.Nullish(ProviderOptionShape.ArrayOf(ProviderOptionShape.ObjectOf(PhraseShape, false, "text"))),
    };

    private readonly ProviderHttp _http;
    private readonly Func<Uri> _url;
    private readonly Func<IReadOnlyDictionary<string, string?>?, Dictionary<string, string?>> _headers;

    /// <summary>Creates a Speech transcription model.</summary>
    public AzureSpeechTranscriptionModel(
        string modelId,
        ProviderHttp http,
        Func<Uri> url,
        Func<IReadOnlyDictionary<string, string?>?, Dictionary<string, string?>> headers)
    {
        ModelId = modelId;
        _http = http;
        _url = url;
        _headers = headers;
    }

    /// <summary>Provider id.</summary>
    public string Provider => "azure.transcription";

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Transcribes <paramref name="call"/> with the Azure Speech API.</summary>
    public async Task<TranscriptionModelResult> DoGenerateAsync(
        TranscriptionModelCall call,
        AzureTranscriptionModelSpeechOptions azureOptions,
        CancellationToken cancellationToken)
    {
        var timestamp = DateTime.UtcNow;
        var maiModel = AzureTranscriptionModelOptions.GetMAITranscribeModel(ModelId);
        var warnings = new List<OperationWarning>();
        var timestamps = azureOptions.Timestamps;
        if (call.TimestampGranularities != null)
        {
            var wantsWord = call.TimestampGranularities.Contains("word");
            var wantsSegment = call.TimestampGranularities.Contains("segment");
            if (maiModel is { SupportsTimestamps: false } && (wantsWord || wantsSegment))
            {
                warnings.Add(OperationWarning.Unsupported("timestampGranularities", maiModel.Name + " cannot return word or segment timings."));
            }
            else if (timestamps == null)
            {
                if (wantsWord)
                {
                    timestamps = "word";
                }
                else if (wantsSegment)
                {
                    timestamps = "segment";
                }
            }
            else if ((wantsWord && timestamps != "word") || (wantsSegment && timestamps == "none"))
            {
                warnings.Add(OperationWarning.Unsupported(
                    "timestampGranularities",
                    "providerOptions.azure.timestamps is \"" + timestamps + "\" and overrides the requested timestamp granularities."));
            }
        }

        var modelOptions = new JsonObject();
        var effectiveTimestamps = timestamps ?? (maiModel is { SupportsTimestamps: false } ? null : "segment");
        if (effectiveTimestamps != null)
        {
            modelOptions["timestamps"] = effectiveTimestamps;
        }

        if (azureOptions.TranscribeStyle != null)
        {
            modelOptions["transcribeStyle"] = azureOptions.TranscribeStyle;
        }

        var definition = new JsonObject
        {
            ["enhancedMode"] = new JsonObject
            {
                ["enabled"] = true,
                ["model"] = maiModel?.Name ?? ModelId,
                ["modelOptions"] = modelOptions,
            },
        };
        if (azureOptions.Locales != null)
        {
            definition["locales"] = azureOptions.Locales.DeepClone();
        }

        if (azureOptions.Diarization != null)
        {
            definition["diarization"] = azureOptions.Diarization.DeepClone();
        }

        if (azureOptions.PhraseList != null)
        {
            definition["phraseList"] = azureOptions.PhraseList.DeepClone();
        }

        using var formData = new MultipartFormDataContent();
        var mediaType = string.IsNullOrEmpty(call.MediaType) ? "audio/mpeg" : call.MediaType;
        var file = new ByteArrayContent(call.Audio);
        file.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        formData.Add(file, "audio", "audio." + OpenAIJson.Extension(mediaType));
        var definitionPart = new StringContent(definition.ToJsonString(), Encoding.UTF8);
        definitionPart.Headers.ContentType = null;
        formData.Add(definitionPart, "definition");

        var callHeaders = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in call.Headers)
        {
            callHeaders[pair.Key] = pair.Value;
        }

        var response = await _http.SendBinaryAsync(HttpMethod.Post, _url(), formData, _headers(callHeaders), cancellationToken).ConfigureAwait(false);
        var body = Encoding.UTF8.GetString(response.Body);
        using var document = OpenAIJson.ParseResponse(body);
        var root = document.RootElement;
        var error = ProviderOptionShape.Validate(root, ResponseShape, false, "combinedPhrases");
        if (error != null)
        {
            throw new ApiException("Invalid JSON response: " + error, 200, body);
        }

        var phrases = root.TryGetProperty("phrases", out var phraseItems) && phraseItems.ValueKind == JsonValueKind.Array
            ? phraseItems.EnumerateArray().ToList()
            : new List<JsonElement>();

        // Consider every reported locale so mixed results (for example en + fil) are not collapsed to one language;
        // only ISO-639-1 codes are reported.
        var languages = new List<string>();
        foreach (var phrase in phrases)
        {
            var locale = OpenAIJson.String(phrase, "locale");
            if (!string.IsNullOrEmpty(locale))
            {
                var code = locale!.Split('-')[0].ToLowerInvariant();
                if (!languages.Contains(code))
                {
                    languages.Add(code);
                }
            }
        }

        var reportedLanguage = languages.Count == 1 && languages[0].Length == 2 && languages[0].All(letter => letter is >= 'a' and <= 'z')
            ? languages[0]
            : null;

        var words = new List<TranscriptSegment>();
        var segments = new List<TranscriptSegment>();
        foreach (var phrase in phrases)
        {
            if (Timing(phrase) is { } phraseTiming)
            {
                segments.Add(new TranscriptSegment(OpenAIJson.String(phrase, "text") ?? string.Empty, phraseTiming.Start, phraseTiming.End));
            }

            if (phrase.TryGetProperty("words", out var wordItems) && wordItems.ValueKind == JsonValueKind.Array)
            {
                foreach (var word in wordItems.EnumerateArray())
                {
                    if (Timing(word) is { } wordTiming)
                    {
                        words.Add(new TranscriptSegment(OpenAIJson.String(word, "text") ?? string.Empty, wordTiming.Start, wordTiming.End));
                    }
                }
            }
        }

        var text = string.Join(" ", root.GetProperty("combinedPhrases").EnumerateArray().Select(phrase => phrase.GetProperty("text").GetString()));
        var duration = OpenAIJson.Double(root, "durationMilliseconds");
        var metadata = new JsonObject
        {
            ["azure"] = new JsonObject { ["phrases"] = new JsonArray(phrases.Select(Phrase).ToArray()) },
        };
        return new TranscriptionModelResult(
            text,
            segments,
            reportedLanguage,
            duration is { } milliseconds ? milliseconds / 1000 : null,
            warnings,
            OpenAIJson.Element(metadata),
            new ProviderResponse(response.Headers, root.Clone(), timestamp: timestamp, modelId: ModelId),
            words.Count > 0 ? words : null);
    }

    private static (double Start, double End)? Timing(JsonElement element)
    {
        var offset = OpenAIJson.Double(element, "offsetMilliseconds");
        var duration = OpenAIJson.Double(element, "durationMilliseconds");
        return offset is { } start && duration is { } length ? (start / 1000, (start + length) / 1000) : null;
    }

    // Zod strips unknown keys, so only the documented phrase fields reach the provider metadata.
    private static JsonNode Phrase(JsonElement phrase)
    {
        var projected = new JsonObject();
        foreach (var name in new[] { "text", "offsetMilliseconds", "durationMilliseconds", "locale", "speaker", "confidence" })
        {
            if (phrase.TryGetProperty(name, out var value))
            {
                projected[name] = JsonNode.Parse(value.GetRawText());
            }
        }

        if (phrase.TryGetProperty("words", out var words))
        {
            if (words.ValueKind == JsonValueKind.Array)
            {
                var items = new JsonArray();
                foreach (var word in words.EnumerateArray())
                {
                    var item = new JsonObject();
                    foreach (var name in new[] { "text", "offsetMilliseconds", "durationMilliseconds" })
                    {
                        if (word.TryGetProperty(name, out var value))
                        {
                            item[name] = JsonNode.Parse(value.GetRawText());
                        }
                    }

                    items.Add(item);
                }

                projected["words"] = items;
            }
            else
            {
                projected["words"] = null;
            }
        }

        return projected;
    }
}
