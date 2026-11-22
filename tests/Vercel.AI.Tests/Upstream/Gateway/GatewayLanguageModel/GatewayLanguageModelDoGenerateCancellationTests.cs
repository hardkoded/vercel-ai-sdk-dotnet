// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; doGenerate cancellation</c>.</summary>
public sealed class GatewayLanguageModelDoGenerateCancellationTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate cancellation::";

    [Theory]
    [InlineData("AbortError")]
    [InlineData("TimeoutError")]
    [UpstreamTest(
        Prefix + "should preserve the original %s from fetch",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f35668be8bd5fffafb1461f34c5d4b1ca9a43beb",
        Note = "A cancelled call surfaces as an ApiUserAbortException whose cause is the original AbortError, as a .NET cancel does. A TimeoutError is not a cancellation exception, so it is rethrown as is."
    )]
    public async Task Should_preserve_the_original_error_from_fetch(string name)
    {
        Exception error =
            name == "AbortError"
                ? new OperationCanceledException("Request cancelled")
                : new TimeoutError("Request cancelled");
        using var source = new CancellationTokenSource();
        source.Cancel();
        var model = CreateTestModel((_, _) => Task.FromException<HttpResponseMessage>(error));

        var thrown = await Assert.ThrowsAnyAsync<Exception>(() =>
            model.DoGenerateAsync(TestPrompt, source.Token)
        );

        if (name == "AbortError")
        {
            Assert.IsType<Vercel.AI.Provider.ApiUserAbortException>(thrown);
            Assert.Same(error, thrown.InnerException);
        }
        else
        {
            Assert.Same(error, thrown);
        }
    }
}
