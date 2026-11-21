// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResponseHandler;

/// <summary>
/// Port of <c>response-handler.test.ts</c> &gt; <c>createJsonResponseHandler with experimental_binaryFields</c> &gt;
/// <c>when a Standard Schema validator rejects the response</c>. A <c>byte[]</c> has no <c>toJSON</c> hook, so the handler removes the
/// base64 of the extracted bytes from the messages of the validator's error instead.
/// </summary>
public sealed class CreateJsonResponseHandlerWithExperimentalBinaryFieldsWhenAStandardSchemaValidatorRejectsTheResponseTests
{
    private const string Prefix =
        "packages/provider-utils/src/response-handler.test.ts::createJsonResponseHandler with experimental_binaryFields::when a Standard Schema validator rejects the response::";

    private const string Commit = "89aff561654b229271a3995bea3d0ff1042fecd2";

    private static byte[] BytesOf(int length)
    {
        var bytes = new byte[length];
        var state = 1u;
        for (var index = 0; index < length; index++)
        {
            state = unchecked(state * 1103515245 + 12345);
            bytes[index] = (byte)(state >> 24);
        }

        return bytes;
    }

    // Not a Zod schema: the error is made from the value the validator was given, which holds the bytes, and the issues may carry
    // the input they are about, like the issues of arktype do.
    private static Func<object?, object?> CreateRejectingStandardSchema(
        Func<Dictionary<string, object?>, (string Message, object? Data)> issueFor
    ) =>
        value =>
        {
            var (message, data) = issueFor((Dictionary<string, object?>)value!);
            throw new FormatException(message + " " + JsonSerializer.Serialize(data ?? value));
        };

    private static async Task<ApiException> Rejection(string body, Func<object?, object?> validate)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        return await Assert.ThrowsAsync<ApiException>(() =>
            ResponseHandlers.CreateJsonResponseHandler(validate, ["images[*]"])(
                response,
                "test-url",
                new { },
                default
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not print the bytes in the message of the error",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_print_the_bytes_in_the_message_of_the_error()
    {
        var body = "{\"images\":[\"" + Convert.ToBase64String(BytesOf(500_000)) + "\"],\"note\":5}";

        var error = await Rejection(
            body,
            CreateRejectingStandardSchema(_ => ("note must be a string", null))
        );

        var cause = Assert.IsType<TypeValidationException>(error.InnerException);
        Assert.Contains("note must be a string", cause.Message);
        Assert.True(cause.Message.Length < 1000);
        // The error of the validator, which the one above wraps.
        Assert.NotNull(cause.InnerException);
        Assert.True(cause.InnerException.Message.Length < 1000);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not print the bytes that an issue carries",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_print_the_bytes_that_an_issue_carries()
    {
        var body = "{\"images\":[\"" + Convert.ToBase64String(BytesOf(500_000)) + "\"]}";

        var error = await Rejection(
            body,
            CreateRejectingStandardSchema(value =>
                ("images[0] must be a string", ((List<object?>)value["images"]!)[0])
            )
        );

        var cause = Assert.IsType<TypeValidationException>(error.InnerException);
        Assert.Contains("images[0] must be a string", cause.Message);
        Assert.True(cause.Message.Length < 1000);
    }
}
