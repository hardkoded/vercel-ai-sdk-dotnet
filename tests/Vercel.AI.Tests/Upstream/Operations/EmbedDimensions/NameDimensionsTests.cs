// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Operations.EmbedDimensions.EmbedDimensionsSupport;

namespace Vercel.AI.Tests.Upstream.Operations.EmbedDimensions;

/// <summary>Port of <c>embed-dimensions.test.ts</c> &gt; <c>$name dimensions</c>, run for embed and each embedMany chunking mode.</summary>
public sealed class NameDimensionsTests
{
    private const string Prefix = "packages/ai/src/embed/embed-dimensions.test.ts::$name dimensions::";

    public static TheoryData<string> Modes => Cases();

    public static TheoryData<string, double> InvalidModes => InvalidCases();

    [Theory]
    [MemberData(nameof(Modes))]
    [UpstreamTest(Prefix + "forwards dimensions and leaves provider options unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_dimensions_and_leaves_provider_options_unchanged(string name)
    {
        var model = CreateModel(name);
        var providerOptions = JsonSerializer.Deserialize<JsonElement>("{\"test\":{\"dimensions\":2}}");
        var starts = new List<EmbedStartEvent>();
        var telemetryStarts = new List<EmbedStartEvent>();
        var embedStarts = new List<EmbeddingModelCallEvent>();

        await Run(
            name,
            model,
            dimensions: 3,
            providerOptions: providerOptions,
            onStart: e =>
            {
                starts.Add(e);
                return Task.CompletedTask;
            },
            telemetry: new EmbedTelemetry
            {
                IncludeRuntimeContext = new Dictionary<string, bool>(),
                OnStart = e =>
                {
                    telemetryStarts.Add(e);
                    return Task.CompletedTask;
                },
                OnEmbedStart = e =>
                {
                    lock (embedStarts)
                    {
                        embedStarts.Add(e);
                    }

                    return Task.CompletedTask;
                },
            });

        var expectedValues = ExpectedValues(name);
        Assert.Equal(expectedValues.Select(values => string.Join(",", values)).OrderBy(x => x), model.Calls.Select(call => string.Join(",", call.Values)).OrderBy(x => x));
        foreach (var call in model.Calls)
        {
            Assert.Equal(3, call.Dimensions);
            Assert.Equal(providerOptions.GetRawText(), call.ProviderOptions!.Value.GetRawText());
        }

        Assert.Equal(3, Assert.Single(starts).Dimensions);
        Assert.Equal(3, Assert.Single(telemetryStarts).Dimensions);
        Assert.Equal(expectedValues.Count, embedStarts.Count);
        foreach (var e in embedStarts)
        {
            Assert.Equal(3, e.Dimensions);
        }
    }

    [Theory]
    [MemberData(nameof(Modes))]
    [UpstreamTest(Prefix + "leaves dimensions unspecified when omitted", Coverage = UpstreamCoverage.Covered)]
    public async Task Leaves_dimensions_unspecified_when_omitted(string name)
    {
        var model = CreateModel(name);

        await Run(name, model);

        Assert.Equal(ExpectedValues(name).Select(values => string.Join(",", values)).OrderBy(x => x), model.Calls.Select(call => string.Join(",", call.Values)).OrderBy(x => x));
        foreach (var call in model.Calls)
        {
            Assert.Null(call.Dimensions);
        }
    }

    [Theory]
    [MemberData(nameof(InvalidModes))]
    [UpstreamTest(
        Prefix + "rejects invalid dimensions %s before calling the model",
        Coverage = UpstreamCoverage.Partial,
        Note = "EmbedRequest.Dimensions is int?, so 1.5, NaN, Infinity and -Infinity cannot reach embed or embedMany. Those values are rejected by Embed.ValidateEmbeddingDimensions only. 0 and -1 go through the operations.")]
    public async Task Rejects_invalid_dimensions_before_calling_the_model(string name, double dimensions)
    {
        var model = CreateModel(name);

        var direct = Assert.Throws<InvalidArgumentException>(() => Embed.ValidateEmbeddingDimensions(dimensions));
        Assert.Equal("dimensions", direct.Parameter);
        Assert.Equal(dimensions, direct.Value);
        Assert.Contains("dimensions must be a positive integer", direct.Message, StringComparison.Ordinal);

        if (dimensions == Math.Floor(dimensions) && Math.Abs(dimensions) < int.MaxValue)
        {
            var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => Run(name, model, dimensions: (int)dimensions));
            Assert.Equal("dimensions", error.Parameter);
            Assert.Equal(dimensions, error.Value);
        }

        Assert.Empty(model.Calls);
    }

    [Theory]
    [MemberData(nameof(Modes))]
    [UpstreamTest(Prefix + "preserves dimensions when retrying a model call", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_dimensions_when_retrying_a_model_call(string name)
    {
        var model = CreateModel(name);
        model.FailFirstAttempt = true;

        await Run(name, model, dimensions: 3, maxRetries: 1);

        Assert.Equal(ExpectedValues(name).Count + 1, model.Attempts);
        Assert.All(model.Calls, call => Assert.Equal(3, call.Dimensions));
    }
}
