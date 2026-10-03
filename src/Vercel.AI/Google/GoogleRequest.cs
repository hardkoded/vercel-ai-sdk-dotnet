// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>A generateContent request body plus the warnings and extra headers that go with it.</summary>
public sealed class GooglePreparedRequest
{
    /// <summary>Creates a prepared request.</summary>
    public GooglePreparedRequest(JsonObject body, IReadOnlyList<GoogleWarning> warnings, IReadOnlyDictionary<string, string?> extraHeaders, GoogleToolPreparation tools)
    {
        Body = body;
        Warnings = warnings;
        ExtraHeaders = extraHeaders;
        Tools = tools;
    }

    /// <summary>Request JSON.</summary>
    public JsonObject Body { get; }

    /// <summary>Preparation warnings.</summary>
    public IReadOnlyList<GoogleWarning> Warnings { get; }

    /// <summary>Headers added for this call, such as Vertex pay-as-you-go request types.</summary>
    public IReadOnlyDictionary<string, string?> ExtraHeaders { get; }

    /// <summary>Prepared tools, including built-in name mapping.</summary>
    public GoogleToolPreparation Tools { get; }
}

/// <summary>Builds a Gemini <c>generateContent</c> body.</summary>
public static class GoogleRequest
{
    private static readonly string[] SafetyCategories =
    {
        "HARM_CATEGORY_HATE_SPEECH",
        "HARM_CATEGORY_DANGEROUS_CONTENT",
        "HARM_CATEGORY_HARASSMENT",
        "HARM_CATEGORY_SEXUALLY_EXPLICIT",
    };

    private static readonly Regex Gemini25 = new(@"(^|/)gemini-2\.5(?:[.-]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    /// <summary>Builds the request for a language-model call.</summary>
    public static GooglePreparedRequest Prepare(string modelId, string provider, LanguageModelCallOptions options, bool streaming)
    {
        options ??= new LanguageModelCallOptions();
        var warnings = new List<GoogleWarning>();
        var vertex = provider != null && provider.IndexOf("vertex", StringComparison.OrdinalIgnoreCase) >= 0;
        var names = vertex ? new[] { "googleVertex", "vertex" } : new[] { "google" };
        var google = ReadOptions(options.ProviderOptions, names, vertex);
        var providerTools = ReadProviderTools(google);

        if (!vertex && HasTool(providerTools, "google.vertex_rag_store"))
        {
            warnings.Add(new GoogleWarning("other", "The 'vertex_rag_store' tool is only supported with the Google Vertex provider and might not be supported or could behave unexpectedly with the current Google provider (" + provider + ")."));
        }

        var streamArgs = Bool(google, "streamFunctionCallArguments");
        if (streamArgs == true && !vertex)
        {
            warnings.Add(new GoogleWarning("other", "'streamFunctionCallArguments' is only supported on the Vertex AI API and will be ignored with the current Google provider (" + provider + ")."));
        }

        var serviceTier = GoogleJson.String(google, "serviceTier");
        if (!string.IsNullOrEmpty(serviceTier) && vertex)
        {
            warnings.Add(new GoogleWarning("other", "'serviceTier' is a Gemini API option and is not supported on Vertex AI. Use 'sharedRequestType' (and optionally 'requestType') instead."));
            serviceTier = null;
        }

        var shared = GoogleJson.String(google, "sharedRequestType");
        var requestType = GoogleJson.String(google, "requestType");
        if ((!string.IsNullOrEmpty(shared) || !string.IsNullOrEmpty(requestType)) && !vertex)
        {
            warnings.Add(new GoogleWarning("other", "'sharedRequestType' and 'requestType' are Vertex AI options and are ignored with the current Google provider (" + provider + ")."));
            shared = null;
            requestType = null;
        }

        var extra = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (vertex && !string.IsNullOrEmpty(shared))
        {
            extra["X-Vertex-AI-LLM-Shared-Request-Type"] = shared;
        }

        if (vertex && !string.IsNullOrEmpty(requestType))
        {
            extra["X-Vertex-AI-LLM-Request-Type"] = requestType;
        }

        var imageConfig = ImageConfig(google, vertex, provider!, warnings);
        var developer25 = !vertex && Gemini25.IsMatch(modelId ?? string.Empty);
        if (developer25 && options.FrequencyPenalty != null)
        {
            warnings.Add(GoogleWarning.Unsupported("frequencyPenalty"));
        }

        if (developer25 && options.PresencePenalty != null)
        {
            warnings.Add(GoogleWarning.Unsupported("presencePenalty"));
        }

        var messageOptions = new GoogleMessageOptions
        {
            ModelId = modelId ?? string.Empty,
            IncludeFunctionCallIds = !vertex,
            ProviderOptionNames = names,
        };
        var prompt = GoogleMessages.Convert(options.Prompt, messageOptions);
        warnings.AddRange(messageOptions.Warnings);

        var tools = GoogleTools.Prepare(options.Tools, providerTools, options.ToolChoice, modelId ?? string.Empty, vertex);
        warnings.AddRange(tools.Warnings);

        var resolved = GoogleThinking.Resolve(options.Reasoning, modelId ?? string.Empty, warnings);
        JsonObject? thinking = resolved;
        if (GoogleJson.TryObject(google, "thinkingConfig", out var thinkingOverride))
        {
            thinking = Merge(resolved, thinkingOverride);
        }

        var config = new JsonObject();
        GoogleJson.Set(config, "maxOutputTokens", options.MaxOutputTokens);
        GoogleJson.Set(config, "temperature", options.Temperature);
        GoogleJson.Set(config, "topK", options.TopK);
        GoogleJson.Set(config, "topP", options.TopP);
        if (!developer25)
        {
            GoogleJson.Set(config, "frequencyPenalty", options.FrequencyPenalty);
            GoogleJson.Set(config, "presencePenalty", options.PresencePenalty);
        }

        if (options.StopSequences is { Count: > 0 })
        {
            var stops = new JsonArray();
            foreach (var stop in options.StopSequences)
            {
                stops.Add(stop);
            }

            config["stopSequences"] = stops;
        }

        GoogleJson.Set(config, "seed", options.Seed);
        if (options.JsonSchema != null)
        {
            config["responseMimeType"] = "application/json";
            var structured = google.ValueKind != JsonValueKind.Object || !google.TryGetProperty("structuredOutputs", out var flag) || flag.ValueKind != JsonValueKind.False;
            if (structured)
            {
                config["responseJsonSchema"] = GoogleJsonSchema.Sanitize(options.JsonSchema.Value);
            }
        }

        GoogleJson.Set(config, "audioTimestamp", Bool(google, "audioTimestamp"));
        if (google.ValueKind == JsonValueKind.Object && google.TryGetProperty("responseModalities", out var modalities) && modalities.ValueKind == JsonValueKind.Array)
        {
            config["responseModalities"] = JsonNode.Parse(modalities.GetRawText());
        }

        if (thinking != null)
        {
            config["thinkingConfig"] = thinking;
        }

        GoogleJson.Set(config, "mediaResolution", GoogleJson.String(google, "mediaResolution"));
        if (imageConfig != null)
        {
            config["imageConfig"] = imageConfig;
        }

        JsonObject? toolConfig = tools.ToolConfig?.DeepClone() as JsonObject;
        var streamFunctionCalls = streaming && vertex && streamArgs == true;
        if (streamFunctionCalls)
        {
            toolConfig ??= new JsonObject();
            var calling = toolConfig["functionCallingConfig"] as JsonObject ?? new JsonObject();
            calling["streamFunctionCallArguments"] = true;
            toolConfig["functionCallingConfig"] = calling;
        }

        if (GoogleJson.TryObject(google, "retrievalConfig", out var retrieval))
        {
            toolConfig ??= new JsonObject();
            toolConfig["retrievalConfig"] = JsonNode.Parse(retrieval.GetRawText());
        }

        var body = new JsonObject
        {
            ["generationConfig"] = config,
            ["contents"] = prompt.Contents,
        };
        if (prompt.SystemInstruction != null)
        {
            body["systemInstruction"] = prompt.SystemInstruction;
        }

        if (toolConfig != null)
        {
            body["toolConfig"] = toolConfig;
        }

        if (tools.Tools != null)
        {
            body["tools"] = tools.Tools;
        }

        var safety = Safety(google);
        if (safety != null)
        {
            body["safetySettings"] = safety;
        }

        GoogleJson.Set(body, "cachedContent", GoogleJson.String(google, "cachedContent"));
        if (GoogleJson.TryObject(google, "labels", out var labels))
        {
            body["labels"] = JsonNode.Parse(labels.GetRawText());
        }

        GoogleJson.Set(body, "serviceTier", vertex ? null : serviceTier);
        return new GooglePreparedRequest(body, warnings, extra, tools);
    }

    private static JsonObject? ImageConfig(JsonElement google, bool vertex, string provider, List<GoogleWarning> warnings)
    {
        if (!GoogleJson.TryObject(google, "imageConfig", out var image))
        {
            return null;
        }

        var clone = (JsonObject)JsonNode.Parse(image.GetRawText())!;
        if (vertex)
        {
            return clone;
        }

        var dropped = new List<string>();
        foreach (var key in new[] { "personGeneration", "prominentPeople", "imageOutputOptions" })
        {
            if (clone.ContainsKey(key) && clone[key] != null)
            {
                dropped.Add("'imageConfig." + key + "'");
                clone.Remove(key);
            }
        }

        if (dropped.Count > 0)
        {
            var verb = dropped.Count == 1 ? "is a Vertex AI option and is" : "are Vertex AI options and are";
            warnings.Add(new GoogleWarning("other", string.Join(", ", dropped) + " " + verb + " ignored with the current Google provider (" + provider + ")."));
        }

        return clone.Count == 0 ? null : clone;
    }

    private static JsonArray? Safety(JsonElement google)
    {
        if (google.ValueKind == JsonValueKind.Object && google.TryGetProperty("safetySettings", out var settings) && settings.ValueKind == JsonValueKind.Array)
        {
            return JsonNode.Parse(settings.GetRawText()) as JsonArray;
        }

        var threshold = GoogleJson.String(google, "threshold");
        if (string.IsNullOrEmpty(threshold))
        {
            return null;
        }

        var array = new JsonArray();
        foreach (var category in SafetyCategories)
        {
            array.Add(new JsonObject { ["category"] = category, ["threshold"] = threshold });
        }

        return array;
    }

    private static JsonObject Merge(JsonObject? resolved, JsonElement overlay)
    {
        var result = resolved?.DeepClone() as JsonObject ?? new JsonObject();
        foreach (var property in overlay.EnumerateObject())
        {
            result[property.Name] = JsonNode.Parse(property.Value.GetRawText());
        }

        return result;
    }

    private static JsonElement ReadOptions(IReadOnlyDictionary<string, JsonElement>? options, IReadOnlyList<string> names, bool vertex)
    {
        if (options != null)
        {
            for (var i = 0; i < names.Count; i++)
            {
                if (options.TryGetValue(names[i], out var value))
                {
                    return value;
                }
            }

            if (vertex && options.TryGetValue("google", out var google))
            {
                return google;
            }
        }

        return default;
    }

    private static List<GoogleProviderTool> ReadProviderTools(JsonElement google)
    {
        var tools = new List<GoogleProviderTool>();
        if (google.ValueKind != JsonValueKind.Object || !google.TryGetProperty("providerTools", out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return tools;
        }

        foreach (var item in array.EnumerateArray())
        {
            var id = GoogleJson.String(item, "id");
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            JsonElement? args = item.TryGetProperty("args", out var value) ? value : null;
            tools.Add(new GoogleProviderTool(id!, args, GoogleJson.String(item, "name")));
        }

        return tools;
    }

    private static bool HasTool(IReadOnlyList<GoogleProviderTool> tools, string id)
    {
        for (var i = 0; i < tools.Count; i++)
        {
            if (tools[i].Id == id)
            {
                return true;
            }
        }

        return false;
    }

    private static bool? Bool(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.True)
        {
            return true;
        }

        if (value.ValueKind == JsonValueKind.False)
        {
            return false;
        }

        return null;
    }
}
