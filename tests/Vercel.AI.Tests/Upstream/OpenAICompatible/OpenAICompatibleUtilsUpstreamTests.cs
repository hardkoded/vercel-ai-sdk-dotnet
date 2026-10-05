// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class OpenAICompatibleUtilsUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::toCamelCase::should convert hyphenated names to camelCase", Coverage = UpstreamCoverage.Covered)]
    public void Hyphens_become_camel_case()
    {
        Assert.Equal("providerName", OpenAICompatibleChat.ToCamelCase("provider-name"));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::toCamelCase::should convert underscored names to camelCase", Coverage = UpstreamCoverage.Covered)]
    public void Underscores_become_camel_case()
    {
        Assert.Equal("providerName", OpenAICompatibleChat.ToCamelCase("provider_name"));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::toCamelCase::should handle multiple separators", Coverage = UpstreamCoverage.Covered)]
    public void Multiple_separators_become_camel_case()
    {
        Assert.Equal("myProviderName", OpenAICompatibleChat.ToCamelCase("my-provider-name"));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::toCamelCase::should return the same string when already camelCase", Coverage = UpstreamCoverage.Covered)]
    public void Camel_case_is_unchanged()
    {
        Assert.Equal("providerName", OpenAICompatibleChat.ToCamelCase("providerName"));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::toCamelCase::should return the same string when no separators", Coverage = UpstreamCoverage.Covered)]
    public void A_name_without_separators_is_unchanged()
    {
        Assert.Equal("openai", OpenAICompatibleChat.ToCamelCase("openai"));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::toCamelCase::should handle empty string", Coverage = UpstreamCoverage.Covered)]
    public void Empty_name_stays_empty()
    {
        Assert.Equal(string.Empty, OpenAICompatibleChat.ToCamelCase(string.Empty));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::resolveProviderOptionsKey::should return camelCase key when camelCase options are present", Coverage = UpstreamCoverage.Covered)]
    public void Camel_case_options_select_the_camel_case_key()
    {
        var key = OpenAICompatibleChat.ResolveProviderOptionsKey("provider-name", UpstreamChat.Bag("providerName", "{\"someOption\":\"value\"}"));
        Assert.Equal("providerName", key);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::resolveProviderOptionsKey::should return raw key when only raw options are present", Coverage = UpstreamCoverage.Covered)]
    public void Raw_options_keep_the_raw_key()
    {
        var key = OpenAICompatibleChat.ResolveProviderOptionsKey("provider-name", UpstreamChat.Bag("provider-name", "{\"someOption\":\"value\"}"));
        Assert.Equal("provider-name", key);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::resolveProviderOptionsKey::should return camelCase key when both are present", Coverage = UpstreamCoverage.Covered)]
    public void Both_keys_prefer_camel_case()
    {
        var options = new Dictionary<string, JsonElement>
        {
            ["provider-name"] = UpstreamChat.Json("{\"a\":1}"),
            ["providerName"] = UpstreamChat.Json("{\"b\":2}"),
        };
        Assert.Equal("providerName", OpenAICompatibleChat.ResolveProviderOptionsKey("provider-name", options));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::resolveProviderOptionsKey::should return raw key when no options are present", Coverage = UpstreamCoverage.Covered)]
    public void An_empty_bag_keeps_the_raw_key()
    {
        Assert.Equal("provider-name", OpenAICompatibleChat.ResolveProviderOptionsKey("provider-name", new Dictionary<string, JsonElement>()));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::resolveProviderOptionsKey::should return raw key when providerOptions is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Missing_options_keep_the_raw_key()
    {
        Assert.Equal("provider-name", OpenAICompatibleChat.ResolveProviderOptionsKey("provider-name", null));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::resolveProviderOptionsKey::should return raw key when name has no separators", Coverage = UpstreamCoverage.Covered)]
    public void A_name_without_separators_stays_the_options_key()
    {
        Assert.Equal("openai", OpenAICompatibleChat.ResolveProviderOptionsKey("openai", UpstreamChat.Bag("openai", "{\"a\":1}")));
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::warnIfDeprecatedProviderOptionsKey::should push a deprecated warning when raw key is used and differs from camelCase", Coverage = UpstreamCoverage.Covered)]
    public void A_raw_key_warns()
    {
        var warnings = new List<CallWarning>();
        OpenAICompatibleChat.WarnIfDeprecated("provider-name", UpstreamChat.Bag("provider-name", "{\"a\":1}"), warnings);
        Assert.Equal("deprecated", warnings[0].Type);
        Assert.Equal("providerOptions key 'provider-name'. Use 'providerName' instead.", warnings[0].Message);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::warnIfDeprecatedProviderOptionsKey::should not push a warning when only camelCase key is used", Coverage = UpstreamCoverage.Covered)]
    public void A_camel_case_key_does_not_warn()
    {
        var warnings = new List<CallWarning>();
        OpenAICompatibleChat.WarnIfDeprecated("provider-name", UpstreamChat.Bag("providerName", "{\"a\":1}"), warnings);
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::warnIfDeprecatedProviderOptionsKey::should not push a warning when raw name is already camelCase", Coverage = UpstreamCoverage.Covered)]
    public void An_already_camel_case_name_does_not_warn()
    {
        var warnings = new List<CallWarning>();
        OpenAICompatibleChat.WarnIfDeprecated("openai", UpstreamChat.Bag("openai", "{\"a\":1}"), warnings);
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::warnIfDeprecatedProviderOptionsKey::should not push a warning when raw key is not present in providerOptions", Coverage = UpstreamCoverage.Covered)]
    public void A_missing_raw_key_does_not_warn()
    {
        var warnings = new List<CallWarning>();
        OpenAICompatibleChat.WarnIfDeprecated("provider-name", UpstreamChat.Bag("other", "{\"a\":1}"), warnings);
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest("packages/openai-compatible/src/utils/to-camel-case.test.ts::warnIfDeprecatedProviderOptionsKey::should not push a warning when providerOptions is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Missing_options_do_not_warn()
    {
        var warnings = new List<CallWarning>();
        OpenAICompatibleChat.WarnIfDeprecated("provider-name", null, warnings);
        Assert.Empty(warnings);
    }
}
