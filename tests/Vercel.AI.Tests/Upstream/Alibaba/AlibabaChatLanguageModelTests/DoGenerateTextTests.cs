// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Port of <c>alibaba-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; text</c>.</summary>
public sealed class DoGenerateTextTests
{
    private const string Upstream = "packages/alibaba/src/alibaba-chat-language-model.test.ts::doGenerate > text::";

    [Fact]
    [UpstreamTest(Upstream + "should extract text content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_text_content()
    {
        var capture = CaptureFixture("alibaba-text");
        var model = Model(capture);

        var result = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        var expected = JsonNode.Parse(Fixture("alibaba-text"))!["choices"]![0]!["message"]!["content"]!.GetValue<string>();
        var part = Assert.Single(result.Content);
        Assert.Equal(expected, Assert.IsType<GeneratedText>(part).Text);
        AssertResultSnapshot(result, capture, "alibaba-text", "stop", FinishReason.Stop, "chatcmpl-2655c1fe-749c-96df-b204-e413daa7caee", new DateTimeOffset(2026, 2, 10, 23, 7, 24, TimeSpan.Zero), 18, 1064, 0);
    }

    [Fact]
    [UpstreamTest(Upstream + "should send correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_request_body()
    {
        var capture = CaptureFixture("alibaba-text");
        var model = Model(capture);

        await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        AssertJson("{\"messages\":[{\"content\":[{\"text\":\"Hello\",\"type\":\"text\"}],\"role\":\"user\"}],\"model\":\"qwen-plus\"}", Body(capture));
    }
}
