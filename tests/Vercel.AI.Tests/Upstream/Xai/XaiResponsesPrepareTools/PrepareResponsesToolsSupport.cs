// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Helpers shared by the <c>xai-responses-prepare-tools.test.ts</c> ports.</summary>
internal static class PrepareResponsesToolsSupport
{
    internal const string TestFile = "packages/xai/src/responses/xai-responses-prepare-tools.test.ts::prepareResponsesTools";

    internal static LanguageModelTool Function(string name, string description, bool? strict = null)
    {
        return new LanguageModelTool(name, description, UpstreamChat.Json("""{"type":"object","properties":{}}"""), strict);
    }

    /// <summary>A provider-defined tool, like <c>{ type: 'provider', id, name, args }</c>.</summary>
    internal static LanguageModelTool ProviderTool(string id, string name, string args = "{}")
    {
        return LanguageModelTool.ProviderDefined(id, name, UpstreamChat.Json(args));
    }

    /// <summary>The <c>type</c> of each prepared tool.</summary>
    internal static string[] Types(XaiResponsesTools.Prepared result)
    {
        return result.Tools!.Select(tool => (string)tool!["type"]!).ToArray();
    }
}
