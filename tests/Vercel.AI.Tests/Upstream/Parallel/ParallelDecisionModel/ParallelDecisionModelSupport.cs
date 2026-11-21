// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Parallel;
using Vercel.AI.Prompt;
using static Vercel.AI.Tests.DecisionFixtures;

namespace Vercel.AI.Tests.Upstream.Parallel.ParallelDecisionModel;

/// <summary>A request the test server received.</summary>
internal sealed class ParallelServerCall
{
    public string Url { get; init; } = string.Empty;

    public string Body { get; init; } = string.Empty;

    public Dictionary<string, string> Headers { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public JsonNode? Json => JsonNode.Parse(Body);
}

/// <summary>Stands in for the mocked <c>fetch</c>: records calls and answers with a configured response.</summary>
internal sealed class ParallelServer : HttpMessageHandler
{
    public ParallelServer(string? body = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        ResponseBody = body ?? ParallelDecisionModelSupport.Fixture;
        Status = status;
    }

    public List<ParallelServerCall> Calls { get; } = new();

    public HttpStatusCode Status { get; }

    public string ResponseBody { get; }

    /// <summary>Tokens the handler received, one per call. Stands in for <c>init.signal</c> of <c>fetch</c>.</summary>
    public List<CancellationToken> Signals { get; } = new();

    /// <summary>When set, the handler waits on this task, honoring the token, before it answers.</summary>
    public Task? Hold { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        Signals.Add(cancellationToken);
        var call = new ParallelServerCall
        {
            Url = request.RequestUri!.AbsoluteUri,
            Body =
                request.Content == null
                    ? string.Empty
                    : await request.Content.ReadAsStringAsync().ConfigureAwait(false),
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
        if (Hold != null)
        {
            await Hold.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        var response = new HttpResponseMessage(Status)
        {
            Content = new StringContent(ResponseBody, Encoding.UTF8, "application/json"),
        };
        response.Headers.Add("retry-after", "2");
        response.Headers.Add("x-request-id", "request-123");
        return response;
    }
}

/// <summary>Fixtures and helpers shared by the <c>parallel-decision-model.test.ts</c> port.</summary>
internal static class ParallelDecisionModelSupport
{
    internal const string Url = "https://example.com/v1/decisions";

    // Illustrative fixture based on the public contract, not a live capture. Upstream src/__fixtures__/decisions.json.
    internal const string Fixture = """
        {
          "mode": "fast",
          "decision_id": "decisions_1234567890abcdef1234567890abcdef",
          "answers": {
            "hasSearch": { "type": "noul", "noul": 0.9992643798509062 },
            "availability": {
              "type": "choice",
              "choice": "available",
              "probabilities": { "available": 0.987654321, "unknown": 0.012345679 },
              "confidence": 0.87654321
            },
            "fit": {
              "type": "score",
              "score": 1.7123456789,
              "legend": { "0": "No fit", "1": "Partial fit", "2": "Full fit" },
              "probabilities": { "0": 0.1, "1": 0.0876543211, "2": 0.8123456789 },
              "confidence": 0.7654321
            }
          },
          "usage": { "input_tokens": 4212, "output_tokens": 0 },
          "citations": [
            { "url": "https://example.com/search", "title": "Search API", "excerpts": ["A web search API.", "Current documentation."] },
            { "url": "https://example.com/reference", "excerpts": [] },
            { "url": "https://example.com/untitled", "title": null, "excerpts": ["Evidence without a page title."] }
          ]
        }
        """;

    internal static JsonObject FixtureNode() => JsonNode.Parse(Fixture)!.AsObject();

    internal static readonly IReadOnlyList<DecisionModelStatePart> DefaultState =
    [
        JsonPart("""{"company":"Parallel","website":"https://parallel.ai"}"""),
    ];

    /// <summary>The upstream <c>setup</c> provider.</summary>
    internal static ParallelProvider CreateProvider(ParallelServer server)
    {
        return ParallelProvider.Create(
            new ParallelProviderSettings
            {
                ApiKey = "test-key",
                BaseUrl = "https://example.com/v1/",
                Headers = new Dictionary<string, string> { ["x-provider"] = "custom" },
            },
            server
        );
    }

    internal static DecisionParallelModel CreateModel(ParallelServer server) =>
        CreateProvider(server).DecisionModel("fast");

    internal static DecisionQuestion HasSearch() => Q("boolean", "Does it offer web search?");

    internal static DecisionQuestion Availability(object? criteria = null) =>
        Q(
            "choice",
            "Is the API available?",
            criteria
                ?? new Dictionary<string, object?>
                {
                    ["available"] = "Publicly available",
                    ["unknown"] = "Insufficient evidence",
                }
        );

    internal static DecisionQuestion Fit(object? criteria = null) =>
        Q(
            "score",
            "How well does it fit?",
            criteria ?? new List<object?> { "No fit", "Partial fit", "Full fit" }
        );

    /// <summary>The upstream <c>options.questions</c>.</summary>
    internal static Dictionary<string, DecisionQuestion> Questions()
    {
        return new Dictionary<string, DecisionQuestion>
        {
            ["hasSearch"] = HasSearch(),
            ["availability"] = Availability(),
            ["fit"] = Fit(),
        };
    }

    internal static DecisionModelCall Call(
        IReadOnlyDictionary<string, DecisionQuestion>? questions = null,
        IReadOnlyList<DecisionModelStatePart>? state = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        double? maxOutputTokens = null,
        string? reasoning = null,
        CancellationToken cancellationToken = default
    )
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new DecisionModelCall(
            state ?? DefaultState,
            questions ?? Questions(),
            document.RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            cancellationToken,
            maxOutputTokens,
            reasoning
        );
    }

    internal static DecisionModelJsonPart JsonPart(string json)
    {
        using var document = JsonDocument.Parse(json);
        return new DecisionModelJsonPart(document.RootElement);
    }

    internal static DecisionModelFilePart FilePart(string mediaType, string base64)
    {
        return new DecisionModelFilePart(
            mediaType,
            new LanguageModelFilePart("data", mediaType) { Base64 = base64 }
        );
    }
}
