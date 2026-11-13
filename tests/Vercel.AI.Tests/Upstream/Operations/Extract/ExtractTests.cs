// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DecisionFixtures;
using static Vercel.AI.Tests.Upstream.Operations.Extract.ExtractSupport;
using ExtractOperation = Vercel.AI.Operations.Extract;

namespace Vercel.AI.Tests.Upstream.Operations.Extract;

/// <summary>Port of the top-level tests of <c>extract.test.ts</c>.</summary>
[Collection("DownloadFetch")]
public sealed class ExtractTests
{
    private const string UpstreamCommit = "8bc7652cdb13fc84722666eb93f45acd0d131eee";

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::extracts every field in one call and preserves metadata", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Extracts_every_field_in_one_call_and_preserves_metadata()
    {
        var warning = OperationWarning.Other("Provider note");
        var timestamp = new DateTime(2026, 11, 11, 12, 0, 0, DateTimeKind.Utc);
        var logged = new List<WarningLogContext>();
        WarningLog.Logger = new Action<WarningLogContext>(logged.Add);
        try
        {
            var model = Setup(new ExtractionModelResult(
                Answers(),
                [warning],
                new OperationUsage(40, 12),
                OperationJson.Parse("{\"test\":{\"passes\":1}}"),
                new ProviderResponse(new Dictionary<string, string> { ["x-request-id"] = "request" }, OperationJson.Parse("{\"raw\":true}"), "response", timestamp, "actual-model")));
            using var abort = new CancellationTokenSource();
            var fields = Fields();

            var result = await ExtractOperation.ExtractAsync(new ExtractRequest
            {
                Model = model,
                State = State,
                Fields = fields,
                ReferenceDate = "2026-11-11",
                MaxOutputTokens = 200,
                Reasoning = "low",
                CancellationToken = abort.Token,
                Headers = new Dictionary<string, string> { ["custom"] = "value" },
                ProviderOptions = OperationJson.Parse("{\"test\":{\"option\":true}}"),
            });

            var call = Assert.Single(model.Calls);
            Assert.Equal(J(new List<object?> { new Dictionary<string, object?> { ["type"] = "text", ["text"] = State } }), J(Canon(call.State)));
            Assert.Same(fields, call.Fields);
            Assert.Equal("2026-11-11", call.ReferenceDate);
            Assert.Equal((double?)200, call.MaxOutputTokens);
            Assert.Equal("low", call.Reasoning);
            Assert.Equal(abort.Token, call.CancellationToken);
            Assert.Equal(new[] { "custom", "user-agent" }, call.Headers.Keys.OrderBy(key => key, StringComparer.Ordinal));
            Assert.Equal("value", call.Headers["custom"]);
            Assert.Contains("ai/", call.Headers["user-agent"]);
            Assert.Equal("{\"test\":{\"option\":true}}", call.ProviderOptions.GetRawText());
            Assert.Equal("INV-2041", State.Substring(8, 16 - 8));
            Assert.Equal("Ana", State.Substring(70, 73 - 70));

            Assert.Equal(
                Sorted("""
                {
                  "callWithin": { "type": "duration", "value": 2 },
                  "contacts": {
                    "items": [
                      { "confidence": 0.9, "source": { "end": 73, "part": 0, "start": 70, "type": "text" } },
                      { "confidence": 0.8 }
                    ],
                    "type": "list",
                    "value": ["Ana", "Bo"]
                  },
                  "dueAt": { "type": "datetime", "value": "2027-03-03T17:30" },
                  "dueDate": { "type": "date", "value": "2027-03-03" },
                  "dueTime": { "type": "time", "value": "17:30" },
                  "invoiceNumber": {
                    "confidence": 0.98,
                    "source": { "end": 16, "part": 0, "start": 8, "type": "text" },
                    "type": "text",
                    "value": "INV-2041"
                  },
                  "total": { "pNull": 0.01, "type": "number", "value": 1250 }
                }
                """),
                ExtractSupport.Canon(result.Answers));
            Assert.Equal("{\"test\":{\"passes\":1}}", result.ProviderMetadata!.Value.GetRawText());
            Assert.Equal("{\"raw\":true}", result.Response.Body!.Value.GetRawText());
            Assert.Equal(new Dictionary<string, string> { ["x-request-id"] = "request" }, result.Response.Headers);
            Assert.Equal("response", result.Response.Id);
            Assert.Equal("actual-model", result.Response.ModelId);
            Assert.Equal((DateTime?)timestamp, result.Response.Timestamp);
            Assert.Equal((int?)40, result.Usage.InputTokens);
            Assert.Equal((int?)12, result.Usage.OutputTokens);
            Assert.Equal((int?)52, result.Usage.TotalTokens);
            var resultWarning = Assert.Single(result.Warnings);
            Assert.Equal("other", resultWarning.Type);
            Assert.Equal("Provider note", resultWarning.Message);

            var log = Assert.Single(logged);
            Assert.Same(warning, Assert.Single(log.Warnings));
            Assert.Equal(model.Provider, log.Provider);
            Assert.Equal(model.ModelId, log.Model);
        }
        finally
        {
            WarningLog.Logger = null;
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::accepts null values for fields the state does not contain", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Accepts_null_values_for_fields_the_state_does_not_contain()
    {
        var fields = Fields();
        var empty = fields.ToDictionary(pair => pair.Key, pair => new ExtractionAnswer(pair.Value.Type));
        var result = await ExtractOperation.ExtractAsync(new ExtractRequest
        {
            Model = Setup(new ExtractionModelResult(empty, [])),
            State = string.Empty,
            Fields = fields,
        });
        Assert.Equal(ExtractSupport.Canon(empty), ExtractSupport.Canon(result.Answers));
        Assert.Null(result.Usage.InputTokens);
        Assert.Null(result.Usage.OutputTokens);
        Assert.Null(result.Usage.TotalTokens);
        Assert.NotNull(result.Response.Timestamp);
        Assert.Equal("mock-model-id", result.Response.ModelId);
        Assert.Null(result.Response.Id);
        Assert.Null(result.Response.Headers);
        Assert.Null(result.Response.Body);
        Assert.Null(result.Response.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::rejects unsupported types before any provider I/O", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Rejects_unsupported_types_before_any_provider_IO()
    {
        var model = new ExtractionMockModel { SupportedFieldTypes = ["text"] };
        var error = await Assert.ThrowsAsync<ExtractionUnsupportedFieldTypeError>(() => ExtractOperation.ExtractAsync(new ExtractRequest { Model = model, State = State, Fields = Fields() }));
        Assert.Equal("total", error.FieldId);
        Assert.Equal("number", error.FieldType);
        Assert.Equal(model.Provider, error.Provider);
        Assert.Equal(model.ModelId, error.ModelId);
        Assert.Empty(model.Calls);
    }

    [Theory]
    [InlineData("total")]
    [InlineData("total", "contacts")]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::rejects refused fields with a refusal error without retrying (%s)", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Rejects_refused_fields_with_a_refusal_error_without_retrying(params string[] refused)
    {
        var answers = Answers();
        foreach (var id in refused)
        {
            answers[id] = new ExtractionAnswer("refusal");
        }

        var model = Setup(new ExtractionModelResult(answers, []));
        var error = await Assert.ThrowsAnyAsync<Exception>(() => ExtractOperation.ExtractAsync(new ExtractRequest { Model = model, State = State, Fields = Fields() }));
        Assert.True(ExtractionRefusalError.IsInstance(error));
        var refusal = Assert.IsType<ExtractionRefusalError>(error);
        Assert.Equal(refused, refusal.FieldIds);
        Assert.Equal(model.Provider, refusal.Provider);
        Assert.Equal(model.ModelId, refusal.ModelId);
        Assert.Single(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::rejects an unsupported model version", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Rejects_an_unsupported_model_version()
    {
        var model = Setup();
        model.SpecificationVersion = "v99";
        await Assert.ThrowsAsync<UnsupportedModelVersionException>(() => ExtractOperation.ExtractAsync(new ExtractRequest { Model = model, State = State, Fields = Fields() }));
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::retries transient errors with the configured retry limit", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Retries_transient_errors_with_the_configured_retry_limit()
    {
        var model = Setup();
        var resolved = model.OnExtract;
        var rejected = false;
        model.OnExtract = call =>
        {
            if (rejected)
            {
                return resolved(call);
            }

            rejected = true;
            return Task.FromException<ExtractionModelResult>(new ApiCallError(
                "Rate limited",
                "https://example.com",
                new Dictionary<string, object?>(),
                statusCode: 429,
                responseHeaders: new Dictionary<string, string> { ["retry-after-ms"] = "0" }));
        };
        var result = await ExtractOperation.ExtractAsync(new ExtractRequest { Model = model, State = State, Fields = Fields(), MaxRetries = 1 });
        Assert.NotNull(result);
        Assert.Equal(2, model.Calls.Count);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::honors maxRetries: 0", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Honors_maxRetries_0()
    {
        var model = Setup();
        var error = new ApiCallError("Overloaded", "https://example.com", new Dictionary<string, object?>(), statusCode: 529);
        model.OnExtract = _ => Task.FromException<ExtractionModelResult>(error);
        var thrown = await Assert.ThrowsAsync<ApiCallError>(() => ExtractOperation.ExtractAsync(new ExtractRequest { Model = model, State = State, Fields = Fields(), MaxRetries = 0 }));
        Assert.Same(error, thrown);
        Assert.Single(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::does not call the provider when already aborted", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Does_not_call_the_provider_when_already_aborted()
    {
        var model = Setup();
        using var controller = new CancellationTokenSource();
        var reason = new InvalidOperationException("Cancelled");
        controller.Cancel();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => ExtractOperation.ExtractAsync(new ExtractRequest
        {
            Model = model,
            State = State,
            Fields = Fields(),
            CancellationToken = controller.Token,
            AbortReason = reason,
        }));
        Assert.Same(reason, error);
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::does not return a result after cancellation during a call", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Does_not_return_a_result_after_cancellation_during_a_call()
    {
        var model = Setup();
        using var controller = new CancellationTokenSource();
        var reason = new InvalidOperationException("Cancelled");
        model.OnExtract = _ =>
        {
            controller.Cancel();
            return Task.FromResult(new ExtractionModelResult(Answers(), []));
        };
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => ExtractOperation.ExtractAsync(new ExtractRequest
        {
            Model = model,
            State = State,
            Fields = Fields(),
            CancellationToken = controller.Token,
            AbortReason = reason,
        }));
        Assert.Same(reason, error);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::sends JSON state as a JSON part", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Sends_JSON_state_as_a_JSON_part()
    {
        var model = Setup(new ExtractionModelResult(InvoiceNumberAnswer(), []));
        await ExtractOperation.ExtractAsync(new ExtractRequest
        {
            Model = model,
            State = S(new Dictionary<string, object?> { ["invoice"] = "INV-2041" }),
            Fields = InvoiceNumberField(),
        });
        Assert.Equal(
            J(new List<object?> { new Dictionary<string, object?> { ["type"] = "json", ["value"] = new Dictionary<string, object?> { ["invoice"] = "INV-2041" } } }),
            J(Canon(model.Calls[0].State)));
    }

    public static TheoryData<string> RemoteFileData()
    {
        return new TheoryData<string> { "string", "URL", "url part" };
    }

    [Theory]
    [MemberData(nameof(RemoteFileData))]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::rejects remote file state without a download hook (%s)", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Rejects_remote_file_state_without_a_download_hook(string form)
    {
        const string url = "https://attacker.example/invoice.pdf";
        var data = form switch
        {
            "string" => FilePartInput.FromString(url),
            "URL" => FilePartInput.FromUrl(new Uri(url)),
            _ => FilePartInput.TaggedUrl(new Uri(url)),
        };
        var model = Setup(new ExtractionModelResult(InvoiceNumberAnswer(), []));
        var fetches = 0;
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (_, _) =>
            {
                fetches++;
                return Task.FromResult(DownloadResponse.Text("invoice", "application/pdf"));
            };
            var error = await Assert.ThrowsAsync<DownloadError>(() => ExtractOperation.ExtractAsync(new ExtractRequest
            {
                Model = model,
                Fields = InvoiceNumberField(),
                State = new DecisionStatePart[] { new DecisionFilePart("application/pdf", data) },
            }));
            Assert.Equal("AI_DownloadError", error.ErrorName);

            // The option upstream calls experimental_download is ExtractRequest.Download here.
            Assert.Contains("requires Download", error.Message);
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Equal(0, fetches);
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::stops before provider I/O when a file download fails", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Stops_before_provider_IO_when_a_file_download_fails()
    {
        var model = Setup();
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (_, _) => Task.FromResult(new DownloadResponse(404, "Not Found", null, null));
            var error = await Assert.ThrowsAsync<DownloadError>(() => ExtractOperation.ExtractAsync(new ExtractRequest
            {
                Model = model,
                Fields = Fields(),
                Download = Download.CreateDownload(),
                State = new DecisionStatePart[] { new DecisionFilePart("application/pdf", FilePartInput.FromUrl(new Uri("https://example.com/invoice.pdf"))) },
            }));
            Assert.Equal("AI_DownloadError", error.ErrorName);
            Assert.Equal((int?)404, error.StatusCode);
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::uses the configured download transport and forwards cancellation", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Uses_the_configured_download_transport_and_forwards_cancellation()
    {
        var model = Setup(new ExtractionModelResult(InvoiceNumberAnswer(), []));
        var bytes = Encoding.UTF8.GetBytes("Invoice INV-2041");
        var fetched = new List<(string Url, DownloadRequest Request)>();
        DownloadFetch fetch = (url, request) =>
        {
            fetched.Add((url, request));
            return Task.FromResult(DownloadResponse.Bytes(bytes, "application/pdf"));
        };
        var globalFetches = 0;
        using var abort = new CancellationTokenSource();
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (_, _) =>
            {
                globalFetches++;
                return Task.FromResult(new DownloadResponse(500, "Unexpected", null, null));
            };
            await ExtractOperation.ExtractAsync(new ExtractRequest
            {
                Model = model,
                Fields = InvoiceNumberField(),
                State = new DecisionStatePart[]
                {
                    new DecisionTextPart("Read the invoice."),
                    new DecisionFilePart("application/pdf", FilePartInput.FromUrl(new Uri("https://example.com/invoice.pdf"))),
                },
                CancellationToken = abort.Token,
                Download = Download.CreateDownload(fetch: fetch),
            });
        }
        finally
        {
            Download.Fetch = previous;
        }

        var (fetchedUrl, request) = Assert.Single(fetched);
        Assert.Equal("https://example.com/invoice.pdf", fetchedUrl);
        Assert.Equal(abort.Token, request.AbortSignal);
        Assert.Equal("manual", request.Redirect);
        Assert.Equal(0, globalFetches);
        Assert.Equal(
            J(new List<object?>
            {
                new Dictionary<string, object?> { ["type"] = "text", ["text"] = "Read the invoice." },
                new Dictionary<string, object?>
                {
                    ["type"] = "file",
                    ["mediaType"] = "application/pdf",
                    ["data"] = new Dictionary<string, object?> { ["type"] = "data", ["data"] = bytes },
                },
            }),
            J(Canon(model.Calls[0].State)));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/extract/extract.test.ts::stops before provider I/O when the download transport blocks a URL", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Stops_before_provider_IO_when_the_download_transport_blocks_a_URL()
    {
        var model = Setup();
        var reason = new InvalidOperationException("Blocked private DNS address");
        var fetches = 0;
        DownloadFetch fetch = (_, _) =>
        {
            fetches++;
            return Task.FromException<DownloadResponse>(reason);
        };
        var globalFetches = 0;
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (_, _) =>
            {
                globalFetches++;
                return Task.FromResult(new DownloadResponse(500, "Unexpected", null, null));
            };
            var error = await Assert.ThrowsAsync<DownloadError>(() => ExtractOperation.ExtractAsync(new ExtractRequest
            {
                Model = model,
                Fields = Fields(),
                State = new DecisionStatePart[] { new DecisionFilePart("application/pdf", FilePartInput.FromUrl(new Uri("https://attacker.example/invoice.pdf"))) },
                Download = Download.CreateDownload(fetch: fetch),
            }));
            Assert.Equal("AI_DownloadError", error.ErrorName);
            Assert.Same(reason, error.Cause);
        }
        finally
        {
            Download.Fetch = previous;
        }

        Assert.Equal(1, fetches);
        Assert.Equal(0, globalFetches);
        Assert.Empty(model.Calls);
    }

    private static Dictionary<string, ExtractionAnswer> InvoiceNumberAnswer()
    {
        return new Dictionary<string, ExtractionAnswer> { ["invoiceNumber"] = new ExtractionAnswer("text", text: "INV-2041") };
    }

    private static Dictionary<string, ExtractionField> InvoiceNumberField()
    {
        return new Dictionary<string, ExtractionField> { ["invoiceNumber"] = Fields()["invoiceNumber"] };
    }
}
