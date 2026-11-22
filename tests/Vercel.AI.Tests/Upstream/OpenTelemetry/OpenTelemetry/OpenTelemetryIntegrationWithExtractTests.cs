// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.Operations.Extract;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Operations.Extract.ExtractSupport;
using ExtractOperation = Vercel.AI.Operations.Extract;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry integration with extract</c>.</summary>
[Collection("DownloadFetch")]
public sealed class OpenTelemetryIntegrationWithExtractTests
{
    private const string UpstreamCommit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e";
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry integration with extract::";

    private static Dictionary<string, ExtractionField> InvoiceFields() =>
        new() { ["invoiceNumber"] = new ExtractionField("text", "Invoice number") };

    private static ExtractionMockModel CreateModel() =>
        new()
        {
            OnExtract = _ =>
                Task.FromResult(
                    new ExtractionModelResult(
                        new Dictionary<string, ExtractionAnswer>
                        {
                            ["invoiceNumber"] = new ExtractionAnswer("text", text: "INV-2041"),
                        },
                        [],
                        new OperationUsage(12, 2),
                        response: new ProviderResponse(
                            id: "test-response-id",
                            modelId: "mock-model-resolved"
                        )
                    )
                ),
        };

    private static TelemetryOptions Integration(
        MockTracer tracer,
        bool? recordInputs = null,
        bool experimentalExtraction = false,
        bool providerMetadata = false
    ) =>
        new()
        {
            RecordInputs = recordInputs,
            Integrations =
            [
                new OtelIntegration(
                    new OtelOptions
                    {
                        Tracer = tracer.Source,
                        ExperimentalExtraction = experimentalExtraction,
                        ProviderMetadata = providerMetadata,
                    }
                ),
            ],
        };

    [Fact]
    [UpstreamTest(Test + "creates extraction spans", Commit = UpstreamCommit)]
    public async Task Creates_extraction_spans()
    {
        using var tracer = new MockTracer();

        await ExtractOperation.ExtractAsync(
            new ExtractRequest
            {
                Model = CreateModel(),
                State = "Invoice INV-2041",
                Fields = InvoiceFields(),
                ReferenceDate = "2026-11-18",
                Telemetry = Integration(tracer, experimentalExtraction: true),
            }
        );

        var spans = tracer.Spans;
        Assert.Equal(2, spans.Count);
        Assert.All(spans, span => Assert.True(span.Ended));
        Assert.All(spans, span => Assert.Equal("extract mock-model-id", span.Name));

        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["ai.extraction.fields"] =
                    "{\"invoiceNumber\":{\"type\":\"text\",\"instructions\":\"Invoice number\"}}",
                ["ai.extraction.referenceDate"] = "2026-11-18",
                ["ai.extraction.state"] = "\"Invoice INV-2041\"",
                ["gen_ai.operation.name"] = "extract",
                ["gen_ai.provider.name"] = "mock-provider",
                ["gen_ai.request.model"] = "mock-model-id",
            },
            spans[0].InitAttributes
        );
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["ai.extraction.answers"] =
                    "{\"invoiceNumber\":{\"type\":\"text\",\"value\":\"INV-2041\"}}",
            },
            spans[0].RuntimeAttributes
        );

        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["ai.extraction.fields"] =
                    "{\"invoiceNumber\":{\"type\":\"text\",\"instructions\":\"Invoice number\"}}",
                ["ai.extraction.referenceDate"] = "2026-11-18",
                ["ai.extraction.state"] = "[{\"type\":\"text\",\"text\":\"Invoice INV-2041\"}]",
                ["gen_ai.operation.name"] = "extract",
                ["gen_ai.provider.name"] = "mock-provider",
                ["gen_ai.request.model"] = "mock-model-id",
            },
            spans[1].InitAttributes
        );
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["ai.extraction.answers"] =
                    "{\"invoiceNumber\":{\"type\":\"text\",\"value\":\"INV-2041\"}}",
                ["gen_ai.response.id"] = "test-response-id",
                ["gen_ai.response.model"] = "mock-model-resolved",
                ["gen_ai.usage.input_tokens"] = 12,
                ["gen_ai.usage.output_tokens"] = 2,
            },
            spans[1].RuntimeAttributes
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "omits extraction content unless experimental_extraction is enabled",
        Commit = UpstreamCommit
    )]
    public async Task Omits_extraction_content_unless_experimental_extraction_is_enabled()
    {
        using var tracer = new MockTracer();

        await ExtractOperation.ExtractAsync(
            new ExtractRequest
            {
                Model = CreateModel(),
                State = "Invoice INV-2041",
                Fields = InvoiceFields(),
                ReferenceDate = "2026-11-18",
                Telemetry = Integration(tracer),
            }
        );

        Assert.Equal(2, tracer.Spans.Count);
        foreach (var span in tracer.Spans)
        {
            Assert.DoesNotContain(span.Attributes.Keys, key =>
                key.StartsWith("ai.extraction.", StringComparison.Ordinal)
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Test + "hides extraction inputs when recordInputs is false",
        Commit = UpstreamCommit
    )]
    public async Task Hides_extraction_inputs_when_recordInputs_is_false()
    {
        using var tracer = new MockTracer();

        await ExtractOperation.ExtractAsync(
            new ExtractRequest
            {
                Model = CreateModel(),
                State = "Invoice INV-2041",
                Fields = InvoiceFields(),
                ReferenceDate = "2026-11-18",
                Telemetry = Integration(tracer, recordInputs: false, experimentalExtraction: true),
            }
        );

        Assert.Equal(2, tracer.Spans.Count);
        foreach (var span in tracer.Spans)
        {
            Assert.False(span.InitAttributes.ContainsKey("ai.extraction.state"));
            Assert.False(span.InitAttributes.ContainsKey("ai.extraction.fields"));
            Assert.False(span.InitAttributes.ContainsKey("ai.extraction.referenceDate"));
            Assert.Equal(
                "{\"invoiceNumber\":{\"type\":\"text\",\"value\":\"INV-2041\"}}",
                span.RuntimeAttributes["ai.extraction.answers"]
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Test + "keeps file urls and provider metadata out of extraction spans",
        Commit = UpstreamCommit
    )]
    public async Task Keeps_file_urls_and_provider_metadata_out_of_extraction_spans()
    {
        using var tracer = new MockTracer();
        const string url = "https://files.example.com/invoice.pdf?X-Amz-Signature=secret";
        var model = new ExtractionMockModel
        {
            OnExtract = _ =>
                Task.FromResult(
                    new ExtractionModelResult(
                        new Dictionary<string, ExtractionAnswer>
                        {
                            ["invoiceNumber"] = new ExtractionAnswer("text", text: "INV-2041"),
                        },
                        [],
                        providerMetadata: System
                            .Text.Json.JsonDocument.Parse(
                                "{\"scaledown\":{\"ocrText\":\"secret document text\"}}"
                            )
                            .RootElement.Clone()
                    )
                ),
        };

        await ExtractOperation.ExtractAsync(
            new ExtractRequest
            {
                Model = model,
                State = DecisionState.From([
                    new DecisionFilePart(
                        "application/pdf",
                        Vercel.AI.Prompt.FilePartInput.FromUrl(new Uri(url))
                    ),
                ]),
                Fields = InvoiceFields(),
                Download = requests =>
                    Task.FromResult<IReadOnlyList<DownloadResult>>([
                        .. requests.Select(_ => new DownloadResult([1, 2, 3], "application/pdf")),
                    ]),
                Telemetry = Integration(
                    tracer,
                    experimentalExtraction: true,
                    providerMetadata: true
                ),
            }
        );

        Assert.Equal(2, tracer.Spans.Count);
        var dump = string.Join(
            "|",
            tracer.Spans.SelectMany(span =>
                span.Attributes.Select(pair => pair.Key + "=" + pair.Value)
            )
        );
        Assert.DoesNotContain("secret", dump);
        Assert.Equal(
            "[{\"type\":\"file\",\"mediaType\":\"application/pdf\",\"data\":\"[redacted]\"}]",
            tracer.Spans[0].InitAttributes["ai.extraction.state"]
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "keeps the url of a failed download out of extraction spans",
        Commit = UpstreamCommit
    )]
    public async Task Keeps_the_url_of_a_failed_download_out_of_extraction_spans()
    {
        using var tracer = new MockTracer();
        const string url = "https://files.example.com/invoice.pdf?X-Amz-Signature=secret";

        var error = await Assert.ThrowsAsync<DownloadError>(() =>
            ExtractOperation.ExtractAsync(
                new ExtractRequest
                {
                    Model = new ExtractionMockModel(),
                    State = DecisionState.From([
                        new DecisionFilePart(
                            "application/pdf",
                            Vercel.AI.Prompt.FilePartInput.FromUrl(new Uri(url))
                        ),
                    ]),
                    Fields = InvoiceFields(),
                    Download = _ =>
                        throw new DownloadError(
                            url,
                            "Download of " + url + " exceeded maximum size of 10 bytes."
                        ),
                    Telemetry = Integration(tracer),
                }
            )
        );

        Assert.Equal(url, error.Url);
        var span = Assert.Single(tracer.Spans);
        Assert.True(span.Ended);
        Assert.Equal(ActivityStatusCode.Error, span.Activity.Status);
        Assert.DoesNotContain("secret", span.Activity.StatusDescription ?? string.Empty);
        Assert.DoesNotContain(
            "secret",
            string.Join(
                "|",
                span.Activity.Events.SelectMany(e => e.Tags.Select(t => t.Value?.ToString()))
            )
        );
    }

    public static TheoryData<string, bool> FailureCases =>
        new() { { "the model call fails", false }, { "a field is refused", true } };

    [Theory]
    [MemberData(nameof(FailureCases))]
    [UpstreamTest(Test + "ends both spans with error status when $name", Commit = UpstreamCommit)]
    public async Task Ends_both_spans_with_error_status(string name, bool refused)
    {
        Assert.NotEmpty(name);
        using var tracer = new MockTracer();
        var message = refused
            ? "Extraction model \"mock-model-id\" from provider \"mock-provider\" refused field \"invoiceNumber\"."
            : "extraction failed";
        var model = new ExtractionMockModel
        {
            OnExtract = _ =>
                refused
                    ? Task.FromResult(
                        new ExtractionModelResult(
                            new Dictionary<string, ExtractionAnswer>
                            {
                                ["invoiceNumber"] = new ExtractionAnswer("refusal"),
                            },
                            []
                        )
                    )
                    : throw new InvalidOperationException("extraction failed"),
        };

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            ExtractOperation.ExtractAsync(
                new ExtractRequest
                {
                    Model = model,
                    State = "Invoice INV-2041",
                    Fields = InvoiceFields(),
                    MaxRetries = 0,
                    Telemetry = Integration(tracer),
                }
            )
        );

        Assert.Contains(message, error.Message);
        Assert.Equal(2, tracer.Spans.Count);
        foreach (var span in tracer.Spans)
        {
            Assert.True(span.Ended);
            Assert.Equal(ActivityStatusCode.Error, span.Activity.Status);
            Assert.Contains(message, span.Activity.StatusDescription);
            Assert.Equal(1, span.ExceptionCount);
        }
    }
}
