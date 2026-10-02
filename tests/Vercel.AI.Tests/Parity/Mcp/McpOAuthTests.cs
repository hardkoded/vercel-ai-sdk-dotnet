// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mcp;

namespace Vercel.AI.Tests;

public sealed class McpOAuthTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceUrlFromServerUrl::should remove fragments",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_url_removes_fragments()
    {
        Assert.Equal("https://example.com/path", McpOAuth.ResourceUrlFromServerUrl("https://example.com/path#fragment"));
        Assert.Equal("https://example.com/", McpOAuth.ResourceUrlFromServerUrl("https://example.com#fragment"));
        Assert.Equal("https://example.com/path?query=1", McpOAuth.ResourceUrlFromServerUrl("https://example.com/path?query=1#fragment"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceUrlFromServerUrl::should return URL unchanged if no fragment",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_url_is_unchanged_without_a_fragment()
    {
        Assert.Equal("https://example.com/", McpOAuth.ResourceUrlFromServerUrl("https://example.com"));
        Assert.Equal("https://example.com/path", McpOAuth.ResourceUrlFromServerUrl("https://example.com/path"));
        Assert.Equal("https://example.com/path?query=1", McpOAuth.ResourceUrlFromServerUrl("https://example.com/path?query=1"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceUrlFromServerUrl::should keep everything else unchanged",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_url_keeps_path_case_ports_query_and_slashes()
    {
        Assert.Equal("https://example.com/PATH", McpOAuth.ResourceUrlFromServerUrl("https://EXAMPLE.COM/PATH"));
        Assert.Equal("https://example.com/path", McpOAuth.ResourceUrlFromServerUrl("https://example.com:443/path"));
        Assert.Equal("https://example.com:8080/path", McpOAuth.ResourceUrlFromServerUrl("https://example.com:8080/path"));
        Assert.Equal("https://example.com/?foo=bar&baz=qux", McpOAuth.ResourceUrlFromServerUrl("https://example.com?foo=bar&baz=qux"));
        Assert.Equal("https://example.com/", McpOAuth.ResourceUrlFromServerUrl("https://example.com/"));
        Assert.Equal("https://example.com/path/", McpOAuth.ResourceUrlFromServerUrl("https://example.com/path/"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceMatches::should match identical URLs",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_matches_identical_urls()
    {
        Assert.True(McpOAuth.CheckResourceAllowed("https://example.com/path", "https://example.com/path"));
        Assert.True(McpOAuth.CheckResourceAllowed("https://example.com/", "https://example.com/"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceMatches::should not match URLs with different paths",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_rejects_different_paths()
    {
        Assert.False(McpOAuth.CheckResourceAllowed("https://example.com/path1", "https://example.com/path2"));
        Assert.False(McpOAuth.CheckResourceAllowed("https://example.com/", "https://example.com/path"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceMatches::should not match URLs with different domains",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_rejects_different_domains()
    {
        Assert.False(McpOAuth.CheckResourceAllowed("https://example.com/path", "https://example.org/path"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceMatches::should not match URLs with different ports",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_rejects_different_ports()
    {
        Assert.False(McpOAuth.CheckResourceAllowed("https://example.com:8080/path", "https://example.com/path"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceMatches::should not match URLs where one path is a sub-path of another",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_matches_only_real_subpaths()
    {
        Assert.False(McpOAuth.CheckResourceAllowed("https://example.com/mcpxxxx", "https://example.com/mcp"));
        Assert.False(McpOAuth.CheckResourceAllowed("https://example.com/folder", "https://example.com/folder/subfolder"));
        Assert.True(McpOAuth.CheckResourceAllowed("https://example.com/api/v1", "https://example.com/api"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/util/oauth.util.test.ts::auth-utils > resourceMatches::should handle trailing slashes vs no trailing slashes",
        Coverage = UpstreamCoverage.Covered)]
    public void Resource_handles_trailing_slashes()
    {
        Assert.True(McpOAuth.CheckResourceAllowed("https://example.com/mcp/", "https://example.com/mcp"));
        Assert.False(McpOAuth.CheckResourceAllowed("https://example.com/folder", "https://example.com/folder/"));
    }
}
