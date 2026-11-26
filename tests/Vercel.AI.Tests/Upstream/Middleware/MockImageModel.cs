// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Middleware;

/// <summary>Maps to <c>MockImageModelV4</c>.</summary>
internal class MockImageModel : IImageCaller, IImageModel
{
    public string Provider { get; init; } = "mock-provider";

    public string ModelId { get; init; } = "mock-model-id";

    public int? MaxImagesPerCall { get; init; } = 1;

    public Func<ImageModelCall, Task<ImageModelResult>>? OnDoGenerate { get; init; }

    public List<ImageModelCall> DoGenerateCalls { get; } = [];

    public virtual Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken) =>
        Task.FromResult(MaxImagesPerCall);

    public Task<ImageModelResult> DoGenerateAsync(
        ImageModelCall call,
        CancellationToken cancellationToken
    )
    {
        DoGenerateCalls.Add(call);
        return OnDoGenerate is null
            ? Task.FromResult(new ImageModelResult([]))
            : OnDoGenerate(call);
    }

    Task<ImageGenerationResult> IImageModel.DoGenerateAsync(
        ImageCallOptions options,
        CancellationToken cancellationToken
    ) => throw new NotSupportedException();
}
