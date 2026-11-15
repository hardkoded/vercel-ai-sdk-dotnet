// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gladia;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.GladiaTranscriptionModel;

public sealed class PollingSettingsValidationTests
{
    private const string Prefix = "packages/gladia/src/gladia-transcription-model.test.ts::polling settings validation::";
    private const string Commit = "3ab5e75d790c3657574cdbff56a8e3f14ba62bd7";

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [UpstreamTest(
        Prefix + "rejects pollIntervalMs = %s",
        Coverage = UpstreamCoverage.Partial,
        Note = "The .NET setting is an int?, so 1.5, NaN and Infinity cannot be passed.",
        Commit = Commit)]
    public void Rejects_pollIntervalMs(int value)
    {
        Assert.Throws<InvalidArgumentError>(() =>
            GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollIntervalMs = value }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [UpstreamTest(
        Prefix + "rejects pollTimeoutMs = %s",
        Coverage = UpstreamCoverage.Partial,
        Note = "The .NET setting is an int?, so 1.5, NaN and Infinity cannot be passed.",
        Commit = Commit)]
    public void Rejects_pollTimeoutMs(int value)
    {
        Assert.Throws<InvalidArgumentError>(() =>
            GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollTimeoutMs = value }));
    }
}
