// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway;

/// <summary>Known AI Gateway model ids. Maps to <c>GatewayRerankingModelId</c>; any other string is accepted too.</summary>
public static class GatewayRerankingModelId
{
    /// <summary><c>cohere/rerank-v3.5</c>.</summary>
    public const string CohereRerankV35 = "cohere/rerank-v3.5";

    /// <summary><c>cohere/rerank-v4-fast</c>.</summary>
    public const string CohereRerankV4Fast = "cohere/rerank-v4-fast";

    /// <summary><c>cohere/rerank-v4-pro</c>.</summary>
    public const string CohereRerankV4Pro = "cohere/rerank-v4-pro";

    /// <summary><c>cohere/rerank-v5-fast</c>.</summary>
    public const string CohereRerankV5Fast = "cohere/rerank-v5-fast";

    /// <summary><c>cohere/rerank-v5-pro</c>.</summary>
    public const string CohereRerankV5Pro = "cohere/rerank-v5-pro";

    /// <summary><c>voyage/rerank-2.5</c>.</summary>
    public const string VoyageRerank25 = "voyage/rerank-2.5";

    /// <summary><c>voyage/rerank-2.5-lite</c>.</summary>
    public const string VoyageRerank25Lite = "voyage/rerank-2.5-lite";

    /// <summary><c>voyage/rerank-3</c>.</summary>
    public const string VoyageRerank3 = "voyage/rerank-3";

    /// <summary><c>voyage/rerank-3-lite</c>.</summary>
    public const string VoyageRerank3Lite = "voyage/rerank-3-lite";
}
