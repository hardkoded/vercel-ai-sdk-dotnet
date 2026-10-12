// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Helpers shared by the <c>xai-responses-prepare-tools.test.ts</c> ports.</summary>
internal static class PrepareResponsesToolsSupport
{
    internal const string TestFile = "packages/xai/src/responses/xai-responses-prepare-tools.test.ts::prepareResponsesTools";

    internal static LanguageModelTool Function(string name, string description, bool? strict = null)
    {
        return new LanguageModelTool(name, description, UpstreamChat.Json("""{"type":"object","properties":{}}"""), strict);
    }
}
