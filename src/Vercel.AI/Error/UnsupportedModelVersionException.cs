// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>A model implements a specification version this SDK does not support.</summary>
public sealed class UnsupportedModelVersionException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_UnsupportedModelVersionError";

    /// <summary>Creates the exception.</summary>
    public UnsupportedModelVersionException(string version, string provider, string modelId)
        : base(
            ErrorName,
            "Unsupported model version " + version + " for provider \"" + provider + "\" and model \"" + modelId + "\". "
                + "AI SDK 5 only supports models that implement specification version \"v2\".",
            null)
    {
        Version = version;
        Provider = provider;
        ModelId = modelId;
    }

    /// <summary>Specification version the model reported.</summary>
    public string Version { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }
}
