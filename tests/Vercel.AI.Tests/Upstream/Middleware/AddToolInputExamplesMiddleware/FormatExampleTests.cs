// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using static Vercel.AI.Tests.Upstream.Middleware.AddToolInputExamplesMiddleware.Support;
using Examples = global::Vercel.AI.AddToolInputExamplesMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.AddToolInputExamplesMiddleware;

/// <summary>Port of <c>add-tool-input-examples-middleware.test.ts</c> &gt; <c>addToolInputExamplesMiddleware &gt; formatExample</c>.</summary>
public sealed class FormatExampleTests
{
    private const string Id = FileId + "formatExample::";

    [Fact]
    [UpstreamTest(
        Id + "should use default JSON.stringify format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_default_JSON_stringify_format()
    {
        var result = await Transform(
            new Examples(),
            new[]
            {
                Tool(
                    "search",
                    "Search for items",
                    Json(
                        "{\"type\":\"object\",\"properties\":{\"query\":{\"type\":\"string\"},\"limit\":{\"type\":\"number\"}}}"
                    ),
                    Example("{\"query\":\"test\",\"limit\":10}")
                ),
            }
        );

        Assert.Equal(
            "Search for items\n\nInput Examples:\n{\"query\":\"test\",\"limit\":10}",
            result.Tools![0].Description
        );
    }

    [Fact]
    [UpstreamTest(Id + "should use custom format function", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_custom_format_function()
    {
        var result = await Transform(
            new Examples(
                format: (example, index) =>
                    $"{index + 1}. {JsonSerializer.Serialize(example.Input)}"
            ),
            new[]
            {
                Tool(
                    "weather",
                    "Get the weather",
                    LocationSchema,
                    Example("{\"location\":\"Paris\"}"),
                    Example("{\"location\":\"Tokyo\"}")
                ),
            }
        );

        Assert.Equal(
            "Get the weather\n\nInput Examples:\n1. {\"location\":\"Paris\"}\n2. {\"location\":\"Tokyo\"}",
            result.Tools![0].Description
        );
    }
}
