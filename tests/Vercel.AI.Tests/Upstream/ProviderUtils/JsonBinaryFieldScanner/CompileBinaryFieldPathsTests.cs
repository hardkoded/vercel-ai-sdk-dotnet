// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using Scanner = Vercel.AI.ProviderUtils.JsonBinaryFieldScanner;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.JsonBinaryFieldScanner;

/// <summary>Port of <c>json-binary-field-scanner.test.ts</c> &gt; <c>compileBinaryFieldPaths</c>.</summary>
public sealed class CompileBinaryFieldPathsTests
{
    private const string Prefix = ScannerSupport.TestFile + "compileBinaryFieldPaths::";

    [Theory]
    [InlineData("images[*]")]
    [InlineData("content[*].data.data")]
    [InlineData("a.b.c")]
    [InlineData("a[*][*]")]
    [InlineData("[*].data")]
    [InlineData("a-b_c.d9")]
    [UpstreamTest(
        Prefix + "accepts %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = ScannerSupport.Commit
    )]
    public void Accepts(string path)
    {
        Scanner.CompileBinaryFieldPaths([path]);
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("a.")]
    [InlineData(".a")]
    [InlineData("a..b")]
    [InlineData("a[0]")]
    [InlineData("a[*")]
    [InlineData("a[*]b")]
    [InlineData("a[*]bc")]
    [InlineData("a.b[*]c")]
    [InlineData("$.images[*]")]
    [InlineData("images[*].")]
    [InlineData("a b")]
    [InlineData("ключ")]
    [InlineData("a\"b")]
    [InlineData("images[*]\n")]
    [UpstreamTest(
        Prefix + "rejects %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = ScannerSupport.Commit
    )]
    public void Rejects(string path)
    {
        Assert.Throws<InvalidArgumentError>(() => Scanner.CompileBinaryFieldPaths([path]));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "describes the problem and the path in the error",
        Coverage = UpstreamCoverage.Covered,
        Commit = ScannerSupport.Commit
    )]
    public void Describes_the_problem_and_the_path_in_the_error()
    {
        var error = Assert.Throws<InvalidArgumentError>(() =>
            Scanner.CompileBinaryFieldPaths(["images[0]"])
        );

        Assert.Contains("Invalid binary field path \"images[0]\"", error.Message);
    }
}
