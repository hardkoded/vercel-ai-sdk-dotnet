// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

/// <summary>Shared JSON and HTTP helpers for Anthropic parity tests.</summary>
internal static class AnthropicParity
{
    /// <summary>Converts a V4 prompt JSON array.</summary>
    public static AnthropicPromptConversion ConvertPrompt(
        string prompt,
        bool sendReasoning = true,
        List<AnthropicWarning>? warnings = null,
        AnthropicCacheControlValidator? cacheControl = null,
        IReadOnlyDictionary<string, string>? toolsetNames = null)
    {
        using var document = JsonDocument.Parse(prompt);
        return AnthropicMessages.Convert(document.RootElement, new AnthropicConvertOptions
        {
            SendReasoning = sendReasoning,
            Warnings = warnings ?? new List<AnthropicWarning>(),
            CacheControl = cacheControl,
            ToolsetNames = toolsetNames,
        });
    }

    /// <summary>Prepares a tools JSON array.</summary>
    public static AnthropicPreparedTools PrepareTools(string? tools, AnthropicToolPrepareOptions? options = null)
    {
        JsonElement? element = null;
        if (tools != null)
        {
            using var document = JsonDocument.Parse(tools);
            element = document.RootElement.Clone();
        }

        return AnthropicTools.Prepare(element, options);
    }

    /// <summary>Compares JSON, ignoring property order and omitted nulls.</summary>
    public static void JsonEqual(JsonNode? actual, string expected)
    {
        var wanted = JsonNode.Parse(expected);
        Assert.True(
            JsonNode.DeepEquals(wanted, actual),
            "Expected " + expected + " but was " + (actual == null ? "null" : actual.ToJsonString()));
    }

    /// <summary>Compares a prompt object.</summary>
    public static void PromptEqual(AnthropicPromptConversion result, string expected)
    {
        JsonEqual(result.Prompt, expected);
    }

    /// <summary>Compares the messages array.</summary>
    public static void MessagesEqual(AnthropicPromptConversion result, string expected)
    {
        JsonEqual(result.Prompt["messages"], expected);
    }

    /// <summary>Parses a JSON object.</summary>
    public static JsonElement Element(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    /// <summary>Builds anthropic provider options.</summary>
    public static IReadOnlyDictionary<string, JsonElement> AnthropicOptions(string json)
    {
        return new Dictionary<string, JsonElement>
        {
            ["anthropic"] = Element(json),
        };
    }
}

/// <summary>Captures the Messages POST and returns a scripted response.</summary>
internal sealed class AnthropicCapturingHandler : HttpMessageHandler
{
    /// <summary>Response status.</summary>
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    /// <summary>Response JSON.</summary>
    public string ResponseJson { get; set; } = "{}";

    /// <summary>Captured request URI.</summary>
    public Uri? RequestUri { get; private set; }

    /// <summary>Captured request body.</summary>
    public string? Body { get; private set; }

    /// <summary>Captured request headers, including content headers.</summary>
    public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestUri = request.RequestUri;
        foreach (var header in request.Headers)
        {
            Headers[header.Key] = string.Join(",", header.Value);
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                Headers[header.Key] = string.Join(",", header.Value);
            }

            Body = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        var response = new HttpResponseMessage(StatusCode);
        response.Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json");
        return response;
    }
}

/// <summary>Builds an Anthropic model against a capturing handler.</summary>
internal static class AnthropicGenerateHarness
{
    /// <summary>Creates a model. Retries are disabled so error statuses surface immediately.</summary>
    public static (AnthropicLanguageModel Model, AnthropicCapturingHandler Handler) Create(string modelId, AnthropicOptions? options = null)
    {
        var handler = new AnthropicCapturingHandler();
        var client = new HttpClient(handler);
        var configured = options ?? new AnthropicOptions();
        if (string.IsNullOrEmpty(configured.ApiKey))
        {
            configured.ApiKey = "test-api-key";
        }

        var provider = new AnthropicProvider(client, configured, new RetryPolicy { MaxRetries = 0 });
        return ((AnthropicLanguageModel)provider.LanguageModel(modelId), handler);
    }

    /// <summary>A user message whose text is Hello.</summary>
    public static LanguageModelCallOptions Hello(Action<LanguageModelCallOptions>? configure = null)
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
        };
        if (configure != null)
        {
            configure(options);
        }

        return options;
    }
}
