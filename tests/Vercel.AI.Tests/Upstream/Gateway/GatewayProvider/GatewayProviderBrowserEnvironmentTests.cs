// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>
/// .NET-only tests of the default <c>GatewayProvider.IsBrowserEnvironment</c> predicate, which maps upstream's
/// <c>typeof globalThis.window</c> check to the Blazor WebAssembly runtime. No upstream <c>it</c> covers it.
/// </summary>
public sealed class GatewayProviderBrowserEnvironmentTests
{
    [Fact]
    public void Default_predicate_reports_no_browser_outside_WebAssembly()
    {
        var gateway = GatewayProvider.Create(new GatewayOptions { ApiKey = "vck_test-token" });

        Assert.False(gateway.IsBrowserEnvironment());
        Assert.False(GatewayProvider.DetectBrowserEnvironment());
    }
}
