// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput;

/// <summary>Helpers shared by the <c>convert-to-xai-responses-input.test.ts</c> ports.</summary>
internal static class ConvertToXaiResponsesInputSupport
{
    internal const string TestFile = "packages/xai/src/responses/convert-to-xai-responses-input.test.ts::convertToXaiResponsesInput";

    internal static JsonArray Convert(params ModelMessage[] prompt)
    {
        return Convert(new List<CallWarning>(), prompt);
    }

    internal static JsonArray Convert(List<CallWarning> warnings, params ModelMessage[] prompt)
    {
        return XaiResponsesInput.Convert(prompt, warnings);
    }

    internal static UserModelMessage User(params UserContentPart[] parts)
    {
        return new UserModelMessage(parts);
    }
}
