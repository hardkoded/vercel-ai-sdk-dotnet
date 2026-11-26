// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Middleware.AddToolInputExamplesMiddleware.Support;
using Examples = global::Vercel.AI.AddToolInputExamplesMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.AddToolInputExamplesMiddleware;

/// <summary>Port of <c>add-tool-input-examples-middleware.test.ts</c> &gt; <c>addToolInputExamplesMiddleware &gt; examplesPrefix</c>.</summary>
public sealed class ExamplesPrefixTests
{
    private const string Id = FileId + "examplesPrefix::";

    [Fact]
    [UpstreamTest(
        Id + "should use provided examplesPrefix as header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_provided_examplesPrefix_as_header()
    {
        var result = await Transform(
            new Examples(prefix: "Here are some example inputs:"),
            new[]
            {
                Tool(
                    "weather",
                    "Get the weather",
                    LocationSchema,
                    Example("{\"location\":\"Paris\"}")
                ),
            }
        );

        Assert.Equal(
            "Get the weather\n\nHere are some example inputs:\n{\"location\":\"Paris\"}",
            result.Tools![0].Description
        );
    }
}
