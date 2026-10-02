// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>Text or reasoning that carries a Gemini thought signature.</summary>
public sealed class GoogleSignedPart : GeneratedContent
{
    /// <summary>Creates a signed part.</summary>
    public GoogleSignedPart(string partType, string text, JsonElement providerMetadata)
        : base(partType)
    {
        Text = text ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Part text.</summary>
    public string Text { get; }

    /// <summary>Provider metadata, keyed by <c>google</c> or <c>vertex</c>.</summary>
    public JsonElement ProviderMetadata { get; }
}

/// <summary>A provider-executed tool call, such as code execution or a server tool.</summary>
public sealed class GoogleProviderToolCall : GeneratedContent
{
    /// <summary>Creates a provider-executed tool call.</summary>
    public GoogleProviderToolCall(string toolCallId, string toolName, string argumentsJson, JsonElement? providerMetadata)
        : base("tool-call")
    {
        ToolCallId = toolCallId ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        ArgumentsJson = argumentsJson ?? "{}";
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON arguments.</summary>
    public string ArgumentsJson { get; }

    /// <summary>Always true. The provider already ran this tool.</summary>
    public bool ProviderExecuted
    {
        get { return true; }
    }

    /// <summary>Provider metadata for the call.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A tool result returned by Gemini, including code execution output.</summary>
public sealed class GoogleToolResultContent : GeneratedContent
{
    /// <summary>Creates a tool result.</summary>
    public GoogleToolResultContent(string toolCallId, string toolName, string resultJson, JsonElement? providerMetadata)
        : base("tool-result")
    {
        ToolCallId = toolCallId ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        ResultJson = resultJson ?? "{}";
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON result.</summary>
    public string ResultJson { get; }

    /// <summary>Provider metadata for the result.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A generated file that may be a thought or carry a thought signature.</summary>
public sealed class GoogleGeneratedFile : GeneratedContent
{
    /// <summary>Creates a file part.</summary>
    public GoogleGeneratedFile(byte[] data, string mediaType, bool thought, JsonElement? providerMetadata)
        : base(thought ? "reasoning-file" : "file")
    {
        Data = data ?? Array.Empty<byte>();
        MediaType = mediaType ?? "application/octet-stream";
        Thought = thought;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>File bytes.</summary>
    public byte[] Data { get; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }

    /// <summary>Whether Gemini marked the file as a thought.</summary>
    public bool Thought { get; }

    /// <summary>Thought signature metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A streamed tool result.</summary>
public sealed class GoogleToolResultStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a streamed tool result.</summary>
    public GoogleToolResultStreamPart(string toolCallId, string toolName, string resultJson, JsonElement? providerMetadata)
        : base("tool-result")
    {
        ToolCallId = toolCallId ?? string.Empty;
        ToolName = toolName ?? string.Empty;
        ResultJson = resultJson ?? "{}";
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON result.</summary>
    public string ResultJson { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A text or reasoning delta that carries a thought signature.</summary>
public sealed class GoogleSignedDelta : LanguageModelStreamPart
{
    /// <summary>Creates a signed delta.</summary>
    public GoogleSignedDelta(string partType, string id, string delta, JsonElement providerMetadata)
        : base(partType)
    {
        Id = id ?? string.Empty;
        Delta = delta ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Block id.</summary>
    public string Id { get; }

    /// <summary>Delta text.</summary>
    public string Delta { get; }

    /// <summary>Thought signature metadata.</summary>
    public JsonElement ProviderMetadata { get; }
}

/// <summary>A streamed file.</summary>
public sealed class GoogleFileStreamPart : LanguageModelStreamPart
{
    /// <summary>Creates a streamed file.</summary>
    public GoogleFileStreamPart(byte[] data, string mediaType, bool thought, JsonElement? providerMetadata)
        : base(thought ? "reasoning-file" : "file")
    {
        Data = data ?? Array.Empty<byte>();
        MediaType = mediaType ?? "application/octet-stream";
        Thought = thought;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>File bytes.</summary>
    public byte[] Data { get; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }

    /// <summary>Whether the file is a thought.</summary>
    public bool Thought { get; }

    /// <summary>Thought signature metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

internal sealed class GooglePreparedCall
{
    public GooglePreparedCall(JsonObject body, Dictionary<string, string?> headers, List<CallWarning> warnings, string[] metadataNames, string codeExecutionName)
    {
        Body = body;
        Headers = headers;
        Warnings = warnings;
        MetadataNames = metadataNames;
        CodeExecutionName = codeExecutionName;
    }

    public JsonObject Body { get; }

    public Dictionary<string, string?> Headers { get; }

    public List<CallWarning> Warnings { get; }

    public string[] MetadataNames { get; }

    public string CodeExecutionName { get; }
}

/// <summary>Builds and reads Gemini <c>generateContent</c> calls.</summary>
public static class GoogleGenerate
{
    private static readonly Regex Gemini25 = new Regex(@"(^|/)gemini-2\.5(?:[.-]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex FlashMinimum = new Regex(@"^gemini-(\d+)\.(\d+)-flash(?:$|-(?!lite(?:-|$)))", RegexOptions.CultureInvariant);
    private static readonly string[] SafetyCategories =
    {
        "HARM_CATEGORY_HATE_SPEECH",
        "HARM_CATEGORY_DANGEROUS_CONTENT",
        "HARM_CATEGORY_HARASSMENT",
        "HARM_CATEGORY_SEXUALLY_EXPLICIT",
    };

    internal static GooglePreparedCall Prepare(GoogleProvider provider, string modelId, LanguageModelCallOptions options, bool streaming)
    {
        options = options ?? new LanguageModelCallOptions();
        var warnings = new List<CallWarning>();
        var vertex = provider.Name.StartsWith("google.vertex.", StringComparison.Ordinal);
        var metadataNames = provider.Name.IndexOf("vertex", StringComparison.Ordinal) >= 0
            ? new[] { "googleVertex", "vertex" }
            : new[] { "google" };
        JsonElement googleOptions = default;
        var hasOptions = TryOptions(options, metadataNames, vertex, out googleOptions);
        var specs = CollectTools(options, hasOptions ? googleOptions : default);
        if (!vertex)
        {
            foreach (var spec in specs)
            {
                if (spec.Id == "google.vertex_rag_store")
                {
                    warnings.Add(new CallWarning(
                        "other",
                        "The 'vertex_rag_store' tool is only supported with the Google Vertex provider " +
                        "and might not be supported or could behave unexpectedly with the current Google provider " +
                        "(" + provider.Name + ")."));
                    break;
                }
            }
        }

        var streamArguments = hasOptions && Bool(googleOptions, "streamFunctionCallArguments") == true;
        if (streamArguments && !vertex)
        {
            warnings.Add(new CallWarning(
                "other",
                "'streamFunctionCallArguments' is only supported on the Vertex AI API " +
                "and will be ignored with the current Google provider " +
                "(" + provider.Name + "). See https://docs.cloud.google.com/vertex-ai/generative-ai/docs/multimodal/function-calling#streaming-fc"));
        }

        var serviceTier = hasOptions ? EnumValue(googleOptions, "serviceTier", "standard", "flex", "priority") : null;
        if (serviceTier != null && vertex)
        {
            warnings.Add(new CallWarning(
                "other",
                "'serviceTier' is a Gemini API option and is not supported on Vertex AI. " +
                "Use 'sharedRequestType' (and optionally 'requestType') instead. See " +
                "https://docs.cloud.google.com/vertex-ai/generative-ai/docs/priority-paygo"));
            serviceTier = null;
        }

        var sharedRequest = hasOptions ? EnumValue(googleOptions, "sharedRequestType", "priority", "flex", "standard") : null;
        var requestType = hasOptions ? EnumValue(googleOptions, "requestType", "shared") : null;
        if ((sharedRequest != null || requestType != null) && !vertex)
        {
            warnings.Add(new CallWarning(
                "other",
                "'sharedRequestType' and 'requestType' are Vertex AI options and " +
                "are ignored with the current Google provider (" + provider.Name + ")."));
            sharedRequest = null;
            requestType = null;
        }

        var preparedTools = GoogleTools.Prepare(specs, options.ToolChoice, modelId, vertex);
        warnings.AddRange(preparedTools.Warnings);
        var prompt = GoogleMessages.Convert(options.Prompt, modelId, vertex);
        warnings.AddRange(prompt.Warnings);
        var developer25 = !vertex && Gemini25.IsMatch(modelId ?? string.Empty);
        if (developer25 && options.FrequencyPenalty != null)
        {
            warnings.Add(new CallWarning("unsupported", "frequencyPenalty"));
        }

        if (developer25 && options.PresencePenalty != null)
        {
            warnings.Add(new CallWarning("unsupported", "presencePenalty"));
        }

        var generation = new JsonObject();
        if (options.MaxOutputTokens is { } maxOutput)
        {
            generation["maxOutputTokens"] = maxOutput;
        }

        if (options.Temperature is { } temperature)
        {
            generation["temperature"] = temperature;
        }

        if (options.TopK is { } topK)
        {
            generation["topK"] = topK;
        }

        if (options.TopP is { } topP)
        {
            generation["topP"] = topP;
        }

        if (!developer25 && options.FrequencyPenalty is { } frequency)
        {
            generation["frequencyPenalty"] = frequency;
        }

        if (!developer25 && options.PresencePenalty is { } presence)
        {
            generation["presencePenalty"] = presence;
        }

        if (options.StopSequences is { Count: > 0 })
        {
            var stops = new JsonArray();
            foreach (var stop in options.StopSequences)
            {
                stops.Add(stop);
            }

            generation["stopSequences"] = stops;
        }

        if (options.Seed is { } seed)
        {
            generation["seed"] = seed;
        }

        if (options.JsonSchema is { } schema && schema.ValueKind == JsonValueKind.Object)
        {
            generation["responseMimeType"] = "application/json";
            var structured = hasOptions ? Bool(googleOptions, "structuredOutputs") : null;
            if (structured != false)
            {
                generation["responseJsonSchema"] = GoogleJsonSchema.Sanitize(schema);
            }
        }

        if (hasOptions && Bool(googleOptions, "audioTimestamp") == true)
        {
            generation["audioTimestamp"] = true;
        }

        if (hasOptions && googleOptions.TryGetProperty("responseModalities", out var modalities) && modalities.ValueKind == JsonValueKind.Array)
        {
            var values = new JsonArray();
            var valid = true;
            foreach (var item in modalities.EnumerateArray())
            {
                var text = item.GetString();
                if (text != "TEXT" && text != "IMAGE")
                {
                    valid = false;
                    break;
                }

                values.Add(text);
            }

            if (valid && values.Count > 0)
            {
                generation["responseModalities"] = values;
            }
        }

        var thinking = ThinkingConfig(options.Reasoning, modelId ?? string.Empty, hasOptions ? googleOptions : default, hasOptions, warnings);
        if (thinking != null)
        {
            generation["thinkingConfig"] = thinking;
        }

        var mediaResolution = hasOptions ? EnumValue(googleOptions, "mediaResolution", "MEDIA_RESOLUTION_UNSPECIFIED", "MEDIA_RESOLUTION_LOW", "MEDIA_RESOLUTION_MEDIUM", "MEDIA_RESOLUTION_HIGH") : null;
        if (mediaResolution != null)
        {
            generation["mediaResolution"] = mediaResolution;
        }

        var image = ImageConfig(hasOptions ? googleOptions : default, hasOptions, vertex, provider.Name, warnings);
        if (image != null)
        {
            generation["imageConfig"] = image;
        }

        var body = new JsonObject
        {
            ["generationConfig"] = generation,
            ["contents"] = prompt.Contents,
        };
        if (prompt.SystemInstruction != null)
        {
            body["systemInstruction"] = prompt.SystemInstruction;
        }

        var safety = Safety(hasOptions ? googleOptions : default, hasOptions);
        if (safety != null)
        {
            body["safetySettings"] = safety;
        }

        if (preparedTools.Tools != null)
        {
            body["tools"] = preparedTools.Tools;
        }

        var toolConfig = preparedTools.ToolConfig;
        if (streaming && vertex && streamArguments)
        {
            toolConfig ??= new JsonObject();
            var functionConfig = toolConfig["functionCallingConfig"] as JsonObject ?? new JsonObject();
            functionConfig["streamFunctionCallArguments"] = true;
            toolConfig["functionCallingConfig"] = functionConfig;
        }

        if (hasOptions && googleOptions.TryGetProperty("retrievalConfig", out var retrieval) && retrieval.ValueKind == JsonValueKind.Object)
        {
            toolConfig ??= new JsonObject();
            toolConfig["retrievalConfig"] = Retrieval(retrieval);
        }

        if (toolConfig != null)
        {
            body["toolConfig"] = toolConfig;
        }

        if (hasOptions && googleOptions.TryGetProperty("cachedContent", out var cached) && cached.ValueKind == JsonValueKind.String)
        {
            body["cachedContent"] = cached.GetString();
        }

        if (hasOptions && googleOptions.TryGetProperty("labels", out var labels) && labels.ValueKind == JsonValueKind.Object)
        {
            var copy = new JsonObject();
            foreach (var property in labels.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    copy[property.Name] = property.Value.GetString();
                }
            }

            body["labels"] = copy;
        }

        if (serviceTier != null)
        {
            body["serviceTier"] = serviceTier;
        }

        var headers = provider.Headers();
        if (options.Headers != null)
        {
            foreach (var pair in options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        if (vertex && sharedRequest != null)
        {
            headers["X-Vertex-AI-LLM-Shared-Request-Type"] = sharedRequest;
        }

        if (vertex && requestType != null)
        {
            headers["X-Vertex-AI-LLM-Request-Type"] = requestType;
        }

        return new GooglePreparedCall(body, headers, warnings, metadataNames, preparedTools.CodeExecutionName);
    }

    internal static LanguageModelGenerateResult Read(JsonElement root, GooglePreparedCall prepared, Func<string> generateId)
    {
        var content = new List<GeneratedContent>();
        JsonElement candidate = default;
        var hasCandidate = root.TryGetProperty("candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array && candidates.GetArrayLength() > 0;
        if (hasCandidate)
        {
            candidate = candidates[0];
        }

        string? finish = null;
        if (hasCandidate && candidate.TryGetProperty("finishReason", out var finishElement) && finishElement.ValueKind == JsonValueKind.String)
        {
            finish = finishElement.GetString();
        }

        string? block = null;
        JsonNode? promptFeedback = null;
        if (root.TryGetProperty("promptFeedback", out var feedback) && feedback.ValueKind == JsonValueKind.Object)
        {
            promptFeedback = GoogleJson.Clone(feedback);
            if (feedback.TryGetProperty("blockReason", out var blockElement) && blockElement.ValueKind == JsonValueKind.String)
            {
                block = blockElement.GetString();
            }
        }

        var confirmed = GoogleFinishReason.IsConfirmedPromptBlock(block);
        var promptBlocked = finish == null && confirmed;
        var raw = finish ?? (confirmed ? block : null);
        string? codeId = null;
        string? serverId = null;
        if (hasCandidate && candidate.TryGetProperty("content", out var candidateContent) && candidateContent.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in parts.EnumerateArray())
            {
                ReadPart(part, content, prepared, generateId, ref codeId, ref serverId);
            }
        }

        if (hasCandidate)
        {
            foreach (var source in Sources(candidate, generateId))
            {
                content.Add(source);
            }
        }

        var hasToolCalls = false;
        foreach (var part in content)
        {
            if (part is GeneratedToolCall)
            {
                hasToolCalls = true;
            }
        }

        var usageElement = root.TryGetProperty("usageMetadata", out var usageNode) ? usageNode : default;
        var usage = usageElement.ValueKind == JsonValueKind.Object
            ? GoogleUsageConverter.Convert(usageElement)
            : new LanguageModelUsage(null, null, null);
        JsonNode? grounding = null;
        JsonNode? urlContext = null;
        JsonNode? safety = null;
        string? finishMessage = null;
        if (hasCandidate)
        {
            if (candidate.TryGetProperty("groundingMetadata", out var groundingElement) && groundingElement.ValueKind == JsonValueKind.Object)
            {
                grounding = GoogleJson.Clone(groundingElement);
            }

            if (candidate.TryGetProperty("urlContextMetadata", out var urlElement) && urlElement.ValueKind == JsonValueKind.Object)
            {
                urlContext = GoogleJson.Clone(urlElement);
            }

            if (candidate.TryGetProperty("safetyRatings", out var safetyElement) && safetyElement.ValueKind == JsonValueKind.Array)
            {
                safety = GoogleJson.Clone(safetyElement);
            }

            if (candidate.TryGetProperty("finishMessage", out var messageElement) && messageElement.ValueKind == JsonValueKind.String)
            {
                finishMessage = messageElement.GetString();
            }
        }

        string? serviceTier = null;
        if (usageElement.ValueKind == JsonValueKind.Object && usageElement.TryGetProperty("serviceTier", out var tier) && tier.ValueKind == JsonValueKind.String)
        {
            serviceTier = tier.GetString();
        }

        var metadata = Metadata(prepared.MetadataNames, promptFeedback, grounding, urlContext, safety, usageElement.ValueKind == JsonValueKind.Object ? GoogleJson.Clone(usageElement) : null, finishMessage, serviceTier);
        string? responseId = root.TryGetProperty("responseId", out var idElement) && idElement.ValueKind == JsonValueKind.String ? idElement.GetString() : null;
        return new LanguageModelGenerateResult(
            content,
            promptBlocked ? FinishReason.ContentFilter : GoogleFinishReason.Map(raw, hasToolCalls),
            usage,
            raw,
            prepared.Warnings,
            responseId,
            metadata,
            root.GetRawText());
    }

    private static void ReadPart(JsonElement part, List<GeneratedContent> content, GooglePreparedCall prepared, Func<string> generateId, ref string? codeId, ref string? serverId)
    {
        if (part.TryGetProperty("executableCode", out var executable) && executable.ValueKind == JsonValueKind.Object && executable.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(code.GetString()))
        {
            codeId = generateId();
            content.Add(new GoogleProviderToolCall(codeId, prepared.CodeExecutionName, executable.GetRawText(), null));
            return;
        }

        if (part.TryGetProperty("codeExecutionResult", out var result) && result.ValueKind == JsonValueKind.Object)
        {
            var output = result.TryGetProperty("output", out var outputElement) && outputElement.ValueKind == JsonValueKind.String ? outputElement.GetString() : string.Empty;
            var outcome = result.TryGetProperty("outcome", out var outcomeElement) ? outcomeElement.GetString() : null;
            var json = new JsonObject { ["outcome"] = outcome, ["output"] = output ?? string.Empty }.ToJsonString();
            content.Add(new GoogleToolResultContent(codeId ?? string.Empty, prepared.CodeExecutionName, json, null));
            return;
        }

        if (part.TryGetProperty("text", out var textElement) && textElement.ValueKind != JsonValueKind.Null && textElement.ValueKind != JsonValueKind.Undefined)
        {
            var text = textElement.GetString() ?? string.Empty;
            var thought = part.TryGetProperty("thought", out var thoughtElement) && thoughtElement.ValueKind == JsonValueKind.True;
            JsonElement? signature = Signature(part, prepared.MetadataNames);
            if (text.Length == 0)
            {
                if (signature != null)
                {
                    content.Add(new GoogleSignedPart(thought ? "reasoning" : "text", string.Empty, signature.Value));
                }

                return;
            }

            if (thought)
            {
                content.Add(new GeneratedReasoning(text));
            }
            else
            {
                content.Add(new GeneratedText(text));
            }

            if (signature != null)
            {
                content.Add(new GoogleSignedPart(thought ? "reasoning" : "text", text, signature.Value));
            }

            return;
        }

        if (part.TryGetProperty("functionCall", out var call) && call.ValueKind == JsonValueKind.Object && call.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(nameElement.GetString()))
        {
            var id = call.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String ? idElement.GetString() : null;
            if (string.IsNullOrEmpty(id))
            {
                id = generateId();
            }

            var args = call.TryGetProperty("args", out var argsElement) && argsElement.ValueKind != JsonValueKind.Null && argsElement.ValueKind != JsonValueKind.Undefined
                ? argsElement.ValueKind == JsonValueKind.String ? argsElement.GetString() ?? "{}" : argsElement.GetRawText()
                : "{}";
            content.Add(new GeneratedToolCall(id!, nameElement.GetString()!, args, Signature(part, prepared.MetadataNames)));
            return;
        }

        if (part.TryGetProperty("inlineData", out var inline) && inline.ValueKind == JsonValueKind.Object)
        {
            var mime = inline.TryGetProperty("mimeType", out var mimeElement) ? mimeElement.GetString() ?? "application/octet-stream" : "application/octet-stream";
            var bytes = Array.Empty<byte>();
            if (inline.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.String)
            {
                try
                {
                    bytes = Convert.FromBase64String(dataElement.GetString() ?? string.Empty);
                }
                catch (FormatException)
                {
                    bytes = Array.Empty<byte>();
                }
            }

            var thought = part.TryGetProperty("thought", out var thoughtElement) && thoughtElement.ValueKind == JsonValueKind.True;
            var signature = Signature(part, prepared.MetadataNames);
            if (thought || signature != null)
            {
                content.Add(new GoogleGeneratedFile(bytes, mime, thought, signature));
            }
            else
            {
                content.Add(new GeneratedFile(bytes, mime));
            }

            return;
        }

        if (part.TryGetProperty("toolCall", out var serverCall) && serverCall.ValueKind == JsonValueKind.Object)
        {
            var id = serverCall.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(idElement.GetString())
                ? idElement.GetString()!
                : generateId();
            serverId = id;
            var toolType = serverCall.TryGetProperty("toolType", out var typeElement) ? typeElement.GetString() ?? string.Empty : string.Empty;
            var args = serverCall.TryGetProperty("args", out var argsElement) && argsElement.ValueKind != JsonValueKind.Null && argsElement.ValueKind != JsonValueKind.Undefined
                ? argsElement.GetRawText()
                : "{}";
            var payload = ServerMetadata(part, prepared.MetadataNames, id, toolType);
            content.Add(new GoogleProviderToolCall(id, "server:" + toolType, args, payload));
            return;
        }

        if (part.TryGetProperty("toolResponse", out var serverResponse) && serverResponse.ValueKind == JsonValueKind.Object)
        {
            var toolType = serverResponse.TryGetProperty("toolType", out var typeElement) ? typeElement.GetString() ?? string.Empty : string.Empty;
            var id = serverId;
            if (string.IsNullOrEmpty(id))
            {
                id = serverResponse.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(idElement.GetString())
                    ? idElement.GetString()
                    : generateId();
            }

            var responseJson = serverResponse.TryGetProperty("response", out var responseElement) && responseElement.ValueKind != JsonValueKind.Null && responseElement.ValueKind != JsonValueKind.Undefined
                ? responseElement.GetRawText()
                : "{}";
            content.Add(new GoogleToolResultContent(id!, "server:" + toolType, responseJson, ServerMetadata(part, prepared.MetadataNames, id!, toolType)));
            serverId = null;
        }
    }

    internal static IEnumerable<GeneratedSource> Sources(JsonElement candidate, Func<string> generateId)
    {
        if (!candidate.TryGetProperty("groundingMetadata", out var grounding) || grounding.ValueKind != JsonValueKind.Object)
        {
            yield break;
        }

        if (!grounding.TryGetProperty("groundingChunks", out var chunks) || chunks.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var chunk in chunks.EnumerateArray())
        {
            if (chunk.TryGetProperty("web", out var web) && web.ValueKind == JsonValueKind.Object && web.TryGetProperty("uri", out var webUri))
            {
                yield return new GeneratedSource(generateId(), webUri.GetString() ?? string.Empty, OptionalString(web, "title"));
            }
            else if (chunk.TryGetProperty("image", out var image) && image.ValueKind == JsonValueKind.Object && image.TryGetProperty("sourceUri", out var sourceUri))
            {
                yield return new GeneratedSource(generateId(), sourceUri.GetString() ?? string.Empty, OptionalString(image, "title"));
            }
            else if (chunk.TryGetProperty("retrievedContext", out var retrieved) && retrieved.ValueKind == JsonValueKind.Object)
            {
                var uri = OptionalString(retrieved, "uri");
                var title = OptionalString(retrieved, "title");
                var store = OptionalString(retrieved, "fileSearchStore");
                if (!string.IsNullOrEmpty(uri) && (uri!.StartsWith("http://", StringComparison.Ordinal) || uri.StartsWith("https://", StringComparison.Ordinal)))
                {
                    yield return new GeneratedSource(generateId(), uri, title);
                }
                else if (!string.IsNullOrEmpty(uri))
                {
                    yield return Document(generateId(), uri!, title ?? "Unknown Document", uri!);
                }
                else if (!string.IsNullOrEmpty(store))
                {
                    yield return Document(generateId(), store!, title ?? "Unknown Document", store!);
                }
            }
            else if (chunk.TryGetProperty("maps", out var maps) && maps.ValueKind == JsonValueKind.Object)
            {
                var uri = OptionalString(maps, "uri");
                if (!string.IsNullOrEmpty(uri))
                {
                    yield return new GeneratedSource(generateId(), uri!, OptionalString(maps, "title"));
                }
            }
        }
    }

    private static GeneratedSource Document(string id, string uri, string title, string filenameSource)
    {
        var filename = filenameSource;
        var slash = filenameSource.LastIndexOf('/');
        if (slash >= 0 && slash < filenameSource.Length - 1)
        {
            filename = filenameSource.Substring(slash + 1);
        }

        var media = "application/octet-stream";
        if (uri.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            media = "application/pdf";
        }
        else if (uri.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            media = "text/plain";
        }
        else if (uri.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            media = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        }
        else if (uri.EndsWith(".doc", StringComparison.OrdinalIgnoreCase))
        {
            media = "application/msword";
        }
        else if (uri.EndsWith(".md", StringComparison.OrdinalIgnoreCase) || uri.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase))
        {
            media = "text/markdown";
        }

        var meta = GoogleJson.Element(new JsonObject
        {
            ["sourceType"] = "document",
            ["mediaType"] = media,
            ["filename"] = filename,
        });
        return new GeneratedSource(id, uri, title, meta);
    }

    internal static JsonElement Metadata(string[] names, JsonNode? promptFeedback, JsonNode? grounding, JsonNode? urlContext, JsonNode? safety, JsonNode? usage, string? finishMessage, string? serviceTier)
    {
        var payload = new JsonObject
        {
            ["promptFeedback"] = promptFeedback ?? GoogleJson.Null(),
            ["groundingMetadata"] = grounding ?? GoogleJson.Null(),
            ["urlContextMetadata"] = urlContext ?? GoogleJson.Null(),
            ["safetyRatings"] = safety ?? GoogleJson.Null(),
            ["usageMetadata"] = usage ?? GoogleJson.Null(),
            ["finishMessage"] = finishMessage == null ? GoogleJson.Null() : JsonValue.Create(finishMessage),
            ["serviceTier"] = serviceTier == null ? GoogleJson.Null() : JsonValue.Create(serviceTier),
        };
        return Wrap(names, payload);
    }

    internal static JsonElement Wrap(IReadOnlyList<string> names, JsonObject payload)
    {
        var root = new JsonObject();
        var raw = payload.ToJsonString();
        foreach (var name in names)
        {
            root[name] = JsonNode.Parse(raw);
        }

        return GoogleJson.Element(root);
    }

    private static JsonElement? Signature(JsonElement part, string[] names)
    {
        if (!part.TryGetProperty("thoughtSignature", out var signature) || signature.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return Wrap(names, new JsonObject { ["thoughtSignature"] = signature.GetString() });
    }

    private static JsonElement ServerMetadata(JsonElement part, string[] names, string id, string toolType)
    {
        var payload = new JsonObject
        {
            ["serverToolCallId"] = id,
            ["serverToolType"] = toolType,
        };
        if (part.TryGetProperty("thoughtSignature", out var signature) && signature.ValueKind == JsonValueKind.String)
        {
            payload["thoughtSignature"] = signature.GetString();
        }

        return Wrap(names, payload);
    }

    private static List<GoogleToolSpec> CollectTools(LanguageModelCallOptions options, JsonElement googleOptions)
    {
        var tools = new List<GoogleToolSpec>();
        if (options.Tools != null)
        {
            foreach (var tool in options.Tools)
            {
                tools.Add(new GoogleToolSpec(tool.Name, tool.Description, tool.InputSchema, tool.Strict));
            }
        }

        if (googleOptions.ValueKind == JsonValueKind.Object && googleOptions.TryGetProperty("providerTools", out var providerTools) && providerTools.ValueKind == JsonValueKind.Array)
        {
            foreach (var tool in providerTools.EnumerateArray())
            {
                var id = tool.TryGetProperty("id", out var idElement) ? idElement.GetString() : string.Empty;
                var name = tool.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : id;
                var args = tool.TryGetProperty("args", out var argsElement) ? argsElement : default;
                tools.Add(new GoogleToolSpec(id ?? string.Empty, name, args));
            }
        }

        return tools;
    }

    private static bool TryOptions(LanguageModelCallOptions options, string[] names, bool vertex, out JsonElement element)
    {
        element = default;
        if (options.ProviderOptions == null)
        {
            return false;
        }

        foreach (var name in names)
        {
            if (options.ProviderOptions.TryGetValue(name, out var value) && value.ValueKind == JsonValueKind.Object)
            {
                element = value;
                return true;
            }
        }

        if (vertex && options.ProviderOptions.TryGetValue("google", out var google) && google.ValueKind == JsonValueKind.Object)
        {
            element = google;
            return true;
        }

        if (!vertex)
        {
            if (options.ProviderOptions.TryGetValue("googleVertex", out var googleVertex) && googleVertex.ValueKind == JsonValueKind.Object)
            {
                element = googleVertex;
                return true;
            }

            if (options.ProviderOptions.TryGetValue("vertex", out var legacy) && legacy.ValueKind == JsonValueKind.Object)
            {
                element = legacy;
                return true;
            }
        }

        return false;
    }

    private static JsonObject? ThinkingConfig(string? reasoning, string modelId, JsonElement googleOptions, bool hasOptions, List<CallWarning> warnings)
    {
        JsonObject? resolved = null;
        if (!string.IsNullOrEmpty(reasoning) && reasoning != "provider-default")
        {
            var model = modelId ?? string.Empty;
            var gemini3 = GoogleModelCapabilitiesMap.Get(model).UsesGemini3Features && model.IndexOf("gemini-3-pro-image", StringComparison.Ordinal) < 0;
            if (gemini3)
            {
                var minimum = MinimumLevel(model);
                if (reasoning == "none")
                {
                    resolved = new JsonObject { ["thinkingLevel"] = minimum };
                }
                else
                {
                    var mapped = reasoning;
                    if (reasoning == "minimal")
                    {
                        mapped = minimum;
                    }
                    else if (reasoning == "xhigh")
                    {
                        mapped = "high";
                    }
                    else if (reasoning != "low" && reasoning != "medium" && reasoning != "high")
                    {
                        warnings.Add(new CallWarning("unsupported", "reasoning \"" + reasoning + "\" is not supported by this model."));
                        mapped = null;
                    }

                    if (mapped != null && mapped != reasoning)
                    {
                        warnings.Add(new CallWarning("compatibility", "reasoning \"" + reasoning + "\" is not directly supported by this model. mapped to effort \"" + mapped + "\"."));
                    }

                    if (mapped != null)
                    {
                        resolved = new JsonObject { ["thinkingLevel"] = mapped };
                    }
                }
            }
            else if (reasoning == "none")
            {
                resolved = new JsonObject { ["thinkingBudget"] = 0 };
            }
            else
            {
                var budget = Budget(reasoning, model);
                if (budget == null)
                {
                    warnings.Add(new CallWarning("unsupported", "reasoning \"" + reasoning + "\" is not supported by this model."));
                }
                else
                {
                    resolved = new JsonObject { ["thinkingBudget"] = budget.Value };
                }
            }
        }

        if (!hasOptions || !googleOptions.TryGetProperty("thinkingConfig", out var config) || config.ValueKind != JsonValueKind.Object)
        {
            return resolved;
        }

        resolved ??= new JsonObject();
        if (config.TryGetProperty("thinkingBudget", out var budgetElement) && budgetElement.ValueKind == JsonValueKind.Number)
        {
            resolved["thinkingBudget"] = budgetElement.GetInt32();
        }

        if (config.TryGetProperty("includeThoughts", out var include) && (include.ValueKind == JsonValueKind.True || include.ValueKind == JsonValueKind.False))
        {
            resolved["includeThoughts"] = include.ValueKind == JsonValueKind.True;
        }

        var level = EnumValue(config, "thinkingLevel", "minimal", "low", "medium", "high");
        if (level != null)
        {
            resolved["thinkingLevel"] = level;
        }

        return resolved.Count == 0 ? null : resolved;
    }

    private static string MinimumLevel(string modelId)
    {
        var name = modelId ?? string.Empty;
        var slash = name.LastIndexOf('/');
        if (slash >= 0)
        {
            name = name.Substring(slash + 1);
        }

        name = name.ToLowerInvariant();
        if (name == "gemini-flash-latest")
        {
            return "low";
        }

        var match = FlashMinimum.Match(name);
        if (!match.Success)
        {
            return "minimal";
        }

        var major = int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        var minor = int.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
        return major > 3 || (major == 3 && minor >= 7) ? "low" : "minimal";
    }

    private static int? Budget(string reasoning, string modelId)
    {
        double? percent = null;
        switch (reasoning)
        {
            case "minimal":
                percent = 0.02;
                break;
            case "low":
                percent = 0.1;
                break;
            case "medium":
                percent = 0.3;
                break;
            case "high":
                percent = 0.6;
                break;
            case "xhigh":
                percent = 0.9;
                break;
        }

        if (percent == null)
        {
            return null;
        }

        var id = modelId ?? string.Empty;
        var maxThink = id.ToLowerInvariant().IndexOf("2.5-pro", StringComparison.Ordinal) >= 0 || id.IndexOf("gemini-3-pro-image", StringComparison.Ordinal) >= 0
            ? 32768
            : 24576;
        var value = (int)Math.Round(65536 * percent.Value, MidpointRounding.AwayFromZero);
        if (value < 0)
        {
            value = 0;
        }

        if (value > maxThink)
        {
            value = maxThink;
        }

        return value;
    }

    private static JsonObject? ImageConfig(JsonElement googleOptions, bool hasOptions, bool vertex, string provider, List<CallWarning> warnings)
    {
        if (!hasOptions || !googleOptions.TryGetProperty("imageConfig", out var image) || image.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var dropped = new List<string>();
        if (!vertex)
        {
            if (image.TryGetProperty("personGeneration", out var person) && person.ValueKind != JsonValueKind.Null && person.ValueKind != JsonValueKind.Undefined)
            {
                dropped.Add("'imageConfig.personGeneration'");
            }

            if (image.TryGetProperty("prominentPeople", out var people) && people.ValueKind != JsonValueKind.Null && people.ValueKind != JsonValueKind.Undefined)
            {
                dropped.Add("'imageConfig.prominentPeople'");
            }

            if (image.TryGetProperty("imageOutputOptions", out var output) && output.ValueKind != JsonValueKind.Null && output.ValueKind != JsonValueKind.Undefined)
            {
                dropped.Add("'imageConfig.imageOutputOptions'");
            }
        }

        if (dropped.Count > 0)
        {
            warnings.Add(new CallWarning(
                "other",
                string.Join(", ", dropped) + " " +
                (dropped.Count == 1 ? "is a Vertex AI option and is" : "are Vertex AI options and are") +
                " ignored with the current Google provider (" + provider + ")."));
        }

        var copy = new JsonObject();
        CopyString(image, copy, "aspectRatio");
        CopyString(image, copy, "imageSize");
        if (vertex)
        {
            CopyString(image, copy, "personGeneration");
            CopyString(image, copy, "prominentPeople");
            if (image.TryGetProperty("imageOutputOptions", out var output) && output.ValueKind == JsonValueKind.Object)
            {
                var nested = new JsonObject();
                CopyString(output, nested, "mimeType");
                if (output.TryGetProperty("compressionQuality", out var quality) && quality.ValueKind == JsonValueKind.Number)
                {
                    nested["compressionQuality"] = quality.GetDouble();
                }

                if (nested.Count > 0)
                {
                    copy["imageOutputOptions"] = nested;
                }
            }
        }

        return copy.Count == 0 ? null : copy;
    }

    private static JsonArray? Safety(JsonElement googleOptions, bool hasOptions)
    {
        if (!hasOptions)
        {
            return null;
        }

        if (googleOptions.TryGetProperty("safetySettings", out var settings) && settings.ValueKind == JsonValueKind.Array)
        {
            return (JsonArray)GoogleJson.Clone(settings);
        }

        var threshold = EnumValue(
            googleOptions,
            "threshold",
            "HARM_BLOCK_THRESHOLD_UNSPECIFIED",
            "BLOCK_LOW_AND_ABOVE",
            "BLOCK_MEDIUM_AND_ABOVE",
            "BLOCK_ONLY_HIGH",
            "BLOCK_NONE",
            "OFF");
        if (threshold == null)
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

    private static JsonObject Retrieval(JsonElement retrieval)
    {
        var copy = new JsonObject();
        if (retrieval.TryGetProperty("latLng", out var latLng) && latLng.ValueKind == JsonValueKind.Object)
        {
            var point = new JsonObject();
            if (latLng.TryGetProperty("latitude", out var latitude) && latitude.ValueKind == JsonValueKind.Number)
            {
                point["latitude"] = latitude.GetDouble();
            }

            if (latLng.TryGetProperty("longitude", out var longitude) && longitude.ValueKind == JsonValueKind.Number)
            {
                point["longitude"] = longitude.GetDouble();
            }

            copy["latLng"] = point;
        }

        return copy;
    }

    private static void CopyString(JsonElement source, JsonObject target, string name)
    {
        if (source.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            target[name] = value.GetString();
        }
    }

    private static bool? Bool(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value))
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

    private static string? EnumValue(JsonElement element, string name, params string[] allowed)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var text = value.GetString();
        foreach (var item in allowed)
        {
            if (item == text)
            {
                return text;
            }
        }

        return null;
    }

    private static string? OptionalString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.GetString();
    }
}

internal sealed class GoogleStreamSession
{
    private readonly GooglePreparedCall _prepared;
    private readonly Func<string> _generateId;
    private string? _textId;
    private string? _reasoningId;
    private int _blocks;
    private bool _metadataSent;
    private bool _hasToolCalls;
    private string? _blockReason;
    private JsonNode? _usage;
    private JsonNode? _promptFeedback;
    private JsonNode? _grounding;
    private JsonNode? _urlContext;
    private JsonNode? _safety;
    private string? _finishMessage;
    private string? _rawFinish;
    private FinishReason _finish = FinishReason.Other;
    private readonly HashSet<string> _urls = new HashSet<string>(StringComparer.Ordinal);
    private string? _codeId;
    private string? _serverId;

    public GoogleStreamSession(GooglePreparedCall prepared, Func<string> generateId)
    {
        _prepared = prepared;
        _generateId = generateId;
    }

    public StreamStartStreamPart Start()
    {
        return new StreamStartStreamPart(_prepared.Warnings);
    }

    public List<LanguageModelStreamPart> Accept(string data, bool includeRaw)
    {
        var parts = new List<LanguageModelStreamPart>();
        if (includeRaw)
        {
            parts.Add(new RawStreamPart(data ?? string.Empty));
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(string.IsNullOrWhiteSpace(data) ? "{}" : data);
        }
        catch (JsonException exception)
        {
            parts.Add(new ErrorStreamPart(exception.Message));
            return parts;
        }

        using (document)
        {
            var root = document.RootElement;
            if (!_metadataSent && root.TryGetProperty("responseId", out var responseId) && responseId.ValueKind == JsonValueKind.String)
            {
                _metadataSent = true;
                parts.Add(new ResponseMetadataStreamPart(responseId.GetString(), null, null));
            }

            if (root.TryGetProperty("usageMetadata", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                _usage = GoogleJson.Clone(usage);
            }

            if (root.TryGetProperty("promptFeedback", out var feedback) && feedback.ValueKind == JsonValueKind.Object && _blockReason == null)
            {
                _promptFeedback = GoogleJson.Clone(feedback);
                if (feedback.TryGetProperty("blockReason", out var block) && block.ValueKind == JsonValueKind.String)
                {
                    var reason = block.GetString();
                    if (GoogleFinishReason.IsConfirmedPromptBlock(reason))
                    {
                        _blockReason = reason;
                        _rawFinish = reason;
                        _finish = FinishReason.ContentFilter;
                    }
                }
            }

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.ValueKind != JsonValueKind.Array || candidates.GetArrayLength() == 0)
            {
                return parts;
            }

            var candidate = candidates[0];
            if (candidate.TryGetProperty("groundingMetadata", out var grounding) && grounding.ValueKind == JsonValueKind.Object)
            {
                _grounding = GoogleJson.Clone(grounding);
            }

            if (candidate.TryGetProperty("urlContextMetadata", out var urlContext) && urlContext.ValueKind == JsonValueKind.Object)
            {
                _urlContext = GoogleJson.Clone(urlContext);
            }

            if (candidate.TryGetProperty("safetyRatings", out var safety) && safety.ValueKind == JsonValueKind.Array)
            {
                _safety = GoogleJson.Clone(safety);
            }

            if (candidate.TryGetProperty("finishMessage", out var finishMessage) && finishMessage.ValueKind == JsonValueKind.String)
            {
                _finishMessage = finishMessage.GetString();
            }

            if (_blockReason != null)
            {
                return parts;
            }

            foreach (var source in GoogleGenerate.Sources(candidate, _generateId))
            {
                if (_urls.Add(source.Url))
                {
                    parts.Add(new SourceStreamPart(source.Id, source.Url, source.Title, source.ProviderMetadata));
                }
            }

            if (candidate.TryGetProperty("content", out var content) && content.TryGetProperty("parts", out var contentParts) && contentParts.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in contentParts.EnumerateArray())
                {
                    AcceptPart(part, parts);
                }
            }

            if (candidate.TryGetProperty("finishReason", out var finish) && finish.ValueKind == JsonValueKind.String)
            {
                _rawFinish = finish.GetString();
                _finish = GoogleFinishReason.Map(_rawFinish, _hasToolCalls);
            }
        }

        return parts;
    }

    public List<LanguageModelStreamPart> Finish()
    {
        var parts = new List<LanguageModelStreamPart>();
        if (_textId != null)
        {
            parts.Add(new TextEndStreamPart(_textId));
            _textId = null;
        }

        if (_reasoningId != null)
        {
            parts.Add(new ReasoningEndStreamPart(_reasoningId));
            _reasoningId = null;
        }

        string? tier = null;
        JsonElement usageElement = default;
        if (_usage != null)
        {
            usageElement = GoogleJson.Element(_usage);
            if (usageElement.TryGetProperty("serviceTier", out var service) && service.ValueKind == JsonValueKind.String)
            {
                tier = service.GetString();
            }
        }

        var usage = usageElement.ValueKind == JsonValueKind.Object
            ? GoogleUsageConverter.Convert(usageElement)
            : new LanguageModelUsage(null, null, null);
        parts.Add(new FinishStreamPart(
            _finish,
            usage,
            _rawFinish,
            GoogleGenerate.Metadata(_prepared.MetadataNames, _promptFeedback, _grounding, _urlContext, _safety, _usage, _finishMessage, tier)));
        return parts;
    }

    private void AcceptPart(JsonElement part, List<LanguageModelStreamPart> parts)
    {
        if (part.TryGetProperty("executableCode", out var executable) && executable.ValueKind == JsonValueKind.Object)
        {
            EndText(parts);
            EndReasoning(parts);
            _codeId = _generateId();
            parts.Add(new ToolCallStreamPart(_codeId, _prepared.CodeExecutionName, executable.GetRawText()));
            return;
        }

        if (part.TryGetProperty("codeExecutionResult", out var codeResult) && codeResult.ValueKind == JsonValueKind.Object)
        {
            var output = codeResult.TryGetProperty("output", out var outputElement) && outputElement.ValueKind == JsonValueKind.String ? outputElement.GetString() : string.Empty;
            var outcome = codeResult.TryGetProperty("outcome", out var outcomeElement) ? outcomeElement.GetString() : null;
            var json = new JsonObject { ["outcome"] = outcome, ["output"] = output ?? string.Empty }.ToJsonString();
            parts.Add(new GoogleToolResultStreamPart(_codeId ?? string.Empty, _prepared.CodeExecutionName, json, null));
            return;
        }

        if (part.TryGetProperty("text", out var textElement) && textElement.ValueKind != JsonValueKind.Null && textElement.ValueKind != JsonValueKind.Undefined)
        {
            var text = textElement.GetString() ?? string.Empty;
            var thought = part.TryGetProperty("thought", out var thoughtElement) && thoughtElement.ValueKind == JsonValueKind.True;
            JsonElement? signature = null;
            if (part.TryGetProperty("thoughtSignature", out var signatureElement) && signatureElement.ValueKind == JsonValueKind.String)
            {
                signature = GoogleGenerate.Wrap(_prepared.MetadataNames, new JsonObject { ["thoughtSignature"] = signatureElement.GetString() });
            }

            if (text.Length == 0)
            {
                if (signature != null && _textId != null)
                {
                    parts.Add(new TextDeltaStreamPart(_textId, string.Empty));
                    parts.Add(new GoogleSignedDelta("text-delta", _textId, string.Empty, signature.Value));
                }

                return;
            }

            if (thought)
            {
                EndText(parts);
                if (_reasoningId == null)
                {
                    _reasoningId = _blocks.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    _blocks++;
                    parts.Add(new ReasoningStartStreamPart(_reasoningId));
                }

                parts.Add(new ReasoningDeltaStreamPart(_reasoningId, text));
                if (signature != null)
                {
                    parts.Add(new GoogleSignedDelta("reasoning-delta", _reasoningId, text, signature.Value));
                }

                return;
            }

            EndReasoning(parts);
            if (_textId == null)
            {
                _textId = _blocks.ToString(System.Globalization.CultureInfo.InvariantCulture);
                _blocks++;
                parts.Add(new TextStartStreamPart(_textId));
            }

            parts.Add(new TextDeltaStreamPart(_textId, text));
            if (signature != null)
            {
                parts.Add(new GoogleSignedDelta("text-delta", _textId, text, signature.Value));
            }

            return;
        }

        if (part.TryGetProperty("inlineData", out var inline) && inline.ValueKind == JsonValueKind.Object)
        {
            EndText(parts);
            EndReasoning(parts);
            var mime = inline.TryGetProperty("mimeType", out var mimeElement) ? mimeElement.GetString() ?? "application/octet-stream" : "application/octet-stream";
            var bytes = Array.Empty<byte>();
            if (inline.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.String)
            {
                try
                {
                    bytes = Convert.FromBase64String(dataElement.GetString() ?? string.Empty);
                }
                catch (FormatException)
                {
                    bytes = Array.Empty<byte>();
                }
            }

            var thought = part.TryGetProperty("thought", out var thoughtElement) && thoughtElement.ValueKind == JsonValueKind.True;
            JsonElement? signature = null;
            if (part.TryGetProperty("thoughtSignature", out var signatureElement) && signatureElement.ValueKind == JsonValueKind.String)
            {
                signature = GoogleGenerate.Wrap(_prepared.MetadataNames, new JsonObject { ["thoughtSignature"] = signatureElement.GetString() });
            }

            parts.Add(new GoogleFileStreamPart(bytes, mime, thought, signature));
            return;
        }

        if (part.TryGetProperty("functionCall", out var call) && call.ValueKind == JsonValueKind.Object)
        {
            var name = call.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String ? nameElement.GetString() : null;
            var hasArgs = call.TryGetProperty("args", out var argsElement) && argsElement.ValueKind != JsonValueKind.Null && argsElement.ValueKind != JsonValueKind.Undefined;
            var hasPartial = call.TryGetProperty("partialArgs", out _);
            var willContinue = call.TryGetProperty("willContinue", out var willElement) && willElement.ValueKind == JsonValueKind.True;
            if (string.IsNullOrEmpty(name) || hasPartial || willContinue)
            {
                return;
            }

            var id = call.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(idElement.GetString())
                ? idElement.GetString()!
                : _generateId();
            var args = hasArgs
                ? argsElement.ValueKind == JsonValueKind.String ? argsElement.GetString() ?? "{}" : argsElement.GetRawText()
                : "{}";
            JsonElement? signature = null;
            if (part.TryGetProperty("thoughtSignature", out var signatureElement) && signatureElement.ValueKind == JsonValueKind.String)
            {
                signature = GoogleGenerate.Wrap(_prepared.MetadataNames, new JsonObject { ["thoughtSignature"] = signatureElement.GetString() });
            }

            parts.Add(new ToolCallStreamPart(id, name!, args, signature));
            _hasToolCalls = true;
            return;
        }

        if (part.TryGetProperty("toolCall", out var server) && server.ValueKind == JsonValueKind.Object)
        {
            var id = server.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(idElement.GetString())
                ? idElement.GetString()!
                : _generateId();
            _serverId = id;
            var toolType = server.TryGetProperty("toolType", out var typeElement) ? typeElement.GetString() ?? string.Empty : string.Empty;
            var args = server.TryGetProperty("args", out var argsElement) && argsElement.ValueKind != JsonValueKind.Undefined && argsElement.ValueKind != JsonValueKind.Null
                ? argsElement.GetRawText()
                : "{}";
            var payload = new JsonObject { ["serverToolCallId"] = id, ["serverToolType"] = toolType };
            if (part.TryGetProperty("thoughtSignature", out var signatureElement) && signatureElement.ValueKind == JsonValueKind.String)
            {
                payload["thoughtSignature"] = signatureElement.GetString();
            }

            parts.Add(new ToolCallStreamPart(id, "server:" + toolType, args, GoogleGenerate.Wrap(_prepared.MetadataNames, payload)));
            return;
        }

        if (part.TryGetProperty("toolResponse", out var response) && response.ValueKind == JsonValueKind.Object)
        {
            var toolType = response.TryGetProperty("toolType", out var typeElement) ? typeElement.GetString() ?? string.Empty : string.Empty;
            var id = _serverId;
            if (string.IsNullOrEmpty(id))
            {
                id = response.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String ? idElement.GetString() : _generateId();
            }

            var json = response.TryGetProperty("response", out var responseElement) && responseElement.ValueKind != JsonValueKind.Undefined && responseElement.ValueKind != JsonValueKind.Null
                ? responseElement.GetRawText()
                : "{}";
            var payload = new JsonObject { ["serverToolCallId"] = id, ["serverToolType"] = toolType };
            if (part.TryGetProperty("thoughtSignature", out var signatureElement) && signatureElement.ValueKind == JsonValueKind.String)
            {
                payload["thoughtSignature"] = signatureElement.GetString();
            }

            parts.Add(new GoogleToolResultStreamPart(id!, "server:" + toolType, json, GoogleGenerate.Wrap(_prepared.MetadataNames, payload)));
            _serverId = null;
        }
    }

    private void EndText(List<LanguageModelStreamPart> parts)
    {
        if (_textId == null)
        {
            return;
        }

        parts.Add(new TextEndStreamPart(_textId));
        _textId = null;
    }

    private void EndReasoning(List<LanguageModelStreamPart> parts)
    {
        if (_reasoningId == null)
        {
            return;
        }

        parts.Add(new ReasoningEndStreamPart(_reasoningId));
        _reasoningId = null;
    }
}
