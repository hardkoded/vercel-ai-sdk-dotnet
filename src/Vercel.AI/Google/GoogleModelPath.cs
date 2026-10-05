// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Google;

/// <summary>Builds the resource path Gemini expects for a model id.</summary>
public static class GoogleModelPath
{
    /// <summary>
    /// Returns <paramref name="modelId"/> when it already contains a slash, such as <c>models/x</c> or <c>tunedModels/x</c>.
    /// Otherwise prefixes <c>models/</c>.
    /// </summary>
    public static string Get(string modelId)
    {
        if (string.IsNullOrEmpty(modelId))
        {
            return "models/";
        }

        return modelId.IndexOf('/') >= 0 ? modelId : "models/" + modelId;
    }
}
