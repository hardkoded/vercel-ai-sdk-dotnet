// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Replicate;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.ReplicateImageModel;

/// <summary>Port of the cancellation tests of <c>replicate-image-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string UpstreamCommit = "0d47a6cf8e46f8e3a0271a162de90b5175236cc2";
    private const string Id = "packages/replicate/src/replicate-image-model.test.ts::doGenerate::";
    private const string Prompt = "The Loch Ness monster getting a manicure";

    [Fact]
    [UpstreamTest(Id + "should cancel the prediction and rethrow the abort error when the caller aborts while polling", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_cancel_the_prediction_and_rethrow_the_abort_error_when_the_caller_aborts_while_polling()
    {
        using var controller = new CancellationTokenSource();
        var statusRequests = 0;
        var prediction = Prediction("aborted-prediction", "processing", "null", "https://api.replicate.com/v1/predictions/aborted-prediction");
        var handler = new ParityHandler(call =>
        {
            if (call.Method == HttpMethod.Get && Interlocked.Increment(ref statusRequests) == 2)
            {
                // abort while the call is polling
                controller.Cancel();
            }

            return ParityHandler.Json(prediction);
        });

        var model = Model(handler);
        var error = await Record(model.DoGenerateAsync(
            Call(new Dictionary<string, string> { ["idempotency-key"] = "create-key", ["x-per-call-header"] = "per-call-value" }, "{\"replicate\":{\"pollIntervalMillis\":1,\"maxPollAttempts\":10}}", controller.Token),
            controller.Token));

        // the original abort error is rethrown unchanged
        Assert.IsAssignableFrom<OperationCanceledException>(error);

        var cancelRequest = await WaitForCancel(handler);
        Assert.Equal("https://api.replicate.com/v1/predictions/aborted-prediction/cancel", cancelRequest.Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, cancelRequest.Method);
        Assert.Equal("Bearer test-api-token", cancelRequest.Header("authorization"));
        // the create call's per-call headers and idempotency key are not repeated
        Assert.Null(cancelRequest.Header("idempotency-key"));
        Assert.Null(cancelRequest.Header("x-per-call-header"));
    }

    [Fact]
    [UpstreamTest(Id + "should cancel the prediction and rethrow the timeout error when the maximum polling attempts are reached", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_cancel_the_prediction_and_rethrow_the_timeout_error_when_the_maximum_polling_attempts_are_reached()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(Prediction("timed-out-prediction", "processing", "null", "https://api.replicate.com/v1/predictions/timed-out-prediction")));

        var error = await Record(Model(handler).DoGenerateAsync(
            Call(new Dictionary<string, string> { ["idempotency-key"] = "create-key" }, "{\"replicate\":{\"pollIntervalMillis\":1,\"maxPollAttempts\":2}}", CancellationToken.None),
            CancellationToken.None));

        // the original timeout error is rethrown unchanged
        var exception = Assert.IsType<AiSdkException>(error);
        Assert.Equal("Replicate image generation did not complete after 2 polling attempts.", exception.Message);

        var cancelRequest = await WaitForCancel(handler);
        Assert.Equal("https://api.replicate.com/v1/predictions/timed-out-prediction/cancel", cancelRequest.Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, cancelRequest.Method);
        Assert.Equal("Bearer test-api-token", cancelRequest.Header("authorization"));
        Assert.Null(cancelRequest.Header("idempotency-key"));
    }

    [Fact]
    [UpstreamTest(Id + "should not send a cancel request when the prediction completes", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_not_send_a_cancel_request_when_the_prediction_completes()
    {
        var completed = Prediction("completed-prediction", "succeeded", "[\"https://replicate.delivery/xezq/abc/out-0.webp\"]", "https://api.replicate.com/v1/predictions/completed-prediction");
        var handler = new ParityHandler(call => call.Uri.Host == "replicate.delivery"
            ? ParityHandler.Bytes(Encoding.UTF8.GetBytes("test-binary-content"), "image/webp")
            : ParityHandler.Json(completed));

        await Model(handler).DoGenerateAsync(Call(null, "{\"replicate\":{\"maxPollAttempts\":10}}", CancellationToken.None), CancellationToken.None);

        // give a stray fire-and-forget cancel the chance to show up
        await Task.Delay(50);

        Assert.DoesNotContain(handler.Calls.ToArray(), call => call.Uri.AbsolutePath.EndsWith("/cancel", StringComparison.Ordinal));
    }

    private static string Prediction(string id, string status, string output, string getUrl)
    {
        return "{\"id\":\"" + id + "\",\"status\":\"" + status + "\",\"output\":" + output + ",\"error\":null,\"urls\":{\"get\":\"" + getUrl + "\"}}";
    }

    private static async Task<Exception> Record(Task task)
    {
        try
        {
            await task;
        }
        catch (Exception exception)
        {
            return exception;
        }

        throw new Xunit.Sdk.XunitException("Expected the call to throw.");
    }

    private static async Task<ParityCall> WaitForCancel(ParityHandler handler)
    {
        for (var i = 0; i < 200; i++)
        {
            var cancels = handler.Calls.ToArray().Where(call => call.Uri.AbsolutePath.EndsWith("/cancel", StringComparison.Ordinal)).ToList();
            if (cancels.Count > 0)
            {
                // a second cancel request would be a bug: give it the chance to show up
                await Task.Delay(50);
                return Assert.Single(handler.Calls.ToArray(), call => call.Uri.AbsolutePath.EndsWith("/cancel", StringComparison.Ordinal));
            }

            await Task.Delay(25);
        }

        throw new Xunit.Sdk.XunitException("No cancel request was sent.");
    }

    private static Vercel.AI.Replicate.ReplicateImageModel Model(ParityHandler handler)
    {
        return new Vercel.AI.Replicate.ReplicateImageModel(ReplicateProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-token" }, handler), "black-forest-labs/flux-schnell");
    }

    private static ImageModelCall Call(IReadOnlyDictionary<string, string>? headers, string providerOptions, CancellationToken cancellationToken)
    {
        using var options = JsonDocument.Parse(providerOptions);
        return new ImageModelCall(Prompt, null, null, 1, null, null, null, options.RootElement.Clone(), headers ?? new Dictionary<string, string>(), cancellationToken);
    }
}
