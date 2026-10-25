// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Providers.NormalizeJsonSchemaForMfjs.NormalizeJsonSchemaForMfjsSupport;

namespace Vercel.AI.Tests.Upstream.Providers.NormalizeJsonSchemaForMfjs;

/// <summary>Port of <c>normalize-json-schema-for-mfjs.test.ts</c> &gt; <c>root guard</c>.</summary>
public sealed class RootGuardTests
{
    [Fact]
    [UpstreamTest(TestFile + "::root guard::throws for a non-object root type", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_a_non_object_root_type()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => Normalize(Json("""{ "type": "array", "items": { "type": "number" } }""")));
    }

    [Fact]
    [UpstreamTest(TestFile + "::root guard::throws for a missing root type", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_a_missing_root_type()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => Normalize(Json("""{ "properties": { "a": { "type": "string" } } }""")));
    }

    [Fact]
    [UpstreamTest(TestFile + "::root guard::throws for a boolean root schema", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_a_boolean_root_schema()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => Normalize(JsonValue.Create(true)));
    }

    [Fact]
    [UpstreamTest(TestFile + "::root guard::produces a clear error message", Coverage = UpstreamCoverage.Covered)]
    public void Produces_a_clear_error_message()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() => Normalize(Json("""{ "type": "array" }""")));

        Assert.Contains(RootMessage, error.Message);
    }
}
