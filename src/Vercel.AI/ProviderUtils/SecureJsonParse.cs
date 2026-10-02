// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

// Licensed under BSD-3-Clause (this file only).
// Code adapted from https://github.com/fastify/secure-json-parse/blob/783fcb1b5434709466759847cec974381939673a/index.js
// Copyright (c) Vercel, Inc. (https://vercel.com)
// Copyright (c) 2019 The Fastify Team
// Copyright (c) 2019, Sideway Inc, and project contributors

using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Vercel.AI.ProviderUtils;

/// <summary>Thrown when JSON contains a forbidden prototype property. Maps to <c>SyntaxError</c>.</summary>
public sealed class JsonSyntaxException : Exception
{
    /// <summary>Creates the error.</summary>
    public JsonSyntaxException(string message)
        : base(message)
    {
    }
}

/// <summary>Parses JSON and rejects prototype pollution. Maps to <c>secureJsonParse</c>.</summary>
public static class SecureJson
{
    private const string SuspectProtoPattern =
        "\"(?:_|\\\\u005[Ff])(?:_|\\\\u005[Ff])(?:p|\\\\u0070)(?:r|\\\\u0072)(?:o|\\\\u006[Ff])(?:t|\\\\u0074)(?:o|\\\\u006[Ff])(?:_|\\\\u005[Ff])(?:_|\\\\u005[Ff])\"\\s*:";

    private const string SuspectConstructorPattern =
        "\"(?:c|\\\\u0063)(?:o|\\\\u006[Ff])(?:n|\\\\u006[Ee])(?:s|\\\\u0073)(?:t|\\\\u0074)(?:r|\\\\u0072)(?:u|\\\\u0075)(?:c|\\\\u0063)(?:t|\\\\u0074)(?:o|\\\\u006[Ff])(?:r|\\\\u0072)\"\\s*:";

    private static readonly Regex SuspectProto = new Regex(SuspectProtoPattern, RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
    private static readonly Regex SuspectConstructor = new Regex(SuspectConstructorPattern, RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    /// <summary>Parses <paramref name="text"/> and throws <see cref="JsonSyntaxException"/> for prototype keys.</summary>
    public static JsonNode? SecureJsonParse(string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        var value = JsonNode.Parse(text);
        if (value is null || value is JsonValue)
        {
            return value;
        }

        if (!SuspectProto.IsMatch(text) && !SuspectConstructor.IsMatch(text))
        {
            return value;
        }

        return Filter(value);
    }

    private static JsonNode Filter(JsonNode root)
    {
        var next = new List<JsonNode> { root };
        while (next.Count > 0)
        {
            var nodes = next;
            next = new List<JsonNode>();
            foreach (var node in nodes)
            {
                if (node is JsonObject obj)
                {
                    if (obj.ContainsKey("__proto__"))
                    {
                        throw new JsonSyntaxException("Object contains forbidden prototype property");
                    }

                    if (obj.TryGetPropertyValue("constructor", out var constructor)
                        && constructor is JsonObject constructorObject
                        && constructorObject.ContainsKey("prototype"))
                    {
                        throw new JsonSyntaxException("Object contains forbidden prototype property");
                    }

                    foreach (var pair in obj)
                    {
                        if (pair.Value is JsonObject || pair.Value is JsonArray)
                        {
                            next.Add(pair.Value);
                        }
                    }
                }
                else if (node is JsonArray array)
                {
                    foreach (var item in array)
                    {
                        if (item is JsonObject || item is JsonArray)
                        {
                            next.Add(item);
                        }
                    }
                }
            }
        }

        return root;
    }
}
