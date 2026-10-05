// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests;

/// <summary>
/// Links a .NET test to one upstream vitest case from
/// <c>tests/parity/upstream-unit-tests.jsonl</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class UpstreamTestAttribute : Attribute
{
    public UpstreamTestAttribute(string upstreamId)
    {
        UpstreamId = upstreamId;
    }

    public string UpstreamId { get; }

    public UpstreamCoverage Coverage { get; set; } = UpstreamCoverage.Partial;

    public string? Note { get; set; }
}

public enum UpstreamCoverage
{
    Covered,
    Partial,
}
