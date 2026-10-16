// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.KlingAI;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel;
using KlingAIProviderType = Vercel.AI.KlingAI.KlingAIProvider;
using KlingAIVideoModelType = Vercel.AI.KlingAI.KlingAIVideoModel;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIProvider;

/// <summary>Helpers for the <c>klingai-provider.test.ts</c> port. The vitest mocks of the model constructor are replaced by a recorded status request.</summary>
internal static class KlingAIProviderSupport
{
    /// <summary>The model id used by every upstream provider test.</summary>
    internal const string ModelId = "kling-v2.6-motion-control";

    /// <summary>Sends a status request through the provider's video model and returns what the server saw.</summary>
    internal static async Task<ParityCall> SendAsync(KlingAIVideoModelSupport.Server server, OpenAICompatibleOptions? options)
    {
        var provider = KlingAIProviderType.Create(options, server.Handler);
        await ((KlingAIVideoModelType)provider.Video(ModelId)).DoStatusAsync(KlingAIVideoModelSupport.Status(), CancellationToken.None).ConfigureAwait(false);
        return Assert.Single(server.Calls);
    }

    internal static async Task<ParityCall> SendAsync(OpenAICompatibleOptions? options)
    {
        return await SendAsync(new KlingAIVideoModelSupport.Server(), options).ConfigureAwait(false);
    }
}
