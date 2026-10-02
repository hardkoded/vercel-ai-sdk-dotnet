// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Shared HTTP capture and JSON helpers for Azure parity tests.</summary>
internal static class AzureParity
{
    public const string Ok = "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"ok\"},\"finish_reason\":\"stop\"}]}";

    public const string ResponsesOk = "{\"id\":\"resp_ok\",\"output\":[],\"usage\":{\"input_tokens\":1,\"output_tokens\":1,\"total_tokens\":2}}";

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

    /// <summary>Provider aimed at <c>test-resource</c> with key <c>test-api-key</c>.</summary>
    public static AzureOpenAIProvider Provider(CaptureHandler handler, Action<AzureOpenAIOptions>? configure = null)
    {
        var options = new AzureOpenAIOptions
        {
            ResourceName = "test-resource",
            ApiKey = "test-api-key",
        };
        configure?.Invoke(options);
        return AzureOpenAIProvider.Create(options, handler);
    }

    /// <summary>User message <c>Hello</c>.</summary>
    public static LanguageModelCallOptions Hello()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
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
