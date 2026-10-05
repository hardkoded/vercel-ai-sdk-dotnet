// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Prompt;

/// <summary>A prompt message used by <see cref="Prompts.StandardizePrompt"/>.</summary>
public sealed class PromptMessage
{
    /// <summary>Creates a message.</summary>
    public PromptMessage(string role, object? content)
    {
        Role = role ?? throw new ArgumentNullException(nameof(role));
        Content = content;
    }

    /// <summary>Message role.</summary>
    public string Role { get; }

    /// <summary>Message content. System messages must be a string.</summary>
    public object? Content { get; }
}

/// <summary>A standardized prompt. Maps to the result of <c>standardizePrompt</c>.</summary>
public sealed class StandardizedPrompt
{
    /// <summary>Creates a standardized prompt.</summary>
    public StandardizedPrompt(object? instructions, IReadOnlyList<PromptMessage> messages)
    {
        Instructions = instructions;
        Messages = messages ?? Array.Empty<PromptMessage>();
    }

    /// <summary>Instructions. A string, one system message, or a list of system messages.</summary>
    public object? Instructions { get; }

    /// <summary>User and assistant messages, plus system messages when they are allowed.</summary>
    public IReadOnlyList<PromptMessage> Messages { get; }
}

/// <summary>The prompt input is invalid. Maps to <c>InvalidPromptError</c>.</summary>
public sealed class InvalidPromptException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public InvalidPromptException(string message)
        : base(message)
    {
    }

    /// <summary>Stable error name.</summary>
    public string Name { get; } = "AI_InvalidPromptError";
}

/// <summary>Input for <see cref="Prompts.StandardizePrompt"/>.</summary>
public sealed class StandardizePromptInput
{
    /// <summary>When true, system messages may appear in <see cref="Prompt"/> or <see cref="Messages"/>.</summary>
    public bool AllowSystemInMessages { get; set; }

    /// <summary>Legacy system string. Used when <see cref="Instructions"/> is null.</summary>
    public string? System { get; set; }

    /// <summary>Instructions string, system message, or list of system messages.</summary>
    public object? Instructions { get; set; }

    /// <summary>True when <see cref="Instructions"/> was set, including an explicit null.</summary>
    public bool InstructionsSet { get; set; }

    /// <summary>A user string or a message list.</summary>
    public object? Prompt { get; set; }

    /// <summary>Message list. Cannot be combined with <see cref="Prompt"/>.</summary>
    public IReadOnlyList<PromptMessage>? Messages { get; set; }
}

/// <summary>Prompt normalization. Maps to <c>standardizePrompt</c>.</summary>
public static class Prompts
{
    /// <summary>Validates and normalizes a prompt into instructions plus messages.</summary>
    public static StandardizedPrompt StandardizePrompt(StandardizePromptInput input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (input.Prompt == null && input.Messages == null)
        {
            throw new InvalidPromptException("prompt or messages must be defined");
        }

        if (input.Prompt != null && input.Messages != null)
        {
            throw new InvalidPromptException("prompt and messages cannot be defined at the same time");
        }

        var instructions = input.InstructionsSet ? input.Instructions : input.System;
        ValidateInstructions(instructions);

        IReadOnlyList<PromptMessage> messages;
        if (input.Prompt is string text)
        {
            messages = new[] { new PromptMessage("user", text) };
        }
        else if (input.Prompt is IReadOnlyList<PromptMessage> promptMessages)
        {
            messages = promptMessages;
        }
        else if (input.Messages != null)
        {
            messages = input.Messages;
        }
        else
        {
            throw new InvalidPromptException("prompt or messages must be defined");
        }

        if (messages.Count == 0)
        {
            throw new InvalidPromptException("messages must not be empty");
        }

        foreach (var message in messages)
        {
            if (!string.Equals(message.Role, "system", StringComparison.Ordinal))
            {
                continue;
            }

            if (!input.AllowSystemInMessages)
            {
                throw new InvalidPromptException("System messages are not allowed in the prompt or messages fields. Use the instructions option instead.");
            }

            if (message.Content is not string)
            {
                throw new InvalidPromptException("The messages do not match the ModelMessage[] schema.");
            }
        }

        return new StandardizedPrompt(instructions, messages);
    }

    private static void ValidateInstructions(object? instructions)
    {
        if (instructions is null || instructions is string)
        {
            return;
        }

        if (instructions is PromptMessage message)
        {
            if (string.Equals(message.Role, "system", StringComparison.Ordinal))
            {
                return;
            }
        }
        else if (instructions is IReadOnlyList<PromptMessage> list)
        {
            foreach (var item in list)
            {
                if (!string.Equals(item.Role, "system", StringComparison.Ordinal))
                {
                    throw new InvalidPromptException("instructions must be a string, SystemModelMessage, or array of SystemModelMessage");
                }
            }

            return;
        }

        throw new InvalidPromptException("instructions must be a string, SystemModelMessage, or array of SystemModelMessage");
    }
}
