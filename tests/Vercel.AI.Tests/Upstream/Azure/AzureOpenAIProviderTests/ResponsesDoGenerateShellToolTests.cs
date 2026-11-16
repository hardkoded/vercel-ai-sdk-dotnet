// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doGenerate</c> &gt; <c>shell tool</c>.</summary>
public sealed class ResponsesDoGenerateShellToolTests
{
    private const string Id = "packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > shell tool::";

    private const string CallJson = "{\"id\":\"sh_test\",\"type\":\"shell_call\",\"status\":\"completed\",\"call_id\":\"call_test\",\"action\":{\"commands\":[\"printf azure-shell\"],\"timeout_ms\":1000,\"max_output_length\":1000}}";
    private const string InputJson = "{\"action\":{\"commands\":[\"printf azure-shell\"],\"timeoutMs\":1000,\"maxOutputLength\":1000}}";
    private const string OutputJson = "{\"output\":[{\"stdout\":\"azure-shell\",\"stderr\":\"\",\"outcome\":{\"type\":\"exit\",\"exitCode\":0}}]}";
    private const string NativeOutputJson = "{\"type\":\"shell_call_output\",\"call_id\":\"call_test\",\"output\":[{\"stdout\":\"azure-shell\",\"stderr\":\"\",\"outcome\":{\"type\":\"exit\",\"exit_code\":0}}]}";
    private const string LocalEnvironment = "{\"type\":\"local\",\"skills\":[{\"name\":\"test\",\"description\":\"Test\",\"path\":\"/skills/test\"}]}";

    public static TheoryData<string> Modes => new() { "local", "containerAuto", "containerReference" };

    public static TheoryData<bool, bool> StoreAndHosted => new() { { true, false }, { false, false }, { true, true }, { false, true } };

    private static string Environment(string mode) => mode switch
    {
        "local" => LocalEnvironment,
        "containerAuto" => "{\"type\":\"containerAuto\"}",
        _ => "{\"type\":\"containerReference\",\"containerId\":\"cntr_test\"}",
    };

    private static string NativeEnvironment(string mode) => mode switch
    {
        "local" => LocalEnvironment,
        "containerAuto" => "{\"type\":\"container_auto\"}",
        _ => "{\"type\":\"container_reference\",\"container_id\":\"cntr_test\"}",
    };

    private static IReadOnlyList<LanguageModelTool> Tools(string mode) =>
        new[] { LanguageModelTool.ProviderDefined("openai.shell", "shell", Json("{\"environment\":" + Environment(mode) + "}")) };

    private static string Response() =>
        "{\"id\":\"resp_test\",\"created_at\":1764713372,\"model\":\"test-deployment\",\"output\":[" + CallJson + "],\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}";

    [Fact]
    [UpstreamTest(
        Id + "should expose the shared Azure shell factory",
        Coverage = UpstreamCoverage.Partial, Commit = "5cc6d284e83b365506e2c577151d82453d8c31d4",
        Note = "The port has no provider.tools member: AzureOpenAITools.Shell is the shared factory and builds the openai.shell provider tool. The port's factory takes no execute callback.")]
    public void Should_expose_the_shared_Azure_shell_factory()
    {
        var tool = AzureOpenAITools.Shell();

        Assert.True(tool.IsProviderDefined);
        Assert.Equal("openai.shell", tool.ProviderToolId);
    }

    [Theory]
    [MemberData(nameof(Modes))]
    [UpstreamTest(Id + "should serialize %s and retain Azure metadata when generating", Coverage = UpstreamCoverage.Covered, Commit = "5cc6d284e83b365506e2c577151d82453d8c31d4")]
    public async Task Should_serialize_mode_and_retain_Azure_metadata_when_generating(string mode)
    {
        var capture = new OpenAICapture { ResponseJson = Response() };

        var result = await Generate(capture, Call(Tools(mode)));

        AssertTools(capture, "[{\"type\":\"shell\",\"environment\":" + NativeEnvironment(mode) + "}]");
        var call = Assert.IsType<GeneratedToolCall>(result.Content.First(part => part is GeneratedToolCall));
        Assert.Equal("call_test", call.ToolCallId);
        Assert.Equal("shell", call.ToolName);
        JsonAssert.Equal(call.ProviderMetadata!.Value, "{\"azure\":{\"itemId\":\"sh_test\"}}");
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(InputJson), JsonNode.Parse(call.ArgumentsJson)));
        Assert.Equal(mode != "local", call.ProviderExecuted);
    }

    [Theory]
    [MemberData(nameof(Modes))]
    [UpstreamTest(Id + "should retain Azure metadata and execution mode for %s streams", Coverage = UpstreamCoverage.Covered, Commit = "5cc6d284e83b365506e2c577151d82453d8c31d4")]
    public async Task Should_retain_Azure_metadata_and_execution_mode_for_mode_streams(string mode)
    {
        var inProgress = (JsonObject)JsonNode.Parse(CallJson)!;
        inProgress["status"] = "in_progress";
        var created = (JsonObject)JsonNode.Parse(Response())!;
        created["output"] = new JsonArray();
        var events = new JsonNode[]
        {
            new JsonObject { ["type"] = "response.created", ["response"] = created },
            new JsonObject { ["type"] = "response.output_item.added", ["output_index"] = 0, ["item"] = inProgress },
            new JsonObject { ["type"] = "response.output_item.done", ["output_index"] = 0, ["item"] = JsonNode.Parse(CallJson) },
            new JsonObject { ["type"] = "response.completed", ["response"] = JsonNode.Parse(Response()) },
        };
        var capture = new OpenAICapture { ServerSentEvents = Sse(events) };

        var parts = await Stream(capture, Call(Tools(mode)));

        var call = parts.OfType<ToolCallStreamPart>().First();
        Assert.Equal("call_test", call.ToolCallId);
        Assert.Equal("shell", call.ToolName);
        JsonAssert.Equal(call.ProviderMetadata!.Value, "{\"azure\":{\"itemId\":\"sh_test\"}}");
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(InputJson), JsonNode.Parse(call.ArgumentsJson)));
        Assert.Equal(mode != "local", call.ProviderExecuted);
    }

    [Theory]
    [MemberData(nameof(StoreAndHosted))]
    [UpstreamTest(Id + "should round-trip shell results with $store storage and hosted=$hosted", Coverage = UpstreamCoverage.Covered, Commit = "5cc6d284e83b365506e2c577151d82453d8c31d4")]
    public async Task Should_round_trip_shell_results_with_store_storage_and_hosted(bool store, bool hosted)
    {
        var capture = new OpenAICapture { ResponseJson = Fixture("azure-text.1.json") };
        var messages = new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantToolCallPart(
                    new GeneratedToolCall("call_test", "shell", InputJson) { ProviderExecuted = hosted },
                    new Dictionary<string, JsonElement> { ["azure"] = Json("{\"itemId\":\"sh_test\"}") }),
            }),
            new ToolModelMessage("call_test", "shell", OutputJson, false, "json"),
        };

        await Generate(capture, Call(Tools(hosted ? "containerAuto" : "local"), messages, "{\"store\":" + (store ? "true" : "false") + "}"));

        var body = RequestBody(capture);
        Assert.Equal(store, body["store"]!.GetValue<bool>());
        var rest = new JsonArray(body["input"]!.AsArray().Skip(1).Select(item => item!.DeepClone()).ToArray());
        var expected = new JsonArray(
            store ? new JsonObject { ["type"] = "item_reference", ["id"] = "sh_test" } : JsonNode.Parse(CallJson),
            JsonNode.Parse(NativeOutputJson));
        Assert.True(JsonNode.DeepEquals(expected, rest), rest.ToJsonString());
    }
}
