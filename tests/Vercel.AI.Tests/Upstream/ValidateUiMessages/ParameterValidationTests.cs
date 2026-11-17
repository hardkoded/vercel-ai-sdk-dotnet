// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.ConvertToModelMessages;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ValidateUiMessages.ValidateUiMessagesSupport;

namespace Vercel.AI.Tests.ValidateUiMessages;

/// <summary>Port of <c>validate-ui-messages.test.ts</c> &gt; <c>validateUIMessages &gt; parameter validation</c>.</summary>
[Collection("LogWarnings")]
public sealed class ParameterValidationTests : WarningsOff
{
    private const string Suite = Prefix + "validateUIMessages > parameter validation::";

    [UpstreamTest(
        Suite + "should continue to accept large valid part arrays",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    [Fact]
    public async Task Should_continue_to_accept_large_valid_part_arrays()
    {
        var parts = string.Join(
            ",",
            Enumerable.Repeat("{\"type\":\"text\",\"text\":\"hello\"}", 2500)
        );
        var json = "[{\"id\":\"1\",\"role\":\"user\",\"parts\":[" + parts + "]}]";

        AssertMessages(json, await Validate(json));
    }

    [UpstreamTest(
        Suite + "should throw InvalidArgumentError when messages parameter is null",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_throw_InvalidArgumentError_when_messages_parameter_is_null()
    {
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            Ai.ValidateUIMessagesAsync(new ValidateUIMessagesOptions { Messages = null })
        );

        Assert.Equal(
            "Invalid argument for parameter messages: messages parameter must be provided",
            error.Message
        );
    }

    [UpstreamTest(
        Suite + "should throw InvalidArgumentError when messages parameter is undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_throw_InvalidArgumentError_when_messages_parameter_is_undefined()
    {
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            Ai.ValidateUIMessagesAsync(new ValidateUIMessagesOptions())
        );

        Assert.Equal(
            "Invalid argument for parameter messages: messages parameter must be provided",
            error.Message
        );
    }

    [UpstreamTest(
        Suite + "should throw TypeValidationError when messages array is empty",
        Note = "Upstream's message carries a zod issue list. The port's message carries the issue text."
    )]
    [Fact]
    public async Task Should_throw_TypeValidationError_when_messages_array_is_empty()
    {
        var error = await Assert.ThrowsAsync<TypeValidationException>(() => Validate("[]"));

        Assert.StartsWith("Type validation failed: Value: [].\nError message: ", error.Message);
        Assert.Contains("Messages array must not be empty", error.Message);
    }

    [UpstreamTest(
        Suite + "should throw TypeValidationError when message has empty parts array",
        Note = "Upstream's message carries a zod issue list. The port's message carries the issue text."
    )]
    [Fact]
    public async Task Should_throw_TypeValidationError_when_message_has_empty_parts_array()
    {
        var error = await Assert.ThrowsAsync<TypeValidationException>(() =>
            Validate("[{\"id\":\"1\",\"role\":\"user\",\"parts\":[]}]")
        );

        Assert.StartsWith(
            "Type validation failed: Value: [{\"id\":\"1\",\"role\":\"user\",\"parts\":[]}].\nError message: ",
            error.Message
        );
        Assert.Contains("Message must contain at least one part", error.Message);
    }

    [UpstreamTest(
        Suite + "should validate chat ending with assistant message with empty parts array",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_validate_chat_ending_with_assistant_message_with_empty_parts_array()
    {
        const string json =
            "[{\"id\":\"1\",\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"text\":\"Hello\"}]},{\"id\":\"2\",\"role\":\"assistant\",\"parts\":[]}]";

        AssertMessages(json, await Validate(json));
    }
}
