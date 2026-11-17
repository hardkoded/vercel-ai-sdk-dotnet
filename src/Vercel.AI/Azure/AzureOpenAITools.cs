// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Azure;

/// <summary>
/// Factories for the provider-defined tools Azure OpenAI Responses supports, the port of upstream <c>azureOpenaiTools</c>.
/// They are the OpenAI factories of <see cref="OpenAITools"/>: Azure sends the same tool definitions and reads the item
/// metadata under the <c>azure</c> provider key.
/// </summary>
public static class AzureOpenAITools
{
    /// <summary>The apply patch tool (<c>openai.apply_patch</c>).</summary>
    public static Tool ApplyPatch(OpenAIApplyPatchToolOptions? options = null) =>
        OpenAITools.ApplyPatch(options);

    /// <summary>The code interpreter tool (<c>openai.code_interpreter</c>).</summary>
    public static LanguageModelTool CodeInterpreter(
        OpenAICodeInterpreterToolOptions? options = null
    ) => OpenAITools.CodeInterpreter(options);

    /// <summary>A custom tool (<c>openai.custom</c>) with the name given in <paramref name="options"/>.</summary>
    public static LanguageModelTool CustomTool(OpenAICustomToolOptions options) =>
        OpenAITools.CustomTool(options);

    /// <summary>The file search tool (<c>openai.file_search</c>).</summary>
    public static LanguageModelTool FileSearch(OpenAIFileSearchToolOptions options) =>
        OpenAITools.FileSearch(options);

    /// <summary>The image generation tool (<c>openai.image_generation</c>).</summary>
    public static LanguageModelTool ImageGeneration(
        OpenAIImageGenerationToolOptions? options = null
    ) => OpenAITools.ImageGeneration(options);

    /// <summary>The shell tool (<c>openai.shell</c>).</summary>
    public static Tool Shell(OpenAIShellToolOptions? options = null) => OpenAITools.Shell(options);

    /// <summary>The tool search tool (<c>openai.tool_search</c>).</summary>
    public static LanguageModelTool ToolSearch(OpenAIToolSearchToolOptions? options = null) =>
        OpenAITools.ToolSearch(options);

    /// <summary>The web search tool (<c>openai.web_search</c>).</summary>
    public static LanguageModelTool WebSearch(OpenAIWebSearchToolOptions? options = null) =>
        OpenAITools.WebSearch(options);

    /// <summary>The web search preview tool (<c>openai.web_search_preview</c>).</summary>
    [Obsolete("Use WebSearch instead.")]
    public static LanguageModelTool WebSearchPreview(
        OpenAIWebSearchPreviewToolOptions? options = null
    ) => OpenAITools.WebSearchPreview(options);
}
