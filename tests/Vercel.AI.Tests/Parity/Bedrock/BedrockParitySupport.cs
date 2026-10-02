// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Shared builders for Amazon Bedrock parity tests.</summary>
internal static class BedrockParity
{
    public static JsonElement Element(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static JsonNode Node(string json)
    {
        return JsonNode.Parse(json)!;
    }

    public static Dictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        var options = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            options[property.Name] = property.Value.Clone();
        }

        return options;
    }

    public static JsonElement ProviderOptions(string innerJson, string key = "bedrock")
    {
        return Element("{\"" + key + "\":" + innerJson + "}");
    }

    public static LanguageModelCallOptions Call(string userText = "Hello", string? systemText = "System Prompt")
    {
        var prompt = new List<ModelMessage>();
        if (systemText != null)
        {
            prompt.Add(new SystemModelMessage(systemText));
        }

        prompt.Add(new UserModelMessage(userText));
        return new LanguageModelCallOptions
        {
            Prompt = prompt,
        };
    }

    public static AmazonBedrockPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, string? modelFamily = null)
    {
        return AmazonBedrockConverseRequest.Prepare(modelId, options, modelFamily);
    }

    public static AmazonBedrockLanguageModel Model(HttpMessageHandler handler, string modelId, AmazonBedrockOptions? options = null)
    {
        options = options ?? new AmazonBedrockOptions
        {
            AccessKeyId = "AKIA",
            SecretAccessKey = "secret",
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        };
        var provider = AmazonBedrockProvider.Create(options, handler);
        return provider.ChatModel(modelId);
    }

    public static bool HasWarning(IReadOnlyList<AmazonBedrockWarning> warnings, string type, string feature, string? details)
    {
        foreach (var warning in warnings)
        {
            if (warning.Type == type && warning.Feature == feature && warning.Details == details)
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasCallWarning(IReadOnlyList<CallWarning> warnings, string type, string message)
    {
        foreach (var warning in warnings)
        {
            if (warning.Type == type && warning.Message == message)
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>Captures one Bedrock HTTP call and returns a scripted JSON body.</summary>
internal sealed class BedrockJsonHandler : HttpMessageHandler
{
    public BedrockJsonHandler(string responseJson)
    {
        ResponseJson = responseJson;
    }

    public string ResponseJson { get; set; }

    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    public Dictionary<string, string> ResponseHeaders { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public int Calls { get; private set; }

    public string Uri { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public string Method { get; private set; } = string.Empty;

    public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        Method = request.Method.Method;
        Uri = request.RequestUri?.AbsoluteUri ?? string.Empty;
        Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        Headers.Clear();
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
        }

        if (request.Headers.Authorization != null)
        {
            Headers["Authorization"] = request.Headers.Authorization.ToString();
        }

        var response = new HttpResponseMessage(Status)
        {
            Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json"),
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
