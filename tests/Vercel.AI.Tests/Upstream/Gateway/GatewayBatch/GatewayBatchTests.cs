// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayBatch.GatewayBatchSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayBatch;

/// <summary>Port of <c>gateway-batch.test.ts</c> &gt; <c>GatewayBatch</c> (the tests outside any nested <c>describe</c>).</summary>
public sealed class GatewayBatchTests
{
    [Fact]
    [UpstreamTest(
        "packages/gateway/src/gateway-batch.test.ts::GatewayBatch::should expose the three batch methods as functions (batch capability duck-type)",
        Coverage = UpstreamCoverage.Covered,
        Note = "C# has no duck-typed capability check. The batch API is an IBatchApi, so the test binds the three methods and checks that the API can cancel."
    )]
    public void Should_expose_the_three_batch_methods_as_functions()
    {
        IBatchApi model = CreateTestBatch(new GatewayLanguageModelServer());

        Func<BatchOperationCall, CancellationToken, Task<BatchStartResult>> start =
            model.DoStartAsync;
        Func<BatchOperationCall, CancellationToken, Task<BatchStatus>> status =
            model.DoGetStatusAsync;
        Func<BatchOperationCall, CancellationToken, Task<IAsyncEnumerable<BatchItem>>> results =
            model.DoGetResultsAsync;

        Assert.NotNull(start);
        Assert.NotNull(status);
        Assert.NotNull(results);
        Assert.True(model.CanCancel);
    }
}
