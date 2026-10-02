// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Google;

/// <summary>Gemini model resource paths.</summary>
public static class GoogleModelPath
{
    /// <summary>
    /// Returns <paramref name="modelId"/> when it already contains a slash, otherwise <c>models/{id}</c>.
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
