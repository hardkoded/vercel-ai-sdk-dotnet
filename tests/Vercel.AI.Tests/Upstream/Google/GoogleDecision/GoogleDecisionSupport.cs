// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Google.GoogleDecision;

/// <summary>Fixtures and helpers shared by the <c>google-decision.test.ts</c> port.</summary>
internal static class GoogleDecisionSupport
{
    internal static JsonObject Fixture()
    {
        return (JsonObject)JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "google-decision.json")))!;
    }

    internal static Dictionary<string, EvaluationQuestion> Questions()
    {
        return new Dictionary<string, EvaluationQuestion>
        {
            ["department"] = new EvaluationQuestion("choice", "Pick the team.", new Dictionary<string, object?> { ["technical"] = "Bugs", ["billing"] = "Charges" }),
            ["severity"] = new EvaluationQuestion("score", "[\"Rate severity.\"]", new List<object?> { "Low", "Medium", "High" }),
        };
    }

    internal static EvaluationModelCall Call(
        IReadOnlyDictionary<string, EvaluationQuestion>? questions = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        using var document = JsonDocument.Parse(providerOptions);
        var state = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["type"] = "json",
                ["value"] = new Dictionary<string, object?> { ["message"] = "A billing issue with a workaround." },
            },
        };
        return new EvaluationModelCall(state, questions ?? Questions(), document.RootElement.Clone(), headers ?? new Dictionary<string, string>(), cancellationToken);
    }

    internal static (DecisionLanguageModel Model, GoogleDecisionCapture Capture) Setup(JsonNode? body = null, bool withBaseUrl = true)
    {
        var capture = new GoogleDecisionCapture { ResponseJson = (body ?? Fixture()).ToJsonString() };
        capture.ResponseHeaders["x-request-id"] = "req-test";
        var options = new GoogleOptions { ApiKey = "test-key" };
        if (withBaseUrl)
        {
            options.BaseUrl = "https://example.com/v1beta";
        }

        options.Headers["x-provider"] = "configured";
        var provider = GoogleProvider.Create(options, capture);
        return (provider.DecisionModel("gemini-3.5-flash-lite"), capture);
    }

    internal static JsonObject RequestBody(GoogleDecisionCapture capture)
    {
        return (JsonObject)JsonNode.Parse(capture.Body)!;
    }
}

/// <summary>Records one Google HTTP call and returns a scripted body.</summary>
internal sealed class GoogleDecisionCapture : HttpMessageHandler
{
    public int Calls { get; private set; }

    public string Uri { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string ResponseJson { get; set; } = "{}";

    public Dictionary<string, string> ResponseHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        Uri = request.RequestUri?.AbsoluteUri ?? string.Empty;
        Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        Headers.Clear();
        foreach (var header in request.Headers)
        {
            Headers[header.Key] = string.Join(",", header.Value);
        }

        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json") };
        foreach (var header in ResponseHeaders)
        {
            response.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return response;
    }
}
