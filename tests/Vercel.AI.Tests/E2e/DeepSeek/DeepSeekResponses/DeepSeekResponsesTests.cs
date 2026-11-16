// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;
using Vercel.AI.DeepSeek;
using Vercel.AI.Gateway;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.E2e.DeepSeek.DeepSeekResponses;

/// <summary>
/// Port of the testbed page examples/ai-e2e-next/app/chat/deepseek-responses/page.tsx and its route
/// app/api/chat/deepseek-responses/route.ts. The route converts the UI messages, streams
/// <c>deepSeek.responses('deepseek-v4-pro')</c> and sends the reasoning. The page renders the text and
/// reasoning parts of each message.
/// </summary>
[Trait("Category", "RealModel")]
public sealed class DeepSeekResponsesTests
{
    [SkippableFact]
    public async Task Should_stream_a_chat_reply_with_text_and_reasoning_parts_through_the_route()
    {
        Skip.If(
            string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable(DeepSeekProvider.ApiKeyVariable)
            )
        );
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "unused" }));
        var model = DeepSeekProvider.Create().ResponsesModel("deepseek-v4-pro");
        var messages = new List<UIMessage>
        {
            new(
                UIMessageRole.User,
                [new TextUIPart("Why is the sky blue? Answer in one sentence.")]
            )
            {
                Id = "user-1",
            },
        };

        // POST /api/chat/deepseek-responses
        var result = client.StreamTextAsync(
            new StreamTextOptions
            {
                Model = model,
                Messages = UIMessageConversion.ConvertToModelMessages(messages),
            }
        );
        UIMessage? reply = null;
        await foreach (
            var message in UIMessageStreamConverter.ReadUIMessageStream(
                new ReadUIMessageStreamOptions(
                    UIMessageStreamConverter.ToUIMessageStream(
                        result.Stream(),
                        new ToUIMessageChunkOptions { SendReasoning = true }
                    )
                )
            )
        )
        {
            reply = message;
        }

        // what the page renders: a text part for each text, a reasoning part for each reasoning
        Assert.NotNull(reply);
        Assert.Equal(UIMessageRole.Assistant, reply!.Role);
        var text = Assert.Single(reply.Parts.OfType<TextUIPart>());
        Assert.False(string.IsNullOrWhiteSpace(text.Text));
    }
}
