// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;

namespace Vercel.AI.Tests;

public sealed class ToolSearchTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::resolves description functions with the current tool context", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_description_functions_from_the_tool_context()
    {
        var tools = Registry();
        tools["getWeather"] = Deferred(describe: context => (string?)context["capability"]);
        var prepare = ToolSearchState.Create(tools, Routing());
        var prepared = prepare(tools, new Dictionary<string, object?> { ["capability"] = "Meteorology" })!;
        var result = await prepared["search"].Execute!("meteorology", new Dictionary<string, object?> { ["capability"] = "Meteorology" });
        Assert.Equal("getWeather", result.Tools[0].Name);
        Assert.Equal("Meteorology", result.Tools[0].Description);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::accumulates independent searches in the same step", Coverage = UpstreamCoverage.Covered)]
    public async Task Accumulates_independent_searches_in_the_same_step()
    {
        var tools = Registry();
        tools["getStockPrice"] = Deferred();
        var routing = Routing();
        routing["getStockPrice"] = new[] { "code" };
        var prepare = ToolSearchState.Create(tools, routing);
        var first = prepare(tools, null)!;
        await Task.WhenAll(first["search"].Execute!("weather", null), first["search"].Execute!("stock price", null));
        Assert.Equal(new[] { "code", "search" }, first.Keys.ToArray());
        Assert.Equal(new[] { "code", "search", "getWeather", "getStockPrice" }, prepare(tools, null)!.Keys.ToArray());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::discovers tools for the next preparation without exposing schemas in results", Coverage = UpstreamCoverage.Covered)]
    public async Task Discovers_tools_on_the_next_preparation()
    {
        var weather = Deferred("Weather forecast.");
        var tools = Registry(weather);
        var prepare = ToolSearchState.Create(tools, Routing());
        var first = prepare(tools, null)!;
        Assert.Equal(new[] { "code", "search" }, first.Keys.ToArray());
        var result = await first["search"].Execute!("WEATHER", null);
        Assert.Equal("getWeather", result.Tools[0].Name);
        Assert.Equal("Weather forecast.", result.Tools[0].Description);
        Assert.False(first.ContainsKey("getWeather"));
        Assert.Same(weather, prepare(tools, null)!["getWeather"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::keeps state isolated when generations share tool instances", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_discovery_state_isolated_per_generation()
    {
        var weather = Deferred("Weather forecast.");
        var tools = Registry(weather);
        var first = ToolSearchState.Create(tools, Routing());
        var second = ToolSearchState.Create(tools, Routing());
        await first(tools, null)!["search"].Execute!("weather", null);
        Assert.Same(weather, first(tools, null)!["getWeather"]);
        Assert.False(second(tools, null)!.ContainsKey("getWeather"));
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => tools["search"].Execute!("weather", null));
        Assert.Contains("must be bound", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::does not discover excluded tools or tools belonging to another caller", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_discover_excluded_tools_or_another_caller()
    {
        var weather = Deferred("Weather forecast.");
        var tools = Registry(weather);
        tools["otherCode"] = Code();
        tools["otherWeather"] = weather;
        var routing = Routing();
        routing["otherWeather"] = new[] { "otherCode" };
        var prepare = ToolSearchState.Create(tools, routing);
        var eligible = tools.Where(pair => pair.Key != "getWeather").ToDictionary(pair => pair.Key, pair => pair.Value);
        Assert.Empty((await prepare(eligible, null)!["search"].Execute!("weather", null)).Tools);
        await prepare(tools, null)!["search"].Execute!("weather", null);
        Assert.False(prepare(eligible, null)!.ContainsKey("getWeather"));
        Assert.False(prepare(tools, null)!.ContainsKey("otherWeather"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::requires an active caller to discover tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Requires_an_active_caller()
    {
        var tools = Registry();
        var prepare = ToolSearchState.Create(tools, Routing());
        var eligible = tools.Where(pair => pair.Key != "code").ToDictionary(pair => pair.Key, pair => pair.Value);
        Assert.Empty((await prepare(eligible, null)!["search"].Execute!("weather", null)).Tools);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::returns no matches for %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_no_matches_for_unrelated_blank_or_punctuation_queries()
    {
        var tools = Registry();
        var prepare = ToolSearchState.Create(tools, Routing());
        foreach (var query in new[] { "unrelated", "   ", ".*" })
        {
            Assert.Empty((await prepare(tools, null)!["search"].Execute!(query, null)).Tools);
            Assert.False(prepare(tools, null)!.ContainsKey("getWeather"));
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::ranks names above descriptions and caps discovery at five tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Ranks_names_above_descriptions_and_caps_at_five()
    {
        var weather = Deferred("Weather forecast.");
        var tools = new Dictionary<string, SearchableTool>();
        for (var i = 0; i < 8; i++)
        {
            tools["candidate" + i] = weather;
        }

        foreach (var pair in Registry(weather))
        {
            tools[pair.Key] = pair.Value;
        }

        var routing = Routing();
        for (var i = 0; i < 8; i++)
        {
            routing["candidate" + i] = new[] { "code" };
        }

        var prepare = ToolSearchState.Create(tools, routing);
        var result = await prepare(tools, null)!["search"].Execute!("weather", null);
        Assert.Equal(new[] { "getWeather", "candidate0", "candidate1", "candidate2", "candidate3" }, result.Tools.Select(tool => tool.Name).ToArray());
        Assert.Equal(7, prepare(tools, null)!.Count);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::preserves the search marker when spreading the tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_the_search_marker_when_the_tool_is_copied()
    {
        var tools = Registry();
        var copied = tools["search"].Copy();
        copied.Description = "Custom search";
        tools["search"] = copied;
        var prepare = ToolSearchState.Create(tools, Routing());
        var result = await prepare(tools, null)!["search"].Execute!("weather", null);
        Assert.Equal("getWeather", result.Tools[0].Name);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::respects activeTools before and after direct discovery", Coverage = UpstreamCoverage.Covered)]
    public async Task Respects_active_tools_around_direct_discovery()
    {
        var tools = Registry();
        var prepare = ToolSearchState.Create(tools, null);
        var eligible = tools.Where(pair => pair.Key != "getWeather").ToDictionary(pair => pair.Key, pair => pair.Value);
        Assert.Empty((await prepare(eligible, null)!["search"].Execute!("weather", null)).Tools);
        await prepare(tools, null)!["search"].Execute!("weather", null);
        Assert.False(prepare(eligible, null)!.ContainsKey("getWeather"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::treats inherited routing properties as omitted entries", Coverage = UpstreamCoverage.Covered)]
    public async Task Treats_a_tool_named_constructor_as_directly_callable()
    {
        var weather = Deferred("Weather forecast.");
        var tools = new Dictionary<string, SearchableTool> { ["search"] = ToolSearch.Create(), ["constructor"] = weather };
        var prepare = ToolSearchState.Create(tools, new Dictionary<string, IReadOnlyList<string>>());
        var result = await prepare(tools, null)!["search"].Execute!("weather", null);
        Assert.Equal("constructor", result.Tools[0].Name);
        Assert.Same(weather, prepare(tools, null)!["constructor"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::rejects description discovery and provider callers", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_callers_that_cannot_publish_a_conversation_catalog()
    {
        foreach (var caller in new[] { new ToolCallerBinding("local", false), new ToolCallerBinding("provider", false) })
        {
            var tools = Registry();
            tools["code"].Caller = caller;
            var error = Assert.Throws<InvalidArgumentException>(() => ToolSearchState.Create(tools, Routing()));
            Assert.Contains("toolDiscovery: 'conversation'", error.Message);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/prepare-tool-search.test.ts::deferred tool search::rejects deferring the search tool itself", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_deferring_the_search_tool()
    {
        var tools = Registry();
        tools["search"].DeferLoading = true;
        var error = Assert.Throws<InvalidArgumentException>(() => ToolSearchState.Create(tools, Routing()));
        Assert.Contains("search tool itself must not defer loading", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/tool-search.test.ts::%s tool search::adds discovered definitions on the next step without injecting messages (early call: %s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Adds_discovered_definitions_on_the_next_step()
    {
        foreach (var mode in new[] { "generateText", "streamText", "agent.generate", "agent.stream" })
        {
            foreach (var callEarly in new[] { false, true })
            {
                var executed = 0;
                string? arguments = null;
                var tools = new Dictionary<string, SearchableTool>
                {
                    ["search"] = ToolSearch.Create(),
                    ["getWeather"] = Deferred("Weather forecast."),
                    ["unrelated"] = Deferred("Send email"),
                };
                tools["getWeather"].Invoke = (json, _) =>
                {
                    executed++;
                    arguments = json;
                    return Task.FromResult("Bangalore: sunny");
                };
                var first = new List<ToolSearchCall> { new ToolSearchCall("search", "search", "{\"query\":\"weather\"}") };
                if (callEarly)
                {
                    first.Add(new ToolSearchCall("too-early", "getWeather", "{\"city\":\"Bangalore\"}"));
                }

                var calls = new[]
                {
                    (IReadOnlyList<ToolSearchCall>)first,
                    new[] { new ToolSearchCall("weather", "getWeather", "{\"city\":\"Bangalore\"}") },
                    Array.Empty<ToolSearchCall>(),
                };
                var beforeStep1 = executed;
                var run = await ToolSearchGeneration.RunAsync(mode, tools, null, "Find the weather.", calls, "sunny");
                Assert.Equal(0, beforeStep1);
                Assert.Equal(1, run.ExecuteCount);
                Assert.Contains("Bangalore", run.ExecutedArguments);
                Assert.Equal(new[] { "search" }, run.ToolNames[0]);
                Assert.Equal(new[] { "search", "getWeather" }, run.ToolNames[1]);
                Assert.Equal(new[] { "search", "getWeather" }, run.ToolNames[2]);
                Assert.DoesNotContain(run.UserMessages.SelectMany(messages => messages), message => message.Contains("unrelated"));
                Assert.Equal("getWeather", ((ToolSearchMatchList)run.Steps[0].Content[0].Output!).Tools[0].Name);
                Assert.Equal("Bangalore: sunny", run.Steps[1].Content[0].Output);
                if (callEarly)
                {
                    Assert.Contains(run.Steps[0].Content, item => item.Type == "tool-error" && item.ToolCallId == "too-early");
                }
            }
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/tool-search/tool-search.test.ts::%s tool search::discovers a nested tool, announces it on the next step, and executes it with a stable model definition", Coverage = UpstreamCoverage.Covered)]
    public async Task Discovers_a_nested_tool_and_announces_it_once()
    {
        foreach (var mode in new[] { "generateText", "streamText", "agent.generate", "agent.stream" })
        {
            var bindings = new List<IReadOnlyList<string>>();
            var code = Code();
            code.Caller!.Bind = names => bindings.Add(names);
            code.Caller.PrepareModelMessage = names => "Catalog: " + string.Join(", ", names);
            var weather = Deferred("Weather forecast.");
            weather.Invoke = (_, _) => Task.FromResult("sunny");
            var tools = new Dictionary<string, SearchableTool>
            {
                ["code"] = code,
                ["search"] = ToolSearch.Create(),
                ["getWeather"] = weather,
                ["unrelated"] = Deferred("Send email"),
            };
            var routing = new Dictionary<string, IReadOnlyList<string>>
            {
                ["search"] = new[] { "code" },
                ["getWeather"] = new[] { "code" },
                ["unrelated"] = new[] { "code" },
            };
            var calls = new[]
            {
                (IReadOnlyList<ToolSearchCall>)new[] { new ToolSearchCall("search", "code", "{\"name\":\"search\",\"input\":{\"query\":\"weather\"}}") },
                new[] { new ToolSearchCall("weather", "code", "{\"name\":\"getWeather\",\"input\":{}}") },
                Array.Empty<ToolSearchCall>(),
            };
            var run = await ToolSearchGeneration.RunAsync(mode, tools, routing, "Find the weather.", calls, "sunny");
            Assert.Equal(1, run.ExecuteCount);
            Assert.Equal(new[] { "search" }, bindings[0]);
            Assert.Equal(new[] { "search", "getWeather" }, bindings[1]);
            Assert.Equal(new[] { "search", "getWeather" }, bindings[2]);
            Assert.Equal(new[] { "code" }, run.ToolNames[0]);
            Assert.Equal(run.ToolNames[0], run.ToolNames[1]);
            Assert.Equal(run.ToolNames[0], run.ToolNames[2]);
            Assert.DoesNotContain("getWeather", string.Join("\n", run.UserMessages[0]));
            Assert.Contains("Catalog: search, getWeather", run.UserMessages[1]);
            Assert.Equal(1, string.Join("\n", run.UserMessages[2]).Split(new[] { "Catalog: search, getWeather" }, StringSplitOptions.None).Length - 1);
            Assert.DoesNotContain(run.UserMessages.SelectMany(messages => messages), message => message.Contains("unrelated"));
        }
    }

    private static Dictionary<string, SearchableTool> Registry(SearchableTool? weather = null)
    {
        return new Dictionary<string, SearchableTool>
        {
            ["code"] = Code(),
            ["search"] = ToolSearch.Create(),
            ["getWeather"] = weather ?? Deferred("Weather forecast."),
        };
    }

    private static Dictionary<string, IReadOnlyList<string>> Routing()
    {
        return new Dictionary<string, IReadOnlyList<string>>
        {
            ["search"] = new[] { "code" },
            ["getWeather"] = new[] { "code" },
        };
    }

    private static SearchableTool Code()
    {
        return new SearchableTool("Stable code tool.")
        {
            Caller = new ToolCallerBinding("local", true)
            {
                PrepareModelMessage = names => "catalog",
                Bind = _ => { },
            },
        };
    }

    private static SearchableTool Deferred(string? description = null, Func<IReadOnlyDictionary<string, object?>, string?>? describe = null)
    {
        return new SearchableTool(description) { DeferLoading = true, Describe = describe };
    }
}
