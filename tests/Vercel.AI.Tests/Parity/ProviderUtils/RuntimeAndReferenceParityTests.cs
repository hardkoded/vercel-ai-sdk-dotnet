// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class RuntimeAndReferenceParityTests
{
    [Fact]
    [UpstreamTest("packages/provider-utils/src/get-runtime-environment-user-agent.test.ts::getRuntimeEnvironmentUserAgent::should return the correct user agent for browsers", Coverage = UpstreamCoverage.Covered)]
    public void Runtime_user_agent_is_browser_when_window_is_set()
    {
        Assert.Equal("runtime/browser", RuntimeEnvironmentInfo.GetRuntimeEnvironmentUserAgent(new RuntimeGlobals { Window = true }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/get-runtime-environment-user-agent.test.ts::getRuntimeEnvironmentUserAgent::should return the correct user agent for test", Coverage = UpstreamCoverage.Covered)]
    public void Runtime_user_agent_uses_the_navigator_string()
    {
        Assert.Equal("runtime/test", RuntimeEnvironmentInfo.GetRuntimeEnvironmentUserAgent(new RuntimeGlobals { HasNavigator = true, NavigatorUserAgent = "test" }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/get-runtime-environment-user-agent.test.ts::getRuntimeEnvironmentUserAgent::should return the correct user agent for Edge Runtime", Coverage = UpstreamCoverage.Covered)]
    public void Runtime_user_agent_is_vercel_edge()
    {
        Assert.Equal("runtime/vercel-edge", RuntimeEnvironmentInfo.GetRuntimeEnvironmentUserAgent(new RuntimeGlobals { EdgeRuntime = true }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/get-runtime-environment-user-agent.test.ts::getRuntimeEnvironmentUserAgent::should return the correct user agent for Node.js", Coverage = UpstreamCoverage.Covered)]
    public void Runtime_user_agent_includes_the_node_version()
    {
        Assert.Equal("runtime/node.js/test", RuntimeEnvironmentInfo.GetRuntimeEnvironmentUserAgent(new RuntimeGlobals { NodeVersion = "test", ProcessVersion = "test" }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-browser-runtime.test.ts::isBrowserRuntime::returns true when a global window is present", Coverage = UpstreamCoverage.Covered)]
    public void Browser_runtime_is_true_when_window_exists()
    {
        Assert.True(RuntimeEnvironmentInfo.IsBrowserRuntime(new RuntimeGlobals { Window = new object() }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-browser-runtime.test.ts::isBrowserRuntime::returns false when there is no window (server runtimes)", Coverage = UpstreamCoverage.Covered)]
    public void Browser_runtime_is_false_without_a_window()
    {
        Assert.False(RuntimeEnvironmentInfo.IsBrowserRuntime(new RuntimeGlobals()));
        Assert.False(RuntimeEnvironmentInfo.IsBrowserRuntime(new RuntimeGlobals { Window = JsUndefined.Value }));
        Assert.False(RuntimeEnvironmentInfo.IsBrowserRuntime(new RuntimeGlobals { NodeVersion = "22" }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-abort-error.test.ts::isAbortError::returns true for recognized Error names", Coverage = UpstreamCoverage.Covered)]
    public void Abort_error_matches_abort_names()
    {
        foreach (var name in new[] { "AbortError", "ResponseAborted", "TimeoutError" })
        {
            Assert.True(AbortErrors.IsAbortError(new JsError("request stopped", name)));
        }
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-abort-error.test.ts::isAbortError::returns true for an abort DOMException", Coverage = UpstreamCoverage.Covered)]
    public void Abort_error_matches_a_dom_exception()
    {
        Assert.True(AbortErrors.IsAbortError(new DomException("Aborted", "AbortError")));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-abort-error.test.ts::isAbortError::returns false for unrelated errors", Coverage = UpstreamCoverage.Covered)]
    public void Abort_error_rejects_a_type_error()
    {
        Assert.False(AbortErrors.IsAbortError(new JsCauseError("some unrelated failure", "TypeError")));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/is-abort-error.test.ts::isAbortError::returns false when DOMException is not a constructor", Coverage = UpstreamCoverage.Covered)]
    public void Abort_error_rejects_a_plain_object()
    {
        Assert.False(AbortErrors.IsAbortError(new Dictionary<string, object?>
        {
            ["name"] = "SomeUnrelatedFailure",
            ["message"] = "some unrelated failure",
        }));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/normalize-batch-request-counts.test.ts::normalizeBatchRequestCounts::returns complete, consistent counts", Coverage = UpstreamCoverage.Covered)]
    public void Batch_counts_accept_a_consistent_total()
    {
        var result = BatchRequests.NormalizeBatchRequestCounts(5, 2, 2, 1);
        Assert.NotNull(result);
        Assert.Equal(5, result!.Total);
        Assert.Equal(2, result.Pending);
        Assert.Equal(2, result.Completed);
        Assert.Equal(1, result.Failed);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/normalize-batch-request-counts.test.ts::normalizeBatchRequestCounts::returns undefined for invalid counts %#", Coverage = UpstreamCoverage.Covered)]
    public void Batch_counts_reject_invalid_combinations()
    {
        Assert.Null(BatchRequests.NormalizeBatchRequestCounts(null, 0, 0, 0));
        Assert.Null(BatchRequests.NormalizeBatchRequestCounts(1, -1, 1, 1));
        Assert.Null(BatchRequests.NormalizeBatchRequestCounts(1, 0.5, 0.5, 0));
        Assert.Null(BatchRequests.NormalizeBatchRequestCounts(9007199254740992d, 0, 0, 0));
        Assert.Null(BatchRequests.NormalizeBatchRequestCounts(2, 0, 1, 0));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/create-null-language-model-usage.test.ts::createNullLanguageModelUsage::creates usage with all token counts undefined", Coverage = UpstreamCoverage.Covered)]
    public void Null_usage_leaves_every_count_unset()
    {
        var usage = LanguageModelUsages.CreateNullLanguageModelUsage();
        Assert.Null(usage.InputTokens.Total);
        Assert.Null(usage.InputTokens.NoCache);
        Assert.Null(usage.InputTokens.CacheRead);
        Assert.Null(usage.InputTokens.CacheWrite);
        Assert.Null(usage.OutputTokens.Total);
        Assert.Null(usage.OutputTokens.Text);
        Assert.Null(usage.OutputTokens.Reasoning);
        Assert.Null(usage.Raw);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/create-null-language-model-usage.test.ts::createNullLanguageModelUsage::creates a fresh usage object", Coverage = UpstreamCoverage.Covered)]
    public void Null_usage_returns_a_new_object()
    {
        var first = LanguageModelUsages.CreateNullLanguageModelUsage();
        var second = LanguageModelUsages.CreateNullLanguageModelUsage();
        Assert.NotSame(first, second);
        Assert.NotSame(first.InputTokens, second.InputTokens);
        Assert.NotSame(first.OutputTokens, second.OutputTokens);
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
        var error = ProviderStreamErrors.CreateProviderStreamError("Overloaded", data, "overloaded_error", "provider_overloaded", 529, true);
        Assert.True(ProviderStreamErrors.IsProviderStreamError(error));
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
        Assert.False(ProviderStreamErrors.IsProviderStreamError(new Dictionary<string, object?>
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
