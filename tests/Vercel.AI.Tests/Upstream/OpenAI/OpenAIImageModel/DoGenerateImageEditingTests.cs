// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http;
using Vercel.AI.OpenAI;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIImageModel;

/// <summary>Port of <c>openai-image-model.test.ts</c> &gt; <c>doGenerate - image editing</c>.</summary>
public sealed class DoGenerateImageEditingTests
{
    [Theory]
    [InlineData("image")]
    [InlineData("mask")]
    [InlineData("markup")]
    [UpstreamTest(
        "packages/openai/src/image/openai-image-model.test.ts::doGenerate - image editing::should use the configured fetch and abort signal when downloading a URL %s",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port sends untrusted downloads through OpenAICompatibleOptions.DownloadHttpClient, not through the provider HttpClient, so this checks that client and the cancellation token.",
        Commit = "2ebc78b65435926718879f85a1a85687b053c542")]
    public async Task Uses_the_configured_download_client_and_abort_signal_when_downloading_a_url(string downloadTarget)
    {
        using var controller = new CancellationTokenSource();
        var downloadHandler = new BlockingHandler();
        var capture = new OpenAICapture { ResponseJson = "{\"data\":[{\"b64_json\":\"AQID\"}]}" };
        var png = new byte[] { 137, 80, 78, 71 };
        var model = new Vercel.AI.OpenAI.OpenAIImageModel(
            OpenAIUpstream.Provider(capture, options => options.DownloadHttpClient = new HttpClient(downloadHandler, disposeHandler: false)),
            "gpt-image-1");
        var call = new OpenAIImageCall("edit")
        {
            Files = downloadTarget == "image"
                ? new[] { OpenAIImageFile.FromUrl("https://example.com/image.png") }
                : new[] { new OpenAIImageFile(png, "image/png", "image.png") },
            Mask = downloadTarget == "mask" ? OpenAIImageFile.FromUrl("https://example.com/mask.png") : null,
            ProviderOptions = downloadTarget == "markup" ? OpenAIUpstream.Json("{\"openai\":{\"markup\":\"https://example.com/markup.png\"}}") : null,
        };

        var operation = model.GenerateAsync(call, controller.Token);
        var observed = operation.ContinueWith(task => task.IsFaulted || task.IsCanceled, TaskScheduler.Default);
        var signal = await downloadHandler.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        controller.Cancel();

        Assert.True(await observed.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.True(signal.IsCancellationRequested);
        Assert.Equal(0, capture.Calls);
    }

    private sealed class BlockingHandler : HttpMessageHandler
    {
        public TaskCompletionSource<CancellationToken> Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Started.TrySetResult(cancellationToken);
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
