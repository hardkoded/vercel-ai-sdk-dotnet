// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>createTelemetryDispatcher</c>. The port dispatches <c>onLanguageModelCallStart</c> where upstream's test uses <c>onStart</c>, which the port has not ported.</summary>
public sealed class CreateTelemetryDispatcherTests
{
    private const string Prefix =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::createTelemetryDispatcher::";

    [Fact]
    [UpstreamTest(
        Prefix + "adds the emitting SDK version without modifying the original event",
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Adds_the_emitting_SDK_version_without_modifying_the_original_event()
    {
        LanguageModelCallStartEvent? delivered = null;
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            new TelemetryOptions
            {
                Integrations =
                [
                    new TestTelemetry
                    {
                        OnLanguageModelCallStart = (e, _) =>
                        {
                            delivered = e;
                            return Task.CompletedTask;
                        },
                    },
                ],
            }
        );
        var original = new LanguageModelCallStartEvent { AiVersion = "other-version" };

        await telemetry.OnLanguageModelCallStart!(original, CancellationToken.None);

        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, delivered!.AiVersion);
        Assert.Null(delivered.RecordInputs);
        Assert.Null(delivered.RecordOutputs);
        Assert.Null(delivered.FunctionId);
        Assert.Equal("other-version", original.AiVersion);
    }
}
