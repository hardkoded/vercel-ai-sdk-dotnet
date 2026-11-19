// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Gateway;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayExtractionModel.GatewayExtractionModelSupport;
using GatewayInternalServerError = Vercel.AI.Gateway.GatewayInternalServerError;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayExtractionModel;

/// <summary>Port of <c>gateway-extraction-model.test.ts</c> &gt; <c>GatewayExtractionModel &gt; error handling</c>.</summary>
public sealed class GatewayExtractionModelErrorHandlingTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-extraction-model.test.ts::GatewayExtractionModel > error handling::";

    [Fact]
    [UpstreamTest(
        Prefix + "should throw ExtractionUnsupportedFieldTypeError for an unsupported field type",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_ExtractionUnsupportedFieldTypeError_for_an_unsupported_field_type()
    {
        var body = Error(
            """
            {
              "message": "Field \"total\" has type \"number\", which is not supported by provider \"upstream.extraction\" and model \"extract-1\".",
              "type": "invalid_request_error",
              "param": { "name": "AI_ExtractionUnsupportedFieldTypeError", "fieldId": "total", "fieldType": "number", "statusCode": 400 }
            }
            """
        );
        var error = await Assert.ThrowsAsync<ExtractionUnsupportedFieldTypeError>(() =>
            Setup(body, HttpStatusCode.BadRequest)
                .Model.DoExtractAsync(Call(), CancellationToken.None)
        );
        Assert.Equal("total", error.FieldId);
        Assert.Equal("number", error.FieldType);
        Assert.Equal("gateway", error.Provider);
        Assert.Equal("scaledown/extract-1", error.ModelId);
        Assert.Equal(
            "Field \"total\" has type \"number\", which is not supported by provider \"gateway\" and model \"scaledown/extract-1\".",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep the list item type of an unsupported list field",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_the_list_item_type_of_an_unsupported_list_field()
    {
        var body = Error(
            """
            {
              "message": "Unsupported field type.",
              "type": "invalid_request_error",
              "param": { "name": "AI_ExtractionUnsupportedFieldTypeError", "fieldId": "contacts", "fieldType": "list<number>" }
            }
            """
        );
        var error = await Assert.ThrowsAsync<ExtractionUnsupportedFieldTypeError>(() =>
            Setup(body, HttpStatusCode.BadRequest)
                .Model.DoExtractAsync(Call(), CancellationToken.None)
        );
        Assert.Equal("contacts", error.FieldId);
        Assert.Equal("list<number>", error.FieldType);
    }

    [Theory]
    [InlineData("no param", "")]
    [InlineData(
        "another error class",
        ",\"param\":{\"name\":\"AI_InvalidArgumentError\",\"fieldId\":\"a\",\"fieldType\":\"b\"}"
    )]
    [InlineData("no field", ",\"param\":{\"name\":\"AI_ExtractionUnsupportedFieldTypeError\"}")]
    [UpstreamTest(
        Prefix + "should throw GatewayInvalidRequestError on a 400 with $name",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_GatewayInvalidRequestError_on_a_400_with_name(
        string name,
        string param
    )
    {
        Assert.NotEmpty(name);
        var body = Error(
            "{\"message\":\"Invalid fields format\",\"type\":\"invalid_request_error\""
                + param
                + "}"
        );
        var error = await Assert.ThrowsAsync<GatewayInvalidRequestError>(() =>
            Setup(body, HttpStatusCode.BadRequest)
                .Model.DoExtractAsync(Call(), CancellationToken.None)
        );
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Invalid fields format", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw GatewayInternalServerError on 500",
        Commit = "f886264eb6605d8c5a9bc26532a214d97fc72cd5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_GatewayInternalServerError_on_500()
    {
        var body = Error(
            "{\"message\":\"Internal server error\",\"type\":\"internal_server_error\"}"
        );
        var error = await Assert.ThrowsAsync<GatewayInternalServerError>(() =>
            Setup(body, HttpStatusCode.InternalServerError)
                .Model.DoExtractAsync(Call(), CancellationToken.None)
        );
        Assert.Equal(500, error.StatusCode);
    }
}
