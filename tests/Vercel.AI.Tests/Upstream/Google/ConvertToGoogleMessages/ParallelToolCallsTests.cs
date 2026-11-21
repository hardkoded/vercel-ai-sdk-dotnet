// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Numerics;
using Vercel.AI.Google;
using static Vercel.AI.ProviderUtils.StringifyWithBigIntAndCyclesUtil;

namespace Vercel.AI.Tests.ConvertToGoogleMessages;

/// <summary>Port of <c>convert-to-google-messages.test.ts</c> &gt; <c>parallel tool calls</c>.</summary>
public sealed class ParallelToolCallsTests
{
    private const string Prefix =
        "packages/google/src/convert-to-google-messages.test.ts::parallel tool calls::";

    private static GoogleToolResultPart ToolResult(string name, string id, string json) =>
        new(name)
        {
            ToolCallId = id,
            OutputKind = "json",
            Output = json,
        };

    private static GooglePrompt Convert(params GoogleTurn[] turns) =>
        GoogleMessages.Convert(turns, new GoogleMessageOptions());

    [Fact]
    [UpstreamTest(
        Prefix + "should include thought signature on functionCall when provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_include_thought_signature_on_functionCall_when_provided()
    {
        var first = new GoogleToolCallAssistantPart("checkweather", "{\"city\":\"paris\"}")
        {
            ToolCallId = "call1",
            ProviderOptions = GoogleUpstream.ProviderOptions(
                "{\"google\":{\"thoughtSignature\":\"sig_parallel\"}}"
            ),
        };
        var second = new GoogleToolCallAssistantPart("checkweather", "{\"city\":\"london\"}")
        {
            ToolCallId = "call2",
        };

        var prompt = Convert(new GoogleAssistantTurn([first, second]));

        GoogleUpstream.JsonEqual(
            prompt.Contents[0]!["parts"]![0]!,
            "{\"functionCall\":{\"id\":\"call1\",\"args\":{\"city\":\"paris\"},\"name\":\"checkweather\"},\"thoughtSignature\":\"sig_parallel\"}"
        );
        GoogleUpstream.JsonEqual(
            prompt.Contents[0]!["parts"]![1]!,
            "{\"functionCall\":{\"id\":\"call2\",\"args\":{\"city\":\"london\"},\"name\":\"checkweather\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should parse JSON string input into args object for functionCall",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_parse_JSON_string_input_into_args_object_for_functionCall()
    {
        var call = new GoogleToolCallAssistantPart(
            "checkweather",
            "{\"city\":\"berlin\",\"units\":\"metric\"}"
        )
        {
            ToolCallId = "call1",
        };

        var prompt = Convert(new GoogleAssistantTurn([call]));

        GoogleUpstream.JsonEqual(
            prompt.Contents[0]!["parts"]![0]!,
            "{\"functionCall\":{\"id\":\"call1\",\"args\":{\"city\":\"berlin\",\"units\":\"metric\"},\"name\":\"checkweather\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle circular references in tool-result without recursion error",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool output is JSON text, so the object graph goes through StringifyWithBigIntAndCycles before the converter."
    )]
    public void Should_handle_circular_references_in_tool_result_without_recursion_error()
    {
        var cyclicObj = new Dictionary<string, object?> { ["status"] = "ok" };
        cyclicObj["self"] = cyclicObj;

        var prompt = Convert(
            new GoogleToolTurn([
                ToolResult("inspectData", "call_cycle", StringifyWithBigIntAndCycles(cyclicObj)),
            ])
        );

        GoogleUpstream.JsonEqual(
            prompt.Contents[0]!["parts"]![0]!,
            "{\"functionResponse\":{\"id\":\"call_cycle\",\"name\":\"inspectData\",\"response\":{\"name\":\"inspectData\",\"content\":{\"status\":\"ok\",\"self\":\"[Circular]\"}}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle circular references and BigInt with $ref in tool-result safely",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool output is JSON text, so the object graph goes through StringifyWithBigIntAndCycles before the converter."
    )]
    public void Should_handle_circular_references_and_BigInt_with_ref_in_tool_result_safely()
    {
        var cyclicSchema = new Dictionary<string, object?>
        {
            ["$ref"] = "#/components/schemas/Item",
            ["count"] = new BigInteger(100),
        };
        cyclicSchema["self"] = cyclicSchema;

        var prompt = Convert(
            new GoogleToolTurn([
                ToolResult("getSchema", "call_schema", StringifyWithBigIntAndCycles(cyclicSchema)),
            ])
        );

        var content = prompt.Contents[0]!["parts"]![0]!["functionResponse"]!["response"]![
            "content"
        ]!;
        var text = content.GetValue<string>();
        Assert.Contains("\"count\":\"100\"", text);
        Assert.Contains("\"self\":\"[Circular]\"", text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve reused non-circular objects with $ref in tool-result",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool output is JSON text, so the shared object graph goes through StringifyWithBigIntAndCycles before the converter."
    )]
    public void Should_preserve_reused_non_circular_objects_with_ref_in_tool_result()
    {
        var shared = new Dictionary<string, object?> { ["id"] = 1 };
        var schema = new Dictionary<string, object?>
        {
            ["$ref"] = "#/components/schemas/Item",
            ["first"] = shared,
            ["second"] = shared,
        };

        var prompt = Convert(
            new GoogleToolTurn([
                ToolResult("getSchema", "call_schema", StringifyWithBigIntAndCycles(schema)),
            ])
        );

        var text = prompt.Contents[0]!["parts"]![0]!["functionResponse"]!["response"]![
            "content"
        ]!.GetValue<string>();
        Assert.Contains("\"first\":{\"id\":1}", text);
        Assert.Contains("\"second\":{\"id\":1}", text);
        Assert.DoesNotContain("[Circular]", text);
    }
}
