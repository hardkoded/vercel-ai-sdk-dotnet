// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayExtractionModel;

/// <summary>Fixtures and helpers shared by the <c>gateway-extraction-model.test.ts</c> port.</summary>
internal static class GatewayExtractionModelSupport
{
    internal const string TestText = "Invoice INV-2041 from Acme. Total due $1,250.00.";

    internal const string FieldsJson = """
        {
          "invoiceNumber": { "type": "text", "instructions": "The invoice number" },
          "total": { "type": "number", "instructions": "Total amount due", "decimals": 2 },
          "contacts": { "type": "list", "items": "text" }
        }
        """;

    internal const string DummyAnswers = """
        {
          "invoiceNumber": {
            "type": "text",
            "value": "INV-2041",
            "confidence": 0.98,
            "pNull": 0.01,
            "source": { "type": "text", "part": 0, "start": 8, "end": 16 }
          },
          "total": { "type": "number", "value": 1250, "guess": 1250 },
          "contacts": { "type": "list", "value": null }
        }
        """;

    internal static IReadOnlyList<DecisionModelStatePart> TestState { get; } =
    [new DecisionModelTextPart(TestText)];

    internal static Dictionary<string, ExtractionField> TestFields()
    {
        return new Dictionary<string, ExtractionField>
        {
            ["invoiceNumber"] = new("text", DecisionInput.From("The invoice number")),
            ["total"] = new("number", DecisionInput.From("Total amount due")) { Decimals = 2 },
            ["contacts"] = new("list") { Items = "text" },
        };
    }

    internal static ExtractionModelCall Call(
        IReadOnlyList<DecisionModelStatePart>? state = null,
        string? providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        string? referenceDate = null,
        double? maxOutputTokens = null,
        string? reasoning = null
    )
    {
        using var document = JsonDocument.Parse(providerOptions ?? "{}");
        var options = providerOptions == null ? default : document.RootElement.Clone();
        return new ExtractionModelCall(
            state ?? TestState,
            TestFields(),
            options,
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None,
            referenceDate,
            maxOutputTokens,
            reasoning
        );
    }

    internal static DecisionModelFilePart File(string mediaType, LanguageModelFilePart data)
    {
        return new DecisionModelFilePart(mediaType, data);
    }

    /// <summary>Builds a model whose requests are answered with <paramref name="body"/> (an extraction-model response).</summary>
    internal static (Vercel.AI.Gateway.GatewayExtractionModel Model, OpenAICapture Capture) Setup(
        string? body = null,
        HttpStatusCode status = HttpStatusCode.OK,
        IReadOnlyDictionary<string, string>? o11yHeaders = null
    )
    {
        var capture = new OpenAICapture { ResponseJson = body ?? Response(), Status = status };
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                ApiKey = "test-token",
                BaseUrl = "https://api.test.com",
                O11yHeaders = o11yHeaders,
            },
            capture
        );
        return (provider.ExtractionModel("scaledown/extract-1"), capture);
    }

    /// <summary>An extraction-model response body. <paramref name="extra"/> adds or replaces top-level members.</summary>
    internal static string Response(
        string? answers = null,
        params (string Name, string Json)[] extra
    )
    {
        var body = new JsonObject { ["answers"] = JsonNode.Parse(answers ?? DummyAnswers) };
        foreach (var pair in extra)
        {
            body[pair.Name] = JsonNode.Parse(pair.Json);
        }

        return body.ToJsonString();
    }

    internal static string Error(string error)
    {
        return "{\"error\":" + error + "}";
    }

    internal static JsonNode? RequestBody(OpenAICapture capture)
    {
        return JsonNode.Parse(capture.Body);
    }
}
