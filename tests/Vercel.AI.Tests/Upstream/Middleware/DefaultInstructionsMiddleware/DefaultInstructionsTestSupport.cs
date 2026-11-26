// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Middleware.DefaultInstructionsMiddleware;

internal static class DefaultInstructionsTestSupport
{
    // Maps to BASE_PARAMS.
    public static LanguageModelCallOptions BaseParams() =>
        new() { Prompt = [new UserModelMessage("Hello, world!")] };

    public static void AssertSystem(ModelMessage message, string content)
    {
        var system = Assert.IsType<SystemModelMessage>(message);
        Assert.Equal(content, system.Content);
    }

    public static void AssertUser(ModelMessage message, string text)
    {
        var user = Assert.IsType<UserModelMessage>(message);
        var part = Assert.IsType<TextContentPart>(Assert.Single(user.Content));
        Assert.Equal(text, part.Text);
    }
}
