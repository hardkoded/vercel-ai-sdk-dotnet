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
        Assert.Equal(tests.Count, manifest.UnitTestCount);
        Assert.Equal(tests.Count(test => test.Scope == "in-scope"), manifest.InScopeCount);
        Assert.Equal(tests.Count(test => test.Scope == "out-of-scope"), manifest.OutOfScopeCount);
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
    public void Coverage_summary_matches_the_catalog_and_links()
    {
        var tests = UpstreamCatalog.LoadTests();
        var manifest = UpstreamCatalog.LoadManifest();
        var links = UpstreamCatalog.LoadLinks(typeof(UpstreamParityTests).Assembly);
        var actual = UpstreamCatalog.Summarize(tests, links, manifest.UpstreamCommit);
        var committed = UpstreamCatalog.LoadSummary();

        Assert.Equal(actual.UpstreamCommit, committed.UpstreamCommit);
        Assert.Equal(actual.UnitTests, committed.UnitTests);
        Assert.Equal(actual.InScope, committed.InScope);
        Assert.Equal(actual.OutOfScope, committed.OutOfScope);
        Assert.Equal(actual.Covered, committed.Covered);
        Assert.Equal(actual.Partial, committed.Partial);
        Assert.Equal(actual.Missing, committed.Missing);
        Assert.Equal(actual.Features.Count, committed.Features.Count);
        var issues = UpstreamCatalog.LoadIssues().Issues.ToDictionary(issue => issue.Feature, StringComparer.Ordinal);
        for (var i = 0; i < actual.Features.Count; i++)
        {
            var expected = actual.Features[i];
            var stored = committed.Features[i];
            Assert.Equal(expected.Feature, stored.Feature);
            Assert.Equal(expected.DotnetProject, stored.DotnetProject);
            Assert.Equal(expected.InScope, stored.InScope);
            Assert.Equal(expected.Covered, stored.Covered);
            Assert.Equal(expected.Partial, stored.Partial);
            Assert.Equal(expected.Missing, stored.Missing);
            if (expected.Missing == 0 && expected.Partial == 0)
            {
                continue;
            }

            Assert.True(issues.TryGetValue(expected.Feature, out var issue), "Missing issue index entry for " + expected.Feature);
            Assert.Equal("Cover upstream unit tests: " + expected.Feature, issue!.Title);
            Assert.Equal(expected.DotnetProject, issue.DotnetProject);
            Assert.Equal(expected.InScope, issue.InScope);
            Assert.Equal(expected.Covered, issue.Covered);
            Assert.Equal(expected.Partial, issue.Partial);
            Assert.Equal(expected.Missing, issue.Missing);
        }
    }
}
