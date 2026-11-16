// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doGenerate</c> &gt; <c>custom tool</c>.</summary>
public sealed class ResponsesDoGenerateCustomToolTests
{
    private const string Id = "packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate > custom tool::";
    private const string Sql = "SELECT \"name\" FROM users WHERE age > 25";
    private const string CustomCall = "{\"type\":\"custom_tool_call\",\"id\":\"ct_sql\",\"call_id\":\"call_sql\",\"name\":\"write_sql\",\"input\":\"SELECT \\\"name\\\" FROM users WHERE age > 25\",\"status\":\"completed\"}";
    private const string ExpectedTools = "[{\"type\":\"custom\",\"name\":\"write_sql\",\"description\":\"Write a SQL query.\",\"format\":{\"type\":\"grammar\",\"syntax\":\"regex\",\"definition\":\"SELECT .+\"}}]";

    private static JsonNode CustomResponse(bool empty = false) => JsonNode.Parse(
        "{\"id\":\"resp_custom\",\"created_at\":1741257730,\"model\":\"test-deployment\",\"output\":" + (empty ? "[]" : "[" + CustomCall + "]") + ",\"usage\":{\"input_tokens\":2,\"output_tokens\":4}}")!;

    private static string AddedItem()
    {
        var item = (JsonObject)JsonNode.Parse(CustomCall)!;
        item["input"] = string.Empty;
        return item.ToJsonString();
    }

    private static IReadOnlyList<LanguageModelTool> CustomTools()
    {
        var tool = AzureOpenAITools.CustomTool(new OpenAICustomToolOptions
        {
            Name = "write_sql",
            Description = "Write a SQL query.",
            GrammarSyntax = "regex",
            GrammarDefinition = "SELECT .+",
        });
        return new[] { LanguageModelTool.ProviderDefined(tool.ProviderToolId!, "write_sql", tool.Args) };
    }

    [Fact]
    [UpstreamTest(
        Id + "exposes the shared provider-defined custom tool factory",
        Coverage = UpstreamCoverage.Partial, Commit = "27528dbf247660366b8ad671d9222098e11e291f",
        Note = "The port has no provider.tools member: AzureOpenAITools.CustomTool is the shared factory and builds the openai.custom provider tool. LanguageModelTool has no isProviderExecuted flag to check.")]
    public void Exposes_the_shared_provider_defined_custom_tool_factory()
    {
        var tool = AzureOpenAITools.CustomTool(new OpenAICustomToolOptions { Name = "custom" });

        Assert.True(tool.IsProviderDefined);
        Assert.Equal("openai.custom", tool.ProviderToolId);
    }

    [Fact]
    [UpstreamTest(Id + "sends custom tool definitions and parses raw string inputs with Azure metadata", Coverage = UpstreamCoverage.Covered, Commit = "27528dbf247660366b8ad671d9222098e11e291f")]
    public async Task Sends_custom_tool_definitions_and_parses_raw_string_inputs_with_Azure_metadata()
    {
        var capture = new OpenAICapture { ResponseJson = CustomResponse().ToJsonString() };

        var options = Call(CustomTools());
        options.ToolChoice = ToolChoice.Required;
        var result = await Generate(capture, options);

        Assert.Equal(Url + "?api-version=v1", capture.Uri);
        AssertTools(capture, ExpectedTools);
        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("call_sql", call.ToolCallId);
        Assert.Equal("write_sql", call.ToolName);
        Assert.Equal(Sql, JsonNode.Parse(call.ArgumentsJson)!.GetValue<string>());
        JsonAssert.Equal(call.ProviderMetadata!.Value, "{\"azure\":{\"itemId\":\"ct_sql\"}}");
    }

    [Fact]
    [UpstreamTest(Id + "streams raw custom input deltas and the final Azure tool call", Coverage = UpstreamCoverage.Covered, Commit = "27528dbf247660366b8ad671d9222098e11e291f")]
    public async Task Streams_raw_custom_input_deltas_and_the_final_Azure_tool_call()
    {
        var deltas = new[] { "SELECT \"name\"", " FROM users WHERE age > 25" };
        var events = new List<JsonNode>
        {
            JsonNode.Parse("{\"type\":\"response.created\",\"response\":" + CustomResponse().ToJsonString() + "}")!,
            JsonNode.Parse("{\"type\":\"response.output_item.added\",\"output_index\":0,\"item\":" + AddedItem() + "}")!,
        };
        events.AddRange(deltas.Select(delta => (JsonNode)new JsonObject
        {
            ["type"] = "response.custom_tool_call_input.delta",
            ["item_id"] = "ct_sql",
            ["output_index"] = 0,
            ["delta"] = delta,
        }));
        events.Add(JsonNode.Parse("{\"type\":\"response.output_item.done\",\"output_index\":0,\"item\":" + CustomCall + "}")!);
        events.Add(JsonNode.Parse("{\"type\":\"response.completed\",\"response\":" + CustomResponse().ToJsonString() + "}")!);
        var capture = new OpenAICapture { ServerSentEvents = Sse(events) };

        var parts = await Stream(capture, Call(CustomTools()));

        Assert.Empty(parts.OfType<ErrorStreamPart>());
        var start = Assert.Single(parts.OfType<ToolInputStartStreamPart>());
        Assert.Equal("call_sql", start.Id);
        Assert.Equal("write_sql", start.ToolName);
        Assert.Equal(ToolInputEncoding.Text, start.InputEncoding);
        Assert.Equal(deltas, parts.OfType<ToolInputDeltaStreamPart>().Select(part => part.Delta));
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("call_sql", call.ToolCallId);
        Assert.Equal("write_sql", call.ToolName);
        Assert.Equal(Sql, JsonNode.Parse(call.ArgumentsJson)!.GetValue<string>());
        JsonAssert.Equal(call.ProviderMetadata!.Value, "{\"azure\":{\"itemId\":\"ct_sql\"}}");
        AssertTools(capture, ExpectedTools);
    }

    [Fact]
    [UpstreamTest(Id + "sends custom tool results in follow-up prompts", Coverage = UpstreamCoverage.Covered, Commit = "27528dbf247660366b8ad671d9222098e11e291f")]
    public async Task Sends_custom_tool_results_in_follow_up_prompts()
    {
        var capture = new OpenAICapture { ResponseJson = CustomResponse(empty: true).ToJsonString() };
        var messages = new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantToolCallPart(new GeneratedToolCall("call_sql", "write_sql", JsonValue.Create(Sql)!.ToJsonString())),
            }),
            new ToolModelMessage("call_sql", "write_sql", "Query received.", false, "text"),
        };

        await Generate(capture, Call(CustomTools(), messages));

        var input = RequestBody(capture)["input"]!.AsArray();
        Assert.Contains(input, item => JsonNode.DeepEquals(
            JsonNode.Parse("{\"type\":\"custom_tool_call_output\",\"call_id\":\"call_sql\",\"output\":\"Query received.\"}"), item));
    }
}
