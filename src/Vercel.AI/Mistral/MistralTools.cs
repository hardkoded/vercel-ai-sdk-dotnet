// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Mistral;

/// <summary>Provider-executed tools of the Mistral Conversations API. Maps to <c>mistralTools</c> in <c>tool/index.ts</c>.</summary>
public static class MistralTools
{
    /// <summary>Searches the web using the Mistral Conversations API. Maps to <c>webSearch</c>.</summary>
    /// <param name="name">The name the tool goes by in the call and its result. It is the key of the tool in upstream's <c>tools</c> record.</param>
    public static LanguageModelTool WebSearch(string name = "web_search")
    {
        return LanguageModelTool.ProviderDefined("mistral.web_search", name, EmptyObject());
    }

    /// <summary>Searches the web and verified news using the Mistral Conversations API. Maps to <c>webSearchPremium</c>.</summary>
    /// <param name="name">The name the tool goes by in the call and its result.</param>
    public static LanguageModelTool WebSearchPremium(string name = "web_search_premium")
    {
        return LanguageModelTool.ProviderDefined("mistral.web_search_premium", name, EmptyObject());
    }

    private static JsonElement EmptyObject()
    {
        using var document = JsonDocument.Parse("{}");
        return document.RootElement.Clone();
    }
}
