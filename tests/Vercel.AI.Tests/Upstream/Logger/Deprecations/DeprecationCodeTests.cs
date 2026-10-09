// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using DeprecationCodes = Vercel.AI.Util.Deprecations;

namespace Vercel.AI.Tests.Upstream.Logger.Deprecations;

/// <summary>Port of <c>deprecations.test.ts</c> &gt; <c>getDeprecationCode</c>.</summary>
public sealed class DeprecationCodeTests
{
    public static TheoryData<string, string> RegisteredCodes
    {
        get
        {
            var data = new TheoryData<string, string>
            {
                { "generateObject", "AISDK_DEP_GENERATE_OBJECT" },
                { "streamObject", "AISDK_DEP_STREAM_OBJECT" },
                { "experimental_generateSpeech", "AISDK_DEP_EXPERIMENTAL_GENERATE_SPEECH" },
                { "experimental_transcribe", "AISDK_DEP_EXPERIMENTAL_TRANSCRIBE" },
                { "\"image\" content part", "AISDK_DEP_IMAGE_CONTENT_PART" },
                { "rawInput in output-error UI message parts", "AISDK_DEP_UI_MESSAGE_RAW_INPUT" },
            };
            foreach (var type in new[] { "file-data", "file-url", "file-id", "file-reference", "image-data", "image-url", "image-file-id", "image-file-reference" })
            {
                data.Add("\"tool-result\" content of type \"" + type + "\"", "AISDK_DEP_TOOL_RESULT_" + type.Replace('-', '_').ToUpperInvariant());
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(RegisteredCodes))]
    [UpstreamTest("packages/ai/src/logger/deprecations.test.ts::getDeprecationCode::keeps the registered code for %s", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_registered_code(string setting, string code)
    {
        Assert.Equal(code, DeprecationCodes.GetDeprecationCode(setting, null));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/logger/deprecations.test.ts::getDeprecationCode::scopes provider warnings separately from built-in deprecations", Coverage = UpstreamCoverage.Covered)]
    public void Scopes_provider_warnings_separately_from_built_in_deprecations()
    {
        Assert.Equal("AISDK_DEP_PROVIDER_test__generateObject", DeprecationCodes.GetDeprecationCode("generateObject", "test"));
        Assert.Equal("AISDK_DEP_PROVIDER_anthropic__temperature", DeprecationCodes.GetDeprecationCode("temperature", "anthropic"));
        Assert.Equal("AISDK_DEP_SETTING_temperature", DeprecationCodes.GetDeprecationCode("temperature", null));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/logger/deprecations.test.ts::getDeprecationCode::does not collapse punctuation, case, Unicode, or provider boundaries", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_collapse_punctuation_case_Unicode_or_provider_boundaries()
    {
        var inputs = new (string Setting, string? Provider)[]
        {
            ("old-key", "test"),
            ("old_key", "test"),
            ("old_002Dkey", "test"),
            ("Old-key", "test"),
            ("old-key", "other"),
            ("old-key", null),
            ("key", "test__old"),
            ("old__key", "test"),
            ("\U0001F600", null),
            ("\U0001F601", null),
            ("\ud800", null),
            ("toString", null),
        };

        var codes = inputs.Select(input => DeprecationCodes.GetDeprecationCode(input.Setting, input.Provider)).ToList();

        Assert.Equal(inputs.Length, new HashSet<string>(codes).Count);
        Assert.Equal(codes, inputs.Select(input => DeprecationCodes.GetDeprecationCode(input.Setting, input.Provider)));
        Assert.All(codes, code => Assert.Matches("^[a-zA-Z0-9_]+$", code));
        Assert.Equal("AISDK_DEP_PROVIDER_test__old_002Dkey", codes[0]);
    }
}
