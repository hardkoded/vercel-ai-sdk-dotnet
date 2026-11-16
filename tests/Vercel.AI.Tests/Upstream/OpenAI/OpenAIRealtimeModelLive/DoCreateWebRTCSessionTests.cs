// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Live = Vercel.AI.OpenAI.OpenAIRealtimeModelLive;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeModelLive;

/// <summary>Port of <c>openai-realtime-model-live.test.ts</c> &gt; <c>OpenAIRealtimeModelLive</c> &gt; <c>doCreateWebRTCSession</c>.</summary>
public sealed class DoCreateWebRTCSessionTests
{
    private const string Prefix = "packages/openai/src/live/openai-realtime-model-live.test.ts::OpenAIRealtimeModelLive > doCreateWebRTCSession::";

    private const string Success = "{\"session\":{\"id\":\"session-1\"},\"transport\":{\"type\":\"webrtc\",\"sdp\":\"answer-sdp\"}}";

    private static OpenAICapture SuccessCapture()
    {
        return new OpenAICapture { ResponseJson = Success };
    }

    [Fact]
    [UpstreamTest(Prefix + "exchanges JSON SDP, omits audio.format, and forwards headers and abort signal", Coverage = UpstreamCoverage.Covered)]
    public async Task Exchanges_JSON_SDP_omits_audio_format_and_forwards_headers_and_abort_signal()
    {
        var capture = SuccessCapture();
        var model = OpenAIRealtimeModelLiveTests.Create(
            options =>
            {
                options.BaseUrl = "https://example.com/proxy/v1";
                options.Organization = "org-test";
                options.Project = "proj-test";
                options.Headers["X-Custom"] = "value";
            },
            capture);
        var config = new RealtimeModelV4SessionConfig
        {
            Instructions = "Hello",
            ProviderOptions = new JsonObject { ["openai"] = new OpenAIRealtimeModelLiveOptions { ClientDelegation = true }.ToJsonObject() },
        };

        var session = await model.CreateWebRTCSessionAsync("offer-sdp", config, CancellationToken.None);

        Assert.Equal("session-1", session.SessionId);
        Assert.Equal("answer-sdp", session.Sdp);
        Assert.Equal("https://example.com/proxy/v1/live/sessions", capture.Uri);
        Assert.Equal("POST", capture.Method);
        Assert.StartsWith("application/json", capture.Headers["content-type"], StringComparison.Ordinal);
        Assert.Equal("Bearer test-key", capture.Headers["authorization"]);
        Assert.Equal("org-test", capture.Headers["openai-organization"]);
        Assert.Equal("proj-test", capture.Headers["openai-project"]);
        Assert.Equal("value", capture.Headers["x-custom"]);
        OpenAIUpstream.Equal(
            JsonNode.Parse(capture.Body),
            "{\"session\":{\"model\":\"gpt-live-1\",\"instructions\":\"Hello\",\"audio\":{\"output\":{\"voice\":\"marin\"}},\"delegation\":{\"type\":\"client\"}},\"transport\":{\"type\":\"webrtc\",\"sdp\":\"offer-sdp\"}}");

        // The caller's cancellation token reaches the request.
        using var controller = new CancellationTokenSource();
        var blocking = new BlockingHandler();
        var provider = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-key" }, blocking);
        var operation = ((Live)provider.RealtimeModel("gpt-live-1")).CreateWebRTCSessionAsync("offer-sdp", null, controller.Token);
        var observed = operation.ContinueWith(task => task.IsFaulted || task.IsCanceled, TaskScheduler.Default);
        var signal = await blocking.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        controller.Cancel();

        Assert.True(await observed.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.True(signal.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts omitted session config", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_omitted_session_config()
    {
        var capture = SuccessCapture();

        await OpenAIRealtimeModelLiveTests.Create(capture: capture).CreateWebRTCSessionAsync("offer", null, CancellationToken.None);

        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body)!["session"], "{\"model\":\"gpt-live-1\",\"audio\":{\"output\":{\"voice\":\"marin\"}}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects Responses delegation before an RTC creation request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_Responses_delegation_before_an_RTC_creation_request()
    {
        var capture = SuccessCapture();
        var config = new RealtimeModelV4SessionConfig
        {
            ProviderOptions = new JsonObject
            {
                ["openai"] = JsonNode.Parse("{\"delegation\":{\"type\":\"responses\",\"responses\":{\"model\":\"test-model\"}}}"),
            },
        };

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(
            () => OpenAIRealtimeModelLiveTests.Create(capture: capture).CreateWebRTCSessionAsync("offer", config, CancellationToken.None));

        Assert.Equal(0, capture.Calls);
    }

    [Theory]
    [InlineData("inputAudioFormat")]
    [InlineData("outputAudioFormat")]
    [UpstreamTest(Prefix + "rejects explicit %s before fetching", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_explicit_format_before_fetching(string field)
    {
        var capture = SuccessCapture();
        var config = field == "inputAudioFormat"
            ? new RealtimeModelV4SessionConfig { InputAudioFormatType = "audio/pcm", InputAudioFormatRate = 24000 }
            : new RealtimeModelV4SessionConfig { OutputAudioFormatType = "audio/pcm", OutputAudioFormatRate = 24000 };

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(
            () => OpenAIRealtimeModelLiveTests.Create(capture: capture).CreateWebRTCSessionAsync("offer", config, CancellationToken.None));

        Assert.Equal(0, capture.Calls);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"session\":{},\"transport\":{\"type\":\"webrtc\",\"sdp\":\"answer\"}}")]
    [InlineData("{\"session\":{\"id\":\"session-1\"},\"transport\":{\"type\":\"websocket\",\"sdp\":\"answer\"}}")]
    [InlineData("{\"session\":{\"id\":\"session-1\"},\"transport\":{\"type\":\"webrtc\",\"sdp\":\"\"}}")]
    [UpstreamTest(Prefix + "rejects malformed successful response %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_malformed_successful_response(string body)
    {
        var capture = new OpenAICapture { ResponseJson = body };

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => OpenAIRealtimeModelLiveTests.Create(capture: capture).CreateWebRTCSessionAsync("offer", null, CancellationToken.None));

        Assert.Equal("Invalid JSON response", exception.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "reports HTTP errors through the standard API error handler", Coverage = UpstreamCoverage.Covered)]
    public async Task Reports_HTTP_errors_through_the_standard_API_error_handler()
    {
        var capture = new OpenAICapture
        {
            Status = HttpStatusCode.BadRequest,
            ResponseJson = "{\"error\":{\"message\":\"Session creation rejected\",\"code\":null}}",
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => OpenAIRealtimeModelLiveTests.Create(capture: capture).CreateWebRTCSessionAsync("offer", null, CancellationToken.None));

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("Session creation rejected", exception.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves abort errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_abort_errors()
    {
        using var controller = new CancellationTokenSource();
        controller.Cancel();
        var error = new OperationCanceledException("Aborted", controller.Token);
        var provider = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-key" }, new ThrowingHandler(error));
        var model = (Live)provider.RealtimeModel("gpt-live-1");

        var thrown = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => model.CreateWebRTCSessionAsync("offer", null, controller.Token));

        // The abort surfaces as a cancellation that carries the original error.
        Assert.True(ReferenceEquals(error, thrown) || ReferenceEquals(error, thrown.InnerException));
    }

    private sealed class BlockingHandler : HttpMessageHandler
    {
        public TaskCompletionSource<CancellationToken> Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Started.TrySetResult(cancellationToken);
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException("unreachable");
        }
    }

    private sealed class ThrowingHandler(Exception error) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromException<HttpResponseMessage>(error);
        }
    }
}
