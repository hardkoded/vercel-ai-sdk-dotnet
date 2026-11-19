// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Operations;

/// <summary>
/// A provider that returns extraction models. <see cref="ProviderRegistry"/> and <see cref="CustomProvider"/> resolve
/// extraction models through it. Maps to <c>ExtractionProvider</c>, the optional <c>extractionModel</c> factory of a provider.
/// </summary>
public interface IExtractionProvider
{
    /// <summary>Returns an extraction model, or null when the provider has no such model.</summary>
    IExtractionCaller? ExtractionModel(string modelId);
}
