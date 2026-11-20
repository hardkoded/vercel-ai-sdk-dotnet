// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of the <c>stream-object.test.ts</c> tests that sit outside any <c>describe</c>.</summary>
[Collection("LogWarnings")]
public sealed class StreamObjectTests
{
    private const string Id = "packages/ai/src/generate-object/stream-object.test.ts::";

    [Fact]
    [UpstreamTest(
        Id + "sanitizes provider request URLs in streamed object results and callbacks",
        Commit = "f3d7a10df6fcf56577d566f13197d5d4a8e7c3f6",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sanitizes_provider_request_URLs_in_streamed_object_results_and_callbacks()
    {
        const string origin = "https://model.example.test:8443";
        var stepEnds = new List<GenerateObjectStepEndEvent>();
        var finishes = new List<GenerateObjectEndEvent<ContentResult>>();
        var model = Model(
            TextStream("{\"content\":\"hello\"}"),
            new LanguageModelStreamRequest
            {
                Url = "https://user:password@model.example.test:8443/private?key=secret#fragment",
            }
        );
        var options = ObjectOptions(model);
        options.OnStepEnd = (e, _) =>
        {
            stepEnds.Add(e);
            return Task.CompletedTask;
        };
        options.OnFinish = (e, _) =>
        {
            finishes.Add(e);
            return Task.CompletedTask;
        };

        var result = Client().StreamObjectAsync(options);
        await Collect(result.PartialObjectStream());

        Assert.Equal(origin, (await result.Request).Url);
        Assert.Equal(origin, Assert.Single(stepEnds).Request.Url);
        Assert.Equal(origin, Assert.Single(finishes).Request.Url);
        var serialized = JsonSerializer.Serialize(
            new[] { (await result.Request).Url, stepEnds[0].Request.Url, finishes[0].Request.Url }
        );
        Assert.DoesNotMatch("password|private|secret|fragment", serialized);
    }
}
