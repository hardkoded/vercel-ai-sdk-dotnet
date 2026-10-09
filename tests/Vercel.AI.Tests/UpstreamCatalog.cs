// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Vercel.AI.Tests;

internal sealed class UpstreamTestCase
{
    public string Id { get; set; } = string.Empty;

    public string Package { get; set; } = string.Empty;

    public string Feature { get; set; } = string.Empty;

    public string File { get; set; } = string.Empty;

    public int Line { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public string? Suite { get; set; }

    public string? DotnetProject { get; set; }

    public string? ScopeReason { get; set; }

    public string? Runtime { get; set; }
}

internal sealed class UpstreamManifest
{
    public string UpstreamRepo { get; set; } = string.Empty;

    public string UpstreamCommit { get; set; } = string.Empty;

    public int UnitTestCount { get; set; }

    public int InScopeCount { get; set; }

    public int OutOfScopeCount { get; set; }
}

internal sealed class UpstreamLink
{
    public string UpstreamId { get; set; } = string.Empty;

    public string DotnetTest { get; set; } = string.Empty;

    public string Coverage { get; set; } = string.Empty;

    public string? Note { get; set; }
}

internal sealed class FeatureCoverage
{
    public string Feature { get; set; } = string.Empty;

    public string DotnetProject { get; set; } = string.Empty;

    public int InScope { get; set; }

    public int Covered { get; set; }

    public int Partial { get; set; }

    public int Missing { get; set; }
}

internal sealed class CoverageSummary
{
    public string UpstreamCommit { get; set; } = string.Empty;

    public int UnitTests { get; set; }

    public int InScope { get; set; }

    public int OutOfScope { get; set; }

    public int Covered { get; set; }

    public int Partial { get; set; }

    public int Missing { get; set; }

    public List<FeatureCoverage> Features { get; set; } = new();
}

internal static class UpstreamCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string ParityDirectory { get; } = Path.Combine(AppContext.BaseDirectory, "parity");

    public static IReadOnlyList<UpstreamTestCase> LoadTests()
    {
        var path = Path.Combine(ParityDirectory, "upstream-unit-tests.jsonl");
        var tests = new List<UpstreamTestCase>();
        foreach (var line in File.ReadLines(path))
        {
            if (line.Length == 0)
            {
                continue;
            }

            var test = JsonSerializer.Deserialize<UpstreamTestCase>(line, JsonOptions);
            if (test == null || string.IsNullOrEmpty(test.Id))
            {
                throw new InvalidOperationException("Catalog row is missing an id.");
            }

            tests.Add(test);
        }

        return tests;
    }

    public static UpstreamManifest LoadManifest()
    {
        var path = Path.Combine(ParityDirectory, "manifest.json");
        return JsonSerializer.Deserialize<UpstreamManifest>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidOperationException("manifest.json is empty.");
    }

    public static string ReadCompatibilityCommit()
    {
        var text = File.ReadAllText(Path.Combine(ParityDirectory, "COMPATIBILITY.md"));
        var match = Regex.Match(text, "`([0-9a-f]{40})`");
        if (!match.Success)
        {
            throw new InvalidOperationException("COMPATIBILITY.md has no upstream commit.");
        }

        return match.Groups[1].Value;
    }

    public static IReadOnlyList<UpstreamLink> LoadLinks(Assembly assembly)
    {
        var links = new List<UpstreamLink>();
        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                foreach (var attribute in method.GetCustomAttributes<UpstreamTestAttribute>())
                {
                    links.Add(new UpstreamLink
                    {
                        UpstreamId = attribute.UpstreamId,
                        DotnetTest = type.FullName + "." + method.Name,
                        Coverage = attribute.Coverage.ToString(),
                        Note = attribute.Note,
                    });
                }
            }
        }

        return links;
    }

    public static CoverageSummary Summarize(IReadOnlyList<UpstreamTestCase> tests, IReadOnlyList<UpstreamLink> links, string commit)
    {
        var linked = new Dictionary<string, UpstreamLink>(StringComparer.Ordinal);
        foreach (var link in links)
        {
            linked[link.UpstreamId] = link;
        }

        var features = new Dictionary<string, FeatureCoverage>(StringComparer.Ordinal);
        var covered = 0;
        var partial = 0;
        var missing = 0;
        foreach (var test in tests)
        {
            if (!string.Equals(test.Scope, "in-scope", StringComparison.Ordinal))
            {
                continue;
            }

            if (!features.TryGetValue(test.Feature, out var feature))
            {
                feature = new FeatureCoverage
                {
                    Feature = test.Feature,
                    DotnetProject = test.DotnetProject ?? string.Empty,
                };
                features.Add(test.Feature, feature);
            }

            feature.InScope++;
            if (!linked.TryGetValue(test.Id, out var link))
            {
                feature.Missing++;
                missing++;
                continue;
            }

            if (string.Equals(link.Coverage, nameof(UpstreamCoverage.Covered), StringComparison.Ordinal))
            {
                feature.Covered++;
                covered++;
            }
            else
            {
                feature.Partial++;
                partial++;
            }
        }

        return new CoverageSummary
        {
            UpstreamCommit = commit,
            UnitTests = tests.Count,
            InScope = tests.Count(test => test.Scope == "in-scope"),
            OutOfScope = tests.Count(test => test.Scope == "out-of-scope"),
            Covered = covered,
            Partial = partial,
            Missing = missing,
            Features = features.Values.OrderBy(feature => feature.Feature, StringComparer.Ordinal).ToList(),
        };
    }
}
