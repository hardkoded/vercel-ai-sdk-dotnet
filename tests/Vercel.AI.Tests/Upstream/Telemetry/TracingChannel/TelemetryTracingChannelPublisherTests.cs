// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Tests.Upstream.Operations.Decide;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Operations.Decide.DecideSupport;
using static Vercel.AI.Tests.Upstream.Operations.Extract.ExtractSupport;
using static Vercel.AI.Tests.Upstream.Telemetry.TracingChannel.TracingChannelSupport;
using DecideOperation = Vercel.AI.Operations.Decide;
using ExtractOperation = Vercel.AI.Operations.Extract;

namespace Vercel.AI.Tests.Upstream.Telemetry.TracingChannel;

/// <summary>
/// Port of the extract and decide tests of <c>tracing-channel.test.ts</c> &gt; <c>telemetry tracing channel publisher</c>.
/// The other tests of the file need spans the port does not publish yet.
/// </summary>
public sealed class TelemetryTracingChannelPublisherTests
{
    private const string UpstreamCommit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e";

    private static ExtractRequest InvoiceRequest(string callId)
    {
        return new ExtractRequest
        {
            Model = Setup(new ExtractionModelResult(InvoiceNumberAnswer(), [])),
            State = "Invoice INV-2041",
            Fields = InvoiceNumberField(),
            GenerateCallId = () => callId,
        };
    }

    private static DecideRequest RefundRequest(string callId)
    {
        return new DecideRequest
        {
            Model = new DecideModel
            {
                Result = new DecisionModelResult(
                    new Dictionary<string, DecisionAnswer>
                    {
                        ["refund"] = new DecisionAnswer("boolean", probability: 0.9),
                    }
                ),
            },
            State = S("Please refund me"),
            Questions = new Dictionary<string, DecisionQuestion>
            {
                ["refund"] = Q("boolean", "Refund?"),
            },
            GenerateCallId = () => callId,
        };
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::traces the current extract lifecycle sequence",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit,
        Note = "Node reports bindStart. The port reports start, because a DiagnosticListener has no bindStore."
    )]
    public async Task Traces_the_current_extract_lifecycle_sequence()
    {
        var sequence = await CollectEventSequence("trace-extract-sequence", () =>
            ExtractOperation.ExtractAsync(InvoiceRequest("trace-extract-sequence"))
        );

        Assert.Equal(["start experimental_extract", "asyncEnd experimental_extract"], sequence);
    }

    [Theory]
    [InlineData("an omitted allowlist", "omitted")]
    [InlineData("an empty allowlist", "empty")]
    [InlineData("an explicitly excluded field", "excluded")]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::excludes extract runtime context with $name",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Excludes_extract_runtime_context_with_name(string name, string kind)
    {
        Assert.NotEmpty(name);
        var callId = "trace-extract-excluded-" + kind;
        var request = InvoiceRequest(callId);
        request.Telemetry = kind switch
        {
            "omitted" => null,
            "empty" => new TelemetryOptions(),
            _ => new TelemetryOptions
            {
                IncludeRuntimeContext = new Dictionary<string, bool> { ["secret"] = false },
            },
        };
        request.RuntimeContext = new Dictionary<string, object?> { ["secret"] = "hidden" };

        var messages = await CollectStartMessages(callId, () =>
            ExtractOperation.ExtractAsync(request)
        );

        var extract = Assert.IsType<ExtractStartEvent>(
            Assert
                .Single(messages, m => m.Type == TelemetryTracingEventType.ExperimentalExtract)
                .Event
        );
        Assert.Empty(extract.RuntimeContext);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::includes only allowlisted extract runtime context in tracing",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Includes_only_allowlisted_extract_runtime_context_in_tracing()
    {
        var request = InvoiceRequest("trace-extract-allowlisted");
        request.Telemetry = new TelemetryOptions
        {
            IncludeRuntimeContext = new Dictionary<string, bool> { ["requestId"] = true },
        };
        request.RuntimeContext = new Dictionary<string, object?>
        {
            ["requestId"] = "request-1",
            ["secret"] = "hidden",
        };

        var messages = await CollectStartMessages("trace-extract-allowlisted", () =>
            ExtractOperation.ExtractAsync(request)
        );

        var extract = Assert.IsType<ExtractStartEvent>(
            Assert
                .Single(messages, m => m.Type == TelemetryTracingEventType.ExperimentalExtract)
                .Event
        );
        Assert.Equal(
            new Dictionary<string, object?> { ["requestId"] = "request-1" },
            extract.RuntimeContext
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::keeps request headers and provider options out of extract tracing",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Keeps_request_headers_and_provider_options_out_of_extract_tracing()
    {
        var request = InvoiceRequest("trace-extract-headers");
        request.Headers = new Dictionary<string, string>
        {
            ["authorization"] = "Bearer secret-token",
        };
        request.ProviderOptions = OperationJson.Parse(
            "{\"gateway\":{\"byok\":{\"openai\":[{\"apiKey\":\"secret-key\"}]}}}"
        );

        var messages = await CollectStartMessages("trace-extract-headers", () =>
            ExtractOperation.ExtractAsync(request)
        );

        var extract = Assert.IsType<ExtractStartEvent>(
            Assert
                .Single(messages, m => m.Type == TelemetryTracingEventType.ExperimentalExtract)
                .Event
        );
        Assert.Equal("{}", extract.ProviderOptions.GetRawText());
        Assert.Null(extract.Headers);
        Assert.DoesNotContain("secret", System.Text.Json.JsonSerializer.Serialize(extract));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::redacts file urls and failed download urls from extract tracing",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Redacts_file_urls_and_failed_download_urls_from_extract_tracing()
    {
        const string url = "https://files.example.com/invoice.pdf?X-Amz-Signature=secret";
        var request = InvoiceRequest("trace-extract-redact");
        request.Model = Setup();
        request.State = new DecisionStatePart[]
        {
            new DecisionFilePart("application/pdf", FilePartInput.FromUrl(new Uri(url))),
        };
        request.Download = _ =>
            throw new DownloadError(url, $"Download of {url} exceeded maximum size of 10 bytes.");

        DownloadError? error = null;
        var published = await Collect(async () =>
        {
            error = await Assert.ThrowsAsync<DownloadError>(() =>
                ExtractOperation.ExtractAsync(request)
            );
        });

        Assert.Equal(url, error!.Url);
        var mine = published
            .Where(item => item.Message.Type == TelemetryTracingEventType.ExperimentalExtract)
            .Where(item =>
                CallIdOf(item.Message) == "trace-extract-redact"
                || item.Message.Error is DownloadError
            )
            .Where(item => item.Channel is "start" or "error")
            .ToList();
        Assert.Equal(2, mine.Count);
        foreach (var item in mine)
        {
            var text =
                item.Channel == "start"
                    ? System.Text.Json.JsonSerializer.Serialize(item.Message.Event)
                    : System.Text.Json.JsonSerializer.Serialize(item.Message.Error)
                        + $" {item.Message.Error!.Message} {item.Message.Error.StackTrace}";
            Assert.DoesNotContain("secret", text);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::hides extraction inputs in tracing when inputs are not recorded",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Hides_extraction_inputs_in_tracing_when_inputs_are_not_recorded()
    {
        var request = InvoiceRequest("trace-extract-hidden");
        request.ReferenceDate = "2026-11-18";
        request.Telemetry = new TelemetryOptions { RecordInputs = false };

        var messages = await CollectStartMessages("trace-extract-hidden", () =>
            ExtractOperation.ExtractAsync(request)
        );

        var extract = Assert.IsType<ExtractStartEvent>(
            Assert
                .Single(messages, m => m.Type == TelemetryTracingEventType.ExperimentalExtract)
                .Event
        );
        Assert.Equal("", extract.State!.Text);
        Assert.Null(extract.ReferenceDate);
        Assert.Empty(extract.Fields);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::traces the current decide lifecycle sequence",
        Coverage = UpstreamCoverage.Covered,
        Note = "Node reports bindStart. The port reports start, because a DiagnosticListener has no bindStore."
    )]
    public async Task Traces_the_current_decide_lifecycle_sequence()
    {
        var sequence = await CollectEventSequence("trace-decide-sequence", () =>
            DecideOperation.DecideAsync(RefundRequest("trace-decide-sequence"))
        );

        Assert.Equal(["start experimental_decide", "asyncEnd experimental_decide"], sequence);
    }

    [Theory]
    [InlineData("an omitted allowlist", "omitted")]
    [InlineData("an empty allowlist", "empty")]
    [InlineData("an explicitly excluded field", "excluded")]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::excludes decide runtime context with $name",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Excludes_decide_runtime_context_with_name(string name, string kind)
    {
        Assert.NotEmpty(name);
        var callId = "trace-decide-excluded-" + kind;
        var request = RefundRequest(callId);
        request.Telemetry = kind switch
        {
            "omitted" => null,
            "empty" => new TelemetryOptions(),
            _ => new TelemetryOptions
            {
                IncludeRuntimeContext = new Dictionary<string, bool> { ["secret"] = false },
            },
        };
        request.RuntimeContext = new Dictionary<string, object?> { ["secret"] = "hidden" };

        var messages = await CollectStartMessages(callId, () =>
            DecideOperation.DecideAsync(request)
        );

        var decide = Assert.IsType<DecideEvent>(
            Assert
                .Single(messages, m => m.Type == TelemetryTracingEventType.ExperimentalDecide)
                .Event
        );
        Assert.Empty(decide.RuntimeContext);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::includes only allowlisted decide runtime context in tracing",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Includes_only_allowlisted_decide_runtime_context_in_tracing()
    {
        var request = RefundRequest("trace-decide-allowlisted");
        request.Telemetry = new TelemetryOptions
        {
            IncludeRuntimeContext = new Dictionary<string, bool> { ["requestId"] = true },
        };
        request.RuntimeContext = new Dictionary<string, object?>
        {
            ["requestId"] = "request-1",
            ["secret"] = "hidden",
        };

        var messages = await CollectStartMessages("trace-decide-allowlisted", () =>
            DecideOperation.DecideAsync(request)
        );

        var decide = Assert.IsType<DecideEvent>(
            Assert
                .Single(messages, m => m.Type == TelemetryTracingEventType.ExperimentalDecide)
                .Event
        );
        Assert.Equal(
            new Dictionary<string, object?> { ["requestId"] = "request-1" },
            decide.RuntimeContext
        );
    }
}
