// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;
using StreamSupport = Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>Builds the generateObject and streamObject calls the dispatcher tests run.</summary>
internal static class GenerateObjectTestSupportFacade
{
    public static AiClient Client() => GenerateObjectTestSupport.Client();

    public static GenerateObjectOptions<GenerateObjectTestSupport.ContentResult> GenerateOptions() =>
        GenerateObjectTestSupport.ObjectOptions(new GenerateObjectTestModel());

    public static StreamObjectOptions<StreamSupport.ContentResult> StreamOptions() =>
        StreamSupport.ObjectOptions(
            StreamSupport.Model(StreamSupport.TextStream("{ \"content\": \"Hello, world!\" }"))
        );

    public static StreamObjectOptions<StreamSupport.ContentResult> FailingStreamOptions(
        Exception error
    ) => StreamSupport.ObjectOptions(StreamSupport.ThrowingModel(error));

    public static async Task CollectAsync(
        StreamObjectOptions<StreamSupport.ContentResult> options
    ) =>
        await StreamSupport.Collect(
            GenerateObjectTestSupport.Client().StreamObjectAsync(options).PartialObjectStream()
        );
}
