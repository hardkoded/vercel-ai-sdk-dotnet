// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Scaledown;

/// <summary>Serializes ScaleDown tests that mutate the <c>SCALEDOWN_API_KEY</c> environment variable.</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ScaleDownEnvironmentCollection
{
    /// <summary>Collection name.</summary>
    public const string Name = "ScaleDownEnvironment";
}
