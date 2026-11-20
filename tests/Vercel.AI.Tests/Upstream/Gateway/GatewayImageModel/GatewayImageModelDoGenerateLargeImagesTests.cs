// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Tests.Upstream.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayImageModel;

/// <summary>Port of <c>gateway-image-model.test.ts</c> &gt; <c>GatewayImageModel &gt; doGenerate &gt; large images with experimental streaming decoding</c>.</summary>
public sealed class GatewayImageModelDoGenerateLargeImagesTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-image-model.test.ts::GatewayImageModel > doGenerate > large images with experimental streaming decoding::";

    private const string Commit = "89aff56";

    private const string StreamingOptions =
        "{\"gateway\":{\"experimental_streamingImageDecoding\":true}}";

    private static readonly byte[] Large = BytesOf(70_000, 1);
    private static readonly string LargeBase64 = Convert.ToBase64String(Large);
    private static readonly byte[] Small = BytesOf(1_000, 2);
    private static readonly string SmallBase64 = Convert.ToBase64String(Small);

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

    private static UpstreamRecordingHandler RespondWith(params string[] images) =>
        GatewayImageModelSupport.PrepareJsonResponse(
            "[" + string.Join(",", images.Select(image => "\"" + image + "\"")) + "]"
        );

    private static Task<Vercel.AI.Operations.ImageModelResult> Generate(
        UpstreamRecordingHandler handler,
        string providerOptions = StreamingOptions
    ) =>
        GatewayImageModelSupport.DoGenerate(
            GatewayImageModelSupport.CreateTestModel(handler),
            n: 2,
            providerOptions: providerOptions
        );

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix + "should preserve base64 results when the option is %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_preserve_base64_results_when_the_option_is(bool? enabled)
    {
        var handler = RespondWith(LargeBase64, SmallBase64);

        var result = await Generate(
            handler,
            enabled is null
                ? "{}"
                : "{\"gateway\":{\"experimental_streamingImageDecoding\":"
                    + (enabled.Value ? "true" : "false")
                    + "}}"
        );

        Assert.Equal(new object?[] { LargeBase64, SmallBase64 }, result.Images);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix + "should remove the client option set to %s while preserving service options",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_remove_the_client_option_set_to_while_preserving_service_options(
        bool enabled
    )
    {
        var handler = RespondWith(LargeBase64);

        await Generate(
            handler,
            "{\"gateway\":{\"experimental_streamingImageDecoding\":"
                + (enabled ? "true" : "false")
                + ",\"tags\":[\"images\"]},\"openai\":{\"quality\":\"hd\"}}"
        );

        var body = GatewayImageModelSupport.Body(handler)!;
        Assert.True(
            GatewayImageModelSupport.Equal(
                "{\"gateway\":{\"tags\":[\"images\"]},\"openai\":{\"quality\":\"hd\"}}",
                body["providerOptions"]
            )
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix + "should omit providerOptions when the client option set to %s is its only entry",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_omit_providerOptions_when_the_client_option_set_to_is_its_only_entry(
        bool enabled
    )
    {
        var handler = RespondWith(SmallBase64);

        await Generate(
            handler,
            "{\"gateway\":{\"experimental_streamingImageDecoding\":"
                + (enabled ? "true" : "false")
                + "}}"
        );

        Assert.False(
            GatewayImageModelSupport.Body(handler)!.AsObject().ContainsKey("providerOptions")
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix + "should omit an empty gateway object when the client option is %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_omit_an_empty_gateway_object_when_the_client_option_is(bool enabled)
    {
        var handler = RespondWith(SmallBase64);

        await Generate(
            handler,
            "{\"gateway\":{\"experimental_streamingImageDecoding\":"
                + (enabled ? "true" : "false")
                + "},\"openai\":{\"quality\":\"hd\"}}"
        );

        Assert.True(
            GatewayImageModelSupport.Equal(
                "{\"openai\":{\"quality\":\"hd\"}}",
                GatewayImageModelSupport.Body(handler)!["providerOptions"]
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return images of 64 KiB and more as Uint8Array",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_return_images_of_64_KiB_and_more_as_bytes()
    {
        var result = await Generate(RespondWith(LargeBase64));

        Assert.Equal(Large, Assert.IsType<byte[]>(Assert.Single(result.Images)));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep images below 64 KiB as base64 strings",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_keep_images_below_64_KiB_as_base64_strings()
    {
        var result = await Generate(RespondWith(SmallBase64, SmallBase64));

        Assert.Equal(new object?[] { SmallBase64, SmallBase64 }, result.Images);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return bytes for every image when large and small images are mixed",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_return_bytes_for_every_image_when_large_and_small_images_are_mixed()
    {
        var result = await Generate(RespondWith(SmallBase64, LargeBase64, SmallBase64));

        Assert.Equal(new object?[] { Small, Large, Small }, result.Images);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should keep strings that cannot be decoded unambiguously and return all images as strings",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_keep_strings_that_cannot_be_decoded_unambiguously_and_return_all_images_as_strings()
    {
        var base64Url = LargeBase64.Replace('+', '-').Replace('/', '_');

        var result = await Generate(RespondWith(base64Url, SmallBase64));

        Assert.Equal(new object?[] { base64Url, SmallBase64 }, result.Images);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should decode large strings that are not canonical base64 like the regular path when mixed with bytes",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_decode_large_strings_that_are_not_canonical_base64_like_the_regular_path_when_mixed_with_bytes()
    {
        var base64Url = LargeBase64.Replace('+', '-').Replace('/', '_');

        var result = await Generate(RespondWith(base64Url, LargeBase64));

        Assert.Equal(new object?[] { Large, Large }, result.Images);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not change the other fields of the response",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_change_the_other_fields_of_the_response()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse(
            "[\"" + LargeBase64 + "\"]",
            "\"warnings\":[{\"type\":\"other\",\"message\":\"careful\"}],"
                + "\"providerMetadata\":{\"gateway\":{\"images\":[{}],\"cost\":\"0.04\"}},"
                + "\"usage\":{\"inputTokens\":1,\"outputTokens\":2,\"totalTokens\":3}"
        );

        var result = await Generate(handler);

        Assert.Equal(new object?[] { Large }, result.Images);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("careful", warning.Message);
        Assert.True(
            GatewayImageModelSupport.Equal(
                "{\"gateway\":{\"images\":[{}],\"cost\":\"0.04\"}}",
                JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())
            )
        );
        Assert.Equal(1, result.Usage!.InputTokens);
        Assert.Equal(2, result.Usage.OutputTokens);
        Assert.Equal(3, result.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should read a body that arrives in small chunks",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_read_a_body_that_arrives_in_small_chunks()
    {
        var body = "{\"images\":[\"" + LargeBase64 + "\",\"" + SmallBase64 + "\"]}";
        var handler = new ChunkedHandler(Encoding.UTF8.GetBytes(body), 1999);

        var result = await Generate(GatewayImageModelSupport.CreateTestModel(handler));

        Assert.Equal(new object?[] { Large, Small }, result.Images);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject a response whose images are not strings",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_a_response_whose_images_are_not_strings()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"" + LargeBase64 + "\",5]");

        var error = await Record.ExceptionAsync(() => Generate(handler));

        Assert.True(GatewayResponseError.IsInstance(error));
        // The error does not carry the image.
        Assert.True(error!.Message.Length < 2000);
    }

    private static Task<Vercel.AI.Operations.ImageModelResult> Generate(
        Vercel.AI.Gateway.GatewayImageModel model
    ) => GatewayImageModelSupport.DoGenerate(model, n: 2, providerOptions: StreamingOptions);

    /// <summary>Answers with a body that a read returns in pieces of at most <c>chunkSize</c> bytes.</summary>
    private sealed class ChunkedHandler(byte[] body, int chunkSize) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var content = new StreamContent(new ChunkedStream(body, chunkSize));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                "application/json"
            );
            return Task.FromResult(
                new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = content }
            );
        }
    }
}
