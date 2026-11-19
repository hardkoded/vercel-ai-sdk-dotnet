// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; result.fullStream</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultFullStreamTests
{
    private const string Id = Prefix + "output = \"object\" > result.fullStream::";

    [Fact]
    [UpstreamTest(Id + "should send full stream data", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_full_stream_data()
    {
        var result = Client().StreamObjectAsync(ObjectOptions(CreateTestModel()));

        var parts = await Collect(result.FullStream());

        string[] expected =
        [
            "object:{}",
            "text-delta:{ ",
            "object:{\"content\":\"Hello, \"}",
            "text-delta:\"content\": \"Hello, ",
            "object:{\"content\":\"Hello, world\"}",
            "text-delta:world",
            "object:{\"content\":\"Hello, world!\"}",
            "text-delta:!\"",
            "text-delta: }",
        ];
        Assert.Equal(
            expected,
            parts
                .Take(expected.Length)
                .Select(part =>
                    part switch
                    {
                        ObjectStreamObjectPart p => "object:" + p.Object.GetRawText(),
                        ObjectStreamTextDeltaPart p => "text-delta:" + p.TextDelta,
                        _ => part.Type,
                    }
                )
        );
        Assert.Equal(expected.Length + 1, parts.Count);
        var finish = Assert.IsType<ObjectStreamFinishPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(
            "{\"testProvider\":{\"testKey\":\"testValue\"}}",
            finish.ProviderMetadata!.Value.GetRawText()
        );
        Assert.Equal("id-0", finish.Response.Id);
        Assert.Equal("mock-model-id", finish.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(0), finish.Response.Timestamp);
        Assert.Equal(3, finish.Usage.InputTokens);
        Assert.Equal(3, finish.Usage.NoCacheInputTokens);
        Assert.Equal(10, finish.Usage.OutputTokens);
        Assert.Equal(10, finish.Usage.TextTokens);
        Assert.Equal(13, finish.Usage.TotalTokens);
    }
}
