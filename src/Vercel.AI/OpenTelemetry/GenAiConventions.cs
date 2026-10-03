// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.OpenTelemetry;

/// <summary>GenAI semantic-convention names shared by spans and the UI message protocol.</summary>
public static class GenAiConventions
{
    private static readonly (string Prefix, string Name)[] ProviderPrefixes =
    {
        ("google.vertex", "gcp.vertex_ai"),
        ("google.generative-ai", "gcp.gemini"),
        ("google-vertex", "gcp.vertex_ai"),
        ("amazon-bedrock", "aws.bedrock"),
        ("azure-openai", "azure.ai.openai"),
        ("anthropic", "anthropic"),
        ("openai", "openai"),
        ("azure", "azure.ai.inference"),
        ("google", "gcp.gemini"),
        ("mistral", "mistral_ai"),
        ("cohere", "cohere"),
        ("bedrock", "aws.bedrock"),
        ("groq", "groq"),
        ("deepseek", "deepseek"),
        ("perplexity", "perplexity"),
        ("xai", "x_ai"),
    };

    /// <summary>
    /// Maps a provider id such as <c>openai.chat</c> to the GenAI well-known provider name.
    /// An unrecognized id is returned unchanged.
    /// </summary>
    /// <param name="provider">Provider id from the model.</param>
    public static string MapProviderName(string provider)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        var lower = provider.ToLowerInvariant();
        foreach (var (prefix, name) in ProviderPrefixes)
        {
            if (lower == prefix
                || lower.StartsWith(prefix + ".", StringComparison.Ordinal)
                || lower.StartsWith(prefix + "-", StringComparison.Ordinal))
            {
                return name;
            }
        }

        return provider;
    }

    /// <summary>
    /// Maps an AI SDK operation id such as <c>ai.generateText</c> to <c>gen_ai.operation.name</c>.
    /// An unrecognized id is returned unchanged, which keeps <c>generateText</c> and <c>streamText</c>.
    /// </summary>
    /// <param name="operationId">Operation id passed to telemetry.</param>
    public static string MapOperationName(string operationId)
    {
        if (operationId is null)
        {
            throw new ArgumentNullException(nameof(operationId));
        }

        return operationId switch
        {
            "ai.generateText" => "invoke_agent",
            "ai.streamText" => "invoke_agent",
            "ai.generateObject" => "invoke_agent",
            "ai.streamObject" => "invoke_agent",
            "ai.embed" => "embeddings",
            "ai.embedMany" => "embeddings",
            "ai.rerank" => "rerank",
            _ => operationId,
        };
    }

    /// <summary>Protocol finish reason used by UI message <c>finish</c> chunks and <c>gen_ai.response.finish_reasons</c>.</summary>
    /// <param name="finishReason">Normalized finish reason.</param>
    public static string FormatFinishReason(FinishReason finishReason)
    {
        return finishReason switch
        {
            FinishReason.Stop => "stop",
            FinishReason.Length => "length",
            FinishReason.ContentFilter => "content-filter",
            FinishReason.ToolCalls => "tool-calls",
            FinishReason.Error => "error",
            _ => "other",
        };
    }
}
