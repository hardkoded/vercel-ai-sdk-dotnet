// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class PrepareResponsesToolsTests
{
    [Fact]
    [UpstreamTest(Suite + "::should remove string propertyNames from function tools and warn", Coverage = UpstreamCoverage.Covered)]
    public void Should_remove_string_propertyNames_from_function_tools_and_warn()
    {
        var result = Prepare(null, Function("get_weather", null, """{"type":"object","properties":{"values":{"type":"object","propertyNames":{"type":"string","format":"uuid"}}}}"""));

        AssertTools(result, """[{"type":"function","name":"get_weather","parameters":{"type":"object","properties":{"values":{"type":"object"}}},"strict":false}]""", expectedWarnings: true);
        AssertNoToolChoice(result);
        AssertWarnings(result, ("compatibility", "JSON Schema propertyNames", "OpenAI does not support JSON Schema propertyNames. It was removed before sending the schema, so OpenAI will not enforce property-name constraints."));
    }

    [Fact]
    [UpstreamTest(Suite + "::should remove regex lookaround patterns from function tools and warn", Coverage = UpstreamCoverage.Covered)]
    public void Should_remove_regex_lookaround_patterns_from_function_tools_and_warn()
    {
        var result = Prepare(null, Function("create_contact", null, """{"type":"object","properties":{"email":{"type":"string","format":"email","pattern":"^(?!\\.).+@.+$"},"username":{"type":"string","pattern":"^@[a-zA-Z0-9_]+$"}}}"""));

        AssertTools(result, """[{"type":"function","name":"create_contact","parameters":{"type":"object","properties":{"email":{"type":"string","format":"email"},"username":{"type":"string","pattern":"^@[a-zA-Z0-9_]+$"}}},"strict":false}]""", expectedWarnings: true);
        AssertNoToolChoice(result);
        AssertWarnings(result, ("compatibility", "JSON Schema pattern with regex lookaround", "OpenAI does not support regex lookaround in JSON Schema patterns. The pattern was removed before sending the schema, so OpenAI will not enforce that constraint."));
    }
}
