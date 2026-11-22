// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Operations.Extract.ExtractSupport;

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>recordInputs / recordOutputs filtering</c>.</summary>
public sealed class RecordInputsRecordOutputsFilteringTests
{
    private const string Prefix =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::recordInputs / recordOutputs filtering::";

    private const string UpstreamCommit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e";

    [Fact]
    [UpstreamTest(
        Prefix + "strips extraction fields and the reference date when recordInputs is false",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Strips_extraction_fields_and_the_reference_date_when_recordInputs_is_false()
    {
        ExtractStartEvent? delivered = null;
        var telemetry = TelemetryDispatcher.Create(
            new TelemetryOptions
            {
                Integrations =
                [
                    new TestTelemetry
                    {
                        OnExtractStart = (item, _) =>
                        {
                            delivered = item;
                            return Task.CompletedTask;
                        },
                    },
                ],
                RecordInputs = false,
            }
        );

        await telemetry.OnExtractStart!(
            new ExtractStartEvent(
                "call-1",
                new Dictionary<string, object?>(),
                "mock-provider",
                "mock-model-id",
                DecisionState.From("secret invoice"),
                Fields(),
                "2026-11-18",
                null,
                null,
                2,
                null,
                OperationJson.Parse("{}")
            ),
            default
        );

        Assert.Equal(string.Empty, delivered!.State!.Text);
        Assert.Empty(delivered.Fields);
        Assert.Null(delivered.ReferenceDate);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "redacts download urls from errors sent to integrations",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Redacts_download_urls_from_errors_sent_to_integrations()
    {
        TelemetryErrorEvent? delivered = null;
        var telemetry = TelemetryDispatcher.Create(
            new TelemetryOptions
            {
                Integrations =
                [
                    new TestTelemetry
                    {
                        OnError = (item, _) =>
                        {
                            delivered = item;
                            return Task.CompletedTask;
                        },
                    },
                ],
            }
        );
        var error = new DownloadError(
            "https://files.example.com/invoice.pdf?X-Amz-Signature=secret",
            "Download failed.",
            413,
            "Payload Too Large"
        );

        await telemetry.OnError!(new TelemetryErrorEvent("call-1", error), default);

        var download = Assert.IsType<DownloadError>(delivered!.Error);
        Assert.Equal("[redacted]", download.Url);
        Assert.Equal(413, download.StatusCode);
        Assert.DoesNotContain("secret", download.Url);
        Assert.DoesNotContain("secret", download.Message);
        Assert.DoesNotContain("secret", download.StackTrace ?? string.Empty);
        Assert.Contains("secret", error.Url);
    }
}
