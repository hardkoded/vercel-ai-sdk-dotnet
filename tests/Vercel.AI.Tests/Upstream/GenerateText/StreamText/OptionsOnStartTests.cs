// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream.Telemetry;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.TelemetryStreamSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; options.onStart</c>, the telemetry context tests.</summary>
public sealed class OptionsOnStartTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.onStart::";

    [Fact]
    [UpstreamTest(
        Prefix + "should include configured runtimeContext properties in telemetry",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_configured_runtimeContext_properties_in_telemetry()
    {
        var contexts = new List<string>();

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CreateTestModel(),
                    Prompt = "test-input",
                    RuntimeContext = TelemetryContextSupport.RuntimeContext(),
                    Telemetry = new TelemetryOptions
                    {
                        IncludeRuntimeContext = new Dictionary<string, bool>
                        {
                            ["requestId"] = true,
                        },
                        Integrations =
                        [
                            TelemetryContextSupport.RuntimeContextIntegration(contexts),
                        ],
                    },
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        await ConsumeAsync(result);

        Assert.Equal(Enumerable.Repeat("{\"requestId\":\"request-123\"}", 4), contexts);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include configured toolsContext properties in telemetry",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_configured_toolsContext_properties_in_telemetry()
    {
        var contexts = new List<string>();

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CreateTestModel(),
                    Prompt = "test-input",
                    Tools = [TelemetryContextSupport.WeatherTool()],
                    ToolsContext = TelemetryContextSupport.WeatherToolsContext(),
                    Telemetry = new TelemetryOptions
                    {
                        IncludeToolsContext = new Dictionary<
                            string,
                            IReadOnlyDictionary<string, bool>
                        >
                        {
                            ["weather"] = new Dictionary<string, bool> { ["region"] = true },
                        },
                        Integrations = [TelemetryContextSupport.ToolsContextIntegration(contexts)],
                    },
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        await ConsumeAsync(result);

        Assert.Equal(Enumerable.Repeat("{\"weather\":{\"region\":\"eu\"}}", 4), contexts);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should exclude runtimeContext from telemetry by default",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_exclude_runtimeContext_from_telemetry_by_default()
    {
        var contexts = new List<string>();

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CreateTestModel(),
                    Prompt = "test-input",
                    RuntimeContext = TelemetryContextSupport.RuntimeContext(),
                    Telemetry = new TelemetryOptions
                    {
                        Integrations =
                        [
                            TelemetryContextSupport.RuntimeContextIntegration(contexts),
                        ],
                    },
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        await ConsumeAsync(result);

        Assert.Equal(Enumerable.Repeat("{}", 4), contexts);
    }
}
