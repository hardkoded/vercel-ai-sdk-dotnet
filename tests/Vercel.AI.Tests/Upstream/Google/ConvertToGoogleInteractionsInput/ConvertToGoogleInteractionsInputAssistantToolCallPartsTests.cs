// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using S = Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput.ConvertToGoogleInteractionsInputMessageSupport;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Port of <c>convert-to-google-interactions-input.test.ts</c> &gt; <c>convertToGoogleInteractionsInput</c> &gt; <c>assistant tool-call parts</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputAssistantToolCallPartsTests
{
    private const string Prefix =
        ConvertToGoogleInteractionsInputSupport.Id + "assistant tool-call parts::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "emits a function_call content block from an assistant tool-call part with object input",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_a_function_call_content_block_from_an_assistant_tool_call_part_with_object_input()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage("What's the weather in NYC?"),
            S.Assistant(S.ToolCall("call_abc", "getWeather", "{\"location\":\"New York\"}"))
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"What's the weather in NYC?\"}]},{\"type\":\"function_call\",\"id\":\"call_abc\",\"name\":\"getWeather\",\"arguments\":{\"location\":\"New York\"}}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "parses a stringified tool-call input safely",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Parses_a_stringified_tool_call_input_safely()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage("Hi"),
            S.Assistant(S.ToolCall("call_abc", "getWeather", "{\"location\":\"Boston\"}"))
        );

        var fnCall = S.FindStep(result, "function_call");
        GoogleUpstream.JsonEqual(fnCall?["arguments"], "{\"location\":\"Boston\"}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects prototype-polluting stringified tool-call input",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Rejects_prototype_polluting_stringified_tool_call_input()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage("Hi"),
            S.Assistant(S.ToolCall("call_abc", "getWeather", "{\"__proto__\":{\"polluted\":true}}"))
        );

        var fnCall = S.FindStep(result, "function_call");

        GoogleUpstream.JsonEqual(
            fnCall?["arguments"],
            "{\"value\":\"{\\\"__proto__\\\":{\\\"polluted\\\":true}}\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "round-trips a function_call signature via providerMetadata.google.signature",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Round_trips_a_function_call_signature_via_providerMetadata_google_signature()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage("Hi"),
            S.Assistant(
                S.ToolCall(
                    "call_abc",
                    "getWeather",
                    "{\"location\":\"NYC\"}",
                    "{\"google\":{\"signature\":\"sig-xyz\"}}"
                )
            )
        );

        var fnCall = S.FindStep(result, "function_call");
        Assert.Equal("sig-xyz", (string?)fnCall?["signature"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "round-trips custom video processing parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Round_trips_custom_video_processing_parts()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            S.Assistant(
                new AssistantCustomPart(
                    "google.processing_call",
                    GoogleUpstream.ProviderOptions(
                        "{\"google\":{\"processingId\":\"processing-1\",\"signature\":\"call-signature\"}}"
                    )
                ),
                new AssistantCustomPart(
                    "google.processing_result",
                    GoogleUpstream.ProviderOptions(
                        "{\"google\":{\"processingCallId\":\"processing-1\",\"signature\":\"result-signature\"}}"
                    )
                )
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"processing_call\",\"id\":\"processing-1\",\"signature\":\"call-signature\"},{\"type\":\"processing_result\",\"call_id\":\"processing-1\",\"signature\":\"result-signature\"}]"
        );
    }
}
