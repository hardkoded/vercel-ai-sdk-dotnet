// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>One Gemini prompt part that the standard message types cannot express.</summary>
public sealed class GooglePromptPart
{
    private GooglePromptPart(string type)
    {
        Type = type;
    }

    /// <summary>Part type: text, file, reasoning, reasoning-file, tool-call, tool-result, or tool-approval-response.</summary>
    public string Type { get; }

    /// <summary>Text or reasoning text.</summary>
    public string? Text { get; private set; }

    /// <summary>IANA media type.</summary>
    public string? MediaType { get; private set; }

    /// <summary>File data kind: data, url, reference, or text.</summary>
    public string? DataType { get; private set; }

    /// <summary>Remote file URI.</summary>
    public string? Url { get; private set; }

    /// <summary>Original URI preserved for <c>gs:</c> files.</summary>
    public string? OriginalUrl { get; private set; }

    /// <summary>Inline bytes. Used when <see cref="Base64"/> is unset.</summary>
    public byte[]? Data { get; private set; }

    /// <summary>Base64 payload already encoded for Gemini.</summary>
    public string? Base64 { get; private set; }

    /// <summary>JSON object of provider file references.</summary>
    public string? ReferenceJson { get; private set; }

    /// <summary>Tool call id.</summary>
    public string? ToolCallId { get; private set; }

    /// <summary>Tool name.</summary>
    public string? ToolName { get; private set; }

    /// <summary>JSON tool input.</summary>
    public string? InputJson { get; private set; }

    /// <summary>Tool output kind: json, text, content, or execution-denied.</summary>
    public string? OutputType { get; private set; }

    /// <summary>JSON or text tool output.</summary>
    public string? OutputJson { get; private set; }

    /// <summary>Reason a tool call was denied.</summary>
    public string? DenialReason { get; private set; }

    /// <summary>Multimodal tool output parts.</summary>
    public IReadOnlyList<GooglePromptPart> OutputParts { get; private set; } = Array.Empty<GooglePromptPart>();

    /// <summary>Per-part provider options, keyed by provider name.</summary>
    public JsonElement? ProviderOptions { get; private set; }

    /// <summary>Creates a text part.</summary>
    public static GooglePromptPart TextPart(string text, JsonElement? providerOptions = null)
    {
        return new GooglePromptPart("text") { Text = text ?? string.Empty, ProviderOptions = providerOptions };
    }

    /// <summary>Creates a reasoning part.</summary>
    public static GooglePromptPart ReasoningPart(string text, JsonElement? providerOptions = null)
    {
        return new GooglePromptPart("reasoning") { Text = text ?? string.Empty, ProviderOptions = providerOptions };
    }

    /// <summary>Creates an inline file part.</summary>
    public static GooglePromptPart FileBytes(byte[] data, string mediaType, JsonElement? providerOptions = null)
    {
        return new GooglePromptPart("file")
        {
            DataType = "data",
            Data = data ?? Array.Empty<byte>(),
            MediaType = mediaType ?? "application/octet-stream",
            ProviderOptions = providerOptions,
        };
    }

    /// <summary>Creates an inline file part from base64 text.</summary>
    public static GooglePromptPart FileBase64(string base64, string mediaType, JsonElement? providerOptions = null)
    {
        return new GooglePromptPart("file")
        {
            DataType = "data",
            Base64 = base64 ?? string.Empty,
            MediaType = mediaType ?? "application/octet-stream",
            ProviderOptions = providerOptions,
        };
    }

    /// <summary>Creates a file URI part.</summary>
    public static GooglePromptPart FileUrl(string url, string mediaType, string? originalUrl = null)
    {
        return new GooglePromptPart("file")
        {
            DataType = "url",
            Url = url ?? string.Empty,
            OriginalUrl = originalUrl,
            MediaType = mediaType ?? "application/octet-stream",
        };
    }

    /// <summary>Creates a provider file reference part.</summary>
    public static GooglePromptPart FileReference(string referenceJson, string mediaType)
    {
        return FileReference(referenceJson, mediaType, null);
    }

    /// <summary>Creates a provider file reference part with provider options.</summary>
    public static GooglePromptPart FileReference(string referenceJson, string mediaType, JsonElement? providerOptions)
    {
        return new GooglePromptPart("file")
        {
            DataType = "reference",
            ReferenceJson = referenceJson ?? "{}",
            MediaType = mediaType ?? "application/octet-stream",
            ProviderOptions = providerOptions,
        };
    }

    /// <summary>Creates a text-backed file part.</summary>
    public static GooglePromptPart FileText(string text, string? mediaType)
    {
        return new GooglePromptPart("file")
        {
            DataType = "text",
            Text = text ?? string.Empty,
            MediaType = mediaType,
        };
    }

    /// <summary>Creates a reasoning file part.</summary>
    public static GooglePromptPart ReasoningFile(string dataType, string? base64, byte[]? data, string? url, string mediaType, JsonElement? providerOptions)
    {
        return new GooglePromptPart("reasoning-file")
        {
            DataType = dataType,
            Base64 = base64,
            Data = data,
            Url = url,
            MediaType = mediaType,
            ProviderOptions = providerOptions,
        };
    }

    /// <summary>Creates a tool-call part.</summary>
    public static GooglePromptPart ToolCall(string toolCallId, string toolName, string inputJson, JsonElement? providerOptions = null)
    {
        return new GooglePromptPart("tool-call")
        {
            ToolCallId = toolCallId,
            ToolName = toolName ?? string.Empty,
            InputJson = inputJson ?? "{}",
            ProviderOptions = providerOptions,
        };
    }

    /// <summary>Creates a JSON tool result.</summary>
    public static GooglePromptPart ToolResultJson(string toolCallId, string toolName, string outputJson, JsonElement? providerOptions = null)
    {
        return new GooglePromptPart("tool-result")
        {
            ToolCallId = toolCallId,
            ToolName = toolName ?? string.Empty,
            OutputType = "json",
            OutputJson = outputJson ?? "null",
            ProviderOptions = providerOptions,
        };
    }

    /// <summary>Creates a text tool result.</summary>
    public static GooglePromptPart ToolResultText(string toolCallId, string toolName, string text, JsonElement? providerOptions = null)
    {
        return new GooglePromptPart("tool-result")
        {
            ToolCallId = toolCallId,
            ToolName = toolName ?? string.Empty,
            OutputType = "text",
            OutputJson = text ?? string.Empty,
            ProviderOptions = providerOptions,
        };
    }

    /// <summary>Creates a multimodal tool result.</summary>
    public static GooglePromptPart ToolResultContent(string toolCallId, string toolName, IReadOnlyList<GooglePromptPart> parts, JsonElement? providerOptions = null)
    {
        return new GooglePromptPart("tool-result")
        {
            ToolCallId = toolCallId,
            ToolName = toolName ?? string.Empty,
            OutputType = "content",
            OutputParts = parts ?? Array.Empty<GooglePromptPart>(),
            ProviderOptions = providerOptions,
        };
    }

    /// <summary>Creates a denied tool result.</summary>
    public static GooglePromptPart ToolResultDenied(string toolCallId, string toolName, string? reason)
    {
        return new GooglePromptPart("tool-result")
        {
            ToolCallId = toolCallId,
            ToolName = toolName ?? string.Empty,
            OutputType = "execution-denied",
            DenialReason = reason,
        };
    }

    /// <summary>Creates a tool approval response that Gemini omits.</summary>
    public static GooglePromptPart ToolApproval()
    {
        return new GooglePromptPart("tool-approval-response");
    }
}

/// <summary>A prompt message with ordered Gemini parts.</summary>
public sealed class GooglePromptMessage : ModelMessage
{
    /// <summary>Creates a rich prompt message.</summary>
    public GooglePromptMessage(string role, string? text, IReadOnlyList<GooglePromptPart>? parts)
        : base(role ?? "user")
    {
        Text = text;
        Parts = parts ?? Array.Empty<GooglePromptPart>();
    }

    /// <summary>System text when <see cref="ModelMessage.Role"/> is system.</summary>
    public string? Text { get; }

    /// <summary>Ordered parts.</summary>
    public IReadOnlyList<GooglePromptPart> Parts { get; }
}

/// <summary>Gemini <c>contents</c> produced from a prompt.</summary>
public sealed class GooglePromptConversion
{
    internal GooglePromptConversion(JsonArray contents, JsonObject? systemInstruction, List<CallWarning> warnings)
    {
        Contents = contents;
        SystemInstruction = systemInstruction;
        Warnings = warnings;
    }

    /// <summary>Gemini contents array.</summary>
    public JsonArray Contents { get; }

    /// <summary>System instruction, omitted for Gemma.</summary>
    public JsonObject? SystemInstruction { get; }

    /// <summary>Warnings raised while converting the prompt.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }
}

/// <summary>Converts SDK messages into Gemini contents.</summary>
public static class GoogleMessages
{
    /// <summary>Sentinel Gemini documents for a replayed function call that has no thought signature.</summary>
    public const string SkipThoughtSignatureValidator = "skip_thought_signature_validator";

    private static readonly Regex DataUrl = new Regex(@"^data:([^;,]+);base64,(.+)$", RegexOptions.Singleline | RegexOptions.CultureInvariant);

    /// <summary>Converts <paramref name="prompt"/> using Google provider metadata.</summary>
    public static GooglePromptConversion Convert(IReadOnlyList<ModelMessage> prompt, string modelId, bool vertex)
    {
        var capabilities = GoogleModelCapabilitiesMap.Get(modelId);
        var names = vertex
            ? new[] { "googleVertex", "vertex" }
            : new[] { "google" };
        return Convert(prompt, new GoogleMessageConversionOptions
        {
            IsGemmaModel = (modelId ?? string.Empty).ToLowerInvariant().StartsWith("gemma-", StringComparison.Ordinal),
            IsGemini3Model = capabilities.UsesGemini3Features,
            ProviderOptionsNames = names,
            SupportsFunctionResponseParts = capabilities.UsesGemini3Features,
            IncludeFunctionCallIds = !vertex,
        });
    }

    /// <summary>Converts <paramref name="prompt"/> with explicit flags.</summary>
    public static GooglePromptConversion Convert(IReadOnlyList<ModelMessage> prompt, GoogleMessageConversionOptions options)
    {
        options = options ?? new GoogleMessageConversionOptions();
        var names = options.ProviderOptionsNames ?? new[] { "google" };
        var vertexLike = true;
        foreach (var name in names)
        {
            if (name == "google")
            {
                vertexLike = false;
                break;
            }
        }

        var systemParts = new JsonArray();
        var contents = new JsonArray();
        var warnings = new List<CallWarning>();
        var systemAllowed = true;
        var sentinel = false;
        var missing = new List<string>();
        foreach (var message in prompt ?? Array.Empty<ModelMessage>())
        {
            if (message is SystemModelMessage system || (message is GooglePromptMessage richSystem && richSystem.Role == "system"))
            {
                if (!systemAllowed)
                {
                    throw new InvalidOperationException("system messages are only supported at the beginning of the conversation");
                }

                var text = message is SystemModelMessage systemMessage ? systemMessage.Content : ((GooglePromptMessage)message).Text ?? string.Empty;
                systemParts.Add(new JsonObject { ["text"] = text });
                continue;
            }

            systemAllowed = false;
            if (message is UserModelMessage user)
            {
                contents.Add(new JsonObject { ["role"] = "user", ["parts"] = UserParts(user, vertexLike) });
            }
            else if (message is GooglePromptMessage rich && rich.Role == "user")
            {
                contents.Add(new JsonObject { ["role"] = "user", ["parts"] = RichParts(rich.Parts, vertexLike, assistant: false) });
            }
            else if (message is AssistantModelMessage assistant)
            {
                contents.Add(ModelContent(AssistantParts(assistant, names, vertexLike, options, missing, ref sentinel)));
            }
            else if (message is GooglePromptMessage richAssistant && richAssistant.Role == "assistant")
            {
                contents.Add(ModelContent(AssistantRichParts(richAssistant.Parts, names, vertexLike, options, missing, ref sentinel)));
            }
            else if (message is ToolModelMessage tool)
            {
                contents.Add(new JsonObject
                {
                    ["role"] = "user",
                    ["parts"] = new JsonArray { FunctionResponse(tool.ToolCallId, tool.ToolName, GoogleJson.TryParse(tool.OutputJson, out var parsed) ? parsed : JsonValue.Create(tool.OutputJson), options.IncludeFunctionCallIds) },
                });
            }
            else if (message is GooglePromptMessage richTool && richTool.Role == "tool")
            {
                var toolParts = ToolParts(richTool.Parts, contents, names, vertexLike, options);
                if (toolParts.Count > 0)
                {
                    contents.Add(new JsonObject
                    {
                        ["role"] = "user",
                        ["parts"] = toolParts,
                    });
                }
            }
        }

        if (options.IsGemmaModel && systemParts.Count > 0 && contents.Count > 0 && contents[0] is JsonObject first && first["role"]?.GetValue<string>() == "user")
        {
            var text = new StringBuilder();
            for (var i = 0; i < systemParts.Count; i++)
            {
                if (i > 0)
                {
                    text.Append("\n\n");
                }

                text.Append(systemParts[i]!["text"]!.GetValue<string>());
            }

            text.Append("\n\n");
            var parts = (JsonArray)first["parts"]!;
            parts.Insert(0, new JsonObject { ["text"] = text.ToString() });
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

            var tools = new StringBuilder();
            for (var i = 0; i < unique.Count; i++)
            {
                if (i > 0)
                {
                    tools.Append(", ");
                }

                tools.Append('`').Append(unique[i]).Append('`');
            }

            warnings.Add(new CallWarning(
                "other",
                "Replayed " + missing.Count + " `functionCall` part(s) " +
                "for a Gemini 3 model without a `thoughtSignature` " +
                "(tools: " + tools + "). " +
                "Injected the documented `skip_thought_signature_validator` sentinel " +
                "to keep the request from failing with HTTP 400. " +
                "The likely cause is application code that drops " +
                "`providerOptions.google.thoughtSignature` when persisting or " +
                "serializing assistant tool-call messages. " +
                "See https://ai.google.dev/gemini-api/docs/thought-signatures."));
        }

        JsonObject? instruction = null;
        if (systemParts.Count > 0 && !options.IsGemmaModel)
        {
            instruction = new JsonObject { ["parts"] = systemParts };
        }

        return new GooglePromptConversion(contents, instruction, warnings);
    }

    private static JsonObject ModelContent(JsonArray parts)
    {
        return new JsonObject { ["role"] = "model", ["parts"] = parts };
    }

    private static JsonArray UserParts(UserModelMessage user, bool vertexLike)
    {
        var parts = new JsonArray();
        foreach (var part in user.Content)
        {
            if (part is TextContentPart text)
            {
                parts.Add(new JsonObject { ["text"] = text.Text });
            }
            else if (part is FileContentPart file)
            {
                parts.Add(FilePart(file, vertexLike));
            }
        }

        return parts;
    }

    private static JsonObject FilePart(FileContentPart file, bool vertexLike)
    {
        if (file.Data != null)
        {
            return new JsonObject
            {
                ["inlineData"] = new JsonObject
                {
                    ["mimeType"] = FullMediaType(file.MediaType),
                    ["data"] = System.Convert.ToBase64String(file.Data),
                },
            };
        }

        return new JsonObject
        {
            ["fileData"] = new JsonObject
            {
                ["mimeType"] = FullMediaType(file.MediaType),
                ["fileUri"] = file.Url ?? string.Empty,
            },
        };
    }

    private static JsonArray RichParts(IReadOnlyList<GooglePromptPart> source, bool vertexLike, bool assistant)
    {
        var parts = new JsonArray();
        foreach (var part in source)
        {
            if (part.Type == "text")
            {
                parts.Add(new JsonObject { ["text"] = part.Text ?? string.Empty });
            }
            else if (part.Type == "file")
            {
                parts.Add(RichFile(part, vertexLike, assistant, thought: false));
            }
        }

        return parts;
    }

    private static JsonObject RichFile(GooglePromptPart part, bool vertexLike, bool assistant, bool thought)
    {
        var options = ReadOptions(part.ProviderOptions, vertexLike ? new[] { "googleVertex", "vertex" } : new[] { "google" }, vertexLike);
        var signature = StringOpt(options, "thoughtSignature");
        if (part.DataType == "url")
        {
            if (assistant)
            {
                throw new InvalidOperationException("File data URLs in assistant messages are not supported");
            }

            var uri = part.Url ?? string.Empty;
            if (uri.StartsWith("gs:", StringComparison.Ordinal) && !string.IsNullOrEmpty(part.OriginalUrl))
            {
                uri = part.OriginalUrl!;
            }

            return WithSignature(new JsonObject
            {
                ["fileData"] = new JsonObject
                {
                    ["mimeType"] = FullMediaType(part.MediaType),
                    ["fileUri"] = uri,
                },
            }, signature, thought || BoolOpt(options, "thought"));
        }

        if (part.DataType == "reference")
        {
            if (vertexLike)
            {
                throw new InvalidOperationException("file parts with provider references");
            }

            return WithSignature(new JsonObject
            {
                ["fileData"] = new JsonObject
                {
                    ["mimeType"] = part.MediaType ?? "application/octet-stream",
                    ["fileUri"] = ResolveReference(part.ReferenceJson),
                },
            }, signature, thought || BoolOpt(options, "thought"));
        }

        if (part.DataType == "text")
        {
            var media = IsFullMediaType(part.MediaType) ? part.MediaType! : "text/plain";
            return WithSignature(new JsonObject
            {
                ["inlineData"] = new JsonObject
                {
                    ["mimeType"] = media,
                    ["data"] = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(part.Text ?? string.Empty)),
                },
            }, signature, thought || BoolOpt(options, "thought"));
        }

        var mimeType = part.DataType == "text"
            ? (IsFullMediaType(part.MediaType) ? part.MediaType! : "text/plain")
            : assistant ? (part.MediaType ?? "application/octet-stream") : FullMediaType(part.MediaType);
        return WithSignature(new JsonObject
        {
            ["inlineData"] = new JsonObject
            {
                ["mimeType"] = mimeType,
                ["data"] = part.Base64 ?? System.Convert.ToBase64String(part.Data ?? Array.Empty<byte>()),
            },
        }, signature, thought || BoolOpt(options, "thought"));
    }

    private static JsonArray AssistantParts(
        AssistantModelMessage assistant,
        IReadOnlyList<string> names,
        bool vertexLike,
        GoogleMessageConversionOptions options,
        List<string> missing,
        ref bool sentinel)
    {
        var parts = new JsonArray();
        var signed = false;
        if (!string.IsNullOrEmpty(assistant.Reasoning))
        {
            parts.Add(new JsonObject { ["text"] = assistant.Reasoning, ["thought"] = true });
        }

        if (!string.IsNullOrEmpty(assistant.Text))
        {
            parts.Add(new JsonObject { ["text"] = assistant.Text });
        }

        foreach (var call in assistant.ToolCalls)
        {
            parts.Add(FunctionCallPart(call.ToolCallId, call.ToolName, call.ArgumentsJson, call.ProviderMetadata, names, vertexLike, options, ref signed, missing, ref sentinel, server: false));
        }

        return parts;
    }

    private static JsonArray AssistantRichParts(
        IReadOnlyList<GooglePromptPart> source,
        IReadOnlyList<string> names,
        bool vertexLike,
        GoogleMessageConversionOptions options,
        List<string> missing,
        ref bool sentinel)
    {
        var parts = new JsonArray();
        var signed = false;
        foreach (var part in source)
        {
            var providerOptions = ReadOptions(part.ProviderOptions, names, vertexLike);
            var signature = StringOpt(providerOptions, "thoughtSignature");
            if (part.Type == "text")
            {
                if ((part.Text ?? string.Empty).Length == 0)
                {
                    continue;
                }

                var text = new JsonObject { ["text"] = part.Text };
                if (signature != null)
                {
                    text["thoughtSignature"] = signature;
                }

                parts.Add(text);
            }
            else if (part.Type == "reasoning")
            {
                if ((part.Text ?? string.Empty).Length == 0)
                {
                    continue;
                }

                var text = new JsonObject { ["text"] = part.Text, ["thought"] = true };
                if (signature != null)
                {
                    text["thoughtSignature"] = signature;
                }

                parts.Add(text);
            }
            else if (part.Type == "reasoning-file")
            {
                if (part.DataType == "url")
                {
                    throw new InvalidOperationException("File data URLs in assistant messages are not supported");
                }

                parts.Add(RichFile(part, vertexLike, assistant: true, thought: true));
            }
            else if (part.Type == "file")
            {
                parts.Add(RichFile(part, vertexLike, assistant: true, thought: false));
            }
            else if (part.Type == "tool-call")
            {
                var serverId = StringOpt(providerOptions, "serverToolCallId");
                var serverType = StringOpt(providerOptions, "serverToolType");
                parts.Add(FunctionCallPart(part.ToolCallId, part.ToolName, part.InputJson, part.ProviderOptions, names, vertexLike, options, ref signed, missing, ref sentinel, serverId != null && serverType != null));
            }
            else if (part.Type == "tool-result")
            {
                var serverId = StringOpt(providerOptions, "serverToolCallId");
                var serverType = StringOpt(providerOptions, "serverToolType");
                if (serverId != null && serverType != null)
                {
                    var response = new JsonObject
                    {
                        ["toolResponse"] = new JsonObject
                        {
                            ["toolType"] = serverType,
                            ["response"] = part.OutputType == "json" && GoogleJson.TryParse(part.OutputJson, out var parsed) ? parsed : new JsonObject(),
                            ["id"] = serverId,
                        },
                    };
                    if (signature != null)
                    {
                        response["thoughtSignature"] = signature;
                    }

                    parts.Add(response);
                }
            }
        }

        return parts;
    }

    private static JsonObject FunctionCallPart(
        string? toolCallId,
        string? toolName,
        string? inputJson,
        JsonElement? providerMetadata,
        IReadOnlyList<string> names,
        bool vertexLike,
        GoogleMessageConversionOptions options,
        ref bool signed,
        List<string> missing,
        ref bool sentinel,
        bool server)
    {
        var providerOptions = ReadOptions(providerMetadata, names, vertexLike);
        var signature = StringOpt(providerOptions, "thoughtSignature");
        var serverId = StringOpt(providerOptions, "serverToolCallId");
        var serverType = StringOpt(providerOptions, "serverToolType");
        var isServer = server && serverId != null && serverType != null;
        if (!isServer)
        {
            isServer = serverId != null && serverType != null;
        }

        var skip = !isServer && signature == null && signed;
        string? effective = signature;
        if (effective == null && options.IsGemini3Model && !skip)
        {
            missing.Add(toolName ?? string.Empty);
            sentinel = true;
            effective = SkipThoughtSignatureValidator;
        }

        if (!isServer && signature != null)
        {
            signed = true;
        }

        JsonNode args = new JsonObject();
        if (GoogleJson.TryParse(string.IsNullOrWhiteSpace(inputJson) ? "{}" : inputJson, out var parsed))
        {
            args = parsed;
        }

        if (isServer)
        {
            var part = new JsonObject
            {
                ["toolCall"] = new JsonObject
                {
                    ["toolType"] = serverType,
                    ["args"] = args,
                    ["id"] = serverId,
                },
            };
            if (effective != null)
            {
                part["thoughtSignature"] = effective;
            }

            return part;
        }

        var call = new JsonObject { ["name"] = toolName ?? string.Empty, ["args"] = args };
        if (options.IncludeFunctionCallIds && toolCallId != null)
        {
            call["id"] = toolCallId;
        }

        var function = new JsonObject { ["functionCall"] = call };
        if (effective != null)
        {
            function["thoughtSignature"] = effective;
        }

        return function;
    }

    private static JsonArray ToolParts(
        IReadOnlyList<GooglePromptPart> source,
        JsonArray contents,
        IReadOnlyList<string> names,
        bool vertexLike,
        GoogleMessageConversionOptions options)
    {
        var parts = new JsonArray();
        foreach (var part in source)
        {
            if (part.Type == "tool-approval-response")
            {
                continue;
            }

            var providerOptions = ReadOptions(part.ProviderOptions, names, vertexLike);
            var serverId = StringOpt(providerOptions, "serverToolCallId");
            var serverType = StringOpt(providerOptions, "serverToolType");
            if (serverId != null && serverType != null && contents.Count > 0 && contents[contents.Count - 1] is JsonObject last && last["role"]?.GetValue<string>() == "model")
            {
                var response = new JsonObject
                {
                    ["toolResponse"] = new JsonObject
                    {
                        ["toolType"] = serverType,
                        ["response"] = part.OutputType == "json" && GoogleJson.TryParse(part.OutputJson, out var parsed) ? parsed : new JsonObject(),
                        ["id"] = serverId,
                    },
                };
                var signature = StringOpt(providerOptions, "thoughtSignature");
                if (signature != null)
                {
                    response["thoughtSignature"] = signature;
                }

                ((JsonArray)last["parts"]!).Add(response);
                continue;
            }

            if (part.OutputType == "content")
            {
                if (options.SupportsFunctionResponseParts)
                {
                    AppendToolResult(parts, part, options.IncludeFunctionCallIds);
                }
                else
                {
                    AppendLegacyToolResult(parts, part, options.IncludeFunctionCallIds);
                }
            }
            else
            {
                JsonNode content = part.OutputType == "execution-denied"
                    ? JsonValue.Create(part.DenialReason ?? "Tool call execution denied.")!
                    : part.OutputType == "text"
                        ? JsonValue.Create(part.OutputJson ?? string.Empty)!
                        : GoogleJson.TryParse(part.OutputJson, out var parsed) ? parsed : JsonValue.Create(part.OutputJson ?? string.Empty)!;
                parts.Add(FunctionResponse(part.ToolCallId, part.ToolName, content, options.IncludeFunctionCallIds));
            }
        }

        return parts;
    }

    private static void AppendToolResult(JsonArray parts, GooglePromptPart part, bool includeIds)
    {
        var files = new JsonArray();
        var texts = new List<string>();
        foreach (var content in part.OutputParts)
        {
            if (content.Type == "text")
            {
                texts.Add(content.Text ?? string.Empty);
            }
            else if (content.Type == "file" && content.DataType == "data")
            {
                files.Add(new JsonObject
                {
                    ["inlineData"] = new JsonObject
                    {
                        ["mimeType"] = FullMediaType(content.MediaType),
                        ["data"] = content.Base64 ?? System.Convert.ToBase64String(content.Data ?? Array.Empty<byte>()),
                    },
                });
            }
            else if (content.Type == "file" && content.DataType == "url" && TryDataUrl(content.Url, out var media, out var data))
            {
                files.Add(new JsonObject
                {
                    ["inlineData"] = new JsonObject { ["mimeType"] = media, ["data"] = data },
                });
            }
            else
            {
                texts.Add(StringifyPart(content));
            }
        }

        var response = new JsonObject
        {
            ["name"] = part.ToolName,
            ["content"] = texts.Count > 0 ? string.Join("\n", texts) : "Tool executed successfully.",
        };
        var function = new JsonObject { ["name"] = part.ToolName, ["response"] = response };
        if (includeIds && part.ToolCallId != null)
        {
            function["id"] = part.ToolCallId;
        }

        if (files.Count > 0)
        {
            function["parts"] = files;
        }

        parts.Add(new JsonObject { ["functionResponse"] = function });
    }

    private static void AppendLegacyToolResult(JsonArray parts, GooglePromptPart part, bool includeIds)
    {
        foreach (var content in part.OutputParts)
        {
            if (content.Type == "text")
            {
                parts.Add(FunctionResponse(part.ToolCallId, part.ToolName, JsonValue.Create(content.Text ?? string.Empty)!, includeIds));
            }
            else if (content.Type == "file" && content.DataType == "data")
            {
                var top = TopLevel(content.MediaType);
                parts.Add(new JsonObject
                {
                    ["inlineData"] = new JsonObject
                    {
                        ["mimeType"] = FullMediaType(content.MediaType),
                        ["data"] = content.Base64 ?? System.Convert.ToBase64String(content.Data ?? Array.Empty<byte>()),
                    },
                });
                parts.Add(new JsonObject
                {
                    ["text"] = "Tool executed successfully and returned this " + (top == "image" ? "image" : "file") + " as a response",
                });
            }
            else
            {
                parts.Add(new JsonObject { ["text"] = StringifyPart(content) });
            }
        }
    }

    private static JsonObject FunctionResponse(string? id, string? name, JsonNode content, bool includeIds)
    {
        var response = new JsonObject { ["name"] = name ?? string.Empty, ["content"] = content };
        var function = new JsonObject { ["name"] = name ?? string.Empty, ["response"] = response };
        if (includeIds && id != null)
        {
            function["id"] = id;
        }

        return new JsonObject { ["functionResponse"] = function };
    }

    private static string StringifyPart(GooglePromptPart part)
    {
        var data = new JsonObject { ["type"] = part.DataType ?? "url" };
        if (part.DataType == "url")
        {
            data["url"] = part.Url ?? string.Empty;
        }

        return new JsonObject
        {
            ["type"] = part.Type,
            ["data"] = data,
            ["mediaType"] = part.MediaType ?? string.Empty,
        }.ToJsonString();
    }

    private static bool TryDataUrl(string? url, out string media, out string data)
    {
        media = string.Empty;
        data = string.Empty;
        if (url == null)
        {
            return false;
        }

        var match = DataUrl.Match(url);
        if (!match.Success)
        {
            return false;
        }

        media = match.Groups[1].Value;
        data = match.Groups[2].Value;
        return true;
    }

    private static JsonObject WithSignature(JsonObject part, string? signature, bool thought)
    {
        if (thought)
        {
            part["thought"] = true;
        }

        if (signature != null)
        {
            part["thoughtSignature"] = signature;
        }

        return part;
    }

    private static string ResolveReference(string? referenceJson)
    {
        if (!GoogleJson.TryParse(referenceJson, out var node) || node is not JsonObject obj)
        {
            throw new InvalidOperationException("No provider reference found for provider 'google'. Available providers: ");
        }

        if (obj["google"] is JsonValue value)
        {
            return value.GetValue<string>();
        }

        var names = new List<string>();
        foreach (var pair in obj)
        {
            names.Add(pair.Key);
        }

        throw new InvalidOperationException("No provider reference found for provider 'google'. Available providers: " + string.Join(", ", names));
    }

    private static JsonElement? ReadOptions(JsonElement? providerOptions, IReadOnlyList<string> names, bool vertexLike)
    {
        if (providerOptions is not { ValueKind: JsonValueKind.Object } element)
        {
            return null;
        }

        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object)
            {
                return value;
            }
        }

        if (vertexLike && element.TryGetProperty("google", out var google) && google.ValueKind == JsonValueKind.Object)
        {
            return google;
        }

        if (!vertexLike)
        {
            if (element.TryGetProperty("googleVertex", out var googleVertex) && googleVertex.ValueKind == JsonValueKind.Object)
            {
                return googleVertex;
            }

            if (element.TryGetProperty("vertex", out var vertex) && vertex.ValueKind == JsonValueKind.Object)
            {
                return vertex;
            }
        }

        if (element.TryGetProperty("thoughtSignature", out _) || element.TryGetProperty("serverToolCallId", out _))
        {
            return element;
        }

        return null;
    }

    private static string? StringOpt(JsonElement? element, string name)
    {
        if (element is not { ValueKind: JsonValueKind.Object } obj || !obj.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static bool BoolOpt(JsonElement? element, string name)
    {
        return element is { ValueKind: JsonValueKind.Object } obj
            && obj.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.True;
    }

    private static string FullMediaType(string? mediaType)
    {
        return IsFullMediaType(mediaType) ? mediaType! : "application/octet-stream";
    }

    private static bool IsFullMediaType(string? mediaType)
    {
        return !string.IsNullOrEmpty(mediaType) && mediaType!.IndexOf('/') >= 0;
    }

    private static string TopLevel(string? mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
        {
            return string.Empty;
        }

        var slash = mediaType!.IndexOf('/');
        return slash < 0 ? mediaType : mediaType.Substring(0, slash);
    }
}

/// <summary>Flags for <see cref="GoogleMessages.Convert(System.Collections.Generic.IReadOnlyList{ModelMessage}, GoogleMessageConversionOptions)"/>.</summary>
public sealed class GoogleMessageConversionOptions
{
    /// <summary>Gemma models receive system text on the first user turn.</summary>
    public bool IsGemmaModel { get; set; }

    /// <summary>Gemini 3 models receive the thought-signature sentinel.</summary>
    public bool IsGemini3Model { get; set; }

    /// <summary>Provider option names, first match wins.</summary>
    public IReadOnlyList<string>? ProviderOptionsNames { get; set; }

    /// <summary>When false, images in tool results use the legacy inline parts.</summary>
    public bool SupportsFunctionResponseParts { get; set; } = true;

    /// <summary>When false, function call and response ids are omitted. Vertex does this.</summary>
    public bool IncludeFunctionCallIds { get; set; } = true;
}
