// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests;

/// <summary>Keeps tests that install a warning recorder or observe the static <c>LogWarnings</c> state from running in parallel with other tests.</summary>
[CollectionDefinition("LogWarnings", DisableParallelization = true)]
public sealed class LogWarningsCollection { }
