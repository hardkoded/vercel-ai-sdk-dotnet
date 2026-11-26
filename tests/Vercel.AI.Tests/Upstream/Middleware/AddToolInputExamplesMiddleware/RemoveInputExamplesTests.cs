// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Middleware.AddToolInputExamplesMiddleware.Support;
using Examples = global::Vercel.AI.AddToolInputExamplesMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.AddToolInputExamplesMiddleware;

/// <summary>Port of <c>add-tool-input-examples-middleware.test.ts</c> &gt; <c>addToolInputExamplesMiddleware &gt; removeInputExamples</c>.</summary>
public sealed class RemoveInputExamplesTests
{
    private const string Id = FileId + "removeInputExamples::";

    [Fact]
    [UpstreamTest(
        Id + "should remove inputExamples by default",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_remove_inputExamples_by_default()
    {
        var result = await Transform(
            new Examples(),
            new[]
            {
                Tool(
                    "weather",
                    "Get the weather",
                    LocationSchema,
                    Example("{\"location\":\"NYC\"}")
                ),
            }
        );

        Assert.Null(result.Tools![0].InputExamples);
    }

    [Fact]
    [UpstreamTest(
        Id + "should keep inputExamples when remove is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_inputExamples_when_remove_is_false()
    {
        var result = await Transform(
            new Examples(remove: false),
            new[]
            {
                Tool(
                    "weather",
                    "Get the weather",
                    LocationSchema,
                    Example("{\"location\":\"NYC\"}")
                ),
            }
        );

        var example = Assert.Single(result.Tools![0].InputExamples!);
        Assert.Equal("{\"location\":\"NYC\"}", example.Input.GetRawText());
    }
}
