// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>Settings for converting a prompt into Gemini <c>contents</c>.</summary>
public sealed class GoogleMessageOptions
{
    /// <summary>Model id used to detect Gemma and Gemini 3 behavior when the flags below are left unset.</summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>When true, system text is prepended to the first user turn and <c>systemInstruction</c> is omitted.</summary>
    public bool? IsGemmaModel { get; set; }

    /// <summary>When true, unsigned Gemini 3 function calls receive the documented skip sentinel.</summary>
    public bool? IsGemini3Model { get; set; }

    /// <summary>When true, multimodal tool results use <c>functionResponse.parts</c>.</summary>
    public bool? SupportsFunctionResponseParts { get; set; }

    /// <summary>When false, function call and response ids are omitted. Vertex omits them.</summary>
    public bool IncludeFunctionCallIds { get; set; } = true;

    /// <summary>Provider-option namespaces, in precedence order.</summary>
    public IReadOnlyList<string> ProviderOptionNames { get; set; } = new[] { "google" };

    /// <summary>Warnings produced while converting.</summary>
    public List<GoogleWarning> Warnings { get; } = new();
}

/// <summary>Gemini <c>contents</c> plus an optional system instruction.</summary>
public sealed class GooglePrompt
{
    /// <summary>Creates a prompt.</summary>
    public GooglePrompt(JsonArray contents, JsonObject? systemInstruction)
    {
        Contents = contents;
        SystemInstruction = systemInstruction;
    }

    /// <summary>Conversation contents.</summary>
    public JsonArray Contents { get; }

    /// <summary>System instruction, omitted for Gemma.</summary>
    public JsonObject? SystemInstruction { get; }
}

/// <summary>One rich prompt turn used by the message converter.</summary>
public abstract class GoogleTurn
{
}

/// <summary>A system turn.</summary>
public sealed class GoogleSystemTurn : GoogleTurn
{
    /// <summary>Creates a system turn.</summary>
    public GoogleSystemTurn(string text)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Instruction text.</summary>
    public string Text { get; }
}

/// <summary>A user turn.</summary>
public sealed class GoogleUserTurn : GoogleTurn
{
    /// <summary>Creates a user turn.</summary>
    public GoogleUserTurn(IReadOnlyList<GoogleUserPart> parts)
    {
        Parts = parts ?? Array.Empty<GoogleUserPart>();
    }

    /// <summary>User parts.</summary>
    public IReadOnlyList<GoogleUserPart> Parts { get; }
}

/// <summary>A user content part.</summary>
public abstract class GoogleUserPart
{
}

/// <summary>User text.</summary>
public sealed class GoogleTextUserPart : GoogleUserPart
{
    /// <summary>Creates text.</summary>
    public GoogleTextUserPart(string text)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Text.</summary>
    public string Text { get; }
}

/// <summary>A user file. <see cref="Kind"/> is <c>url</c>, <c>data</c>, <c>text</c>, or <c>reference</c>.</summary>
public sealed class GoogleFileUserPart : GoogleUserPart
{
    /// <summary>Creates a file part.</summary>
    public GoogleFileUserPart(string mediaType, string kind)
    {
        MediaType = mediaType ?? "application/octet-stream";
        Kind = kind ?? "data";
    }

    /// <summary>Declared media type. A top-level type such as <c>image</c> is completed from the bytes.</summary>
    public string MediaType { get; }

    /// <summary><c>url</c>, <c>data</c>, <c>text</c>, or <c>reference</c>.</summary>
    public string Kind { get; }

    /// <summary>Remote URL, including <c>gs://</c> and <c>data:</c> URLs.</summary>
    public string? Url { get; set; }

    /// <summary>Original URI preserved when a <c>gs:</c> URL was normalized.</summary>
    public string? OriginalUrl { get; set; }

    /// <summary>Inline bytes.</summary>
    public byte[]? Data { get; set; }

    /// <summary>Inline text source.</summary>
    public string? Text { get; set; }

    /// <summary>Provider reference object. The <c>google</c> property is the file URI.</summary>
    public JsonElement? Reference { get; set; }
}

/// <summary>An assistant turn.</summary>
public sealed class GoogleAssistantTurn : GoogleTurn
{
    /// <summary>Creates an assistant turn.</summary>
    public GoogleAssistantTurn(IReadOnlyList<GoogleAssistantPart> parts)
    {
        Parts = parts ?? Array.Empty<GoogleAssistantPart>();
    }

    /// <summary>Assistant parts.</summary>
    public IReadOnlyList<GoogleAssistantPart> Parts { get; }
}

/// <summary>An assistant content part.</summary>
public abstract class GoogleAssistantPart
{
    /// <summary>Provider options keyed by namespace.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
}

/// <summary>Assistant text.</summary>
public sealed class GoogleTextAssistantPart : GoogleAssistantPart
{
    /// <summary>Creates text.</summary>
    public GoogleTextAssistantPart(string text)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Text.</summary>
    public string Text { get; }
}

/// <summary>Assistant reasoning.</summary>
public sealed class GoogleReasoningAssistantPart : GoogleAssistantPart
{
    /// <summary>Creates reasoning.</summary>
    public GoogleReasoningAssistantPart(string text)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Reasoning text.</summary>
    public string Text { get; }
}

/// <summary>An assistant function or server tool call.</summary>
public sealed class GoogleToolCallAssistantPart : GoogleAssistantPart
{
    /// <summary>Creates a tool call.</summary>
    public GoogleToolCallAssistantPart(string toolName, string argumentsJson)
    {
        ToolName = toolName ?? string.Empty;
        ArgumentsJson = string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson;
    }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON arguments.</summary>
    public string ArgumentsJson { get; }

    /// <summary>Client tool-call id.</summary>
    public string? ToolCallId { get; set; }
}

/// <summary>An assistant file or reasoning file.</summary>
public sealed class GoogleFileAssistantPart : GoogleAssistantPart
{
    /// <summary>Creates a file part.</summary>
    public GoogleFileAssistantPart(string mediaType, string kind, bool reasoning)
    {
        MediaType = mediaType ?? "application/octet-stream";
        Kind = kind ?? "data";
        Reasoning = reasoning;
    }

    /// <summary>Media type.</summary>
    public string MediaType { get; }

    /// <summary><c>url</c>, <c>data</c>, <c>text</c>, or <c>reference</c>.</summary>
    public string Kind { get; }

    /// <summary>True for a reasoning file.</summary>
    public bool Reasoning { get; }

    /// <summary>Inline bytes.</summary>
    public byte[]? Data { get; set; }

    /// <summary>Inline text.</summary>
    public string? Text { get; set; }

    /// <summary>Remote URL.</summary>
    public string? Url { get; set; }

    /// <summary>Provider reference object.</summary>
    public JsonElement? Reference { get; set; }
}

/// <summary>A tool-result turn.</summary>
public sealed class GoogleToolTurn : GoogleTurn
{
    /// <summary>Creates a tool turn.</summary>
    public GoogleToolTurn(IReadOnlyList<GoogleToolResultPart> results)
    {
        Results = results ?? Array.Empty<GoogleToolResultPart>();
    }

    /// <summary>Tool results.</summary>
    public IReadOnlyList<GoogleToolResultPart> Results { get; }
}

/// <summary>One tool result.</summary>
public sealed class GoogleToolResultPart
{
    /// <summary>Creates a tool result.</summary>
    public GoogleToolResultPart(string toolName)
    {
        ToolName = toolName ?? string.Empty;
    }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>Matching tool-call id.</summary>
    public string? ToolCallId { get; set; }

    /// <summary><c>json</c>, <c>text</c>, <c>content</c>, <c>error-text</c>, <c>error-json</c>, or <c>execution-denied</c>. The last three are sent on <c>response.error</c>.</summary>
    public string OutputKind { get; set; } = "json";

    /// <summary>JSON or text payload.</summary>
    public string? Output { get; set; }

    /// <summary>Reason a tool call was denied.</summary>
    public string? DenialReason { get; set; }

    /// <summary>Multimodal content parts when <see cref="OutputKind"/> is <c>content</c>.</summary>
    public IReadOnlyList<GoogleToolContentPart>? Content { get; set; }

    /// <summary>Provider options keyed by namespace.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
}

/// <summary>One multimodal tool-result part.</summary>
public sealed class GoogleToolContentPart
{
    /// <summary>Creates a content part.</summary>
    public GoogleToolContentPart(string type)
    {
        Type = type ?? "text";
    }

    /// <summary><c>text</c> or <c>file</c>.</summary>
    public string Type { get; }

    /// <summary>Text, when <see cref="Type"/> is <c>text</c>.</summary>
    public string? Text { get; set; }

    /// <summary>Media type of a file.</summary>
    public string? MediaType { get; set; }

    /// <summary><c>data</c> or <c>url</c>.</summary>
    public string? DataKind { get; set; }

    /// <summary>Inline bytes.</summary>
    public byte[]? Data { get; set; }

    /// <summary>URL or data URL.</summary>
    public string? Url { get; set; }
}

/// <summary>Converts prompts into Gemini contents.</summary>
public static class GoogleMessages
{
    /// <summary>Sentinel Gemini documents for replaying a function call whose thought signature was dropped.</summary>
    public const string SkipThoughtSignatureValidator = "skip_thought_signature_validator";

    /// <summary>Converts shared prompt messages.</summary>
    public static GooglePrompt Convert(IReadOnlyList<ModelMessage> prompt, GoogleMessageOptions? options = null)
    {
        return Convert(FromModelMessages(prompt), options);
    }

    /// <summary>Converts rich turns.</summary>
    public static GooglePrompt Convert(IReadOnlyList<GoogleTurn> turns, GoogleMessageOptions? options = null)
    {
        options ??= new GoogleMessageOptions();
        var capabilities = GoogleModelCapability.Get(options.ModelId);
        var isGemma = options.IsGemmaModel ?? GoogleModelCapability.IsGemma(options.ModelId);
        var isGemini3 = options.IsGemini3Model ?? capabilities.UsesGemini3Features;
        var supportsParts = options.SupportsFunctionResponseParts ?? capabilities.UsesGemini3Features;
        var names = options.ProviderOptionNames ?? new[] { "google" };
        var vertexLike = !Contains(names, "google");
        var systemParts = new JsonArray();
        var contents = new JsonArray();
        var systemAllowed = true;
        var missing = new List<string>();
        var sentinel = false;

        foreach (var turn in turns)
        {
            if (turn is GoogleSystemTurn system)
            {
                if (!systemAllowed)
                {
                    throw new InvalidOperationException("system messages are only supported at the beginning of the conversation");
                }

                systemParts.Add(new JsonObject { ["text"] = system.Text });
                continue;
            }

            systemAllowed = false;
            if (turn is GoogleUserTurn user)
            {
                var parts = new JsonArray();
                foreach (var part in user.Parts)
                {
                    if (part is GoogleTextUserPart text)
                    {
                        parts.Add(new JsonObject { ["text"] = text.Text });
                    }
                    else if (part is GoogleFileUserPart file)
                    {
                        parts.Add(UserFile(file, vertexLike));
                    }
                }

                contents.Add(new JsonObject { ["role"] = "user", ["parts"] = parts });
            }
            else if (turn is GoogleAssistantTurn assistant)
            {
                var signed = false;
                var parts = new JsonArray();
                foreach (var part in assistant.Parts)
                {
                    var node = AssistantPart(part, names, vertexLike, isGemini3, options.IncludeFunctionCallIds, ref signed, missing, ref sentinel);
                    if (node != null)
                    {
                        parts.Add(node);
                    }
                }

                contents.Add(new JsonObject { ["role"] = "model", ["parts"] = parts });
            }
            else if (turn is GoogleToolTurn tool)
            {
                var parts = new JsonArray();
                foreach (var result in tool.Results)
                {
                    AppendToolResult(contents, parts, result, names, supportsParts, options.IncludeFunctionCallIds);
                }

                contents.Add(new JsonObject { ["role"] = "user", ["parts"] = parts });
            }
        }

        if (isGemma && systemParts.Count > 0 && contents.Count > 0 && contents[0] is JsonObject first && (string?)first["role"] == "user" && first["parts"] is JsonArray firstParts)
        {
            var text = new StringBuilder();
            for (var i = 0; i < systemParts.Count; i++)
            {
                if (i > 0)
                {
                    text.Append("\n\n");
                }

                text.Append((string?)systemParts[i]!["text"]);
            }

            text.Append("\n\n");
            firstParts.Insert(0, new JsonObject { ["text"] = text.ToString() });
        }

        if (sentinel)
        {
            var unique = new List<string>();
            foreach (var name in missing)
            {
                if (!unique.Contains(name))
                {
                    unique.Add(name);
                }
            }

            var listed = new StringBuilder();
            for (var i = 0; i < unique.Count; i++)
            {
                if (i > 0)
                {
                    listed.Append(", ");
                }

                listed.Append('`').Append(unique[i]).Append('`');
            }

            options.Warnings.Add(new GoogleWarning(
                "other",
                "Replayed " + missing.Count + " `functionCall` part(s) for a Gemini 3 model without a `thoughtSignature` (tools: " + listed + "). Injected the documented `skip_thought_signature_validator` sentinel."));
        }

        JsonObject? instruction = null;
        if (!isGemma && systemParts.Count > 0)
        {
            instruction = new JsonObject { ["parts"] = systemParts };
        }

        return new GooglePrompt(contents, instruction);
    }

    private static List<GoogleTurn> FromModelMessages(IReadOnlyList<ModelMessage> prompt)
    {
        var turns = new List<GoogleTurn>();
        if (prompt == null)
        {
            return turns;
        }

        foreach (var message in prompt)
        {
            if (message is SystemModelMessage system)
            {
                turns.Add(new GoogleSystemTurn(system.Content));
            }
            else if (message is UserModelMessage user)
            {
                var parts = new List<GoogleUserPart>();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart text)
                    {
                        parts.Add(new GoogleTextUserPart(text.Text));
                    }
                    else if (part is FileContentPart file)
                    {
                        var kind = file.Data != null ? "data" : "url";
                        parts.Add(new GoogleFileUserPart(file.MediaType, kind) { Data = file.Data, Url = file.Url });
                    }
                }

                turns.Add(new GoogleUserTurn(parts));
            }
            else if (message is AssistantModelMessage assistant)
            {
                var parts = new List<GoogleAssistantPart>();
                if (!string.IsNullOrEmpty(assistant.Reasoning))
                {
                    parts.Add(new GoogleReasoningAssistantPart(assistant.Reasoning!));
                }

                if (!string.IsNullOrEmpty(assistant.Text))
                {
                    parts.Add(new GoogleTextAssistantPart(assistant.Text!));
                }

                foreach (var call in assistant.ToolCalls)
                {
                    var tool = new GoogleToolCallAssistantPart(call.ToolName, call.ArgumentsJson) { ToolCallId = call.ToolCallId };
                    if (call.ProviderMetadata is { } metadata)
                    {
                        tool.ProviderOptions = new Dictionary<string, JsonElement> { ["google"] = metadata };
                    }

                    parts.Add(tool);
                }

                turns.Add(new GoogleAssistantTurn(parts));
            }
            else if (message is ToolModelMessage tool)
            {
                var result = new GoogleToolResultPart(tool.ToolName)
                {
                    ToolCallId = tool.ToolCallId,
                    OutputKind = tool.OutputType,
                    Output = tool.OutputJson,
                    DenialReason = tool.OutputType == "execution-denied" && tool.OutputJson.Length > 0 ? tool.OutputJson : null,
                };
                if (tool.ProviderMetadata is { } metadata)
                {
                    result.ProviderOptions = new Dictionary<string, JsonElement> { ["google"] = metadata };
                }

                turns.Add(new GoogleToolTurn(new[] { result }));
            }
        }

        return turns;
    }

    private static JsonObject UserFile(GoogleFileUserPart file, bool vertexLike)
    {
        switch (file.Kind)
        {
            case "url":
                var uri = file.Url ?? string.Empty;
                if (uri.StartsWith("gs:", StringComparison.Ordinal) && !string.IsNullOrEmpty(file.OriginalUrl))
                {
                    uri = file.OriginalUrl!;
                }

                return new JsonObject
                {
                    ["fileData"] = new JsonObject { ["mimeType"] = ResolveMediaType(file.MediaType, file.Data), ["fileUri"] = uri },
                };
            case "reference":
                if (vertexLike)
                {
                    throw new InvalidOperationException("file parts with provider references");
                }

                return new JsonObject
                {
                    ["fileData"] = new JsonObject
                    {
                        ["mimeType"] = ResolveMediaType(file.MediaType, file.Data),
                        ["fileUri"] = ResolveReference(file.Reference),
                    },
                };
            case "text":
                return Inline(FullOrPlain(file.MediaType), Encoding.UTF8.GetBytes(file.Text ?? string.Empty));
            default:
                return Inline(ResolveMediaType(file.MediaType, file.Data), file.Data ?? Array.Empty<byte>());
        }
    }

    private static JsonObject? AssistantPart(
        GoogleAssistantPart part,
        IReadOnlyList<string> names,
        bool vertexLike,
        bool isGemini3,
        bool includeIds,
        ref bool signedFunction,
        List<string> missing,
        ref bool sentinel)
    {
        var options = ReadOptions(part.ProviderOptions, names, vertexLike);
        var signature = Signature(options);
        if (part is GoogleTextAssistantPart text)
        {
            if (text.Text.Length == 0)
            {
                return null;
            }

            var node = new JsonObject { ["text"] = text.Text };
            GoogleJson.Set(node, "thoughtSignature", signature);
            return node;
        }

        if (part is GoogleReasoningAssistantPart reasoning)
        {
            if (reasoning.Text.Length == 0)
            {
                return null;
            }

            var node = new JsonObject { ["text"] = reasoning.Text, ["thought"] = true };
            GoogleJson.Set(node, "thoughtSignature", signature);
            return node;
        }

        if (part is GoogleFileAssistantPart file)
        {
            if (file.Kind == "url")
            {
                throw new InvalidOperationException(file.Reasoning
                    ? "File data URLs in assistant messages are not supported"
                    : "File data URLs in assistant messages are not supported");
            }

            if (file.Kind == "reference")
            {
                if (vertexLike)
                {
                    throw new InvalidOperationException("file parts with provider references");
                }

                var node = new JsonObject
                {
                    ["fileData"] = new JsonObject { ["mimeType"] = file.MediaType, ["fileUri"] = ResolveReference(file.Reference) },
                };
                if (Thought(options) || file.Reasoning)
                {
                    node["thought"] = true;
                }

                GoogleJson.Set(node, "thoughtSignature", signature);
                return node;
            }

            byte[] bytes;
            string media;
            if (file.Kind == "text")
            {
                bytes = Encoding.UTF8.GetBytes(file.Text ?? string.Empty);
                media = FullOrPlain(file.MediaType);
            }
            else
            {
                bytes = file.Data ?? Array.Empty<byte>();
                media = file.MediaType;
            }

            var inline = Inline(media, bytes);
            if (file.Reasoning || Thought(options))
            {
                inline["thought"] = true;
            }

            GoogleJson.Set(inline, "thoughtSignature", signature);
            return inline;
        }

        if (part is GoogleToolCallAssistantPart call)
        {
            var serverId = GoogleJson.String(options, "serverToolCallId");
            var serverType = GoogleJson.String(options, "serverToolType");
            var server = serverId != null && serverType != null;
            var skipMitigation = !server && signature == null && signedFunction;
            var effective = signature;
            if (effective == null && isGemini3 && !skipMitigation)
            {
                missing.Add(call.ToolName);
                sentinel = true;
                effective = SkipThoughtSignatureValidator;
            }

            if (!server && signature != null)
            {
                signedFunction = true;
            }

            JsonObject node;
            if (server)
            {
                node = new JsonObject
                {
                    ["toolCall"] = new JsonObject
                    {
                        ["toolType"] = serverType,
                        ["args"] = ParseArgs(call.ArgumentsJson),
                        ["id"] = serverId,
                    },
                };
            }
            else
            {
                var function = new JsonObject { ["name"] = call.ToolName, ["args"] = ParseArgs(call.ArgumentsJson) };
                if (includeIds && !string.IsNullOrEmpty(call.ToolCallId))
                {
                    function["id"] = call.ToolCallId;
                }

                node = new JsonObject { ["functionCall"] = function };
            }

            GoogleJson.Set(node, "thoughtSignature", effective);
            return node;
        }

        return null;
    }

    private static void AppendToolResult(JsonArray contents, JsonArray parts, GoogleToolResultPart result, IReadOnlyList<string> names, bool supportsParts, bool includeIds)
    {
        var options = ReadOptions(result.ProviderOptions, names, !Contains(names, "google"));
        var serverId = GoogleJson.String(options, "serverToolCallId");
        var serverType = GoogleJson.String(options, "serverToolType");
        if (serverId != null && serverType != null && contents.Count > 0 && contents[contents.Count - 1] is JsonObject last && (string?)last["role"] == "model" && last["parts"] is JsonArray modelParts)
        {
            var response = new JsonObject
            {
                ["toolResponse"] = new JsonObject
                {
                    ["toolType"] = serverType,
                    ["response"] = ParseArgs(result.Output ?? "{}"),
                    ["id"] = serverId,
                },
            };
            GoogleJson.Set(response, "thoughtSignature", Signature(options));
            modelParts.Add(response);
            return;
        }

        if (result.OutputKind == "content" && result.Content != null)
        {
            if (supportsParts)
            {
                parts.Add(ModernFunctionResponse(result, includeIds));
            }
            else
            {
                LegacyFunctionResponse(parts, result, includeIds);
            }

            return;
        }

        var body = new JsonObject { ["name"] = result.ToolName };
        switch (result.OutputKind)
        {
            case "execution-denied":
                body["error"] = result.DenialReason ?? "Tool call execution denied.";
                break;
            case "error-text":
                body["error"] = result.Output ?? string.Empty;
                break;
            case "error-json":
                body["error"] = SerializeFunctionResponseContent(ParseLoose(result.Output));
                break;
            case "text":
                body["content"] = result.Output ?? string.Empty;
                break;
            default:
                body["content"] = SerializeFunctionResponseContent(ParseLoose(result.Output));
                break;
        }

        var function = new JsonObject { ["name"] = result.ToolName, ["response"] = body };
        if (includeIds && !string.IsNullOrEmpty(result.ToolCallId))
        {
            function["id"] = result.ToolCallId;
        }

        parts.Add(new JsonObject { ["functionResponse"] = function });
    }

    private static JsonObject ModernFunctionResponse(GoogleToolResultPart result, bool includeIds)
    {
        var texts = new List<string>();
        var files = new JsonArray();
        foreach (var part in result.Content!)
        {
            if (part.Type == "text")
            {
                texts.Add(part.Text ?? string.Empty);
            }
            else if (part.DataKind == "data")
            {
                files.Add(new JsonObject
                {
                    ["inlineData"] = new JsonObject
                    {
                        ["mimeType"] = ResolveMediaType(part.MediaType ?? "application/octet-stream", part.Data),
                        ["data"] = System.Convert.ToBase64String(part.Data ?? Array.Empty<byte>()),
                    },
                });
            }
            else if (part.DataKind == "url" && TryDataUrl(part.Url, out var media, out var data))
            {
                files.Add(new JsonObject { ["inlineData"] = new JsonObject { ["mimeType"] = media, ["data"] = data } });
            }
            else
            {
                texts.Add(JsonSerializer.Serialize(new { type = part.Type, url = part.Url }, GoogleJson.Options));
            }
        }

        var function = new JsonObject
        {
            ["name"] = result.ToolName,
            ["response"] = new JsonObject
            {
                ["name"] = result.ToolName,
                ["content"] = texts.Count > 0 ? string.Join("\n", texts) : "Tool executed successfully.",
            },
        };
        if (includeIds && !string.IsNullOrEmpty(result.ToolCallId))
        {
            function["id"] = result.ToolCallId;
        }

        if (files.Count > 0)
        {
            function["parts"] = files;
        }

        return new JsonObject { ["functionResponse"] = function };
    }

    private static void LegacyFunctionResponse(JsonArray parts, GoogleToolResultPart result, bool includeIds)
    {
        foreach (var part in result.Content!)
        {
            if (part.Type == "text")
            {
                var function = new JsonObject
                {
                    ["name"] = result.ToolName,
                    ["response"] = new JsonObject { ["name"] = result.ToolName, ["content"] = part.Text ?? string.Empty },
                };
                if (includeIds && !string.IsNullOrEmpty(result.ToolCallId))
                {
                    function["id"] = result.ToolCallId;
                }

                parts.Add(new JsonObject { ["functionResponse"] = function });
            }
            else if (part.DataKind == "data")
            {
                var media = ResolveMediaType(part.MediaType ?? "application/octet-stream", part.Data);
                var top = media.StartsWith("image/", StringComparison.Ordinal) ? "image" : "file";
                parts.Add(Inline(media, part.Data ?? Array.Empty<byte>()));
                parts.Add(new JsonObject { ["text"] = "Tool executed successfully and returned this " + top + " as a response" });
            }
            else
            {
                parts.Add(new JsonObject { ["text"] = JsonSerializer.Serialize(new { type = part.Type, url = part.Url }, GoogleJson.Options) });
            }
        }
    }

    // Google reserves { $ref: displayName } in structured function responses for multimodal parts.
    // That conflicts with JSON Schema $ref, so a result that holds one is sent as a JSON string.
    private static JsonNode SerializeFunctionResponseContent(JsonNode value)
    {
        return ContainsSchemaReference(value) ? JsonValue.Create(value.ToJsonString(GoogleJson.Options))! : value;
    }

    private static bool ContainsSchemaReference(JsonNode? value)
    {
        switch (value)
        {
            case JsonArray array:
                foreach (var item in array)
                {
                    if (ContainsSchemaReference(item))
                    {
                        return true;
                    }
                }

                return false;
            case JsonObject obj:
                foreach (var property in obj)
                {
                    if (property.Key == "$ref" || ContainsSchemaReference(property.Value))
                    {
                        return true;
                    }
                }

                return false;
            default:
                return false;
        }
    }

    private static JsonNode ParseLoose(string? output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return JsonValue.Create(string.Empty)!;
        }

        try
        {
            return JsonNode.Parse(output) ?? JsonValue.Create(output)!;
        }
        catch (JsonException)
        {
            return JsonValue.Create(output)!;
        }
    }

    private static JsonNode ParseArgs(string json)
    {
        try
        {
            return JsonNode.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json) ?? new JsonObject();
        }
        catch (JsonException)
        {
            return new JsonObject();
        }
    }

    private static JsonObject Inline(string mediaType, byte[] data)
    {
        return new JsonObject
        {
            ["inlineData"] = new JsonObject { ["mimeType"] = mediaType, ["data"] = System.Convert.ToBase64String(data) },
        };
    }

    private static string ResolveMediaType(string mediaType, byte[]? data)
    {
        if (mediaType != null && mediaType.Contains("/"))
        {
            if (mediaType.EndsWith("/*", StringComparison.Ordinal))
            {
                return Detect(data) ?? "application/octet-stream";
            }

            return mediaType;
        }

        if (string.Equals(mediaType, "image", StringComparison.OrdinalIgnoreCase))
        {
            return Detect(data) ?? throw new InvalidOperationException("image media type requires inline bytes");
        }

        return string.IsNullOrEmpty(mediaType) ? "application/octet-stream" : mediaType + "/octet-stream";
    }

    private static string FullOrPlain(string? mediaType)
    {
        return mediaType != null && mediaType.Contains("/") ? mediaType : "text/plain";
    }

    private static string? Detect(byte[]? data)
    {
        if (data == null || data.Length < 4)
        {
            return null;
        }

        if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
        {
            return "image/png";
        }

        if (data[0] == 0xFF && data[1] == 0xD8)
        {
            return "image/jpeg";
        }

        if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46)
        {
            return "image/gif";
        }

        return null;
    }

    private static string ResolveReference(JsonElement? reference)
    {
        if (reference is { } element && element.ValueKind == JsonValueKind.Object && element.TryGetProperty("google", out var google) && google.ValueKind == JsonValueKind.String)
        {
            return google.GetString() ?? string.Empty;
        }

        if (reference is { ValueKind: JsonValueKind.String })
        {
            return reference.Value.GetString() ?? string.Empty;
        }

        throw new InvalidOperationException("file parts with provider references require a google file URI");
    }

    private static bool TryDataUrl(string? url, out string media, out string data)
    {
        media = string.Empty;
        data = string.Empty;
        if (url == null || !url.StartsWith("data:", StringComparison.Ordinal))
        {
            return false;
        }

        var comma = url.IndexOf(',');
        var marker = url.IndexOf(";base64", StringComparison.Ordinal);
        if (comma < 0 || marker < 0 || marker > comma)
        {
            return false;
        }

        media = url.Substring(5, marker - 5);
        data = url.Substring(comma + 1);
        return media.Length > 0 && data.Length > 0;
    }

    private static JsonElement ReadOptions(IReadOnlyDictionary<string, JsonElement>? options, IReadOnlyList<string> names, bool vertexLike)
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

            if (vertexLike && options.TryGetValue("google", out var google))
            {
                return google;
            }

            if (!vertexLike)
            {
                if (options.TryGetValue("googleVertex", out var vertex))
                {
                    return vertex;
                }

                if (options.TryGetValue("vertex", out var legacy))
                {
                    return legacy;
                }
            }
        }

        return default;
    }

    private static string? Signature(JsonElement options)
    {
        return options.ValueKind == JsonValueKind.Object ? GoogleJson.String(options, "thoughtSignature") : null;
    }

    private static bool Thought(JsonElement options)
    {
        return options.ValueKind == JsonValueKind.Object
            && options.TryGetProperty("thought", out var thought)
            && thought.ValueKind == JsonValueKind.True;
    }

    private static bool Contains(IReadOnlyList<string> names, string value)
    {
        for (var i = 0; i < names.Count; i++)
        {
            if (names[i] == value)
            {
                return true;
            }
        }

        return false;
    }
}
