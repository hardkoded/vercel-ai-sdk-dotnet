// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayExtractionModel.GatewayExtractionModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayExtractionModel;

/// <summary>Port of <c>gateway-extraction-model.test.ts</c> &gt; <c>GatewayExtractionModel &gt; doExtract</c>.</summary>
public sealed class GatewayExtractionModelDoExtractTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-extraction-model.test.ts::GatewayExtractionModel > doExtract::";

    private const string RequestJson =
        "{\"stateParts\":[{\"type\":\"text\",\"text\":\"Invoice INV-2041 from Acme. Total due $1,250.00.\"}],\"fields\":"
        + FieldsJson;

    [Fact]
    [UpstreamTest(
        Prefix + "should post to the /extraction-model endpoint",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_post_to_the_extraction_model_endpoint()
    {
        var (model, capture) = Setup();
        await model.DoExtractAsync(Call(), CancellationToken.None);
        Assert.Equal("POST", capture.Method);
        Assert.Equal("https://api.test.com/extraction-model", capture.Uri);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send every call option in the request body",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_every_call_option_in_the_request_body()
    {
        var (model, capture) = Setup();
        const string providerOptions = """
            { "gateway": { "models": ["openai/gpt-5.4-nano"], "user": "user-123" }, "scaledown": { "threshold": 0.4 } }
            """;
        await model.DoExtractAsync(
            Call(
                providerOptions: providerOptions,
                referenceDate: "2026-11-11",
                maxOutputTokens: 512,
                reasoning: "low"
            ),
            CancellationToken.None
        );
        JsonAssert.Equal(
            RequestBody(capture),
            RequestJson
                + ",\"referenceDate\":\"2026-11-11\",\"maxOutputTokens\":512,\"reasoning\":\"low\",\"providerOptions\":"
                + providerOptions
                + "}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit options that are not set",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_options_that_are_not_set()
    {
        var (model, capture) = Setup();
        await model.DoExtractAsync(Call(providerOptions: null), CancellationToken.None);
        JsonAssert.Equal(RequestBody(capture), RequestJson + "}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass headers correctly",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_headers_correctly()
    {
        var (model, capture) = Setup(
            o11yHeaders: new Dictionary<string, string> { ["ai-o11y-region"] = "iad1" }
        );
        await model.DoExtractAsync(
            Call(headers: new Dictionary<string, string> { ["Custom-Header"] = "test-value" }),
            CancellationToken.None
        );
        Assert.Equal("Bearer test-token", capture.Headers["authorization"]);
        Assert.Equal("test-value", capture.Headers["custom-header"]);
        Assert.Equal("4", capture.Headers["ai-extraction-model-specification-version"]);
        Assert.Equal("scaledown/extract-1", capture.Headers["ai-model-id"]);
        Assert.Equal("iad1", capture.Headers["ai-o11y-region"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return answers of every type",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_answers_of_every_type()
    {
        const string answers = """
            {
              "text": { "type": "text", "value": "INV-2041", "guess": "INV-2041", "pNull": 0.01, "confidence": 0.98, "source": { "type": "text", "part": 0, "start": 8, "end": 16 } },
              "number": { "type": "number", "value": 1250 },
              "date": { "type": "date", "value": "2026-03-03" },
              "time": { "type": "time", "value": "17:30" },
              "datetime": { "type": "datetime", "value": "2026-03-03T17:30:00Z" },
              "duration": { "type": "duration", "value": 120 },
              "missing": { "type": "text", "value": null, "guess": "PO-1", "pNull": 0.9 },
              "fromFile": { "type": "text", "value": "Acme", "source": { "type": "file", "part": 1 } },
              "texts": { "type": "list", "value": ["Ana", "Bo"], "confidence": 0.9, "items": [{ "confidence": 0.95, "source": { "type": "text", "part": 0, "start": 1, "end": 4 } }, {}] },
              "numbers": { "type": "list", "value": [1, 2.5] },
              "emptyList": { "type": "list", "value": [] },
              "missingList": { "type": "list", "value": null, "pNull": 0.8 }
            }
            """;
        var (model, _) = Setup(Response(answers));
        var result = await model.DoExtractAsync(Call(), CancellationToken.None);
        var a = result.Answers;

        Assert.Equal(12, a.Count);
        Assert.Equal("text", a["text"].Type);
        Assert.Equal("INV-2041", a["text"].Text);
        Assert.Equal("INV-2041", a["text"].GuessText);
        Assert.Equal(0.01, a["text"].PNull);
        Assert.Equal(0.98, a["text"].Confidence);
        Assert.Equal("text", a["text"].Source!.Type);
        Assert.Equal(
            (0, 8, 16),
            (a["text"].Source!.Part, a["text"].Source!.Start, a["text"].Source!.End)
        );
        Assert.Equal(1250, a["number"].Number);
        Assert.Equal("2026-03-03", a["date"].Text);
        Assert.Equal("17:30", a["time"].Text);
        Assert.Equal("2026-03-03T17:30:00Z", a["datetime"].Text);
        Assert.Equal("duration", a["duration"].Type);
        Assert.Equal(120, a["duration"].Number);
        Assert.Null(a["missing"].Text);
        Assert.Equal("PO-1", a["missing"].GuessText);
        Assert.Equal(0.9, a["missing"].PNull);
        Assert.Equal("file", a["fromFile"].Source!.Type);
        Assert.Equal(1, a["fromFile"].Source!.Part);
        Assert.Null(a["fromFile"].Source!.Start);
        Assert.Equal(new[] { "Ana", "Bo" }, a["texts"].TextItems);
        Assert.Equal(0.9, a["texts"].Confidence);
        Assert.Equal(2, a["texts"].Items!.Count);
        Assert.Equal(0.95, a["texts"].Items![0].Confidence);
        Assert.Equal(
            (0, 1, 4),
            (
                a["texts"].Items![0].Source!.Part,
                a["texts"].Items![0].Source!.Start,
                a["texts"].Items![0].Source!.End
            )
        );
        Assert.Null(a["texts"].Items![1].Confidence);
        Assert.Null(a["texts"].Items![1].Source);
        Assert.Equal(new[] { 1.0, 2.5 }, a["numbers"].NumberItems);
        Assert.Empty(a["emptyList"].TextItems!);
        Assert.Null(a["missingList"].TextItems);
        Assert.Null(a["missingList"].NumberItems);
        Assert.Equal(0.8, a["missingList"].PNull);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return refusal answers",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_refusal_answers()
    {
        var (model, _) = Setup(
            Response(
                """
                {
                  "invoiceNumber": { "type": "refusal" },
                  "total": { "type": "number", "value": 1250 },
                  "contacts": { "type": "refusal" }
                }
                """
            )
        );
        var result = await model.DoExtractAsync(Call(), CancellationToken.None);
        Assert.Equal("refusal", result.Answers["invoiceNumber"].Type);
        Assert.Equal("number", result.Answers["total"].Type);
        Assert.Equal(1250, result.Answers["total"].Number);
        Assert.Equal("refusal", result.Answers["contacts"].Type);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return usage, warnings, and provider metadata",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_usage_warnings_and_provider_metadata()
    {
        var (model, _) = Setup(
            Response(
                null,
                ("usage", """{ "inputTokens": 120, "outputTokens": 30 }"""),
                (
                    "warnings",
                    """[{ "type": "unsupported", "feature": "reasoning", "details": "Ignored." }, { "type": "other", "message": "Something else." }]"""
                ),
                ("providerMetadata", """{ "gateway": { "cost": "0.002" } }""")
            )
        );
        var result = await model.DoExtractAsync(Call(), CancellationToken.None);
        Assert.Equal(120, result.Usage!.InputTokens);
        Assert.Equal(30, result.Usage!.OutputTokens);
        Assert.Equal(2, result.Warnings.Count);
        Assert.Equal("unsupported", result.Warnings[0].Type);
        Assert.Equal("reasoning", result.Warnings[0].Feature);
        Assert.Equal("Ignored.", result.Warnings[0].Details);
        Assert.Equal("other", result.Warnings[1].Type);
        Assert.Equal("Something else.", result.Warnings[1].Message);
        JsonAssert.Equal(result.ProviderMetadata!.Value, """{ "gateway": { "cost": "0.002" } }""");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit usage and default warnings when absent",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_usage_and_default_warnings_when_absent()
    {
        var (model, _) = Setup();
        var result = await model.DoExtractAsync(Call(), CancellationToken.None);
        Assert.Null(result.Usage);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should drop null optional fields from the response",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_drop_null_optional_fields_from_the_response()
    {
        var (model, _) = Setup(
            Response(
                """
                {
                  "invoiceNumber": { "type": "text", "value": null, "guess": null, "pNull": 0.9, "confidence": null, "source": null },
                  "total": { "type": "number", "value": 1250, "guess": null, "source": null },
                  "contacts": { "type": "list", "value": ["Ana"], "pNull": null, "items": [{ "confidence": null, "source": null }] }
                }
                """,
                ("model", "null"),
                ("usage", """{ "inputTokens": 120, "outputTokens": null }"""),
                ("warnings", "null"),
                ("providerMetadata", "null")
            )
        );
        var result = await model.DoExtractAsync(Call(), CancellationToken.None);
        var invoice = result.Answers["invoiceNumber"];
        Assert.Equal("text", invoice.Type);
        Assert.Null(invoice.Text);
        Assert.Null(invoice.GuessText);
        Assert.Equal(0.9, invoice.PNull);
        Assert.Null(invoice.Confidence);
        Assert.Null(invoice.Source);
        var total = result.Answers["total"];
        Assert.Equal(1250, total.Number);
        Assert.Null(total.GuessNumber);
        Assert.Null(total.Source);
        var contacts = result.Answers["contacts"];
        Assert.Equal(new[] { "Ana" }, contacts.TextItems);
        Assert.Null(contacts.PNull);
        var item = Assert.Single(contacts.Items!);
        Assert.Null(item.Confidence);
        Assert.Null(item.Source);
        Assert.Equal(120, result.Usage!.InputTokens);
        Assert.Null(result.Usage!.OutputTokens);
        Assert.Empty(result.Warnings);
        Assert.Null(result.ProviderMetadata);
        Assert.Equal("scaledown/extract-1", result.Response!.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep an unknown warning as an other warning",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_an_unknown_warning_as_an_other_warning()
    {
        var (model, _) = Setup(
            Response(
                null,
                (
                    "warnings",
                    """[{ "type": "other", "message": "Known." }, { "type": "future", "detail": "New." }]"""
                )
            )
        );
        var result = await model.DoExtractAsync(Call(), CancellationToken.None);
        Assert.Equal(2, result.Warnings.Count);
        Assert.Equal("other", result.Warnings[0].Type);
        Assert.Equal("Known.", result.Warnings[0].Message);
        Assert.Equal("other", result.Warnings[1].Type);
        Assert.Equal(
            "Unknown warning: {\"type\":\"future\",\"detail\":\"New.\"}",
            result.Warnings[1].Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return response metadata",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_response_metadata()
    {
        var (model, capture) = Setup(Response(null, ("model", "\"scaledown/extract-1-fallback\"")));
        capture.ResponseHeaders["x-request-id"] = "req-1";
        var result = await model.DoExtractAsync(Call(), CancellationToken.None);
        Assert.Equal("scaledown/extract-1-fallback", result.Response!.ModelId);
        Assert.Equal("req-1", result.Response!.Headers!["x-request-id"]);
        JsonAssert.Equal(
            (JsonElement)result.Response.Body!,
            "{\"answers\":" + DummyAnswers + ",\"model\":\"scaledown/extract-1-fallback\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should attribute the response to the requested model when none is returned",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_attribute_the_response_to_the_requested_model_when_none_is_returned()
    {
        var (model, _) = Setup();
        var result = await model.DoExtractAsync(Call(), CancellationToken.None);
        Assert.Equal("scaledown/extract-1", result.Response!.ModelId);
    }
}
