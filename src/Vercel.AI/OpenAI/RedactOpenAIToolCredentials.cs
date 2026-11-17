// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

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
}
