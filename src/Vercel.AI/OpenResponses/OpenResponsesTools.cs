// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenResponses;

/// <summary>The raw text format of a custom tool. Maps to the <c>format</c> member of <c>OpenResponsesCustomToolOptions</c>.</summary>
public sealed class OpenResponsesCustomToolFormat
{
    /// <summary>The format type: <c>grammar</c> or <c>text</c>.</summary>
    public string Type { get; set; } = "text";

    /// <summary>Grammar syntax: <c>regex</c> or <c>lark</c>. Used with the <c>grammar</c> type.</summary>
    public string? Syntax { get; set; }

    /// <summary>Grammar definition. Used with the <c>grammar</c> type.</summary>
    public string? Definition { get; set; }
}

/// <summary>Options of <see cref="OpenResponsesTools.CustomTool"/>. Maps to <c>OpenResponsesCustomToolOptions</c>.</summary>
public sealed class OpenResponsesCustomToolOptions
{
    /// <summary>The tool name.</summary>
    public string Name { get; set; } = "";

    /// <summary>An optional description of what the custom tool does.</summary>
    public string? Description { get; set; }

    /// <summary>The raw text format returned by the model.</summary>
    public OpenResponsesCustomToolFormat? Format { get; set; }

    /// <summary>Runs a call with the raw string input and returns its output. Maps to <c>execute</c>.</summary>
    public Func<string, ToolExecutionOptions, Task<string>>? Execute { get; set; }

    /// <summary>When true, a call needs approval before it runs. Maps to <c>needsApproval: true</c>.</summary>
    public bool NeedsApproval { get; set; }

    /// <summary>Decides for each call whether it needs approval. Used instead of <see cref="NeedsApproval"/>.</summary>
    public Func<string, ToolNeedsApprovalOptions, Task<bool>>? NeedsApprovalFunction { get; set; }
}

/// <summary>The provider tools of an Open Responses provider. Maps to <c>createOpenResponsesTools</c>.</summary>
public sealed class OpenResponsesTools
{
    private readonly string _customToolId;

    /// <summary>Creates the tools for a custom tool ID, which has the shape <c>&lt;provider&gt;.&lt;tool&gt;</c>.</summary>
    public OpenResponsesTools(string customToolId)
    {
        _customToolId = customToolId ?? throw new ArgumentNullException(nameof(customToolId));
    }

    /// <summary>
    /// Creates a caller-executed OpenAI-compatible custom tool. Custom tools accept a raw string, optionally constrained by a
    /// grammar. Requires endpoint support.
    /// </summary>
    public Tool CustomTool(OpenResponsesCustomToolOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var args = new JsonObject();
        if (options.Description != null)
        {
            args["description"] = options.Description;
        }

        if (options.Format != null)
        {
            var format = new JsonObject { ["type"] = options.Format.Type };
            if (options.Format.Syntax != null)
            {
                format["syntax"] = options.Format.Syntax;
            }

            if (options.Format.Definition != null)
            {
                format["definition"] = options.Format.Definition;
            }

            args["format"] = format;
        }

        var tool = Tool.ProviderDefined(
            _customToolId,
            options.Name,
            JsonSerializer.SerializeToElement(args),
            OpenAIClientTools.ParseSchema("{\"type\":\"string\"}")
        );
        return OpenAIClientTools.Build(
            tool,
            options.Execute,
            options.NeedsApproval,
            options.NeedsApprovalFunction
        );
    }
}
