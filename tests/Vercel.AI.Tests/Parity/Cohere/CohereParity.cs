// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Cohere;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Shared HTTP capture and JSON helpers for Cohere parity tests.</summary>
internal static class CohereParity
{
    public const string TextResponse = """
        {"id":"e7592632-1e3d-424f-b129-bd5f9f980f7b","message":{"role":"assistant","content":[{"type":"text","text":"The capital of France is Paris."}]},"finish_reason":"COMPLETE","usage":{"billed_units":{"input_tokens":12,"output_tokens":7},"tokens":{"input_tokens":507,"output_tokens":10},"cached_tokens":448}}
        """;

    /// <summary>Captures the next request and returns <paramref name="responseBody"/>.</summary>
    public static CaptureHandler Handler(string responseBody, string? events = null)
    {
        var handler = new CaptureHandler
        {
            ResponseBody = responseBody,
        };
        if (events != null)
        {
            handler.ServerSentEvents = events;
        }

        return handler;
    }

    /// <summary>Provider with key <c>test-api-key</c>.</summary>
    public static CohereProvider Provider(CaptureHandler handler, Action<CohereOptions>? configure = null)
    {
        var options = new CohereOptions
        {
            ApiKey = "test-api-key",
        };
        configure?.Invoke(options);
        return CohereProvider.Create(options, handler);
    }

    /// <summary>System instructions plus a user <c>Hello</c>.</summary>
    public static LanguageModelCallOptions Prompt()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("you are a friendly bot!"),
                new UserModelMessage("Hello"),
            },
        };
    }

    /// <summary>Parses JSON into an independent element.</summary>
    public static JsonElement Element(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    /// <summary>Compares JSON without regard to property order.</summary>
    public static void JsonEqual(string actual, string expected)
    {
        var left = JsonNode.Parse(string.IsNullOrWhiteSpace(actual) ? "null" : actual);
        var right = JsonNode.Parse(expected);
        Assert.True(
            JsonNode.DeepEquals(left, right),
            "expected " + expected + " but was " + actual);
    }

    /// <summary>Reads every stream part.</summary>
    public static async Task<List<LanguageModelStreamPart>> Collect(IAsyncEnumerable<LanguageModelStreamPart> parts)
    {
        var list = new List<LanguageModelStreamPart>();
        await foreach (var part in parts.ConfigureAwait(false))
        {
            list.Add(part);
        }

        return list;
    }

    /// <summary>Joins JSON objects into SSE <c>data:</c> events.</summary>
    public static string Sse(params string[] jsonLines)
    {
        var builder = new System.Text.StringBuilder();
        foreach (var line in jsonLines)
        {
            builder.Append("data: ");
            builder.Append(line);
            builder.Append("\n\n");
        }

        return builder.ToString();
    }
}
