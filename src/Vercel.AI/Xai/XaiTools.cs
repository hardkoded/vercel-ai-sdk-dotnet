// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Xai;

/// <summary>Restricts what the <c>xai.image_generation</c> tool can do. Maps to the <c>action</c> argument.</summary>
public enum XaiImageGenerationAction
{
    /// <summary>The model can generate and edit images.</summary>
    Auto,

    /// <summary>Text-to-image generation only.</summary>
    Generate,

    /// <summary>Image editing only.</summary>
    Edit,
}

/// <summary>Arguments of <see cref="XaiTools.WebSearch"/>. Maps to <c>webSearchArgsSchema</c>.</summary>
public sealed class XaiWebSearchOptions
{
    /// <summary>Domains to search. At most 5.</summary>
    public IReadOnlyList<string>? AllowedDomains { get; init; }

    /// <summary>Domains to leave out. At most 5.</summary>
    public IReadOnlyList<string>? ExcludedDomains { get; init; }

    /// <summary>Whether the tool searches for images.</summary>
    public bool? EnableImageSearch { get; init; }

    /// <summary>Whether the tool understands the images it finds.</summary>
    public bool? EnableImageUnderstanding { get; init; }
}

/// <summary>Arguments of <see cref="XaiTools.XSearch"/>. Maps to <c>xSearchArgsSchema</c>.</summary>
public sealed class XaiXSearchOptions
{
    /// <summary>X handles to search. At most 20.</summary>
    public IReadOnlyList<string>? AllowedXHandles { get; init; }

    /// <summary>X handles to leave out. At most 20.</summary>
    public IReadOnlyList<string>? ExcludedXHandles { get; init; }

    /// <summary>Start of the date range.</summary>
    public string? FromDate { get; init; }

    /// <summary>End of the date range.</summary>
    public string? ToDate { get; init; }

    /// <summary>Whether the tool understands images in posts.</summary>
    public bool? EnableImageUnderstanding { get; init; }

    /// <summary>Whether the tool understands videos in posts.</summary>
    public bool? EnableVideoUnderstanding { get; init; }
}

/// <summary>Arguments of <see cref="XaiTools.ImageGeneration"/>. Maps to <c>imageGenerationArgsSchema</c>.</summary>
public sealed class XaiImageGenerationOptions
{
    /// <summary>What the tool can do. xAI uses <see cref="XaiImageGenerationAction.Auto"/> when this is null.</summary>
    public XaiImageGenerationAction? Action { get; init; }
}

/// <summary>Arguments of <see cref="XaiTools.FileSearch"/>. Maps to <c>fileSearchArgsSchema</c>.</summary>
public sealed class XaiFileSearchOptions
{
    /// <summary>Creates the arguments.</summary>
    public XaiFileSearchOptions(IReadOnlyList<string> vectorStoreIds)
    {
        VectorStoreIds = vectorStoreIds ?? throw new ArgumentNullException(nameof(vectorStoreIds));
    }

    /// <summary>Vector store (collection) ids to search.</summary>
    public IReadOnlyList<string> VectorStoreIds { get; }

    /// <summary>Maximum number of results to return.</summary>
    public double? MaxNumResults { get; init; }
}

/// <summary>Arguments of <see cref="XaiTools.McpServer"/>. Maps to <c>mcpServerArgsSchema</c>.</summary>
public sealed class XaiMcpServerOptions
{
    /// <summary>Creates the arguments.</summary>
    public XaiMcpServerOptions(string serverUrl)
    {
        ServerUrl = serverUrl ?? throw new ArgumentNullException(nameof(serverUrl));
    }

    /// <summary>The URL of the MCP server.</summary>
    public string ServerUrl { get; }

    /// <summary>A label for the MCP server.</summary>
    public string? ServerLabel { get; init; }

    /// <summary>A description of the MCP server.</summary>
    public string? ServerDescription { get; init; }

    /// <summary>Names of the tools the model may call.</summary>
    public IReadOnlyList<string>? AllowedTools { get; init; }

    /// <summary>Custom headers to send.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; init; }

    /// <summary>The authorization header value.</summary>
    public string? Authorization { get; init; }
}

/// <summary>
/// Factories for the xAI provider-defined tools. Each returns a <see cref="Tool"/> that xAI runs. Maps to <c>xai.tools</c>.
/// The <c>name</c> is the name the model and the results use. It defaults to the tool's wire name.
/// </summary>
public static class XaiTools
{
    /// <summary>The <c>xai.code_execution</c> tool. Maps to <c>codeExecution</c>.</summary>
    public static Tool CodeExecution(string name = "code_execution")
    {
        return Tool.ProviderDefined("xai.code_execution", name, Args());
    }

    /// <summary>The <c>xai.file_search</c> tool. Maps to <c>fileSearch</c>.</summary>
    public static Tool FileSearch(XaiFileSearchOptions options, string name = "file_search")
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return Tool.ProviderDefined("xai.file_search", name, Args(
            ("vectorStoreIds", Strings(options.VectorStoreIds)),
            ("maxNumResults", options.MaxNumResults is { } max ? JsonValue.Create(max) : null)));
    }

    /// <summary>The <c>xai.image_generation</c> tool. Maps to <c>imageGeneration</c>.</summary>
    public static Tool ImageGeneration(XaiImageGenerationOptions? options = null, string name = "image_generation")
    {
        return Tool.ProviderDefined("xai.image_generation", name, Args(
            ("action", options?.Action switch
            {
                XaiImageGenerationAction.Auto => JsonValue.Create("auto"),
                XaiImageGenerationAction.Generate => JsonValue.Create("generate"),
                XaiImageGenerationAction.Edit => JsonValue.Create("edit"),
                _ => null,
            })));
    }

    /// <summary>The <c>xai.mcp</c> tool. Maps to <c>mcpServer</c>.</summary>
    public static Tool McpServer(XaiMcpServerOptions options, string name = "mcp")
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        JsonNode? headers = null;
        if (options.Headers != null)
        {
            var node = new JsonObject();
            foreach (var header in options.Headers)
            {
                node[header.Key] = header.Value;
            }

            headers = node;
        }

        return Tool.ProviderDefined("xai.mcp", name, Args(
            ("serverUrl", JsonValue.Create(options.ServerUrl)),
            ("serverLabel", Text(options.ServerLabel)),
            ("serverDescription", Text(options.ServerDescription)),
            ("allowedTools", Strings(options.AllowedTools)),
            ("headers", headers),
            ("authorization", Text(options.Authorization))));
    }

    /// <summary>The <c>xai.view_image</c> tool. Maps to <c>viewImage</c>.</summary>
    public static Tool ViewImage(string name = "view_image")
    {
        return Tool.ProviderDefined("xai.view_image", name, Args());
    }

    /// <summary>The <c>xai.view_x_video</c> tool. Maps to <c>viewXVideo</c>.</summary>
    public static Tool ViewXVideo(string name = "view_x_video")
    {
        return Tool.ProviderDefined("xai.view_x_video", name, Args());
    }

    /// <summary>The <c>xai.web_search</c> tool. Maps to <c>webSearch</c>.</summary>
    public static Tool WebSearch(XaiWebSearchOptions? options = null, string name = "web_search")
    {
        return Tool.ProviderDefined("xai.web_search", name, Args(
            ("allowedDomains", Strings(options?.AllowedDomains)),
            ("excludedDomains", Strings(options?.ExcludedDomains)),
            ("enableImageSearch", Flag(options?.EnableImageSearch)),
            ("enableImageUnderstanding", Flag(options?.EnableImageUnderstanding))));
    }

    /// <summary>The <c>xai.x_search</c> tool. Maps to <c>xSearch</c>.</summary>
    public static Tool XSearch(XaiXSearchOptions? options = null, string name = "x_search")
    {
        return Tool.ProviderDefined("xai.x_search", name, Args(
            ("allowedXHandles", Strings(options?.AllowedXHandles)),
            ("excludedXHandles", Strings(options?.ExcludedXHandles)),
            ("fromDate", Text(options?.FromDate)),
            ("toDate", Text(options?.ToDate)),
            ("enableImageUnderstanding", Flag(options?.EnableImageUnderstanding)),
            ("enableVideoUnderstanding", Flag(options?.EnableVideoUnderstanding))));
    }

    private static JsonNode? Text(string? value)
    {
        return value == null ? null : JsonValue.Create(value);
    }

    private static JsonNode? Flag(bool? value)
    {
        return value is { } flag ? JsonValue.Create(flag) : null;
    }

    private static JsonNode? Strings(IReadOnlyList<string>? values)
    {
        if (values == null)
        {
            return null;
        }

        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(JsonValue.Create(value));
        }

        return array;
    }

    /// <summary>The JSON object of the arguments that are set. The request validates it against the tool's schema.</summary>
    private static JsonElement Args(params (string Name, JsonNode? Value)[] fields)
    {
        var args = new JsonObject();
        foreach (var (name, value) in fields)
        {
            if (value != null)
            {
                args[name] = value;
            }
        }

        using var document = JsonDocument.Parse(args.ToJsonString());
        return document.RootElement.Clone();
    }
}
