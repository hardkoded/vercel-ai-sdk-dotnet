// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Alibaba;

namespace Vercel.AI.Tests.Upstream.Alibaba.SupportsJsonSchemaOutput;

/// <summary>Port of <c>supports-json-schema-output.test.ts</c> &gt; <c>supportsJsonSchemaOutput</c>.</summary>
public sealed class SupportsJsonSchemaOutputTests
{
    private const string Prefix =
        "packages/alibaba/src/supports-json-schema-output.test.ts::supportsJsonSchemaOutput::";

    [Theory]
    [InlineData("qwen3.7-plus")]
    [InlineData("qwen3.7-plus-2026-09-01")]
    [InlineData("qwen3.7-flash")]
    [InlineData("qwen3.7-max")]
    [InlineData("qwen3.8-max")]
    [InlineData("qwen3.8-flash")]
    [InlineData("qwen3.8-flash-latest")]
    [UpstreamTest(Prefix + "returns true for %s", Coverage = UpstreamCoverage.Covered)]
    public void Returns_true_for_modelId(string modelId)
    {
        Assert.True(SupportsJsonSchemaOutputChecker.SupportsJsonSchemaOutput(modelId));
    }

    [Theory]
    [InlineData("qwen-plus")]
    [InlineData("qwen3-max")]
    [InlineData("qwen3.6-plus")]
    [InlineData("qwen3.8-plus")]
    [InlineData("qwen3.8-maximal")]
    [InlineData("deepseek-v4-pro")]
    [InlineData("deepseek-v4.1-flash")]
    [InlineData("kimi-k3")]
    [InlineData("glm-5.1")]
    [UpstreamTest(Prefix + "returns false for %s", Coverage = UpstreamCoverage.Covered)]
    public void Returns_false_for_modelId(string modelId)
    {
        Assert.False(SupportsJsonSchemaOutputChecker.SupportsJsonSchemaOutput(modelId));
    }
}
