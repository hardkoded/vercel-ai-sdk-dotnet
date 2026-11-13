// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Replicate;
using ReplicateApiProvider = Vercel.AI.Replicate.ReplicateProvider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using ImageModel = Vercel.AI.Replicate.ReplicateImageModel;
using static Vercel.AI.Tests.Upstream.Replicate.ReplicateImageModel.ReplicateImageModelSupport;

namespace Vercel.AI.Tests.Upstream.Replicate.ReplicateImageModel;

/// <summary>Port of <c>replicate-image-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix = "packages/replicate/src/replicate-image-model.test.ts::doGenerate::";

    [Fact]
    [UpstreamTest(Prefix + "should pass the model and the settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Body_has_the_input_settings_and_replicate_options()
    {
        var handler = Server();
        await Generate(handler, Call(size: "1024x768", aspectRatio: "3:4", seed: 123, providerOptions: "{\"replicate\":{\"style\":\"realistic_image\"},\"other\":{\"something\":\"else\"}}"));
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"prompt\":\"The Loch Ness monster getting a manicure\",\"num_outputs\":1,\"aspect_ratio\":\"3:4\",\"size\":\"1024x768\",\"seed\":123,\"style\":\"realistic_image\"}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should call the correct url", Coverage = UpstreamCoverage.Covered)]
    public async Task Unversioned_models_post_to_the_model_predictions_route()
    {
        var handler = Server();
        await Generate(handler, Call());
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Equal("https://api.replicate.com/v1/models/black-forest-labs/flux-schnell/predictions", handler.Calls[0].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers and set the prefer header", Coverage = UpstreamCoverage.Covered)]
    public async Task Request_sends_bearer_custom_headers_and_prefer_wait()
    {
        var handler = Server();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-token" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = ReplicateApiProvider.Create(options, handler).ImageModel("black-forest-labs/flux-schnell");
        await ((ImageModel)model).DoGenerateAsync(Call(headers: new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }), CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal("Bearer test-api-token", call.Header("Authorization"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Equal("wait", call.Header("Prefer"));
        Assert.Contains("ai-sdk/replicate/" + AiSdkVersion.Version, call.Header("User-Agent") ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should set custom wait time in prefer header when maxWaitTimeInSeconds is specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Max_wait_time_sets_the_prefer_wait_duration()
    {
        var handler = Server();
        await Generate(handler, Call(providerOptions: "{\"replicate\":{\"maxWaitTimeInSeconds\":120}}"));
        Assert.Equal("wait=120", handler.Calls[0].Header("Prefer"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should not include maxWaitTimeInSeconds in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Call_settings_are_not_sent_as_input()
    {
        var handler = Server();
        await Generate(handler, Call(providerOptions: "{\"replicate\":{\"maxWaitTimeInSeconds\":120,\"pollIntervalMillis\":10,\"maxPollAttempts\":100,\"guidance_scale\":7.5}}"));
        var input = JsonNode.Parse(handler.Calls[0].Text)!["input"]!.AsObject();
        Assert.False(input.ContainsKey("maxWaitTimeInSeconds"));
        Assert.False(input.ContainsKey("pollIntervalMillis"));
        Assert.False(input.ContainsKey("maxPollAttempts"));
        Assert.Equal(7.5, input["guidance_scale"]!.GetValue<double>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract the generated image from array response", Coverage = UpstreamCoverage.Covered)]
    public async Task Array_output_is_downloaded()
    {
        var handler = Server();
        var result = await Generate(handler, Call());
        Assert.Equal(ImageBytes, (byte[])Assert.Single(result.Images)!);
        Assert.Equal(HttpMethod.Get, handler.Calls[1].Method);
        Assert.Equal(OutputUrl, handler.Calls[1].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract the generated image from string response", Coverage = UpstreamCoverage.Covered)]
    public async Task String_output_is_downloaded()
    {
        var handler = Server(Prediction("\"" + OutputUrl + "\""));
        var result = await Generate(handler, Call());
        Assert.Equal(ImageBytes, (byte[])Assert.Single(result.Images)!);
        Assert.Equal(HttpMethod.Get, handler.Calls[1].Method);
        Assert.Equal(OutputUrl, handler.Calls[1].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Prefix + "should poll until output is available when the sync wait expires", Coverage = UpstreamCoverage.Covered)]
    public async Task Pending_predictions_are_polled_with_credentials()
    {
        var predictions = 0;
        var handler = new ParityHandler(call =>
        {
            if (call.Uri.AbsoluteUri == OutputUrl)
            {
                return ParityHandler.Bytes(ImageBytes, "image/webp");
            }

            var number = predictions++;
            var status = number == 0 ? "starting" : number == 1 ? "processing" : "succeeded";
            return ParityHandler.Json(Pending(status, number < 2 ? "null" : "[\"" + OutputUrl + "\"]", PollUrl));
        });
        var result = await Generate(handler, Call(providerOptions: "{\"replicate\":{\"pollIntervalMillis\":1,\"maxPollAttempts\":100}}"));
        Assert.Equal(ImageBytes, (byte[])Assert.Single(result.Images)!);
        Assert.Equal(new[] { HttpMethod.Post, HttpMethod.Get, HttpMethod.Get, HttpMethod.Get }, handler.Calls.Select(call => call.Method));
        Assert.Equal(PollUrl, handler.Calls[1].Uri.AbsoluteUri);
        Assert.Equal("Bearer test-api-token", handler.Calls[1].Header("Authorization"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should not send credentials to a polling URL on a different origin", Coverage = UpstreamCoverage.Covered)]
    public async Task Polls_on_another_origin_omit_credentials()
    {
        var otherOrigin = "https://example.com/predictions/pending-prediction";
        var handler = new ParityHandler(call => call.Uri.AbsoluteUri switch
        {
            OutputUrl => ParityHandler.Bytes(ImageBytes, "image/webp"),
            "https://example.com/predictions/pending-prediction" => ParityHandler.Json(Pending("succeeded", "[\"" + OutputUrl + "\"]", otherOrigin)),
            _ => ParityHandler.Json(Pending("starting", "null", otherOrigin)),
        });
        await Generate(handler, Call(providerOptions: "{\"replicate\":{\"pollIntervalMillis\":1,\"maxPollAttempts\":100}}"));
        Assert.Equal(otherOrigin, handler.Calls[1].Uri.AbsoluteUri);
        Assert.Null(handler.Calls[1].Header("Authorization"));
    }

    [Theory]
    [InlineData("failed")]
    [InlineData("canceled")]
    [UpstreamTest(Prefix + "should throw when polling returns %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Failed_or_canceled_polls_throw(string status)
    {
        var predictions = 0;
        var handler = new ParityHandler(_ =>
        {
            var first = predictions++ == 0;
            return ParityHandler.Json("{\"id\":\"pending-prediction\",\"status\":\"" + (first ? "starting" : status) + "\",\"output\":null,\"error\":" + (first ? "null" : "\"Prediction did not complete\"") + ",\"urls\":{\"get\":\"" + PollUrl + "\"}}");
        });
        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => Generate(handler, Call(providerOptions: "{\"replicate\":{\"pollIntervalMillis\":1,\"maxPollAttempts\":100}}")));
        Assert.Equal("Replicate image generation " + status + ": Prediction did not complete", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw when a succeeded prediction has no output", Coverage = UpstreamCoverage.Covered)]
    public async Task A_succeeded_prediction_without_output_throws()
    {
        var handler = Server(Pending("succeeded", "null", "https://api.replicate.com/v1/predictions/completed-prediction"));
        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() => Generate(handler, Call()));
        Assert.Equal("Replicate image generation completed without output.", error.Message);
        Assert.Single(handler.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should stop when the maximum polling attempts are reached", Coverage = UpstreamCoverage.Covered)]
    public async Task Polling_stops_after_the_maximum_attempts()
    {
        var handler = Server(Pending("processing", "null", PollUrl));
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Generate(handler, Call(providerOptions: "{\"replicate\":{\"pollIntervalMillis\":1,\"maxPollAttempts\":2}}")));
        Assert.Equal("Replicate image generation did not complete after 2 polling attempts.", error.Message);
        await WaitForAsync(() => handler.Calls.Count == 4);
        Assert.Equal(HttpMethod.Post, handler.Calls[3].Method);
        Assert.Equal("https://api.replicate.com/v1/predictions/pending-prediction/cancel", handler.Calls[3].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Prefix + "should cancel the prediction and rethrow the abort error when the caller aborts while polling", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_the_prediction_and_rethrows_the_abort_error_when_the_caller_aborts_while_polling()
    {
        using var cancellation = new CancellationTokenSource();
        var prediction = Pending("processing", "null", "https://api.replicate.com/v1/predictions/aborted-prediction").Replace("pending-prediction", "aborted-prediction", StringComparison.Ordinal);
        var statusRequests = 0;
        var handler = new ParityHandler(call =>
        {
            if (call.Method == HttpMethod.Get && Interlocked.Increment(ref statusRequests) == 2)
            {
                // abort while the call is polling
                cancellation.Cancel();
            }

            return ParityHandler.Json(prediction);
        });
        var model = Model(handler, "black-forest-labs/flux-schnell");

        var error = await Record.ExceptionAsync(() => model.DoGenerateAsync(
            Call(headers: new Dictionary<string, string> { ["idempotency-key"] = "create-key", ["x-per-call-header"] = "per-call-value" }, providerOptions: "{\"replicate\":{\"pollIntervalMillis\":1,\"maxPollAttempts\":10}}"),
            cancellation.Token));

        // the original abort error is rethrown unchanged
        Assert.IsAssignableFrom<OperationCanceledException>(error);

        await WaitForAsync(() => CancelCalls(handler).Length == 1);
        var cancelRequest = Assert.Single(CancelCalls(handler));
        Assert.Equal("https://api.replicate.com/v1/predictions/aborted-prediction/cancel", cancelRequest.Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, cancelRequest.Method);
        Assert.Equal("Bearer test-api-token", cancelRequest.Header("Authorization"));
        // the create call's per-call headers and idempotency key are not repeated
        Assert.Null(cancelRequest.Header("idempotency-key"));
        Assert.Null(cancelRequest.Header("x-per-call-header"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should cancel the prediction and rethrow the timeout error when the maximum polling attempts are reached", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_the_prediction_and_rethrows_the_timeout_error_when_the_maximum_polling_attempts_are_reached()
    {
        var prediction = Pending("processing", "null", "https://api.replicate.com/v1/predictions/timed-out-prediction").Replace("pending-prediction", "timed-out-prediction", StringComparison.Ordinal);
        var handler = new ParityHandler(_ => ParityHandler.Json(prediction));

        var error = await Assert.ThrowsAsync<AiSdkException>(() => Generate(handler, Call(
            headers: new Dictionary<string, string> { ["idempotency-key"] = "create-key" },
            providerOptions: "{\"replicate\":{\"pollIntervalMillis\":1,\"maxPollAttempts\":2}}")));

        // the original timeout error is rethrown unchanged
        Assert.Equal("Replicate image generation did not complete after 2 polling attempts.", error.Message);

        await WaitForAsync(() => CancelCalls(handler).Length == 1);
        var cancelRequest = Assert.Single(CancelCalls(handler));
        Assert.Equal("https://api.replicate.com/v1/predictions/timed-out-prediction/cancel", cancelRequest.Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, cancelRequest.Method);
        Assert.Equal("Bearer test-api-token", cancelRequest.Header("Authorization"));
        Assert.Null(cancelRequest.Header("idempotency-key"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should not send a cancel request when the prediction completes", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_send_a_cancel_request_when_the_prediction_completes()
    {
        var completed = "{\"id\":\"completed-prediction\",\"status\":\"succeeded\",\"output\":[\"" + OutputUrl + "\"],\"error\":null,\"urls\":{\"get\":\"https://api.replicate.com/v1/predictions/completed-prediction\"}}";
        var handler = Server(completed);

        await Generate(handler, Call(providerOptions: "{\"replicate\":{\"maxPollAttempts\":10}}"));

        // give a stray fire-and-forget cancel the chance to show up
        await Task.Delay(10);

        Assert.Empty(CancelCalls(handler));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Response_has_timestamp_model_id_and_headers()
    {
        var date = new DateTime(2024, 1, 1);
        var result = await Generate(Server(), Call(), () => date, "https://api.replicate.com");
        Assert.Equal(date, result.Response.Timestamp);
        Assert.Equal("black-forest-labs/flux-schnell", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include response headers in metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Response_headers_come_from_the_prediction_response()
    {
        var date = new DateTime(2024, 1, 1);
        var handler = new ParityHandler(call => call.Uri.AbsoluteUri == OutputUrl
            ? ParityHandler.Bytes(ImageBytes, "image/webp")
            : ParityHandler.Json(Prediction("[\"" + OutputUrl + "\"]"), ("custom-response-header", "response-header-value")));
        var result = await Generate(handler, Call(), () => date, "https://api.replicate.com");
        Assert.Equal(date, result.Response.Timestamp);
        Assert.Equal("black-forest-labs/flux-schnell", result.Response.ModelId);
        Assert.Equal("application/json", result.Response.Headers!["content-type"]);
        Assert.Equal("response-header-value", result.Response.Headers["custom-response-header"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should set version in request body for versioned models", Coverage = UpstreamCoverage.Covered)]
    public async Task Versioned_models_post_the_version_to_predictions()
    {
        var handler = Server();
        await Generate(handler, Call(), modelId: "bytedance/sdxl-lightning-4step:5599ed30703defd1d160a25a63321b4dec97101d98b4674bcc56e41f62f35637");
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Equal("https://api.replicate.com/v1/predictions", handler.Calls[0].Uri.AbsoluteUri);
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"input\":{\"prompt\":\"The Loch Ness monster getting a manicure\",\"num_outputs\":1},\"version\":\"5599ed30703defd1d160a25a63321b4dec97101d98b4674bcc56e41f62f35637\"}");
    }
}
