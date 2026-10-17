// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>Options of <see cref="OpenAITools.ImageGeneration"/>. Maps to the upstream <c>imageGeneration</c> args.</summary>
public sealed class OpenAIImageGenerationToolOptions
{
    /// <summary>Whether to generate a new image or edit an existing one: <c>generate</c>, <c>edit</c>, or <c>auto</c>.</summary>
    public string? Action { get; set; }

    /// <summary>Background type: <c>auto</c>, <c>opaque</c>, or <c>transparent</c>.</summary>
    public string? Background { get; set; }

    /// <summary>Input fidelity: <c>low</c> or <c>high</c>.</summary>
    public string? InputFidelity { get; set; }

    /// <summary>File id of the inpainting mask.</summary>
    public string? InputImageMaskFileId { get; set; }

    /// <summary>Base64-encoded inpainting mask.</summary>
    public string? InputImageMaskImageUrl { get; set; }

    /// <summary>The image generation model. Default: <c>gpt-image-1</c>.</summary>
    public string? Model { get; set; }

    /// <summary>Moderation level: <c>auto</c> or <c>low</c>.</summary>
    public string? Moderation { get; set; }

    /// <summary>Compression level of the output image, 0 to 100.</summary>
    public int? OutputCompression { get; set; }

    /// <summary>Output format: <c>png</c>, <c>jpeg</c>, or <c>webp</c>.</summary>
    public string? OutputFormat { get; set; }

    /// <summary>Number of partial images in streaming mode, 0 to 3.</summary>
    public int? PartialImages { get; set; }

    /// <summary>Quality: <c>auto</c>, <c>low</c>, <c>medium</c>, <c>high</c>, <c>xhigh</c>, or <c>max</c>.</summary>
    public string? Quality { get; set; }

    /// <summary>Size: <c>auto</c>, <c>1024x1024</c>, <c>1024x1536</c>, <c>1536x1024</c>, or <c>WIDTHxHEIGHT</c>.</summary>
    public string? Size { get; set; }
}

/// <summary>Options of <see cref="OpenAITools.WebSearch"/>.</summary>
public sealed class OpenAIWebSearchToolOptions
{
    /// <summary>Whether the tool may reach the live web.</summary>
    public bool? ExternalWebAccess { get; set; }

    /// <summary>Domains the search may use.</summary>
    public IReadOnlyList<string>? AllowedDomains { get; set; }

    /// <summary>Domains the search must not use.</summary>
    public IReadOnlyList<string>? BlockedDomains { get; set; }

    /// <summary>Search context size: <c>low</c>, <c>medium</c>, or <c>high</c>.</summary>
    public string? SearchContextSize { get; set; }

    /// <summary>Approximate user location.</summary>
    public OpenAIApproximateLocation? UserLocation { get; set; }
}

/// <summary>An approximate user location for the OpenAI web search tools.</summary>
public sealed class OpenAIApproximateLocation
{
    /// <summary>Country.</summary>
    public string? Country { get; set; }

    /// <summary>City.</summary>
    public string? City { get; set; }

    /// <summary>Region.</summary>
    public string? Region { get; set; }

    /// <summary>IANA timezone.</summary>
    public string? Timezone { get; set; }
}

/// <summary>A file search filter: a comparison (<see cref="Key"/>, <see cref="Type"/>, one value) or a compound (<c>and</c>/<c>or</c> over <see cref="Filters"/>).</summary>
public sealed class OpenAIFileSearchFilter
{
    /// <summary>Operator: <c>eq</c>, <c>ne</c>, <c>gt</c>, <c>gte</c>, <c>lt</c>, <c>lte</c>, <c>in</c>, <c>nin</c>, <c>and</c>, or <c>or</c>.</summary>
    public string Type { get; set; } = "";

    /// <summary>Attribute key of a comparison filter.</summary>
    public string? Key { get; set; }

    /// <summary>String value of a comparison filter.</summary>
    public string? StringValue { get; set; }

    /// <summary>Number value of a comparison filter.</summary>
    public double? NumberValue { get; set; }

    /// <summary>Boolean value of a comparison filter.</summary>
    public bool? BooleanValue { get; set; }

    /// <summary>String values of an <c>in</c>/<c>nin</c> comparison filter.</summary>
    public IReadOnlyList<string>? StringValues { get; set; }

    /// <summary>Nested filters of a compound filter.</summary>
    public IReadOnlyList<OpenAIFileSearchFilter>? Filters { get; set; }
}

/// <summary>Options of <see cref="OpenAITools.FileSearch"/>.</summary>
public sealed class OpenAIFileSearchToolOptions
{
    /// <summary>Ids of the vector stores to search.</summary>
    public IReadOnlyList<string> VectorStoreIds { get; set; } = [];

    /// <summary>Maximum number of results to return.</summary>
    public int? MaxNumResults { get; set; }

    /// <summary>Ranker used for the search.</summary>
    public string? Ranker { get; set; }

    /// <summary>Score threshold between 0 and 1.</summary>
    public double? ScoreThreshold { get; set; }

    /// <summary>A filter applied to the search.</summary>
    public OpenAIFileSearchFilter? Filters { get; set; }
}

/// <summary>Options of <see cref="OpenAITools.CodeInterpreter"/>. <see cref="ContainerId"/> and <see cref="FileIds"/> are alternatives; with neither the container is created automatically.</summary>
public sealed class OpenAICodeInterpreterToolOptions
{
    /// <summary>Id of an existing container.</summary>
    public string? ContainerId { get; set; }

    /// <summary>File ids made available to an automatically created container.</summary>
    public IReadOnlyList<string>? FileIds { get; set; }
}

/// <summary>A skill available in a local shell environment.</summary>
public sealed class OpenAIShellLocalSkill
{
    /// <summary>Skill name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Skill description.</summary>
    public string Description { get; set; } = "";

    /// <summary>Path of the skill directory.</summary>
    public string Path { get; set; } = "";
}

/// <summary>A domain secret of an allowlist network policy.</summary>
public sealed class OpenAIShellDomainSecret
{
    /// <summary>Domain the secret applies to.</summary>
    public string Domain { get; set; } = "";

    /// <summary>Secret name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Secret value.</summary>
    public string Value { get; set; } = "";
}

/// <summary>
/// Environment of <see cref="OpenAITools.Shell"/>: <c>containerAuto</c>, <c>containerReference</c>, or <c>local</c>.
/// </summary>
public sealed class OpenAIShellEnvironment
{
    /// <summary><c>containerAuto</c>, <c>containerReference</c>, or <c>local</c>.</summary>
    public string Type { get; set; } = "local";

    /// <summary>Container id of a <c>containerReference</c> environment.</summary>
    public string? ContainerId { get; set; }

    /// <summary>File ids of a <c>containerAuto</c> environment.</summary>
    public IReadOnlyList<string>? FileIds { get; set; }

    /// <summary>Memory limit of a <c>containerAuto</c> environment: <c>1g</c>, <c>4g</c>, <c>16g</c>, or <c>64g</c>.</summary>
    public string? MemoryLimit { get; set; }

    /// <summary>Disable network access of a <c>containerAuto</c> environment.</summary>
    public bool NetworkDisabled { get; set; }

    /// <summary>Allowed domains of a <c>containerAuto</c> environment network allowlist.</summary>
    public IReadOnlyList<string>? AllowedDomains { get; set; }

    /// <summary>Domain secrets of a network allowlist.</summary>
    public IReadOnlyList<OpenAIShellDomainSecret>? DomainSecrets { get; set; }

    /// <summary>Skills of a <c>local</c> environment.</summary>
    public IReadOnlyList<OpenAIShellLocalSkill>? Skills { get; set; }
}

/// <summary>Options of <see cref="OpenAITools.Shell"/>.</summary>
public sealed class OpenAIShellToolOptions
{
    /// <summary>The shell environment.</summary>
    public OpenAIShellEnvironment? Environment { get; set; }
}

/// <summary>Options of <see cref="OpenAITools.CustomTool"/>.</summary>
public sealed class OpenAICustomToolOptions
{
    /// <summary>The tool name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Description of the tool.</summary>
    public string? Description { get; set; }

    /// <summary>Grammar syntax of the input: <c>regex</c> or <c>lark</c>. Without it the input is unconstrained text.</summary>
    public string? GrammarSyntax { get; set; }

    /// <summary>Grammar definition, used with <see cref="GrammarSyntax"/>.</summary>
    public string? GrammarDefinition { get; set; }
}

/// <summary>Options of <see cref="OpenAITools.ToolSearch"/>.</summary>
public sealed class OpenAIToolSearchToolOptions
{
    /// <summary>Where the search runs: <c>server</c> or <c>client</c>.</summary>
    public string? Execution { get; set; }

    /// <summary>Description of the tool.</summary>
    public string? Description { get; set; }

    /// <summary>JSON schema of the search arguments.</summary>
    public JsonObject? Parameters { get; set; }
}

/// <summary>Options of <see cref="OpenAITools.WebSearchPreview"/>.</summary>
public sealed class OpenAIWebSearchPreviewToolOptions
{
    /// <summary>Search context size: <c>low</c>, <c>medium</c>, or <c>high</c>.</summary>
    public string? SearchContextSize { get; set; }

    /// <summary>Approximate user location.</summary>
    public OpenAIApproximateLocation? UserLocation { get; set; }
}

/// <summary>Options of <see cref="OpenAITools.Mcp"/>. One of <see cref="ServerUrl"/> or <see cref="ConnectorId"/> must be set.</summary>
public sealed class OpenAIMcpToolOptions
{
    /// <summary>A label for this MCP server, used to identify it in tool calls.</summary>
    public string ServerLabel { get; set; } = "";

    /// <summary>Allowed tool names.</summary>
    public IReadOnlyList<string>? AllowedToolNames { get; set; }

    /// <summary>Allow only read-only tools.</summary>
    public bool? AllowedToolsReadOnly { get; set; }

    /// <summary>OAuth access token for the MCP server or connector.</summary>
    public string? Authorization { get; set; }

    /// <summary>Identifier of a service connector.</summary>
    public string? ConnectorId { get; set; }

    /// <summary>HTTP headers to send to the MCP server.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; set; }

    /// <summary>Approval policy: <c>always</c> or <c>never</c>.</summary>
    public string? RequireApproval { get; set; }

    /// <summary>Tool names that never require approval.</summary>
    public IReadOnlyList<string>? NeverRequireApprovalToolNames { get; set; }

    /// <summary>Description of the server.</summary>
    public string? ServerDescription { get; set; }

    /// <summary>URL of the MCP server.</summary>
    public string? ServerUrl { get; set; }
}

/// <summary>
/// Factories for the OpenAI provider-defined tools, the port of upstream <c>openaiTools</c>. Each builds the
/// <see cref="LanguageModelTool"/> that OpenAI Responses maps to a hosted tool.
/// </summary>
public static class OpenAITools
{
    /// <summary>The image generation tool (<c>openai.image_generation</c>).</summary>
    public static LanguageModelTool ImageGeneration(OpenAIImageGenerationToolOptions? options = null)
    {
        var args = new JsonObject();
        if (options != null)
        {
            Set(args, "action", options.Action);
            Set(args, "background", options.Background);
            Set(args, "inputFidelity", options.InputFidelity);
            if (options.InputImageMaskFileId != null || options.InputImageMaskImageUrl != null)
            {
                var mask = new JsonObject();
                Set(mask, "fileId", options.InputImageMaskFileId);
                Set(mask, "imageUrl", options.InputImageMaskImageUrl);
                args["inputImageMask"] = mask;
            }

            Set(args, "model", options.Model);
            Set(args, "moderation", options.Moderation);
            if (options.OutputCompression is { } compression)
            {
                args["outputCompression"] = compression;
            }

            Set(args, "outputFormat", options.OutputFormat);
            if (options.PartialImages is { } partialImages)
            {
                args["partialImages"] = partialImages;
            }

            Set(args, "quality", options.Quality);
            Set(args, "size", options.Size);
        }

        return Create("openai.image_generation", "image_generation", args);
    }

    /// <summary>The web search tool (<c>openai.web_search</c>).</summary>
    public static LanguageModelTool WebSearch(OpenAIWebSearchToolOptions? options = null)
    {
        var args = new JsonObject();
        if (options != null)
        {
            if (options.ExternalWebAccess is { } access)
            {
                args["externalWebAccess"] = access;
            }

            if (options.AllowedDomains != null || options.BlockedDomains != null)
            {
                var filters = new JsonObject();
                SetList(filters, "allowedDomains", options.AllowedDomains);
                SetList(filters, "blockedDomains", options.BlockedDomains);
                args["filters"] = filters;
            }

            Set(args, "searchContextSize", options.SearchContextSize);
            if (options.UserLocation is { } location)
            {
                var userLocation = new JsonObject { ["type"] = "approximate" };
                Set(userLocation, "country", location.Country);
                Set(userLocation, "city", location.City);
                Set(userLocation, "region", location.Region);
                Set(userLocation, "timezone", location.Timezone);
                args["userLocation"] = userLocation;
            }
        }

        return Create("openai.web_search", "web_search", args);
    }

    /// <summary>The MCP tool (<c>openai.mcp</c>).</summary>
    public static LanguageModelTool Mcp(OpenAIMcpToolOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.ServerUrl == null && options.ConnectorId == null)
        {
            throw new ArgumentException("One of serverUrl or connectorId must be provided.", nameof(options));
        }

        var args = new JsonObject { ["serverLabel"] = options.ServerLabel };
        if (options.AllowedToolsReadOnly != null)
        {
            var allowed = new JsonObject { ["readOnly"] = options.AllowedToolsReadOnly };
            SetList(allowed, "toolNames", options.AllowedToolNames);
            args["allowedTools"] = allowed;
        }
        else
        {
            SetList(args, "allowedTools", options.AllowedToolNames);
        }

        Set(args, "authorization", options.Authorization);
        Set(args, "connectorId", options.ConnectorId);
        if (options.Headers != null)
        {
            var headers = new JsonObject();
            foreach (var header in options.Headers)
            {
                headers[header.Key] = header.Value;
            }

            args["headers"] = headers;
        }

        if (options.NeverRequireApprovalToolNames != null)
        {
            var never = new JsonObject();
            SetList(never, "toolNames", options.NeverRequireApprovalToolNames);
            args["requireApproval"] = new JsonObject { ["never"] = never };
        }
        else
        {
            Set(args, "requireApproval", options.RequireApproval);
        }

        Set(args, "serverDescription", options.ServerDescription);
        Set(args, "serverUrl", options.ServerUrl);
        return Create("openai.mcp", "mcp", args);
    }

    /// <summary>The local shell tool (<c>openai.local_shell</c>).</summary>
    public static LanguageModelTool LocalShell() => Create("openai.local_shell", "local_shell", new JsonObject());

    /// <summary>The apply patch tool (<c>openai.apply_patch</c>).</summary>
    public static LanguageModelTool ApplyPatch() => Create("openai.apply_patch", "apply_patch", new JsonObject());

    /// <summary>The computer tool (<c>openai.computer</c>).</summary>
    public static LanguageModelTool Computer() => Create("openai.computer", "computer", new JsonObject());

    /// <summary>The file search tool (<c>openai.file_search</c>).</summary>
    public static LanguageModelTool FileSearch(OpenAIFileSearchToolOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var args = new JsonObject();
        SetList(args, "vectorStoreIds", options.VectorStoreIds);
        if (options.MaxNumResults is { } max)
        {
            args["maxNumResults"] = max;
        }

        if (options.Ranker != null || options.ScoreThreshold != null)
        {
            var ranking = new JsonObject();
            Set(ranking, "ranker", options.Ranker);
            if (options.ScoreThreshold is { } threshold)
            {
                ranking["scoreThreshold"] = threshold;
            }

            args["ranking"] = ranking;
        }

        if (options.Filters != null)
        {
            args["filters"] = FilterToJson(options.Filters);
        }

        return Create("openai.file_search", "file_search", args);
    }

    /// <summary>The code interpreter tool (<c>openai.code_interpreter</c>).</summary>
    public static LanguageModelTool CodeInterpreter(OpenAICodeInterpreterToolOptions? options = null)
    {
        var args = new JsonObject();
        if (options?.ContainerId != null)
        {
            args["container"] = options.ContainerId;
        }
        else if (options?.FileIds != null)
        {
            var container = new JsonObject();
            SetList(container, "fileIds", options.FileIds);
            args["container"] = container;
        }

        return Create("openai.code_interpreter", "code_interpreter", args);
    }

    /// <summary>The shell tool (<c>openai.shell</c>).</summary>
    public static LanguageModelTool Shell(OpenAIShellToolOptions? options = null)
    {
        var args = new JsonObject();
        if (options?.Environment is { } environment)
        {
            var mapped = new JsonObject { ["type"] = environment.Type };
            Set(mapped, "containerId", environment.ContainerId);
            SetList(mapped, "fileIds", environment.FileIds);
            Set(mapped, "memoryLimit", environment.MemoryLimit);
            if (environment.NetworkDisabled)
            {
                mapped["networkPolicy"] = new JsonObject { ["type"] = "disabled" };
            }
            else if (environment.AllowedDomains != null)
            {
                var policy = new JsonObject { ["type"] = "allowlist" };
                SetList(policy, "allowedDomains", environment.AllowedDomains);
                if (environment.DomainSecrets != null)
                {
                    var secrets = new JsonArray();
                    foreach (var secret in environment.DomainSecrets)
                    {
                        secrets.Add(new JsonObject { ["domain"] = secret.Domain, ["name"] = secret.Name, ["value"] = secret.Value });
                    }

                    policy["domainSecrets"] = secrets;
                }

                mapped["networkPolicy"] = policy;
            }

            if (environment.Skills != null)
            {
                var skills = new JsonArray();
                foreach (var skill in environment.Skills)
                {
                    skills.Add(new JsonObject { ["name"] = skill.Name, ["description"] = skill.Description, ["path"] = skill.Path });
                }

                mapped["skills"] = skills;
            }

            args["environment"] = mapped;
        }

        return Create("openai.shell", "shell", args);
    }

    /// <summary>A custom tool (<c>openai.custom</c>) with the name given in <paramref name="options"/>.</summary>
    public static LanguageModelTool CustomTool(OpenAICustomToolOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var args = new JsonObject();
        Set(args, "description", options.Description);
        if (options.GrammarSyntax != null)
        {
            args["format"] = new JsonObject
            {
                ["type"] = "grammar",
                ["syntax"] = options.GrammarSyntax,
                ["definition"] = options.GrammarDefinition,
            };
        }

        return Create("openai.custom", options.Name, args);
    }

    /// <summary>The tool search tool (<c>openai.tool_search</c>).</summary>
    public static LanguageModelTool ToolSearch(OpenAIToolSearchToolOptions? options = null)
    {
        var args = new JsonObject();
        Set(args, "execution", options?.Execution);
        Set(args, "description", options?.Description);
        if (options?.Parameters != null)
        {
            args["parameters"] = options.Parameters.DeepClone();
        }

        return Create("openai.tool_search", "tool_search", args);
    }

    /// <summary>The web search preview tool (<c>openai.web_search_preview</c>).</summary>
    public static LanguageModelTool WebSearchPreview(OpenAIWebSearchPreviewToolOptions? options = null)
    {
        var args = new JsonObject();
        Set(args, "searchContextSize", options?.SearchContextSize);
        if (options?.UserLocation is { } location)
        {
            var userLocation = new JsonObject { ["type"] = "approximate" };
            Set(userLocation, "country", location.Country);
            Set(userLocation, "city", location.City);
            Set(userLocation, "region", location.Region);
            Set(userLocation, "timezone", location.Timezone);
            args["userLocation"] = userLocation;
        }

        return Create("openai.web_search_preview", "web_search_preview", args);
    }

    /// <summary>The programmatic tool calling tool (<c>openai.programmatic_tool_calling</c>).</summary>
    public static LanguageModelTool ProgrammaticToolCalling() =>
        Create("openai.programmatic_tool_calling", "programmatic_tool_calling", new JsonObject());

    private static JsonObject FilterToJson(OpenAIFileSearchFilter filter)
    {
        var result = new JsonObject { ["type"] = filter.Type };
        if (filter.Filters != null)
        {
            var nested = new JsonArray();
            foreach (var child in filter.Filters)
            {
                nested.Add(FilterToJson(child));
            }

            result["filters"] = nested;
            return result;
        }

        Set(result, "key", filter.Key);
        if (filter.StringValues != null)
        {
            SetList(result, "value", filter.StringValues);
        }
        else if (filter.StringValue != null)
        {
            result["value"] = filter.StringValue;
        }
        else if (filter.NumberValue is { } number)
        {
            result["value"] = number;
        }
        else if (filter.BooleanValue is { } boolean)
        {
            result["value"] = boolean;
        }

        return result;
    }

    private static LanguageModelTool Create(string id, string name, JsonObject args)
    {
        using var document = JsonDocument.Parse(args.ToJsonString());
        return LanguageModelTool.ProviderDefined(id, name, document.RootElement.Clone());
    }

    private static void Set(JsonObject target, string name, string? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    private static void SetList(JsonObject target, string name, IReadOnlyList<string>? values)
    {
        if (values == null)
        {
            return;
        }

        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        target[name] = array;
    }
}
