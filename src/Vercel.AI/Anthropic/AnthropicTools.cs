// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

/// <summary>A function or provider tool passed to <see cref="AnthropicToolPreparer"/>.</summary>
public sealed class AnthropicToolDefinition
{
    /// <summary><c>function</c> or <c>provider</c>.</summary>
    public string Type { get; set; } = "function";

    /// <summary>Tool name sent to the model for function tools.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Function description.</summary>
    public string? Description { get; set; }

    /// <summary>JSON Schema for a function tool.</summary>
    public JsonElement? InputSchema { get; set; }

    /// <summary>Strict structured-output flag.</summary>
    public bool? Strict { get; set; }

    /// <summary>Provider tool id, such as <c>anthropic.web_search_20250305</c>.</summary>
    public string? Id { get; set; }

    /// <summary>Provider tool arguments.</summary>
    public JsonElement? Args { get; set; }

    /// <summary>Provider options, including Anthropic cache control and tool options.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Example inputs. Each item is an object with an <c>input</c> property, or the input itself.</summary>
    public JsonElement? InputExamples { get; set; }
}

/// <summary>Tools and tool choice ready for a Messages request.</summary>
public sealed class AnthropicPreparedTools
{
    /// <summary>Creates a prepared tool set.</summary>
    public AnthropicPreparedTools(JsonArray? tools, JsonObject? toolChoice, IReadOnlyList<AnthropicWarning> warnings, IReadOnlyList<string> betas)
    {
        Tools = tools;
        ToolChoice = toolChoice;
        Warnings = warnings;
        Betas = betas;
    }

    /// <summary>Anthropic <c>tools</c> array. Null when tools are omitted.</summary>
    public JsonArray? Tools { get; }

    /// <summary>Anthropic <c>tool_choice</c>. Null when omitted.</summary>
    public JsonObject? ToolChoice { get; }

    /// <summary>Warnings from unsupported tools or strict mode.</summary>
    public IReadOnlyList<AnthropicWarning> Warnings { get; }

    /// <summary>Beta headers required by the tools.</summary>
    public IReadOnlyList<string> Betas { get; }
}

/// <summary>Builds the Anthropic <c>tools</c> and <c>tool_choice</c> fields.</summary>
public static class AnthropicToolPreparer
{
    /// <summary>Prepares tools. An empty list omits both tools and tool choice.</summary>
    public static AnthropicPreparedTools Prepare(
        IReadOnlyList<AnthropicToolDefinition>? tools,
        string? toolChoiceType,
        string? toolName,
        bool? disableParallelToolUse,
        AnthropicCacheControlValidator? cacheControl = null,
        bool supportsStructuredOutput = false,
        bool supportsStrictTools = false,
        bool defaultEagerInputStreaming = false,
        bool rejectsForcedToolUse = false)
    {
        var warnings = new List<AnthropicWarning>();
        var betas = new List<string>();
        var validator = cacheControl ?? new AnthropicCacheControlValidator();
        if (tools == null || tools.Count == 0)
        {
            return new AnthropicPreparedTools(null, null, warnings, betas);
        }

        var prepared = new JsonArray();
        foreach (var tool in tools)
        {
            if (tool.Type == "provider")
            {
                AddProviderTool(tool, prepared, betas, warnings);
                continue;
            }

            if (tool.Type != "function")
            {
                warnings.Add(new AnthropicWarning("unsupported", "tool " + tool.Type));
                continue;
            }

            var anthropicOptions = AnthropicJson.Anthropic(tool.ProviderOptions);
            var cache = validator.Get(tool.ProviderOptions, "tool definition", true);
            var eager = anthropicOptions == null ? defaultEagerInputStreaming : AnthropicJson.Bool(anthropicOptions.Value, "eagerInputStreaming") ?? defaultEagerInputStreaming;
            bool? defer = anthropicOptions == null ? null : AnthropicJson.Bool(anthropicOptions.Value, "deferLoading");
            JsonArray? callers = null;
            if (anthropicOptions != null && AnthropicJson.Property(anthropicOptions.Value, "allowedCallers") is { } callerValue && callerValue.ValueKind == JsonValueKind.Array)
            {
                callers = JsonNode.Parse(callerValue.GetRawText()) as JsonArray;
            }

            if (!supportsStrictTools && tool.Strict != null)
            {
                warnings.Add(new AnthropicWarning(
                    "unsupported",
                    "strict",
                    "Tool '" + tool.Name + "' has strict: " + (tool.Strict.Value ? "true" : "false") + ", but strict mode is not supported by this provider. The strict property will be ignored."));
            }

            var node = new JsonObject { ["name"] = tool.Name };
            AnthropicJson.Set(node, "description", tool.Description);
            node["input_schema"] = tool.InputSchema is { } schema ? AnthropicJson.Node(schema) : new JsonObject();
            AnthropicJson.Set(node, "cache_control", cache);
            if (eager)
            {
                node["eager_input_streaming"] = true;
            }

            if (supportsStrictTools && tool.Strict != null)
            {
                node["strict"] = tool.Strict.Value;
            }

            if (defer != null)
            {
                node["defer_loading"] = defer.Value;
            }

            if (callers != null)
            {
                node["allowed_callers"] = callers;
            }

            if (tool.InputExamples is { } examples && examples.ValueKind == JsonValueKind.Array)
            {
                var inputs = new JsonArray();
                foreach (var example in examples.EnumerateArray())
                {
                    if (example.ValueKind == JsonValueKind.Object && example.TryGetProperty("input", out var input))
                    {
                        inputs.Add(AnthropicJson.Node(input));
                    }
                    else
                    {
                        inputs.Add(AnthropicJson.Node(example));
                    }
                }

                node["input_examples"] = inputs;
            }

            prepared.Add(node);
            if (supportsStructuredOutput)
            {
                AddBeta(betas, "structured-outputs-2025-11-13");
            }

            if (tool.InputExamples != null || callers != null)
            {
                AddBeta(betas, "advanced-tool-use-2025-11-20");
            }
        }

        if (toolChoiceType == null)
        {
            JsonObject? choice = null;
            if (disableParallelToolUse != null)
            {
                choice = Choice("auto", null, disableParallelToolUse);
            }

            return new AnthropicPreparedTools(prepared, choice, warnings, betas);
        }

        switch (toolChoiceType)
        {
            case "auto":
                return new AnthropicPreparedTools(prepared, Choice("auto", null, disableParallelToolUse), warnings, betas);
            case "required":
                if (rejectsForcedToolUse)
                {
                    warnings.Add(new AnthropicWarning(
                        "unsupported",
                        "toolChoice",
                        "toolChoice 'required' is not supported by this model because it rejects forced tool use. Using 'auto' instead. Instruct the model to use a tool in the prompt and verify that a tool call was made."));
                    return new AnthropicPreparedTools(prepared, Choice("auto", null, disableParallelToolUse), warnings, betas);
                }

                return new AnthropicPreparedTools(prepared, Choice("any", null, disableParallelToolUse), warnings, betas);
            case "none":
                return new AnthropicPreparedTools(null, null, warnings, betas);
            case "tool":
                if (rejectsForcedToolUse)
                {
                    warnings.Add(new AnthropicWarning(
                        "unsupported",
                        "toolChoice",
                        "toolChoice 'tool' is not supported by this model because it rejects forced tool use. Only the '" + toolName + "' tool is sent with 'auto' tool choice. Instruct the model to use the tool in the prompt and verify that a tool call was made."));
                    var filtered = new JsonArray();
                    foreach (var item in prepared)
                    {
                        if (item is JsonObject obj && obj["name"]?.GetValue<string>() == toolName)
                        {
                            filtered.Add(obj.DeepClone());
                        }
                    }

                    return new AnthropicPreparedTools(filtered, Choice("auto", null, disableParallelToolUse), warnings, betas);
                }

                return new AnthropicPreparedTools(prepared, Choice("tool", toolName, disableParallelToolUse), warnings, betas);
            default:
                throw new Vercel.AI.Provider.AiSdkException("Unsupported tool choice type: " + toolChoiceType);
        }
    }

    private static JsonObject Choice(string type, string? name, bool? disableParallel)
    {
        var choice = new JsonObject { ["type"] = type };
        AnthropicJson.Set(choice, "name", name);
        if (disableParallel != null)
        {
            choice["disable_parallel_tool_use"] = disableParallel.Value;
        }

        return choice;
    }

    private static void AddProviderTool(AnthropicToolDefinition tool, JsonArray prepared, List<string> betas, List<AnthropicWarning> warnings)
    {
        var id = tool.Id ?? string.Empty;
        var args = tool.Args ?? default;
        switch (id)
        {
            case "anthropic.code_execution_20250522":
                AddBeta(betas, "code-execution-2025-05-22");
                prepared.Add(new JsonObject { ["type"] = "code_execution_20250522", ["name"] = "code_execution" });
                return;
            case "anthropic.code_execution_20250825":
                AddBeta(betas, "code-execution-2025-08-25");
                prepared.Add(new JsonObject { ["type"] = "code_execution_20250825", ["name"] = "code_execution" });
                return;
            case "anthropic.code_execution_20260120":
                prepared.Add(new JsonObject { ["type"] = "code_execution_20260120", ["name"] = "code_execution" });
                return;
            case "anthropic.computer_20241022":
                AddBeta(betas, "computer-use-2024-10-22");
                prepared.Add(Computer("computer_20241022", args, false));
                return;
            case "anthropic.computer_20250124":
                AddBeta(betas, "computer-use-2025-01-24");
                prepared.Add(Computer("computer_20250124", args, false));
                return;
            case "anthropic.computer_20251124":
                AddBeta(betas, "computer-use-2025-11-24");
                prepared.Add(Computer("computer_20251124", args, true));
                return;
            case "anthropic.computer_toolset_20260801":
                prepared.Add(Toolset(args));
                return;
            case "anthropic.text_editor_20241022":
                AddBeta(betas, "computer-use-2024-10-22");
                prepared.Add(new JsonObject { ["name"] = "str_replace_editor", ["type"] = "text_editor_20241022" });
                return;
            case "anthropic.text_editor_20250124":
                AddBeta(betas, "computer-use-2025-01-24");
                prepared.Add(new JsonObject { ["name"] = "str_replace_editor", ["type"] = "text_editor_20250124" });
                return;
            case "anthropic.text_editor_20250429":
                AddBeta(betas, "computer-use-2025-01-24");
                prepared.Add(new JsonObject { ["name"] = "str_replace_based_edit_tool", ["type"] = "text_editor_20250429" });
                return;
            case "anthropic.text_editor_20250728":
                var editor = new JsonObject { ["name"] = "str_replace_based_edit_tool", ["type"] = "text_editor_20250728" };
                if (args.ValueKind == JsonValueKind.Object)
                {
                    AnthropicJson.Set(editor, "max_characters", AnthropicJson.Int(args, "maxCharacters"));
                }

                prepared.Add(editor);
                return;
            case "anthropic.bash_20241022":
                AddBeta(betas, "computer-use-2024-10-22");
                prepared.Add(new JsonObject { ["name"] = "bash", ["type"] = "bash_20241022" });
                return;
            case "anthropic.bash_20250124":
                AddBeta(betas, "computer-use-2025-01-24");
                prepared.Add(new JsonObject { ["name"] = "bash", ["type"] = "bash_20250124" });
                return;
            case "anthropic.memory_20250818":
                AddBeta(betas, "context-management-2025-06-27");
                prepared.Add(new JsonObject { ["name"] = "memory", ["type"] = "memory_20250818" });
                return;
            case "anthropic.web_search_20250305":
                prepared.Add(Web("web_search_20250305", "web_search", args, false));
                return;
            case "anthropic.web_search_20260209":
                AddBeta(betas, "code-execution-web-tools-2026-02-09");
                prepared.Add(Web("web_search_20260209", "web_search", args, false));
                return;
            case "anthropic.web_search_20260318":
                prepared.Add(Web("web_search_20260318", "web_search", args, true));
                return;
            case "anthropic.web_fetch_20250910":
                AddBeta(betas, "web-fetch-2025-09-10");
                prepared.Add(Fetch("web_fetch_20250910", args, false));
                return;
            case "anthropic.web_fetch_20260209":
                AddBeta(betas, "code-execution-web-tools-2026-02-09");
                prepared.Add(Fetch("web_fetch_20260209", args, false));
                return;
            case "anthropic.web_fetch_20260318":
                prepared.Add(Fetch("web_fetch_20260318", args, true));
                return;
            case "anthropic.tool_search_regex_20251119":
                prepared.Add(new JsonObject { ["type"] = "tool_search_tool_regex_20251119", ["name"] = "tool_search_tool_regex" });
                return;
            case "anthropic.tool_search_bm25_20251119":
                prepared.Add(new JsonObject { ["type"] = "tool_search_tool_bm25_20251119", ["name"] = "tool_search_tool_bm25" });
                return;
            case "anthropic.advisor_20260301":
                AddBeta(betas, "advisor-tool-2026-03-01");
                prepared.Add(Advisor(args));
                return;
            default:
                warnings.Add(new AnthropicWarning("unsupported", "provider-defined tool " + id));
                return;
        }
    }

    private static JsonObject Computer(string type, JsonElement args, bool zoom)
    {
        var node = new JsonObject
        {
            ["name"] = "computer",
            ["type"] = type,
            ["display_width_px"] = AnthropicJson.Int(args, "displayWidthPx") ?? 0,
            ["display_height_px"] = AnthropicJson.Int(args, "displayHeightPx") ?? 0,
            ["display_number"] = AnthropicJson.Int(args, "displayNumber") ?? 0,
        };
        if (zoom && args.ValueKind == JsonValueKind.Object && args.TryGetProperty("enableZoom", out _))
        {
            node["enable_zoom"] = AnthropicJson.Bool(args, "enableZoom") ?? false;
        }

        return node;
    }

    private static JsonObject Toolset(JsonElement args)
    {
        var node = new JsonObject { ["type"] = "computer_toolset_20260801" };
        if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("configs", out var configs) && configs.ValueKind == JsonValueKind.Object)
        {
            var mapped = new JsonObject();
            foreach (var member in configs.EnumerateObject())
            {
                if (member.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var config = new JsonObject();
                var enabled = AnthropicJson.Bool(member.Value, "enabled");
                var defer = AnthropicJson.Bool(member.Value, "deferLoading");
                if (enabled != null)
                {
                    config["enabled"] = enabled.Value;
                }

                if (defer != null)
                {
                    config["defer_loading"] = defer.Value;
                }

                mapped[member.Name] = config;
            }

            node["configs"] = mapped;
        }

        return node;
    }

    private static JsonObject Web(string type, string name, JsonElement args, bool responseInclusion)
    {
        var node = new JsonObject { ["type"] = type, ["name"] = name };
        CopyOptional(node, args, "maxUses", "max_uses");
        CopyArray(node, args, "allowedDomains", "allowed_domains");
        CopyArray(node, args, "blockedDomains", "blocked_domains");
        if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("userLocation", out var location) && location.ValueKind == JsonValueKind.Object)
        {
            node["user_location"] = AnthropicJson.Node(location);
        }

        if (responseInclusion)
        {
            CopyOptionalString(node, args, "responseInclusion", "response_inclusion");
        }

        return node;
    }

    private static JsonObject Fetch(string type, JsonElement args, bool extra)
    {
        var node = Web(type, "web_fetch", args, false);
        CopyOptional(node, args, "maxContentTokens", "max_content_tokens");
        if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("citations", out var citations))
        {
            node["citations"] = AnthropicJson.Node(citations);
        }

        if (extra)
        {
            var useCache = AnthropicJson.Bool(args, "useCache");
            if (useCache != null)
            {
                node["use_cache"] = useCache.Value;
            }

            CopyOptionalString(node, args, "responseInclusion", "response_inclusion");
        }

        return node;
    }

    private static JsonObject Advisor(JsonElement args)
    {
        var model = AnthropicJson.String(args, "model");
        if (string.IsNullOrEmpty(model))
        {
            throw new Vercel.AI.Provider.AiSdkException("advisor_20260301 requires a model.");
        }

        if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("maxTokens", out var maxTokens))
        {
            if (maxTokens.ValueKind != JsonValueKind.Number || !maxTokens.TryGetInt32(out var integer) || Math.Abs(maxTokens.GetDouble() - integer) > 0.0000001)
            {
                throw new Vercel.AI.Provider.AiSdkException("advisor_20260301 maxTokens must be an integer.");
            }

            if (integer < 1024)
            {
                throw new Vercel.AI.Provider.AiSdkException("advisor_20260301 maxTokens must be at least 1024.");
            }
        }

        var node = new JsonObject { ["type"] = "advisor_20260301", ["name"] = "advisor", ["model"] = model };
        CopyOptional(node, args, "maxUses", "max_uses");
        CopyOptional(node, args, "maxTokens", "max_tokens");
        if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("caching", out var caching) && caching.ValueKind == JsonValueKind.Object)
        {
            node["caching"] = AnthropicJson.Node(caching);
        }

        return node;
    }

    private static void CopyOptional(JsonObject target, JsonElement args, string from, string to)
    {
        var value = AnthropicJson.Int(args, from);
        if (value != null)
        {
            target[to] = value.Value;
        }
    }

    private static void CopyOptionalString(JsonObject target, JsonElement args, string from, string to)
    {
        var value = AnthropicJson.String(args, from);
        if (value != null)
        {
            target[to] = value;
        }
    }

    private static void CopyArray(JsonObject target, JsonElement args, string from, string to)
    {
        var value = AnthropicJson.Property(args, from);
        if (value != null && value.Value.ValueKind == JsonValueKind.Array)
        {
            target[to] = AnthropicJson.Node(value.Value);
        }
    }

    private static void AddBeta(List<string> betas, string beta)
    {
        if (!betas.Contains(beta))
        {
            betas.Add(beta);
        }
    }
}

/// <summary>Runs a bash provider tool through a caller-supplied sandbox.</summary>
public static class AnthropicBashTool
{
    /// <summary>
    /// Executes <paramref name="command"/> and forwards <paramref name="cancellationToken"/> to the sandbox.
    /// </summary>
    public static async Task<AnthropicBashResult> ExecuteAsync(
        string command,
        Func<string, CancellationToken, Task<AnthropicBashResult>> sandbox,
        CancellationToken cancellationToken)
    {
        if (sandbox == null)
        {
            throw new InvalidOperationException("Sandbox session is not available");
        }

        return await sandbox(command, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>Output of a bash sandbox command.</summary>
public sealed class AnthropicBashResult
{
    /// <summary>Creates a bash result.</summary>
    public AnthropicBashResult(int exitCode, string stdout, string stderr, CancellationToken cancellationToken)
    {
        ExitCode = exitCode;
        Stdout = stdout ?? string.Empty;
        Stderr = stderr ?? string.Empty;
        CancellationToken = cancellationToken;
    }

    /// <summary>Process exit code.</summary>
    public int ExitCode { get; }

    /// <summary>Standard output.</summary>
    public string Stdout { get; }

    /// <summary>Standard error.</summary>
    public string Stderr { get; }

    /// <summary>Token the sandbox observed.</summary>
    public CancellationToken CancellationToken { get; }
}
