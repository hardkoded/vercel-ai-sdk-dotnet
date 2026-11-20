// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.GoogleStreamOrderSupport;

namespace Vercel.AI.Tests.GoogleLanguageModelTests;

/// <summary>Port of <c>google-language-model.test.ts</c> &gt; <c>completed function call and text stream order</c>.</summary>
public sealed class CompletedFunctionCallAndTextStreamOrderTests
{
    private const string Prefix =
        "packages/google/src/google-language-model.test.ts::completed function call and text stream order > <dynamic>::";

    // These are synthetic protocol cases, not recorded provider responses.
    private const string Signature = "c3ludGhldGljLXNpZ25hdHVyZS8rPQ==";
    private const string Input =
        "{\"query\":\"example\",\"options\":{\"limit\":0,\"enabled\":false}}";

    private static readonly string FirstCall =
        "{\"functionCall\":{\"id\":\"call-1\",\"name\":\"lookup\",\"args\":"
        + Input
        + "},\"thoughtSignature\":\""
        + Signature
        + "\"}";

    private const string SecondCall =
        "{\"functionCall\":{\"id\":\"call-2\",\"name\":\"read_theme\"}}";
    private const string A = "{\"text\":\"A\"}";
    private const string B = "{\"text\":\"B\"}";
    private const string C = "{\"text\":\"C\"}";
    private static readonly string Call1 = "call:call-1:lookup:" + Input;
    private const string Call2 = "call:call-2:read_theme:{}";

    public static IEnumerable<object[]> Cases()
    {
        foreach (var provider in Providers)
        {
            foreach (var (name, groups, expected) in CaseList())
            {
                yield return [provider, name, groups, expected];
            }
        }
    }

    private static IEnumerable<(string Name, string[][] Groups, string[] Expected)> CaseList()
    {
        yield return (
            "one event, call between text",
            [
                [A, FirstCall, B],
            ],
            ["text:A", Call1, "text:B"]
        );
        yield return (
            "separate events, call between text",
            [
                [A],
                [FirstCall],
                [B],
            ],
            ["text:A", Call1, "text:B"]
        );
        yield return (
            "call before text",
            [
                [FirstCall, A],
            ],
            [Call1, "text:A"]
        );
        yield return (
            "call after text",
            [
                [A, FirstCall],
            ],
            ["text:A", Call1]
        );
        yield return (
            "multiple calls within one event",
            [
                [A, FirstCall, B, SecondCall, C],
            ],
            ["text:A", Call1, "text:B", Call2, "text:C"]
        );
        yield return (
            "multiple calls over events",
            [
                [FirstCall],
                [A, SecondCall],
                [B],
            ],
            [Call1, "text:A", Call2, "text:B"]
        );
        yield return (
            "adjacent calls including a no-args call",
            [
                [FirstCall, SecondCall],
            ],
            [Call1, Call2]
        );
        yield return (
            "no-args call between text",
            [
                [A, SecondCall, B],
            ],
            ["text:A", Call2, "text:B"]
        );
        yield return (
            "adjacent text within and across events",
            [
                [A, B],
                [C],
            ],
            ["text:ABC"]
        );
    }

    private static List<string> ProjectContent(IEnumerable<object> parts)
    {
        var result = new List<string>();
        foreach (var part in parts)
        {
            switch (part)
            {
                case TextDeltaStreamPart or GeneratedText:
                    var text = part is TextDeltaStreamPart delta
                        ? delta.Delta
                        : ((GeneratedText)part).Text;
                    if (
                        result.Count > 0
                        && result[^1].StartsWith("text:", StringComparison.Ordinal)
                    )
                    {
                        result[^1] += text;
                    }
                    else
                    {
                        result.Add("text:" + text);
                    }

                    break;
                case ToolCallStreamPart call:
                    result.Add(
                        "call:" + call.ToolCallId + ":" + call.ToolName + ":" + call.ArgumentsJson
                    );
                    break;
                case GeneratedToolCall generated:
                    result.Add(
                        "call:"
                            + generated.ToolCallId
                            + ":"
                            + generated.ToolName
                            + ":"
                            + generated.ArgumentsJson
                    );
                    break;
            }
        }

        return result;
    }

    private static async Task<List<LanguageModelStreamPart>> StreamAsync(
        string provider,
        params string[][] groups
    )
    {
        var model = Model(provider, Stream(groups.Select(group => Parts(group)).ToList()));
        return await GoogleUpstream.Collect(
            await model.DoStreamAsync(GoogleUpstream.Hello(), CancellationToken.None)
        );
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(
        Prefix + "$name",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public async Task Preserves_the_order_of_completed_calls_and_text(
        string provider,
        string name,
        string[][] groups,
        string[] expected
    )
    {
        Assert.NotEmpty(name);
        var events = await StreamAsync(provider, groups);

        Assert.Empty(events.OfType<ErrorStreamPart>());
        Assert.Equal(expected, ProjectContent(events));

        // A completed call is a content boundary, so the text part must close.
        string? openTextId = null;
        var textIds = new List<string>();
        foreach (var part in events)
        {
            switch (part)
            {
                case TextStartStreamPart start:
                    Assert.Null(openTextId);
                    openTextId = start.Id;
                    textIds.Add(start.Id);
                    break;
                case TextDeltaStreamPart delta:
                    Assert.Equal(openTextId, delta.Id);
                    break;
                case TextEndStreamPart end:
                    Assert.Equal(openTextId, end.Id);
                    openTextId = null;
                    break;
                case ToolCallStreamPart:
                    Assert.Null(openTextId);
                    break;
            }
        }

        Assert.Null(openTextId);
        Assert.Equal(textIds.Count, textIds.Distinct().Count());

        var first = events
            .OfType<ToolCallStreamPart>()
            .FirstOrDefault(call => call.ToolCallId == "call-1");
        if (first != null)
        {
            Assert.Equal(Input, first.ArgumentsJson);
            Assert.Equal(
                Signature,
                first
                    .ProviderMetadata!.Value.GetProperty(MetadataKey(provider))
                    .GetProperty("thoughtSignature")
                    .GetString()
            );
        }
    }

    [Theory]
    [MemberData(nameof(ProviderData))]
    [UpstreamTest(
        Prefix + "preserves a completed call without a provider ID",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public async Task Preserves_a_completed_call_without_a_provider_ID(string provider)
    {
        var events = await StreamAsync(
            provider,
            [A, "{\"functionCall\":{\"name\":\"read_theme\"}}", B]
        );

        Assert.Equal(
            ["text:A", "call:generated-call-id:read_theme:{}", "text:B"],
            ProjectContent(events)
        );
    }

    [Theory]
    [MemberData(nameof(ProviderData))]
    [UpstreamTest(
        Prefix + "does not discard an opaque signature on empty text after a completed call",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public async Task Does_not_discard_an_opaque_signature_on_empty_text_after_a_completed_call(
        string provider
    )
    {
        var events = await StreamAsync(
            provider,
            [A, SecondCall, "{\"text\":\"\",\"thoughtSignature\":\"" + Signature + "\"}"]
        );
        var key = MetadataKey(provider);

        Assert.Empty(events.OfType<ErrorStreamPart>());
        var callIndex = events.FindIndex(part => part is ToolCallStreamPart);
        var block = events.Skip(callIndex + 1).Take(3).ToList();
        Assert.Equal(["text-start", "text-delta", "text-end"], block.Select(part => part.Type));
        var start = Assert.IsType<TextStartStreamPart>(block[0]);
        Assert.Equal("1", start.Id);
        Assert.Equal(
            Signature,
            start
                .ProviderMetadata!.Value.GetProperty(key)
                .GetProperty("thoughtSignature")
                .GetString()
        );
        var delta = Assert.IsType<TextDeltaStreamPart>(block[1]);
        Assert.Equal("1", delta.Id);
        Assert.Equal(string.Empty, delta.Delta);
        Assert.Equal("1", Assert.IsType<TextEndStreamPart>(block[2]).Id);
        var call = Assert.IsType<ToolCallStreamPart>(events[callIndex]);
        Assert.Equal("call-2", call.ToolCallId);
        Assert.Equal("{}", call.ArgumentsJson);
        Assert.Contains(events, part =>
            part is TextStartStreamPart { ProviderMetadata: { } metadata }
            && metadata.GetProperty(key).GetProperty("thoughtSignature").GetString() == Signature
        );
    }

    [Theory]
    [MemberData(nameof(ProviderData))]
    [UpstreamTest(
        Prefix + "keeps non-streaming text and call content order",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public async Task Keeps_non_streaming_text_and_call_content_order(string provider)
    {
        var body = new JsonObject
        {
            ["candidates"] = new JsonArray(
                new JsonObject
                {
                    ["content"] = new JsonObject
                    {
                        ["role"] = "model",
                        ["parts"] = Parts(A, FirstCall, B, SecondCall, C),
                    },
                    ["finishReason"] = "STOP",
                    ["index"] = 0,
                }
            ),
        };
        var model = Model(provider, new RecordingHandler { ResponseText = body.ToJsonString() });

        var result = await model.DoGenerateAsync(GoogleUpstream.Hello(), CancellationToken.None);

        Assert.Equal(["text:A", Call1, "text:B", Call2, "text:C"], ProjectContent(result.Content));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves text order around a terminal partialArgs function call",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public async Task Preserves_text_order_around_a_terminal_partialArgs_function_call()
    {
        var events = await StreamAsync(
            "google.vertex.chat",
            [
                A,
                "{\"functionCall\":{\"id\":\"partial-complete\",\"name\":\"lookup\",\"partialArgs\":[{\"jsonPath\":\"$.query\",\"stringValue\":\"example\"}],\"willContinue\":false},\"thoughtSignature\":\""
                    + Signature
                    + "\"}",
                B,
            ]
        );

        Assert.Empty(events.OfType<ErrorStreamPart>());
        var callIndex = events.FindIndex(part => part is ToolCallStreamPart);
        Assert.Contains(events.Take(callIndex), part => part is TextEndStreamPart);
        Assert.Equal(["0", "1"], events.OfType<TextStartStreamPart>().Select(part => part.Id));
        Assert.Equal(
            ["text:A", "call:partial-complete:lookup:{\"query\":\"example\"}", "text:B"],
            ProjectContent(events)
        );
    }

    [Theory]
    [MemberData(nameof(ProviderData))]
    [UpstreamTest(
        Prefix + "preserves streamed content order and opaque signature in a follow-up request",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public async Task Preserves_streamed_content_order_and_opaque_signature_in_a_follow_up_request(
        string provider
    )
    {
        var events = await StreamAsync(provider, [A, FirstCall, B]);
        var assistant = new List<AssistantContentPart>();
        foreach (var part in events)
        {
            if (part is TextDeltaStreamPart delta)
            {
                assistant.Add(new AssistantTextPart(delta.Delta));
            }
            else if (part is ToolCallStreamPart call)
            {
                assistant.Add(
                    new AssistantToolCallPart(
                        new GeneratedToolCall(
                            call.ToolCallId,
                            call.ToolName,
                            call.ArgumentsJson,
                            call.ProviderMetadata
                        )
                    )
                );
            }
        }

        var handler = new RecordingHandler
        {
            ResponseText =
                "{\"candidates\":[{\"content\":{\"role\":\"model\",\"parts\":[{\"text\":\"Done\"}]},\"finishReason\":\"STOP\",\"index\":0}]}",
        };
        var followUp = Model(provider, handler);
        await followUp.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new UserModelMessage("Hello"),
                    new AssistantModelMessage(assistant),
                    new ToolModelMessage(
                        "call-1",
                        "lookup",
                        "{\"answer\":\"example\"}",
                        false,
                        "json"
                    ),
                    new UserModelMessage("Continue"),
                },
            },
            CancellationToken.None
        );

        var contents = JsonNode.Parse(handler.Body)!["contents"]!.AsArray();
        var call1 =
            provider == "google.vertex.chat"
                ? "{\"name\":\"lookup\",\"args\":" + Input + "}"
                : "{\"id\":\"call-1\",\"name\":\"lookup\",\"args\":" + Input + "}";
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"role\":\"model\",\"parts\":["
                        + A
                        + ",{\"functionCall\":"
                        + call1
                        + ",\"thoughtSignature\":\""
                        + Signature
                        + "\"},"
                        + B
                        + "]}"
                ),
                contents[1]
            ),
            contents[1]!.ToJsonString()
        );
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"role\":\"user\",\"parts\":[{\"text\":\"Continue\"}]}"),
                contents[^1]
            ),
            contents[^1]!.ToJsonString()
        );
    }

    public static IEnumerable<object[]> ProviderData() =>
        Providers.Select(provider => new object[] { provider });
}
