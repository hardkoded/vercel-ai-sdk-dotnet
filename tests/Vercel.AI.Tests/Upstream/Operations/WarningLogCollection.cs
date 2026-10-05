// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests;

/// <summary>Keeps tests that set the static <c>WarningLog.Observer</c> from racing each other.</summary>
[CollectionDefinition("WarningLog", DisableParallelization = true)]
public sealed class WarningLogCollection
{
}
