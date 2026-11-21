// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vercel.AI.Google;
using static Vercel.AI.ProviderUtils.StringifyWithBigIntAndCyclesUtil;

namespace Vercel.AI.Tests.ConvertToGoogleMessages;

/// <summary>Port of the <c>convert-to-google-messages.test.ts</c> tests that sit outside any <c>describe</c>.</summary>
public sealed class ConvertToGoogleMessagesTests
{
    private const string Prefix = "packages/google/src/convert-to-google-messages.test.ts::";

    [Theory]
    [InlineData("{\"city\":")]
    [InlineData("null")]
    [UpstreamTest(
        Prefix + "retains malformed input or parsed null without throwing: %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Retains_malformed_input_or_parsed_null_without_throwing(string input)
    {
        var call = new GoogleToolCallAssistantPart("weather", input) { ToolCallId = "call1" };

        var prompt = GoogleMessages.Convert(
            [new GoogleAssistantTurn([call])],
            new GoogleMessageOptions()
        );

        var args = prompt.Contents[0]!["parts"]![0]!["functionCall"]!["args"];
        if (input == "null")
        {
            Assert.Null(args);
        }
        else
        {
            Assert.Equal(input, args!.GetValue<string>());
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps non-ref tool results JSON serializable with BigInt and cycles",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool output is JSON text, so the object graph goes through StringifyWithBigIntAndCycles before the converter."
    )]
    public void Keeps_non_ref_tool_results_JSON_serializable_with_BigInt_and_cycles()
    {
        var value = new Dictionary<string, object?> { ["count"] = new BigInteger(42) };
        value["self"] = value;

        var prompt = GoogleMessages.Convert(
            [
                new GoogleToolTurn([
                    new GoogleToolResultPart("inspect")
                    {
                        ToolCallId = "call1",
                        OutputKind = "json",
                        Output = StringifyWithBigIntAndCycles(value),
                    },
                ]),
            ],
            new GoogleMessageOptions()
        );

        var content = prompt.Contents[0]!["parts"]![0]!["functionResponse"]!["response"]![
            "content"
        ];
        GoogleUpstream.JsonEqual(content!, "{\"count\":\"42\",\"self\":\"[Circular]\"}");
        Assert.NotNull(prompt.Contents.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "serializes BigInt returned by toJSON in structured tool results",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool output is JSON text, so the value goes through StringifyWithBigIntAndCycles before the converter. A JsonConverter is the .NET mapping of toJSON."
    )]
    public void Serializes_BigInt_returned_by_toJSON_in_structured_tool_results()
    {
        var prompt = GoogleMessages.Convert(
            [
                new GoogleToolTurn([
                    new GoogleToolResultPart("inspect")
                    {
                        ToolCallId = "call1",
                        OutputKind = "json",
                        Output = StringifyWithBigIntAndCycles(
                            new Dictionary<string, object?> { ["count"] = new CountToJson() }
                        ),
                    },
                ]),
            ],
            new GoogleMessageOptions()
        );

        GoogleUpstream.JsonEqual(
            prompt.Contents[0]!["parts"]![0]!["functionResponse"]!["response"]!["content"]!,
            "{\"count\":\"42\"}"
        );
    }

    [JsonConverter(typeof(CountConverter))]
    private sealed class CountToJson;

    private sealed class CountConverter : JsonConverter<CountToJson>
    {
        public override CountToJson Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) =>
            throw new NotSupportedException();

        public override void Write(Utf8JsonWriter w, CountToJson v, JsonSerializerOptions o) =>
            JsonSerializer.Serialize(w, new BigInteger(42), o);
    }
}
