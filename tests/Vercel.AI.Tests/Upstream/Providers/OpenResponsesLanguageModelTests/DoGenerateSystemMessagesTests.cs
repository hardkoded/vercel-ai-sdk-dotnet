// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; system messages</c>.</summary>
public sealed class DoGenerateSystemMessagesTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > system messages::";

    [Fact]
    [UpstreamTest(Prefix + "should send instructions from system message", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_instructions_from_system_message()
    {
        var capture = await Send(new SystemModelMessage("You are a helpful assistant."), new UserModelMessage("Hello"));

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],"instructions":"You are a helpful assistant.","model":"gemma-7b-it"}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should join multiple system messages with newlines", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_join_multiple_system_messages_with_newlines()
    {
        var capture = await Send(new SystemModelMessage("You are a helpful assistant."), new SystemModelMessage("Always be concise."), new UserModelMessage("Hello"));

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],"instructions":"You are a helpful assistant.\nAlways be concise.","model":"gemma-7b-it"}
            """);
    }

    private static async Task<UpstreamCapture> Send(params ModelMessage[] prompt)
    {
        var capture = JsonFixture("lmstudio-basic.1");
        await Model(capture).DoGenerateAsync(new LanguageModelCallOptions { Prompt = prompt }, CancellationToken.None);
        return capture;
    }
}
