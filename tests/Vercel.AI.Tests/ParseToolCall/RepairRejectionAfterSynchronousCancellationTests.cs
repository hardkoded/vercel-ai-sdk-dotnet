// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.ParseToolCall.ParseToolCallFixture;

namespace Vercel.AI.Tests.ParseToolCall;

/// <summary>Port of <c>parse-tool-call.test.ts</c> &gt; <c>parseToolCall</c> &gt; <c>tool call repair</c> &gt; <c>repair rejection after synchronous cancellation</c>.</summary>
public sealed class RepairRejectionAfterSynchronousCancellationTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool call repair > repair rejection after synchronous cancellation::should observe the repair rejection",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldObserveTheRepairRejection()
    {
        var unhandledRejections = new List<Exception>();
        var repairError = new InvalidOperationException("repair failed");
        void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args)
        {
            if (args.Exception.Flatten().InnerExceptions.Contains(repairError))
            {
                lock (unhandledRejections)
                {
                    unhandledRejections.Add(args.Exception);
                }
            }
        }

        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        try
        {
            using var abortController = new CancellationTokenSource();

            var result = ParseAsync(
                new GeneratedToolCall("123", "testTool", "invalid json"),
                new[] { TestTool(ParamsSchema) },
                repairToolCall: async _ =>
                {
                    abortController.Cancel();
                    await Task.Yield();
                    throw repairError;
                },
                cancellationToken: abortController.Token);

            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => result);
            Assert.Equal(abortController.Token, exception.CancellationToken);

            await Task.Delay(50);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Assert.Empty(unhandledRejections);
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
        }
    }
}
