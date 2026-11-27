// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doGenerate &gt; text</c>.</summary>
public sealed class DoGenerateTextTests
{
    private const string Prefix = TestFile + " > doGenerate > text::";

    private const string FrequencyPenaltyDeprecated =
        "frequencyPenalty is deprecated by DeepSeek and has been omitted. Remove frequencyPenalty from the request.";

    private const string PresencePenaltyDeprecated =
        "presencePenalty is deprecated by DeepSeek and has been omitted. Remove presencePenalty from the request.";

    public static TheoryData<string, string> InvalidUserIds()
    {
        return new TheoryData<string, string>
        {
            { string.Empty, "userId must match /^[a-zA-Z0-9_-]+$/" },
            { "contains space", "userId must match /^[a-zA-Z0-9_-]+$/" },
            { new string('a', 513), "userId must be at most 512 characters long" },
        };
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send correct request body",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_correct_request_body()
    {
        var capture = JsonFixture("deepseek-text");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("You are a helpful assistant."),
                new UserModelMessage("Hello"),
            },
            Temperature = 0.5,
            TopP = 0.3,
        };

        await Model(capture, "deepseek-chat").DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            RequestBody(capture),
            """
            {
              "messages": [
                { "content": "You are a helpful assistant.", "role": "system" },
                { "content": "Hello", "role": "user" }
              ],
              "model": "deepseek-chat",
              "temperature": 0.5,
              "top_p": 0.3
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should omit deprecated and ineffective sampling options in default V4 thinking mode",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_omit_deprecated_and_ineffective_sampling_options_in_default_V4_thinking_mode()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Temperature = 0.2;
        options.TopP = 0.4;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.6;

        var result = await Model(capture, "deepseek-v4-flash")
            .DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            RequestBody(capture),
            """{"model":"deepseek-v4-flash","messages":[{"role":"user","content":"Hello"}]}"""
        );
        AssertWarnings(
            result,
            ("deprecated", null, FrequencyPenaltyDeprecated),
            ("deprecated", null, PresencePenaltyDeprecated),
            (
                "unsupported",
                "temperature",
                "temperature has no effect when DeepSeek thinking is enabled. Set providerOptions.deepseek.thinking.type to 'disabled' to use temperature."
            ),
            (
                "unsupported",
                "topP",
                "topP has no effect when DeepSeek thinking is enabled. Set providerOptions.deepseek.thinking.type to 'disabled' to use topP."
            )
        );
        AssertDeprecatedSettings(result, "frequencyPenalty", "presencePenalty");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve supported sampling options when V4 thinking is disabled",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_preserve_supported_sampling_options_when_V4_thinking_is_disabled()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Temperature = 0.2;
        options.TopP = 0.4;
        options.FrequencyPenalty = 0.5;
        options.PresencePenalty = 0.6;
        options.ProviderOptions = DeepSeekOptions("""{"thinking":{"type":"disabled"}}""");

        var result = await Model(capture, "deepseek-v4-flash")
            .DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            RequestBody(capture),
            """
            {
              "model": "deepseek-v4-flash",
              "messages": [{ "role": "user", "content": "Hello" }],
              "temperature": 0.2,
              "top_p": 0.4,
              "thinking": { "type": "disabled" }
            }
            """
        );
        AssertWarnings(
            result,
            ("deprecated", null, FrequencyPenaltyDeprecated),
            ("deprecated", null, PresencePenaltyDeprecated)
        );
        AssertDeprecatedSettings(result, "frequencyPenalty", "presencePenalty");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send message names",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_message_names()
    {
        var capture = JsonFixture("deepseek-text");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage(
                    "You are a helpful assistant.",
                    DeepSeekOptions("""{"name":"guide"}""")
                ),
                new UserModelMessage("Hello")
                {
                    ProviderOptions = DeepSeekOptions("""{"name":"alice"}"""),
                },
                new AssistantModelMessage("Hello, Alice.", null, null)
                {
                    ProviderOptions = DeepSeekOptions("""{"name":"assistant"}"""),
                },
                new UserModelMessage("How are you?")
                {
                    ProviderOptions = DeepSeekOptions("""{"name":"alice"}"""),
                },
            },
        };

        await Model(capture, "deepseek-chat").DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            RequestBody(capture),
            """
            {
              "messages": [
                { "content": "You are a helpful assistant.", "name": "guide", "role": "system" },
                { "content": "Hello", "name": "alice", "role": "user" },
                { "content": "Hello, Alice.", "name": "assistant", "role": "assistant" },
                { "content": "How are you?", "name": "alice", "role": "user" }
              ],
              "model": "deepseek-chat"
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass providerOptions userId as user_id",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_providerOptions_userId_as_user_id()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.ProviderOptions = DeepSeekOptions("""{"userId":"tenant_123-user"}""");

        await Model(capture, "deepseek-chat").DoGenerateAsync(options, CancellationToken.None);

        AssertMatchObject(RequestBody(capture), """{"user_id":"tenant_123-user"}""");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit user_id when userId is not provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_omit_user_id_when_userId_is_not_provided()
    {
        var capture = JsonFixture("deepseek-text");

        await Model(capture, "deepseek-chat").DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.False(RequestBody(capture).ContainsKey("user_id"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject strict tools on the standard endpoint before fetching",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_strict_tools_on_the_standard_endpoint_before_fetching()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "getWeather",
                null,
                UpstreamChat.Json("""{"type":"object","properties":{}}"""),
                strict: true
            ),
        };

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            Model(capture, "deepseek-chat").DoGenerateAsync(options, CancellationToken.None)
        );

        Assert.Contains(
            "DeepSeek strict tool calls require a beta base URL ending in `/beta`.",
            error.Message
        );
        Assert.Empty(capture.Requests);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send all-strict tools on the beta endpoint",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_all_strict_tools_on_the_beta_endpoint()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "getWeather",
                null,
                UpstreamChat.Json("""{"type":"object","properties":{}}"""),
                strict: true
            ),
        };

        await BetaModel(capture, "deepseek-chat").DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(
            "https://api.deepseek.com/beta/chat/completions",
            capture.Requests[0].Uri!.ToString()
        );
        AssertMatchObject(
            RequestBody(capture),
            """{"tools":[{"type":"function","function":{"name":"getWeather","strict":true}}]}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject mixed strict tools in streaming requests",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_mixed_strict_tools_in_streaming_requests()
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "strictTool",
                null,
                UpstreamChat.Json("""{"type":"object","properties":{}}"""),
                strict: true
            ),
            new LanguageModelTool(
                "nonStrictTool",
                null,
                UpstreamChat.Json("""{"type":"object","properties":{}}""")
            ),
        };

        var error = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            BetaModel(capture, "deepseek-chat").DoStreamAsync(options, CancellationToken.None)
        );

        Assert.Contains(
            "DeepSeek strict mode requires every function tool in the request to set `strict: true`.",
            error.Message
        );
        Assert.Empty(capture.Requests);
    }

    [Theory]
    [MemberData(nameof(InvalidUserIds))]
    [UpstreamTest(
        Prefix + "should reject invalid userId %j before making an API request",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_invalid_userId_before_making_an_API_request(
        string userId,
        string validationMessage
    )
    {
        var capture = JsonFixture("deepseek-text");
        var options = TestPrompt();
        options.ProviderOptions = DeepSeekOptions(
            "{\"userId\":" + JsonSerializer.Serialize(userId) + "}"
        );

        var error = await Assert.ThrowsAsync<InvalidArgumentError>(() =>
            Model(capture, "deepseek-chat").DoGenerateAsync(options, CancellationToken.None)
        );

        Assert.Equal("AI_InvalidArgumentError", error.ErrorName);
        Assert.Equal("providerOptions", error.Parameter);
        Assert.Equal(
            "Invalid argument for parameter providerOptions: invalid deepseek provider options",
            error.Message
        );
        Assert.Contains(validationMessage, error.Cause!.Message);
        Assert.Empty(capture.Requests);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract text content",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_extract_text_content()
    {
        var capture = JsonFixture("deepseek-text");

        var result = await Model(capture, "deepseek-chat")
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        MatchSnapshot("deepseek-text.generate.snap.json", result, capture);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include the system fingerprint in provider metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_include_the_system_fingerprint_in_provider_metadata()
    {
        var capture = JsonFixture("deepseek-text");

        var result = await Model(capture, "deepseek-chat")
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(
            "fp_eaab8d114b_prod0820_fp8_kvcache",
            DeepSeekMetadata(result)["systemFingerprint"]!.GetValue<string>()
        );
    }

    [Theory]
    [InlineData("null")]
    [InlineData("undefined")]
    [UpstreamTest(
        Prefix + "should tolerate a %s system fingerprint",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_tolerate_a_systemFingerprint_system_fingerprint(
        string systemFingerprint
    )
    {
        var responseBody = FixtureBody("deepseek-text");
        responseBody["system_fingerprint"] = null;
        if (systemFingerprint == "undefined")
        {
            responseBody.Remove("system_fingerprint");
        }

        var capture = new UpstreamCapture { ResponseBody = responseBody.ToJsonString() };

        var result = await Model(capture, "deepseek-chat")
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.False(DeepSeekMetadata(result).ContainsKey("systemFingerprint"));
    }
}
