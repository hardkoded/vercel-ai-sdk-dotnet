// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ScaleDown;

/// <summary>Provider options of a ScaleDown extraction model, under <c>providerOptions.scaledown</c>. Maps to <c>ScaleDownExtractionModelOptions</c>.</summary>
public sealed class ScaleDownExtractionModelOptions
{
    /// <summary>Minimum confidence for returned matches, from 0 to 1. Defaults to 0.</summary>
    public double? Threshold { get; init; }
}

/// <summary>Known extraction model ids. Maps to <c>ScaleDownExtractionModelId</c>. Any other string is accepted.</summary>
public static class ScaleDownExtractionModelId
{
    /// <summary>The latest extraction model.</summary>
    public const string Extract = "extract";

    /// <summary>Version 1 of the extraction model.</summary>
    public const string Extract1 = "extract-1";
}
