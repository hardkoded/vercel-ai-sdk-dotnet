// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Liquid;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using static Vercel.AI.Tests.DecisionFixtures;
using LiquidProviderType = Vercel.AI.Liquid.LiquidProvider;

namespace Vercel.AI.Tests.Upstream.Liquid.LiquidDecisionModel;

/// <summary>Fixtures and helpers shared by the <c>liquid-decision-model.test.ts</c> port.</summary>
internal static class LiquidDecisionModelSupport
{
    internal static readonly byte[] Png = [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a];

    internal static readonly byte[] Wav =
    [
        0x52,
        0x49,
        0x46,
        0x46,
        0,
        0,
        0,
        0,
        0x57,
        0x41,
        0x56,
        0x45,
    ];

    /// <summary>The upstream <c>fixture</c>: <c>decisions.json</c>, captured from the live Liquid API.</summary>
    internal static JsonObject Fixture() => Load("decisions.json");

    /// <summary>The upstream <c>boxNotFoundFixture</c>.</summary>
    internal static JsonObject BoxNotFoundFixture() => Load("decisions-box-not-found.json");

    /// <summary>The upstream <c>textAnswers</c>: the answers of the fixture that a text-only request asks for.</summary>
    internal static JsonObject TextAnswers()
    {
        var answers = (JsonObject)Fixture()["answers"]!;
        return new JsonObject
        {
            ["department"] = answers["department"]!.DeepClone(),
            ["is_urgent"] = answers["is_urgent"]!.DeepClone(),
            ["frustration"] = answers["frustration"]!.DeepClone(),
        };
    }

    /// <summary>The upstream <c>{ ...fixture, answers: textAnswers }</c>.</summary>
    internal static JsonObject TextFixture()
    {
        var body = Fixture();
        body["answers"] = TextAnswers();
        return body;
    }

    /// <summary>The upstream <c>options.questions</c>.</summary>
    internal static Dictionary<string, DecisionQuestion> Questions()
    {
        return new Dictionary<string, DecisionQuestion>
        {
            ["department"] = Department(),
            ["is_urgent"] = IsUrgent(),
            ["frustration"] = Frustration(),
        };
    }

    internal static DecisionQuestion Department()
    {
        return Q(
            "choice",
            "Which team should handle this?",
            new Dictionary<string, object?>
            {
                ["billing"] = "Payments, invoicing, refunds",
                ["technical"] = "Bugs, outages, integrations",
                ["sales"] = "Pricing, upgrades, new accounts",
            }
        );
    }

    internal static DecisionQuestion IsUrgent() => Q("boolean", "Does this convey urgency?");

    internal static DecisionQuestion Frustration()
    {
        return Q(
            "score",
            "How frustrated is the customer?",
            new List<object?> { "Calm", "Frustrated", "Very angry" }
        );
    }

    /// <summary>The upstream <c>boxOptions.questions</c>.</summary>
    internal static Dictionary<string, DecisionQuestion> BoxQuestions()
    {
        return new Dictionary<string, DecisionQuestion>
        {
            ["subject"] = Q("box", "The animal in the picture."),
        };
    }

    internal static IReadOnlyList<DecisionModelStatePart> DefaultState { get; } =
    [new DecisionModelTextPart("Help! My payouts have been failing for 3 days.")];

    /// <summary>The upstream <c>boxOptions.state</c>.</summary>
    internal static IReadOnlyList<DecisionModelStatePart> BoxState { get; } =
    [new DecisionModelTextPart("A support screenshot."), Data("image/png", Png)];

    /// <summary>The upstream <c>options</c> plus overrides.</summary>
    internal static DecisionModelCall Call(
        IReadOnlyDictionary<string, DecisionQuestion>? questions = null,
        IReadOnlyList<DecisionModelStatePart>? state = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        double? maxOutputTokens = null,
        string? reasoning = null
    )
    {
        using var document = JsonDocument.Parse(providerOptions);
        return new DecisionModelCall(
            state ?? DefaultState,
            questions ?? Questions(),
            document.RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None,
            maxOutputTokens,
            reasoning
        );
    }

    /// <summary>The upstream <c>setup</c>.</summary>
    internal static (
        LiquidProviderType Provider,
        DecisionLiquidModel Model,
        OpenAICapture Capture
    ) Setup(JsonNode? body = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        var capture = new OpenAICapture
        {
            ResponseJson = (body ?? TextFixture()).ToJsonString(),
            Status = status,
        };
        capture.ResponseHeaders["x-trace"] = "trace-1";
        var options = new OpenAICompatibleOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://example.com/",
        };
        options.Headers["x-provider"] = "custom";
        options.Headers["shared"] = "provider";
        var provider = LiquidProviderType.Create(options, capture);
        return (provider, provider.DecisionModel("d1.2"), capture);
    }

    internal static JsonObject RequestBody(OpenAICapture capture) =>
        (JsonObject)JsonNode.Parse(capture.Body)!;

    internal static DecisionModelFilePart Data(string mediaType, byte[] bytes)
    {
        return new DecisionModelFilePart(
            mediaType,
            new LanguageModelFilePart("data", mediaType) { Bytes = bytes }
        );
    }

    internal static DecisionModelFilePart Base64(string mediaType, string base64)
    {
        return new DecisionModelFilePart(
            mediaType,
            new LanguageModelFilePart("data", mediaType) { Base64 = base64 }
        );
    }

    internal static DecisionModelFilePart Url(string mediaType, string url)
    {
        return new DecisionModelFilePart(
            mediaType,
            new LanguageModelFilePart("url", mediaType) { Url = new Uri(url) }
        );
    }

    internal static DecisionModelFilePart Reference(string mediaType, string provider, string id)
    {
        return new DecisionModelFilePart(
            mediaType,
            new LanguageModelFilePart("reference", mediaType)
            {
                Reference = new Dictionary<string, string> { [provider] = id },
            }
        );
    }

    private static JsonObject Load(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Liquid", name);
        return (JsonObject)JsonNode.Parse(File.ReadAllText(path))!;
    }
}
