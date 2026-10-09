// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests;

public sealed class UpstreamParityTests
{
    [Fact]
    public void Catalog_matches_the_compatibility_commit_and_its_manifest()
    {
        var tests = UpstreamCatalog.LoadTests();
        var manifest = UpstreamCatalog.LoadManifest();
        var commit = UpstreamCatalog.ReadCompatibilityCommit();

        Assert.Equal(commit, manifest.UpstreamCommit);
        Assert.Equal("https://github.com/vercel/ai", manifest.UpstreamRepo);
        Assert.Equal(tests.Count, tests.Select(test => test.Id).Distinct(StringComparer.Ordinal).Count());

        var ordered = tests.OrderBy(test => test.File, StringComparer.Ordinal)
            .ThenBy(test => test.Line)
            .ThenBy(test => test.Id, StringComparer.Ordinal)
            .Select(test => test.Id);
        Assert.Equal(ordered, tests.Select(test => test.Id));
    }

    [Fact]
    public void Linked_dotnet_tests_point_at_in_scope_upstream_tests()
    {
        var tests = UpstreamCatalog.LoadTests();
        var byId = tests.ToDictionary(test => test.Id, StringComparer.Ordinal);
        var links = UpstreamCatalog.LoadLinks(typeof(UpstreamParityTests).Assembly);
        var duplicates = links.GroupBy(link => link.UpstreamId, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        Assert.Empty(duplicates);
        foreach (var link in links)
        {
            Assert.True(byId.ContainsKey(link.UpstreamId), "Unknown upstream id: " + link.UpstreamId);
            Assert.Equal("in-scope", byId[link.UpstreamId].Scope);
            Assert.True(link.Coverage is nameof(UpstreamCoverage.Covered) or nameof(UpstreamCoverage.Partial));
        }
    }

    [Fact]
    public void Summary_counts_are_consistent_with_the_catalog_and_links()
    {
        var tests = UpstreamCatalog.LoadTests();
        var manifest = UpstreamCatalog.LoadManifest();
        var links = UpstreamCatalog.LoadLinks(typeof(UpstreamParityTests).Assembly);
        var summary = UpstreamCatalog.Summarize(tests, links, manifest.UpstreamCommit);

        Assert.Equal(summary.UnitTests, summary.InScope + summary.OutOfScope);
        Assert.Equal(links.Count, summary.Covered + summary.Partial);
        Assert.Equal(summary.InScope, summary.Covered + summary.Partial + summary.Missing);
    }
}
