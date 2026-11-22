// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Tests.Upstream.Telemetry;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Operations.Extract.ExtractSupport;
using ExtractOperation = Vercel.AI.Operations.Extract;

namespace Vercel.AI.Tests.Upstream.Operations.Extract;

/// <summary>Port of <c>extract.test.ts</c> &gt; <c>telemetry</c>.</summary>
[Collection("DownloadFetch")]
public sealed class TelemetryTests
{
    private const string UpstreamCommit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e";

    [Fact]
    [UpstreamTest(
        "packages/ai/src/extract/extract.test.ts::telemetry::emits operation and model-call lifecycle events",
        Coverage = UpstreamCoverage.Partial,
        Note = "ITelemetry has no generic OnStart, and its OnEnd takes a GenerateTextResult, so the check that the stable onStart and onEnd are not called does not apply.",
        Commit = UpstreamCommit
    )]
    public async Task Emits_operation_and_model_call_lifecycle_events()
    {
        var events = new List<string>();
        var calls = new Dictionary<string, object>();
        Task Record<T>(string name, T item)
            where T : notnull
        {
            events.Add(name);
            calls[name] = item;
            return Task.CompletedTask;
        }

        var fields = Fields();
        await ExtractOperation.ExtractAsync(
            new ExtractRequest
            {
                Model = Setup(
                    new ExtractionModelResult(
                        new Dictionary<string, ExtractionAnswer>(Answers())
                        {
                            ["contacts"] = new ExtractionAnswer("list", textItems: []),
                        },
                        [],
                        new OperationUsage(40, 12)
                    )
                ),
                State = State,
                Fields = fields,
                ReferenceDate = "2026-11-18",
                RuntimeContext = new Dictionary<string, object?>
                {
                    ["requestId"] = "request-1",
                    ["secret"] = "hidden",
                },
                GenerateCallId = () => "test-call-id",
                OnStart = item => Record("extractOnStart", item),
                OnEnd = item => Record("extractOnEnd", item),
                Telemetry = new TelemetryOptions
                {
                    FunctionId = "extract-test",
                    RecordInputs = false,
                    RecordOutputs = true,
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnExtractStart = (item, _) =>
                                Record("experimental_onExtractStart", item),
                            OnExtractionModelCallStart = (item, _) =>
                                Record("experimental_onExtractionModelCallStart", item),
                            OnExtractionModelCallEnd = (item, _) =>
                                Record("experimental_onExtractionModelCallEnd", item),
                            OnExtractEnd = (item, _) => Record("experimental_onExtractEnd", item),
                        },
                    ],
                },
            }
        );

        Assert.Equal(
            [
                "extractOnStart",
                "experimental_onExtractStart",
                "experimental_onExtractionModelCallStart",
                "experimental_onExtractionModelCallEnd",
                "extractOnEnd",
                "experimental_onExtractEnd",
            ],
            events
        );
        var start = (ExtractStartEvent)calls["experimental_onExtractStart"];
        Assert.Equal("test-call-id", start.CallId);
        Assert.Equal("ai.extract", start.OperationId);
        Assert.Equal("mock-provider", start.Provider);
        Assert.Equal("mock-model-id", start.ModelId);
        Assert.Equal(string.Empty, start.State!.Text);
        Assert.Empty(start.Fields);
        Assert.Empty(start.RuntimeContext);
        Assert.Equal(2, start.MaxRetries);
        Assert.False(start.RecordInputs);
        Assert.True(start.RecordOutputs);
        Assert.Equal("extract-test", start.FunctionId);
        Assert.Equal(AiSdkVersion.Version, start.AiVersion);

        var modelStart = (ExtractionModelCallStartEvent)
            calls["experimental_onExtractionModelCallStart"];
        Assert.Equal("test-call-id", modelStart.CallId);
        Assert.Equal("ai.extract.doExtract", modelStart.OperationId);
        Assert.Empty(modelStart.State);
        Assert.Empty(modelStart.Fields);

        var modelEnd = (ExtractionModelCallEndEvent)calls["experimental_onExtractionModelCallEnd"];
        Assert.Equal("test-call-id", modelEnd.CallId);
        Assert.Equal("ai.extract.doExtract", modelEnd.OperationId);
        Assert.Null(modelEnd.Answers["contacts"].TextItems);
        Assert.Equal("INV-2041", modelEnd.Answers["invoiceNumber"].Text);
        Assert.Equal(40, modelEnd.Usage!.InputTokens);
        Assert.Equal(12, modelEnd.Usage.OutputTokens);

        var end = (ExtractEndEvent)calls["experimental_onExtractEnd"];
        Assert.Equal("test-call-id", end.CallId);
        Assert.Equal("ai.extract", end.OperationId);
        Assert.Null(end.Answers["contacts"].TextItems);
        Assert.Equal(40, end.Usage.InputTokens);
        Assert.Equal(12, end.Usage.OutputTokens);
        Assert.Equal(52, end.Usage.TotalTokens);
        Assert.Empty(end.RuntimeContext);

        foreach (var item in new[] { start, end })
        {
            Assert.Empty(item.Fields);
            Assert.Null(item.ReferenceDate);
            Assert.Equal(string.Empty, item.State!.Text);
        }

        foreach (var item in new[] { modelStart, modelEnd })
        {
            Assert.Empty(item.Fields);
            Assert.Null(item.ReferenceDate);
            Assert.Empty(item.State);
        }

        var userStart = (ExtractStartEvent)calls["extractOnStart"];
        Assert.Equal("ai.extract", userStart.OperationId);
        Assert.Equal(State, userStart.State!.Text);
        Assert.Same(fields, userStart.Fields);
        Assert.Equal("2026-11-18", userStart.ReferenceDate);
        Assert.Equal("hidden", userStart.RuntimeContext["secret"]);
        var userEnd = (ExtractEndEvent)calls["extractOnEnd"];
        Assert.Equal("ai.extract", userEnd.OperationId);
        Assert.Equal("hidden", userEnd.RuntimeContext["secret"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/extract/extract.test.ts::telemetry::hides answers from integrations when outputs are not recorded",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Hides_answers_from_integrations_when_outputs_are_not_recorded()
    {
        ExtractionModelCallEndEvent? modelEnd = null;
        ExtractEndEvent? telemetryEnd = null;
        ExtractEndEvent? userEnd = null;
        var fields = Fields();

        await ExtractOperation.ExtractAsync(
            new ExtractRequest
            {
                Model = Setup(),
                State = State,
                Fields = fields,
                OnEnd = item =>
                {
                    userEnd = item;
                    return Task.CompletedTask;
                },
                Telemetry = new TelemetryOptions
                {
                    RecordOutputs = false,
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnExtractionModelCallEnd = (item, _) =>
                            {
                                modelEnd = item;
                                return Task.CompletedTask;
                            },
                            OnExtractEnd = (item, _) =>
                            {
                                telemetryEnd = item;
                                return Task.CompletedTask;
                            },
                        },
                    ],
                },
            }
        );

        Assert.Empty(modelEnd!.Answers);
        Assert.Same(fields, modelEnd.Fields);
        Assert.Empty(telemetryEnd!.Answers);
        Assert.Same(fields, telemetryEnd.Fields);
        Assert.Equal(Answers().Count, userEnd!.Answers.Count);
        Assert.Equal(Canon(Answers()), Canon(userEnd.Answers));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        "packages/ai/src/extract/extract.test.ts::telemetry::keeps request headers and provider options out of integrations with recordInputs %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Keeps_request_headers_and_provider_options_out_of_integrations_with_recordInputs(
        bool recordInputs
    )
    {
        var seen = new List<ExtractStartEvent>();
        var headers = new Dictionary<string, string> { ["authorization"] = "Bearer secret-token" };
        var providerOptions = OperationJson.Parse(
            "{\"gateway\":{\"byok\":{\"openai\":[{\"apiKey\":\"secret-key\"}]}}}"
        );
        ExtractStartEvent? userStart = null;
        Task Collect(ExtractStartEvent item, CancellationToken _)
        {
            seen.Add(item);
            return Task.CompletedTask;
        }

        await ExtractOperation.ExtractAsync(
            new ExtractRequest
            {
                Model = Setup(),
                State = State,
                Fields = Fields(),
                Headers = headers,
                ProviderOptions = providerOptions,
                OnStart = item =>
                {
                    userStart = item;
                    return Task.CompletedTask;
                },
                Telemetry = new TelemetryOptions
                {
                    RecordInputs = recordInputs,
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnExtractStart = Collect,
                            OnExtractEnd = (item, token) => Collect(item, token),
                        },
                    ],
                },
            }
        );

        Assert.Equal(2, seen.Count);
        foreach (var item in seen)
        {
            Assert.Null(item.Headers);
            Assert.Equal("{}", item.ProviderOptions.GetRawText());
            Assert.DoesNotContain("secret", JsonSerializer.Serialize(item.State));
        }

        Assert.Same(headers, userStart!.Headers);
        Assert.Equal(providerOptions.GetRawText(), userStart.ProviderOptions.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/extract/extract.test.ts::telemetry::redacts file urls and references from state sent to integrations",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Redacts_file_urls_and_references_from_state_sent_to_integrations()
    {
        ExtractStartEvent? telemetryStart = null;
        ExtractStartEvent? userStart = null;
        const string url = "https://files.example.com/invoice.pdf?X-Amz-Signature=secret";
        var fileState = DecisionState.From(
            new DecisionStatePart[]
            {
                new DecisionTextPart("Invoice"),
                new DecisionFilePart("application/pdf", FilePartInput.FromUrl(new Uri(url))),
                new DecisionFilePart("application/pdf", FilePartInput.FromString(url)),
                new DecisionFilePart(
                    "application/pdf",
                    FilePartInput.TaggedReference(
                        new Dictionary<string, string> { ["openai"] = "secret-reference" }
                    )
                ),
                new DecisionFilePart(
                    "application/pdf",
                    FilePartInput.FromReference(
                        new Dictionary<string, string> { ["openai"] = "secret-bare-reference" }
                    )
                ),
            }
        );

        await ExtractOperation.ExtractAsync(
            new ExtractRequest
            {
                Model = Setup(new ExtractionModelResult(InvoiceNumberAnswer(), [])),
                State = fileState,
                Fields = InvoiceNumberField(),
                Download = requests =>
                    Task.FromResult<IReadOnlyList<DownloadResult>>(
                        requests
                            .Select(_ => new DownloadResult([1, 2, 3], "application/pdf"))
                            .ToArray()
                    ),
                OnStart = item =>
                {
                    userStart = item;
                    return Task.CompletedTask;
                },
                Telemetry = new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnExtractStart = (item, _) =>
                            {
                                telemetryStart = item;
                                return Task.CompletedTask;
                            },
                        },
                    ],
                },
            }
        );

        var parts = telemetryStart!.State!.Parts!;
        Assert.Equal(5, parts.Count);
        Assert.Equal("Invoice", Assert.IsType<DecisionTextPart>(parts[0]).Text);
        for (var index = 1; index < parts.Count; index++)
        {
            var file = Assert.IsType<DecisionFilePart>(parts[index]);
            Assert.Equal("application/pdf", file.MediaType);
            Assert.Equal("[redacted]", file.Data.TextData);
            Assert.Null(file.Data.Url);
            Assert.Null(file.Data.Reference);
        }

        Assert.Same(fileState, userStart!.State);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/extract/extract.test.ts::telemetry::redacts the url of a failed download from telemetry errors",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Redacts_the_url_of_a_failed_download_from_telemetry_errors()
    {
        Exception? delivered = null;
        const string url = "https://files.example.com/invoice.pdf?X-Amz-Signature=secret";
        var model = Setup();

        var error = await Assert.ThrowsAsync<DownloadError>(() =>
            ExtractOperation.ExtractAsync(
                new ExtractRequest
                {
                    Model = model,
                    State = new DecisionStatePart[]
                    {
                        new DecisionFilePart(
                            "application/pdf",
                            FilePartInput.FromUrl(new Uri(url))
                        ),
                    },
                    Fields = InvoiceNumberField(),
                    Download = _ =>
                        throw new DownloadError(
                            url,
                            "Download of " + url + " exceeded maximum size of 10 bytes."
                        ),
                    Telemetry = new TelemetryOptions
                    {
                        Integrations =
                        [
                            new TestTelemetry
                            {
                                OnError = (item, _) =>
                                {
                                    delivered = item.Error;
                                    return Task.CompletedTask;
                                },
                            },
                        ],
                    },
                }
            )
        );

        Assert.Equal(url, error.Url);
        Assert.Empty(model.Calls);
        var download = Assert.IsType<DownloadError>(delivered);
        Assert.DoesNotContain(
            "secret",
            download.Url + " " + download.Message + " " + download.StackTrace
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/extract/extract.test.ts::telemetry::includes only selected runtime context fields",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Includes_only_selected_runtime_context_fields()
    {
        var seen = new List<IReadOnlyDictionary<string, object?>>();

        await ExtractOperation.ExtractAsync(
            new ExtractRequest
            {
                Model = Setup(),
                State = State,
                Fields = Fields(),
                RuntimeContext = new Dictionary<string, object?>
                {
                    ["requestId"] = "request-1",
                    ["secret"] = "hidden",
                },
                Telemetry = new TelemetryOptions
                {
                    IncludeRuntimeContext = new Dictionary<string, bool> { ["requestId"] = true },
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnExtractStart = (item, _) =>
                            {
                                seen.Add(item.RuntimeContext);
                                return Task.CompletedTask;
                            },
                            OnExtractEnd = (item, _) =>
                            {
                                seen.Add(item.RuntimeContext);
                                return Task.CompletedTask;
                            },
                        },
                    ],
                },
            }
        );

        Assert.Equal(2, seen.Count);
        foreach (var context in seen)
        {
            Assert.Equal(new[] { "requestId" }, context.Keys);
            Assert.Equal("request-1", context["requestId"]);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/extract/extract.test.ts::telemetry::emits an error event when extraction fails",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Emits_an_error_event_when_extraction_fails()
    {
        var failure = new InvalidOperationException("extraction failed");
        string? callId = null;
        Exception? seen = null;
        string? aiVersion = null;
        var ended = 0;
        var model = Setup();
        model.OnExtract = _ => throw failure;

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ExtractOperation.ExtractAsync(
                new ExtractRequest
                {
                    Model = model,
                    State = State,
                    Fields = Fields(),
                    MaxRetries = 0,
                    GenerateCallId = () => "test-call-id",
                    Telemetry = new TelemetryOptions
                    {
                        Integrations =
                        [
                            new TestTelemetry
                            {
                                OnError = (item, _) =>
                                {
                                    callId = item.CallId;
                                    seen = item.Error;
                                    aiVersion = item.AiVersion;
                                    return Task.CompletedTask;
                                },
                                OnExtractEnd = (_, _) =>
                                {
                                    ended++;
                                    return Task.CompletedTask;
                                },
                            },
                        ],
                    },
                }
            )
        );

        Assert.Same(failure, error);
        Assert.Equal("test-call-id", callId);
        Assert.Same(failure, seen);
        Assert.Equal(AiSdkVersion.Version, aiVersion);
        Assert.Equal(0, ended);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/extract/extract.test.ts::telemetry::emits an error event and no end events when a field is refused",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Emits_an_error_event_and_no_end_events_when_a_field_is_refused()
    {
        string? callId = null;
        var ended = 0;
        var answers = new Dictionary<string, ExtractionAnswer>(Answers())
        {
            ["total"] = new ExtractionAnswer("refusal"),
        };

        await Assert.ThrowsAsync<ExtractionRefusalError>(() =>
            ExtractOperation.ExtractAsync(
                new ExtractRequest
                {
                    Model = Setup(new ExtractionModelResult(answers, [])),
                    State = State,
                    Fields = Fields(),
                    GenerateCallId = () => "test-call-id",
                    OnEnd = _ =>
                    {
                        ended++;
                        return Task.CompletedTask;
                    },
                    Telemetry = new TelemetryOptions
                    {
                        Integrations =
                        [
                            new TestTelemetry
                            {
                                OnError = (item, _) =>
                                {
                                    callId = item.CallId;
                                    return Task.CompletedTask;
                                },
                                OnExtractionModelCallEnd = (_, _) =>
                                {
                                    ended++;
                                    return Task.CompletedTask;
                                },
                                OnExtractEnd = (_, _) =>
                                {
                                    ended++;
                                    return Task.CompletedTask;
                                },
                            },
                        ],
                    },
                }
            )
        );

        Assert.Equal("test-call-id", callId);
        Assert.Equal(0, ended);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/extract/extract.test.ts::telemetry::emits no events when a field type is unsupported",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Emits_no_events_when_a_field_type_is_unsupported()
    {
        var events = 0;
        Task Count<T>(T _)
        {
            events++;
            return Task.CompletedTask;
        }

        await Assert.ThrowsAsync<ExtractionUnsupportedFieldTypeError>(() =>
            ExtractOperation.ExtractAsync(
                new ExtractRequest
                {
                    Model = new ExtractionMockModel { SupportedFieldTypes = ["text"] },
                    State = State,
                    Fields = Fields(),
                    OnStart = Count,
                    Telemetry = new TelemetryOptions
                    {
                        Integrations =
                        [
                            new TestTelemetry
                            {
                                OnError = (_, _) => Count(0),
                                OnExtractStart = (item, _) => Count(item),
                            },
                        ],
                    },
                }
            )
        );

        Assert.Equal(0, events);
    }
}
