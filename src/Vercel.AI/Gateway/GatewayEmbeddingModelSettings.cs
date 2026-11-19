// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway;

/// <summary>Known AI Gateway model ids. Maps to <c>GatewayEmbeddingModelId</c>; any other string is accepted too.</summary>
public static class GatewayEmbeddingModelId
{
    /// <summary><c>alibaba/qwen3-embedding-0.6b</c>.</summary>
    public const string AlibabaQwen3Embedding06b = "alibaba/qwen3-embedding-0.6b";

    /// <summary><c>alibaba/qwen3-embedding-4b</c>.</summary>
    public const string AlibabaQwen3Embedding4b = "alibaba/qwen3-embedding-4b";

    /// <summary><c>alibaba/qwen3-embedding-8b</c>.</summary>
    public const string AlibabaQwen3Embedding8b = "alibaba/qwen3-embedding-8b";

    /// <summary><c>amazon/titan-embed-text-v2</c>.</summary>
    public const string AmazonTitanEmbedTextV2 = "amazon/titan-embed-text-v2";

    /// <summary><c>cohere/embed-v4.0</c>.</summary>
    public const string CohereEmbedV40 = "cohere/embed-v4.0";

    /// <summary><c>cohere/embed-v5.0-fast</c>.</summary>
    public const string CohereEmbedV50Fast = "cohere/embed-v5.0-fast";

    /// <summary><c>cohere/embed-v5.0-pro</c>.</summary>
    public const string CohereEmbedV50Pro = "cohere/embed-v5.0-pro";

    /// <summary><c>google/gemini-embedding-001</c>.</summary>
    public const string GoogleGeminiEmbedding001 = "google/gemini-embedding-001";

    /// <summary><c>google/gemini-embedding-2</c>.</summary>
    public const string GoogleGeminiEmbedding2 = "google/gemini-embedding-2";

    /// <summary><c>google/text-embedding-005</c>.</summary>
    public const string GoogleTextEmbedding005 = "google/text-embedding-005";

    /// <summary><c>google/text-multilingual-embedding-002</c>.</summary>
    public const string GoogleTextMultilingualEmbedding002 =
        "google/text-multilingual-embedding-002";

    /// <summary><c>mistral/codestral-embed</c>.</summary>
    public const string MistralCodestralEmbed = "mistral/codestral-embed";

    /// <summary><c>mistral/mistral-embed</c>.</summary>
    public const string MistralMistralEmbed = "mistral/mistral-embed";

    /// <summary><c>openai/text-embedding-3-large</c>.</summary>
    public const string OpenaiTextEmbedding3Large = "openai/text-embedding-3-large";

    /// <summary><c>openai/text-embedding-3-small</c>.</summary>
    public const string OpenaiTextEmbedding3Small = "openai/text-embedding-3-small";

    /// <summary><c>openai/text-embedding-ada-002</c>.</summary>
    public const string OpenaiTextEmbeddingAda002 = "openai/text-embedding-ada-002";

    /// <summary><c>perplexity/pplx-embed-v1-0.6b</c>.</summary>
    public const string PerplexityPplxEmbedV106b = "perplexity/pplx-embed-v1-0.6b";

    /// <summary><c>perplexity/pplx-embed-v1-4b</c>.</summary>
    public const string PerplexityPplxEmbedV14b = "perplexity/pplx-embed-v1-4b";

    /// <summary><c>voyage/voyage-3-large</c>.</summary>
    public const string VoyageVoyage3Large = "voyage/voyage-3-large";

    /// <summary><c>voyage/voyage-3.5</c>.</summary>
    public const string VoyageVoyage35 = "voyage/voyage-3.5";

    /// <summary><c>voyage/voyage-3.5-lite</c>.</summary>
    public const string VoyageVoyage35Lite = "voyage/voyage-3.5-lite";

    /// <summary><c>voyage/voyage-4</c>.</summary>
    public const string VoyageVoyage4 = "voyage/voyage-4";

    /// <summary><c>voyage/voyage-4-large</c>.</summary>
    public const string VoyageVoyage4Large = "voyage/voyage-4-large";

    /// <summary><c>voyage/voyage-4-lite</c>.</summary>
    public const string VoyageVoyage4Lite = "voyage/voyage-4-lite";

    /// <summary><c>voyage/voyage-code-2</c>.</summary>
    public const string VoyageVoyageCode2 = "voyage/voyage-code-2";

    /// <summary><c>voyage/voyage-code-3</c>.</summary>
    public const string VoyageVoyageCode3 = "voyage/voyage-code-3";

    /// <summary><c>voyage/voyage-finance-2</c>.</summary>
    public const string VoyageVoyageFinance2 = "voyage/voyage-finance-2";

    /// <summary><c>voyage/voyage-law-2</c>.</summary>
    public const string VoyageVoyageLaw2 = "voyage/voyage-law-2";
}
