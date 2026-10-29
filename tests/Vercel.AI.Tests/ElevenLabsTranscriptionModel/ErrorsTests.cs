// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.ElevenLabs;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.ElevenLabsTranscriptionModel;

public sealed class ErrorsTests
{
    private const string Prefix = "packages/elevenlabs/src/elevenlabs-transcription-model.test.ts::doGenerate > errors::";

    [Fact]
    [UpstreamTest(
        Prefix + "should surface the ElevenLabs error message and code",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7062ab6818c90e135d068433ce99b11ac0dfa081")]
    public async Task Should_surface_the_ElevenLabs_error_message_and_code()
    {
        const string body = """
            {"detail":{"type":"not_found","code":"model_not_found","message":"A model with model_id 'scribe_v1' was not found.","status":"model_not_found","request_id":"req_test_transcription"}}
            """;
        var handler = new ParityHandler(_ => Error(HttpStatusCode.NotFound, body));
        var provider = ElevenLabsProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        var model = (ElevenLabsProvider.ElevenLabsTranscriptionModel)provider.TranscriptionModel("scribe_v1");

        var error = Assert.IsAssignableFrom<ApiException>(await Record.ExceptionAsync(() =>
            model.TranscribeAsync(new AudioInput(new byte[] { 1, 2, 3 }, "audio/wav", null), null, CancellationToken.None)));

        Assert.Equal("A model with model_id 'scribe_v1' was not found. (model_not_found)", error.Message);
        Assert.Equal(404, error.StatusCode);
        var data = Assert.IsType<ElevenLabsErrorData>(error.ErrorData);
        Assert.Equal("req_test_transcription", data.Detail!.RequestId);
    }

    private static HttpResponseMessage Error(HttpStatusCode status, string body)
    {
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
    }
}
