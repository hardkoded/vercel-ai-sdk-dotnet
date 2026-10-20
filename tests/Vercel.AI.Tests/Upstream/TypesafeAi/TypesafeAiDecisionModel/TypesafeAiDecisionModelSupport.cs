// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.TypeSafeAi;
using static Vercel.AI.Tests.DecisionFixtures;

namespace Vercel.AI.Tests.Upstream.TypesafeAi.TypesafeAiDecisionModel;

/// <summary>One part of a <c>multipart/form-data</c> request.</summary>
internal sealed class TypeSafeAiFormPart
{
    public string Name { get; init; } = string.Empty;

    public string? FileName { get; init; }

    public string? ContentType { get; init; }

    public byte[] Bytes { get; init; } = Array.Empty<byte>();

    public string Text => Encoding.UTF8.GetString(Bytes);
}

/// <summary>A request the test server received.</summary>
internal sealed class TypeSafeAiServerCall
{
    public string Url { get; init; } = string.Empty;

    public string Body { get; init; } = string.Empty;

    public Dictionary<string, string> Headers { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public List<TypeSafeAiFormPart> Form { get; init; } = new();
}

/// <summary>Stands in for <c>createTestServer</c>: records calls and answers with a configured response.</summary>
internal sealed class TypeSafeAiServer : HttpMessageHandler
{
    public List<TypeSafeAiServerCall> Calls { get; } = new();

    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    public string ResponseBody { get; set; } = string.Empty;

    public Dictionary<string, string> ResponseHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var call = new TypeSafeAiServerCall
        {
            Url = request.RequestUri!.AbsoluteUri,
            Body = request.Content == null || request.Content is MultipartContent ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false),
        };
        if (request.Content is MultipartContent multipart)
        {
            foreach (var part in multipart)
            {
                call.Form.Add(new TypeSafeAiFormPart
                {
                    Name = part.Headers.ContentDisposition!.Name!.Trim('"'),
                    FileName = part.Headers.ContentDisposition.FileName?.Trim('"'),
                    ContentType = part.Headers.ContentType?.ToString(),
                    Bytes = await part.ReadAsByteArrayAsync().ConfigureAwait(false),
                });
            }
        }

        foreach (var header in request.Headers)
        {
            call.Headers[header.Key] = string.Join(",", header.Value);
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                call.Headers[header.Key] = string.Join(",", header.Value);
            }
        }

        Calls.Add(call);
        var response = new HttpResponseMessage(Status) { Content = new StringContent(ResponseBody, Encoding.UTF8, "application/json") };
        foreach (var pair in ResponseHeaders)
        {
            response.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
        }

        return response;
    }
}

/// <summary>Fixtures and helpers shared by the <c>typesafe-ai-decision-model.test.ts</c> port.</summary>
internal static class TypesafeAiDecisionModelSupport
{
    internal const string Url = "https://api.typesafe.ai/v1/systemone";

    internal static readonly string NativeRequestJson = Fixture("decision-request.json");

    internal static readonly string NativeResponseJson = Fixture("decision-response.json");

    internal static JsonObject NativeRequest => JsonNode.Parse(NativeRequestJson)!.AsObject();

    internal static string Fixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "TypesafeAi", name));
    }

    internal static TypeSafeAiServer Server()
    {
        return new TypeSafeAiServer { ResponseBody = NativeResponseJson };
    }

    internal static TypeSafeAiProvider CreateProvider(TypeSafeAiServer server, string? apiKey = "test-api-key")
    {
        return TypeSafeAiProvider.Create(new TypeSafeAiOptions { ApiKey = apiKey }, server);
    }

    /// <summary>The questions of the native request, with <c>noul</c> spelled <c>boolean</c> as the public API does.</summary>
    internal static Dictionary<string, DecisionQuestion> Questions()
    {
        var questions = new Dictionary<string, DecisionQuestion>();
        foreach (var pair in NativeRequest["questions"]!.AsObject())
        {
            var question = pair.Value!.AsObject();
            var type = question["type"]!.GetValue<string>();
            questions[pair.Key] = Q(
                type == "noul" ? "boolean" : type,
                ToObject(question["instructions"]),
                ToObject(question["criteria"]));
        }

        return questions;
    }

    internal static DecisionModelCall Call(
        IReadOnlyDictionary<string, DecisionQuestion>? questions = null,
        object? state = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        double? maxOutputTokens = null,
        string? reasoning = null)
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new DecisionModelCall(
            P((IEnumerable<object?>)(state ?? JsonState(ToObject(NativeRequest["state"])))),
            questions ?? Questions(),
            document.RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None,
            maxOutputTokens,
            reasoning);
    }

    internal static Dictionary<string, object?> FilePart(string mediaType, object? data, string? filename = null, object? reference = null, bool withReference = false, bool? insertReference = null)
    {
        var part = new Dictionary<string, object?>
        {
            ["type"] = "file",
            ["mediaType"] = mediaType,
            ["data"] = data is IDictionary<string, object?> ? data : new Dictionary<string, object?> { ["type"] = "data", ["data"] = data },
        };
        if (filename != null)
        {
            part["filename"] = filename;
        }

        if (withReference || insertReference != null)
        {
            var options = new Dictionary<string, object?>();
            if (withReference)
            {
                options["reference"] = reference;
            }

            if (insertReference != null)
            {
                options["insertReference"] = insertReference;
            }

            part["providerOptions"] = new Dictionary<string, object?> { ["typesafe"] = options };
        }

        return part;
    }

    internal static Dictionary<string, object?> TextPart(string text)
    {
        return new Dictionary<string, object?> { ["type"] = "text", ["text"] = text };
    }

    internal static Dictionary<string, object?> JsonPart(object? value)
    {
        return new Dictionary<string, object?> { ["type"] = "json", ["value"] = value };
    }

    internal static List<object?> JsonState(object? value)
    {
        return new List<object?> { new Dictionary<string, object?> { ["type"] = "json", ["value"] = value } };
    }

    internal static object? ToObject(JsonNode? node)
    {
        switch (node)
        {
            case null:
                return null;
            case JsonObject value:
                return value.ToDictionary(pair => pair.Key, pair => ToObject(pair.Value));
            case JsonArray value:
                return value.Select(ToObject).ToList();
            default:
                var element = node.GetValue<JsonElement>();
                return element.ValueKind == JsonValueKind.String ? element.GetString() : (object)element.GetDouble();
        }
    }
}
