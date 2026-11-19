// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayExtractionModel.GatewayExtractionModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayExtractionModel;

/// <summary>Port of <c>gateway-extraction-model.test.ts</c> &gt; <c>GatewayExtractionModel &gt; state parts</c>.</summary>
public sealed class GatewayExtractionModelStatePartsTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-extraction-model.test.ts::GatewayExtractionModel > state parts::";

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static TheoryData<string, string> StatePartsCases()
    {
        return new TheoryData<string, string>
        {
            { "a text part", """[{ "type": "text", "text": "Inspect." }]""" },
            {
                "a JSON part",
                """[{ "type": "json", "value": { "product": "vase", "history": ["new"] } }]"""
            },
            {
                "mixed parts",
                """
                    [
                      { "type": "text", "text": "Inspect." },
                      { "type": "json", "value": { "product": "vase" } },
                      { "type": "text", "text": "Again." }
                    ]
                    """
            },
        };
    }

    private static List<DecisionModelStatePart> Parts(string json)
    {
        var parts = new List<DecisionModelStatePart>();
        foreach (var part in Json(json).EnumerateArray())
        {
            parts.Add(
                part.GetProperty("type").GetString() == "text"
                    ? new DecisionModelTextPart(part.GetProperty("text").GetString()!)
                    : new DecisionModelJsonPart(part.GetProperty("value"))
            );
        }

        return parts;
    }

    [Theory]
    [MemberData(nameof(StatePartsCases))]
    [UpstreamTest(
        Prefix + "sends $name as ordered state parts",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sends_name_as_ordered_state_parts(string name, string state)
    {
        Assert.NotEmpty(name);
        var (model, capture) = Setup();
        await model.DoExtractAsync(Call(Parts(state)), CancellationToken.None);
        JsonAssert.Equal(
            RequestBody(capture),
            "{\"stateParts\":" + state + ",\"fields\":" + FieldsJson + ",\"providerOptions\":{}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "sends file bytes as base64 in ordered state parts",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sends_file_bytes_as_base64_in_ordered_state_parts()
    {
        var (model, capture) = Setup();
        var data = new LanguageModelFilePart("data", "application/pdf")
        {
            Bytes = new byte[] { 1, 2, 3 },
        };
        await model.DoExtractAsync(
            Call([
                new DecisionModelTextPart("Read this invoice."),
                new DecisionModelFilePart("application/pdf", data) { FileName = "invoice.pdf" },
            ]),
            CancellationToken.None
        );
        JsonAssert.Equal(
            RequestBody(capture)!["stateParts"],
            """
            [
              { "type": "text", "text": "Read this invoice." },
              { "type": "file", "mediaType": "application/pdf", "filename": "invoice.pdf", "data": { "type": "data", "data": "AQID" } }
            ]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "sends base64 file strings unchanged",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sends_base64_file_strings_unchanged()
    {
        var (model, capture) = Setup();
        var data = new LanguageModelFilePart("data", "image/jpeg") { Base64 = "AQID" };
        await model.DoExtractAsync(
            Call([new DecisionModelFilePart("image/jpeg", data)]),
            CancellationToken.None
        );
        JsonAssert.Equal(
            RequestBody(capture)!["stateParts"],
            """[{ "type": "file", "mediaType": "image/jpeg", "data": { "type": "data", "data": "AQID" } }]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "sends file references unchanged",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sends_file_references_unchanged()
    {
        var (model, capture) = Setup();
        var data = new LanguageModelFilePart("reference", "application/pdf")
        {
            Reference = new Dictionary<string, string> { ["gateway"] = "file_123" },
        };
        await model.DoExtractAsync(
            Call([new DecisionModelFilePart("application/pdf", data)]),
            CancellationToken.None
        );
        JsonAssert.Equal(
            RequestBody(capture)!["stateParts"],
            """[{ "type": "file", "mediaType": "application/pdf", "data": { "type": "reference", "reference": { "gateway": "file_123" } } }]"""
        );
    }

    [Theory]
    [InlineData("url")]
    [InlineData("text")]
    [UpstreamTest(
        Prefix + "rejects $type file data before sending a Gateway request",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_type_file_data_before_sending_a_Gateway_request(string type)
    {
        var (model, capture) = Setup();
        var data = new LanguageModelFilePart(type, "application/pdf")
        {
            Url = type == "url" ? new Uri("https://example.com/a.pdf") : null,
            Text = type == "text" ? "Invoice" : null,
        };
        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            model.DoExtractAsync(
                Call([new DecisionModelFilePart("application/pdf", data)]),
                CancellationToken.None
            )
        );
        Assert.Equal("Gateway extraction file input: " + type + " data", error.Functionality);
        Assert.Equal(0, capture.Calls);
    }
}
