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
using ImageModel = Vercel.AI.Replicate.ReplicateImageModel;
using ReplicateApiProvider = Vercel.AI.Replicate.ReplicateProvider;

namespace Vercel.AI.Tests.Upstream.Replicate.ReplicateImageModel;

/// <summary>Shared fixtures for the ports of <c>replicate-image-model.test.ts</c>.</summary>
internal static class ReplicateImageModelSupport
{
    public const string Prompt = "The Loch Ness monster getting a manicure";
    public const string OutputUrl = "https://replicate.delivery/xezq/abc/out-0.webp";
    public const string PollUrl = "https://api.replicate.com/v1/predictions/pending-prediction";
    public static readonly byte[] ImageBytes = Encoding.UTF8.GetBytes("test-binary-content");

    public static string Prediction(string output)
    {
        return "{\"id\":\"s7x1e3dcmhrmc0cm8rbatcneec\",\"model\":\"black-forest-labs/flux-schnell\",\"version\":\"dp-4d0bcc010b3049749a251855f12800be\",\"input\":{\"num_outputs\":1,\"prompt\":\"The Loch Ness Monster getting a manicure\"},"
            + "\"logs\":\"\",\"output\":" + output + ",\"data_removed\":false,\"error\":null,\"status\":\"processing\",\"created_at\":\"2025-01-08T13:24:38.692Z\","
            + "\"urls\":{\"get\":\"https://api.replicate.com/v1/predictions/s7x1e3dcmhrmc0cm8rbatcneec\",\"stream\":\"https://stream.replicate.com/v1/files/bcwr-3okdfv3o2wehstv5f2okyftwxy57hhypqsi6osiim5iaq5k7u24a\"}}";
    }

    public static string Pending(string status, string output, string getUrl)
    {
        return "{\"id\":\"pending-prediction\",\"status\":\"" + status + "\",\"output\":" + output + ",\"error\":null,\"urls\":{\"get\":\"" + getUrl + "\"}}";
    }

    /// <summary>Waits until <paramref name="condition"/> holds, up to five seconds.</summary>
    public static async Task WaitForAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(5);
        }

        Assert.True(condition(), "The condition was not met within five seconds.");
    }

    /// <summary>Calls that went to a prediction cancel endpoint.</summary>
    public static ParityCall[] CancelCalls(ParityHandler handler)
    {
        lock (handler.Calls)
        {
            return handler.Calls.Where(call => call.Uri.AbsolutePath.EndsWith("/cancel", StringComparison.Ordinal)).ToArray();
        }
    }

    public static ParityHandler Server(string? prediction = null)
    {
        var body = prediction ?? Prediction("[\"" + OutputUrl + "\"]");
        return new ParityHandler(call => call.Uri.AbsoluteUri == OutputUrl ? ParityHandler.Bytes(ImageBytes, "image/webp") : ParityHandler.Json(body));
    }

    public static ImageModel Model(ParityHandler handler, string modelId, Func<DateTime>? clock = null, string? baseUrl = null)
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-token" };
        if (baseUrl != null)
        {
            options.BaseUrl = baseUrl;
        }

        return new ImageModel(ReplicateApiProvider.Create(options, handler), modelId, clock);
    }

    public static Task<ImageModelResult> Generate(ParityHandler handler, ImageModelCall call, Func<DateTime>? clock = null, string? baseUrl = null, string modelId = "black-forest-labs/flux-schnell")
    {
        return Model(handler, modelId, clock, baseUrl).DoGenerateAsync(call, CancellationToken.None);
    }

    public static ImageModelCall Call(string prompt = Prompt, string? size = null, string? aspectRatio = null, int? seed = null, string? providerOptions = null, IReadOnlyList<ImageModelFile>? files = null, ImageModelFile? mask = null, IReadOnlyDictionary<string, string>? headers = null)
    {
        using var options = JsonDocument.Parse(providerOptions ?? "{}");
        return new ImageModelCall(prompt, files, mask, 1, size, aspectRatio, seed, options.RootElement.Clone(), headers ?? new Dictionary<string, string>(), CancellationToken.None);
    }
}
