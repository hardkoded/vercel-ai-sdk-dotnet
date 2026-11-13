// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tencent;
using Vercel.AI.Tests.MoreProviders;
using TencentProviderType = Vercel.AI.Tencent.TencentProvider;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentProvider;

/// <summary>Helpers for the <c>tencent-provider.test.ts</c> port.</summary>
internal static class TencentProviderSupport
{
    internal const string Commit = "1c7a311d5384a6741daade550e4d3a7f9253d71e";
    internal const string UpstreamFile = "packages/tencent/src/tencent-provider.test.ts";

    private const string ChatBody = """{"id":"chatcmpl-1","object":"chat.completion","created":1794120000,"model":"hy4","choices":[{"index":0,"message":{"role":"assistant","content":"Hello"},"finish_reason":"stop"}],"usage":{"prompt_tokens":3,"completion_tokens":1,"total_tokens":4}}""";

    /// <summary>The upstream <c>createTestServer</c> with its three URLs.</summary>
    internal static ParityHandler Server()
    {
        return new ParityHandler(call => call.Uri.AbsoluteUri switch
        {
            "https://tokenhub-intl.tencentcloudmaas.com/v1/chat/completions" => ParityHandler.Json(ChatBody),
            "https://custom.example.com/v1/chat/completions" => ParityHandler.Json(ChatBody),
            "https://vod.intl.tencentcloudapi.com/" => ParityHandler.Json("""{"Response":{"TaskId":"task-1","RequestId":"req-1"}}"""),
            _ => throw new InvalidOperationException("Unexpected URL " + call.Uri),
        });
    }

    /// <summary>The upstream <c>prompt</c>.</summary>
    internal static LanguageModelCallOptions ChatPrompt()
    {
        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hi") } };
    }

    /// <summary>The upstream <c>startVideo</c> with <c>videoOptions</c>.</summary>
    internal static Task<VideoStartResult> StartVideo(IVideoModel model)
    {
        return ((IVideoCaller)model).DoStartAsync(new VideoModelCall("A cat", 1, null, null, null, null, null, null, null, null, null, OperationJson.Parse("{}"), new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);
    }

    internal static TencentProviderType Create(TencentOptions options, ParityHandler handler) => TencentProviderType.Create(options, handler);
}
