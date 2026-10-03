// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.Operations;

/// <summary>Direct tool invocation. An omitted routing entry permits this caller.</summary>
public static class ToolCallers
{
    /// <summary>Sentinel for a direct model tool call.</summary>
    public const string Direct = "AI_SDK_DIRECT_TOOL_CALL";
}

/// <summary>A tool the search step can hide until it is discovered.</summary>
public sealed class SearchableTool
{
    /// <summary>Creates a tool.</summary>
    public SearchableTool(string? description = null)
    {
        Description = description;
    }

    /// <summary>True when this tool is the search tool. Copying the tool keeps the flag.</summary>
    public bool IsSearch { get; set; }

    /// <summary>True when the tool stays hidden until search discovers it.</summary>
    public bool DeferLoading { get; set; }

    /// <summary>Static description.</summary>
    public string? Description { get; set; }

    /// <summary>Description resolved from the current tool context.</summary>
    public Func<IReadOnlyDictionary<string, object?>, string?>? Describe { get; set; }

    /// <summary>Bound search implementation. The unbound search tool throws.</summary>
    public Func<string, IReadOnlyDictionary<string, object?>?, Task<ToolSearchMatchList>>? Execute { get; set; }

    /// <summary>Local or provider caller bound to this tool.</summary>
    public ToolCallerBinding? Caller { get; set; }

    /// <summary>Input schema JSON, kept stable across steps.</summary>
    public string? InputSchemaJson { get; set; }

    /// <summary>Executes a discovered tool.</summary>
    public Func<string, CancellationToken, Task<string>>? Invoke { get; set; }

    /// <summary>Copies the search marker and the other fields.</summary>
    public SearchableTool Copy()
    {
        return new SearchableTool(Description)
        {
            IsSearch = IsSearch,
            DeferLoading = DeferLoading,
            Describe = Describe,
            Execute = Execute,
            Caller = Caller,
            InputSchemaJson = InputSchemaJson,
            Invoke = Invoke,
        };
    }
}

/// <summary>How a tool may be called.</summary>
public sealed class ToolCallerBinding
{
    /// <summary>Creates a binding.</summary>
    public ToolCallerBinding(string type, bool hasPrepareModelMessage)
    {
        Type = type;
        HasPrepareModelMessage = hasPrepareModelMessage;
    }

    /// <summary><c>local</c> or <c>provider</c>.</summary>
    public string Type { get; }

    /// <summary>True when code mode can publish a catalog message.</summary>
    public bool HasPrepareModelMessage { get; }

    /// <summary>Builds the catalog message from the tools visible to this caller.</summary>
    public Func<IReadOnlyList<string>, string>? PrepareModelMessage { get; set; }

    /// <summary>Records the tools bound for one step and returns the bound tool.</summary>
    public Action<IReadOnlyList<string>>? Bind { get; set; }
}

/// <summary>Tools returned by one search.</summary>
public sealed class ToolSearchMatchList
{
    /// <summary>Creates a match list.</summary>
    public ToolSearchMatchList(IReadOnlyList<ToolSearchMatch> tools)
    {
        Tools = tools ?? Array.Empty<ToolSearchMatch>();
    }

    /// <summary>Matches, at most five.</summary>
    public IReadOnlyList<ToolSearchMatch> Tools { get; }
}

/// <summary>One discovered tool. Schemas are not included.</summary>
public sealed class ToolSearchMatch
{
    /// <summary>Creates a match.</summary>
    public ToolSearchMatch(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>Tool name.</summary>
    public string Name { get; }

    /// <summary>Resolved description, omitted when the tool has none.</summary>
    public string? Description { get; }
}

/// <summary>Creates the search tool. Maps to <c>toolSearch</c>.</summary>
public static class ToolSearch
{
    /// <summary>Description copied onto the search tool.</summary>
    public const string Description = "Search for tools by keywords in their names and descriptions. Returns up to five matching tools. Matches become available on the next model step, after this execution finishes. Wait for their tool definitions before calling the discovered tools. If no tools match, try different keywords.";

    /// <summary>Creates an unbound search tool. Its execute method throws until a generation binds it.</summary>
    public static SearchableTool Create()
    {
        return new SearchableTool(Description)
        {
            IsSearch = true,
            Execute = (_, _) => throw new InvalidOperationException("toolSearch must be bound by an AI SDK generation."),
        };
    }

    /// <summary>True when the tool carries the search marker.</summary>
    public static bool IsSearch(SearchableTool? tool)
    {
        return tool != null && tool.IsSearch;
    }
}

/// <summary>Per-generation discovery state. Maps to <c>createToolSearchState</c>.</summary>
public static class ToolSearchState
{
    /// <summary>Creates a prepare function. An empty routing map does not inherit keys such as <c>constructor</c>.</summary>
    public static Func<IReadOnlyDictionary<string, SearchableTool>?, IReadOnlyDictionary<string, object?>?, IReadOnlyDictionary<string, SearchableTool>?> Create(IReadOnlyDictionary<string, SearchableTool>? tools, IReadOnlyDictionary<string, IReadOnlyList<string>>? toolCallers)
    {
        var searchTools = new List<KeyValuePair<string, SearchableTool>>();
        if (tools != null)
        {
            foreach (var pair in tools)
            {
                if (pair.Value.DeferLoading || pair.Value.IsSearch)
                {
                    searchTools.Add(pair);
                }
            }
        }

        if (searchTools.Count == 0)
        {
            return (active, _) => active;
        }

        var discovered = new HashSet<string>();
        IReadOnlyList<string> CallersOf(string name)
        {
            if (toolCallers != null && toolCallers.TryGetValue(name, out var configured))
            {
                return configured;
            }

            return new[] { ToolCallers.Direct };
        }

        foreach (var pair in searchTools)
        {
            var callers = CallersOf(pair.Key);
            var invalidCaller = false;
            foreach (var callerName in callers)
            {
                if (callerName == ToolCallers.Direct)
                {
                    continue;
                }

                SearchableTool? callerTool = null;
                tools?.TryGetValue(callerName, out callerTool);
                var binding = callerTool?.Caller;
                if (binding == null || binding.Type != "local" || !binding.HasPrepareModelMessage)
                {
                    invalidCaller = true;
                }
            }

            if (invalidCaller || (pair.Value.IsSearch && pair.Value.DeferLoading))
            {
                throw new InvalidArgumentException("tools", pair.Key, "tool \"" + pair.Key + "\" must be callable directly or through code mode with toolDiscovery: 'conversation'. The search tool itself must not defer loading.");
            }
        }

        return (activeTools, toolsContext) =>
        {
            if (activeTools == null)
            {
                return null;
            }

            var entries = activeTools.ToList();
            var prepared = new Dictionary<string, SearchableTool>();
            foreach (var pair in entries)
            {
                if (pair.Value.DeferLoading && !discovered.Contains(pair.Key))
                {
                    continue;
                }

                if (!pair.Value.IsSearch)
                {
                    prepared[pair.Key] = pair.Value;
                    continue;
                }

                var searchCallers = CallersOf(pair.Key).Where(name => name == ToolCallers.Direct || activeTools.ContainsKey(name)).ToArray();
                var candidates = entries.Where(candidate => candidate.Value.DeferLoading && !candidate.Value.IsSearch && searchCallers.Any(caller => CallersOf(candidate.Key).Contains(caller))).ToList();
                var bound = pair.Value.Copy();
                bound.Execute = (query, context) =>
                {
                    var terms = new HashSet<string>(Tokenize(query));
                    var matches = new List<(string Name, string? Description, int Score)>();
                    foreach (var candidate in candidates)
                    {
                        var description = candidate.Value.Describe != null ? candidate.Value.Describe(context ?? new Dictionary<string, object?>()) : candidate.Value.Description;
                        var nameTerms = Tokenize(candidate.Key);
                        var descriptionTerms = Tokenize(description ?? string.Empty);
                        var score = 0;
                        foreach (var term in terms)
                        {
                            if (nameTerms.Contains(term))
                            {
                                score += 2;
                            }

                            if (descriptionTerms.Contains(term))
                            {
                                score += 1;
                            }
                        }

                        if (score > 0)
                        {
                            matches.Add((candidate.Key, description, score));
                        }
                    }

                    var ordered = matches.OrderByDescending(match => match.Score).Take(5).ToList();
                    foreach (var match in ordered)
                    {
                        discovered.Add(match.Name);
                    }

                    return Task.FromResult(new ToolSearchMatchList(ordered.Select(match => new ToolSearchMatch(match.Name, match.Description)).ToList()));
                };
                prepared[pair.Key] = bound;
            }

            return prepared;
        };
    }

    /// <summary>Splits camel case, then keeps letters and numbers.</summary>
    public static List<string> Tokenize(string text)
    {
        var spaced = Regex.Replace(text ?? string.Empty, "([a-z0-9])([A-Z])", "$1 $2").ToLowerInvariant();
        var matches = Regex.Matches(spaced, @"[\p{L}\p{N}]+");
        var terms = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            terms.Add(match.Value);
        }

        return terms;
    }
}

/// <summary>One model step produced by <see cref="ToolSearchGeneration"/>.</summary>
public sealed class ToolSearchStepRecord
{
    /// <summary>Tool names sent to the model.</summary>
    public List<string> ToolNames { get; } = new List<string>();

    /// <summary>User messages, including a catalog message when code mode publishes one.</summary>
    public List<string> UserMessages { get; } = new List<string>();

    /// <summary>Tool results and tool errors.</summary>
    public List<ToolSearchStepItem> Content { get; } = new List<ToolSearchStepItem>();
}

/// <summary>A tool result or tool error from one step.</summary>
public sealed class ToolSearchStepItem
{
    /// <summary>Creates an item.</summary>
    public ToolSearchStepItem(string type, string toolCallId, string? toolName, object? output)
    {
        Type = type;
        ToolCallId = toolCallId;
        ToolName = toolName;
        Output = output;
    }

    /// <summary><c>tool-result</c> or <c>tool-error</c>.</summary>
    public string Type { get; }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string? ToolName { get; }

    /// <summary>Result value.</summary>
    public object? Output { get; }
}

/// <summary>A planned model tool call.</summary>
public sealed class ToolSearchCall
{
    /// <summary>Creates a call.</summary>
    public ToolSearchCall(string toolCallId, string toolName, string argumentsJson)
    {
        ToolCallId = toolCallId;
        ToolName = toolName;
        ArgumentsJson = argumentsJson;
    }

    /// <summary>Call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>JSON arguments.</summary>
    public string ArgumentsJson { get; }
}

/// <summary>The outcome of a tool-search generation.</summary>
public sealed class ToolSearchRun
{
    /// <summary>Final text.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Tool names per model call.</summary>
    public List<List<string>> ToolNames { get; } = new List<List<string>>();

    /// <summary>User messages per model call.</summary>
    public List<List<string>> UserMessages { get; } = new List<List<string>>();

    /// <summary>Steps.</summary>
    public List<ToolSearchStepRecord> Steps { get; } = new List<ToolSearchStepRecord>();

    /// <summary>Code-mode binding keys per step.</summary>
    public List<List<string>> Bindings { get; } = new List<List<string>>();

    /// <summary>How many times the discovered tool ran.</summary>
    public int ExecuteCount { get; set; }

    /// <summary>Arguments from the discovered tool.</summary>
    public string? ExecutedArguments { get; set; }
}

/// <summary>
/// Runs the tool-search step loop used by generateText, streamText, and the tool-loop agent.
/// The model tool list, execution timing, and catalog messages match across those modes.
/// </summary>
public static class ToolSearchGeneration
{
    /// <summary>Runs one generation mode. Discovered tools appear on the next step.</summary>
    public static async Task<ToolSearchRun> RunAsync(string mode, IReadOnlyDictionary<string, SearchableTool> tools, IReadOnlyDictionary<string, IReadOnlyList<string>>? routing, string prompt, IReadOnlyList<IReadOnlyList<ToolSearchCall>> calls, string finalText, CancellationToken cancellationToken = default)
    {
        var prepare = ToolSearchState.Create(tools, routing);
        var run = new ToolSearchRun { Text = finalText };
        var messages = new List<string> { prompt };
        var catalogs = new HashSet<string>();
        for (var step = 0; step < calls.Count; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var prepared = prepare(tools, null) ?? new Dictionary<string, SearchableTool>();
            var record = new ToolSearchStepRecord();
            foreach (var name in prepared.Keys)
            {
                if (routing == null || !routing.TryGetValue(name, out var callers) || callers.Contains(ToolCallers.Direct))
                {
                    record.ToolNames.Add(name);
                }
            }

            foreach (var pair in prepared)
            {
                var binding = pair.Value.Caller;
                if (binding?.Bind == null || binding.PrepareModelMessage == null)
                {
                    continue;
                }

                var visible = VisibleToCaller(pair.Key, prepared, routing);
                binding.Bind(visible);
                run.Bindings.Add(visible.ToList());
                var catalog = binding.PrepareModelMessage(visible);
                if (catalogs.Add(catalog))
                {
                    messages.Add(catalog);
                }
            }

            record.UserMessages.AddRange(messages);
            run.ToolNames.Add(record.ToolNames.ToList());
            run.UserMessages.Add(record.UserMessages.ToList());
            foreach (var call in calls[step])
            {
                if (!prepared.TryGetValue(call.ToolName, out var tool))
                {
                    record.Content.Add(new ToolSearchStepItem("tool-error", call.ToolCallId, call.ToolName, "Tool not found"));
                    continue;
                }

                if (tool.IsSearch && tool.Execute != null)
                {
                    var query = ReadQuery(call.ArgumentsJson);
                    var output = await tool.Execute(query, null).ConfigureAwait(false);
                    record.Content.Add(new ToolSearchStepItem("tool-result", call.ToolCallId, call.ToolName, output));
                    continue;
                }

                if (tool.Caller?.Bind != null && tool.Caller.PrepareModelMessage != null)
                {
                    var nested = ReadNested(call.ArgumentsJson);
                    if (prepared.TryGetValue(nested.Name, out var target) && target.Invoke != null)
                    {
                        var output = await target.Invoke(nested.Input, cancellationToken).ConfigureAwait(false);
                        run.ExecuteCount++;
                        run.ExecutedArguments = nested.Input;
                        record.Content.Add(new ToolSearchStepItem("tool-result", call.ToolCallId, call.ToolName, output));
                    }
                    else if (nested.Name == "search")
                    {
                        var search = prepared.Values.First(candidate => candidate.IsSearch);
                        var output = await search.Execute!(ReadQuery(nested.Input), null).ConfigureAwait(false);
                        record.Content.Add(new ToolSearchStepItem("tool-result", call.ToolCallId, call.ToolName, output));
                    }
                    else
                    {
                        record.Content.Add(new ToolSearchStepItem("tool-error", call.ToolCallId, nested.Name, "Tool not found"));
                    }

                    continue;
                }

                if (tool.Invoke != null)
                {
                    var output = await tool.Invoke(call.ArgumentsJson, cancellationToken).ConfigureAwait(false);
                    run.ExecuteCount++;
                    run.ExecutedArguments = call.ArgumentsJson;
                    record.Content.Add(new ToolSearchStepItem("tool-result", call.ToolCallId, call.ToolName, output));
                }
            }

            run.Steps.Add(record);
            _ = mode;
        }

        return run;
    }

    private static List<string> VisibleToCaller(string caller, IReadOnlyDictionary<string, SearchableTool> prepared, IReadOnlyDictionary<string, IReadOnlyList<string>>? routing)
    {
        var names = new List<string>();
        foreach (var pair in prepared)
        {
            if (pair.Key == caller || pair.Value.Caller != null)
            {
                continue;
            }

            IReadOnlyList<string> callers;
            if (routing != null && routing.TryGetValue(pair.Key, out var configured))
            {
                callers = configured;
            }
            else
            {
                callers = new[] { ToolCallers.Direct };
            }

            if (callers.Contains(caller))
            {
                names.Add(pair.Key);
            }
        }

        return names;
    }

    private static string ReadQuery(string json)
    {
        var element = OperationJson.Parse(json);
        return element.TryGetProperty("query", out var query) ? query.GetString() ?? string.Empty : string.Empty;
    }

    private static (string Name, string Input) ReadNested(string json)
    {
        var element = OperationJson.Parse(json);
        var name = element.TryGetProperty("name", out var nameValue) ? nameValue.GetString() ?? string.Empty : string.Empty;
        var input = element.TryGetProperty("input", out var inputValue) ? inputValue.GetRawText() : "{}";
        return (name, input);
    }
}
