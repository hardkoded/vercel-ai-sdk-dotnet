// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Sdk.OpenTelemetry;

namespace Vercel.AI.Sdk.Tests;

public sealed class TelemetryTests
{
    [Fact]
    public void Begin_returns_a_disposable_span()
    {
        var telemetry = new OpenTelemetryAiTelemetry();
        using var span = telemetry.Begin("generateText", "test-model");
        Assert.NotNull(span);
    }
}
