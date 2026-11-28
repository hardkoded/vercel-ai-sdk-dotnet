// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIProvider;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class MoonshotAIEnvironmentCollection
{
    public const string Name = "MoonshotAIEnvironment";
}
