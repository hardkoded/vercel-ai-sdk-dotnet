// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doStream</c> &gt; <c>basic text</c>.
/// </summary>
public sealed class DoStreamBasicTextTests
{
    private const string Prefix = "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doStream > basic text::";

    private const string InteractionId = "v1_ChdUR3NIYXVyQkFlYVA2ZGtQajZERThBVRIXVEdzSGF1ckJBZWFQNmRrUGo2REU4QVU";

    private const string Signature = "CiQBDDnWxzCnKBoG0/vIUQ9fHy3JYGvjTmIZFY4uKrzvEqSAJh8KdQEMOdbH472i8Wb3Z10/9wPWQyeSH2KMnQfWxi4Z+jlD+igWd1veIZW9QWMqrhgPQqsabcZiwzNAyNaSOJFu0D2ulKtvde8IrVcZkeoG0IR7QVcZWFbKF/uuXxeAV2CYsqXF8Xhv362V/Lc17nWUjzFwvkrftAq8AQEMOdbHZ41h6ebbkW11izLCzDhm/aW2Zh5LR60hYvYMFrL22tOZFEHoBAzZJR+NaPaGbCCd6YtSKUMWXDckIRL42Ms6xISx5eW2xKiNk4Pf+6CDD09wP7kxx/jrvn17+oFEB86PDzUCjK79WXHTufzNZ2NFyXV9wqAX6VcrxSz6eA7BvXtZcHnP5mIlSpBwNDkuWwqtWZq7ebtySPbjjUq4tZw8xB/I0guOpC+u+lCAoSoCOtf8Y+Zc7SG6Cs8BAQw51sel75qdPe/A19DGEaH4sRjNgXHSNzW6zsOqn+nMZEQznZSP+EMitUNYJSCv9ceM5a2+hs9PhGJuPVKrSxnmVya1iHYdquju9DD0q+wSycGXsor1UI4TMic4jg2+5xAFSUir9qA6nSZeuRDjMkCQ0fbJESf2if70CBFNWQe1Q2kKpfc1CrZR2dtc28YfS/c8iyarj3cts6sybBAxgAR834OcVIttdMt8eimPXfCPAP0qGu1cxjXSk746D3XmMwQtpIfW1/xpIjVhJsGA";

    private static LanguageModelCallOptions Options(bool includeRawChunks)
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.IncludeRawChunks = includeRawChunks;
        return options;
    }

    [Fact]
    [UpstreamTest(Prefix + "streams text-start, reasoning, text-delta, text-end, finish parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_text_start_reasoning_text_delta_text_end_finish_parts()
    {
        var handler = GoogleInteractionsStreamSupport.FixtureHandler("basic");
        var model = GoogleInteractionsStreamSupport.Model(handler);

        var parts = await GoogleInteractionsStreamSupport.Stream(model, Options(false));

        var expected = new JsonArray
        {
            JsonNode.Parse("{\"type\":\"stream-start\",\"warnings\":[]}"),
            JsonNode.Parse("{\"type\":\"response-metadata\",\"id\":\"" + InteractionId + "\",\"modelId\":\"gemini-2.5-flash\"}"),
            JsonNode.Parse("{\"type\":\"reasoning-start\",\"id\":\"" + InteractionId + ":0\"}"),
            JsonNode.Parse("{\"type\":\"reasoning-end\",\"id\":\"" + InteractionId + ":0\",\"providerMetadata\":{\"google\":{\"interactionId\":\"" + InteractionId + "\",\"signature\":\"" + Signature + "\"}}}"),
            JsonNode.Parse("{\"type\":\"text-start\",\"id\":\"" + InteractionId + ":1\"}"),
            JsonNode.Parse("{\"type\":\"text-delta\",\"id\":\"" + InteractionId + ":1\",\"delta\":\"I'm doing great, thank you for asking!\\n\\nHow are you doing today? And what can I do for you?\"}"),
            JsonNode.Parse("{\"type\":\"text-end\",\"id\":\"" + InteractionId + ":1\",\"providerMetadata\":{\"google\":{\"interactionId\":\"" + InteractionId + "\"}}}"),
            JsonNode.Parse("{\"type\":\"finish\",\"finishReason\":{\"raw\":\"completed\",\"unified\":\"stop\"},\"providerMetadata\":{\"google\":{\"interactionId\":\"" + InteractionId + "\",\"serviceTier\":\"standard\"}},\"usage\":{\"inputTokens\":{\"cacheRead\":0,\"noCache\":7,\"total\":7},\"outputTokens\":{\"reasoning\":115,\"text\":26,\"total\":141},\"raw\":{\"input_tokens_by_modality\":[{\"modality\":\"text\",\"tokens\":7}],\"total_cached_tokens\":0,\"total_input_tokens\":7,\"total_output_tokens\":26,\"total_thought_tokens\":115,\"total_tokens\":148,\"total_tool_use_tokens\":0}}}"),
        };
        var actual = GoogleInteractionsStreamSupport.Snapshot(parts);
        Assert.True(JsonNode.DeepEquals(expected, actual), actual.ToJsonString());
        Assert.True((bool?)JsonNode.Parse(handler.Bodies[0])!["stream"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "exposes interactionId via the finish part providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_interactionId_via_the_finish_part_providerMetadata()
    {
        var model = GoogleInteractionsStreamSupport.Model(GoogleInteractionsStreamSupport.FixtureHandler("basic"));

        var parts = await GoogleInteractionsStreamSupport.Stream(model, Options(false));

        var finish = Assert.IsType<FinishStreamPart>(parts.Single(part => part is FinishStreamPart));
        var google = finish.ProviderMetadata!.Value.GetProperty("google");
        Assert.True(google.TryGetProperty("interactionId", out var interactionId));
        Assert.NotEqual(System.Text.Json.JsonValueKind.Null, interactionId.ValueKind);
    }

    [Fact]
    [UpstreamTest(Prefix + "emits raw chunks when includeRawChunks is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_raw_chunks_when_includeRawChunks_is_true()
    {
        var model = GoogleInteractionsStreamSupport.Model(GoogleInteractionsStreamSupport.FixtureHandler("basic"));

        var parts = await GoogleInteractionsStreamSupport.Stream(model, Options(true));

        Assert.True(parts.Count(part => part is RawStreamPart) > 0);
    }
}
