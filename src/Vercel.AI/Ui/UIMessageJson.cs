// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Ui;

/// <summary>
/// Converts a <see cref="UIMessage"/> to and from its JSON form. The streaming state mutates the JSON form,
/// as upstream mutates the plain <c>UIMessage</c> object.
/// </summary>
internal static class UIMessageJson
{
    private static readonly string[] ToolStates =
    [
        "input-streaming",
        "input-available",
        "approval-requested",
        "approval-responded",
        "output-available",
        "output-error",
        "output-denied",
    ];

    public static JsonObject ToJson(UIMessage message)
    {
        var json = new JsonObject
        {
            ["id"] = message.Id,
            ["role"] = message.Role switch
            {
                UIMessageRole.System => "system",
                UIMessageRole.User => "user",
                _ => "assistant",
            },
        };

        if (message.Metadata is { } metadata)
        {
            json["metadata"] = ToNode(metadata);
        }

        var parts = new JsonArray();
        foreach (var part in message.Parts)
        {
            parts.Add(PartToJson(part));
        }

        json["parts"] = parts;
        return json;
    }

    public static UIMessage FromJson(JsonObject json)
    {
        var role = (string?)json["role"] switch
        {
            "system" => UIMessageRole.System,
            "user" => UIMessageRole.User,
            _ => UIMessageRole.Assistant,
        };

        var parts = new List<UIMessagePart>();
        if (json["parts"] is JsonArray array)
        {
            foreach (var node in array)
            {
                if (node is JsonObject partJson && PartFromJson(partJson) is { } part)
                {
                    parts.Add(part);
                }
            }
        }

        var metadata = json["metadata"];
        return new UIMessage(role, parts)
        {
            Id = (string?)json["id"] ?? string.Empty,
            Metadata = metadata == null ? null : ToElement(metadata),
        };
    }

    public static bool IsToolPart(JsonObject part)
    {
        var type = (string?)part["type"];
        return type != null && (type.StartsWith("tool-", StringComparison.Ordinal) || type == "dynamic-tool");
    }

    public static bool IsStaticToolPart(JsonObject part)
    {
        var type = (string?)part["type"];
        return type != null && type.StartsWith("tool-", StringComparison.Ordinal);
    }

    private static JsonObject PartToJson(UIMessagePart part)
    {
        JsonObject json;
        switch (part)
        {
            case TextUIPart text:
                json = new JsonObject { ["type"] = "text", ["text"] = text.Text };
                if (text.Citations != null)
                {
                    json["citations"] = CitationJson.ToJson(text.Citations);
                }

                Set(json, "state", text.State);
                SetMetadata(json, "providerMetadata", text.ProviderMetadata);
                break;
            case CustomContentUIPart custom:
                json = new JsonObject { ["type"] = "custom", ["kind"] = custom.Kind };
                SetMetadata(json, "providerMetadata", custom.ProviderMetadata);
                break;
            case ReasoningUIPart reasoning:
                json = new JsonObject { ["type"] = "reasoning", ["text"] = reasoning.Text };
                Set(json, "id", reasoning.Id);
                Set(json, "state", reasoning.State);
                SetMetadata(json, "providerMetadata", reasoning.ProviderMetadata);
                break;
            case SourceUrlUIPart sourceUrl:
                json = new JsonObject { ["type"] = "source-url", ["sourceId"] = sourceUrl.SourceId, ["url"] = sourceUrl.Url };
                Set(json, "title", sourceUrl.Title);
                SetMetadata(json, "providerMetadata", sourceUrl.ProviderMetadata);
                break;
            case SourceDocumentUIPart sourceDocument:
                json = new JsonObject
                {
                    ["type"] = "source-document",
                    ["sourceId"] = sourceDocument.SourceId,
                    ["mediaType"] = sourceDocument.MediaType,
                    ["title"] = sourceDocument.Title,
                };
                Set(json, "filename", sourceDocument.Filename);
                SetMetadata(json, "providerMetadata", sourceDocument.ProviderMetadata);
                break;
            case FileUIPart file:
                json = new JsonObject { ["type"] = "file", ["mediaType"] = file.MediaType, ["url"] = file.Url };
                Set(json, "filename", file.Filename);
                if (file.ProviderReference != null)
                {
                    var reference = new JsonObject();
                    foreach (var pair in file.ProviderReference)
                    {
                        reference[pair.Key] = pair.Value;
                    }

                    json["providerReference"] = reference;
                }

                SetMetadata(json, "providerMetadata", file.ProviderMetadata);
                break;
            case ReasoningFileUIPart reasoningFile:
                json = new JsonObject { ["type"] = "reasoning-file", ["mediaType"] = reasoningFile.MediaType, ["url"] = reasoningFile.Url };
                SetMetadata(json, "providerMetadata", reasoningFile.ProviderMetadata);
                break;
            case StepStartUIPart:
                json = new JsonObject { ["type"] = "step-start" };
                break;
            case DataUIPart data:
                json = new JsonObject { ["type"] = data.Type, ["data"] = ToNode(data.Data) };
                Set(json, "id", data.Id);
                break;
            case SteeringMessageUIPart steering:
                var steeringParts = new JsonArray();
                foreach (var steeringPart in steering.Parts)
                {
                    steeringParts.Add(PartToJson(steeringPart));
                }

                json = new JsonObject { ["type"] = "steering-message", ["id"] = steering.Id, ["parts"] = steeringParts };
                break;
            case ToolUIPart tool:
                json = new JsonObject { ["type"] = tool.Type, ["toolCallId"] = tool.ToolCallId, ["state"] = ToolStates[(int)tool.State] };
                if (tool.IsDynamic)
                {
                    json["toolName"] = tool.ToolName;
                    if (tool.Dynamic is { } dynamic)
                    {
                        json["dynamic"] = dynamic;
                    }
                }

                Set(json, "title", tool.Title);
                SetMetadata(json, "toolMetadata", tool.ToolMetadata);
                if (tool.ProviderExecuted is { } providerExecuted)
                {
                    json["providerExecuted"] = providerExecuted;
                }

                if (tool.Input is { } input)
                {
                    json["input"] = ToNode(input);
                }

                if (tool.RawInput is { } rawInput)
                {
                    json["rawInput"] = ToNode(rawInput);
                }

                if (tool.InputEncoding is { } inputEncoding)
                {
                    json["inputEncoding"] = inputEncoding == ToolInputEncoding.Text ? "text" : "json";
                }

                if (tool.Output is { } output)
                {
                    json["output"] = ToNode(output);
                }

                Set(json, "errorText", tool.ErrorText);
                if (tool.Preliminary is { } preliminary)
                {
                    json["preliminary"] = preliminary;
                }

                SetMetadata(json, "callProviderMetadata", tool.CallProviderMetadata);
                SetMetadata(json, "resultProviderMetadata", tool.ResultProviderMetadata);
                if (tool.Approval is { } approval)
                {
                    json["approval"] = ApprovalToJson(approval);
                }

                break;
            default:
                throw new NotSupportedException("Unsupported UI message part: " + part.GetType().Name);
        }

        return json;
    }

    private static UIMessagePart? PartFromJson(JsonObject json)
    {
        var type = (string?)json["type"] ?? string.Empty;
        switch (type)
        {
            case "text":
                return new TextUIPart((string?)json["text"] ?? string.Empty)
                {
                    Citations = json["citations"] is JsonArray citations ? CitationJson.FromJson(citations) : null,
                    State = (string?)json["state"],
                    ProviderMetadata = GetMetadata(json["providerMetadata"]),
                };
            case "custom":
                return new CustomContentUIPart((string?)json["kind"] ?? string.Empty) { ProviderMetadata = GetMetadata(json["providerMetadata"]) };
            case "reasoning":
                return new ReasoningUIPart((string?)json["text"] ?? string.Empty)
                {
                    Id = (string?)json["id"],
                    State = (string?)json["state"],
                    ProviderMetadata = GetMetadata(json["providerMetadata"]),
                };
            case "source-url":
                return new SourceUrlUIPart((string?)json["sourceId"] ?? string.Empty, (string?)json["url"] ?? string.Empty)
                {
                    Title = (string?)json["title"],
                    ProviderMetadata = GetMetadata(json["providerMetadata"]),
                };
            case "source-document":
                return new SourceDocumentUIPart((string?)json["sourceId"] ?? string.Empty, (string?)json["mediaType"] ?? string.Empty, (string?)json["title"] ?? string.Empty)
                {
                    Filename = (string?)json["filename"],
                    ProviderMetadata = GetMetadata(json["providerMetadata"]),
                };
            case "file":
                Dictionary<string, string>? reference = null;
                if (json["providerReference"] is JsonObject referenceJson)
                {
                    reference = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (var pair in referenceJson)
                    {
                        reference[pair.Key] = (string?)pair.Value ?? string.Empty;
                    }
                }

                return new FileUIPart((string?)json["mediaType"] ?? string.Empty, (string?)json["url"] ?? string.Empty)
                {
                    Filename = (string?)json["filename"],
                    ProviderReference = reference,
                    ProviderMetadata = GetMetadata(json["providerMetadata"]),
                };
            case "reasoning-file":
                return new ReasoningFileUIPart((string?)json["mediaType"] ?? string.Empty, (string?)json["url"] ?? string.Empty)
                {
                    ProviderMetadata = GetMetadata(json["providerMetadata"]),
                };
            case "step-start":
                return new StepStartUIPart();
            case "steering-message":
                var steeringParts = new List<UIMessagePart>();
                if (json["parts"] is JsonArray steeringArray)
                {
                    foreach (var node in steeringArray)
                    {
                        if (node is JsonObject steeringJson && PartFromJson(steeringJson) is { } steeringPart)
                        {
                            steeringParts.Add(steeringPart);
                        }
                    }
                }

                return new SteeringMessageUIPart((string?)json["id"] ?? string.Empty, steeringParts);
            case "dynamic-tool":
                return ToolFromJson(json, (string?)json["toolName"] ?? string.Empty, true);
        }

        if (type.StartsWith("data-", StringComparison.Ordinal))
        {
            return new DataUIPart(type.Substring(5), ToElement(json["data"])) { Id = (string?)json["id"] };
        }

        if (type.StartsWith("tool-", StringComparison.Ordinal))
        {
            return ToolFromJson(json, type.Substring(5), false);
        }

        // Parts that have no .NET type are not part of the typed message.
        return null;
    }

    private static ToolUIPart ToolFromJson(JsonObject json, string toolName, bool isDynamic)
    {
        var stateName = (string?)json["state"] ?? "input-streaming";
        var state = (ToolUIPartState)Math.Max(0, Array.IndexOf(ToolStates, stateName));
        return new ToolUIPart(toolName, (string?)json["toolCallId"] ?? string.Empty, state, isDynamic)
        {
            Title = (string?)json["title"],
            Dynamic = isDynamic && json["dynamic"] is JsonValue dyn && dyn.TryGetValue<bool>(out var dynamicValue) ? dynamicValue : null,
            ToolMetadata = GetMetadata(json["toolMetadata"]),
            InputEncoding = (string?)json["inputEncoding"] switch
            {
                "text" => ToolInputEncoding.Text,
                "json" => ToolInputEncoding.Json,
                _ => null,
            },
            ProviderExecuted = json["providerExecuted"] is JsonValue pe && pe.TryGetValue<bool>(out var executed) ? executed : null,
            Input = Element(json, "input"),
            RawInput = Element(json, "rawInput"),
            Output = Element(json, "output"),
            ErrorText = (string?)json["errorText"],
            Preliminary = json["preliminary"] is JsonValue pre && pre.TryGetValue<bool>(out var preliminary) ? preliminary : null,
            CallProviderMetadata = GetMetadata(json["callProviderMetadata"]),
            ResultProviderMetadata = GetMetadata(json["resultProviderMetadata"]),
            Approval = json["approval"] is JsonObject approval ? ApprovalFromJson(approval) : null,
        };
    }

    private static JsonObject ApprovalToJson(ToolUIApproval approval)
    {
        var json = new JsonObject { ["id"] = approval.Id };
        if (approval.Approved is { } approved)
        {
            json["approved"] = approved;
        }

        Set(json, "requestReason", approval.RequestReason);
        Set(json, "reason", approval.Reason);
        if (approval.IsAutomatic is { } isAutomatic)
        {
            json["isAutomatic"] = isAutomatic;
        }

        if (approval.Descriptor is { } descriptor)
        {
            json["descriptor"] = ToNode(descriptor);
        }

        Set(json, "signature", approval.Signature);
        if (approval.InputSchemaInput is { } inputSchemaInput)
        {
            json["inputSchemaInput"] = ToNode(inputSchemaInput);
        }

        return json;
    }

    private static ToolUIApproval ApprovalFromJson(JsonObject json)
    {
        return new ToolUIApproval((string?)json["id"] ?? string.Empty)
        {
            Approved = json["approved"] is JsonValue approved && approved.TryGetValue<bool>(out var a) ? a : null,
            RequestReason = (string?)json["requestReason"],
            Reason = (string?)json["reason"],
            IsAutomatic = json["isAutomatic"] is JsonValue automatic && automatic.TryGetValue<bool>(out var b) ? b : null,
            Descriptor = Element(json, "descriptor"),
            Signature = (string?)json["signature"],
            InputSchemaInput = Element(json, "inputSchemaInput"),
        };
    }

    private static JsonElement? Element(JsonObject json, string key)
        => json.ContainsKey(key) ? ToElement(json[key]) : null;

    private static JsonNode? ToNode(JsonElement element)
        => JsonNode.Parse(element.GetRawText());

    private static JsonElement ToElement(JsonNode? node)
        => node == null ? JsonSerializer.SerializeToElement<object?>(null) : JsonSerializer.SerializeToElement(node);

    private static void Set(JsonObject json, string key, string? value)
    {
        if (value != null)
        {
            json[key] = value;
        }
    }

    private static void SetMetadata(JsonObject json, string key, IReadOnlyDictionary<string, JsonElement>? metadata)
    {
        if (metadata == null)
        {
            return;
        }

        var node = new JsonObject();
        foreach (var pair in metadata)
        {
            node[pair.Key] = ToNode(pair.Value);
        }

        json[key] = node;
    }

    private static IReadOnlyDictionary<string, JsonElement>? GetMetadata(JsonNode? node)
    {
        if (node is not JsonObject metadata)
        {
            return null;
        }

        var result = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var pair in metadata)
        {
            result[pair.Key] = ToElement(pair.Value);
        }

        return result;
    }
}
