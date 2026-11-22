// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject.GenerateObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of the tests of <c>generate-object.test.ts</c> that sit outside any <c>describe</c>.</summary>
[Collection("LogWarnings")]
public sealed class GenerateObjectTests
{
    private sealed class ValueResult
    {
        public string Value { get; set; } = "";
    }

    private sealed class RequestUrlModel : ILanguageModel
    {
        public string SpecificationVersion => "V4";

        public string Provider => "mock-provider";

        public string ModelId => "mock-model-id";

        public IReadOnlyDictionary<string, string> SupportedUrls =>
            new Dictionary<string, string>();

        public Task<LanguageModelGenerateResult> DoGenerateAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new LanguageModelGenerateResult(
                    new GeneratedContent[] { new GeneratedText("{\"value\":\"hello\"}") },
                    FinishReason.Stop,
                    new LanguageModelUsage(10, 20, 30),
                    request: new LanguageModelStreamRequest
                    {
                        Url =
                            "https://user:password@model.example.test:8443/private?key=secret#fragment",
                    }
                )
            );

        public Task<LanguageModelStreamResult> DoStreamAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-object/generate-object.test.ts::sanitizes provider request URLs in object results and callbacks",
        Coverage = UpstreamCoverage.Covered,
        Commit = "463b139c991058f53e0c21974f62880ba43a85d0"
    )]
    public async Task Sanitizes_provider_request_URLs_in_object_results_and_callbacks()
    {
        var stepEnds = new List<GenerateObjectStepEndEvent>();
        var finishes = new List<GenerateObjectEndEvent<ValueResult>>();
        var result = await Client()
            .GenerateObjectAsync(
                new GenerateObjectOptions<ValueResult>
                {
                    Model = new RequestUrlModel(),
                    Schema =
                        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}",
                    Prompt = "hello",
                    SerializerOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    },
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

        Assert.Equal("https://model.example.test:8443", result.Request.Url);
        Assert.Equal("https://model.example.test:8443", Assert.Single(stepEnds).Request.Url);
        Assert.Equal("https://model.example.test:8443", Assert.Single(finishes).Request.Url);
        var serialized = JsonSerializer.Serialize(
            new[] { result.Request.Url, stepEnds[0].Request.Url, finishes[0].Request.Url }
        );
        Assert.DoesNotMatch("password|private|secret|fragment", serialized);
    }
}
