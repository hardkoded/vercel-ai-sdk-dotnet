// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Operations;
using BedrockImageModel = Vercel.AI.AmazonBedrock.AmazonBedrockImageModel;
using BedrockProvider = Vercel.AI.AmazonBedrock.AmazonBedrockProvider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockImageModel;

internal static class BedrockImageTestSupport
{
    public const string Prompt = "A cute baby sea otter";

    public const string ModelId = "amazon.nova-canvas-v1:0";

    public const string ImagesBody = "{\"images\":[\"base64-image-1\",\"base64-image-2\"]}";

    public static readonly byte[] PngBytes = { 137, 80, 78, 71 };

    public static readonly byte[] MaskBytes = { 255, 255, 255, 0 };

    public static BedrockProvider CreateProvider(
        UpstreamRecordingHandler handler,
        Action<AmazonBedrockOptions>? configure = null
    )
    {
        var options = new AmazonBedrockOptions { AccessKeyId = "AKIA", SecretAccessKey = "secret" };
        configure?.Invoke(options);
        return BedrockProvider.Create(options, handler);
    }

    public static BedrockImageModel Model(
        BedrockProvider provider,
        string modelId = ModelId,
        Func<DateTime>? currentDate = null
    ) => new(provider, modelId, currentDate);

    public static JsonElement Options(string json) => JsonDocument.Parse(json).RootElement.Clone();

    public static ImageModelCall Call(
        string? prompt = Prompt,
        IReadOnlyList<ImageModelFile>? files = null,
        ImageModelFile? mask = null,
        int n = 1,
        string? size = null,
        string? aspectRatio = null,
        int? seed = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null
    ) =>
        new(
            prompt,
            files,
            mask,
            n,
            size,
            aspectRatio,
            seed,
            Options(providerOptions),
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None
        );

    public static ImageModelFile File(byte[] data, string mediaType = "image/png") =>
        ImageModelFile.FromFile(data, mediaType);

    public static JsonNode? Body(UpstreamRecordingHandler handler) => JsonNode.Parse(handler.Body);
}
