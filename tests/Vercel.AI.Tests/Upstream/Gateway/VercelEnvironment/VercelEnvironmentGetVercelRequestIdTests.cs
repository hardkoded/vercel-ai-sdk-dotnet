// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;

// The directory name would shadow the VercelEnvironment type in sibling tests, so the namespace gets a suffix.
namespace Vercel.AI.Tests.Upstream.Gateway.VercelEnvironmentTests;

/// <summary>Port of <c>vercel-environment.test.ts</c> &gt; <c>getVercelRequestId</c>.</summary>
public sealed class VercelEnvironmentGetVercelRequestIdTests
{
    [Fact]
    public void Should_get_request_ID_from_request_headers_when_available()
    {
        using var _ = VercelRequestContext.Use(
            new Dictionary<string, string?> { ["x-vercel-id"] = "req_1234567890abcdef" }
        );

        Assert.Equal(
            "req_1234567890abcdef",
            Vercel.AI.Gateway.VercelEnvironment.GetVercelRequestId()
        );
    }

    [Fact]
    public void Should_return_undefined_when_request_ID_header_is_not_available()
    {
        using var _ = VercelRequestContext.Use(new Dictionary<string, string?>());

        Assert.Null(Vercel.AI.Gateway.VercelEnvironment.GetVercelRequestId());
    }

    [Fact]
    public void Should_return_undefined_when_no_headers_are_available()
    {
        using var _ = VercelRequestContext.Use(null);

        Assert.Null(Vercel.AI.Gateway.VercelEnvironment.GetVercelRequestId());
    }

    [Fact]
    public void Should_handle_missing_request_context_gracefully()
    {
        Assert.Null(Vercel.AI.Gateway.VercelEnvironment.GetVercelRequestId());
    }

    [Fact]
    public void Should_handle_missing_get_method_in_request_context()
    {
        // The port's context is a headers dictionary, so a context without a `get` method is the same as no headers.
        using var _ = VercelRequestContext.Use(null);

        Assert.Null(Vercel.AI.Gateway.VercelEnvironment.GetVercelRequestId());
    }
}
