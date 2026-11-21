// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenResponses;

namespace Vercel.AI.Tests.Upstream.OpenResponses.OpenResponsesTools;

// C# has no test-d.ts. The method compiles the use that the upstream type test checks.
// The catalog lists only *.test.ts files, so it carries no UpstreamTest attribute.
public sealed class OpenResponsesToolsTests
{
    // open-responses-tools.test-d.ts > exposes typed caller-executed custom tools
    [Fact]
    public void Exposes_typed_caller_executed_custom_tools()
    {
        var provider = OpenResponsesProvider.Create(
            new OpenResponsesOptions { Name = "test", BaseUrl = "https://example.com/v1" }
        );

        var customTool = provider.Tools.CustomTool(
            new OpenResponsesCustomToolOptions
            {
                Name = "source",
                Description = "Return source text",
                Format = new OpenResponsesCustomToolFormat
                {
                    Type = "grammar",
                    Syntax = "regex",
                    Definition = ".+",
                },
                Execute = (input, _) => Task.FromResult(input.Length.ToString()),
            }
        );

        Assert.Equal("open-responses.custom", customTool.ProviderToolId);
        Assert.NotNull(customTool.ExecuteWithOptions);
    }
}
