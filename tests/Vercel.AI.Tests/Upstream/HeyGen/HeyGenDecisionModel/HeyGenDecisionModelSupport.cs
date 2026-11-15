// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.HeyGen;
using HeyGenProviderType = Vercel.AI.HeyGen.HeyGenProvider;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using static Vercel.AI.Tests.DecisionFixtures;

namespace Vercel.AI.Tests.Upstream.HeyGen.HeyGenDecisionModel;

/// <summary>Fixtures and helpers shared by the <c>heygen-decision-model.test.ts</c> port.</summary>
internal static class HeyGenDecisionModelSupport
{
    // Illustrative response from HeyGen's Decision documentation, not a live capture.
    private const string FixtureJson = """
        {"data":{"id":"decision-0123456789abcdef","request_id":"0123456789abcdef0123456789abcdef","model":"heygen-decision-1","answers":{"blocked":{"type":"noul","noul":0.96},"team":{"type":"choice","choice":"video","probabilities":{"video":0.8,"account":0.15,"billing":0.05},"confidence":0.7},"impact":{"type":"score","score":1.75,"probabilities":{"0":0.05,"1":0.15,"2":0.8},"legend":{"0":"Cosmetic","1":"Slower","2":"Blocked"},"confidence":0.625}},"usage":{"input_tokens":1200,"output_tokens":0}}}
        """;

    internal static JsonObject Fixture() => (JsonObject)JsonNode.Parse(FixtureJson)!;

    /// <summary>The upstream <c>options.questions</c>.</summary>
    internal static Dictionary<string, DecisionQuestion> Questions()
    {
        return new Dictionary<string, DecisionQuestion>
        {
            ["blocked"] = Blocked(),
            ["team"] = Team(),
            ["impact"] = Impact(),
        };
    }

    internal static DecisionQuestion Blocked() => Q("boolean", "Is the customer unable to work?");

    internal static DecisionQuestion Team()
    {
        return Q("choice", "Which team should handle this request?", new Dictionary<string, object?> { ["video"] = "Rendering", ["account"] = "Access", ["billing"] = "Payments" });
    }

    internal static DecisionQuestion Impact()
    {
        return Q("score", "How much does this affect the customer?", new List<object?> { "Cosmetic", "Slower", "Blocked" });
    }

    internal static IReadOnlyList<DecisionModelStatePart> DefaultState { get; } = [new DecisionModelTextPart("Video export fails for everyone.")];

    /// <summary>The upstream <c>options</c> plus overrides.</summary>
    internal static DecisionModelCall Call(
        IReadOnlyDictionary<string, DecisionQuestion>? questions = null,
        IReadOnlyList<DecisionModelStatePart>? state = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        double? maxOutputTokens = null,
        string? reasoning = null)
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new DecisionModelCall(state ?? DefaultState, questions ?? Questions(), document.RootElement.Clone(), headers ?? new Dictionary<string, string>(), CancellationToken.None, maxOutputTokens, reasoning);
    }

    /// <summary>The upstream <c>setup</c>.</summary>
    internal static (HeyGenProviderType Provider, DecisionHeyGenModel Model, OpenAICapture Capture) Setup(JsonNode? body = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        var capture = new OpenAICapture { ResponseJson = (body ?? Fixture()).ToJsonString(), Status = status };
        capture.ResponseHeaders["x-trace"] = "trace-1";
        var options = new OpenAICompatibleOptions { ApiKey = "test-key", BaseUrl = "https://example.com/" };
        options.Headers["x-provider"] = "custom";
        options.Headers["shared"] = "provider";
        var provider = HeyGenProviderType.Create(options, capture);
        return (provider, provider.DecisionModel("heygen-decision-1"), capture);
    }

    internal static JsonObject RequestBody(OpenAICapture capture) => (JsonObject)JsonNode.Parse(capture.Body)!;

    internal static DecisionModelFilePart Data(string mediaType, byte[] bytes)
    {
        return new DecisionModelFilePart(mediaType, new LanguageModelFilePart("data", mediaType) { Bytes = bytes });
    }

    internal static DecisionModelFilePart Base64(string mediaType, string base64)
    {
        return new DecisionModelFilePart(mediaType, new LanguageModelFilePart("data", mediaType) { Base64 = base64 });
    }

    internal static DecisionModelFilePart Url(string mediaType, string url)
    {
        return new DecisionModelFilePart(mediaType, new LanguageModelFilePart("url", mediaType) { Url = new Uri(url) });
    }

    internal static DecisionModelFilePart Reference(string mediaType, string provider, string id)
    {
        return new DecisionModelFilePart(mediaType, new LanguageModelFilePart("reference", mediaType) { Reference = new Dictionary<string, string> { [provider] = id } });
    }

    internal static DecisionModelFilePart InlineText(string mediaType, string text)
    {
        return new DecisionModelFilePart(mediaType, new LanguageModelFilePart("text", mediaType) { Text = text });
    }
}
