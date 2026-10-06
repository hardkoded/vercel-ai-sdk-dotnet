// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>Parses a Gemini generateContent response.</summary>
public static class GoogleResponse
{
    /// <summary>Parses one generateContent JSON document.</summary>
    public static LanguageModelGenerateResult Parse(JsonElement root, GoogleParseContext context)
    {
        context ??= new GoogleParseContext();
        var content = new List<GeneratedContent>();
        var candidate = First(root, "candidates");
        string? finish = null;
        string? finishMessage = null;
        JsonElement? grounding = null;
        JsonElement? urlContext = null;
        JsonElement? safety = null;
        if (candidate is { } candidateElement)
        {
            finish = GoogleJson.String(candidateElement, "finishReason");
            finishMessage = GoogleJson.String(candidateElement, "finishMessage");
            if (candidateElement.TryGetProperty("groundingMetadata", out var groundingValue))
            {
                grounding = groundingValue;
            }

            if (candidateElement.TryGetProperty("urlContextMetadata", out var urlValue))
            {
                urlContext = urlValue;
            }

            if (candidateElement.TryGetProperty("safetyRatings", out var safetyValue))
            {
                safety = safetyValue;
            }

            if (candidateElement.TryGetProperty("content", out var body) && body.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
            {
                string? codeId = null;
                string? serverId = null;
                foreach (var part in parts.EnumerateArray())
                {
                    AppendPart(content, part, context, ref codeId, ref serverId);
                }
            }
        }

        if (grounding != null)
        {
            foreach (var source in Sources(grounding.Value, context))
            {
                content.Add(source);
            }
        }

        var block = root.TryGetProperty("promptFeedback", out var feedback) ? GoogleJson.String(feedback, "blockReason") : null;
        var confirmed = GoogleFinishReason.IsConfirmedPromptBlock(block);
        var blocked = finish == null && confirmed;
        var raw = finish ?? (confirmed ? block : null);
        var clientTool = false;
        foreach (var part in content)
        {
            if (part is GoogleToolCall tool && !tool.ProviderExecuted)
            {
                clientTool = true;
            }
            else if (part is GeneratedToolCall && part is not GoogleToolCall)
            {
                clientTool = true;
            }
        }

        var usageElement = root.TryGetProperty("usageMetadata", out var usageValue) ? usageValue : (JsonElement?)null;
        var usage = GoogleUsage.Convert(usageElement);
        var metadata = Metadata(context, feedbackOrNull(root), grounding, urlContext, safety, usageElement, finishMessage);
        return new LanguageModelGenerateResult(
            content,
            blocked ? FinishReason.ContentFilter : GoogleFinishReason.Map(raw, clientTool),
            usage?.ToLanguageModelUsage() ?? LanguageModelUsage.Empty,
            raw,
            CallWarnings(context.Warnings),
            root.TryGetProperty("responseId", out var id) ? id.GetString() : null,
            metadata,
            root.GetRawText());
    }

    /// <summary>Reads URL and document sources from grounding metadata.</summary>
    public static IReadOnlyList<GeneratedSource> Sources(JsonElement grounding, GoogleParseContext context)
    {
        var sources = new List<GeneratedSource>();
        if (!grounding.TryGetProperty("groundingChunks", out var chunks) || chunks.ValueKind != JsonValueKind.Array)
        {
            return sources;
        }

        foreach (var chunk in chunks.EnumerateArray())
        {
            if (chunk.TryGetProperty("web", out var web) && web.ValueKind == JsonValueKind.Object)
            {
                var uri = GoogleJson.String(web, "uri");
                if (!string.IsNullOrEmpty(uri))
                {
                    sources.Add(new GeneratedSource(context.NextId(), uri!, GoogleJson.String(web, "title")));
                }
            }
            else if (chunk.TryGetProperty("image", out var image) && image.ValueKind == JsonValueKind.Object)
            {
                var uri = GoogleJson.String(image, "sourceUri");
                if (!string.IsNullOrEmpty(uri))
                {
                    sources.Add(new GeneratedSource(context.NextId(), uri!, GoogleJson.String(image, "title")));
                }
            }
            else if (chunk.TryGetProperty("retrievedContext", out var retrieved))
            {
                var uri = GoogleJson.String(retrieved, "uri");
                var store = GoogleJson.String(retrieved, "fileSearchStore");
                var title = GoogleJson.String(retrieved, "title");
                if (!string.IsNullOrEmpty(uri) && (uri!.StartsWith("http://", StringComparison.Ordinal) || uri.StartsWith("https://", StringComparison.Ordinal)))
                {
                    sources.Add(new GeneratedSource(context.NextId(), uri, title));
                }
                else if (!string.IsNullOrEmpty(uri))
                {
                    sources.Add(new GeneratedSource(context.NextId(), uri!, title ?? "Unknown Document"));
                }
                else if (!string.IsNullOrEmpty(store))
                {
                    sources.Add(new GeneratedSource(context.NextId(), store!, title ?? "Unknown Document"));
                }
            }
            else if (chunk.TryGetProperty("maps", out var maps))
            {
                var uri = GoogleJson.String(maps, "uri");
                if (!string.IsNullOrEmpty(uri))
                {
                    sources.Add(new GeneratedSource(context.NextId(), uri!, GoogleJson.String(maps, "title")));
                }
            }
        }

        return sources;
    }

    private static void AppendPart(List<GeneratedContent> content, JsonElement part, GoogleParseContext context, ref string? codeId, ref string? serverId)
    {
        if (part.TryGetProperty("executableCode", out var code) && code.ValueKind == JsonValueKind.Object && code.TryGetProperty("code", out _))
        {
            codeId = context.NextId();
            content.Add(new GoogleToolCall(codeId, context.Tools.CustomName("code_execution"), code.GetRawText(), null, true));
            return;
        }

        if (part.TryGetProperty("codeExecutionResult", out var result) && result.ValueKind == JsonValueKind.Object)
        {
            var output = result.TryGetProperty("output", out var outputValue) && outputValue.ValueKind == JsonValueKind.String ? outputValue.GetString() : string.Empty;
            content.Add(new GoogleToolResult(codeId ?? context.NextId(), context.Tools.CustomName("code_execution"), GoogleJson.String(result, "outcome") ?? string.Empty, output ?? string.Empty));
            return;
        }

        if (part.TryGetProperty("functionCall", out var call) && call.ValueKind == JsonValueKind.Object && call.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
        {
            var id = GoogleJson.String(call, "id");
            if (string.IsNullOrEmpty(id))
            {
                id = context.NextId();
            }

            var args = call.TryGetProperty("args", out var argsValue) ? argsValue.GetRawText() : "{}";
            content.Add(new GeneratedToolCall(id!, name.GetString() ?? string.Empty, args, SignatureMetadata(part, context)));
            return;
        }

        if (part.TryGetProperty("toolCall", out var server) && server.ValueKind == JsonValueKind.Object)
        {
            serverId = GoogleJson.String(server, "id") ?? context.NextId();
            var toolType = GoogleJson.String(server, "toolType") ?? string.Empty;
            var args = server.TryGetProperty("args", out var serverArgs) ? serverArgs.GetRawText() : "{}";
            content.Add(new GoogleToolCall(serverId, "server:" + toolType, args, ServerMetadata(part, context, serverId, toolType), true));
            return;
        }

        if (part.TryGetProperty("toolResponse", out var serverResult) && serverResult.ValueKind == JsonValueKind.Object)
        {
            var toolType = GoogleJson.String(serverResult, "toolType") ?? string.Empty;
            var id = serverId ?? GoogleJson.String(serverResult, "id") ?? context.NextId();
            var response = serverResult.TryGetProperty("response", out var responseValue) ? responseValue.GetRawText() : "{}";
            content.Add(new GoogleToolResult(id, "server:" + toolType, string.Empty, response, ServerMetadata(part, context, id, toolType)));
            serverId = null;
            return;
        }

        if (part.TryGetProperty("inlineData", out var inline) && inline.ValueKind == JsonValueKind.Object)
        {
            var bytes = inline.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.String
                ? Decode(data.GetString())
                : Array.Empty<byte>();
            var media = GoogleJson.String(inline, "mimeType") ?? "application/octet-stream";
            var thought = part.TryGetProperty("thought", out var thoughtValue) && thoughtValue.ValueKind == JsonValueKind.True;
            content.Add(new GoogleGeneratedFile(bytes, media, thought, SignatureMetadata(part, context)));
            return;
        }

        if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
        {
            var value = text.GetString() ?? string.Empty;
            var signature = SignatureMetadata(part, context);
            if (value.Length == 0)
            {
                if (signature != null && content.Count > 0 && content[content.Count - 1] is GeneratedToolCall last)
                {
                    content[content.Count - 1] = new GeneratedToolCall(last.ToolCallId, last.ToolName, last.ArgumentsJson, signature);
                }

                return;
            }

            var thought = part.TryGetProperty("thought", out var thoughtFlag) && thoughtFlag.ValueKind == JsonValueKind.True;
            if (thought)
            {
                content.Add(new GoogleReasoning(value, signature));
            }
            else
            {
                content.Add(new GoogleText(value, signature));
            }
        }
    }

    private static JsonElement? Metadata(GoogleParseContext context, JsonElement? feedback, JsonElement? grounding, JsonElement? urlContext, JsonElement? safety, JsonElement? usage, string? finishMessage)
    {
        var payload = new Dictionary<string, object?>
        {
            ["promptFeedback"] = feedback,
            ["groundingMetadata"] = grounding,
            ["urlContextMetadata"] = urlContext,
            ["safetyRatings"] = safety,
            ["usageMetadata"] = usage,
            ["finishMessage"] = finishMessage,
            ["serviceTier"] = usage is { } usageElement ? GoogleJson.String(usageElement, "serviceTier") : null,
        };
        var wrapped = new Dictionary<string, object?>();
        foreach (var name in context.MetadataNames)
        {
            wrapped[name] = payload;
        }

        return JsonSerializer.SerializeToElement(wrapped, GoogleJson.Metadata);
    }

    private static JsonElement? feedbackOrNull(JsonElement root)
    {
        return root.TryGetProperty("promptFeedback", out var feedback) ? feedback : null;
    }

    internal static JsonElement? SignatureMetadata(JsonElement part, GoogleParseContext context)
    {
        var signature = GoogleJson.String(part, "thoughtSignature");
        if (signature == null)
        {
            return null;
        }

        return Wrap(context, new Dictionary<string, string> { ["thoughtSignature"] = signature });
    }

    private static JsonElement ServerMetadata(JsonElement part, GoogleParseContext context, string id, string toolType)
    {
        var payload = new Dictionary<string, string?>
        {
            ["serverToolCallId"] = id,
            ["serverToolType"] = toolType,
            ["thoughtSignature"] = GoogleJson.String(part, "thoughtSignature"),
        };
        return Wrap(context, payload) ?? default;
    }

    private static JsonElement? Wrap(GoogleParseContext context, object payload)
    {
        var wrapped = new Dictionary<string, object>();
        foreach (var name in context.MetadataNames)
        {
            wrapped[name] = payload;
        }

        return JsonSerializer.SerializeToElement(wrapped, GoogleJson.Metadata);
    }

    private static byte[] Decode(string? data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return Array.Empty<byte>();
        }

        try
        {
            return Convert.FromBase64String(data!);
        }
        catch (FormatException)
        {
            return Array.Empty<byte>();
        }
    }

    private static JsonElement? First(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var array) || array.ValueKind != JsonValueKind.Array || array.GetArrayLength() == 0)
        {
            return null;
        }

        return array[0];
    }

    private static IReadOnlyList<CallWarning> CallWarnings(IReadOnlyList<GoogleWarning>? warnings)
    {
        if (warnings == null || warnings.Count == 0)
        {
            return Array.Empty<CallWarning>();
        }

        var result = new List<CallWarning>(warnings.Count);
        foreach (var warning in warnings)
        {
            result.Add(warning.ToCallWarning());
        }

        return result;
    }
}

/// <summary>Ids and metadata keys used while parsing a Gemini response.</summary>
public sealed class GoogleParseContext
{
    private int _ids;

    /// <summary>Creates a parse context.</summary>
    public GoogleParseContext(string? provider = null, Func<string>? generateId = null, GoogleToolPreparation? tools = null, IReadOnlyList<GoogleWarning>? warnings = null)
    {
        var vertex = provider != null && provider.IndexOf("vertex", StringComparison.OrdinalIgnoreCase) >= 0;
        MetadataNames = vertex ? new[] { "vertex", "googleVertex" } : new[] { "google" };
        GenerateId = generateId ?? (() => "id_" + _ids);
        Tools = tools ?? new GoogleToolPreparation(null, null, Array.Empty<GoogleWarning>(), new Dictionary<string, string>());
        Warnings = warnings ?? Array.Empty<GoogleWarning>();
    }

    /// <summary>Metadata namespaces written onto the result.</summary>
    public IReadOnlyList<string> MetadataNames { get; }

    /// <summary>Id factory. The default uses an increasing counter.</summary>
    public Func<string> GenerateId { get; set; }

    /// <summary>Tool name mapping.</summary>
    public GoogleToolPreparation Tools { get; }

    /// <summary>Warnings copied onto the result.</summary>
    public IReadOnlyList<GoogleWarning> Warnings { get; }

    /// <summary>Returns the next synthetic id.</summary>
    public string NextId()
    {
        _ids++;
        return GenerateId();
    }
}

/// <summary>Generated text that can carry a thought signature.</summary>
public sealed class GoogleText : GeneratedText
{
    /// <summary>Creates text.</summary>
    public GoogleText(string text, JsonElement? providerMetadata)
        : base(text)
    {
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Provider metadata, such as a thought signature.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Reasoning text that can carry a thought signature.</summary>
public sealed class GoogleReasoning : GeneratedReasoning
{
    /// <summary>Creates reasoning.</summary>
    public GoogleReasoning(string text, JsonElement? providerMetadata)
        : base(text)
    {
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A tool call that records whether the provider executed it.</summary>
public sealed class GoogleToolCall : GeneratedToolCall
{
    /// <summary>Creates a tool call.</summary>
    public GoogleToolCall(string toolCallId, string toolName, string argumentsJson, JsonElement? providerMetadata, bool providerExecuted)
        : base(toolCallId, toolName, argumentsJson, providerMetadata)
    {
        ProviderExecuted = providerExecuted;
    }

    /// <summary>True when Gemini executed the tool itself.</summary>
    public bool ProviderExecuted { get; }
}

/// <summary>A provider-executed tool result.</summary>
public sealed class GoogleToolResult : GeneratedContent
{
    /// <summary>Creates a tool result.</summary>
    public GoogleToolResult(string toolCallId, string toolName, string outcome, string output, JsonElement? providerMetadata = null)
        : base("tool-result")
    {
        ToolCallId = toolCallId;
        ToolName = toolName;
        Outcome = outcome;
        Output = output ?? string.Empty;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Matching tool-call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>Code-execution outcome, when this result came from code execution.</summary>
    public string Outcome { get; }

    /// <summary>Result payload.</summary>
    public string Output { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>A generated file, including thought images.</summary>
public sealed class GoogleGeneratedFile : GeneratedFile
{
    /// <summary>Creates a file.</summary>
    public GoogleGeneratedFile(byte[] data, string mediaType, bool thought, JsonElement? providerMetadata)
        : base(data, mediaType)
    {
        Thought = thought;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>True when the file was part of the model's thoughts.</summary>
    public bool Thought { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }
}
