// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>OpenAI owns both the SDK configuration paths and their wire equivalents. Maps to <c>redact-openai-tool-credentials.ts</c>.</summary>
internal static class RedactOpenAIToolCredentials
{
    private const string Redacted = "[redacted]";

    /// <summary>
    /// Returns a copy of <paramref name="args"/> without the credentials of an OpenAI tool: the MCP <c>authorization</c> and header values, and the
    /// domain secret values of a code interpreter container or shell environment. Maps to <c>redactOpenAIToolArgs</c>.
    /// </summary>
    public static JsonElement RedactOpenAIToolArgs(
        string? type,
        JsonElement args,
        bool wireFormat = false
    )
    {
        if (
            args.ValueKind != JsonValueKind.Object
            || JsonNode.Parse(args.GetRawText()) is not JsonObject copy
        )
        {
            return args;
        }

        if (type == "mcp")
        {
            if (copy["authorization"] != null)
            {
                copy["authorization"] = Redacted;
            }

            if (copy["headers"] is JsonObject headers)
            {
                foreach (var name in headers.Select(header => header.Key).ToList())
                {
                    headers[name] = Redacted;
                }
            }

            return JsonSerializer.SerializeToElement(copy);
        }

        var key = type switch
        {
            "code_interpreter" => "container",
            "shell" => "environment",
            _ => null,
        };
        if (key == null || copy[key] is not JsonObject container)
        {
            return args;
        }

        var policyKey = wireFormat ? "network_policy" : "networkPolicy";
        var secretsKey = wireFormat ? "domain_secrets" : "domainSecrets";
        if (
            container[policyKey] is not JsonObject policy
            || policy[secretsKey] is not JsonArray secrets
        )
        {
            return args;
        }

        foreach (var secret in secrets)
        {
            if (secret is JsonObject entry && entry.ContainsKey("value"))
            {
                entry["value"] = Redacted;
            }
        }

        return JsonSerializer.SerializeToElement(copy);
    }

    /// <summary>
    /// Preserves a raw request or response body while providing a safe telemetry view. A body without a <c>tools</c> array needs no
    /// redaction. Maps to <c>withOpenAIResponseRedaction</c>.
    /// </summary>
    public static TelemetryRedactedValue WithOpenAIResponseRedaction(JsonElement value)
    {
        if (
            value.ValueKind != JsonValueKind.Object
            || !value.TryGetProperty("tools", out var tools)
            || tools.ValueKind != JsonValueKind.Array
        )
        {
            return new TelemetryRedactedValue(value, null);
        }

        return TelemetryRedaction.WithTelemetryRedaction(value, RedactResponseBody);
    }

    /// <summary>The telemetry view of an OpenAI Responses body: every tool with its credentials redacted. Maps to the redactor of <c>withOpenAIResponseRedaction</c>.</summary>
    internal static JsonElement RedactResponseBody(JsonElement body)
    {
        if (
            body.ValueKind != JsonValueKind.Object
            || JsonNode.Parse(body.GetRawText()) is not JsonObject copy
            || copy["tools"] is not JsonArray tools
        )
        {
            return body;
        }

        for (var index = 0; index < tools.Count; index++)
        {
            if (tools[index] is not JsonObject tool)
            {
                continue;
            }

            var type = tool["type"]?.GetValueKind() == JsonValueKind.String
                ? tool["type"]!.GetValue<string>()
                : null;
            using var document = JsonDocument.Parse(tool.ToJsonString());
            tools[index] = JsonNode.Parse(
                RedactOpenAIToolArgs(type, document.RootElement, true).GetRawText()
            );
        }

        return JsonSerializer.SerializeToElement(copy);
    }
}
