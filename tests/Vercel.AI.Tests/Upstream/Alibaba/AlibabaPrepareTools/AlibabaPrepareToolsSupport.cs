// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaPrepareTools;

/// <summary>Helpers shared by the <c>alibaba-prepare-tools.test.ts</c> ports.</summary>
internal static class AlibabaPrepareToolsSupport
{
    internal const string TestFile = "packages/alibaba/src/alibaba-prepare-tools.test.ts";

    /// <summary>A function tool like <c>{ type: 'function', name, description, inputSchema }</c>.</summary>
    internal static LanguageModelTool Function(string name, string? description, string inputSchema = """{"type":"object","properties":{}}""", bool? strict = null)
    {
        return new LanguageModelTool(name, description, UpstreamChat.Json(inputSchema), strict);
    }
}
