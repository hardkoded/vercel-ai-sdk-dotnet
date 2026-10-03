// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class RuntimeAndReferenceTests
{
    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-browser-runtime.test.ts::isBrowserRuntime::returns true when a global window is present", Coverage = UpstreamCoverage.Covered)]
    public void Browser_runtime_is_true_when_window_exists()
    {
        Assert.True(ProviderValues.IsBrowserRuntime(new ProviderValues.RuntimeProbe { Window = true }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-browser-runtime.test.ts::isBrowserRuntime::returns false when there is no window (server runtimes)", Coverage = UpstreamCoverage.Covered)]
    public void Browser_runtime_is_false_without_a_window()
    {
        Assert.False(ProviderValues.IsBrowserRuntime(new ProviderValues.RuntimeProbe()));
        Assert.False(ProviderValues.IsBrowserRuntime(null));
        Assert.False(ProviderValues.IsBrowserRuntime(new ProviderValues.RuntimeProbe { NodeVersion = "22" }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/create-provider-stream-error.test.ts::createProviderStreamError::preserves provider-owned metadata and raw data", Coverage = UpstreamCoverage.Covered)]
    public void Provider_stream_error_keeps_the_payload()
    {
        var data = new Dictionary<string, object?>
        {
            ["message"] = "Overloaded",
            ["type"] = "overloaded_error",
            ["code"] = "provider_overloaded",
        };
        var error = new StreamProviderError("Overloaded", "overloaded_error", "provider_overloaded", 529, true, data);
        Assert.True(StreamProviderError.IsInstance(error));
        Assert.Equal("Overloaded", error.Message);
        Assert.Equal("overloaded_error", error.Type);
        Assert.Equal("provider_overloaded", error.Code);
        Assert.Equal(529, error.StatusCode);
        Assert.True(error.IsRetryable);
        Assert.Same(data, error.Data);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/create-provider-stream-error.test.ts::createProviderStreamError::does not identify unmarked provider payloads", Coverage = UpstreamCoverage.Covered)]
    public void Provider_stream_error_rejects_an_unmarked_object()
    {
        Assert.False(StreamProviderError.IsInstance(new Dictionary<string, object?>
        {
            ["message"] = "Overloaded",
            ["type"] = "overloaded_error",
        }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-provider-reference.test.ts::isProviderReference::returns true for a plain record of provider ids", Coverage = UpstreamCoverage.Covered)]
    public void Provider_reference_accepts_a_provider_map()
    {
        Assert.True(ProviderReferences.IsProviderReference(new Dictionary<string, object?> { ["openai"] = "file-abc123" }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-provider-reference.test.ts::isProviderReference::returns true for a record with a single fileId-like key", Coverage = UpstreamCoverage.Covered)]
    public void Provider_reference_accepts_a_file_id_map()
    {
        Assert.True(ProviderReferences.IsProviderReference(new Dictionary<string, object?> { ["fileId"] = "abc" }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-provider-reference.test.ts::isProviderReference::returns false for an object carrying a type property (tagged reference)", Coverage = UpstreamCoverage.Covered)]
    public void Provider_reference_rejects_a_typed_object()
    {
        Assert.False(ProviderReferences.IsProviderReference(new Dictionary<string, object?>
        {
            ["type"] = "reference",
            ["reference"] = new Dictionary<string, object?> { ["fileId"] = "abc" },
        }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-provider-reference.test.ts::isProviderReference::returns false for a tagged data object with type: \"data\"", Coverage = UpstreamCoverage.Covered)]
    public void Provider_reference_rejects_data_objects()
    {
        Assert.False(ProviderReferences.IsProviderReference(new Dictionary<string, object?> { ["type"] = "data", ["data"] = "x" }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-provider-reference.test.ts::isProviderReference::returns false for a Uint8Array", Coverage = UpstreamCoverage.Covered)]
    public void Provider_reference_rejects_bytes()
    {
        Assert.False(ProviderReferences.IsProviderReference(new byte[] { 1, 2, 3 }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-provider-reference.test.ts::isProviderReference::returns false for a URL instance", Coverage = UpstreamCoverage.Covered)]
    public void Provider_reference_rejects_a_url()
    {
        Assert.False(ProviderReferences.IsProviderReference(new Uri("https://example.com/file")));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-provider-reference.test.ts::isProviderReference::returns false for null", Coverage = UpstreamCoverage.Covered)]
    public void Provider_reference_rejects_null()
    {
        Assert.False(ProviderReferences.IsProviderReference(null));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-provider-reference.test.ts::isProviderReference::returns false for a string primitive", Coverage = UpstreamCoverage.Covered)]
    public void Provider_reference_rejects_a_string()
    {
        Assert.False(ProviderReferences.IsProviderReference("some-string"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-provider-reference.test.ts::isProviderReference::returns false for a number primitive", Coverage = UpstreamCoverage.Covered)]
    public void Provider_reference_rejects_a_number()
    {
        Assert.False(ProviderReferences.IsProviderReference(42));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/resolve-provider-reference.test.ts::resolveProviderReference::should return the provider-specific identifier when the provider key exists", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_provider_reference_returns_the_matching_id()
    {
        var reference = new Dictionary<string, string> { ["openai"] = "file-abc", ["anthropic"] = "file-xyz" };
        Assert.Equal("file-abc", ProviderReferences.ResolveProviderReference(reference, "openai"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/resolve-provider-reference.test.ts::resolveProviderReference::should return the correct identifier for a different provider", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_provider_reference_returns_the_other_provider()
    {
        var reference = new Dictionary<string, string> { ["openai"] = "file-abc", ["anthropic"] = "file-xyz" };
        Assert.Equal("file-xyz", ProviderReferences.ResolveProviderReference(reference, "anthropic"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/resolve-provider-reference.test.ts::resolveProviderReference::should throw NoSuchProviderReferenceError when no entry exists for the given provider", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_provider_reference_throws_when_the_provider_is_missing()
    {
        var reference = new Dictionary<string, string> { ["anthropic"] = "file-xyz", ["google"] = "file-123" };
        var error = Assert.Throws<NoSuchProviderReferenceError>(() => ProviderReferences.ResolveProviderReference(reference, "openai"));
        Assert.True(NoSuchProviderReferenceError.IsInstance(error));
        Assert.Equal("openai", error.Provider);
        Assert.Equal(reference, error.Reference);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/resolve-provider-reference.test.ts::resolveProviderReference::should throw NoSuchProviderReferenceError when reference is empty", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_provider_reference_throws_for_an_empty_map()
    {
        var error = Assert.Throws<NoSuchProviderReferenceError>(() => ProviderReferences.ResolveProviderReference(new Dictionary<string, string>(), "openai"));
        Assert.True(NoSuchProviderReferenceError.IsInstance(error));
        Assert.Equal("openai", error.Provider);
        Assert.Empty(error.Reference);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/resolve-provider-reference.test.ts::resolveProviderReference::should work with a single-provider reference", Coverage = UpstreamCoverage.Covered)]
    public void Resolve_provider_reference_accepts_one_entry()
    {
        Assert.Equal("file-only", ProviderReferences.ResolveProviderReference(new Dictionary<string, string> { ["openai"] = "file-only" }, "openai"));
    }
}
