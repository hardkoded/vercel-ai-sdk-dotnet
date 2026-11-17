// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doGenerate</c> &gt; <c>apply patch tool</c>.</summary>
public sealed class ResponsesDoGenerateApplyPatchToolTests
{
    private const string Id =
        "packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > apply patch tool::";

    private static readonly JsonNode FixtureBody = JsonNode.Parse(
        Fixture("openai-apply-patch-tool.1.json")
    )!;
    private static readonly JsonNode Patch = FixtureBody["output"]![0]!;

    private static IReadOnlyList<LanguageModelTool> Tools() =>
        new[]
        {
            LanguageModelTool.ProviderDefined("openai.apply_patch", "apply_patch", Json("{}")),
        };

    private static JsonNode ExpectedInput() =>
        new JsonObject
        {
            ["callId"] = Patch["call_id"]!.DeepClone(),
            ["operation"] = Patch["operation"]!.DeepClone(),
        };

    [Fact]
    [UpstreamTest(
        Id + "should expose a client-executed Azure tool factory",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bf10079bc232aaa84e9f6ca03c1487bb06fa4b7d",
        Note = "The port has no provider.tools member: AzureOpenAITools.ApplyPatch is the shared factory."
    )]
    public void Should_expose_a_client_executed_Azure_tool_factory()
    {
        var tool = AzureOpenAITools.ApplyPatch(
            new OpenAIApplyPatchToolOptions
            {
                Execute = (_, _) =>
                    Task.FromResult(new OpenAIApplyPatchOutput { Status = "completed" }),
            }
        );

        Assert.True(tool.IsProviderDefined);
        Assert.Equal("openai.apply_patch", tool.ProviderToolId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should send the native definition and retain Azure item metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bf10079bc232aaa84e9f6ca03c1487bb06fa4b7d"
    )]
    public async Task Should_send_the_native_definition_and_retain_Azure_item_metadata()
    {
        var capture = new OpenAICapture { ResponseJson = FixtureBody.ToJsonString() };

        var result = await Generate(capture, Call(Tools()));

        AssertTools(capture, "[{\"type\":\"apply_patch\"}]");
        var call = Assert.IsType<GeneratedToolCall>(
            result.Content.First(part => part is GeneratedToolCall)
        );
        Assert.Equal(Patch["call_id"]!.GetValue<string>(), call.ToolCallId);
        Assert.Equal("apply_patch", call.ToolName);
        JsonAssert.Equal(
            call.ProviderMetadata!.Value,
            "{\"azure\":{\"itemId\":\"" + Patch["id"]!.GetValue<string>() + "\"}}"
        );
        Assert.True(JsonNode.DeepEquals(ExpectedInput(), JsonNode.Parse(call.ArgumentsJson)));
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        Id + "should stream the patch input with Azure item metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bf10079bc232aaa84e9f6ca03c1487bb06fa4b7d"
    )]
    public async Task Should_stream_the_patch_input_with_Azure_item_metadata()
    {
        var lines = Fixture("openai-apply-patch-tool.1.chunks.txt")
            .Split('\n')
            .Where(line => line.Trim().Length > 0)
            .ToList();
        var capture = new OpenAICapture
        {
            ServerSentEvents =
                string.Concat(lines.Select(line => "data: " + line + "\n\n")) + "data: [DONE]\n\n",
        };

        var parts = await Stream(capture, Call(Tools()));

        AssertTools(capture, "[{\"type\":\"apply_patch\"}]");
        var streamedPatch = lines
            .Select(line => JsonNode.Parse(line)!)
            .First(@event => @event["type"]!.GetValue<string>() == "response.output_item.done")[
            "item"
        ]!;
        var call = parts.OfType<ToolCallStreamPart>().First();
        Assert.Equal(streamedPatch["call_id"]!.GetValue<string>(), call.ToolCallId);
        Assert.Equal("apply_patch", call.ToolName);
        JsonAssert.Equal(
            call.ProviderMetadata!.Value,
            "{\"azure\":{\"itemId\":\"" + streamedPatch["id"]!.GetValue<string>() + "\"}}"
        );
        var expected = new JsonObject
        {
            ["callId"] = streamedPatch["call_id"]!.DeepClone(),
            ["operation"] = streamedPatch["operation"]!.DeepClone(),
        };
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(call.ArgumentsJson)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Id + "should serialize patch calls and results with store=%s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bf10079bc232aaa84e9f6ca03c1487bb06fa4b7d"
    )]
    public async Task Should_serialize_patch_calls_and_results_with_store(bool store)
    {
        var capture = new OpenAICapture { ResponseJson = Fixture("azure-text.1.json") };
        var itemId = Patch["id"]!.GetValue<string>();
        var callId = Patch["call_id"]!.GetValue<string>();
        var messages = new ModelMessage[]
        {
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(
                        new GeneratedToolCall(
                            callId,
                            "apply_patch",
                            ExpectedInput().ToJsonString()
                        ),
                        new Dictionary<string, System.Text.Json.JsonElement>
                        {
                            ["azure"] = Json("{\"itemId\":\"" + itemId + "\"}"),
                        }
                    ),
                }
            ),
            new ToolModelMessage(
                callId,
                "apply_patch",
                "{\"status\":\"completed\",\"output\":\"Created file\"}",
                false,
                "json"
            ),
        };

        await Generate(
            capture,
            Call(Tools(), messages, "{\"store\":" + (store ? "true" : "false") + "}")
        );

        var body = RequestBody(capture);
        Assert.Equal(store, body["store"]!.GetValue<bool>());
        var rest = new JsonArray(
            body["input"]!.AsArray().Skip(1).Select(item => item!.DeepClone()).ToArray()
        );
        var first = store
            ? new JsonObject { ["type"] = "item_reference", ["id"] = itemId }
            : new JsonObject
            {
                ["type"] = "apply_patch_call",
                ["id"] = itemId,
                ["call_id"] = callId,
                ["status"] = "completed",
                ["operation"] = Patch["operation"]!.DeepClone(),
            };
        var expected = new JsonArray(
            first,
            new JsonObject
            {
                ["type"] = "apply_patch_call_output",
                ["call_id"] = callId,
                ["status"] = "completed",
                ["output"] = "Created file",
            }
        );
        Assert.True(JsonNode.DeepEquals(expected, rest), rest.ToJsonString());
    }
}
