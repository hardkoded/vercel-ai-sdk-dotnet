// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.GenerateText.ToolCallConcurrency;

/// <summary>Stands in for vitest <c>vi.waitFor</c>.</summary>
internal static class ToolCallConcurrencySupport
{
    internal static async Task WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException("The condition was not met in time.");
            }

            await Task.Delay(5);
        }
    }

    internal static async Task WaitForEvents(System.Collections.Concurrent.ConcurrentQueue<string> events, params string[] expected)
    {
        await WaitFor(() => events.ToArray().SequenceEqual(expected));
    }

    internal static async Task WaitForStarted(System.Collections.Concurrent.ConcurrentQueue<int> started, params int[] expected)
    {
        await WaitFor(() => started.ToArray().SequenceEqual(expected));
    }
}
