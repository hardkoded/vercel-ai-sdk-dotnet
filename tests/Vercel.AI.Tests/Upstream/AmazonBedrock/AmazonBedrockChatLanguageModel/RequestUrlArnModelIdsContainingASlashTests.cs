// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModel;

/// <summary>Port of <c>amazon-bedrock-chat-language-model.test.ts</c> &gt; <c>request URL &gt; ARN model IDs containing a slash</c>.</summary>
public sealed class RequestUrlArnModelIdsContainingASlashTests
{
    private const string Suite = "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::request URL > ARN model IDs containing a slash::";
    private const string InferenceProfileArn = "arn:aws:bedrock:eu-west-1:474668406012:inference-profile/eu.amazon.nova-lite-v1:0";
    private const string UnknownOperationResponse = """{"Output":{"__type":"com.amazon.coral.service#UnknownOperationException"},"Version":"1.0"}""";
    private const string ConverseResponse = """{"output":{"message":{"role":"assistant","content":[{"text":"Hello!"}]}},"stopReason":"end_turn","usage":{"inputTokens":1,"outputTokens":1,"totalTokens":2}}""";

    private static readonly string[] ConverseStreamChunks =
    {
        """{"messageStart":{"role":"assistant"}}""",
        """{"contentBlockDelta":{"contentBlockIndex":0,"delta":{"text":"Hello!"}}}""",
        """{"contentBlockStop":{"contentBlockIndex":0}}""",
        """{"messageStop":{"stopReason":"end_turn"}}""",
        """{"metadata":{"usage":{"inputTokens":1,"outputTokens":1,"totalTokens":2}}}""",
    };

    [Fact]
    [UpstreamTest(Suite + "should generate text through the encoded Converse route", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_generate_text_through_the_encoded_Converse_route()
    {
        var result = await CreateInferenceProfileModel().DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = BedrockStreamSupport.TestPrompt },
            CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Hello!", text.Text);
    }

    [Fact]
    [UpstreamTest(Suite + "should stream text through the encoded Converse route", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text_through_the_encoded_Converse_route()
    {
        var stream = await CreateInferenceProfileModel().DoStreamAsync(
            new LanguageModelCallOptions { Prompt = BedrockStreamSupport.TestPrompt, IncludeRawChunks = false },
            CancellationToken.None);
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in stream)
        {
            parts.Add(part);
        }

        Assert.Equal(6, parts.Count);
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Null(metadata.Id);
        Assert.Equal(InferenceProfileArn, metadata.ModelId);
        Assert.Null(metadata.Timestamp);
        Assert.Equal("0", Assert.IsType<TextStartStreamPart>(parts[2]).Id);
        var delta = Assert.IsType<TextDeltaStreamPart>(parts[3]);
        Assert.Equal("0", delta.Id);
        Assert.Equal("Hello!", delta.Delta);
        Assert.Equal("0", Assert.IsType<TextEndStreamPart>(parts[4]).Id);
        var finish = Assert.IsType<FinishStreamPart>(Assert.Single(parts, part => part is FinishStreamPart));
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("end_turn", finish.RawFinishReason);
        Assert.Equal(1, finish.Usage.InputTokens);
        Assert.Equal(1, finish.Usage.OutputTokens);
        Assert.Equal(2, finish.Usage.TotalTokens);
    }

    private static ILanguageModel CreateInferenceProfileModel()
    {
        return BedrockStreamSupport.CreateModel(new EncodedRouteHandler(), InferenceProfileArn);
    }

    private sealed class EncodedRouteHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri!.AbsoluteUri;
            var isEncodedRoute = url.Contains(Uri.EscapeDataString(InferenceProfileArn), StringComparison.Ordinal);
            var isStream = isEncodedRoute && url.EndsWith("/converse-stream", StringComparison.Ordinal);
            HttpContent content;
            if (isStream)
            {
                var stream = new MemoryStream();
                foreach (var frame in EventStreamFrames.FromChunkLines(ConverseStreamChunks))
                {
                    stream.Write(frame, 0, frame.Length);
                }

                stream.Position = 0;
                content = new UnknownLengthContent(stream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.amazon.eventstream");
            }
            else
            {
                content = new StringContent(isEncodedRoute ? ConverseResponse : UnknownOperationResponse, Encoding.UTF8, "application/json");
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }
}
