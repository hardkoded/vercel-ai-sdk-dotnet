// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class GenAiConventionTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapProviderName::should map known providers to well-known values",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_known_providers_to_well_known_values()
    {
        Assert.Equal("anthropic", GenAiConventions.MapProviderName("anthropic.messages"));
        Assert.Equal("openai", GenAiConventions.MapProviderName("openai.chat"));
        Assert.Equal("gcp.gemini", GenAiConventions.MapProviderName("google.generative-ai"));
        Assert.Equal("mistral_ai", GenAiConventions.MapProviderName("mistral.chat"));
        Assert.Equal("groq", GenAiConventions.MapProviderName("groq.chat"));
        Assert.Equal("deepseek", GenAiConventions.MapProviderName("deepseek.chat"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapProviderName::should map google vertex provider strings",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_google_vertex_provider_strings()
    {
        Assert.Equal("gcp.vertex_ai", GenAiConventions.MapProviderName("google.vertex.chat"));
        Assert.Equal("gcp.vertex_ai", GenAiConventions.MapProviderName("google.vertex.embedding"));
        Assert.Equal("gcp.vertex_ai", GenAiConventions.MapProviderName("google.vertex.image"));
        Assert.Equal("gcp.vertex_ai", GenAiConventions.MapProviderName("google-vertex"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapProviderName::should map bare google prefix to gcp.gemini",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_a_bare_google_prefix_to_gcp_gemini()
    {
        Assert.Equal("gcp.gemini", GenAiConventions.MapProviderName("google.chat"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapProviderName::should map bedrock provider",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_bedrock_providers()
    {
        Assert.Equal("aws.bedrock", GenAiConventions.MapProviderName("amazon-bedrock.chat"));
        Assert.Equal("aws.bedrock", GenAiConventions.MapProviderName("bedrock.chat"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapProviderName::should map azure providers",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_azure_providers()
    {
        Assert.Equal("azure.ai.inference", GenAiConventions.MapProviderName("azure.chat"));
        Assert.Equal("azure.ai.openai", GenAiConventions.MapProviderName("azure-openai.chat"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapProviderName::should return the original string for unknown providers",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_the_original_string_for_an_unknown_provider()
    {
        Assert.Equal("custom-provider.chat", GenAiConventions.MapProviderName("custom-provider.chat"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapOperationName::should map generateText/streamText to invoke_agent",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_generate_text_and_stream_text_to_invoke_agent()
    {
        Assert.Equal("invoke_agent", GenAiConventions.MapOperationName("ai.generateText"));
        Assert.Equal("invoke_agent", GenAiConventions.MapOperationName("ai.streamText"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapOperationName::should map generateObject/streamObject to invoke_agent",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_object_generation_to_invoke_agent()
    {
        Assert.Equal("invoke_agent", GenAiConventions.MapOperationName("ai.generateObject"));
        Assert.Equal("invoke_agent", GenAiConventions.MapOperationName("ai.streamObject"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapOperationName::should map embed/embedMany to embeddings",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_embed_operations_to_embeddings()
    {
        Assert.Equal("embeddings", GenAiConventions.MapOperationName("ai.embed"));
        Assert.Equal("embeddings", GenAiConventions.MapOperationName("ai.embedMany"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapOperationName::should map rerank to rerank",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_rerank_to_rerank()
    {
        Assert.Equal("rerank", GenAiConventions.MapOperationName("ai.rerank"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapOperationName::should return the original string for unknown operations",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_the_original_string_for_an_unknown_operation()
    {
        Assert.Equal("ai.unknown", GenAiConventions.MapOperationName("ai.unknown"));
        Assert.Equal("generateText", GenAiConventions.MapOperationName("generateText"));
        Assert.Equal("streamText", GenAiConventions.MapOperationName("streamText"));
        Assert.Equal("generateSpeech", GenAiConventions.MapOperationName("generateSpeech"));
        Assert.Equal("transcribe", GenAiConventions.MapOperationName("transcribe"));
    }

    [Fact]
    public void Format_finish_reason_uses_protocol_values()
    {
        Assert.Equal("stop", GenAiConventions.FormatFinishReason(FinishReason.Stop));
        Assert.Equal("length", GenAiConventions.FormatFinishReason(FinishReason.Length));
        Assert.Equal("content-filter", GenAiConventions.FormatFinishReason(FinishReason.ContentFilter));
        Assert.Equal("tool-calls", GenAiConventions.FormatFinishReason(FinishReason.ToolCalls));
        Assert.Equal("error", GenAiConventions.FormatFinishReason(FinishReason.Error));
        Assert.Equal("other", GenAiConventions.FormatFinishReason(FinishReason.Other));
    }
}
