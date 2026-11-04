// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.E2e.OpenAI.AdditionalTools;

/// <summary>
/// Port of the testbed page examples/ai-e2e-next/app/chat/openai-additional-tools/page.tsx, its route
/// app/api/chat/openai-additional-tools/route.ts, and the agent examples/ai-e2e-next/agent/openai/additional-tools-agent.ts.
/// The page's two buttons are the two turns: "Find Linear and list issues", then "List issues again".
/// </summary>
[Trait("Category", "RealModel")]
public sealed class AdditionalToolsAgentTests
{
    private const string DiscoveredTool = "{\"type\":\"function\",\"name\":\"linear__list_issues\",\"description\":\"List Linear issues.\",\"parameters\":{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}}";

    private const string Instructions =
        "To list issues, first call connection_search if no search result exists in the conversation. "
        + "Then call linear__list_issues and summarize its result. "
        + "On later turns, reuse the discovered tool without searching again.";

    [SkippableFact]
    public async Task Should_list_issues_with_a_tool_loaded_from_the_search_result_and_reuse_it_on_the_next_turn()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY")));
        var requestBodies = new List<string>();
        var model = OpenAIProvider.Create(new OpenAIOptions(), new RecordingHandler(requestBodies)).ResponsesModel("gpt-5.4");
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "unused" }));
        var listIssues = Tool.Function(
            "linear__list_issues",
            null,
            "{\"type\":\"object\",\"properties\":{}}",
            (_, _) => Task.FromResult("{\"issues\":[\"Synthetic issue\"]}"));
        // This replaces activeTools: ['connection_search'].
        listIssues.ProviderOptions = JsonDocument.Parse("{\"openai\":{\"omitFromRequestTools\":true}}").RootElement.Clone();
        var tools = new[]
        {
            Tool.Function(
                "connection_search",
                "Find the connection tool for listing Linear issues.",
                "{\"type\":\"object\",\"properties\":{}}",
                (_, _) => Task.FromResult("{\"tool\":\"linear__list_issues\",\"connected\":true}")),
            listIssues,
        };
        var providerOptions = new Dictionary<string, JsonElement>
        {
            ["openai"] = JsonDocument.Parse("{\"store\":false}").RootElement.Clone(),
        };
        var messages = new List<ModelMessage> { new UserModelMessage("Find Linear and list my issues.") };

        async Task<GenerateTextResult> Turn() => await client.GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Instructions = Instructions,
            Tools = tools,
            ProviderOptions = providerOptions,
            Messages = messages.ToArray(),
            StopWhen = StopWhen.IsStepCount(20),
            PrepareStep = context => new PrepareStepUpdate { Messages = context.Messages.Select(LoadDiscoveredTool).ToArray() },
        });

        var first = await Turn();
        Assert.Contains(first.Steps.SelectMany(step => step.ToolResults), result => result.ToolName == "connection_search");
        Assert.Contains(first.Steps.SelectMany(step => step.ToolResults), result => result.ToolName == "linear__list_issues" && !result.IsError);
        Assert.False(string.IsNullOrEmpty(first.Text));
        messages.AddRange(first.ResponseMessages);
        messages.Add(new UserModelMessage("List my issues again."));

        var second = await Turn();
        Assert.DoesNotContain(second.Steps.SelectMany(step => step.ToolResults), result => result.ToolName == "connection_search");
        Assert.Contains(second.Steps.SelectMany(step => step.ToolResults), result => result.ToolName == "linear__list_issues" && !result.IsError);

        // The discovered function is never a top-level tool: it is only defined by the additional_tools item in the history.
        Assert.All(requestBodies, body =>
        {
            var request = JsonNode.Parse(body)!.AsObject();
            Assert.DoesNotContain(request["tools"]?.AsArray() ?? new JsonArray(), tool => tool!["name"]!.GetValue<string>() == "linear__list_issues");
        });
        Assert.Contains(requestBodies, body => JsonNode.Parse(body)!["input"]!.AsArray().Any(item => item!["type"]?.GetValue<string>() == "additional_tools"));
    }

    // Attaches the discovered definition to every connection_search result, as the agent's prepareStep does.
    private static ModelMessage LoadDiscoveredTool(ModelMessage message)
    {
        if (message is not ToolModelMessage tool || tool.ToolName != "connection_search" || tool.OutputType != "json")
        {
            return message;
        }

        var providerMetadata = JsonDocument.Parse("{\"openai\":{\"additionalTools\":[" + DiscoveredTool + "]}}").RootElement.Clone();
        return new ToolModelMessage(tool.ToolCallId, tool.ToolName, tool.OutputJson, tool.IsError, tool.OutputType, providerMetadata);
    }

    private sealed class RecordingHandler(List<string> bodies) : DelegatingHandler(new HttpClientHandler())
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content is not null)
            {
                bodies.Add(await request.Content.ReadAsStringAsync().ConfigureAwait(false));
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
