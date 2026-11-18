// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Ui;

/// <summary>An issue of <see cref="UIMessagesSchema"/>. Maps to a zod issue: its <c>code</c>, <c>path</c>, and <c>message</c>.</summary>
internal sealed class UIMessagesSchemaIssue(string code, IReadOnlyList<object> path, string message)
{
    internal string Code { get; } = code;

    /// <summary>The path to the invalid value. Each segment is an <see cref="int"/> index or a <see cref="string"/> key.</summary>
    internal IReadOnlyList<object> Path { get; } = path;

    internal string Message { get; } = message;
}

/// <summary>The cause of the <see cref="ProviderUtils.TypeValidationException"/> that <see cref="UIMessagesSchema"/> reports.</summary>
internal sealed class UIMessagesSchemaException : Exception
{
    internal UIMessagesSchemaException(IReadOnlyList<UIMessagesSchemaIssue> issues)
        : base(Format(issues))
    {
        Issues = issues;
    }

    internal IReadOnlyList<UIMessagesSchemaIssue> Issues { get; }

    private static string Format(IReadOnlyList<UIMessagesSchemaIssue> issues)
    {
        var array = new JsonArray();
        foreach (var issue in issues)
        {
            var path = new JsonArray();
            foreach (var segment in issue.Path)
            {
                path.Add(
                    segment is int index
                        ? JsonValue.Create(index)
                        : JsonValue.Create((string)segment)
                );
            }

            array.Add(
                new JsonObject
                {
                    ["code"] = issue.Code,
                    ["path"] = path,
                    ["message"] = issue.Message,
                }
            );
        }

        return array.ToJsonString(
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }
        );
    }
}

/// <summary>
/// Validates untyped UI messages structurally and reads them into typed messages. Maps to <c>uiMessagesSchema</c>.
/// UI messages are untrusted input, so each array stops at its first invalid element, each part is validated only against
/// the schema of its type, and a JSON value is checked in place and reported with a single issue.
/// </summary>
internal static class UIMessagesSchema
{
    private const string InvalidJsonValueMessage =
        "Invalid JSON value. Expected null, a string, a finite number, a boolean, an array, or an object.";

    private const string InvalidPartTypeMessage =
        "Invalid UI message part type. Expected a known part type "
        + "(for example \"text\" or \"dynamic-tool\") or a type starting with \"data-\" or \"tool-\".";

    private static readonly string[] StaticPartTypes =
    [
        "text",
        "reasoning",
        "custom",
        "source-url",
        "source-document",
        "file",
        "reasoning-file",
        "steering-message",
        "step-start",
    ];

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

    /// <summary>Validates the messages. Returns the issues, which are empty when the messages are valid.</summary>
    internal static IReadOnlyList<UIMessagesSchemaIssue> Validate(JsonElement messages)
    {
        var issues = new List<UIMessagesSchemaIssue>();
        var path = new List<object>();
        if (messages.ValueKind != JsonValueKind.Array)
        {
            Invalid(issues, path, "array", messages);
            return issues;
        }

        var index = 0;
        foreach (var message in messages.EnumerateArray())
        {
            path.Add(index);
            ValidateMessage(message, path, issues);
            path.RemoveAt(path.Count - 1);
            if (issues.Count > 0)
            {
                return issues;
            }

            index++;
        }

        if (index == 0)
        {
            issues.Add(
                new UIMessagesSchemaIssue("too_small", [], "Messages array must not be empty")
            );
        }

        return issues;
    }

    /// <summary>Reads validated messages into typed messages. Removes the <c>__proto__</c> keys of provider metadata.</summary>
    internal static IReadOnlyList<UIMessage> Read(JsonElement messages)
    {
        var result = new List<UIMessage>();
        foreach (var message in messages.EnumerateArray())
        {
            var node = (JsonObject)JsonNode.Parse(message.GetRawText(), null, DeepDocument)!;
            if (node["parts"] is JsonArray parts)
            {
                foreach (var part in parts)
                {
                    if (part is JsonObject partObject)
                    {
                        RemoveProtoKeys(partObject);
                    }
                }
            }

            result.Add(UIMessageJson.FromJson(node));
        }

        return result;
    }

    private static void RemoveProtoKeys(JsonObject part)
    {
        foreach (var key in MetadataKeys)
        {
            if (part[key] is { } metadata)
            {
                StripProto(metadata);
            }
        }

        if (part["citations"] is JsonArray citations)
        {
            foreach (var citation in citations)
            {
                if (citation?["source"]?["providerMetadata"] is { } metadata)
                {
                    StripProto(metadata);
                }
            }
        }

        if (part["parts"] is JsonArray steeringParts)
        {
            foreach (var steeringPart in steeringParts)
            {
                if (steeringPart is JsonObject steeringObject)
                {
                    RemoveProtoKeys(steeringObject);
                }
            }
        }
    }

    private static readonly JsonDocumentOptions DeepDocument = new() { MaxDepth = 4096 };

    private static readonly string[] MetadataKeys =
    [
        "providerMetadata",
        "callProviderMetadata",
        "resultProviderMetadata",
        "toolMetadata",
    ];

    private static void StripProto(JsonNode node)
    {
        switch (node)
        {
            case JsonObject obj:
                obj.Remove("__proto__");
                foreach (var pair in obj.ToList())
                {
                    if (pair.Value is { } child)
                    {
                        StripProto(child);
                    }
                }

                break;
            case JsonArray array:
                foreach (var child in array)
                {
                    if (child is { })
                    {
                        StripProto(child);
                    }
                }

                break;
        }
    }

    private static void ValidateMessage(
        JsonElement message,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (message.ValueKind != JsonValueKind.Object)
        {
            Invalid(issues, path, "object", message);
            return;
        }

        RequireString(message, "id", path, issues);
        if (
            !message.TryGetProperty("role", out var role)
            || role.ValueKind != JsonValueKind.String
            || role.GetString() is not ("system" or "user" or "assistant")
        )
        {
            Add(
                issues,
                path,
                "role",
                "invalid_value",
                "Invalid option: expected one of \"system\"|\"user\"|\"assistant\""
            );
        }

        // Validate each part only against the schemas for its type, and stop at the first invalid part.
        var partCount = -1;
        if (
            message.TryGetProperty("parts", out var parts)
            && parts.ValueKind == JsonValueKind.Array
        )
        {
            partCount = 0;
            path.Add("parts");
            var index = 0;
            foreach (var part in parts.EnumerateArray())
            {
                path.Add(index);
                ValidatePart(part, path, issues);
                path.RemoveAt(path.Count - 1);
                if (issues.Count > 0)
                {
                    break;
                }

                index++;
                partCount++;
            }

            path.RemoveAt(path.Count - 1);
        }
        else
        {
            Add(
                issues,
                path,
                "parts",
                "invalid_type",
                "Invalid input: expected array, received "
                    + (
                        message.TryGetProperty("parts", out var invalidParts)
                            ? Kind(invalidParts)
                            : "undefined"
                    )
            );
        }

        if (issues.Count == 0 && partCount == 0 && role.GetString() != "assistant")
        {
            Add(issues, path, "parts", "too_small", "Message must contain at least one part");
        }
    }

    private static void ValidatePart(
        JsonElement part,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (part.ValueKind != JsonValueKind.Object)
        {
            Invalid(issues, path, "object", part);
            return;
        }

        var type =
            part.TryGetProperty("type", out var typeElement)
            && typeElement.ValueKind == JsonValueKind.String
                ? typeElement.GetString()!
                : null;
        if (type != null && type.StartsWith("data-", StringComparison.Ordinal))
        {
            OptionalString(part, "id", path, issues);
            return;
        }

        if (
            type != null
            && (type.StartsWith("tool-", StringComparison.Ordinal) || type == "dynamic-tool")
        )
        {
            ValidateToolPart(part, type == "dynamic-tool", path, issues);
            return;
        }

        if (type == null || Array.IndexOf(StaticPartTypes, type) < 0)
        {
            Add(issues, path, "type", "invalid_union", InvalidPartTypeMessage);
            return;
        }

        ValidateStaticPart(part, type, path, issues);
    }

    private static void ValidateStaticPart(
        JsonElement part,
        string type,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        switch (type)
        {
            case "text":
                RequireString(part, "text", path, issues);
                ValidateCitations(part, path, issues);
                OptionalEnum(part, "state", ["streaming", "done"], path, issues);
                OptionalProviderMetadata(part, "providerMetadata", path, issues);
                break;
            case "reasoning":
                OptionalString(part, "id", path, issues);
                RequireString(part, "text", path, issues);
                OptionalEnum(part, "state", ["streaming", "done"], path, issues);
                OptionalProviderMetadata(part, "providerMetadata", path, issues);
                break;
            case "custom":
                RequireString(part, "kind", path, issues);
                OptionalProviderMetadata(part, "providerMetadata", path, issues);
                break;
            case "source-url":
                RequireString(part, "sourceId", path, issues);
                RequireString(part, "url", path, issues);
                OptionalString(part, "title", path, issues);
                OptionalProviderMetadata(part, "providerMetadata", path, issues);
                break;
            case "source-document":
                RequireString(part, "sourceId", path, issues);
                RequireString(part, "mediaType", path, issues);
                RequireString(part, "title", path, issues);
                OptionalString(part, "filename", path, issues);
                OptionalProviderMetadata(part, "providerMetadata", path, issues);
                break;
            case "file":
                ValidateFile(part, path, issues, allowFilename: true);
                break;
            case "reasoning-file":
                RequireString(part, "mediaType", path, issues);
                RequireString(part, "url", path, issues);
                OptionalProviderMetadata(part, "providerMetadata", path, issues);
                break;
            case "steering-message":
                RequireString(part, "id", path, issues);
                ValidateSteeringParts(part, path, issues);
                break;
        }
    }

    private static void ValidateFile(
        JsonElement part,
        List<object> path,
        List<UIMessagesSchemaIssue> issues,
        bool allowFilename
    )
    {
        RequireString(part, "mediaType", path, issues);
        if (allowFilename)
        {
            OptionalString(part, "filename", path, issues);
        }

        RequireString(part, "url", path, issues);
        if (part.TryGetProperty("providerReference", out var reference))
        {
            // A record of strings.
            if (reference.ValueKind != JsonValueKind.Object)
            {
                Add(
                    issues,
                    path,
                    "providerReference",
                    "invalid_type",
                    Expected("object", reference)
                );
            }
            else
            {
                foreach (var property in reference.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.String)
                    {
                        path.Add("providerReference");
                        Add(
                            issues,
                            path,
                            property.Name,
                            "invalid_type",
                            Expected("string", property.Value)
                        );
                        path.RemoveAt(path.Count - 1);
                        break;
                    }
                }
            }
        }

        OptionalProviderMetadata(part, "providerMetadata", path, issues);
    }

    private static void ValidateSteeringParts(
        JsonElement part,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (!part.TryGetProperty("parts", out var parts) || parts.ValueKind != JsonValueKind.Array)
        {
            Add(
                issues,
                path,
                "parts",
                "invalid_type",
                Expected("array", part.TryGetProperty("parts", out var invalid) ? invalid : default)
            );
            return;
        }

        path.Add("parts");
        var index = 0;
        foreach (var steeringPart in parts.EnumerateArray())
        {
            path.Add(index);
            if (steeringPart.ValueKind != JsonValueKind.Object)
            {
                Invalid(issues, path, "object", steeringPart);
            }
            else
            {
                var type =
                    steeringPart.TryGetProperty("type", out var typeElement)
                    && typeElement.ValueKind == JsonValueKind.String
                        ? typeElement.GetString()
                        : null;
                switch (type)
                {
                    case "text":
                        RequireString(steeringPart, "text", path, issues);
                        OptionalProviderMetadata(steeringPart, "providerMetadata", path, issues);
                        break;
                    case "file":
                        ValidateFile(steeringPart, path, issues, allowFilename: true);
                        break;
                    default:
                        Add(
                            issues,
                            path,
                            "type",
                            "invalid_union",
                            "Invalid discriminator value. Expected 'text' | 'file'"
                        );
                        break;
                }
            }

            path.RemoveAt(path.Count - 1);
            if (issues.Count > 0)
            {
                break;
            }

            index++;
        }

        path.RemoveAt(path.Count - 1);
    }

    private static void ValidateCitations(
        JsonElement part,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (!part.TryGetProperty("citations", out var citations))
        {
            return;
        }

        if (citations.ValueKind != JsonValueKind.Array)
        {
            Add(issues, path, "citations", "invalid_type", Expected("array", citations));
            return;
        }

        path.Add("citations");
        var index = 0;
        foreach (var citation in citations.EnumerateArray())
        {
            path.Add(index);
            ValidateCitation(citation, path, issues);
            path.RemoveAt(path.Count - 1);
            if (issues.Count > 0)
            {
                break;
            }

            index++;
        }

        path.RemoveAt(path.Count - 1);
    }

    private static void ValidateCitation(
        JsonElement citation,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (citation.ValueKind != JsonValueKind.Object)
        {
            Invalid(issues, path, "object", citation);
            return;
        }

        if (
            citation.TryGetProperty("source", out var source)
            && source.ValueKind == JsonValueKind.Object
        )
        {
            path.Add("source");
            var sourceType =
                source.TryGetProperty("sourceType", out var sourceTypeElement)
                && sourceTypeElement.ValueKind == JsonValueKind.String
                    ? sourceTypeElement.GetString()
                    : null;
            if (sourceType is "url" or "document")
            {
                RequireLiteral(source, "type", "source", path, issues);
                RequireString(source, "id", path, issues);
                if (sourceType == "url")
                {
                    RequireString(source, "url", path, issues);
                    OptionalString(source, "title", path, issues);
                }
                else
                {
                    RequireString(source, "mediaType", path, issues);
                    RequireString(source, "title", path, issues);
                    OptionalString(source, "filename", path, issues);
                }

                OptionalProviderMetadata(source, "providerMetadata", path, issues);
            }
            else
            {
                Add(
                    issues,
                    path,
                    "sourceType",
                    "invalid_union",
                    "Invalid discriminator value. Expected 'url' | 'document'"
                );
            }

            path.RemoveAt(path.Count - 1);
        }
        else
        {
            Add(
                issues,
                path,
                "source",
                "invalid_type",
                Expected(
                    "object",
                    citation.TryGetProperty("source", out var invalid) ? invalid : default
                )
            );
        }

        if (issues.Count > 0)
        {
            return;
        }

        OptionalNonNegativeInteger(citation, "startIndex", path, issues);
        OptionalNonNegativeInteger(citation, "endIndex", path, issues);
        OptionalString(citation, "citedText", path, issues);
    }

    private static void ValidateToolPart(
        JsonElement part,
        bool isDynamic,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        var state =
            part.TryGetProperty("state", out var stateElement)
            && stateElement.ValueKind == JsonValueKind.String
                ? stateElement.GetString()
                : null;
        if (state == null || Array.IndexOf(ToolStates, state) < 0)
        {
            Add(
                issues,
                path,
                "state",
                "invalid_union",
                "Invalid discriminator value. Expected "
                    + string.Join(" | ", ToolStates.Select(name => "'" + name + "'"))
            );
            return;
        }

        if (isDynamic)
        {
            if (
                part.TryGetProperty("dynamic", out var dynamicElement)
                && dynamicElement.ValueKind != JsonValueKind.False
            )
            {
                Add(issues, path, "dynamic", "invalid_value", "Invalid input: expected false");
            }

            RequireString(part, "toolName", path, issues);
        }

        RequireString(part, "toolCallId", path, issues);
        OptionalString(part, "title", path, issues);
        OptionalToolMetadata(part, path, issues);
        OptionalBoolean(part, "providerExecuted", path, issues);
        OptionalProviderMetadata(part, "callProviderMetadata", path, issues);

        switch (state)
        {
            case "input-streaming":
                OptionalString(part, "rawInput", path, issues);
                OptionalEnum(part, "inputEncoding", ["json", "text"], path, issues);
                Never(part, "output", path, issues);
                Never(part, "errorText", path, issues);
                Never(part, "approval", path, issues);
                break;
            case "input-available":
                Never(part, "output", path, issues);
                Never(part, "errorText", path, issues);
                Never(part, "approval", path, issues);
                break;
            case "approval-requested":
                Never(part, "output", path, issues);
                Never(part, "errorText", path, issues);
                ValidateApproval(part, required: true, ApprovalKind.Requested, path, issues);
                break;
            case "approval-responded":
                Never(part, "output", path, issues);
                Never(part, "errorText", path, issues);
                ValidateApproval(part, required: true, ApprovalKind.Responded, path, issues);
                break;
            case "output-available":
                Never(part, "errorText", path, issues);
                OptionalProviderMetadata(part, "resultProviderMetadata", path, issues);
                OptionalBoolean(part, "preliminary", path, issues);
                ValidateApproval(part, required: false, ApprovalKind.Granted, path, issues);
                break;
            case "output-error":
                Never(part, "output", path, issues);
                RequireString(part, "errorText", path, issues);
                OptionalProviderMetadata(part, "resultProviderMetadata", path, issues);
                ValidateApproval(part, required: false, ApprovalKind.Granted, path, issues);
                break;
            case "output-denied":
                Never(part, "output", path, issues);
                Never(part, "errorText", path, issues);
                ValidateApproval(part, required: true, ApprovalKind.Denied, path, issues);
                break;
        }
    }

    private enum ApprovalKind
    {
        Requested,
        Responded,
        Granted,
        Denied,
    }

    private static void ValidateApproval(
        JsonElement part,
        bool required,
        ApprovalKind kind,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (!part.TryGetProperty("approval", out var approval))
        {
            if (required)
            {
                Add(issues, path, "approval", "invalid_type", Expected("object", default));
            }

            return;
        }

        if (approval.ValueKind != JsonValueKind.Object)
        {
            Add(issues, path, "approval", "invalid_type", Expected("object", approval));
            return;
        }

        path.Add("approval");
        RequireString(approval, "id", path, issues);
        switch (kind)
        {
            case ApprovalKind.Requested:
                Never(approval, "approved", path, issues);
                Never(approval, "reason", path, issues);
                break;
            case ApprovalKind.Responded:
                RequireBoolean(approval, "approved", path, issues);
                OptionalString(approval, "reason", path, issues);
                break;
            default:
                var expected =
                    kind == ApprovalKind.Granted ? JsonValueKind.True : JsonValueKind.False;
                if (
                    !approval.TryGetProperty("approved", out var approved)
                    || approved.ValueKind != expected
                )
                {
                    Add(
                        issues,
                        path,
                        "approved",
                        "invalid_value",
                        "Invalid input: expected "
                            + (kind == ApprovalKind.Granted ? "true" : "false")
                    );
                }

                OptionalString(approval, "reason", path, issues);
                break;
        }

        OptionalString(approval, "requestReason", path, issues);
        OptionalBoolean(approval, "isAutomatic", path, issues);
        OptionalString(approval, "signature", path, issues);
        path.RemoveAt(path.Count - 1);
    }

    // The tool metadata is a record of JSON values.
    private static void OptionalToolMetadata(
        JsonElement obj,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (!obj.TryGetProperty("toolMetadata", out var metadata))
        {
            return;
        }

        path.Add("toolMetadata");
        if (metadata.ValueKind != JsonValueKind.Object)
        {
            Invalid(issues, path, "object", metadata);
        }
        else
        {
            ValidateJsonValue(metadata, path, issues);
        }

        path.RemoveAt(path.Count - 1);
    }

    // The provider metadata is a record of records of JSON values.
    private static void OptionalProviderMetadata(
        JsonElement obj,
        string key,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (!obj.TryGetProperty(key, out var metadata))
        {
            return;
        }

        path.Add(key);
        if (metadata.ValueKind != JsonValueKind.Object)
        {
            Invalid(issues, path, "object", metadata);
        }
        else if (ValidateJsonValue(metadata, path, issues))
        {
            foreach (var provider in metadata.EnumerateObject())
            {
                if (provider.Value.ValueKind != JsonValueKind.Object)
                {
                    path.Add(provider.Name);
                    Invalid(issues, path, "object", provider.Value);
                    path.RemoveAt(path.Count - 1);
                    break;
                }
            }
        }

        path.RemoveAt(path.Count - 1);
    }

    /// <summary>
    /// Checks that a value is a JSON value with a finite number at every number, iteratively so that a deeply nested value
    /// cannot exhaust the stack. Reports the first invalid value with a single issue. Maps to <c>normalizeJsonValue</c>.
    /// </summary>
    private static bool ValidateJsonValue(
        JsonElement value,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        var basePath = path.Count;
        var stack = new Stack<(JsonElement Value, int Depth, object? Segment)>();
        stack.Push((value, 0, null));
        var segments = new List<object?>();
        while (stack.Count > 0)
        {
            var (current, depth, segment) = stack.Pop();
            segments.RemoveRange(depth, segments.Count - depth);
            segments.Add(segment);
            if (current.ValueKind == JsonValueKind.Number)
            {
                if (
                    !current.TryGetDouble(out var number)
                    || double.IsNaN(number)
                    || double.IsInfinity(number)
                )
                {
                    var invalidPath = new List<object>(path.Take(basePath));
                    invalidPath.AddRange(segments.Skip(1).Select(s => s!));
                    issues.Add(
                        new UIMessagesSchemaIssue("custom", invalidPath, InvalidJsonValueMessage)
                    );
                    return false;
                }
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                var elements = current.EnumerateArray().ToList();
                for (var i = elements.Count - 1; i >= 0; i--)
                {
                    stack.Push((elements[i], depth + 1, i));
                }
            }
            else if (current.ValueKind == JsonValueKind.Object)
            {
                var properties = current.EnumerateObject().ToList();
                for (var i = properties.Count - 1; i >= 0; i--)
                {
                    stack.Push((properties[i].Value, depth + 1, properties[i].Name));
                }
            }
        }

        return true;
    }

    private static void RequireLiteral(
        JsonElement obj,
        string key,
        string expected,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (
            !obj.TryGetProperty(key, out var value)
            || value.ValueKind != JsonValueKind.String
            || value.GetString() != expected
        )
        {
            Add(issues, path, key, "invalid_value", "Invalid input: expected \"" + expected + "\"");
        }
    }

    private static void RequireString(
        JsonElement obj,
        string key,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (!obj.TryGetProperty(key, out var value) || value.ValueKind != JsonValueKind.String)
        {
            Add(
                issues,
                path,
                key,
                "invalid_type",
                Expected("string", obj.TryGetProperty(key, out var invalid) ? invalid : default)
            );
        }
    }

    private static void RequireBoolean(
        JsonElement obj,
        string key,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (
            !obj.TryGetProperty(key, out var value)
            || value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)
        )
        {
            Add(
                issues,
                path,
                key,
                "invalid_type",
                Expected("boolean", obj.TryGetProperty(key, out var invalid) ? invalid : default)
            );
        }
    }

    private static void OptionalString(
        JsonElement obj,
        string key,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (obj.TryGetProperty(key, out var value) && value.ValueKind != JsonValueKind.String)
        {
            Add(issues, path, key, "invalid_type", Expected("string", value));
        }
    }

    private static void OptionalBoolean(
        JsonElement obj,
        string key,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (
            obj.TryGetProperty(key, out var value)
            && value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)
        )
        {
            Add(issues, path, key, "invalid_type", Expected("boolean", value));
        }
    }

    private static void OptionalEnum(
        JsonElement obj,
        string key,
        string[] options,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (
            obj.TryGetProperty(key, out var value)
            && (
                value.ValueKind != JsonValueKind.String
                || Array.IndexOf(options, value.GetString()) < 0
            )
        )
        {
            Add(
                issues,
                path,
                key,
                "invalid_value",
                "Invalid option: expected one of "
                    + string.Join("|", options.Select(o => "\"" + o + "\""))
            );
        }
    }

    private static void OptionalNonNegativeInteger(
        JsonElement obj,
        string key,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (!obj.TryGetProperty(key, out var value))
        {
            return;
        }

        if (value.ValueKind != JsonValueKind.Number)
        {
            Add(issues, path, key, "invalid_type", Expected("number", value));
        }
        else if (
            !value.TryGetDouble(out var number)
            || double.IsInfinity(number)
            || Math.Floor(number) != number
            || number < 0
        )
        {
            Add(
                issues,
                path,
                key,
                "invalid_value",
                "Invalid input: expected a non-negative integer"
            );
        }
    }

    private static void Never(
        JsonElement obj,
        string key,
        List<object> path,
        List<UIMessagesSchemaIssue> issues
    )
    {
        if (obj.TryGetProperty(key, out var value))
        {
            Add(issues, path, key, "invalid_type", Expected("never", value));
        }
    }

    private static void Add(
        List<UIMessagesSchemaIssue> issues,
        List<object> path,
        string key,
        string code,
        string message
    )
    {
        var issuePath = new List<object>(path) { key };
        issues.Add(new UIMessagesSchemaIssue(code, issuePath, message));
    }

    private static void Invalid(
        List<UIMessagesSchemaIssue> issues,
        List<object> path,
        string expected,
        JsonElement value
    )
    {
        issues.Add(new UIMessagesSchemaIssue("invalid_type", [.. path], Expected(expected, value)));
    }

    private static string Expected(string expected, JsonElement value) =>
        "Invalid input: expected " + expected + ", received " + Kind(value);

    private static string Kind(JsonElement value) =>
        value.ValueKind switch
        {
            JsonValueKind.Undefined => "undefined",
            JsonValueKind.Null => "null",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Number => "number",
            JsonValueKind.String => "string",
            JsonValueKind.Array => "array",
            _ => "object",
        };
}
