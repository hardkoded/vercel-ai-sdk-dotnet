// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.OpenAICompatible;

/// <summary>Extracts provider-specific metadata from a chat response. Maps to <c>MetadataExtractor</c>.</summary>
public abstract class OpenAICompatibleMetadataExtractor
{
    /// <summary>
    /// Extracts provider metadata from a complete response body. Returns an object keyed by provider name, or null.
    /// </summary>
    public abstract JsonObject? ExtractMetadata(JsonElement parsedBody);

    /// <summary>Creates an extractor for one stream. It sees every chunk, then builds the metadata at the end.</summary>
    public abstract OpenAICompatibleStreamMetadataExtractor CreateStreamExtractor();
}

/// <summary>Accumulates provider-specific metadata from stream chunks.</summary>
public abstract class OpenAICompatibleStreamMetadataExtractor
{
    /// <summary>Processes one parsed stream chunk.</summary>
    public abstract void ProcessChunk(JsonElement parsedChunk);

    /// <summary>Builds the provider metadata. Returns an object keyed by provider name, or null.</summary>
    public abstract JsonObject? BuildMetadata();
}
