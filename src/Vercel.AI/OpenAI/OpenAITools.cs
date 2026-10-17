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
