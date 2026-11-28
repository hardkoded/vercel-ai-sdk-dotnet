// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mcp;

namespace Vercel.AI.Tests.Upstream.Mcp.GetEnvironment;

public sealed class GetEnvironmentTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/get-environment.test.ts::getEnvironment::should not mutate the original custom environment object",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldNotMutateTheOriginalCustomEnvironmentObject()
    {
        var custom = new Dictionary<string, string> { ["CUSTOM_VAR"] = "custom_value" };
        var result = StdioEnvironment.GetEnvironment(custom);
        Assert.Equal("custom_value", custom["CUSTOM_VAR"]);
        Assert.Single(custom);
        Assert.NotSame(custom, result);
        Assert.Equal("custom_value", result["CUSTOM_VAR"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/get-environment.test.ts::getEnvironment::should let a custom value override an inherited default",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldLetACustomValueOverrideAnInheritedDefault()
    {
        var result = StdioEnvironment.GetEnvironment(
            new Dictionary<string, string> { ["PATH"] = "custom-path" },
            windows: false
        );

        Assert.Equal("custom-path", result["PATH"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/get-environment.test.ts::getEnvironment::should match custom environment keys case-insensitively on Windows",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldMatchCustomEnvironmentKeysCaseInsensitivelyOnWindows()
    {
        var result = StdioEnvironment.GetEnvironment(
            new Dictionary<string, string> { ["Path"] = "custom-path" },
            windows: true
        );

        Assert.Equal("custom-path", result["Path"]);
        Assert.False(result.ContainsKey("PATH"));
    }
}
