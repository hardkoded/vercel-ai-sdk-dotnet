// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; warnings</c>.</summary>
[Collection("LogWarnings")]
public sealed class WarningsTests
{
    private const string Id = Prefix + "warnings::";

    [Fact]
    [UpstreamTest(
        Id + "should resolve warnings promise with undefined when no warnings are present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_resolve_warnings_promise_with_undefined_when_no_warnings_are_present()
    {
        var model = CreateTestModel(warnings: []);
        var result = Client().StreamObjectAsync(ObjectOptions(model));

        // Consume the stream to completion
        await Collect(result.PartialObjectStream());

        // Wait for the warnings promise to resolve
        Assert.Empty((await result.Warnings)!);
    }

    [Fact]
    [UpstreamTest(
        Id + "should resolve warnings promise with warnings when warnings are present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_resolve_warnings_promise_with_warnings_when_warnings_are_present()
    {
        CallWarning[] expected =
        [
            new(
                "unsupported",
                "frequency_penalty",
                "This model does not support the frequency_penalty setting."
            ),
            new("other", "Test warning message"),
        ];
        var result = Client().StreamObjectAsync(ObjectOptions(CreateTestModel(warnings: expected)));

        // Consume the stream to completion
        await Collect(result.PartialObjectStream());

        // Wait for the warnings promise to resolve
        Assert.Equal(expected, await result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Id + "should call logWarnings with the correct warnings",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_logWarnings_with_the_correct_warnings()
    {
        CallWarning[] expected =
        [
            new("other", "Setting is not supported"),
            new("unsupported", "temperature", "Temperature parameter not supported"),
        ];
        var logged = await WithLogWarnings(async () =>
        {
            var result = Client()
                .StreamObjectAsync(ObjectOptions(CreateTestModel(warnings: expected)));

            // Consume the stream to completion
            await Collect(result.PartialObjectStream());
        });

        Assert.Equal(2, logged.Count);
        Assert.Equal("mock-provider", logged[1].Provider);
        Assert.Equal("mock-model-id", logged[1].Model);
        Assert.Equal(2, logged[1].Warnings.Count);
        Assert.Equal(
            "Setting is not supported",
            Assert.IsType<OtherWarning>(logged[1].Warnings[0]).Message
        );
        var unsupported = Assert.IsType<UnsupportedWarning>(logged[1].Warnings[1]);
        Assert.Equal("temperature", unsupported.Feature);
        Assert.Equal("Temperature parameter not supported", unsupported.Details);
    }
}
