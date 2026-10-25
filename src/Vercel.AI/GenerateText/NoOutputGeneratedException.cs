// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>No LLM output was generated, for example because a model stream ended without output. Maps to <c>NoOutputGeneratedError</c>.</summary>
public sealed class NoOutputGeneratedException : AiSdkException
{
    /// <summary>Creates the exception with the default message.</summary>
    public NoOutputGeneratedException()
        : base("No output generated.")
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    public NoOutputGeneratedException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and the underlying error.</summary>
    public NoOutputGeneratedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Stable error name.</summary>
    public string Name { get; } = "AI_NoOutputGeneratedError";
}
