// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.DeepSeek;

/// <summary>DeepSeek server-side tools for the Responses API. Maps to <c>deepseekTools</c>.</summary>
public static class DeepSeekTools
{
    /// <summary>Id of the <see cref="WebSearch"/> tool.</summary>
    public const string WebSearchToolId = "deepseek.web_search";

    /// <summary>
    /// DeepSeek's server-side web search for the Responses API. DeepSeek runs the searches and page reads during
    /// generation and adds the results to the model context. Supported with <c>deepseek-v4-pro</c>.
    /// Successful searches and page reads are billed per operation and reported in <c>usage.raw.server_tool_use</c>
    /// and in the <c>serverToolUse</c> provider metadata. Maps to <c>webSearch</c>.
    /// </summary>
    /// <param name="name">Name the tool goes by in the call and its result.</param>
    public static Tool WebSearch(string name = "web_search")
    {
        using var document = JsonDocument.Parse("{}");
        return Tool.ProviderDefined(WebSearchToolId, name, document.RootElement.Clone());
    }
}
