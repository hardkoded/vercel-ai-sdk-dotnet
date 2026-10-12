// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.TypeSafeAi;

namespace Vercel.AI.Tests.Upstream.TypesafeAi.TypesafeAiDecisionModel;

/// <summary>A request the test server received.</summary>
internal sealed class TypeSafeAiServerCall
{
    public string Url { get; init; } = string.Empty;

    public string Body { get; init; } = string.Empty;

    public Dictionary<string, string> Headers { get; init; } = new(StringComparer.OrdinalIgnoreCase);
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
            Body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false),
        };
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
            questions[pair.Key] = new DecisionQuestion(
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
        IReadOnlyDictionary<string, string>? headers = null)
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new DecisionModelCall(
            state ?? JsonState(ToObject(NativeRequest["state"])),
            questions ?? Questions(),
            document.RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None);
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
