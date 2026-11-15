// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.ScaleDown;
using static Vercel.AI.Tests.DecisionFixtures;

namespace Vercel.AI.Tests.Upstream.Scaledown.ScaledownDecisionModel;

/// <summary>A request the test server received.</summary>
internal sealed class ScaleDownServerCall
{
    public string Url { get; init; } = string.Empty;

    public string Body { get; init; } = string.Empty;

    public Dictionary<string, string> Headers { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public JsonNode? Json => JsonNode.Parse(Body);
}

/// <summary>Stands in for <c>createTestServer</c>: records calls and answers with a configured response.</summary>
internal sealed class ScaleDownServer : HttpMessageHandler
{
    public List<ScaleDownServerCall> Calls { get; } = new();

    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    public string ResponseBody { get; set; } = ScaledownDecisionModelSupport.ResponseJson;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var call = new ScaleDownServerCall
        {
            Url = request.RequestUri!.AbsoluteUri,
            Body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false),
        };
        foreach (var header in request.Headers)
        {
            call.Headers[header.Key] = string.Join(",", header.Value);
        }

        Calls.Add(call);
        return new HttpResponseMessage(Status) { Content = new StringContent(ResponseBody, Encoding.UTF8, "application/json") };
    }
}

/// <summary>Fixtures and helpers shared by the <c>scaledown-decision-model.test.ts</c> port.</summary>
internal static class ScaledownDecisionModelSupport
{
    internal const string Url = "https://api.scaledown.xyz/v1/scaledown";

    // Based on the official choice, noul, and score examples: https://docs.scaledown.ai/api-reference/decisions
    internal const string ResponseJson = """
        {"model":"classify-1","answers":{"category":{"type":"choice","choice":"bug","probabilities":{"bug":0.93,"feedback":0.07},"confidence":0.93},"workaround":{"type":"noul","noul":0.79},"severity":{"type":"score","score":1.43,"probabilities":{"0":0,"1":0.57,"2":0.43},"confidence":0.57,"legend":{"0":"Cosmetic","1":"Degraded","2":"Blocking"}}},"usage":{"input_tokens":103,"output_tokens":3,"cost":0.0000044}}
        """;

    internal static readonly IReadOnlyList<DecisionModelStatePart> DefaultState = [new DecisionModelTextPart("The export crashes in Safari.")];

    internal static ScaleDownProvider CreateProvider(ScaleDownServer server, string? apiKey = "test-key")
    {
        return ScaleDownProvider.Create(new ScaleDownProviderSettings { ApiKey = apiKey }, server);
    }

    /// <summary>The upstream <c>options.questions</c>.</summary>
    internal static Dictionary<string, DecisionQuestion> Questions()
    {
        return new Dictionary<string, DecisionQuestion>
        {
            ["category"] = Q("choice", null, new Dictionary<string, object?> { ["bug"] = "Broken functionality", ["feedback"] = null }),
            ["workaround"] = Q("boolean", "Is there a workaround?"),
            ["severity"] = Q("score", null, new List<object?> { "Cosmetic", "Degraded", "Blocking" }),
        };
    }

    internal static DecisionModelCall Call(
        IReadOnlyDictionary<string, DecisionQuestion>? questions = null,
        IReadOnlyList<DecisionModelStatePart>? state = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        double? maxOutputTokens = null,
        string? reasoning = null,
        CancellationToken cancellationToken = default)
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new DecisionModelCall(
            state ?? DefaultState,
            questions ?? Questions(),
            document.RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            cancellationToken,
            maxOutputTokens,
            reasoning);
    }

    internal static DecisionModelJsonPart JsonPart(string json)
    {
        using var document = JsonDocument.Parse(json);
        return new DecisionModelJsonPart(document.RootElement);
    }

    internal static DecisionModelFilePart FilePart(string mediaType, byte[]? bytes = null, string? base64 = null)
    {
        return new DecisionModelFilePart(mediaType, new LanguageModelFilePart("data", mediaType) { Bytes = bytes, Base64 = base64 });
    }
}
