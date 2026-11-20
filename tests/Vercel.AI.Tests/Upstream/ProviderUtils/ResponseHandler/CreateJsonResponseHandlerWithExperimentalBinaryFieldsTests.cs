// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResponseHandler;

/// <summary>
/// Port of <c>response-handler.test.ts</c> &gt; <c>createJsonResponseHandler with experimental_binaryFields</c>.
/// <c>JsonStreams.ReadJsonAsync</c> takes <c>experimentalBinaryFields</c>. A JSON element cannot hold bytes, so
/// <c>value</c> and <c>rawValue</c> hold the placeholder and <c>Binaries</c> maps it to the decoded bytes.
/// </summary>
public sealed class CreateJsonResponseHandlerWithExperimentalBinaryFieldsTests
{
    private const string Prefix =
        "packages/provider-utils/src/response-handler.test.ts::createJsonResponseHandler with experimental_binaryFields::";

    private const string Commit = "89aff561654b229271a3995bea3d0ff1042fecd2";

    private static readonly string[] BinaryFields = ["images[*]"];

    private static readonly JsonNode Schema = JsonSchemas.Object(
        [
            new KeyValuePair<string, JsonNode>("images", JsonSchemas.Array(JsonSchemas.String())),
            new KeyValuePair<string, JsonNode>("note", JsonSchemas.String()),
        ],
        ["images"]
    );

    private static byte[] BytesOf(int length, uint seed = 1)
    {
        var bytes = new byte[length];
        var state = seed;
        for (var index = 0; index < length; index++)
        {
            state = unchecked(state * 1103515245 + 12345);
            bytes[index] = (byte)(state >> 24);
        }

        return bytes;
    }

    private static string Body(string[] images, string? note = null) =>
        "{\"images\":["
        + string.Join(",", images.Select(image => "\"" + image + "\""))
        + "]"
        + (note is null ? "" : ",\"note\":\"" + note + "\"")
        + "}";

    private static HttpResponseMessage Response(
        string body,
        int chunkSize = 0,
        bool contentLength = false
    )
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        HttpContent content =
            chunkSize == 0
                ? new StringContent(body, Encoding.UTF8, "application/json")
                : new StreamContent(new ChunkedStream(bytes, chunkSize));
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "application/json"
        );
        if (chunkSize != 0 && contentLength)
        {
            content.Headers.ContentLength = bytes.Length;
        }

        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    private static Task<JsonStreams.JsonBody> Run(
        HttpResponseMessage response,
        string[]? binaryFields = null
    ) => JsonStreams.ReadJsonAsync(response, Schema, default, binaryFields ?? BinaryFields);

    private static string Placeholder(JsonElement value) => value.GetString()!;

    [Fact]
    [UpstreamTest(
        Prefix + "should return decoded bytes as value and a placeholder in rawValue",
        Coverage = UpstreamCoverage.Partial,
        Note = "A JsonElement cannot hold bytes: Value holds the placeholder and Binaries the decoded bytes.",
        Commit = Commit
    )]
    public async Task Value_and_RawValue_hold_a_placeholder_and_Binaries_the_decoded_bytes()
    {
        var bytes = BytesOf(70_001);
        var body = Body([Convert.ToBase64String(bytes)], "hello");

        var result = await Run(Response(body));

        var placeholder = Placeholder(result.RawValue.GetProperty("images")[0]);
        Assert.Matches(@"^\[binary data: 70001 bytes; ref \d+-\d+\]\z", placeholder);
        Assert.Equal(bytes, result.Binaries[placeholder]);
        Assert.Equal(placeholder, Placeholder(result.Value.GetProperty("images")[0]));
        Assert.Equal("hello", result.RawValue.GetProperty("note").GetString());
        Assert.Equal("hello", result.Value.GetProperty("note").GetString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return the bytes that decoding the string would give",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_return_the_bytes_that_decoding_the_string_would_give()
    {
        var base64 = Convert.ToBase64String(BytesOf(70_003, 5));

        var result = await Run(Response(Body([base64])));

        Assert.Equal(Convert.FromBase64String(base64), Assert.Single(result.Binaries).Value);
    }

    [Theory]
    [InlineData("a Response built from a string", 0, false)]
    [InlineData("a stream without Content-Length", 16 * 1024, false)]
    [InlineData("a stream with Content-Length", 16 * 1024, true)]
    [InlineData("a stream of small chunks with Content-Length", 4093, true)]
    [UpstreamTest(
        Prefix + "should decode when the body arrives as %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_decode_when_the_body_arrives_as(
        string name,
        int chunkSize,
        bool contentLength
    )
    {
        _ = name;
        var bytes = BytesOf(66_001, 3);
        var body = Body([Convert.ToBase64String(bytes)], "ok");

        var result = await Run(Response(body, chunkSize, contentLength));

        Assert.Equal(bytes, Assert.Single(result.Binaries).Value);
        Assert.Equal("ok", result.Value.GetProperty("note").GetString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should decode several images and keep the order",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_decode_several_images_and_keep_the_order()
    {
        var first = BytesOf(66_000, 1);
        var second = BytesOf(70_000, 2);
        var body = Body([Convert.ToBase64String(first), Convert.ToBase64String(second)]);

        var result = await Run(Response(body, 9999));

        var images = result.Value.GetProperty("images").EnumerateArray().ToList();
        Assert.Equal(2, images.Count);
        Assert.Equal(first, result.Binaries[Placeholder(images[0])]);
        Assert.Equal(second, result.Binaries[Placeholder(images[1])]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep strings below 64 KiB and non-canonical strings as strings",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_keep_strings_below_64_KiB_and_non_canonical_strings_as_strings()
    {
        var small = Convert.ToBase64String(BytesOf(65_535, 1));
        var large = BytesOf(65_536, 2);
        var notCanonical = Convert.ToBase64String(BytesOf(100_000, 3)) + "\\n";
        var body = Body([small, Convert.ToBase64String(large), notCanonical]);

        var result = await Run(Response(body));

        var images = result.RawValue.GetProperty("images").EnumerateArray().ToList();
        Assert.Equal(small, images[0].GetString());
        Assert.Equal(large, result.Binaries[Placeholder(images[1])]);
        Assert.Matches(@"^\[binary data: 65536 bytes; ref \d+-\d+\]\z", Placeholder(images[1]));
        Assert.Equal(notCanonical.Replace("\\n", "\n"), images[2].GetString());
        Assert.Single(result.Binaries);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should produce the same result as the default handler when nothing is extracted",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_produce_the_same_result_as_the_default_handler_when_nothing_is_extracted()
    {
        var body =
            "{\"images\":[\"AAAA\",\"data:image/png;base64,AAAA\"],\"note\":\"héllo — 日本語\"}";

        var withFields = await Run(Response(body));
        var withoutFields = await Run(Response(body), []);

        Assert.Equal(withoutFields.Value.GetRawText(), withFields.Value.GetRawText());
        Assert.Equal(withoutFields.RawValue.GetRawText(), withFields.RawValue.GetRawText());
        Assert.Equal(withoutFields.Headers, withFields.Headers);
        Assert.Empty(withFields.Binaries);
    }

    [Theory]
    [InlineData("no options")]
    [InlineData("empty options")]
    [InlineData("an empty list")]
    [UpstreamTest(
        Prefix + "should not change the behavior with %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_change_the_behavior_with(string name)
    {
        var base64 = Convert.ToBase64String(BytesOf(100_000));
        var response = Response("{\"images\":[\"" + base64 + "\"]}");

        var result = await JsonStreams.ReadJsonAsync(
            response,
            null,
            default,
            name == "an empty list" ? [] : null
        );

        Assert.Equal(base64, result.Value.GetProperty("images")[0].GetString());
        Assert.Equal(base64, result.RawValue.GetProperty("images")[0].GetString());
        Assert.Empty(result.Binaries);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject an invalid path when the handler is created",
        Coverage = UpstreamCoverage.Partial,
        Note = "ReadJsonAsync has no separate creation step: the path is rejected when the call starts, before the body is read.",
        Commit = Commit
    )]
    public async Task An_invalid_path_is_rejected_before_the_body_is_read()
    {
        var (response, content) = OversizedResponseContent.Create();

        await Assert.ThrowsAsync<InvalidArgumentError>(() => Run(response, ["images[0]"]));

        Assert.False(content.BodyRead);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw an APICallError with a JSONParseError cause for invalid JSON",
        Coverage = UpstreamCoverage.Partial,
        Note = "Upstream wraps the error in an APICallError; ReadJsonAsync throws the JsonParseException.",
        Commit = Commit
    )]
    public async Task Invalid_JSON_throws_a_JsonParseException_with_placeholders_in_its_text()
    {
        var base64 = Convert.ToBase64String(BytesOf(300_000));
        var body = "{\"images\":[\"" + base64 + "\"],";

        var error = await Assert.ThrowsAsync<JsonParseException>(() =>
            Run(Response(body, 16 * 1024))
        );

        Assert.Matches(
            @"^\{""images"":\[""\[binary data: 300000 bytes; ref \d+-\d+\]""\],\z",
            error.Text
        );
        Assert.True(error.Message.Length < 1000);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should throw the same errors as the default handler for a body without binary data",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_throw_the_same_errors_as_the_default_handler_for_a_body_without_binary_data()
    {
        var body = "{\"images\": [1,";

        var withFields = await Assert.ThrowsAsync<JsonParseException>(() => Run(Response(body)));
        var withoutFields = await Assert.ThrowsAsync<JsonParseException>(() =>
            Run(Response(body), [])
        );

        Assert.Equal(withoutFields.Message, withFields.Message);
        Assert.Equal(withoutFields.Text, withFields.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep rejecting forbidden prototype keys",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_keep_rejecting_forbidden_prototype_keys()
    {
        var base64 = Convert.ToBase64String(BytesOf(100_000));
        var body = "{\"images\":[\"" + base64 + "\"],\"__proto__\":{\"polluted\":true}}";

        await Assert.ThrowsAsync<JsonParseException>(() => Run(Response(body)));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject oversized responses before reading the body",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_oversized_responses_before_reading_the_body()
    {
        var (response, content) = OversizedResponseContent.Create();

        var error = await Assert.ThrowsAsync<DownloadError>(() => Run(response));

        Assert.Contains("exceeded maximum size", error.Message);
        Assert.False(content.BodyRead);
    }
}
