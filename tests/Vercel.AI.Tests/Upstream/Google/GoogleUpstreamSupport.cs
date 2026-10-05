// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

/// <summary>HTTP stub and JSON helpers for Google parity tests.</summary>
internal static class GoogleUpstream
{
    public static LanguageModelCallOptions Hello()
    {
        return Prompt(new UserModelMessage("Hello"));
    }

    public static LanguageModelCallOptions Prompt(params ModelMessage[] messages)
    {
        return new LanguageModelCallOptions { Prompt = messages };
    }

    public static Dictionary<string, JsonElement> ProviderOptions(string json)
    {
        using var document = JsonDocument.Parse(json);
        var map = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            map[property.Name] = property.Value.Clone();
        }

        return map;
    }

    public static LanguageModelTool Function(string name, string schema, string? description = "", bool? strict = null)
    {
        using var document = JsonDocument.Parse(schema);
        return new LanguageModelTool(name, description, document.RootElement.Clone(), strict);
    }

    public static GoogleProviderTool Tool(string id, string? args = null, string? name = null)
    {
        JsonElement? element = null;
        if (args != null)
        {
            using var document = JsonDocument.Parse(args);
            element = document.RootElement.Clone();
        }

        return new GoogleProviderTool(id, element, name);
    }

    public static JsonElement Element(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static void JsonEqual(JsonNode? actual, string expected)
    {
        JsonAssert.Equal(actual, expected);
    }

    public static void JsonEqual(JsonElement actual, string expected)
    {
        JsonAssert.Equal(actual, expected);
    }

    public static GooglePreparedRequest Prepare(string modelId, string provider, LanguageModelCallOptions options, bool streaming = false)
    {
        return GoogleRequest.Prepare(modelId, provider, options, streaming);
    }

    public static LanguageModelGenerateResult Parse(string json, string? provider = null, Func<string>? generateId = null)
    {
        using var document = JsonDocument.Parse(json);
        return GoogleResponse.Parse(document.RootElement, new GoogleParseContext(provider, generateId));
    }

    public static GoogleProvider Client(RecordingHandler handler, GoogleOptions? options = null)
    {
        options ??= new GoogleOptions { ApiKey = "test-api-key" };
        if (string.IsNullOrEmpty(options.ApiKey) && !options.UseBearerToken)
        {
            options.ApiKey = "test-api-key";
        }

        return GoogleProvider.Create(options, handler);
    }

    public static async Task<List<LanguageModelStreamPart>> Collect(IAsyncEnumerable<LanguageModelStreamPart> parts)
    {
        var list = new List<LanguageModelStreamPart>();
        await foreach (var part in parts.ConfigureAwait(false))
        {
            list.Add(part);
        }

        return list;
    }
}

/// <summary>Captures one request and returns a scripted body.</summary>
internal sealed class RecordingHandler : HttpMessageHandler
{
    public List<string> Uris { get; } = new();

    public string Body { get; private set; } = string.Empty;

    public Dictionary<string, string> RequestHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string ResponseText { get; set; } = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}]}";

    public string MediaType { get; set; } = "application/json";

    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    public Dictionary<string, string> ResponseHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    public int Calls
    {
        get { return Uris.Count; }
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Uris.Add(request.RequestUri?.AbsoluteUri ?? string.Empty);
        Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        RequestHeaders.Clear();
        foreach (var header in request.Headers)
        {
            RequestHeaders[header.Key] = string.Join(",", header.Value);
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                RequestHeaders[header.Key] = string.Join(",", header.Value);
            }
        }

        if (request.Headers.Authorization != null)
        {
            RequestHeaders["Authorization"] = request.Headers.Authorization.ToString();
        }

        var message = new HttpResponseMessage(Status)
        {
            Content = new StringContent(ResponseText, Encoding.UTF8, MediaType),
        };
        foreach (var header in ResponseHeaders)
        {
            message.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return message;
    }
}
