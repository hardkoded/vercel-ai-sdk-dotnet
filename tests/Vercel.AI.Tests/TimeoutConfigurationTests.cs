// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;

namespace Vercel.AI.Tests;

public sealed class TimeoutConfigurationTests
{
    [Fact]
    public void A_negative_TimeSpan_is_rejected_even_when_it_rounds_to_zero_milliseconds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => (TimeoutConfiguration)TimeSpan.FromTicks(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => (TimeoutConfiguration)TimeSpan.FromSeconds(-5));
    }

    [Fact]
    public void An_infinite_TimeSpan_means_no_timeout()
    {
        TimeoutConfiguration configuration = Timeout.InfiniteTimeSpan;

        Assert.Null(configuration.TotalMs);
    }

    [Fact]
    public void A_positive_TimeSpan_is_rounded_up_to_a_whole_millisecond()
    {
        TimeoutConfiguration configuration = TimeSpan.FromTicks(10_001);

        Assert.Equal(2, configuration.TotalMs);
    }
}
