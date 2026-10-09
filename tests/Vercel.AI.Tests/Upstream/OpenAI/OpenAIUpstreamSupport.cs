// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

/// <summary>HTTP capture and JSON helpers for OpenAI upstream parity tests.</summary>
internal static class OpenAIUpstream
{
    internal const string ChatOk =
        "{\"id\":\"chatcmpl\",\"object\":\"chat.completion\",\"created\":1,\"model\":\"m\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"ok\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}";

    internal const string ResponsesEmpty =
        "{\"id\":\"resp_test\",\"object\":\"response\",\"created_at\":1,\"status\":\"completed\",\"model\":\"gpt-99\",\"output\":[],\"usage\":{\"input_tokens\":1,\"output_tokens\":1,\"total_tokens\":2}}";

    internal static OpenAIProvider Provider(OpenAICapture capture, Action<OpenAIOptions>? configure = null)
    {
        var options = new OpenAIOptions { ApiKey = "test-api-key" };
        configure?.Invoke(options);
        return OpenAIProvider.Create(options, capture);
    }

    internal static LanguageModelCallOptions Hello()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
        };
    }

    internal static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    internal static IReadOnlyDictionary<string, JsonElement> OpenAIOptionsJson(string openaiObject)
    {
        return new Dictionary<string, JsonElement> { ["openai"] = Json(openaiObject) };
    }

    internal static LanguageModelTool Tool(string name, string? description, bool? strict = null, string schema = "{\"type\":\"object\",\"properties\":{}}")
    {
        return new LanguageModelTool(name, description, Json(schema), strict);
    }

    internal static void Equal(JsonNode? actual, string expected)
    {
        JsonAssert.Equal(actual, expected);
    }

    internal static string Header(OpenAICapture capture, string name)
    {
        return capture.Headers.TryGetValue(name, out var value) ? value : string.Empty;
    }

    internal static void AssertStandardHeaders(OpenAICapture capture)
    {
        Assert.Equal("Bearer test-api-key", Header(capture, "Authorization"));
        Assert.Equal("test-organization", Header(capture, "OpenAI-Organization"));
        Assert.Equal("test-project", Header(capture, "OpenAI-Project"));
        Assert.Equal("provider-header-value", Header(capture, "Custom-Provider-Header"));
        Assert.Equal("request-header-value", Header(capture, "Custom-Request-Header"));
        Assert.Equal("application/json", MediaTypeHeaderValue.Parse(Header(capture, "Content-Type")).MediaType);
    }

    internal static async Task<List<LanguageModelStreamPart>> Read(IAsyncEnumerable<LanguageModelStreamPart> parts)
    {
        var list = new List<LanguageModelStreamPart>();
        await foreach (var part in parts.ConfigureAwait(false))
        {
            list.Add(part);
        }

        return list;
    }

    internal static async Task<Dictionary<string, List<string>>> Fields(MultipartFormDataContent content)
    {
        var fields = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var part in content)
        {
            var name = part.Headers.ContentDisposition?.Name?.Trim('"') ?? string.Empty;
            var value = await part.ReadAsStringAsync().ConfigureAwait(false);
            if (!fields.TryGetValue(name, out var list))
            {
                list = new List<string>();
                fields[name] = list;
            }

            list.Add(value);
        }

        content.Dispose();
        return fields;
    }
}

/// <summary>Keeps tests that set <c>OPENAI_BASE_URL</c> from racing other OpenAI tests.</summary>
[CollectionDefinition("OpenAIEnvironment", DisableParallelization = true)]
public sealed class OpenAIEnvironmentCollection
{
}

/// <summary>One part of a captured multipart request.</summary>
internal sealed record OpenAIMultipartPart(string Name, string? FileName, string? MediaType, byte[] Data);

/// <summary>Records one OpenAI HTTP call and returns a scripted body.</summary>
internal sealed class OpenAICapture : HttpMessageHandler
{
    public string Method { get; private set; } = string.Empty;

    public int Calls { get; private set; }

    public string Uri { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<OpenAIMultipartPart> Parts { get; } = new();

    public string ResponseJson { get; set; } = OpenAIUpstream.ChatOk;

    public string? ServerSentEvents { get; set; }

    public byte[]? ResponseBytes { get; set; }

    public string ResponseMediaType { get; set; } = "application/json";

    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    public Dictionary<string, string> ResponseHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        Method = request.Method.Method;
        Uri = request.RequestUri?.AbsoluteUri ?? string.Empty;
        Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
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

        Parts.Clear();
        if (request.Content is MultipartFormDataContent multipart)
        {
            foreach (var part in multipart)
            {
                var disposition = part.Headers.ContentDisposition;
                Parts.Add(new OpenAIMultipartPart(
                    disposition?.Name?.Trim('"') ?? string.Empty,
                    disposition?.FileName?.Trim('"'),
                    part.Headers.ContentType?.MediaType,
                    await part.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false)));
            }
        }

        HttpContent content;
        if (ResponseBytes != null)
        {
            content = new ByteArrayContent(ResponseBytes);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(ResponseMediaType);
        }
        else if (ServerSentEvents != null)
        {
            content = new StringContent(ServerSentEvents, Encoding.UTF8, "text/event-stream");
        }
        else
        {
            content = new StringContent(ResponseJson, Encoding.UTF8, "application/json");
        }

        var response = new HttpResponseMessage(Status) { Content = content };
        foreach (var pair in ResponseHeaders)
        {
            if (!response.Headers.TryAddWithoutValidation(pair.Key, pair.Value))
            {
                response.Content.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }
        }

        return response;
    }
}
