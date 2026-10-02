// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

internal static class OpenAIParity
{
    public const string ChatUrl = "https://api.openai.com/v1/chat/completions";

    public const string HelloBody = """{"model":"gpt-3.5-turbo","messages":[{"role":"user","content":"Hello"}]}""";

    public const string ValueSchema = """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}""";

    public static JsonElement Element(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static IReadOnlyDictionary<string, JsonElement> OpenAIOptions(string json)
    {
        return new Dictionary<string, JsonElement> { ["openai"] = Element(json) };
    }

    public static LanguageModelCallOptions Hello(string? reasoning = null)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            Reasoning = reasoning,
        };
    }

    public static LanguageModelTool ValueTool(string name = "test-tool", string? description = null, bool? strict = null)
    {
        return new LanguageModelTool(name, description, Element(ValueSchema), strict);
    }

    public static void AssertJson(string actual, string expected)
    {
        var left = JsonNode.Parse(actual);
        var right = JsonNode.Parse(expected);
        Assert.True(JsonNode.DeepEquals(left, right), "Expected " + expected + " but was " + actual);
    }

    public static void AssertBody(RecordingHandler handler, string expected)
    {
        Assert.Equal(ChatUrl, handler.Uri);
        AssertJson(handler.Body, expected);
    }

    public static void AssertWarnings(IReadOnlyList<CallWarning> warnings, params (string Type, string? Feature, string? Details, string? Message)[] expected)
    {
        Assert.Equal(expected.Length, warnings.Count);
        for (var index = 0; index < expected.Length; index++)
        {
            var warning = Assert.IsType<OpenAICallWarning>(warnings[index]);
            Assert.Equal(expected[index].Type, warning.Type);
            Assert.Equal(expected[index].Feature, warning.Feature);
            Assert.Equal(expected[index].Details, warning.Details);
            Assert.Equal(expected[index].Message, warning.WarningMessage);
        }
    }

    public static void AssertUsage(
        LanguageModelUsage usage,
        int? input,
        int? output,
        int? reasoning,
        int? cacheRead,
        int? cacheWrite,
        int? total = null)
    {
        Assert.Equal(input, usage.InputTokens);
        Assert.Equal(output, usage.OutputTokens);
        Assert.Equal(reasoning, usage.ReasoningTokens);
        Assert.Equal(cacheRead, usage.CacheReadTokens);
        Assert.Equal(cacheWrite, usage.CacheWriteTokens);
        if (total != null)
        {
            Assert.Equal(total, usage.TotalTokens);
        }

        if (input != null && cacheRead != null)
        {
            var noCache = input.Value - cacheRead.Value - (cacheWrite ?? 0);
            if (cacheWrite != null)
            {
                Assert.Equal(noCache, usage.NoCacheInputTokens);
            }
            else
            {
                Assert.Null(usage.NoCacheInputTokens);
                Assert.Equal(noCache, input.Value - cacheRead.Value);
            }
        }

        if (output != null && reasoning != null)
        {
            Assert.Equal(Math.Max(0, output.Value - reasoning.Value), usage.TextTokens);
        }
    }

    public static string Sse(params string[] events)
    {
        var builder = new StringBuilder();
        foreach (var item in events)
        {
            builder.Append("data: ").Append(item).Append("\n\n");
        }

        builder.Append("data: [DONE]\n\n");
        return builder.ToString();
    }

    public static OpenAIChatLanguageModel Model(RecordingHandler handler, string modelId = "gpt-3.5-turbo", OpenAIOptions? options = null)
    {
        options ??= new OpenAIOptions { ApiKey = "test-api-key" };
        if (string.IsNullOrEmpty(options.ApiKey))
        {
            options.ApiKey = "test-api-key";
        }

        return (OpenAIChatLanguageModel)OpenAIProvider.Create(options, handler).LanguageModel(modelId);
    }
}

internal sealed class RecordingHandler : HttpMessageHandler
{
    public string Uri { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string ResponseBody { get; set; } = """{"id":"chatcmpl","object":"chat.completion","created":1,"model":"m","choices":[{"index":0,"message":{"role":"assistant","content":"ok"},"finish_reason":"stop"}],"usage":{"prompt_tokens":1,"completion_tokens":1,"total_tokens":2}}""";

    public string MediaType { get; set; } = "application/json";

    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    public Dictionary<string, string> ResponseHeaders { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Uri = request.RequestUri?.AbsoluteUri ?? string.Empty;
        Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        Headers.Clear();
        foreach (var header in request.Headers)
        {
            Headers[header.Key] = string.Join(",", header.Value);
        }

        if (request.Headers.Authorization != null)
        {
            Headers["Authorization"] = request.Headers.Authorization.ToString();
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                Headers[header.Key] = string.Join(",", header.Value);
            }
        }

        var response = new HttpResponseMessage(Status)
        {
            Content = new StringContent(ResponseBody, Encoding.UTF8, MediaType),
        };
        foreach (var header in ResponseHeaders)
        {
            if (!response.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return response;
    }
}
