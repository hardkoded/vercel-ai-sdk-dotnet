// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenTelemetry;

/// <summary>
/// Maps to <c>redact-tool-definition.ts</c>. It keeps exporter protection for older providers and plain tool definitions whose
/// arguments carry no provider-owned telemetry redaction. It only creates telemetry copies and never modifies the provider's arguments.
/// </summary>
internal static class RedactToolDefinition
{
    private const string Redacted = "[redacted]";

    /// <summary>Maps to <c>redactToolDefinition</c>. Returns the telemetry view of a tool definition.</summary>
    public static Dictionary<string, object?> Redact(LanguageModelTool tool)
    {
        if (!tool.IsProviderDefined)
        {
            var function = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["type"] = "function",
                ["name"] = tool.Name,
            };
            if (tool.Description is not null)
            {
                function["description"] = tool.Description;
            }

            if (tool.InputSchema.ValueKind != JsonValueKind.Undefined)
            {
                function["inputSchema"] = tool.InputSchema;
            }

            if (tool.Strict is { } strict)
            {
                function["strict"] = strict;
            }

            if (tool.ProviderOptions is { } providerOptions)
            {
                function["providerOptions"] = providerOptions;
            }

            return function;
        }

        var definition = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["type"] = "provider",
            ["id"] = tool.ProviderToolId,
            ["name"] = tool.Name,
        };

        // The provider-owned view wins; a redactor that throws leaves no arguments, so nothing leaks.
        if (tool.RedactTelemetryArgs() is { } args && args.ValueKind == JsonValueKind.Object)
        {
            definition["args"] = RedactArgs(tool.ProviderToolId!, args);
        }

        return definition;
    }

    private static JsonNode? RedactArgs(string id, JsonElement args)
    {
        var node = JsonNode.Parse(args.GetRawText());
        if (node is not JsonObject obj)
        {
            return node;
        }

        switch (id)
        {
            case "openai.mcp":
                if (obj["authorization"] is not null)
                {
                    obj["authorization"] = Redacted;
                }

                RedactHeaders(obj);
                break;
            case "google.mcp_server":
                RedactHeaders(obj);
                break;
            case "openai.code_interpreter":
                RedactDomainSecrets(obj, "container");
                break;
            case "openai.shell":
                RedactDomainSecrets(obj, "environment");
                break;
        }

        return obj;
    }

    private static void RedactHeaders(JsonObject args)
    {
        if (args["headers"] is not JsonObject headers)
        {
            return;
        }

        foreach (var name in headers.Select(pair => pair.Key).ToList())
        {
            headers[name] = Redacted;
        }
    }

    private static void RedactDomainSecrets(JsonObject args, string containerKey)
    {
        if (
            args[containerKey] is JsonObject container
            && container["networkPolicy"] is JsonObject networkPolicy
            && networkPolicy["domainSecrets"] is JsonArray secrets
        )
        {
            foreach (var secret in secrets)
            {
                if (secret is JsonObject entry && entry.ContainsKey("value"))
                {
                    entry["value"] = Redacted;
                }
            }
        }
    }
}
