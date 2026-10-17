// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Perplexity;
using static Vercel.AI.Tests.DecisionFixtures;

namespace Vercel.AI.Tests.Upstream.Perplexity.ConvertToPerplexityDecisionState;

/// <summary>Fixtures shared by the <c>convert-to-perplexity-decision-state.test.ts</c> port.</summary>
internal static class ConvertToPerplexityDecisionStateSupport
{
    internal const string Image = """{ "type": "image_url", "image_url": { "url": "data:image/png;base64,iVBORw==" } }""";

    internal static JsonNode? Convert(params object?[] parts)
    {
        return PerplexityDecisionState.Convert(P(parts));
    }

    internal static Dictionary<string, object?> Text(string text)
    {
        return new Dictionary<string, object?> { ["type"] = "text", ["text"] = text };
    }

    internal static Dictionary<string, object?> Json(object? value)
    {
        return new Dictionary<string, object?> { ["type"] = "json", ["value"] = value };
    }

    internal static Dictionary<string, object?> File(string mediaType, object data)
    {
        return new Dictionary<string, object?>
        {
            ["type"] = "file",
            ["mediaType"] = mediaType,
            ["data"] = new Dictionary<string, object?> { ["type"] = "data", ["data"] = data },
        };
    }
}
