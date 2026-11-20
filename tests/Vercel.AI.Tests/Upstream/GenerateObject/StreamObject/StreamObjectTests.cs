// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of the tests of <c>stream-object.test.ts</c> that sit outside any <c>describe</c>.</summary>
[Collection("LogWarnings")]
public sealed class StreamObjectTests
{
    private sealed class ValueResult
    {
        public string Value { get; set; } = "";
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-object/stream-object.test.ts::sanitizes provider request URLs in streamed object results and callbacks",
        Coverage = UpstreamCoverage.Covered,
        Commit = "463b139c991058f53e0c21974f62880ba43a85d0"
    )]
    public async Task Sanitizes_provider_request_URLs_in_streamed_object_results_and_callbacks()
    {
        var stepEnds = new List<GenerateObjectStepEndEvent>();
        var finishes = new List<GenerateObjectEndEvent<ValueResult>>();
        var model = Model(
            [
                new TextStartStreamPart("text"),
                new TextDeltaStreamPart("text", "{\"value\":\"hello\"}"),
                new TextEndStreamPart("text"),
                Finish(),
            ],
            new LanguageModelStreamRequest
            {
                Url = "https://user:password@model.example.test:8443/private?key=secret#fragment",
            }
        );
        var result = Client()
            .StreamObjectAsync(
                new StreamObjectOptions<ValueResult>
                {
                    Model = model,
                    Schema =
                        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}",
                    Prompt = "hello",
                    SerializerOptions = Json,
                    OnStepEnd = (@event, _) =>
                    {
                        stepEnds.Add(@event);
                        return Task.CompletedTask;
                    },
                    OnFinish = (@event, _) =>
                    {
                        finishes.Add(@event);
                        return Task.CompletedTask;
                    },
                }
            );

        await Collect(result.PartialObjectStream());

        Assert.Equal("https://model.example.test:8443", (await result.Request).Url);
        Assert.Equal("https://model.example.test:8443", Assert.Single(stepEnds).Request.Url);
        Assert.Equal("https://model.example.test:8443", Assert.Single(finishes).Request.Url);
        var serialized = JsonSerializer.Serialize(
            new[] { stepEnds[0].Request.Url, finishes[0].Request.Url }
        );
        Assert.DoesNotMatch("password|private|secret|fragment", serialized);
    }
}
