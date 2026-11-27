// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.OpenAI;

/// <summary>Responses API request body and the warnings produced while building it.</summary>
public sealed class OpenAIResponsesPreparedRequest
{
    internal OpenAIResponsesPreparedRequest(
        JsonObject body,
        IReadOnlyList<OpenAICallWarning> warnings
    )
    {
        Body = body;
        Warnings = warnings;
    }

    /// <summary>JSON body sent to <c>/responses</c>.</summary>
    public JsonObject Body { get; }

    /// <summary>Warnings produced while preparing the request.</summary>
    public IReadOnlyList<OpenAICallWarning> Warnings { get; }
}

/// <summary>OpenAI Responses API language model.</summary>
public sealed class OpenAIResponsesLanguageModel : ILanguageModel
{
    /// <summary>URLs the Responses API reads itself. Maps to <c>openaiResponsesSupportedUrls</c>.</summary>
    internal static readonly IReadOnlyDictionary<string, string> ResponsesSupportedUrls =
        new Dictionary<string, string>
        {
            ["image/*"] = "^https?://.*\\z",
            ["application/pdf"] = "^https?://.*\\z",
        };

    private static readonly JsonSerializerOptions CustomToolInputOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly ProviderHttp _http;
    private readonly string _providerName;
    private readonly string _providerOptionsName;
    private readonly string? _requestModelId;
    private readonly Func<Uri> _url;
    private readonly bool _explicitMessageItemType;
    private readonly Func<
        IReadOnlyDictionary<string, string?>?,
        CancellationToken,
        Task<Dictionary<string, string?>>
    > _headers;

    /// <summary>
    /// Whether a request with a <c>web_search</c> or <c>web_search_preview</c> tool adds
    /// <c>web_search_call.action.sources</c> to <c>include</c>. The default is true. The
    /// <c>includeWebSearchSources</c> provider option set to false also turns it off.
    /// </summary>
    public bool SupportsWebSearchSourcesInclude { get; init; } = true;

    /// <summary>Creates a Responses model.</summary>
    public OpenAIResponsesLanguageModel(OpenAIProvider provider, string modelId)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _http = provider.Http;
        _providerName = provider.Name;
        _providerOptionsName = "openai";
        _url = () => ApiKeys.Combine(provider.Options.BaseUrl, "responses");
        _headers = provider.CreateOpenAIHeadersAsync;
    }

    /// <summary>
    /// Creates an Azure OpenAI Responses model. Message and request options are read from the
    /// <c>azure</c> provider options first and fall back to <c>openai</c> when <c>azure</c> is absent.
    /// </summary>
    public OpenAIResponsesLanguageModel(Azure.AzureOpenAIProvider provider, string modelId)
        : this(provider, modelId, null) { }

    /// <summary>
    /// Creates an Azure OpenAI Responses model for a deployment. <see cref="ModelId"/> reports
    /// <paramref name="settings"/>.<see cref="Azure.AzureOpenAILanguageModelSettings.ModelId"/> when set and
    /// drives capability detection. Requests always use <paramref name="deploymentId"/>.
    /// </summary>
    public OpenAIResponsesLanguageModel(
        Azure.AzureOpenAIProvider provider,
        string deploymentId,
        Azure.AzureOpenAILanguageModelSettings? settings
    )
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        _requestModelId = deploymentId ?? throw new ArgumentNullException(nameof(deploymentId));
        ModelId = settings?.ModelId ?? deploymentId;
        _http = provider.Http;
        _providerName = provider.Name;
        _providerOptionsName = "azure";
        _url = () => provider.AzureUri("responses", deploymentId);
        _explicitMessageItemType = Azure
            .AzureOpenAIProvider.GetAzureOpenAIBaseURLInfo(provider.Options.BaseUrl)
            .IsFoundryProject;
        _headers = provider.CreateHeadersAsync;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> SupportedUrls => ResponsesSupportedUrls;

    /// <inheritdoc />
    public string Provider => _providerName + ".responses";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>
    /// Prepares Responses tools and the tool choice. Strict is false unless the tool sets <see cref="LanguageModelTool.Strict"/>.
    /// Maps to <c>prepareResponsesTools</c>.
    /// </summary>
    /// <exception cref="ProviderUtils.TypeValidationException">The arguments of a provider-defined tool are invalid.</exception>
    /// <exception cref="UnsupportedFunctionalityException">The tools or the tool choice cannot be expressed for OpenAI.</exception>
    public static PreparedResponsesTools PrepareResponsesTools(
        IReadOnlyList<LanguageModelTool>? tools,
        ToolChoice? toolChoice,
        PrepareResponsesToolsOptions? options = null
    )
    {
        return OpenAIResponsesPrepareTools.Prepare(tools, toolChoice, options);
    }

    private static readonly IReadOnlyDictionary<string, string> ProviderToolNames = new Dictionary<
        string,
        string
    >
    {
        ["openai.code_interpreter"] = "code_interpreter",
        ["openai.computer"] = "computer",
        ["openai.file_search"] = "file_search",
        ["openai.image_generation"] = "image_generation",
        ["openai.local_shell"] = "local_shell",
        ["openai.shell"] = "shell",
        ["openai.web_search"] = "web_search",
        ["openai.web_search_preview"] = "web_search_preview",
        ["openai.mcp"] = "mcp",
        ["openai.apply_patch"] = "apply_patch",
        ["openai.tool_search"] = "tool_search",
        ["openai.programmatic_tool_calling"] = "programmatic_tool_calling",
    };

    internal static ToolNameMapping CreateToolNameMapping(
        IReadOnlyList<LanguageModelTool>? tools
    ) => ToolNames.CreateToolNameMapping(MappedTools(tools), ProviderToolNames);

    /// <summary>Whether the shell tool of the request runs in a hosted container, so its calls are provider-executed.</summary>
    internal static bool IsShellProviderExecuted(IReadOnlyList<LanguageModelTool>? tools)
    {
        var shell = tools?.FirstOrDefault(tool =>
            tool.IsProviderDefined && tool.ProviderToolId == "openai.shell"
        );
        if (
            shell is null
            || shell.Args.ValueKind != JsonValueKind.Object
            || !shell.Args.TryGetProperty("environment", out var environment)
            || environment.ValueKind != JsonValueKind.Object
        )
        {
            return false;
        }

        return OpenAIJson.String(environment, "type") is "containerAuto" or "containerReference";
    }

    /// <summary>The client name of the web search tool, if the request defines one.</summary>
    internal static string? WebSearchToolName(IReadOnlyList<LanguageModelTool>? tools) =>
        tools
            ?.FirstOrDefault(tool =>
                tool.ProviderToolId == "openai.web_search"
                || tool.ProviderToolId == "openai.web_search_preview"
            )
            ?.Name;

    private static IReadOnlyList<MappedTool>? MappedTools(IReadOnlyList<LanguageModelTool>? tools)
    {
        return tools
            ?.Select(tool =>
                tool.IsProviderDefined
                    ? new MappedTool("provider", tool.Name, tool.ProviderToolId)
                    : new MappedTool("function", tool.Name)
            )
            .ToArray();
    }

    private static OpenAIResponsesAllowedTools? ParseAllowedTools(JsonElement? openai)
    {
        if (OpenAIJson.Child(openai, "allowedTools") is not { } allowed)
        {
            return null;
        }

        var mode = OpenAIJson.String(allowed, "mode");
        var names = OpenAIJson.Child(allowed, "toolNames");
        if (
            allowed.ValueKind != JsonValueKind.Object
            || names is not { ValueKind: JsonValueKind.Array } nameArray
            || nameArray.GetArrayLength() == 0
            || nameArray.EnumerateArray().Any(name => name.ValueKind != JsonValueKind.String)
            || OpenAIJson.Child(allowed, "mode") != null && mode != "auto" && mode != "required"
        )
        {
            throw new InvalidArgumentException(
                "providerOptions",
                allowed.GetRawText(),
                "invalid openai provider options"
            );
        }

        return new OpenAIResponsesAllowedTools(
            nameArray.EnumerateArray().Select(name => name.GetString()!).ToArray(),
            mode
        );
    }

    private static JsonNode? ParsePromptCacheOptions(JsonElement? options)
    {
        if (options is not { } value || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var mode = OpenAIJson.String(value, "mode");
        var ttl = OpenAIJson.String(value, "ttl");
        var prewarm = OpenAIJson.Child(value, "prewarm");
        var comparisonResponseId = OpenAIJson.Child(value, "comparisonResponseId");
        if (
            value.ValueKind != JsonValueKind.Object
            || OpenAIJson.Child(value, "mode") != null && mode != "implicit" && mode != "explicit"
            || OpenAIJson.Child(value, "ttl") != null && ttl != "30m" && ttl != "12h"
            || prewarm is { } rawPrewarm
                && rawPrewarm.ValueKind != JsonValueKind.True
                && rawPrewarm.ValueKind != JsonValueKind.False
            || comparisonResponseId is { } rawComparison
                && rawComparison.ValueKind != JsonValueKind.String
        )
        {
            throw new InvalidArgumentException(
                "providerOptions",
                value.GetRawText(),
                "invalid openai provider options"
            );
        }

        var parsed = new JsonObject();
        if (mode != null)
        {
            parsed["mode"] = mode;
        }

        if (ttl != null)
        {
            parsed["ttl"] = ttl;
        }

        if (prewarm is { } flag)
        {
            parsed["prewarm"] = flag.GetBoolean();
        }

        if (comparisonResponseId is { } comparison)
        {
            parsed["comparison_response_id"] = comparison.GetString();
        }

        return parsed;
    }

    /// <summary>Builds the Responses body for <paramref name="modelId"/>. Generate calls omit <c>stream</c>.</summary>
    public static OpenAIResponsesPreparedRequest Prepare(
        string modelId,
        LanguageModelCallOptions options,
        bool stream
    )
    {
        return Prepare(modelId, options, stream, "openai");
    }

    /// <summary>
    /// Builds the Responses body. Options come from <paramref name="providerOptionsName"/> and fall back to
    /// <c>openai</c> when that key is absent.
    /// </summary>
    public static OpenAIResponsesPreparedRequest Prepare(
        string modelId,
        LanguageModelCallOptions options,
        bool stream,
        string providerOptionsName,
        bool supportsWebSearchSourcesInclude = true
    )
    {
        return Prepare(
            modelId,
            options,
            stream,
            providerOptionsName,
            supportsWebSearchSourcesInclude,
            null
        );
    }

    /// <summary>
    /// Builds the Responses body. <paramref name="modelId"/> drives capability detection and
    /// <paramref name="requestModelId"/>, when set, is sent as the request <c>model</c>.
    /// <paramref name="explicitMessageItemType"/> adds <c>type: "message"</c> to message items (Foundry projects).
    /// </summary>
    public static OpenAIResponsesPreparedRequest Prepare(
        string modelId,
        LanguageModelCallOptions options,
        bool stream,
        string providerOptionsName,
        bool supportsWebSearchSourcesInclude,
        string? requestModelId,
        bool explicitMessageItemType = false
    )
    {
        options ??= new LanguageModelCallOptions();
        var warnings = new List<OpenAICallWarning>();
        var openai = ProviderOptionsFor(options.ProviderOptions, providerOptionsName);
        var capabilities = OpenAILanguageModelCapabilities.ForModel(modelId);
        var effort = OpenAILanguageModelCapabilities.ResolveOpenAIReasoningEffort(
            OpenAIJson.String(openai, "reasoningEffort"),
            options.Reasoning,
            capabilities.SupportedReasoningEfforts,
            warnings
        );

        // An explicit null reasoningSummary turns the default off.
        var summaryExplicit =
            openai is { ValueKind: JsonValueKind.Object } summaryHolder
            && summaryHolder.TryGetProperty("reasoningSummary", out _);
        var reasoningSummary =
            summaryExplicit ? OpenAIJson.String(openai, "reasoningSummary")
            : effort != null && effort != "none" ? "detailed"
            : null;
        var reasoning = OpenAIJson.Bool(openai, "forceReasoning") ?? capabilities.IsReasoningModel;
        if (
            OpenAIJson.String(openai, "conversation") != null
            && OpenAIJson.String(openai, "previousResponseId") != null
        )
        {
            warnings.Add(
                new OpenAICallWarning(
                    "unsupported",
                    "conversation",
                    "conversation and previousResponseId cannot be used together"
                )
            );
        }

        if (options.TopK != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "topK", null));
        }

        var systemMode = OpenAIJson.String(openai, "systemMessageMode")
            ?? (reasoning ? "developer" : capabilities.SystemMessageMode);
        var explicitItem =
            explicitMessageItemType || OpenAIJson.Bool(openai, "explicitMessageItemType") == true;
        var configurationUpdateUnsupportedReason = GetConfigurationUpdateUnsupportedReason(
            capabilities,
            openai
        );
        var hasContinuation =
            OpenAIJson.String(openai, "conversation") != null
            || OpenAIJson.String(openai, "previousResponseId") != null;
        var outputSchemaToolNames = new HashSet<string>();
        var customProviderToolNames = new HashSet<string>();
        var toolNameMapping = CreateToolNameMapping(options.Tools);
        var prepared = PrepareResponsesTools(
            options.Tools,
            options.ToolChoice,
            new PrepareResponsesToolsOptions
            {
                AllowedTools = ParseAllowedTools(openai),
                ToolNameMapping = toolNameMapping,
                CustomProviderToolNames = customProviderToolNames,
                OutputSchemaToolNames = outputSchemaToolNames,
                SupportsAsyncToolCalling = capabilities.SupportsAsyncToolCalling,
                ProviderOptionsName = providerOptionsName,
            }
        );
        var allowedTools = ParseAllowedTools(openai);
        var allowedToolNames =
            allowedTools == null
                ? null
                : new HashSet<string>(
                    allowedTools.ToolNames.Select(name => toolNameMapping.ToProviderToolName(name))
                );
        var additionalToolResolutions = new Dictionary<string, AdditionalToolResolution?>();

        // Prompts can contain client-supplied conversation history. Only the tools registered for this request may
        // be loaded from such history; each entry resolves to the definition derived from its registered tool. Cache
        // the resolutions because replayed history can repeat an entry.
        AdditionalToolResolution? FindAdditionalToolForRequest(string type, string name)
        {
            var key = type + ":" + name;
            if (!additionalToolResolutions.TryGetValue(key, out var resolution))
            {
                resolution = OpenAIResponsesPrepareTools.FindAdditionalToolDefinition(
                    options.Tools,
                    (type, name),
                    providerOptionsName,
                    allowedToolNames
                );
                additionalToolResolutions[key] = resolution;
            }

            return resolution;
        }

        var converted = ConvertInput(
            options.Prompt,
            systemMode,
            explicitItem,
            configurationUpdateUnsupportedReason,
            providerOptionsName,
            hasContinuation,
            OpenAIJson.Bool(openai, "store") ?? true,
            customToolNames: customProviderToolNames.Count > 0 ? customProviderToolNames : null,
            outputSchemaToolNames: outputSchemaToolNames,
            isReasoningModel: reasoning,
            hasConversation: OpenAIJson.String(openai, "conversation") != null,
            toolNameMapping: toolNameMapping,
            hasApplyPatchTool: options.Tools?.Any(tool =>
                tool.IsProviderDefined && tool.ProviderToolId == "openai.apply_patch"
            ) == true,
            hasLocalShellTool: options.Tools?.Any(tool =>
                tool.IsProviderDefined && tool.ProviderToolId == "openai.local_shell"
            ) == true,
            hasPreviousResponseId: OpenAIJson.String(openai, "previousResponseId") != null,
            findAdditionalToolDefinition: FindAdditionalToolForRequest
        );
        warnings.AddRange(converted.Warnings);

        // The schema accepts update efforts supported by any model. Check this model's list.
        string? UpdateEffortUnsupportedReason(string? updateEffort) =>
            updateEffort != null
            && capabilities.SupportedReasoningEfforts != null
            && !Contains(capabilities.SupportedReasoningEfforts, updateEffort)
                ? modelId
                    + " only supports the following reasoning efforts: "
                    + string.Join(", ", capabilities.SupportedReasoningEfforts)
                : null;

        foreach (var item in converted.Input)
        {
            if (
                item is JsonObject { } update
                && update["type"]?.GetValue<string>() == "configuration_update"
            )
            {
                var reason = UpdateEffortUnsupportedReason(
                    update["reasoning"]?["effort"]?.GetValue<string>()
                );
                if (reason != null)
                {
                    throw new UnsupportedFunctionalityException(
                        "Message-level reasoningEffortUpdate",
                        reason
                    );
                }
            }
        }

        var effortUpdate = OpenAIJson.String(openai, "reasoningEffortUpdate");
        if (effortUpdate != null)
        {
            ValidateUpdateEffort(effortUpdate);
            var requestReason = configurationUpdateUnsupportedReason
                ?? UpdateEffortUnsupportedReason(effortUpdate);
            if (requestReason != null)
            {
                warnings.Add(
                    new OpenAICallWarning("unsupported", "reasoningEffortUpdate", requestReason)
                );
            }
            else if (
                !(
                    converted.Input.Count > 0
                    && converted.Input[0] is JsonObject { } first
                    && first["type"]?.GetValue<string>() == "configuration_update"
                    && first["reasoning"]?["effort"]?.GetValue<string>() == effortUpdate
                )
            )
            {
                // An identical first item would otherwise leave two adjacent updates, which OpenAI rejects.
                converted.Input.Insert(0, ConfigurationUpdate(effortUpdate));
            }
        }

        for (var i = 1; i < converted.Input.Count; i++)
        {
            if (
                IsConfigurationUpdate(converted.Input[i - 1])
                && IsConfigurationUpdate(converted.Input[i])
            )
            {
                throw new UnsupportedFunctionalityException(
                    "Adjacent configuration updates",
                    "Configuration updates cannot be consecutive. Set reasoningEffortUpdate and textFormatUpdate in the same empty system message, and omit request-level reasoningEffortUpdate when using message-level updates."
                );
            }
        }
        // A compaction trigger is a request control and must be the final input item.
        if (OpenAIJson.Bool(openai, "compactionTrigger") == true)
        {
            converted.Input.Add(new JsonObject { ["type"] = "compaction_trigger" });
        }

        var body = new JsonObject
        {
            ["model"] = requestModelId ?? modelId,
            ["input"] = converted.Input,
        };
        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            body["top_p"] = topP;
        }

        if (options.MaxOutputTokens is { } maxOutput)
        {
            body["max_output_tokens"] = maxOutput;
        }

        var textBody = new JsonObject();
        if (OpenAIJson.Child(openai, "textFormat") is { } textFormat)
        {
            textBody["format"] = OpenAIResponsesTextFormat.Normalize(textFormat, warnings);
        }
        else if (options.JsonSchema is { } responseSchema)
        {
            var normalizedSchema = OpenAIJsonSchema.Normalize(responseSchema);
            warnings.AddRange(normalizedSchema.Warnings);
            var responseFormat = new JsonObject
            {
                ["type"] = "json_schema",
                ["strict"] = OpenAIJson.Bool(openai, "strictJsonSchema") ?? true,
                ["name"] = string.IsNullOrEmpty(options.JsonSchemaName)
                    ? "response"
                    : options.JsonSchemaName,
            };
            if (options.JsonSchemaDescription != null)
            {
                responseFormat["description"] = options.JsonSchemaDescription;
            }

            responseFormat["schema"] = normalizedSchema.Schema;
            textBody["format"] = responseFormat;
        }
        else if (options.JsonResponse)
        {
            textBody["format"] = new JsonObject { ["type"] = "json_object" };
        }

        if (OpenAIJson.String(openai, "textVerbosity") is { Length: > 0 } verbosity)
        {
            textBody["verbosity"] = verbosity;
        }

        if (textBody.Count > 0)
        {
            body["text"] = textBody;
        }

        if (OpenAIJson.Int(openai, "maxToolCalls") is { } maxToolCalls)
        {
            body["max_tool_calls"] = maxToolCalls;
        }

        Copy(body, "metadata", OpenAIJson.Node(OpenAIJson.Child(openai, "metadata")));
        if (OpenAIJson.Bool(openai, "parallelToolCalls") is { } parallel)
        {
            body["parallel_tool_calls"] = parallel;
        }

        Copy(body, "conversation", OpenAIJson.String(openai, "conversation"));
        Copy(body, "previous_response_id", OpenAIJson.String(openai, "previousResponseId"));
        if (OpenAIJson.Bool(openai, "store") is { } store)
        {
            body["store"] = store;
        }

        Copy(body, "user", OpenAIJson.String(openai, "user"));
        Copy(body, "instructions", OpenAIJson.String(openai, "instructions"));
        Copy(body, "service_tier", OpenAIJson.String(openai, "serviceTier"));
        Copy(body, "prompt_cache_key", OpenAIJson.String(openai, "promptCacheKey"));
        Copy(
            body,
            "prompt_cache_options",
            ParsePromptCacheOptions(OpenAIJson.Child(openai, "promptCacheOptions"))
        );
        Copy(body, "prompt_cache_retention", OpenAIJson.String(openai, "promptCacheRetention"));
        Copy(body, "safety_identifier", OpenAIJson.String(openai, "safetyIdentifier"));
        Copy(body, "truncation", OpenAIJson.String(openai, "truncation"));
        if (
            OpenAIJson.Child(openai, "contextManagement") is
            { ValueKind: JsonValueKind.Array } contextManagement
        )
        {
            var managed = new JsonArray();
            foreach (var entry in contextManagement.EnumerateArray())
            {
                var managedEntry = new JsonObject { ["type"] = OpenAIJson.String(entry, "type") };
                if (OpenAIJson.Child(entry, "compactThreshold") is { } threshold)
                {
                    managedEntry["compact_threshold"] = OpenAIJson.Node(threshold);
                }

                managed.Add(managedEntry);
            }

            body["context_management"] = managed;
        }

        var reasoningMode = OpenAIJson.String(openai, "reasoningMode");
        var reasoningContext = OpenAIJson.String(openai, "reasoningContext");
        if (
            reasoning
            && (
                effort != null
                || reasoningSummary != null
                || reasoningMode != null
                || reasoningContext != null
            )
        )
        {
            var reasoningBody = new JsonObject();
            if (effort != null)
            {
                reasoningBody["effort"] = effort;
            }

            if (reasoningSummary != null)
            {
                reasoningBody["summary"] = reasoningSummary;
            }

            if (reasoningMode != null)
            {
                reasoningBody["mode"] = reasoningMode;
            }

            if (reasoningContext != null)
            {
                reasoningBody["context"] = reasoningContext;
            }

            body["reasoning"] = reasoningBody;
        }
        else if (!reasoning && OpenAIJson.String(openai, "reasoningEffort") != null)
        {
            warnings.Add(
                new OpenAICallWarning(
                    "unsupported",
                    "reasoningEffort",
                    "reasoningEffort is not supported for non-reasoning models"
                )
            );
        }

        if (!reasoning)
        {
            if (summaryExplicit && reasoningSummary != null)
            {
                warnings.Add(
                    new OpenAICallWarning(
                        "unsupported",
                        "reasoningSummary",
                        "reasoningSummary is not supported for non-reasoning models"
                    )
                );
            }

            if (reasoningMode != null)
            {
                warnings.Add(
                    new OpenAICallWarning(
                        "unsupported",
                        "reasoningMode",
                        "reasoningMode is not supported for non-reasoning models"
                    )
                );
            }

            if (reasoningContext != null)
            {
                warnings.Add(
                    new OpenAICallWarning(
                        "unsupported",
                        "reasoningContext",
                        "reasoningContext is not supported for non-reasoning models"
                    )
                );
            }
        }

        if (capabilities.SupportsConfigurationUpdate && body["prompt_cache_retention"] != null)
        {
            body.Remove("prompt_cache_retention");
            warnings.Add(
                new OpenAICallWarning(
                    "unsupported",
                    "promptCacheRetention",
                    "promptCacheRetention is not supported by GPT-6 and later models; use promptCacheOptions instead"
                )
            );
        }

        var topLogprobs = OpenAIJson.Int(openai, "logprobs")
            ?? (OpenAIJson.Bool(openai, "logprobs") == true ? 20 : null);
        var include = new List<string>();
        if (OpenAIJson.Child(openai, "include") is { ValueKind: JsonValueKind.Array } includeValues)
        {
            foreach (var value in includeValues.EnumerateArray())
            {
                if (
                    value.ValueKind == JsonValueKind.String
                    && !include.Contains(value.GetString()!)
                )
                {
                    include.Add(value.GetString()!);
                }
            }
        }

        if (topLogprobs is > 0)
        {
            body["top_logprobs"] = topLogprobs;
            if (!include.Contains("message.output_text.logprobs"))
            {
                include.Add("message.output_text.logprobs");
            }
        }

        if (
            supportsWebSearchSourcesInclude
            && OpenAIJson.Bool(openai, "includeWebSearchSources") != false
            && options.Tools?.Any(tool =>
                tool.ProviderToolId == "openai.web_search"
                || tool.ProviderToolId == "openai.web_search_preview"
            ) == true
            && !include.Contains("web_search_call.action.sources")
        )
        {
            include.Add("web_search_call.action.sources");
        }

        // When a code interpreter tool is present, the outputs are included automatically.
        if (
            options.Tools?.Any(tool => tool.ProviderToolId == "openai.code_interpreter") == true
            && !include.Contains("code_interpreter_call.outputs")
        )
        {
            include.Add("code_interpreter_call.outputs");
        }

        if (include.Count > 0)
        {
            var includeArray = new JsonArray();
            foreach (var value in include)
            {
                includeArray.Add(value);
            }

            body["include"] = includeArray;
        }

        if (reasoning)
        {
            // Input updates change the effort used to validate sampling parameters.
            var effectiveEffort = effort;
            foreach (var item in converted.Input)
            {
                if (IsConfigurationUpdate(item))
                {
                    effectiveEffort = item!["reasoning"]?["effort"]?.GetValue<string>()
                        ?? effectiveEffort;
                }
            }

            if (!(effectiveEffort == "none" && capabilities.SupportsNonReasoningParameters))
            {
                Remove(
                    body,
                    warnings,
                    "temperature",
                    "temperature",
                    "temperature is not supported for reasoning models"
                );
                Remove(
                    body,
                    warnings,
                    "top_p",
                    "topP",
                    "topP is not supported for reasoning models"
                );
                if (
                    capabilities.SupportsConfigurationUpdate
                    && (
                        body["top_logprobs"] != null
                        || include.Contains("message.output_text.logprobs")
                    )
                )
                {
                    body.Remove("top_logprobs");
                    include.Remove("message.output_text.logprobs");
                    if (include.Count == 0)
                    {
                        body.Remove("include");
                    }
                    else
                    {
                        var kept = new JsonArray();
                        foreach (var value in include)
                        {
                            kept.Add(value);
                        }

                        body["include"] = kept;
                    }

                    warnings.Add(
                        new OpenAICallWarning(
                            "unsupported",
                            "logprobs",
                            "logprobs is not supported for reasoning models"
                        )
                    );
                }
            }
        }

        var serviceTier = OpenAIJson.String(openai, "serviceTier");
        if (serviceTier == "flex" && !capabilities.SupportsFlexProcessing)
        {
            body.Remove("service_tier");
            warnings.Add(
                new OpenAICallWarning(
                    "unsupported",
                    "serviceTier",
                    "flex processing is only available for o3, o4-mini, and gpt-5 models"
                )
            );
        }

        foreach (var warning in prepared.ToolWarnings)
        {
            warnings.Add(new OpenAICallWarning(warning.Type, warning.Feature, warning.Message));
        }

        if (prepared.Tools is { Count: > 0 })
        {
            body["tools"] = prepared.Tools;
        }

        var choice = prepared.ToolChoice;
        if (choice != null)
        {
            body["tool_choice"] = choice;
        }

        if (stream)
        {
            body["stream"] = true;
        }

        return new OpenAIResponsesPreparedRequest(body, warnings);
    }

    /// <summary>The id generator of the model config, <c>config.generateId</c> upstream. Tests set it to get predictable ids.</summary>
    internal Func<string>? GenerateId { get; set; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        var prepared = Prepare(
            ModelId,
            options,
            false,
            _providerOptionsName,
            SupportsWebSearchSourcesInclude,
            _requestModelId,
            _explicitMessageItemType
        );
        var response = await _http
            .SendJsonStringAsync(
                HttpMethod.Post,
                _url(),
                prepared.Body.ToJsonString(),
                await _headers(options.Headers, cancellationToken).ConfigureAwait(false),
                cancellationToken
            )
            .ConfigureAwait(false);
        using var document = JsonDocument.Parse(
            string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body
        );
        var parsedResponse = OpenAIResponsesSchema.ParseResponse(document.RootElement)
            ?? throw new ProviderUtils.TypeValidationException(
                "Type validation failed: Value: " + document.RootElement.GetRawText() + ".",
                document.RootElement.Clone()
            );
        var namedOptions =
            options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue(_providerOptionsName, out var providerOptions)
                ? providerOptions
                : (JsonElement?)null;
        var collectLogprobs =
            OpenAIJson.Bool(namedOptions, "logprobs") == true
            || OpenAIJson.Int(namedOptions, "logprobs") is not null and not 0;
        return Parse(
                parsedResponse,
                prepared.Warnings,
                response.Body,
                response.Headers,
                GenerateId,
                OpenAIParallelToolCall.FunctionToolNames(options.Tools),
                CreateToolNameMapping(options.Tools),
                options.JsonResponse || options.JsonSchema != null,
                _providerOptionsName,
                collectLogprobs,
                WebSearchToolName(options.Tools),
                IsShellProviderExecuted(options.Tools),
                ExtractApprovalRequestIdToToolCallIdMapping(options.Prompt)
            )
            .WithRedactRawResponse(
                RedactOpenAIToolCredentials.WithOpenAIResponseRedaction(document.RootElement).Redact
            )
            .WithRequest(
                new LanguageModelStreamRequest(JsonSerializer.SerializeToElement(prepared.Body))
                {
                    RedactBody = RedactOpenAIToolCredentials
                        .WithOpenAIResponseRedaction(
                            JsonSerializer.SerializeToElement(prepared.Body)
                        )
                        .Redact,
                }
            );
    }

    /// <inheritdoc />
    public async Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        var prepared = Prepare(
            ModelId,
            options,
            true,
            _providerOptionsName,
            SupportsWebSearchSourcesInclude,
            _requestModelId,
            _explicitMessageItemType
        );
        var json = prepared.Body.ToJsonString();
        var sse = await _http
            .OpenSseResponseAsync(
                _url(),
                json,
                await _headers(options.Headers, cancellationToken).ConfigureAwait(false),
                cancellationToken
            )
            .ConfigureAwait(false);
        return sse.ToStreamResult(
            StreamAsync(
                options,
                prepared,
                sse.Events,
                _url().ToString(),
                json,
                sse.Headers,
                cancellationToken
            ),
            json,
            RedactOpenAIToolCredentials
                .WithOpenAIResponseRedaction(JsonSerializer.SerializeToElement(prepared.Body))
                .Redact
        );
    }

    private async IAsyncEnumerable<LanguageModelStreamPart> StreamAsync(
        LanguageModelCallOptions options,
        OpenAIResponsesPreparedRequest prepared,
        IAsyncEnumerable<string> events,
        string url,
        string requestBodyJson,
        IReadOnlyDictionary<string, string> responseHeaders,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        yield return new StreamStartStreamPart(ToCallWarnings(prepared.Warnings));
        var state = new OpenAIResponsesStream(
            GenerateId,
            OpenAIParallelToolCall.FunctionToolNames(options.Tools),
            CreateToolNameMapping(options.Tools),
            prepared.Body["store"]?.GetValue<bool>() == true,
            _providerOptionsName,
            WebSearchToolName(options.Tools),
            IsShellProviderExecuted(options.Tools),
            ExtractApprovalRequestIdToToolCallIdMapping(options.Prompt)
        );
        await foreach (var data in events.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            JsonDocument? chunkDocument = null;
            try
            {
                try
                {
                    chunkDocument = JsonDocument.Parse(data);
                }
                catch (JsonException)
                {
                    state.MarkParseError();
                }

                if (chunkDocument == null)
                {
                    yield return new ErrorStreamPart("JSON parsing failed: Text: " + data + ".");
                    continue;
                }

                // Mirrors openaiResponsesChunkSchema: an invalid chunk (including a non-object value) fails the stream,
                // an unknown chunk (unknown_chunk) is ignored.
                var typedChunk = OpenAIResponsesSchema.ParseChunk(chunkDocument.RootElement);
                if (typedChunk == null)
                {
                    state.MarkParseError();
                    var node = JsonNode.Parse(data) as JsonObject;
                    yield return node != null && IsOpenAIChatCompletionChunk(node)
                        ? new ErrorStreamPart(
                            CreateChatCompletionsMismatchError(
                                node,
                                url,
                                requestBodyJson,
                                responseHeaders
                            )
                        )
                        : new ErrorStreamPart(
                            new ProviderUtils.TypeValidationException(
                                "Type validation failed: Value: " + data + ".",
                                chunkDocument.RootElement.Clone()
                            )
                        );
                    continue;
                }

                if (typedChunk is OpenAIResponsesApi.UnknownChunk)
                {
                    continue;
                }

                foreach (var part in state.Push(typedChunk))
                {
                    yield return part;
                }
            }
            finally
            {
                chunkDocument?.Dispose();
            }
        }

        foreach (var part in state.Complete())
        {
            yield return part;
        }
    }

    private static bool IsOpenAIChatCompletionChunk(JsonObject chunk) =>
        chunk["choices"] is JsonArray && chunk["type"]?.GetValueKind() != JsonValueKind.String;

    private static ApiCallError CreateChatCompletionsMismatchError(
        JsonObject value,
        string url,
        string requestBodyJson,
        IReadOnlyDictionary<string, string> responseHeaders
    ) =>
        new(
            "Received a Chat Completions stream while using the OpenAI Responses API. "
                + "The default OpenAI provider model uses the Responses API. If your custom baseURL targets a Chat Completions-compatible endpoint, use openai.chat('model-id') or createOpenAI(...).chat('model-id') instead. "
                + "You can also use @ai-sdk/openai-compatible for OpenAI-compatible providers.",
            url,
            JsonNode.Parse(requestBodyJson)!,
            responseHeaders: responseHeaders,
            responseBody: value.ToJsonString(),
            cause: new ProviderUtils.TypeValidationException(
                "Type validation failed: Value: " + value.ToJsonString() + ".",
                JsonDocument.Parse(value.ToJsonString()).RootElement.Clone()
            ),
            isRetryable: false,
            data: value.DeepClone()
        );

    /// <summary>
    /// Maps MCP approval request ids to the tool call ids of the prompt. When an MCP tool needs approval, the model
    /// tracks the pending approval with a generated tool call id. When the conversation continues after the approval
    /// response, the approval request id maps back to that id so tool results reference the right call.
    /// </summary>
    internal static Dictionary<string, string> ExtractApprovalRequestIdToToolCallIdMapping(
        IReadOnlyList<ModelMessage> prompt
    )
    {
        var mapping = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var message in prompt)
        {
            if (message is not AssistantModelMessage assistant)
            {
                continue;
            }

            foreach (var part in assistant.Parts.OfType<AssistantToolCallPart>())
            {
                if (
                    part.ProviderOptions != null
                    && part.ProviderOptions.TryGetValue("openai", out var openai)
                    && OpenAIJson.String(openai, "approvalRequestId") is { } approvalRequestId
                )
                {
                    mapping[approvalRequestId] = part.ToolCall.ToolCallId;
                }
            }
        }

        return mapping;
    }

    internal static LanguageModelGenerateResult Parse(
        OpenAIResponsesApi.Response response,
        IReadOnlyList<OpenAICallWarning> warnings,
        string raw,
        IReadOnlyDictionary<string, string> headers,
        Func<string>? generateId = null,
        IReadOnlyCollection<string>? functionToolNames = null,
        ToolNameMapping? toolNameMapping = null,
        bool jsonResponse = false,
        string providerOptionsName = "openai",
        bool collectLogprobs = false,
        string? webSearchToolName = null,
        bool isShellProviderExecuted = false,
        IReadOnlyDictionary<string, string>? approvalRequestIdToDummyToolCallIdFromPrompt = null
    )
    {
        functionToolNames ??= Array.Empty<string>();
        approvalRequestIdToDummyToolCallIdFromPrompt ??= new Dictionary<string, string>();
        generateId ??= () => JsonValues.GenerateId("source_");
        if (response.Error is { } error)
        {
            throw new BadRequestException(error.Message, raw);
        }

        if (response.Output is not { } output)
        {
            var detail = response.IncompleteReason;
            var message = string.IsNullOrEmpty(detail)
                ? "Responses API returned no output"
                : "Responses API returned no output (" + detail + ")";
            throw new InternalServerException(message, 500, raw);
        }

        var content = new List<GeneratedContent>();
        var hasFunctionCall = false;
        var hasRefusal = false;
        var hasWebSearchActionSources = false;
        var hasFileSearchResults = false;
        foreach (var item in output)
        {
            hasWebSearchActionSources |=
                item is OpenAIResponsesApi.WebSearchCall webSearch
                && OpenAIResponsesOutput.WebSearchUrls(webSearch.Action).Count > 0;
            hasFileSearchResults |=
                item is OpenAIResponsesApi.FileSearchCall fileSearch
                && OpenAIResponsesOutput.HasResults(fileSearch.Results);
        }

        string? structuredOutputText = null;
        var logprobs = new JsonArray();
        var retrievedFileIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in output)
        {
            switch (item)
            {
                case OpenAIResponsesApi.Message message:
                    var itemId = message.Id;
                    var phase = OpenAIResponsesOutput.PhaseName(message.Phase);

                    // Parse only the last final answer for JSON output, while every message stays in the content.
                    if (jsonResponse && phase == "final_answer")
                    {
                        structuredOutputText = string.Concat(
                            message.Content.Select(contentPart =>
                                contentPart switch
                                {
                                    OpenAIResponsesApi.Refusal refusal => refusal.RefusalText,
                                    OpenAIResponsesApi.OutputText outputText => outputText.Text,
                                    _ => string.Empty,
                                }
                            )
                        );
                    }

                    foreach (var part in message.Content)
                    {
                        if (part is OpenAIResponsesApi.Refusal refusalPart)
                        {
                            hasRefusal = true;
                            content.Add(
                                new OpenAIText(
                                    refusalPart.RefusalText,
                                    null,
                                    OpenAIResponsesOutput.TextMetadata(
                                        itemId,
                                        [],
                                        phase,
                                        providerOptionsName
                                    )
                                )
                            );
                            continue;
                        }

                        if (part is not OpenAIResponsesApi.OutputText textPart)
                        {
                            continue;
                        }

                        var annotations = textPart.Annotations;
                        content.Add(
                            new OpenAIText(
                                textPart.Text,
                                OpenAIResponsesOutput.Citations(annotations, providerOptionsName),
                                OpenAIResponsesOutput.TextMetadata(
                                    itemId,
                                    annotations,
                                    phase,
                                    providerOptionsName
                                )
                            )
                        );
                        if (collectLogprobs && textPart.Logprobs is { } partLogprobs)
                        {
                            logprobs.Add(LogprobsJson(partLogprobs));
                        }

                        // Citation-derived sources are only used when retrieval data is unavailable.
                        foreach (var annotation in annotations)
                        {
                            if (
                                (
                                    annotation is OpenAIResponsesApi.UrlCitation
                                    && hasWebSearchActionSources
                                )
                                || (
                                    annotation is OpenAIResponsesApi.FileCitation
                                    && hasFileSearchResults
                                )
                            )
                            {
                                continue;
                            }

                            content.Add(
                                OpenAIResponsesOutput.AnnotationSource(
                                    annotation,
                                    generateId(),
                                    providerOptionsName
                                )
                            );
                        }
                    }

                    break;
                case OpenAIResponsesApi.McpCall mcpCall:
                    var mcpToolName = OpenAIResponsesOutput.McpToolName(mcpCall.Name);
                    var mcpToolCallId =
                        mcpCall.ApprovalRequestId != null
                            ? approvalRequestIdToDummyToolCallIdFromPrompt.TryGetValue(
                                mcpCall.ApprovalRequestId,
                                out var mappedToolCallId
                            )
                                ? mappedToolCallId
                                : mcpCall.Id
                            : mcpCall.Id;
                    content.Add(
                        new GeneratedToolCall(mcpToolCallId, mcpToolName, mcpCall.Arguments)
                        {
                            ProviderExecuted = true,
                            Dynamic = true,
                        }
                    );
                    content.Add(
                        new GeneratedToolResult(
                            mcpToolCallId,
                            mcpToolName,
                            OpenAIResponsesOutput.McpCallResult(mcpCall)
                        )
                        {
                            ProviderMetadata = OpenAIResponsesOutput.ItemMetadata(
                                mcpCall.Id,
                                providerOptionsName
                            ),
                        }
                    );
                    break;
                case OpenAIResponsesApi.McpListTools:
                    // Skip listTools: it is not exposed and not sent back.
                    break;
                case OpenAIResponsesApi.McpApprovalRequest approvalRequest:
                    var approvalRequestId = approvalRequest.ApprovalRequestId ?? approvalRequest.Id;
                    var dummyToolCallId = generateId();
                    content.Add(
                        new GeneratedToolCall(
                            dummyToolCallId,
                            OpenAIResponsesOutput.McpToolName(approvalRequest.Name),
                            approvalRequest.Arguments
                        )
                        {
                            ProviderExecuted = true,
                            Dynamic = true,
                        }
                    );
                    content.Add(
                        new GeneratedToolApprovalRequest(approvalRequestId, dummyToolCallId)
                    );
                    break;
                case OpenAIResponsesApi.WebSearchCall webSearchCall:
                    var webSearchName = toolNameMapping?.ToCustomToolName(
                        webSearchToolName ?? "web_search"
                    )
                        ?? webSearchToolName
                        ?? "web_search";
                    content.Add(
                        new GeneratedToolCall(webSearchCall.Id, webSearchName, "{}")
                        {
                            ProviderExecuted = true,
                        }
                    );
                    content.Add(
                        new GeneratedToolResult(
                            webSearchCall.Id,
                            webSearchName,
                            OpenAIResponsesOutput.WebSearchResult(webSearchCall.Action)
                        )
                    );
                    foreach (var url in OpenAIResponsesOutput.WebSearchUrls(webSearchCall.Action))
                    {
                        content.Add(new GeneratedSource(generateId(), url, null));
                    }

                    break;
                case OpenAIResponsesApi.ImageGenerationCall imageGenerationCall:
                    var imageGenerationName = toolNameMapping?.ToCustomToolName("image_generation")
                        ?? "image_generation";
                    content.Add(
                        new GeneratedToolCall(imageGenerationCall.Id, imageGenerationName, "{}")
                        {
                            ProviderExecuted = true,
                        }
                    );
                    content.Add(
                        new GeneratedToolResult(
                            imageGenerationCall.Id,
                            imageGenerationName,
                            OpenAIResponsesOutput.ImageGenerationResult(imageGenerationCall.Result)
                        )
                    );
                    break;
                case OpenAIResponsesApi.CodeInterpreterCall codeInterpreterCall:
                    var codeInterpreterName = toolNameMapping?.ToCustomToolName("code_interpreter")
                        ?? "code_interpreter";
                    content.Add(
                        new GeneratedToolCall(
                            codeInterpreterCall.Id,
                            codeInterpreterName,
                            OpenAIResponsesOutput.CodeInterpreterInput(
                                codeInterpreterCall.Code,
                                codeInterpreterCall.ContainerId
                            )
                        )
                        {
                            ProviderExecuted = true,
                        }
                    );
                    content.Add(
                        new GeneratedToolResult(
                            codeInterpreterCall.Id,
                            codeInterpreterName,
                            OpenAIResponsesOutput.CodeInterpreterResult(codeInterpreterCall.Outputs)
                        )
                    );
                    break;
                case OpenAIResponsesApi.FileSearchCall fileSearchCall:
                    var fileSearchName = toolNameMapping?.ToCustomToolName("file_search")
                        ?? "file_search";
                    content.Add(
                        new GeneratedToolCall(fileSearchCall.Id, fileSearchName, "{}")
                        {
                            ProviderExecuted = true,
                        }
                    );
                    content.Add(
                        new GeneratedToolResult(
                            fileSearchCall.Id,
                            fileSearchName,
                            OpenAIResponsesOutput.FileSearchResult(
                                fileSearchCall.Queries,
                                fileSearchCall.Results
                            )
                        )
                    );
                    foreach (
                        var (fileId, filename) in OpenAIResponsesOutput.RetrievedFiles(
                            fileSearchCall.Results
                        )
                    )
                    {
                        if (retrievedFileIds.Add(fileId))
                        {
                            content.Add(
                                new GeneratedDocumentSource(
                                    generateId(),
                                    "text/plain",
                                    filename,
                                    filename,
                                    OpenAIResponsesOutput.FileSearchMetadata(
                                        fileId,
                                        providerOptionsName
                                    )
                                )
                            );
                        }
                    }

                    break;
                case OpenAIResponsesApi.FunctionCall functionCall:
                    hasFunctionCall = true;
                    var expanded = OpenAIParallelToolCall.Expand(
                        functionCall.CallId,
                        functionCall.Name,
                        functionCall.Arguments,
                        functionToolNames,
                        functionCall.Id,
                        providerOptionsName
                    );
                    if (expanded != null)
                    {
                        content.AddRange(
                            expanded.Select(call => new GeneratedToolCall(
                                call.ToolCallId,
                                call.ToolName,
                                call.Input,
                                call.ProviderMetadata
                            ))
                        );
                        break;
                    }

                    content.Add(
                        new GeneratedToolCall(
                            functionCall.CallId,
                            functionCall.Name,
                            functionCall.Arguments,
                            OpenAIParallelToolCall.CallMetadata(
                                functionCall.Id,
                                functionCall.Async,
                                functionCall.Namespace,
                                functionCall.Caller,
                                providerName: providerOptionsName
                            )
                        )
                    );
                    break;
                case OpenAIResponsesApi.Program program:
                    content.Add(
                        new GeneratedToolCall(
                            program.CallId,
                            ProgrammaticToolName(toolNameMapping),
                            OpenAIResponsesOutput.ProgramInput(program),
                            OpenAIResponsesOutput.ItemMetadata(program.Id, providerOptionsName)
                        )
                        {
                            ProviderExecuted = true,
                        }
                    );
                    break;
                case OpenAIResponsesApi.ProgramOutput programOutput:
                    content.Add(
                        new GeneratedToolResult(
                            programOutput.CallId,
                            ProgrammaticToolName(toolNameMapping),
                            OpenAIResponsesOutput.ProgramOutputResult(programOutput)
                        )
                        {
                            ProviderMetadata = OpenAIResponsesOutput.ItemMetadata(
                                programOutput.Id,
                                providerOptionsName
                            ),
                        }
                    );
                    break;
                case OpenAIResponsesApi.CustomToolCall customToolCall:
                    hasFunctionCall = true;
                    content.Add(
                        new GeneratedToolCall(
                            customToolCall.CallId,
                            customToolCall.Name,
                            JsonSerializer.Serialize(customToolCall.Input, CustomToolInputOptions),
                            OpenAIParallelToolCall.CallMetadata(
                                customToolCall.Id,
                                customToolCall.Async,
                                providerName: providerOptionsName
                            )
                        )
                    );
                    break;
                case OpenAIResponsesApi.LocalShellCallOutput localShell:
                    content.Add(
                        new GeneratedToolCall(
                            localShell.Call.CallId,
                            toolNameMapping?.ToCustomToolName("local_shell") ?? "local_shell",
                            OpenAIResponsesOutput.LocalShellCallInput(localShell.Call),
                            OpenAIResponsesOutput.ItemMetadata(
                                localShell.Call.Id,
                                providerOptionsName
                            )
                        )
                    );
                    break;
                case OpenAIResponsesApi.ShellCall shellCall:
                    content.Add(
                        new GeneratedToolCall(
                            shellCall.CallId,
                            toolNameMapping?.ToCustomToolName("shell") ?? "shell",
                            OpenAIResponsesOutput.ShellCallInput(shellCall),
                            OpenAIResponsesOutput.ItemMetadata(shellCall.Id, providerOptionsName)
                        )
                        {
                            ProviderExecuted = isShellProviderExecuted,
                        }
                    );
                    break;
                case OpenAIResponsesApi.ShellCallOutput shellCallOutput:
                    content.Add(
                        new GeneratedToolResult(
                            shellCallOutput.CallId,
                            toolNameMapping?.ToCustomToolName("shell") ?? "shell",
                            OpenAIResponsesOutput.ShellCallOutputResult(shellCallOutput)
                        )
                    );
                    break;
                case OpenAIResponsesApi.ApplyPatchCall applyPatchCall:
                    hasFunctionCall = true;
                    content.Add(
                        new GeneratedToolCall(
                            applyPatchCall.CallId,
                            toolNameMapping?.ToCustomToolName("apply_patch") ?? "apply_patch",
                            OpenAIResponsesOutput.ApplyPatchInput(applyPatchCall),
                            OpenAIResponsesOutput.ItemMetadata(
                                applyPatchCall.Id,
                                providerOptionsName
                            )
                        )
                    );
                    break;
                case OpenAIResponsesApi.Reasoning reasoning:
                    var reasoningValues = new JsonObject();
                    if (!string.IsNullOrEmpty(reasoning.Id))
                    {
                        reasoningValues["itemId"] = reasoning.Id;
                    }

                    reasoningValues["reasoningEncryptedContent"] = reasoning.EncryptedContent;
                    JsonElement? reasoningMetadata =
                        reasoningValues.Count > 0
                            ? OpenAIJson.ProviderMetadata(reasoningValues, providerOptionsName)
                            : null;
                    if (reasoning.Summary.Count == 0)
                    {
                        content.Add(new GeneratedReasoning(string.Empty, reasoningMetadata));
                    }

                    foreach (var text in reasoning.Summary)
                    {
                        content.Add(new GeneratedReasoning(text, reasoningMetadata));
                    }

                    break;
                case OpenAIResponsesApi.Compaction compaction:
                    content.Add(
                        new GeneratedCustom(
                            "openai.compaction",
                            OpenAIResponsesOutput.CompactionMetadata(
                                compaction,
                                providerOptionsName
                            )
                        )
                    );
                    break;
            }
        }

        var incompleteReason = response.IncompleteReason;
        var finish = MapFinishReason(incompleteReason, hasFunctionCall, hasRefusal);
        var responseId = response.Id;
        var responseModel = response.Model;
        var providerMetadata = new JsonObject { ["responseId"] = responseId };
        if (logprobs.Count > 0)
        {
            providerMetadata["logprobs"] = logprobs;
        }

        if (response.ServiceTier is { } serviceTier)
        {
            providerMetadata["serviceTier"] = serviceTier;
        }

        if (response.ReasoningContext is { } reasoningContext)
        {
            providerMetadata["reasoningContext"] = reasoningContext;
        }

        if (PromptCacheDiagnostics(response.PromptCacheDiagnostics) is { } promptCacheDiagnostics)
        {
            providerMetadata["promptCacheDiagnostics"] = promptCacheDiagnostics;
        }

        if (InputTransformations(response.InputTransformations) is { } inputTransformations)
        {
            providerMetadata["inputTransformations"] = inputTransformations;
        }

        return new LanguageModelGenerateResult(
            content,
            finish,
            OpenAIJson.ResponsesUsage(response.Usage),
            incompleteReason,
            ToCallWarnings(warnings),
            string.IsNullOrEmpty(responseId) ? null : responseId,
            OpenAIJson.ProviderMetadata(providerMetadata, providerOptionsName),
            raw,
            string.IsNullOrEmpty(responseModel) ? null : responseModel,
            OpenAIJson.UnixSeconds(response.CreatedAt is { } createdAt ? (long)createdAt : null),
            headers,
            structuredOutputText
        );
    }

    private static JsonArray LogprobsJson(IReadOnlyList<OpenAIResponsesApi.TokenLogprob> logprobs)
    {
        var array = new JsonArray();
        foreach (var entry in logprobs)
        {
            var top = new JsonArray();
            foreach (var alternative in entry.TopLogprobs)
            {
                top.Add(
                    new JsonObject
                    {
                        ["token"] = alternative.Token,
                        ["logprob"] = alternative.Logprob,
                    }
                );
            }

            array.Add(
                new JsonObject
                {
                    ["token"] = entry.Token,
                    ["logprob"] = entry.Logprob,
                    ["top_logprobs"] = top,
                }
            );
        }

        return array;
    }

    internal static string ProgrammaticToolName(ToolNameMapping? toolNameMapping) =>
        toolNameMapping?.ToCustomToolName("programmatic_tool_calling")
        ?? "programmatic_tool_calling";

    internal static FinishReason MapFinishReason(
        string? incompleteReason,
        bool hasFunctionCall,
        bool hasRefusal = false
    )
    {
        switch (incompleteReason)
        {
            case null:
                // A client-side function call must win over a refusal so that the tools are still executed.
                // The refusal text remains part of the content.
                if (hasFunctionCall)
                {
                    return FinishReason.ToolCalls;
                }

                return hasRefusal ? FinishReason.ContentFilter : FinishReason.Stop;
            case "max_output_tokens":
                return FinishReason.Length;
            case "content_filter":
                return FinishReason.ContentFilter;
            default:
                return hasFunctionCall ? FinishReason.ToolCalls : FinishReason.Other;
        }
    }

    private static readonly string[] UpdateEfforts =
    {
        "none",
        "low",
        "medium",
        "high",
        "xhigh",
        "max",
    };

    private static string? GetConfigurationUpdateUnsupportedReason(
        OpenAILanguageModelCapabilities capabilities,
        JsonElement? openai
    )
    {
        if (!capabilities.SupportsConfigurationUpdate)
        {
            return "Configuration updates are only supported by GPT-6 and later models";
        }

        if (
            OpenAIJson.String(openai, "reasoningMode") == "pro"
            || OpenAIJson.Child(openai, "contextManagement") != null
            || OpenAIJson.String(openai, "truncation") == "auto"
        )
        {
            return "Configuration updates require standard reasoning mode without automatic compaction or automatic truncation";
        }

        return null;
    }

    private static void ValidateUpdateEffort(string effort)
    {
        if (Array.IndexOf(UpdateEfforts, effort) < 0)
        {
            throw new InvalidArgumentException(
                "providerOptions",
                effort,
                "invalid openai provider options"
            );
        }
    }

    private static JsonObject ConfigurationUpdate(string? effort, JsonObject? textFormat = null)
    {
        var update = new JsonObject { ["type"] = "configuration_update" };
        if (effort != null)
        {
            update["reasoning"] = new JsonObject { ["effort"] = effort };
        }

        if (textFormat != null)
        {
            update["text"] = new JsonObject { ["format"] = textFormat };
        }

        return update;
    }

    /// <summary>Returns <c>input_transformations</c> as provider metadata, or <c>null</c> when the response did not report it. An empty list is kept.</summary>
    internal static JsonArray? InputTransformations(
        IReadOnlyList<OpenAIResponsesApi.InputTransformation>? transformations
    )
    {
        if (transformations is null)
        {
            return null;
        }

        var result = new JsonArray();
        foreach (var transformation in transformations)
        {
            var entry = new JsonObject();
            if (transformation.Id is { } id)
            {
                entry["id"] = id;
            }

            entry["type"] = transformation.Type;
            entry["action"] = transformation.Action;
            entry["reason"] = transformation.Reason;
            result.Add(entry);
        }

        return result;
    }

    /// <summary>Returns the <c>prompt_cache_diagnostics</c> fields that the response carries, or <c>null</c> when it has none.</summary>
    internal static JsonObject? PromptCacheDiagnostics(
        OpenAIResponsesApi.PromptCacheDiagnostics? diagnostics
    )
    {
        if (diagnostics is null)
        {
            return null;
        }

        var result = new JsonObject { ["type"] = diagnostics.Type };
        if (diagnostics.Reason is { } reason)
        {
            result["reason"] = reason;
        }

        if (diagnostics.CacheMissedTokens is { } cacheMissedTokens)
        {
            result["cache_missed_tokens"] = cacheMissedTokens;
        }

        if (diagnostics.ComparisonReusableTokens is { } comparisonReusableTokens)
        {
            result["comparison_reusable_tokens"] = comparisonReusableTokens;
        }

        return result;
    }

    private static bool IsConfigurationUpdate(JsonNode? item) =>
        item is JsonObject obj && obj["type"]?.GetValue<string>() == "configuration_update";

    private static JsonElement? ProviderOptionsFor(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string name
    )
    {
        if (providerOptions != null)
        {
            if (providerOptions.TryGetValue(name, out var named))
            {
                return named;
            }

            if (name != "openai" && providerOptions.TryGetValue("openai", out var fallback))
            {
                return fallback;
            }
        }

        return null;
    }

    internal static ResponsesInput ConvertInput(
        IReadOnlyList<ModelMessage> prompt,
        string systemMessageMode,
        bool explicitItem,
        string? configurationUpdateUnsupportedReason,
        string providerOptionsName,
        bool hasContinuation,
        bool store,
        ISet<string>? customToolNames = null,
        ISet<string>? outputSchemaToolNames = null,
        bool isReasoningModel = false,
        bool hasConversation = false,
        ToolNameMapping? toolNameMapping = null,
        bool hasApplyPatchTool = false,
        bool hasLocalShellTool = false,
        bool hasPreviousResponseId = false,
        Func<string, string, AdditionalToolResolution?>? findAdditionalToolDefinition = null
    )
    {
        var input = new JsonArray();
        var seenAdditionalToolWarnings = new HashSet<string>();
        var droppedAdditionalToolNames = new List<string>();
        var programmaticToolCallIds = new HashSet<string>();
        var parallelToolResultGroups =
            hasConversation || hasPreviousResponseId
                ? CollectCompleteParallelToolResultGroups(prompt, providerOptionsName)
                : new Dictionary<string, ParallelToolResultGroup>();
        var emittedParallelToolCalls = new HashSet<string>();
        var emittedParallelToolResults = new HashSet<string>();
        var processedApprovalIds = new HashSet<string>();
        var warnings = new List<OpenAICallWarning>();
        foreach (var message in ToolModelMessage.Flatten(prompt))
        {
            switch (message)
            {
                case SystemModelMessage system:
                    var systemOptions = ProviderOptionsFor(
                        system.ProviderOptions,
                        providerOptionsName
                    );
                    var messageEffort = OpenAIJson.String(systemOptions, "reasoningEffortUpdate");
                    var messageTextFormat = OpenAIJson.Child(systemOptions, "textFormatUpdate");
                    var messageAdditionalTools = AdditionalToolsList(systemOptions);
                    if (
                        messageEffort != null
                        || messageTextFormat != null
                        || messageAdditionalTools != null
                    )
                    {
                        var hasConfigurationUpdate =
                            messageEffort != null || messageTextFormat != null;
                        if (messageEffort != null)
                        {
                            ValidateUpdateEffort(messageEffort);
                        }

                        var feature =
                            messageEffort != null ? "reasoningEffortUpdate"
                            : messageTextFormat != null ? "textFormatUpdate"
                            : "additionalTools";
                        var reason =
                            system.Content != string.Empty
                                ? "Message-level "
                                    + feature
                                    + " requires empty system message content."
                            : hasConfigurationUpdate ? configurationUpdateUnsupportedReason
                            : null;
                        if (reason != null)
                        {
                            throw new UnsupportedFunctionalityException(
                                "Message-level " + feature,
                                reason
                            );
                        }

                        // The controls are independent of systemMessageMode's text handling.
                        if (hasConfigurationUpdate)
                        {
                            var normalizedFormat = messageTextFormat is { } rawFormat
                                ? OpenAIResponsesTextFormat.Normalize(rawFormat, warnings)
                                : null;
                            input.Add(ConfigurationUpdate(messageEffort, normalizedFormat));
                        }

                        var resolvedMessageAdditionalTools = FilterAdditionalTools(
                            messageAdditionalTools,
                            findAdditionalToolDefinition,
                            seenAdditionalToolWarnings,
                            droppedAdditionalToolNames,
                            warnings
                        );
                        if (resolvedMessageAdditionalTools != null)
                        {
                            input.Add(AdditionalToolsItem(resolvedMessageAdditionalTools));
                        }

                        break;
                    }

                    if (systemMessageMode == "remove")
                    {
                        warnings.Add(
                            new OpenAICallWarning(
                                "other",
                                null,
                                null,
                                "system messages are removed for this model"
                            )
                        );
                        break;
                    }

                    if (systemMessageMode != "system" && systemMessageMode != "developer")
                    {
                        throw new AiSdkException(
                            "Unsupported system message mode: " + systemMessageMode
                        );
                    }

                    var systemBreakpoint = OpenAIJson.PromptCacheBreakpoint(
                        ProviderOptionsFor(system.ProviderOptions, providerOptionsName)
                    );
                    JsonNode systemContent =
                        systemBreakpoint == null
                            ? JsonValue.Create(system.Content)!
                            : new JsonArray(
                                new JsonObject
                                {
                                    ["type"] = "input_text",
                                    ["text"] = system.Content,
                                    ["prompt_cache_breakpoint"] = systemBreakpoint,
                                }
                            );
                    input.Add(MessageItem(systemMessageMode, systemContent, explicitItem));
                    break;
                case UserModelMessage user:
                    var parts = new JsonArray();
                    foreach (var part in user.Content)
                    {
                        if (part is TextContentPart text)
                        {
                            var item = new JsonObject
                            {
                                ["type"] = "input_text",
                                ["text"] = text.Text,
                            };
                            if (
                                OpenAIJson.PromptCacheBreakpoint(
                                    ProviderOptionsFor(text.ProviderOptions, providerOptionsName)
                                ) is
                                { } textBreakpoint
                            )
                            {
                                item["prompt_cache_breakpoint"] = textBreakpoint;
                            }

                            parts.Add(item);
                        }
                        else if (part is FileContentPart file)
                        {
                            var item = ConvertFile(file);
                            if (
                                OpenAIJson.PromptCacheBreakpoint(
                                    ProviderOptionsFor(file.ProviderOptions, providerOptionsName)
                                ) is
                                { } fileBreakpoint
                            )
                            {
                                item["prompt_cache_breakpoint"] = fileBreakpoint;
                            }

                            parts.Add(item);
                        }
                    }

                    input.Add(MessageItem("user", parts, explicitItem));
                    break;
                case AssistantModelMessage assistant:
                    AddStoredReasoning(
                        input,
                        assistant,
                        providerOptionsName,
                        hasContinuation,
                        store
                    );
                    var hasTextItemIds = assistant.Parts.Any(part =>
                        part is AssistantTextPart && TextItemId(part, providerOptionsName) != null
                    );
                    if (hasTextItemIds)
                    {
                        AddStoredText(
                            input,
                            assistant,
                            providerOptionsName,
                            explicitItem,
                            store,
                            isReasoningModel,
                            hasConversation
                        );
                    }
                    else if (!string.IsNullOrEmpty(assistant.Text))
                    {
                        input.Add(
                            MessageItem(
                                "assistant",
                                JsonValue.Create(assistant.Text)!,
                                explicitItem
                            )
                        );
                    }

                    foreach (var call in assistant.ToolCalls)
                    {
                        var parallelCallMetadata =
                            OpenAIParallelToolCall.GetParallelToolCallMetadata(
                                AssistantCallOptions(assistant, call),
                                providerOptionsName
                            );
                        if (
                            parallelCallMetadata != null
                            && parallelToolResultGroups.TryGetValue(
                                parallelCallMetadata.ToolCallId,
                                out var parallelCallGroup
                            )
                            && OpenAIParallelToolCall.HasSameParallelToolCall(
                                parallelCallGroup.Metadata,
                                parallelCallMetadata
                            )
                        )
                        {
                            if (
                                emittedParallelToolCalls.Add(parallelCallGroup.Metadata.ToolCallId)
                                && !hasConversation
                            )
                            {
                                // Conversations already contain the original wrapper item.
                                // previousResponseId chains require plain client function calls to be reconstructed in full.
                                input.Add(
                                    new JsonObject
                                    {
                                        ["type"] = "function_call",
                                        ["call_id"] = parallelCallGroup.Metadata.ToolCallId,
                                        ["name"] = parallelCallGroup.Metadata.ToolName,
                                        ["arguments"] = parallelCallGroup.Metadata.Input,
                                    }
                                );
                            }

                            continue;
                        }

                        if (
                            ToolCaller(
                                ProviderOptionsFor(
                                    AssistantCallOptions(assistant, call),
                                    providerOptionsName
                                )
                            )
                                is JsonObject { } callCaller
                            && callCaller["type"]?.GetValue<string>() == "program"
                        )
                        {
                            programmaticToolCallIds.Add(call.ToolCallId);
                        }

                        // When using a conversation, skip items that already exist in the conversation context to avoid "Duplicate item found" errors.
                        if (
                            hasConversation
                            && OpenAIJson.String(
                                ProviderOptionsFor(
                                    AssistantCallOptions(assistant, call),
                                    providerOptionsName
                                ),
                                "itemId"
                            ) != null
                        )
                        {
                            continue;
                        }

                        if (IsProgrammaticToolName(call.ToolName, toolNameMapping))
                        {
                            AddProgramCall(
                                input,
                                call,
                                ProviderOptionsFor(
                                    AssistantCallOptions(assistant, call),
                                    providerOptionsName
                                ),
                                store,
                                hasConversation
                            );
                            continue;
                        }

                        if (IsShellToolName(call.ToolName, toolNameMapping))
                        {
                            AddShellCall(
                                input,
                                call,
                                ProviderOptionsFor(
                                    AssistantCallOptions(assistant, call),
                                    providerOptionsName
                                ),
                                store,
                                hasContinuation,
                                hasConversation
                            );
                            continue;
                        }

                        if (
                            hasLocalShellTool
                            && toolNameMapping?.ToProviderToolName(call.ToolName) == "local_shell"
                        )
                        {
                            AddLocalShellCall(
                                input,
                                call,
                                ProviderOptionsFor(
                                    AssistantCallOptions(assistant, call),
                                    providerOptionsName
                                ),
                                store,
                                hasContinuation,
                                hasConversation
                            );
                            continue;
                        }

                        if (
                            hasApplyPatchTool
                            && toolNameMapping?.ToProviderToolName(call.ToolName) == "apply_patch"
                        )
                        {
                            AddApplyPatchCall(
                                input,
                                call,
                                ProviderOptionsFor(
                                    AssistantCallOptions(assistant, call),
                                    providerOptionsName
                                ),
                                store,
                                hasContinuation,
                                hasConversation
                            );
                            continue;
                        }

                        // A provider-executed call has no client function to replay. Upstream references the stored item, or drops the call.
                        if (call.ProviderExecuted)
                        {
                            if (
                                store
                                && OpenAIJson.String(
                                    ProviderOptionsFor(
                                        AssistantCallOptions(assistant, call),
                                        providerOptionsName
                                    ),
                                    "itemId"
                                )
                                    is { } providerItemId
                            )
                            {
                                input.Add(
                                    new JsonObject
                                    {
                                        ["type"] = "item_reference",
                                        ["id"] = providerItemId,
                                    }
                                );
                            }

                            continue;
                        }

                        if (customToolNames?.Contains(call.ToolName) == true)
                        {
                            var callOptions = ProviderOptionsFor(
                                assistant
                                    .Parts.OfType<AssistantToolCallPart>()
                                    .FirstOrDefault(part => part.ToolCall == call)
                                    ?.ProviderOptions,
                                providerOptionsName
                            );
                            var itemId = OpenAIJson.String(callOptions, "itemId");
                            if (hasConversation && itemId != null)
                            {
                                continue;
                            }

                            // A stored custom tool call is sent as a reference. With a previous response id, the chain already holds it.
                            if (store && itemId != null)
                            {
                                if (!hasContinuation)
                                {
                                    input.Add(
                                        new JsonObject
                                        {
                                            ["type"] = "item_reference",
                                            ["id"] = itemId,
                                        }
                                    );
                                }

                                continue;
                            }

                            var customCall = new JsonObject
                            {
                                ["type"] = "custom_tool_call",
                                ["call_id"] = call.ToolCallId,
                                ["name"] = call.ToolName,
                                ["input"] = CustomToolCallInput(call.ArgumentsJson),
                            };
                            if (OpenAIJson.Bool(callOptions, "async") is { } isAsync)
                            {
                                customCall["async"] = isAsync;
                            }

                            if (itemId != null)
                            {
                                customCall["id"] = itemId;
                            }

                            input.Add(customCall);
                            continue;
                        }

                        var functionCall = new JsonObject
                        {
                            ["type"] = "function_call",
                            ["call_id"] = call.ToolCallId,
                            ["name"] = call.ToolName,
                            ["arguments"] = string.IsNullOrEmpty(call.ArgumentsJson)
                                ? "{}"
                                : call.ArgumentsJson,
                        };
                        if (
                            ToolCaller(
                                ProviderOptionsFor(
                                    AssistantCallOptions(assistant, call),
                                    providerOptionsName
                                )
                            ) is
                            { } functionCaller
                        )
                        {
                            functionCall["caller"] = functionCaller;
                        }

                        input.Add(functionCall);
                    }

                    foreach (var resultPart in assistant.Parts.OfType<AssistantToolResultPart>())
                    {
                        if (IsProgrammaticToolName(resultPart.ToolName, toolNameMapping))
                        {
                            AddProgramOutput(
                                input,
                                resultPart,
                                providerOptionsName,
                                store,
                                hasConversation
                            );
                        }
                        else if (
                            IsShellToolName(resultPart.ToolName, toolNameMapping)
                            && !hasConversation
                            && resultPart.Output.OutputType == "json"
                        )
                        {
                            // The shell_call_output has its own item id, which the call's metadata does not hold, so it is rebuilt.
                            input.Add(
                                ShellCallOutput(resultPart.ToolCallId, resultPart.Output.OutputJson)
                            );
                        }
                    }

                    if (hasTextItemIds)
                    {
                        AddStoredText(
                            input,
                            assistant,
                            providerOptionsName,
                            explicitItem,
                            store,
                            isReasoningModel,
                            hasConversation,
                            afterToolParts: true
                        );
                    }

                    AddCompaction(input, assistant, providerOptionsName, store, hasConversation);
                    break;
                case ToolApprovalResponseModelMessage approvalResponse:
                    // The denied tool result that follows an approval response is skipped upstream (execution-denied with an
                    // approvalId): the mcp_approval_response already carries the decision.
                    if (!processedApprovalIds.Add(approvalResponse.ApprovalId))
                    {
                        break;
                    }

                    if (store && !hasConversation && !hasPreviousResponseId)
                    {
                        input.Add(
                            new JsonObject
                            {
                                ["type"] = "item_reference",
                                ["id"] = approvalResponse.ApprovalId,
                            }
                        );
                    }

                    input.Add(
                        new JsonObject
                        {
                            ["type"] = "mcp_approval_response",
                            ["approval_request_id"] = approvalResponse.ApprovalId,
                            ["approve"] = approvalResponse.Approved,
                        }
                    );
                    break;
                case ToolModelMessage tool:
                    // Skip execution-denied with approvalId: already handled via the tool approval response.
                    if (
                        tool.OutputType == "execution-denied"
                        && OpenAIJson.String(
                            OpenAIJson.Child(tool.OutputProviderOptions, "openai"),
                            "approvalId"
                        ) != null
                    )
                    {
                        break;
                    }

                    var parallelResultMetadata = OpenAIParallelToolCall.GetParallelToolCallMetadata(
                        ToOptions(tool.ProviderMetadata),
                        providerOptionsName
                    );
                    if (
                        parallelResultMetadata != null
                        && parallelToolResultGroups.TryGetValue(
                            parallelResultMetadata.ToolCallId,
                            out var parallelResultGroup
                        )
                        && OpenAIParallelToolCall.HasSameParallelToolCall(
                            parallelResultGroup.Metadata,
                            parallelResultMetadata
                        )
                    )
                    {
                        if (emittedParallelToolResults.Add(parallelResultGroup.Metadata.ToolCallId))
                        {
                            AddParallelToolResults(
                                input,
                                parallelResultGroup,
                                outputSchemaToolNames,
                                providerOptionsName,
                                findAdditionalToolDefinition,
                                seenAdditionalToolWarnings,
                                droppedAdditionalToolNames,
                                warnings
                            );
                        }

                        break;
                    }

                    // The output's own breakpoint wins over the tool result's.
                    var toolBreakpoint = OpenAIJson.PromptCacheBreakpoint(
                        tool.OutputProviderOptions
                    ) ?? OpenAIJson.PromptCacheBreakpoint(tool.ProviderMetadata);
                    var toolIsCustom = customToolNames?.Contains(tool.ToolName) == true;
                    var toolHasSchema =
                        !toolIsCustom && outputSchemaToolNames?.Contains(tool.ToolName) == true;
                    var toolText = OpenAIJson.ToolOutputText(
                        tool.OutputType,
                        tool.OutputType == "execution-denied"
                            ? tool.ExecutionDeniedReason
                            : tool.OutputJson,
                        toolHasSchema
                    );
                    JsonNode toolOutput = toolText;
                    if (toolBreakpoint != null)
                    {
                        toolOutput = new JsonArray(
                            new JsonObject
                            {
                                ["type"] = "input_text",
                                ["text"] = toolText,
                                ["prompt_cache_breakpoint"] = toolBreakpoint,
                            }
                        );
                    }

                    if (
                        IsShellToolName(tool.ToolName, toolNameMapping)
                        && tool.OutputType == "json"
                    )
                    {
                        input.Add(ShellCallOutput(tool.ToolCallId, tool.OutputJson));
                        break;
                    }

                    if (
                        hasLocalShellTool
                        && toolNameMapping?.ToProviderToolName(tool.ToolName) == "local_shell"
                        && tool.OutputType == "json"
                    )
                    {
                        input.Add(LocalShellCallOutput(tool.ToolCallId, tool.OutputJson));
                        break;
                    }

                    if (
                        hasApplyPatchTool
                        && tool.ToolName == "apply_patch"
                        && tool.OutputType == "json"
                    )
                    {
                        input.Add(ApplyPatchCallOutput(tool.ToolCallId, tool.OutputJson));
                        break;
                    }

                    var resultCaller = ToolCaller(
                        ProviderOptionsFor(ToOptions(tool.ProviderMetadata), providerOptionsName)
                    );
                    if (
                        tool.OutputType == "execution-denied"
                        && (
                            programmaticToolCallIds.Contains(tool.ToolCallId)
                            || (
                                resultCaller is JsonObject { } denied
                                && denied["type"]?.GetValue<string>() == "program"
                            )
                        )
                    )
                    {
                        throw new UnsupportedFunctionalityException(
                            "execution-denied results for programmatic tool calls",
                            "functionality not supported: execution-denied results for programmatic tool calls"
                        );
                    }

                    var toolResultItem = new JsonObject
                    {
                        ["type"] = toolIsCustom
                            ? "custom_tool_call_output"
                            : "function_call_output",
                        ["call_id"] = tool.ToolCallId,
                        ["output"] = toolOutput,
                    };
                    if (!toolIsCustom && resultCaller != null)
                    {
                        toolResultItem["caller"] = resultCaller;
                    }

                    input.Add(toolResultItem);

                    var additionalTools = ResolveAdditionalTools(
                        tool.ProviderMetadata,
                        providerOptionsName,
                        findAdditionalToolDefinition,
                        seenAdditionalToolWarnings,
                        droppedAdditionalToolNames,
                        warnings
                    );
                    if (additionalTools != null)
                    {
                        input.Add(AdditionalToolsItem(additionalTools));
                    }

                    break;
                default:
                    throw new AiSdkException("Unsupported role: " + message.Role);
            }
        }

        if (droppedAdditionalToolNames.Count > 0)
        {
            warnings.Add(
                new OpenAICallWarning(
                    "unsupported",
                    "additionalTools",
                    "additional tool definitions are only sent for tools registered in the request; dropped the definitions of unregistered tools: "
                        + FormatDroppedToolNames(droppedAdditionalToolNames)
                )
            );
        }

        return new ResponsesInput(input, warnings);
    }

    private sealed record ParallelToolResultGroup(
        OpenAIParallelToolCallMetadata Metadata,
        IReadOnlyList<ToolModelMessage> Results
    );

    private sealed class PendingParallelToolResultGroup(
        OpenAIParallelToolCallMetadata metadata,
        ToolModelMessage first
    )
    {
        public OpenAIParallelToolCallMetadata Metadata { get; set; } = metadata;

        public Dictionary<int, ToolModelMessage> Results { get; } =
            new() { [metadata.Index] = first };

        public bool Invalid { get; set; }
    }

    private static Dictionary<
        string,
        ParallelToolResultGroup
    > CollectCompleteParallelToolResultGroups(
        IReadOnlyList<ModelMessage> prompt,
        string providerOptionsName
    )
    {
        var pendingGroups = new Dictionary<string, PendingParallelToolResultGroup>();
        foreach (var message in ToolModelMessage.Flatten(prompt))
        {
            if (
                message is not ToolModelMessage part
                || OpenAIParallelToolCall.GetParallelToolCallMetadata(
                    ToOptions(part.ProviderMetadata),
                    providerOptionsName
                )
                    is not { } metadata
            )
            {
                continue;
            }

            if (!pendingGroups.TryGetValue(metadata.ToolCallId, out var existing))
            {
                pendingGroups[metadata.ToolCallId] = new PendingParallelToolResultGroup(
                    metadata,
                    part
                );
                continue;
            }

            if (
                !OpenAIParallelToolCall.HasSameParallelToolCall(existing.Metadata, metadata)
                || existing.Results.ContainsKey(metadata.Index)
            )
            {
                existing.Invalid = true;
                continue;
            }

            if (existing.Metadata.Input is null && metadata.Input is not null)
            {
                existing.Metadata = metadata;
            }

            existing.Results[metadata.Index] = part;
        }

        var completeGroups = new Dictionary<string, ParallelToolResultGroup>();
        foreach (var pair in pendingGroups)
        {
            var toolCallId = pair.Key;
            var group = pair.Value;
            if (
                group.Invalid
                || group.Results.Count != group.Metadata.Count
                || group.Metadata.Input is null
            )
            {
                continue;
            }

            var results = new List<ToolModelMessage>(group.Metadata.Count);
            for (var index = 0; index < group.Metadata.Count; index++)
            {
                if (!group.Results.TryGetValue(index, out var result))
                {
                    break;
                }

                results.Add(result);
            }

            if (results.Count == group.Metadata.Count)
            {
                completeGroups[toolCallId] = new ParallelToolResultGroup(group.Metadata, results);
            }
        }

        return completeGroups;
    }

    // The internal wrapper returns one output containing the child results in the order of the original tool_uses array.
    private static void AddParallelToolResults(
        JsonArray input,
        ParallelToolResultGroup group,
        ISet<string>? outputSchemaToolNames,
        string providerOptionsName,
        Func<string, string, AdditionalToolResolution?>? findAdditionalToolDefinition,
        ISet<string> seenAdditionalToolWarnings,
        List<string> droppedAdditionalToolNames,
        List<OpenAICallWarning> warnings
    )
    {
        var texts = new List<string>();
        var breakpoints = new List<JsonNode?>();
        foreach (var result in group.Results)
        {
            var hasSchema = outputSchemaToolNames?.Contains(result.ToolName) == true;
            texts.Add(
                OpenAIJson.ToolOutputText(
                    result.OutputType,
                    result.OutputType == "execution-denied"
                        ? result.ExecutionDeniedReason
                        : result.OutputJson,
                    hasSchema
                )
            );
            breakpoints.Add(
                OpenAIJson.PromptCacheBreakpoint(result.OutputProviderOptions)
                    ?? OpenAIJson.PromptCacheBreakpoint(result.ProviderMetadata)
            );
        }

        JsonNode output;
        if (breakpoints.Any(breakpoint => breakpoint != null))
        {
            var items = new JsonArray();
            for (var index = 0; index < texts.Count; index++)
            {
                var item = new JsonObject
                {
                    ["type"] = "input_text",
                    ["text"] = index == 0 ? texts[index] : "\n" + texts[index],
                };
                if (breakpoints[index] is { } breakpoint)
                {
                    item["prompt_cache_breakpoint"] = breakpoint;
                }

                items.Add(item);
            }

            output = items;
        }
        else
        {
            output = JsonValue.Create(string.Join("\n", texts))!;
        }

        input.Add(
            new JsonObject
            {
                ["type"] = "function_call_output",
                ["call_id"] = group.Metadata.ToolCallId,
                ["output"] = output,
            }
        );

        foreach (var result in group.Results)
        {
            if (
                ResolveAdditionalTools(
                    result.ProviderMetadata,
                    providerOptionsName,
                    findAdditionalToolDefinition,
                    seenAdditionalToolWarnings,
                    droppedAdditionalToolNames,
                    warnings
                ) is
                { } additionalTools
            )
            {
                input.Add(AdditionalToolsItem(additionalTools));
            }
        }
    }

    private static bool IsProgrammaticToolName(string toolName, ToolNameMapping? toolNameMapping) =>
        toolNameMapping?.ToProviderToolName(toolName) == "programmatic_tool_calling";

    private static bool IsShellToolName(string toolName, ToolNameMapping? toolNameMapping) =>
        toolNameMapping?.ToProviderToolName(toolName) == "shell";

    // A stored shell call replays as an item reference. Without storage, a call is rebuilt as a shell_call.
    private static void AddShellCall(
        JsonArray input,
        GeneratedToolCall call,
        JsonElement? options,
        bool store,
        bool hasContinuation,
        bool hasConversation
    )
    {
        var itemId = OpenAIJson.String(options, "itemId");
        if (hasConversation && itemId != null)
        {
            return;
        }

        if (store && itemId != null)
        {
            // A previous response id already holds the client-executed call.
            if (call.ProviderExecuted || !hasContinuation)
            {
                input.Add(new JsonObject { ["type"] = "item_reference", ["id"] = itemId });
            }

            return;
        }

        // A stored provider-executed call has no id to reference, and it cannot be rebuilt without its output.
        if (store && call.ProviderExecuted)
        {
            return;
        }

        var action = ShellInput(call.ArgumentsJson);
        var shellAction = new JsonObject { ["commands"] = action["commands"]!.DeepClone() };
        if (action["timeoutMs"] is { } timeout)
        {
            shellAction["timeout_ms"] = timeout.DeepClone();
        }

        if (action["maxOutputLength"] is { } maxOutput)
        {
            shellAction["max_output_length"] = maxOutput.DeepClone();
        }

        var shellCall = new JsonObject { ["type"] = "shell_call", ["call_id"] = call.ToolCallId };
        if (itemId != null)
        {
            shellCall["id"] = itemId;
        }

        shellCall["status"] = "completed";
        shellCall["action"] = shellAction;
        input.Add(shellCall);
    }

    // The shell input schema: { action: { commands: string[], timeoutMs?: number, maxOutputLength?: number } }.
    private static JsonObject ShellInput(string argumentsJson)
    {
        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(argumentsJson);
        }
        catch (JsonException exception)
        {
            throw new AiSdkException("The shell tool call input is not valid JSON.", exception);
        }

        if (
            (parsed as JsonObject)?["action"] is not JsonObject action
            || action["commands"] is not JsonArray commands
            || commands.Any(command => !IsKind(command, JsonValueKind.String))
            || action["timeoutMs"] is { } timeout && !IsKind(timeout, JsonValueKind.Number)
            || action["maxOutputLength"] is { } maxOutput
                && !IsKind(maxOutput, JsonValueKind.Number)
        )
        {
            throw new AiSdkException(
                "The shell tool call input needs an action with a commands array of strings."
            );
        }

        return action;
    }

    private static bool IsKind(JsonNode? node, JsonValueKind kind) =>
        node is JsonValue value && value.GetValueKind() == kind;

    // The shell output schema: { maxOutputLength?: number, output: { stdout, stderr, outcome: { type: "timeout" } | { type: "exit", exitCode } }[] }.
    private static JsonObject ShellCallOutput(string callId, string outputJson)
    {
        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(outputJson);
        }
        catch (JsonException exception)
        {
            throw new AiSdkException("The shell tool result is not valid JSON.", exception);
        }

        if (parsed is not JsonObject parsedObject || parsedObject["output"] is not JsonArray items)
        {
            throw new AiSdkException("The shell tool result needs an output array.");
        }

        var output = new JsonArray();
        foreach (var item in items)
        {
            if (
                item is not JsonObject entry
                || !IsKind(entry["stdout"], JsonValueKind.String)
                || !IsKind(entry["stderr"], JsonValueKind.String)
                || entry["outcome"] is not JsonObject outcome
            )
            {
                throw new AiSdkException(
                    "Each shell tool result output needs stdout, stderr, and an outcome."
                );
            }

            var outcomeType = IsKind(outcome["type"], JsonValueKind.String)
                ? outcome["type"]!.GetValue<string>()
                : null;
            JsonObject converted = outcomeType switch
            {
                "timeout" => new JsonObject { ["type"] = "timeout" },
                "exit" when IsKind(outcome["exitCode"], JsonValueKind.Number) => new JsonObject
                {
                    ["type"] = "exit",
                    ["exit_code"] = outcome["exitCode"]!.DeepClone(),
                },
                _ => throw new AiSdkException(
                    "The shell tool result outcome must be a timeout or an exit with an exitCode."
                ),
            };

            output.Add(
                new JsonObject
                {
                    ["stdout"] = entry["stdout"]!.DeepClone(),
                    ["stderr"] = entry["stderr"]!.DeepClone(),
                    ["outcome"] = converted,
                }
            );
        }

        var shellCallOutput = new JsonObject
        {
            ["type"] = "shell_call_output",
            ["call_id"] = callId,
        };
        if (parsedObject["maxOutputLength"] is { } maxOutputLength)
        {
            if (!IsKind(maxOutputLength, JsonValueKind.Number))
            {
                throw new AiSdkException("The shell tool result maxOutputLength must be a number.");
            }

            shellCallOutput["max_output_length"] = maxOutputLength.DeepClone();
        }

        shellCallOutput["output"] = output;
        return shellCallOutput;
    }

    // A stored apply_patch call replays as an item reference. Without storage, it is rebuilt as an apply_patch_call that the apply_patch_call_output matches.
    private static void AddApplyPatchCall(
        JsonArray input,
        GeneratedToolCall call,
        JsonElement? options,
        bool store,
        bool hasContinuation,
        bool hasConversation
    )
    {
        var itemId = OpenAIJson.String(options, "itemId");
        if (hasConversation && itemId != null)
        {
            return;
        }

        if (store && itemId != null)
        {
            // A previous response id already holds the call.
            if (!hasContinuation)
            {
                input.Add(new JsonObject { ["type"] = "item_reference", ["id"] = itemId });
            }

            return;
        }

        var applyPatchInput = ApplyPatchInput(call.ArgumentsJson);
        var applyPatchCall = new JsonObject
        {
            ["type"] = "apply_patch_call",
            ["call_id"] = applyPatchInput["callId"]!.DeepClone(),
        };
        if (itemId != null)
        {
            applyPatchCall["id"] = itemId;
        }

        applyPatchCall["status"] = "completed";
        applyPatchCall["operation"] = applyPatchInput["operation"]!.DeepClone();
        input.Add(applyPatchCall);
    }

    // A stored local_shell call replays as an item reference. Without storage, it is rebuilt as a local_shell_call that the local_shell_call_output matches.
    private static void AddLocalShellCall(
        JsonArray input,
        GeneratedToolCall call,
        JsonElement? options,
        bool store,
        bool hasContinuation,
        bool hasConversation
    )
    {
        var itemId = OpenAIJson.String(options, "itemId");
        if (hasConversation && itemId != null)
        {
            return;
        }

        if (store && itemId != null)
        {
            // A previous response id already holds the call.
            if (!hasContinuation)
            {
                input.Add(new JsonObject { ["type"] = "item_reference", ["id"] = itemId });
            }

            return;
        }

        var action = LocalShellAction(call.ArgumentsJson);
        var wireAction = new JsonObject
        {
            ["type"] = "exec",
            ["command"] = action["command"]!.DeepClone(),
        };
        foreach (
            var (key, wireKey) in new[]
            {
                ("timeoutMs", "timeout_ms"),
                ("user", "user"),
                ("workingDirectory", "working_directory"),
                ("env", "env"),
            }
        )
        {
            if (action[key] is { } value)
            {
                wireAction[wireKey] = value.DeepClone();
            }
        }

        var localShellCall = new JsonObject
        {
            ["type"] = "local_shell_call",
            ["call_id"] = call.ToolCallId,
        };
        if (itemId != null)
        {
            localShellCall["id"] = itemId;
        }

        localShellCall["action"] = wireAction;
        input.Add(localShellCall);
    }

    // The local_shell input schema: { action: { type: "exec", command: string[], timeoutMs?: number, user?: string, workingDirectory?: string, env?: Record<string, string> } }.
    private static JsonObject LocalShellAction(string argumentsJson)
    {
        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(argumentsJson);
        }
        catch (JsonException exception)
        {
            throw new AiSdkException(
                "The local_shell tool call input is not valid JSON.",
                exception
            );
        }

        if (
            (parsed as JsonObject)?["action"] is not JsonObject action
            || action["type"] is not JsonValue { } type
            || type.GetValueKind() != JsonValueKind.String
            || type.GetValue<string>() != "exec"
            || action["command"] is not JsonArray command
            || command.Any(part => !IsKind(part, JsonValueKind.String))
            || action["timeoutMs"] is { } timeout && !IsKind(timeout, JsonValueKind.Number)
            || action["user"] is { } user && !IsKind(user, JsonValueKind.String)
            || action["workingDirectory"] is { } workingDirectory
                && !IsKind(workingDirectory, JsonValueKind.String)
            || action["env"] is { } env
                && (
                    env is not JsonObject envObject
                    || envObject.Any(entry => !IsKind(entry.Value, JsonValueKind.String))
                )
        )
        {
            throw new AiSdkException(
                "The local_shell tool call input needs an exec action with a command array of strings."
            );
        }

        return action;
    }

    // The local_shell output schema: { output: string }.
    private static JsonObject LocalShellCallOutput(string callId, string outputJson)
    {
        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(outputJson);
        }
        catch (JsonException exception)
        {
            throw new AiSdkException("The local_shell tool result is not valid JSON.", exception);
        }

        if (
            (parsed as JsonObject)?["output"] is not { } output
            || !IsKind(output, JsonValueKind.String)
        )
        {
            throw new AiSdkException("The local_shell tool result needs an output string.");
        }

        return new JsonObject
        {
            ["type"] = "local_shell_call_output",
            ["call_id"] = callId,
            ["output"] = output.DeepClone(),
        };
    }

    // The apply_patch input schema: { callId: string, operation: create_file | update_file { path, diff } | delete_file { path } }.
    private static JsonObject ApplyPatchInput(string argumentsJson)
    {
        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(argumentsJson);
        }
        catch (JsonException exception)
        {
            throw new AiSdkException(
                "The apply_patch tool call input is not valid JSON.",
                exception
            );
        }

        if (
            parsed is not JsonObject root
            || !IsKind(root["callId"], JsonValueKind.String)
            || root["operation"] is not JsonObject operation
            || !IsKind(operation["path"], JsonValueKind.String)
            || operation["type"] is not JsonValue typeValue
            || typeValue.GetValueKind() != JsonValueKind.String
        )
        {
            throw new AiSdkException(
                "The apply_patch tool call input needs a callId and an operation with a type and a path."
            );
        }

        var type = typeValue.GetValue<string>();
        var rebuilt = new JsonObject { ["type"] = type, ["path"] = operation["path"]!.DeepClone() };
        switch (type)
        {
            case "create_file":
            case "update_file":
                if (!IsKind(operation["diff"], JsonValueKind.String))
                {
                    throw new AiSdkException(
                        "The apply_patch " + type + " operation needs a diff string."
                    );
                }

                rebuilt["diff"] = operation["diff"]!.DeepClone();
                break;
            case "delete_file":
                break;
            default:
                throw new AiSdkException(
                    "The apply_patch operation type must be create_file, update_file, or delete_file."
                );
        }

        return new JsonObject { ["callId"] = root["callId"]!.DeepClone(), ["operation"] = rebuilt };
    }

    // The apply_patch output schema: { status: "completed" | "failed", output?: string }.
    private static JsonObject ApplyPatchCallOutput(string callId, string outputJson)
    {
        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(outputJson);
        }
        catch (JsonException exception)
        {
            throw new AiSdkException("The apply_patch tool result is not valid JSON.", exception);
        }

        if (
            parsed is not JsonObject parsedObject
            || !IsKind(parsedObject["status"], JsonValueKind.String)
            || parsedObject["status"]!.GetValue<string>() is not ("completed" or "failed")
        )
        {
            throw new AiSdkException(
                "The apply_patch tool result needs a status of completed or failed."
            );
        }

        var item = new JsonObject
        {
            ["type"] = "apply_patch_call_output",
            ["call_id"] = callId,
            ["status"] = parsedObject["status"]!.DeepClone(),
        };
        if (parsedObject["output"] is { } output)
        {
            if (!IsKind(output, JsonValueKind.String))
            {
                throw new AiSdkException("The apply_patch tool result output must be a string.");
            }

            item["output"] = output.DeepClone();
        }

        return item;
    }

    private static IReadOnlyDictionary<string, JsonElement>? AssistantCallOptions(
        AssistantModelMessage assistant,
        GeneratedToolCall call
    ) =>
        assistant
            .Parts.OfType<AssistantToolCallPart>()
            .FirstOrDefault(part => part.ToolCall == call)
            ?.ProviderOptions;

    private static IReadOnlyDictionary<string, JsonElement>? ToOptions(
        JsonElement? providerMetadata
    ) =>
        providerMetadata is { ValueKind: JsonValueKind.Object } metadata
            ? metadata
                .EnumerateObject()
                .ToDictionary(property => property.Name, property => property.Value.Clone())
            : null;

    // The Responses caller of a tool call: a program caller uses caller_id, others pass through.
    private static JsonNode? ToolCaller(JsonElement? options) =>
        OpenAIJson.Child(options, "caller") is { ValueKind: JsonValueKind.Object } caller
            ? OpenAIJson.String(caller, "type") == "program"
                ? new JsonObject
                {
                    ["type"] = "program",
                    ["caller_id"] = OpenAIJson.String(caller, "callerId"),
                }
                : JsonNode.Parse(caller.GetRawText())
            : null;

    // A provider-executed program call replays as an item reference when stored, else as a program item.
    private static void AddProgramCall(
        JsonArray input,
        GeneratedToolCall call,
        JsonElement? options,
        bool store,
        bool hasConversation
    )
    {
        var itemId = OpenAIJson.String(options, "itemId");
        if (hasConversation && itemId != null)
        {
            return;
        }

        if (store && itemId != null)
        {
            input.Add(new JsonObject { ["type"] = "item_reference", ["id"] = itemId });
            return;
        }

        JsonObject? programInput;
        try
        {
            programInput = JsonNode.Parse(call.ArgumentsJson) as JsonObject;
        }
        catch (JsonException exception)
        {
            throw new AiSdkException(
                "The programmatic tool call input is not valid JSON.",
                exception
            );
        }

        if (
            programInput?["code"] is not JsonValue code
            || code.GetValueKind() != JsonValueKind.String
            || programInput["fingerprint"] is not JsonValue fingerprint
            || fingerprint.GetValueKind() != JsonValueKind.String
        )
        {
            throw new AiSdkException(
                "The programmatic tool call input needs a string code and fingerprint."
            );
        }

        input.Add(
            new JsonObject
            {
                ["type"] = "program",
                ["id"] = itemId ?? call.ToolCallId,
                ["call_id"] = call.ToolCallId,
                ["code"] = code.GetValue<string>(),
                ["fingerprint"] = fingerprint.GetValue<string>(),
            }
        );
    }

    // A program result replays as an item reference when stored, else as a program_output item.
    private static void AddProgramOutput(
        JsonArray input,
        AssistantToolResultPart part,
        string providerOptionsName,
        bool store,
        bool hasConversation
    )
    {
        // The conversation already holds provider-executed results.
        if (hasConversation)
        {
            return;
        }

        var itemId = OpenAIJson.String(
            ProviderOptionsFor(part.ProviderOptions, providerOptionsName),
            "itemId"
        )
            ?? OpenAIJson.String(
                ProviderOptionsFor(ToOptions(part.Output.ProviderMetadata), providerOptionsName),
                "itemId"
            )
            ?? part.ToolCallId;
        if (store)
        {
            input.Add(new JsonObject { ["type"] = "item_reference", ["id"] = itemId });
            return;
        }

        if (part.Output.OutputType != "json")
        {
            return;
        }

        JsonObject? output;
        try
        {
            output = JsonNode.Parse(part.Output.OutputJson) as JsonObject;
        }
        catch (JsonException exception)
        {
            throw new AiSdkException("The programmatic tool result is not valid JSON.", exception);
        }

        if (
            output?["result"] is not JsonValue result
            || result.GetValueKind() != JsonValueKind.String
            || output["status"] is not JsonValue status
            || status.GetValueKind() != JsonValueKind.String
            || (
                status.GetValue<string>() != "completed"
                && status.GetValue<string>() != "incomplete"
            )
        )
        {
            throw new AiSdkException(
                "The programmatic tool result needs a string result and a completed or incomplete status."
            );
        }

        input.Add(
            new JsonObject
            {
                ["type"] = "program_output",
                ["id"] = itemId,
                ["call_id"] = part.ToolCallId,
                ["result"] = result.GetValue<string>(),
                ["status"] = status.GetValue<string>(),
            }
        );
    }

    // Replaces the additionalTools of a tool result with the definitions of the tools registered for this request.
    private static JsonArray? ResolveAdditionalTools(
        JsonElement? toolResultOptions,
        string providerOptionsName,
        Func<string, string, AdditionalToolResolution?>? findAdditionalToolDefinition,
        ISet<string> seenWarnings,
        List<string> droppedToolNames,
        List<OpenAICallWarning> warnings
    ) =>
        FilterAdditionalTools(
            ToolResultAdditionalTools(toolResultOptions, providerOptionsName),
            findAdditionalToolDefinition,
            seenWarnings,
            droppedToolNames,
            warnings
        );

    // Prompts can contain replayed conversation history, so their tool definitions are not trusted. An entry only
    // selects a registered tool: the provider sends the definition derived from the registered tool, and entries
    // for tools that are not registered are dropped.
    private static JsonArray? FilterAdditionalTools(
        JsonArray? additionalTools,
        Func<string, string, AdditionalToolResolution?>? findAdditionalToolDefinition,
        ISet<string> seenWarnings,
        List<string> droppedToolNames,
        List<OpenAICallWarning> warnings
    )
    {
        if (additionalTools == null || additionalTools.Count == 0)
        {
            return null;
        }

        var resolvedTools = new JsonArray();
        var seenDefinitions = new HashSet<string>();
        foreach (var node in additionalTools)
        {
            var additionalTool = node!.AsObject();
            var type = additionalTool["type"]!.GetValue<string>();
            var name = additionalTool["name"]!.GetValue<string>();
            var resolved = findAdditionalToolDefinition?.Invoke(type, name);
            if (resolved == null)
            {
                AddUnique(droppedToolNames, name);
                continue;
            }

            foreach (var warning in resolved.Warnings)
            {
                AddWarning(
                    new OpenAICallWarning(
                        warning.Type,
                        warning.Feature,
                        warning.Feature == null ? null : warning.Message,
                        warning.Feature == null ? warning.Message : null
                    ),
                    seenWarnings,
                    warnings
                );
            }

            if (resolved.Definition == null)
            {
                continue;
            }

            PushDefinitionMismatchWarning(
                additionalTool,
                resolved.Definition,
                seenWarnings,
                warnings
            );
            var definition = NarrowNamespaceDefinition(
                resolved.Definition,
                additionalTool,
                droppedToolNames
            );
            if (definition == null)
            {
                continue;
            }

            // Replayed history can repeat an entry; send each definition once.
            if (seenDefinitions.Add(definition.ToJsonString()))
            {
                // The resolution is cached per request, and a node has one parent.
                resolvedTools.Add(definition.DeepClone());
            }
        }

        return resolvedTools.Count > 0 ? resolvedTools : null;
    }

    // A namespace entry names the tools to load. Send only the registered tools of the namespace that the entry
    // names, so an entry cannot load more of a namespace than it asked for.
    private static JsonObject? NarrowNamespaceDefinition(
        JsonObject registeredDefinition,
        JsonObject additionalTool,
        List<string> droppedToolNames
    )
    {
        if (
            registeredDefinition["type"]?.GetValue<string>() != "namespace"
            || additionalTool["type"]?.GetValue<string>() != "namespace"
        )
        {
            return registeredDefinition;
        }

        var requestedToolNames = additionalTool["tools"]!
            .AsArray()
            .Select(tool => tool!["name"]!.GetValue<string>())
            .ToList();
        var registeredTools = registeredDefinition["tools"]!.AsArray();
        var registeredToolNames = new HashSet<string>(
            registeredTools.Select(tool => tool!["name"]!.GetValue<string>())
        );
        foreach (var toolName in requestedToolNames)
        {
            if (!registeredToolNames.Contains(toolName))
            {
                AddUnique(droppedToolNames, toolName);
            }
        }

        var tools = new JsonArray();
        foreach (var tool in registeredTools)
        {
            if (requestedToolNames.Contains(tool!["name"]!.GetValue<string>()))
            {
                tools.Add(tool.DeepClone());
            }
        }

        if (tools.Count == 0)
        {
            return null;
        }

        var narrowed = (JsonObject)registeredDefinition.DeepClone();
        narrowed["tools"] = tools;
        return narrowed;
    }

    // The entry in the history is not sent, so a strict or defer_loading value that differs from the registered
    // tool is ignored. Warn instead of silently changing the behavior the entry asks for.
    private static void PushDefinitionMismatchWarning(
        JsonObject additionalTool,
        JsonObject definition,
        ISet<string> seenWarnings,
        List<OpenAICallWarning> warnings
    )
    {
        if (
            additionalTool["type"]?.GetValue<string>() != "function"
            || definition["type"]?.GetValue<string>() != "function"
        )
        {
            return;
        }

        var mismatchedFields = new List<string>();
        if (
            additionalTool["strict"] is { } strict
            && strict.GetValue<bool>() != definition["strict"]?.GetValue<bool>()
        )
        {
            mismatchedFields.Add("strict");
        }

        if (
            additionalTool["defer_loading"] is { } deferLoading
            && deferLoading.GetValue<bool>()
                != (definition["defer_loading"]?.GetValue<bool>() ?? false)
        )
        {
            mismatchedFields.Add("defer_loading");
        }

        if (mismatchedFields.Count == 0)
        {
            return;
        }

        AddWarning(
            new OpenAICallWarning(
                "unsupported",
                "additionalTools entry \"" + definition["name"]!.GetValue<string>() + "\"",
                "the history entry sets "
                    + string.Join(" and ", mismatchedFields)
                    + " differently from the registered tool; the registered definition is sent"
            ),
            seenWarnings,
            warnings
        );
    }

    private static void AddWarning(
        OpenAICallWarning warning,
        ISet<string> seenWarnings,
        List<OpenAICallWarning> warnings
    )
    {
        var signature =
            warning.Type + "\n" + warning.Feature + "\n" + warning.Details + "\n" + warning.Message;
        if (seenWarnings.Add(signature))
        {
            warnings.Add(warning);
        }
    }

    private static void AddUnique(List<string> names, string name)
    {
        if (!names.Contains(name))
        {
            names.Add(name);
        }
    }

    private static string FormatDroppedToolNames(List<string> names)
    {
        const int maxNames = 5;
        const int maxNameLength = 80;
        var formattedNames = names
            .Take(maxNames)
            .Select(name =>
                JsonSerializer.Serialize(
                    name.Length > maxNameLength ? name.Substring(0, maxNameLength) + "..." : name
                )
            );
        return names.Count > maxNames
            ? string.Join(", ", formattedNames) + " and " + (names.Count - maxNames) + " more"
            : string.Join(", ", formattedNames);
    }

    private static JsonObject AdditionalToolsItem(JsonArray tools) =>
        new()
        {
            ["type"] = "additional_tools",
            ["role"] = "developer",
            ["tools"] = tools,
        };

    // Tool definitions loaded immediately after an ordinary tool result. Options under the provider name win,
    // then the "openai" ones. A malformed list is an error.
    private static JsonArray? ToolResultAdditionalTools(
        JsonElement? toolResultOptions,
        string providerOptionsName
    )
    {
        if (toolResultOptions is not { ValueKind: JsonValueKind.Object } root)
        {
            return null;
        }

        if (
            root.TryGetProperty(providerOptionsName, out var named)
            && AdditionalToolsList(named) is { } list
        )
        {
            return list;
        }

        return providerOptionsName != "openai" && root.TryGetProperty("openai", out var openai)
            ? AdditionalToolsList(openai)
            : null;
    }

    // The validated additionalTools of a provider options object, or null when it has none.
    private static JsonArray? AdditionalToolsList(JsonElement? options)
    {
        if (
            options is not { ValueKind: JsonValueKind.Object } root
            || !root.TryGetProperty("additionalTools", out var list)
        )
        {
            return null;
        }

        if (list.ValueKind != JsonValueKind.Array)
        {
            throw new AiSdkException(
                "Invalid additionalTools: expected an array of tool definitions."
            );
        }

        var tools = new JsonArray();
        foreach (var definition in list.EnumerateArray())
        {
            tools.Add(ValidateAdditionalTool(definition));
        }

        return tools;
    }

    private static JsonNode ValidateAdditionalTool(JsonElement definition)
    {
        if (
            definition.ValueKind != JsonValueKind.Object
            || !definition.TryGetProperty("type", out var type)
            || type.ValueKind != JsonValueKind.String
        )
        {
            throw new AiSdkException(
                "Invalid additionalTools: each definition must be an object with a type."
            );
        }

        switch (type.GetString())
        {
            case "function":
                ValidateFunctionDefinition(definition);
                break;
            case "custom":
                ValidateCustomDefinition(definition);
                break;
            case "namespace":
                ValidateNamespaceDefinition(definition);
                break;
            default:
                throw new AiSdkException(
                    "Invalid additionalTools: unsupported type '" + type.GetString() + "'."
                );
        }

        return JsonNode.Parse(definition.GetRawText())!;
    }

    private static void ValidateFunctionDefinition(JsonElement definition)
    {
        var hasName = false;
        var hasParameters = false;
        foreach (var property in definition.EnumerateObject())
        {
            switch (property.Name)
            {
                case "type":
                    break;
                case "name" when IsNonEmptyString(property.Value):
                    hasName = true;
                    break;
                case "description" when property.Value.ValueKind == JsonValueKind.String:
                    break;
                case "parameters" when property.Value.ValueKind == JsonValueKind.Object:
                    hasParameters = true;
                    break;
                case "strict"
                or "defer_loading"
                    when property.Value.ValueKind is JsonValueKind.True or JsonValueKind.False:
                    break;
                default:
                    throw new AiSdkException(
                        "Invalid additionalTools: unexpected or malformed '" + property.Name + "'."
                    );
            }
        }

        if (!hasName || !hasParameters)
        {
            throw new AiSdkException(
                "Invalid additionalTools: a function definition needs type, name and parameters."
            );
        }
    }

    private static void ValidateCustomDefinition(JsonElement definition)
    {
        var hasName = false;
        foreach (var property in definition.EnumerateObject())
        {
            switch (property.Name)
            {
                case "type":
                    break;
                case "name" when IsNonEmptyString(property.Value):
                    hasName = true;
                    break;
                case "description" when property.Value.ValueKind == JsonValueKind.String:
                    break;
                case "format" when property.Value.ValueKind == JsonValueKind.Object:
                    ValidateCustomFormat(property.Value);
                    break;
                default:
                    throw new AiSdkException(
                        "Invalid additionalTools: unexpected or malformed '" + property.Name + "'."
                    );
            }
        }

        if (!hasName)
        {
            throw new AiSdkException(
                "Invalid additionalTools: a custom definition needs type and name."
            );
        }
    }

    private static void ValidateCustomFormat(JsonElement format)
    {
        if (!format.TryGetProperty("type", out var type) || type.ValueKind != JsonValueKind.String)
        {
            throw new AiSdkException("Invalid additionalTools: a custom format needs a type.");
        }

        var isGrammar = type.GetString() == "grammar";
        if (!isGrammar && type.GetString() != "text")
        {
            throw new AiSdkException(
                "Invalid additionalTools: unsupported custom format type '"
                    + type.GetString()
                    + "'."
            );
        }

        var hasSyntax = false;
        var hasDefinition = false;
        foreach (var property in format.EnumerateObject())
        {
            switch (property.Name)
            {
                case "type":
                    break;
                case "syntax"
                    when isGrammar
                        && property.Value.ValueKind == JsonValueKind.String
                        && property.Value.GetString() is "regex" or "lark":
                    hasSyntax = true;
                    break;
                case "definition"
                    when isGrammar && property.Value.ValueKind == JsonValueKind.String:
                    hasDefinition = true;
                    break;
                default:
                    throw new AiSdkException(
                        "Invalid additionalTools: unexpected or malformed '" + property.Name + "'."
                    );
            }
        }

        if (isGrammar && (!hasSyntax || !hasDefinition))
        {
            throw new AiSdkException(
                "Invalid additionalTools: a grammar format needs syntax and definition."
            );
        }
    }

    private static void ValidateNamespaceDefinition(JsonElement definition)
    {
        var hasName = false;
        var hasDescription = false;
        var hasTools = false;
        foreach (var property in definition.EnumerateObject())
        {
            switch (property.Name)
            {
                case "type":
                    break;
                case "name" when IsNonEmptyString(property.Value):
                    hasName = true;
                    break;
                case "description" when property.Value.ValueKind == JsonValueKind.String:
                    hasDescription = true;
                    break;
                case "tools"
                    when property.Value.ValueKind == JsonValueKind.Array
                        && property.Value.GetArrayLength() > 0:
                    foreach (var nested in property.Value.EnumerateArray())
                    {
                        if (
                            nested.ValueKind != JsonValueKind.Object
                            || !nested.TryGetProperty("type", out var nestedType)
                            || nestedType.ValueKind != JsonValueKind.String
                            || nestedType.GetString() != "function"
                        )
                        {
                            throw new AiSdkException(
                                "Invalid additionalTools: a namespace holds function definitions."
                            );
                        }

                        ValidateFunctionDefinition(nested);
                    }

                    hasTools = true;
                    break;
                default:
                    throw new AiSdkException(
                        "Invalid additionalTools: unexpected or malformed '" + property.Name + "'."
                    );
            }
        }

        if (!hasName || !hasDescription || !hasTools)
        {
            throw new AiSdkException(
                "Invalid additionalTools: a namespace definition needs type, name, description and a non-empty tools list."
            );
        }
    }

    private static bool IsNonEmptyString(JsonElement value) =>
        value.ValueKind == JsonValueKind.String && value.GetString()!.Length > 0;

    // The input of a custom tool call is text. A string input is sent as is and any other input as JSON.
    private static string CustomToolCallInput(string argumentsJson)
    {
        using var document = JsonDocument.Parse(argumentsJson);
        return document.RootElement.ValueKind == JsonValueKind.String
            ? document.RootElement.GetString()!
            : JsonSerializer.Serialize(document.RootElement);
    }

    private static string? TextItemId(AssistantContentPart part, string providerOptionsName) =>
        OpenAIJson.String(ProviderOptionsFor(part.ProviderOptions, providerOptionsName), "itemId");

    // Stored messages of reasoning models are bound to the reasoning item that produced them. When the reasoning
    // has been removed (for example with pruneMessages), referencing the message fails, so it is sent with its content.
    // An openai.compaction custom part is replayed as a reference when stored, else as the full compaction item.
    private static void AddCompaction(
        JsonArray input,
        AssistantModelMessage assistant,
        string providerOptionsName,
        bool store,
        bool hasConversation
    )
    {
        foreach (var part in assistant.Parts.OfType<AssistantCustomPart>())
        {
            if (part.Kind != "openai.compaction")
            {
                continue;
            }

            var options = ProviderOptionsFor(part.ProviderOptions, providerOptionsName);
            var id = OpenAIJson.String(options, "itemId");
            if (id == null || hasConversation)
            {
                continue;
            }

            if (store)
            {
                input.Add(new JsonObject { ["type"] = "item_reference", ["id"] = id });
                continue;
            }

            input.Add(
                new JsonObject
                {
                    ["type"] = "compaction",
                    ["id"] = id,
                    ["encrypted_content"] = OpenAIJson.String(options, "encryptedContent"),
                }
            );
        }
    }

    private static void AddStoredText(
        JsonArray input,
        AssistantModelMessage assistant,
        string providerOptionsName,
        bool explicitItem,
        bool store,
        bool isReasoningModel,
        bool hasConversation,
        bool afterToolParts = false
    )
    {
        var emittedTextItemIds = new HashSet<string>();
        var hasPrecedingReasoning = false;
        var seenToolPart = false;
        foreach (var part in assistant.Parts)
        {
            if (part is AssistantReasoningPart)
            {
                hasPrecedingReasoning = true;
                continue;
            }

            if (part is AssistantToolCallPart or AssistantToolResultPart)
            {
                seenToolPart = true;
                continue;
            }

            // Text keeps its place around the tool parts: one pass sends the text before them, the other the text after them.
            if (part is not AssistantTextPart text || seenToolPart != afterToolParts)
            {
                continue;
            }

            var id = TextItemId(part, providerOptionsName);

            // A conversation already holds its items, so sending them again fails with "Duplicate item found".
            if (hasConversation && id != null)
            {
                continue;
            }

            if (store && id != null && (!isReasoningModel || hasPrecedingReasoning))
            {
                if (emittedTextItemIds.Add(id))
                {
                    input.Add(new JsonObject { ["type"] = "item_reference", ["id"] = id });
                }

                continue;
            }

            input.Add(MessageItem("assistant", JsonValue.Create(text.Text)!, explicitItem));
        }
    }

    // Reasoning already held by a conversation or previous response is filtered out.
    private static void AddStoredReasoning(
        JsonArray input,
        AssistantModelMessage assistant,
        string providerOptionsName,
        bool hasContinuation,
        bool store
    )
    {
        var options = ProviderOptionsFor(assistant.ReasoningProviderOptions, providerOptionsName);
        var itemId = OpenAIJson.String(options, "itemId");
        if (itemId == null || hasContinuation)
        {
            return;
        }

        if (store)
        {
            input.Add(new JsonObject { ["type"] = "item_reference", ["id"] = itemId });
            return;
        }

        var summary = new JsonArray();
        if (!string.IsNullOrEmpty(assistant.Reasoning))
        {
            summary.Add(
                new JsonObject { ["type"] = "summary_text", ["text"] = assistant.Reasoning }
            );
        }

        var item = new JsonObject { ["type"] = "reasoning", ["id"] = itemId };
        if (OpenAIJson.String(options, "reasoningEncryptedContent") is { } encrypted)
        {
            item["encrypted_content"] = encrypted;
        }

        item["summary"] = summary;
        input.Add(item);
    }

    private static JsonObject ConvertFile(FileContentPart file)
    {
        var mediaType = file.MediaType ?? "application/octet-stream";
        var inline = file.Data != null;
        var full = OpenAIJson.ResolveFullMediaType(
            mediaType,
            file.Data,
            inline && string.IsNullOrEmpty(file.Url)
        );
        if (OpenAIJson.TopLevel(full) == "image")
        {
            var image = new JsonObject { ["type"] = "input_image" };
            if (!string.IsNullOrEmpty(file.Url))
            {
                image["image_url"] = file.Url;
            }
            else
            {
                image["image_url"] =
                    "data:"
                    + full
                    + ";base64,"
                    + Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
            }

            return image;
        }

        if (!string.IsNullOrEmpty(file.Url))
        {
            return new JsonObject { ["type"] = "input_file", ["file_url"] = file.Url };
        }

        if (full != "application/pdf")
        {
            throw new AiSdkException("file part media type " + full);
        }

        return new JsonObject
        {
            ["type"] = "input_file",
            ["filename"] = string.IsNullOrEmpty(file.FileName) ? "data" : file.FileName,
            ["file_data"] =
                "data:"
                + full
                + ";base64,"
                + Convert.ToBase64String(file.Data ?? Array.Empty<byte>()),
        };
    }

    private static JsonObject MessageItem(string role, JsonNode content, bool explicitItem)
    {
        var item = new JsonObject { ["role"] = role, ["content"] = content };
        if (explicitItem)
        {
            item["type"] = "message";
        }

        return item;
    }

    private static void Copy(JsonObject body, string name, string? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static void Copy(JsonObject body, string name, JsonNode? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }

    private static void Remove(
        JsonObject body,
        List<OpenAICallWarning> warnings,
        string field,
        string feature,
        string details
    )
    {
        if (body[field] == null)
        {
            return;
        }

        body.Remove(field);
        warnings.Add(new OpenAICallWarning("unsupported", feature, details));
    }

    private static bool Contains(IReadOnlyList<string> values, string effort)
    {
        foreach (var value in values)
        {
            if (value == effort)
            {
                return true;
            }
        }

        return false;
    }

    private static List<CallWarning> ToCallWarnings(IReadOnlyList<OpenAICallWarning> warnings)
    {
        var result = new List<CallWarning>(warnings.Count);
        foreach (var warning in warnings)
        {
            result.Add(warning.ToCallWarning());
        }

        return result;
    }

    internal sealed class ResponsesInput
    {
        public ResponsesInput(JsonArray input, IReadOnlyList<OpenAICallWarning> warnings)
        {
            Input = input;
            Warnings = warnings;
        }

        public JsonArray Input { get; }

        public IReadOnlyList<OpenAICallWarning> Warnings { get; }
    }
}
