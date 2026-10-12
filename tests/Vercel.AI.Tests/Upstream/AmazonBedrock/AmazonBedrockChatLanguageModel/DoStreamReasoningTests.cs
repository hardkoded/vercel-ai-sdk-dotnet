// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.AmazonBedrock.BedrockStreamSupport;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModel;

public sealed class DoStreamReasoningTests
{
    private const string Suite = "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::doStream > reasoning::";

    [Fact]
    [UpstreamTest(Suite + "should stream reasoning and text parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_reasoning_and_text_parts()
    {
        var parts = await Stream(Fixture("reasoning"));

        const string Signature = "Ep0CCkgICxABGAIqQOWPB6/PmA5SW9jC6FvaNq3E+U9ev4FMWcFWuAho+VGLCtazKc5WDjQ5i0MuxsY0o5pKDSVWVKii8KJDusXH4eASDK7jyzuk8iij7fJNihoMxHO9haYzt48R36HVIjCb/EmIFrJLIXShqN6DN//T6vZBtO9qj1QhNWJa3CGm8VZoq80S2/Ok4U0aVaIDiZcqggHC2b8BHuv8BHZrmsR0wjU1ynansBGMdfjnG+iIv8R5lPpRmYGhSVwNybwP3aQZ6o8Dr48Rau8TJfdsArW+r7bvL7bPs4f5nnlp2vG7WkMzWwABHK3fdM44zZ1GZQaWyECNWR2GfY6dXiklo94vgpFTPuZ97mfiN3LY6uYyBwL8RkDaGAE=";
        Assert.Equal(
            new[]
            {
                Start,
                Meta,
                "reasoning-start 0",
                "reasoning-delta 0 \"Let me count the r\"",
                "reasoning-delta 0 \"'s in \\\"\"",
                "reasoning-delta 0 \"strawberry\\\":\"",
                "reasoning-delta 0 \"\\n\\ns-t-r-a\"",
                "reasoning-delta 0 \"-w-b-e-r\"",
                "reasoning-delta 0 \"-r-y\\n\\nr\"",
                "reasoning-delta 0 \" appears at positions \"",
                "reasoning-delta 0 \"3, 8\"",
                "reasoning-delta 0 \", and 9.\\n\\nSo there\"",
                "reasoning-delta 0 \" are 3 r's.\"",
                "reasoning-delta 0 \"\" meta={\"amazonBedrock\":{\"signature\":\"" + Signature + "\"},\"bedrock\":{\"signature\":\"" + Signature + "\"}}",
                "reasoning-end 0",
                "text-start 1",
                "text-delta 1 \"There\"",
                "text-delta 1 \" are **3** r's in \\\"\"",
                "text-delta 1 \"strawberry\\\":\"",
                "text-delta 1 \"\\n\\n1\"",
                "text-delta 1 \". st\"",
                "text-delta 1 \"**\"",
                "text-delta 1 \"r**awbe\"",
                "text-delta 1 \"**r****\"",
                "text-delta 1 \"r**y\"",
                "text-end 1",
                "finish stop raw=end_turn usage=in(51,51,0,0) out(94,94,undefined) raw={\"inputTokens\":51,\"outputTokens\":94,\"serverToolUsage\":{},\"totalTokens\":145}",
            },
            Render(parts));
    }
}
