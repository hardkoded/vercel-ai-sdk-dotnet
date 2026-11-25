// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.OpenTelemetry;

/// <summary>Maps to <c>redact-tool-definition.ts</c>.</summary>
internal static class ToolDefinitionRedaction
{
    private const string Redacted = "[redacted]";

    /// <summary>
    /// Keeps exporter protection for older providers and plain tool definitions whose arguments carry no provider-owned telemetry
    /// redaction. It only makes a telemetry copy and never changes <paramref name="tool"/>. Maps to <c>redactToolDefinition</c>.
    /// </summary>
    public static JsonObject RedactToolDefinition(JsonObject tool)
    {
        if (tool["type"]?.GetValue<string>() != "provider" || tool["args"] is not JsonObject args)
        {
            return tool;
        }

        var id =
            tool["id"] is JsonValue idValue && idValue.TryGetValue<string>(out var text)
                ? text
                : null;

        if (id == "openai.mcp")
        {
            var copy = Copy(args);
            if (args["authorization"] is not null)
            {
                copy["authorization"] = Redacted;
            }

            if (args["headers"] is JsonObject headers)
            {
                copy["headers"] = RedactValues(headers);
            }

            return WithArgs(tool, copy);
        }

        if (id == "google.mcp_server")
        {
            if (args["headers"] is not JsonObject headers)
            {
                return tool;
            }

            var copy = Copy(args);
            copy["headers"] = RedactValues(headers);
            return WithArgs(tool, copy);
        }

        var containerKey = id switch
        {
            "openai.code_interpreter" => "container",
            "openai.shell" => "environment",
            _ => null,
        };
        if (
            containerKey is null
            || args[containerKey] is not JsonObject container
            || container["networkPolicy"] is not JsonObject networkPolicy
            || networkPolicy["domainSecrets"] is not JsonArray secrets
        )
        {
            return tool;
        }

        var redactedSecrets = new JsonArray();
        foreach (var secret in secrets)
        {
            if (secret is JsonObject secretObject && secretObject.ContainsKey("value"))
            {
                var secretCopy = Copy(secretObject);
                secretCopy["value"] = Redacted;
                redactedSecrets.Add(secretCopy);
            }
            else
            {
                redactedSecrets.Add(secret?.DeepClone());
            }
        }

        var policyCopy = Copy(networkPolicy);
        policyCopy["domainSecrets"] = redactedSecrets;
        var containerCopy = Copy(container);
        containerCopy["networkPolicy"] = policyCopy;
        var argsCopy = Copy(args);
        argsCopy[containerKey] = containerCopy;
        return WithArgs(tool, argsCopy);
    }

    private static JsonObject Copy(JsonObject source) => (JsonObject)source.DeepClone();

    private static JsonObject RedactValues(JsonObject headers)
    {
        var result = new JsonObject();
        foreach (var pair in headers)
        {
            result[pair.Key] = Redacted;
        }

        return result;
    }

    private static JsonObject WithArgs(JsonObject tool, JsonObject args)
    {
        var copy = Copy(tool);
        copy["args"] = args;
        return copy;
    }
}
