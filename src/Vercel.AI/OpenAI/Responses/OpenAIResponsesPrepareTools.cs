// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>
/// The tools a request may call, sent as <c>tool_choice: { type: "allowed_tools" }</c>. Maps to the
/// <c>allowedTools</c> option of the OpenAI Responses provider options.
/// </summary>
public sealed class OpenAIResponsesAllowedTools
{
    /// <summary>Creates the allow-list.</summary>
    /// <param name="toolNames">Names of the tools the model may call. Not empty.</param>
    /// <param name="mode"><c>auto</c> (default) or <c>required</c>.</param>
    public OpenAIResponsesAllowedTools(IReadOnlyList<string> toolNames, string? mode = null)
    {
        ToolNames = toolNames ?? throw new ArgumentNullException(nameof(toolNames));
        Mode = mode;
    }

    /// <summary>Names of the tools the model may call.</summary>
    public IReadOnlyList<string> ToolNames { get; }

    /// <summary><c>auto</c> or <c>required</c>. Null means <c>auto</c>.</summary>
    public string? Mode { get; }
}

/// <summary>Options of <see cref="OpenAIResponsesLanguageModel.PrepareResponsesTools"/> besides the tools and the tool choice.</summary>
public sealed class PrepareResponsesToolsOptions
{
    /// <summary>When set, replaces <c>toolChoice</c> with an <c>allowed_tools</c> choice.</summary>
    public OpenAIResponsesAllowedTools? AllowedTools { get; init; }

    /// <summary>Maps tool names of the request to provider tool names such as <c>web_search</c>.</summary>
    public ToolNameMapping? ToolNameMapping { get; init; }

    /// <summary>Receives the names of the <c>openai.custom</c> tools.</summary>
    public ISet<string>? CustomProviderToolNames { get; init; }

    /// <summary>Receives the names of the function tools that declare an output schema.</summary>
    public ISet<string>? OutputSchemaToolNames { get; init; }

    /// <summary>Whether the model accepts <c>async</c> tools. When false, the option is dropped with a warning. Default true.</summary>
    public bool SupportsAsyncToolCalling { get; init; } = true;
}

/// <summary>Maps to <c>prepareResponsesTools</c>.</summary>
internal static class OpenAIResponsesPrepareTools
{
    private const string NamespaceUnsupported = "tools inside an OpenAI tool namespace are not visible to tool_choice.allowed_tools";
    private const string DeferredUnsupported = "deferred tools are not visible to tool_choice.allowed_tools";
    private const string OmittedUnsupported = "tools omitted from the request tools array cannot be allow-listed";

    private static readonly string[] AllowedBuiltInTypes =
    {
        "file_search", "web_search", "web_search_preview", "image_generation", "code_interpreter", "computer",
        "apply_patch", "shell", "local_shell", "programmatic_tool_calling",
    };

    private static readonly string[] HostedChoiceTypes =
    {
        "code_interpreter", "file_search", "image_generation", "web_search_preview", "web_search", "mcp",
        "apply_patch", "computer", "programmatic_tool_calling",
    };

    private sealed class AllowedResolution
    {
        public static readonly AllowedResolution Ambiguous = new(null, null);

        private AllowedResolution(JsonObject? entry, string? reason)
        {
            Entry = entry;
            Reason = reason;
        }

        public JsonObject? Entry { get; }

        public string? Reason { get; }

        public bool Supported => Entry != null;

        public static AllowedResolution Allow(JsonObject entry) => new(entry, null);

        public static AllowedResolution Reject(string reason) => new(null, reason);

        public bool IsSameAs(AllowedResolution other)
        {
            return Supported && other.Supported
                ? Key(Entry!) == Key(other.Entry!)
                : !Supported && !other.Supported && Reason == other.Reason;
        }

        private static string Key(JsonObject entry)
        {
            var type = entry["type"]!.GetValue<string>();
            return type switch
            {
                "mcp" => "mcp:" + entry["server_label"]!.GetValue<string>(),
                "function" or "custom" => type + ":" + entry["name"]!.GetValue<string>(),
                _ => type,
            };
        }
    }

    /// <exception cref="TypeValidationException">The arguments of a provider-defined tool are invalid.</exception>
    /// <exception cref="UnsupportedFunctionalityException">The tools or the tool choice cannot be expressed for OpenAI.</exception>
    public static PreparedResponsesTools Prepare(IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice, PrepareResponsesToolsOptions? options)
    {
        if (tools == null || tools.Count == 0)
        {
            return new PreparedResponsesTools(null, null, Array.Empty<CallWarning>());
        }

        options ??= new PrepareResponsesToolsOptions();
        var openaiTools = new JsonArray();
        var warnings = new List<CallWarning>();
        var namespaces = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
        var customToolNames = options.CustomProviderToolNames ?? new HashSet<string>();
        var resolutions = new Dictionary<string, AllowedResolution>(StringComparer.Ordinal);
        var aliases = new Dictionary<string, AllowedResolution>(StringComparer.Ordinal);

        void Record(string toolName, AllowedResolution resolution, string? canonicalName)
        {
            resolutions[toolName] = resolution;
            if (canonicalName == null || canonicalName == toolName)
            {
                return;
            }

            if (!aliases.TryGetValue(canonicalName, out var existing))
            {
                aliases[canonicalName] = resolution;
            }
            else if (existing != AllowedResolution.Ambiguous && !existing.IsSameAs(resolution))
            {
                aliases[canonicalName] = AllowedResolution.Ambiguous;
            }
        }

        foreach (var tool in tools)
        {
            if (tool.IsProviderDefined)
            {
                var mapped = MapProviderTool(tool, options, warnings);
                if (mapped == null)
                {
                    continue;
                }

                openaiTools.Add(mapped);
                if (mapped["type"]!.GetValue<string>() == "custom")
                {
                    customToolNames.Add(tool.Name);
                }

                Record(tool.Name, ToAllowedResolution(mapped), options.ToolNameMapping?.ToProviderToolName(tool.Name));
                continue;
            }

            var openaiOptions = OpenAIToolOptions(tool);
            var outputSchema = OpenAIJson.Child(openaiOptions, "outputSchema") ?? tool.OutputSchema;
            if (outputSchema != null)
            {
                options.OutputSchemaToolNames?.Add(tool.Name);
            }

            if (OpenAIJson.Bool(openaiOptions, "omitFromRequestTools") == true)
            {
                Record(tool.Name, AllowedResolution.Reject(OmittedUnsupported), null);
                if (options.AllowedTools == null && toolChoice is ToolChoice.NamedChoice forced && forced.ToolName == tool.Name)
                {
                    throw Unsupported("forcing tool \"" + tool.Name + "\" omitted from the request tools array; use automatic tool choice for history-loaded tools");
                }

                continue;
            }

            var function = PrepareFunctionTool(
                tool,
                openaiOptions,
                outputSchema,
                ResolveAsync(OpenAIJson.Bool(openaiOptions, "async"), options.SupportsAsyncToolCalling, tool.Name, warnings),
                warnings);
            var toolNamespace = OpenAIJson.Child(openaiOptions, "namespace");
            if (toolNamespace == null)
            {
                openaiTools.Add(function);
            }
            else
            {
                var namespaceName = OpenAIJson.String(toolNamespace, "name") ?? string.Empty;
                var namespaceDescription = OpenAIJson.String(toolNamespace, "description");
                if (!namespaces.TryGetValue(namespaceName, out var namespaceTool))
                {
                    namespaceTool = new JsonObject
                    {
                        ["type"] = "namespace",
                        ["name"] = namespaceName,
                        ["description"] = namespaceDescription,
                        ["tools"] = new JsonArray(),
                    };
                    namespaces[namespaceName] = namespaceTool;
                    openaiTools.Add(namespaceTool);
                }
                else if (namespaceTool["description"]?.GetValue<string>() != namespaceDescription)
                {
                    throw Unsupported("conflicting descriptions for OpenAI tool namespace \"" + namespaceName + "\"");
                }

                namespaceTool["tools"]!.AsArray().Add(function);
            }

            Record(
                tool.Name,
                toolNamespace != null
                    ? AllowedResolution.Reject(NamespaceUnsupported)
                    : OpenAIJson.Bool(openaiOptions, "deferLoading") == true
                        ? AllowedResolution.Reject(DeferredUnsupported)
                        : AllowedResolution.Allow(new JsonObject { ["type"] = "function", ["name"] = tool.Name }),
                null);
        }

        if (options.AllowedTools != null)
        {
            return new PreparedResponsesTools(openaiTools, AllowedToolChoice(options.AllowedTools, resolutions, aliases, options.ToolNameMapping, warnings), warnings);
        }

        return new PreparedResponsesTools(openaiTools, MapChoice(toolChoice, options.ToolNameMapping, customToolNames), warnings);
    }

    private static JsonNode AllowedToolChoice(
        OpenAIResponsesAllowedTools allowedTools,
        Dictionary<string, AllowedResolution> resolutions,
        Dictionary<string, AllowedResolution> aliases,
        ToolNameMapping? toolNameMapping,
        List<CallWarning> warnings)
    {
        var entries = new JsonArray();
        var dropped = new List<string>();
        foreach (var name in allowedTools.ToolNames)
        {
            var hasDirect = resolutions.TryGetValue(name, out var direct);
            var hasAlias = aliases.TryGetValue(name, out var alias);
            var resolution = hasDirect ? direct : hasAlias ? alias : null;
            var feature = "allowedTools entry \"" + name + "\"";
            if (hasDirect && hasAlias)
            {
                warnings.Add(new CallWarning(
                    "unsupported",
                    feature,
                    "this name is both a tool name and the provider tool name of another tool in this request; the tool with this name is allowed"));
            }

            if (resolution == AllowedResolution.Ambiguous)
            {
                warnings.Add(new CallWarning(
                    "unsupported",
                    feature,
                    "several tools in this request share this provider tool name; use the tool name from the tools for this request instead"));
                dropped.Add(name);
            }
            else if (resolution == null)
            {
                warnings.Add(new CallWarning(
                    "unsupported",
                    feature,
                    "the tool is not part of the tools for this request and is sent as a function tool"));
                entries.Add(new JsonObject { ["type"] = "function", ["name"] = toolNameMapping?.ToProviderToolName(name) ?? name });
            }
            else if (!resolution.Supported)
            {
                warnings.Add(new CallWarning("unsupported", feature, resolution.Reason + "; the tool is removed from the allowed tools"));
                dropped.Add(name);
            }
            else
            {
                entries.Add(resolution.Entry!.DeepClone());
            }
        }

        if (entries.Count == 0)
        {
            throw Unsupported("allowedTools with only tools that cannot be allow-listed (" + string.Join(", ", dropped) + ")");
        }

        return new JsonObject
        {
            ["type"] = "allowed_tools",
            ["mode"] = allowedTools.Mode ?? "auto",
            ["tools"] = entries,
        };
    }

    private static AllowedResolution ToAllowedResolution(JsonObject tool)
    {
        var type = tool["type"]!.GetValue<string>();
        if (type == "custom")
        {
            return AllowedResolution.Allow(new JsonObject { ["type"] = "custom", ["name"] = tool["name"]!.GetValue<string>() });
        }

        if (type == "mcp")
        {
            return AllowedResolution.Allow(new JsonObject { ["type"] = "mcp", ["server_label"] = tool["server_label"]!.GetValue<string>() });
        }

        return Array.IndexOf(AllowedBuiltInTypes, type) >= 0
            ? AllowedResolution.Allow(new JsonObject { ["type"] = type })
            : AllowedResolution.Reject("OpenAI does not support " + type + " tools in tool_choice.allowed_tools");
    }

    private static JsonNode? MapChoice(ToolChoice? toolChoice, ToolNameMapping? toolNameMapping, ISet<string> customToolNames)
    {
        if (toolChoice == null)
        {
            return null;
        }

        if (toolChoice.Type == "auto" || toolChoice.Type == "none" || toolChoice.Type == "required")
        {
            return JsonValue.Create(toolChoice.Type);
        }

        if (toolChoice is ToolChoice.NamedChoice named)
        {
            var resolved = toolNameMapping?.ToProviderToolName(named.ToolName) ?? named.ToolName;
            if (Array.IndexOf(HostedChoiceTypes, resolved) >= 0)
            {
                return new JsonObject { ["type"] = resolved };
            }

            return customToolNames.Contains(resolved)
                ? new JsonObject { ["type"] = "custom", ["name"] = resolved }
                : new JsonObject { ["type"] = "function", ["name"] = resolved };
        }

        return null;
    }

    private static UnsupportedFunctionalityException Unsupported(string functionality)
    {
        return new UnsupportedFunctionalityException(functionality, "functionality not supported: " + functionality);
    }

    private static JsonElement? OpenAIToolOptions(LanguageModelTool tool)
    {
        return tool.ProviderOptions is { } providerOptions ? OpenAIJson.Child(providerOptions, "openai") : null;
    }

    private static JsonObject PrepareFunctionTool(
        LanguageModelTool tool,
        JsonElement? openaiOptions,
        JsonElement? outputSchema,
        bool? async,
        List<CallWarning> warnings)
    {
        var input = OpenAIJsonSchema.Normalize(tool.InputSchema);
        var output = outputSchema is { } schema ? OpenAIJsonSchema.Normalize(schema) : null;
        foreach (var warning in input.Warnings.Concat(output?.Warnings ?? Array.Empty<OpenAICallWarning>()))
        {
            warnings.Add(warning.ToCallWarning());
        }

        var function = new JsonObject { ["type"] = "function", ["name"] = tool.Name };
        if (tool.Description != null)
        {
            function["description"] = tool.Description;
        }

        function["parameters"] = input.Schema;
        if (async != null)
        {
            function["async"] = async;
        }

        function["strict"] = tool.Strict ?? false;
        if (OpenAIJson.Bool(openaiOptions, "deferLoading") is { } deferLoading)
        {
            function["defer_loading"] = deferLoading;
        }

        if (OpenAIJson.Child(openaiOptions, "allowedCallers") is { ValueKind: JsonValueKind.Array } allowedCallers)
        {
            function["allowed_callers"] = JsonNode.Parse(allowedCallers.GetRawText());
        }

        if (output != null)
        {
            function["output_schema"] = output.Schema;
        }

        return function;
    }

    private static bool? ResolveAsync(bool? value, bool supportsAsyncToolCalling, string toolName, List<CallWarning> warnings)
    {
        if (value != true || supportsAsyncToolCalling)
        {
            return value;
        }

        warnings.Add(new CallWarning(
            "unsupported",
            "async tool calling for \"" + toolName + "\"",
            "Async tool calling is only supported by GPT-6 and later models."));
        return null;
    }

    private static JsonObject? MapProviderTool(LanguageModelTool tool, PrepareResponsesToolsOptions options, List<CallWarning> warnings)
    {
        var args = OpenAIResponsesToolArgs.Validate(tool.ProviderToolId!, tool.Args) ?? new JsonObject();

        JsonNode? Arg(string name) => args[name]?.DeepClone();

        JsonObject? Object(string name) => args[name] as JsonObject;

        void Set(JsonObject target, string name, JsonNode? value)
        {
            if (value != null)
            {
                target[name] = value;
            }
        }

        var result = new JsonObject();
        switch (tool.ProviderToolId)
        {
            case "openai.file_search":
                result["type"] = "file_search";
                Set(result, "vector_store_ids", Arg("vectorStoreIds"));
                Set(result, "max_num_results", Arg("maxNumResults"));
                if (Object("ranking") is { } ranking)
                {
                    var rankingOptions = new JsonObject();
                    Set(rankingOptions, "ranker", ranking["ranker"]?.DeepClone());
                    Set(rankingOptions, "score_threshold", ranking["scoreThreshold"]?.DeepClone());
                    result["ranking_options"] = rankingOptions;
                }

                Set(result, "filters", Arg("filters"));
                return result;
            case "openai.local_shell":
            case "openai.apply_patch":
            case "openai.computer":
            case "openai.programmatic_tool_calling":
                result["type"] = tool.ProviderToolId.Substring("openai.".Length);
                return result;
            case "openai.shell":
                result["type"] = "shell";
                if (Object("environment") is { } environment)
                {
                    result["environment"] = MapShellEnvironment(environment);
                }

                return result;
            case "openai.web_search_preview":
                result["type"] = "web_search_preview";
                Set(result, "search_context_size", Arg("searchContextSize"));
                Set(result, "user_location", Arg("userLocation"));
                return result;
            case "openai.web_search":
                result["type"] = "web_search";
                if (Object("filters") is { } filters)
                {
                    var mappedFilters = new JsonObject();
                    Set(mappedFilters, "allowed_domains", filters["allowedDomains"]?.DeepClone());
                    Set(mappedFilters, "blocked_domains", filters["blockedDomains"]?.DeepClone());
                    result["filters"] = mappedFilters;
                }

                Set(result, "external_web_access", Arg("externalWebAccess"));
                Set(result, "search_context_size", Arg("searchContextSize"));
                Set(result, "user_location", Arg("userLocation"));
                return result;
            case "openai.code_interpreter":
                result["type"] = "code_interpreter";
                var container = Arg("container");
                if (container is JsonValue)
                {
                    result["container"] = container;
                }
                else
                {
                    var auto = new JsonObject { ["type"] = "auto" };
                    Set(auto, "file_ids", (container as JsonObject)?["fileIds"]?.DeepClone());
                    result["container"] = auto;
                }

                return result;
            case "openai.image_generation":
                result["type"] = "image_generation";
                Set(result, "action", Arg("action"));
                Set(result, "background", Arg("background"));
                Set(result, "input_fidelity", Arg("inputFidelity"));
                if (Object("inputImageMask") is { } mask)
                {
                    var mappedMask = new JsonObject();
                    Set(mappedMask, "file_id", mask["fileId"]?.DeepClone());
                    Set(mappedMask, "image_url", mask["imageUrl"]?.DeepClone());
                    result["input_image_mask"] = mappedMask;
                }

                Set(result, "model", Arg("model"));
                Set(result, "moderation", Arg("moderation"));
                Set(result, "partial_images", Arg("partialImages"));
                Set(result, "quality", Arg("quality"));
                Set(result, "output_compression", Arg("outputCompression"));
                Set(result, "output_format", Arg("outputFormat"));
                Set(result, "size", Arg("size"));
                return result;
            case "openai.mcp":
                result["type"] = "mcp";
                Set(result, "server_label", Arg("serverLabel"));
                var allowed = Arg("allowedTools");
                if (allowed is JsonArray)
                {
                    result["allowed_tools"] = allowed;
                }
                else if (allowed is JsonObject allowedObject)
                {
                    var mappedAllowed = new JsonObject();
                    Set(mappedAllowed, "read_only", allowedObject["readOnly"]?.DeepClone());
                    Set(mappedAllowed, "tool_names", allowedObject["toolNames"]?.DeepClone());
                    result["allowed_tools"] = mappedAllowed;
                }

                Set(result, "authorization", Arg("authorization"));
                Set(result, "connector_id", Arg("connectorId"));
                Set(result, "headers", Arg("headers"));
                var approval = Arg("requireApproval");
                JsonNode requireApproval = "never";
                if (approval is JsonValue)
                {
                    requireApproval = approval;
                }
                else if (approval is JsonObject approvalObject && approvalObject["never"] is JsonObject never)
                {
                    var names = new JsonObject();
                    Set(names, "tool_names", never["toolNames"]?.DeepClone());
                    requireApproval = new JsonObject { ["never"] = names };
                }

                result["require_approval"] = requireApproval;
                Set(result, "server_description", Arg("serverDescription"));
                Set(result, "server_url", Arg("serverUrl"));
                return result;
            case "openai.custom":
                result["type"] = "custom";
                result["name"] = tool.Name;
                Set(result, "description", Arg("description"));
                if (ResolveAsync(args["async"]?.GetValue<bool>(), options.SupportsAsyncToolCalling, tool.Name, warnings) != null)
                {
                    result["async"] = args["async"]!.DeepClone();
                }

                Set(result, "format", Arg("format"));
                return result;
            case "openai.tool_search":
                result["type"] = "tool_search";
                Set(result, "execution", Arg("execution"));
                Set(result, "description", Arg("description"));
                Set(result, "parameters", Arg("parameters"));
                return result;
            default:
                return null;
        }
    }

    private static JsonObject MapShellEnvironment(JsonObject environment)
    {
        var type = environment["type"]?.GetValue<string>();
        var result = new JsonObject();
        if (type == "containerReference")
        {
            result["type"] = "container_reference";
            result["container_id"] = environment["containerId"]?.DeepClone();
            return result;
        }

        if (type == "containerAuto")
        {
            result["type"] = "container_auto";
            if (environment["fileIds"] is { } fileIds)
            {
                result["file_ids"] = fileIds.DeepClone();
            }

            if (environment["memoryLimit"] is { } memoryLimit)
            {
                result["memory_limit"] = memoryLimit.DeepClone();
            }

            if (environment["networkPolicy"] is JsonObject policy)
            {
                if (policy["type"]?.GetValue<string>() == "disabled")
                {
                    result["network_policy"] = new JsonObject { ["type"] = "disabled" };
                }
                else
                {
                    var allowlist = new JsonObject
                    {
                        ["type"] = "allowlist",
                        ["allowed_domains"] = policy["allowedDomains"]?.DeepClone(),
                    };
                    if (policy["domainSecrets"] is { } secrets)
                    {
                        allowlist["domain_secrets"] = secrets.DeepClone();
                    }

                    result["network_policy"] = allowlist;
                }
            }

            if (environment["skills"] is JsonArray skills)
            {
                result["skills"] = new JsonArray(skills.Select(skill => MapShellSkill(skill!.AsObject())).ToArray());
            }

            return result;
        }

        result["type"] = "local";
        if (environment["skills"] is { } localSkills)
        {
            result["skills"] = localSkills.DeepClone();
        }

        return result;
    }

    private static JsonNode MapShellSkill(JsonObject skill)
    {
        if (skill["type"]!.GetValue<string>() == "skillReference")
        {
            var reference = skill["providerReference"]!.AsObject().ToDictionary(
                property => property.Key,
                property => property.Value!.GetValue<string>());
            return new JsonObject
            {
                ["type"] = "skill_reference",
                ["skill_id"] = ProviderReferences.ResolveProviderReference(reference, "openai"),
                ["version"] = skill["version"]?.GetValue<string>() ?? "latest",
            };
        }

        var source = skill["source"]!;
        return new JsonObject
        {
            ["type"] = "inline",
            ["name"] = skill["name"]!.DeepClone(),
            ["description"] = skill["description"]!.DeepClone(),
            ["source"] = new JsonObject
            {
                ["type"] = "base64",
                ["media_type"] = source["mediaType"]!.DeepClone(),
                ["data"] = source["data"]!.DeepClone(),
            },
        };
    }
}
