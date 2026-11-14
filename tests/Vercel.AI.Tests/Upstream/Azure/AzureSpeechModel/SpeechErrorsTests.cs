// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureSpeechModel;

/// <summary>Port of <c>azure-speech-model.test.ts</c> &gt; <c>Speech errors</c>.</summary>
public sealed class SpeechErrorsTests
{
    private static HttpResponseMessage Response(HttpStatusCode status, string? body, string mediaType = "text/plain")
    {
        var response = new HttpResponseMessage(status);
        if (body != null)
        {
            response.Content = new StringContent(body, Encoding.UTF8, mediaType);
        }

        return response;
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech errors::explains empty 400 responses", Coverage = UpstreamCoverage.Covered)]
    public async Task ExplainsEmpty400Responses()
    {
        var setup = AzureSpeechModelSetup.Create(null, () => Response(HttpStatusCode.BadRequest, null));
        var error = await Assert.ThrowsAsync<BadRequestException>(() =>
            setup.Provider.Speech("mai-voice-2").GenerateAsync(AzureSpeechModelSetup.Call("Hi"), CancellationToken.None));
        Assert.Equal(400, error.StatusCode);
        Assert.False(error.IsRetryable);
        Assert.Equal("Azure Speech request failed with status 400. Check the voice name, style, and output format.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech errors::uses JSON error messages", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesJsonErrorMessages()
    {
        var setup = AzureSpeechModelSetup.Create(
            null,
            () => Response(HttpStatusCode.Unauthorized, "{\"error\":{\"code\":\"401\",\"message\":\"Access denied\"}}", "application/json"));
        var error = await Assert.ThrowsAsync<AuthenticationException>(() =>
            setup.Provider.Speech("mai-voice-2").GenerateAsync(AzureSpeechModelSetup.Call("Hi"), CancellationToken.None));
        Assert.Equal(401, error.StatusCode);
        Assert.Equal("Access denied", error.Message);
    }

    [Theory]
    [InlineData("upstream connect error or disconnect/reset before headers. reset reason: protocol error")]
    [InlineData("upstream connect error or disconnect/reset before headers. reset reason: connection termination")]
    [InlineData("unavailable")]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech errors::keeps 502 responses retryable (%s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps502ResponsesRetryable(string body)
    {
        var setup = AzureSpeechModelSetup.Create(null, () => Response(HttpStatusCode.BadGateway, body));
        var error = await Assert.ThrowsAsync<InternalServerException>(() =>
            setup.Provider.Speech("mai-voice-2").GenerateAsync(AzureSpeechModelSetup.Call("Hi"), CancellationToken.None));
        Assert.Equal(502, error.StatusCode);
        Assert.True(error.IsRetryable);
        Assert.Equal("Azure Speech request failed with status 502.", error.Message);
        Assert.Equal(body, error.ResponseBody);
    }

    [Theory]
    [InlineData(429)]
    [InlineData(503)]
    [UpstreamTest("packages/azure/src/azure-speech-model.test.ts::Speech errors::keeps HTTP %s retryable", Coverage = UpstreamCoverage.Covered)]
    public async Task KeepsHttpRetryable(int status)
    {
        var setup = AzureSpeechModelSetup.Create(null, () => Response((HttpStatusCode)status, "unavailable"));
        var error = await Assert.ThrowsAnyAsync<ApiException>(() =>
            setup.Provider.Speech("mai-voice-2").GenerateAsync(AzureSpeechModelSetup.Call("Hi"), CancellationToken.None));
        Assert.Equal(status, error.StatusCode);
        Assert.True(error.IsRetryable);
    }
}
