// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText.OpenAIAdditionalTools;

/// <summary>Port of <c>openai-additional-tools.test.ts</c> &gt; <c>OpenAI additional tools through %sText</c>.</summary>
public sealed class OpenAIAdditionalToolsThroughTextTests
{
    private const string ListIssuesOutput = "{\"team\":\"SDK\",\"issues\":[\"SDK-1\"]}";

    [Theory]
    [InlineData("generate", false)]
    [InlineData("generate", true)]
    [InlineData("stream", false)]
    [InlineData("stream", true)]
    [UpstreamTest(
        "packages/ai/src/generate-text/openai-additional-tools.test.ts::OpenAI additional tools through %sText::executes history-loaded tools through automatic steps and manual replay (store: %s)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Executes_history_loaded_tools_through_automatic_steps_and_manual_replay(string method, bool store)
    {
        var definition = JsonNode.Parse("""
            {"type":"function","name":"list_issues","parameters":{"type":"object","properties":{"team":{"type":"string"}},"required":["team"],"additionalProperties":false},"strict":true}
            """)!;
        var searchResult = JsonNode.Parse("""
            [{"connection":"linear","tool":"list_issues"},{"connection":"github","error":"Not authorized"}]
            """)!;
        var resultOptions = JsonDocument.Parse("{\"openai\":{\"additionalTools\":[" + definition.ToJsonString() + "]}}").RootElement.Clone();
        var messages = new List<ModelMessage>
        {
            new UserModelMessage("Find Linear and list issues for SDK."),
            new AssistantModelMessage(null, new[] { new GeneratedToolCall("search-call", "search", "{}") }, null),
            new ToolModelMessage("search-call", "search", searchResult.ToJsonString(), false, resultOptions),
        };
        var executeCalls = 0;
        var handler = new CallNumberHandler(method == "generate", callNumber =>
        {
            // Each invocation makes a tool call followed by a final completion.
            // The last invocation deliberately returns invalid arguments.
            var item = new JsonObject
            {
                ["type"] = "function_call",
                ["id"] = "fc_" + callNumber,
                ["call_id"] = "call_" + callNumber,
                ["name"] = "list_issues",
                ["arguments"] = callNumber == 4 ? "{\"team\":123}" : "{\"team\":\"SDK\"}",
                ["status"] = "completed",
            };
            var callsTool = callNumber % 2 == 0;
            return (item, callsTool, new JsonObject
            {
                ["id"] = "resp_" + callNumber,
                ["created_at"] = 0,
                ["model"] = "gpt-5.4",
                ["output"] = callsTool ? new JsonArray(item.DeepClone()) : new JsonArray(),
                ["usage"] = new JsonObject { ["input_tokens"] = 10, ["output_tokens"] = 5 },
            });
        });
        var model = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test" }, handler).ResponsesModel("gpt-5.4");
        var listIssues = Tool.Function(
            "list_issues",
            null,
            "{\"type\":\"object\",\"properties\":{\"team\":{\"type\":\"string\"}},\"required\":[\"team\"]}",
            (_, _) =>
            {
                Interlocked.Increment(ref executeCalls);
                return Task.FromResult(ListIssuesOutput);
            });
        listIssues.ProviderOptions = JsonDocument.Parse("{\"openai\":{\"omitFromRequestTools\":true}}").RootElement.Clone();
        var tools = new[]
        {
            Tool.Function("search", null, "{\"type\":\"object\",\"properties\":{}}", null),
            listIssues,
        };
        var providerOptions = new Dictionary<string, JsonElement>
        {
            ["openai"] = JsonDocument.Parse("{\"store\":" + (store ? "true" : "false") + "}").RootElement.Clone(),
        };
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "unused" }));

        async Task<(IReadOnlyList<StepResult> Steps, IReadOnlyList<ModelMessage> ResponseMessages)> Invoke(int steps)
        {
            if (method == "generate")
            {
                var generated = await client.GenerateTextAsync(new GenerateTextOptions
                {
                    Model = model,
                    Tools = tools,
                    ProviderOptions = providerOptions,
                    MaxRetries = 0,
                    Messages = messages.ToArray(),
                    StopWhen = StopWhen.IsStepCount(steps),
                });
                return (generated.Steps, generated.ResponseMessages);
            }

            var result = client.StreamTextAsync(new StreamTextOptions
            {
                Model = model,
                Tools = tools,
                ProviderOptions = providerOptions,
                MaxRetries = 0,
                Messages = messages.ToArray(),
                StopWhen = StopWhen.IsStepCount(steps),
            });
            await foreach (var _ in result.Stream())
            {
            }

            return (await result.Steps, await result.ResponseMessages);
        }

        var first = await Invoke(2);
        Assert.Equal(2, first.Steps.Count);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(ListIssuesOutput), JsonNode.Parse(first.Steps[0].ToolResults[0].OutputJson)));
        messages.AddRange(first.ResponseMessages);
        messages.Add(new UserModelMessage("List issues again."));
        var second = await Invoke(2);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(ListIssuesOutput), JsonNode.Parse(second.Steps[0].ToolResults[0].OutputJson)));
        Assert.Equal(2, executeCalls);

        var bodies = handler.Bodies.Select(body => JsonNode.Parse(body)!.AsObject()).ToArray();
        Assert.Equal(4, bodies.Length);
        var prefix = new JsonArray(
            JsonNode.Parse("""{"role":"user","content":[{"type":"input_text","text":"Find Linear and list issues for SDK."}]}"""),
            JsonNode.Parse("""{"type":"function_call","call_id":"search-call","name":"search","arguments":"{}"}"""),
            new JsonObject
            {
                ["type"] = "function_call_output",
                ["call_id"] = "search-call",
                ["output"] = searchResult.ToJsonString(),
            },
            new JsonObject
            {
                ["type"] = "additional_tools",
                ["role"] = "developer",
                ["tools"] = new JsonArray(definition.DeepClone()),
            });
        foreach (var body in bodies)
        {
            var input = body["input"]!.AsArray();
            Assert.True(JsonNode.DeepEquals(prefix, new JsonArray(input.Take(4).Select(item => item!.DeepClone()).ToArray())));
            Assert.Single(input, item => item!["type"]?.GetValue<string>() == "additional_tools");
            Assert.Equal(new[] { "search" }, body["tools"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>()));
        }

        Assert.True(JsonNode.DeepEquals(
            new JsonArray(bodies[1]["input"]!.AsArray().Select(item => item!.DeepClone()).Append(
                JsonNode.Parse("""{"role":"user","content":[{"type":"input_text","text":"List issues again."}]}""")!).ToArray()),
            bodies[2]["input"]));
        Assert.True(JsonNode.DeepEquals(
            new JsonObject { ["type"] = "function_call_output", ["call_id"] = "call_0", ["output"] = ListIssuesOutput },
            bodies[1]["input"]!.AsArray().Last()));
        Assert.True(JsonNode.DeepEquals(
            new JsonObject { ["type"] = "function_call_output", ["call_id"] = "call_2", ["output"] = ListIssuesOutput },
            bodies[3]["input"]!.AsArray().Last()));

        // Omission is only about serialization: normal input validation still runs.
        messages.AddRange(second.ResponseMessages);
        messages.Add(new UserModelMessage("Try again."));
        var invalid = await Invoke(1);
        var invalidCall = invalid.Steps[0].ToolCalls[0];
        Assert.Equal("list_issues", invalidCall.ToolName);
        Assert.True(invalidCall.Invalid);
        Assert.Equal(2, executeCalls);
    }

    /// <summary>Answers each request from its zero-based call number, as JSON or as a Responses event stream, and records the bodies.</summary>
    private sealed class CallNumberHandler(bool json, Func<int, (JsonObject Item, bool CallsTool, JsonObject Response)> respond) : HttpMessageHandler
    {
        private int _calls = -1;

        public List<string> Bodies { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Bodies.Add(request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false));
            var (item, callsTool, response) = respond(Interlocked.Increment(ref _calls));
            if (json)
            {
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response.ToJsonString(), Encoding.UTF8, "application/json") };
            }

            var events = new List<JsonObject>();
            if (callsTool)
            {
                var added = (JsonObject)item.DeepClone();
                added["arguments"] = string.Empty;
                events.Add(new JsonObject { ["type"] = "response.output_item.added", ["output_index"] = 0, ["item"] = added });
                events.Add(new JsonObject { ["type"] = "response.output_item.done", ["output_index"] = 0, ["item"] = item.DeepClone() });
            }

            events.Add(new JsonObject { ["type"] = "response.completed", ["response"] = response });
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Concat(events.Select(evt => "data: " + evt.ToJsonString() + "\n\n")), Encoding.UTF8, "text/event-stream"),
            };
        }
    }
}
