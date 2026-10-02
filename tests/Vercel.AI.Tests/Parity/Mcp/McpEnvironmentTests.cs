// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mcp;

namespace Vercel.AI.Tests;

public sealed class McpEnvironmentTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/get-environment.test.ts::getEnvironment::should not mutate the original custom environment object",
        Coverage = UpstreamCoverage.Covered)]
    public void Get_environment_does_not_mutate_the_custom_environment()
    {
        var custom = new Dictionary<string, string>
        {
            ["CUSTOM_VAR"] = "custom_value",
        };

        var result = McpEnvironment.GetEnvironment(custom);

        Assert.Equal("custom_value", custom["CUSTOM_VAR"]);
        Assert.Single(custom);
        Assert.NotSame(custom, result);
        Assert.Equal("custom_value", result["CUSTOM_VAR"]);
    }
}
