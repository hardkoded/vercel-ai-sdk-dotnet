// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests.Upstream.Xai.XSearch;

/// <summary>Port of <c>x-search.test.ts</c> &gt; <c>xSearchArgsSchema</c>.</summary>
public sealed class XSearchArgsSchemaTests
{
    private const string Prefix = "packages/xai/src/tool/x-search.test.ts::xSearchArgsSchema::";

    [Theory]
    [InlineData("allowedXHandles")]
    [InlineData("excludedXHandles")]
    [UpstreamTest(Prefix + "accepts 20 %s", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_20(string field)
    {
        var result = JsonParsing.SafeValidateTypes(Handles(field, 20), XaiResponsesTools.XSearchArgsSchema);

        Assert.True(result.Success);
    }

    [Theory]
    [InlineData("allowedXHandles")]
    [InlineData("excludedXHandles")]
    [UpstreamTest(Prefix + "rejects 21 %s", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_21(string field)
    {
        var result = JsonParsing.SafeValidateTypes(Handles(field, 21), XaiResponsesTools.XSearchArgsSchema);

        Assert.False(result.Success);
    }

    private static System.Text.Json.JsonElement Handles(string field, int count)
    {
        var handles = string.Join(",", Enumerable.Range(0, count).Select(i => "\"handle" + i + "\""));
        return UpstreamChat.Json("{\"" + field + "\":[" + handles + "]}");
    }
}
