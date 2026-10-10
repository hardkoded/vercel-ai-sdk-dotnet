// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Operations.EmbedDimensions;

/// <summary>Shared setup for the <c>$name dimensions</c> cases of <c>embed-dimensions.test.ts</c>.</summary>
internal static class EmbedDimensionsSupport
{
    public static readonly string[] Names =
    {
        "embed",
        "embedMany without chunking",
        "embedMany with sequential chunks",
        "embedMany with parallel chunks",
        "embedMany with input byte chunks",
    };

    public static readonly double[] InvalidDimensions =
    {
        0,
        -1,
        1.5,
        double.NaN,
        double.PositiveInfinity,
        double.NegativeInfinity,
    };

    public static TheoryData<string> Cases()
    {
        var data = new TheoryData<string>();
        foreach (var name in Names)
        {
            data.Add(name);
        }

        return data;
    }

    public static TheoryData<string, double> InvalidCases()
    {
        var data = new TheoryData<string, double>();
        foreach (var name in Names)
        {
            foreach (var dimensions in InvalidDimensions)
            {
                data.Add(name, dimensions);
            }
        }

        return data;
    }

    public static IReadOnlyList<string[]> ExpectedValues(string name)
    {
        return name switch
        {
            "embed" => new[] { new[] { "one" } },
            "embedMany without chunking" => new[] { new[] { "one", "two", "three" } },
            _ => new[] { new[] { "one", "two" }, new[] { "three" } },
        };
    }

    public static DimensionsModel CreateModel(string name)
    {
        return new DimensionsModel
        {
            MaxEmbeddingsPerCall = name.EndsWith("chunks", StringComparison.Ordinal) && !name.EndsWith("byte chunks", StringComparison.Ordinal) ? 2 : null,
            MaxInputBytesPerCall = name == "embedMany with input byte chunks" ? 6 : null,
            SupportsParallelCalls = name == "embedMany with parallel chunks",
        };
    }

    public static Task Run(string name, DimensionsModel model, int? dimensions = null, JsonElement? providerOptions = null, Func<EmbedStartEvent, Task>? onStart = null, EmbedTelemetry? telemetry = null, int? maxRetries = null)
    {
        if (name == "embed")
        {
            return Embed.EmbedAsync(new EmbedRequest { Model = model, Value = "one", Dimensions = dimensions, ProviderOptions = providerOptions, OnStart = onStart, Telemetry = telemetry, MaxRetries = maxRetries });
        }

        return EmbedMany.EmbedManyAsync(new EmbedManyRequest { Model = model, Values = new[] { "one", "two", "three" }, Dimensions = dimensions, ProviderOptions = providerOptions, OnStart = onStart, Telemetry = telemetry, MaxRetries = maxRetries });
    }

    internal sealed class DimensionsModel : IEmbeddingCaller
    {
        private int _attempts;

        public string Provider => "mock-provider";

        public string ModelId => "mock-model-id";

        public string SpecificationVersion => "v4";

        public int? MaxEmbeddingsPerCall { get; set; }

        public double? MaxInputBytesPerCall { get; set; }

        public bool SupportsParallelCalls { get; set; }

        public int Attempts => _attempts;

        /// <summary>When set, the first attempt fails with a retryable 429.</summary>
        public bool FailFirstAttempt { get; set; }

        public List<EmbeddingModelCall> Calls { get; } = new();

        public Task<JsonElement?> TransformProviderOptionsAsync(JsonElement? providerOptions, IReadOnlyList<string> values, int startIndex, int endIndex, CancellationToken cancellationToken)
        {
            return Task.FromResult(providerOptions);
        }

        public Task<EmbeddingModelResponse> DoEmbedAsync(EmbeddingModelCall call, CancellationToken cancellationToken)
        {
            lock (Calls)
            {
                Calls.Add(call);
            }

            if (FailFirstAttempt && Interlocked.Increment(ref _attempts) == 1)
            {
                throw new RetryableCallException("Retry this request", 429, new Dictionary<string, string> { ["retry-after-ms"] = "0" });
            }

            if (!FailFirstAttempt)
            {
                Interlocked.Increment(ref _attempts);
            }

            return Task.FromResult(new EmbeddingModelResponse(call.Values.Select(_ => new[] { 0.1, 0.2, 0.3 }).ToArray(), warnings: Array.Empty<OperationWarning>()));
        }
    }
}
