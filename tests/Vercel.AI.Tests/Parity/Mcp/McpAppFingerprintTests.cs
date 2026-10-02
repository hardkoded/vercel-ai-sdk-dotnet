// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Mcp;

namespace Vercel.AI.Tests;

public sealed class McpAppFingerprintTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-app-fingerprint.test.ts::fingerprintMCPAppResource::produces a stable digest for equal resources",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Fingerprint_is_stable_for_equal_resources()
    {
        Assert.Equal(await McpAppFingerprint.FingerprintAsync(Resource()), await McpAppFingerprint.FingerprintAsync(Resource()));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-app-fingerprint.test.ts::fingerprintMCPAppResource::ignores key ordering in csp / permissions",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Fingerprint_ignores_key_order()
    {
        var first = await McpAppFingerprint.FingerprintAsync(Resource(
            "{\"connectDomains\":[\"https://api.example\"],\"frameDomains\":[]}",
            "{\"microphone\":{},\"camera\":{}}"));
        var second = await McpAppFingerprint.FingerprintAsync(Resource(
            "{\"frameDomains\":[],\"connectDomains\":[\"https://api.example\"]}",
            "{\"camera\":{},\"microphone\":{}}"));

        Assert.Equal(first, second);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-app-fingerprint.test.ts::fingerprintMCPAppResource::changes when html, csp, or permissions mutate",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Fingerprint_changes_when_html_csp_or_permissions_change()
    {
        var baseline = await McpAppFingerprint.FingerprintAsync(Resource());
        var html = Resource();
        var changedHtml = new McpAppResource(html.Uri, "<html>evil</html>", html.MimeType)
        {
            Meta = html.Meta,
        };

        Assert.NotEqual(baseline, await McpAppFingerprint.FingerprintAsync(changedHtml));
        Assert.NotEqual(baseline, await McpAppFingerprint.FingerprintAsync(Resource("{\"connectDomains\":[\"https://evil.example\"]}", null)));
        Assert.NotEqual(baseline, await McpAppFingerprint.FingerprintAsync(Resource(null, "{\"camera\":{}}")));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-app-fingerprint.test.ts::detectMCPAppResourceDrift::flags a changed fingerprint",
        Coverage = UpstreamCoverage.Covered)]
    public void Detect_drift_flags_a_changed_fingerprint()
    {
        Assert.True(McpAppFingerprint.DetectDrift("a", "b"));
        Assert.False(McpAppFingerprint.DetectDrift("a", "a"));
    }

    private static McpAppResource Resource(string? csp = "{\"connectDomains\":[\"https://api.example\"]}", string? permissions = "{\"microphone\":{}}")
    {
        return new McpAppResource("ui://app/dashboard", "<!doctype html><html></html>")
        {
            Meta = new McpAppResourceMeta
            {
                Csp = csp == null ? null : JsonNode.Parse(csp),
                Permissions = permissions == null ? null : JsonNode.Parse(permissions),
            },
        };
    }
}
