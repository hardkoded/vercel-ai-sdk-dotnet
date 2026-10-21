// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Fal;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.MoreProviders;
using FalTranscriptionModelType = Vercel.AI.Fal.FalTranscriptionModel;

namespace Vercel.AI.Tests.Upstream.Fal.FalTranscriptionModel;

/// <summary>Port of <c>fal-transcription-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>transcription</c>.</summary>
public sealed class DoGenerateTranscriptionTests
{
    private const string Prefix = "packages/fal/src/fal-transcription-model.test.ts::doGenerate > transcription::";
    private const string Transcript = "{\"text\":\"Hello from the Versal AISDK.\",\"chunks\":[{\"timestamp\":[0,2.508],\"text\":\"Hello from the Versal AISDK.\"}],\"languages\":[\"en\"]}";

    [Fact]
    [UpstreamTest(Prefix + "should encode the request ID as a single URL path segment", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_encode_the_request_ID_as_a_single_URL_path_segment()
    {
        var requestId = "abc/../../internal";
        var queued = "{\"status\":\"IN_QUEUE\",\"request_id\":\"" + requestId + "\",\"response_url\":\"https://queue.fal.run/fal-ai/wizper/requests/" + requestId + "/result\",\"status_url\":\"https://queue.fal.run/fal-ai/wizper/requests/" + requestId + "\",\"cancel_url\":\"https://queue.fal.run/fal-ai/wizper/requests/" + requestId + "/cancel\",\"logs\":null,\"metrics\":{},\"queue_position\":0}";
        var handler = new ParityHandler(call => ParityHandler.Json(call.Method == HttpMethod.Post ? queued : Transcript));
        var model = new FalTranscriptionModelType(FalProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler), "wizper", null);
        var call = new TranscriptionModelCall(new byte[] { 0x49, 0x44, 0x33, 0x04 }, "audio/wav", JsonValues.EmptyObject(), new Dictionary<string, string>(), CancellationToken.None);

        await model.DoGenerateAsync(call, CancellationToken.None);

        Assert.Equal("https://queue.fal.run/fal-ai/wizper/requests/" + Uri.EscapeDataString(requestId), handler.Calls[1].Uri.AbsoluteUri);
    }
}
