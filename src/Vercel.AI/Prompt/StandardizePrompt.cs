// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Prompt;

/// <summary>
/// Instructions for a call: a string, one system message, or a list of system messages.
/// Maps to <c>Instructions</c>. A single system message is kept as a one-item <see cref="Messages"/> list.
/// </summary>
public sealed class Instructions
{
    private Instructions(string? text, IReadOnlyList<SystemModelMessage>? messages)
    {
        Text = text;
        Messages = messages;
    }

    /// <summary>The instruction text, when the instructions are a string.</summary>
    public string? Text { get; }

    /// <summary>The system messages, when the instructions are one or more system messages.</summary>
    public IReadOnlyList<SystemModelMessage>? Messages { get; }

    /// <summary>Creates instructions from a string.</summary>
    public static Instructions From(string text)
    {
        return new Instructions(text ?? throw new ArgumentNullException(nameof(text)), null);
    }

    /// <summary>Creates instructions from one system message.</summary>
    public static Instructions From(SystemModelMessage message)
    {
        return new Instructions(null, new[] { message ?? throw new ArgumentNullException(nameof(message)) });
    }

    /// <summary>Creates instructions from a list of system messages.</summary>
    public static Instructions From(IReadOnlyList<SystemModelMessage> messages)
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        if (messages.Any(message => message is null))
        {
            throw new InvalidPromptException("instructions must be a string, SystemModelMessage, or array of SystemModelMessage");
        }

        return new Instructions(null, messages.ToArray());
    }

    /// <summary>Converts a string to instructions.</summary>
    public static implicit operator Instructions(string text)
    {
        return From(text);
    }

    /// <summary>Converts a system message to instructions.</summary>
    public static implicit operator Instructions(SystemModelMessage message)
    {
        return From(message);
    }

    /// <summary>Converts an array of system messages to instructions.</summary>
    public static implicit operator Instructions(SystemModelMessage[] messages)
    {
        return From(messages);
    }
}

/// <summary>The prompt of a call: a user string or a list of messages. Maps to <c>prompt: string | ModelMessage[]</c>.</summary>
public sealed class PromptInput
{
    private PromptInput(string? text, IReadOnlyList<ModelMessage>? messages)
    {
        Text = text;
        Messages = messages;
    }

    /// <summary>The user text, when the prompt is a string.</summary>
    public string? Text { get; }

    /// <summary>The messages, when the prompt is a message list.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; }

    /// <summary>Creates a prompt from a user string.</summary>
    public static PromptInput From(string text)
    {
        return new PromptInput(text ?? throw new ArgumentNullException(nameof(text)), null);
    }

    /// <summary>Creates a prompt from a message list.</summary>
    public static PromptInput From(IReadOnlyList<ModelMessage> messages)
    {
        return new PromptInput(null, messages ?? throw new ArgumentNullException(nameof(messages)));
    }

    /// <summary>Converts a string to a prompt.</summary>
    public static implicit operator PromptInput(string text)
    {
        return From(text);
    }

    /// <summary>Converts an array of messages to a prompt.</summary>
    public static implicit operator PromptInput(ModelMessage[] messages)
    {
        return From(messages);
    }
}

/// <summary>A standardized prompt. Maps to the result of <c>standardizePrompt</c>.</summary>
public sealed class StandardizedPrompt
{
    /// <summary>Creates a standardized prompt.</summary>
    public StandardizedPrompt(Instructions? instructions, IReadOnlyList<ModelMessage> messages)
    {
        Instructions = instructions;
        Messages = messages ?? Array.Empty<ModelMessage>();
    }

    /// <summary>Instructions, or null when there are none.</summary>
    public Instructions? Instructions { get; }

    /// <summary>User and assistant messages, plus system messages when they are allowed.</summary>
    public IReadOnlyList<ModelMessage> Messages { get; }
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

    /// <summary>Instructions. They win over <see cref="System"/>.</summary>
    public Instructions? Instructions { get; set; }

    /// <summary>A user string or a message list. Cannot be combined with <see cref="Messages"/>.</summary>
    public PromptInput? Prompt { get; set; }

    /// <summary>Message list. Cannot be combined with <see cref="Prompt"/>.</summary>
    public IReadOnlyList<ModelMessage>? Messages { get; set; }
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

        var instructions = input.Instructions ?? (input.System != null ? Instructions.From(input.System) : null);

        IReadOnlyList<ModelMessage> messages;
        if (input.Prompt?.Text is { } text)
        {
            messages = new ModelMessage[] { new UserModelMessage(text) };
        }
        else if (input.Prompt?.Messages is { } promptMessages)
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

        if (!input.AllowSystemInMessages && messages.Any(message => message is SystemModelMessage))
        {
            throw new InvalidPromptException("System messages are not allowed in the prompt or messages fields. Use the instructions option instead.");
        }

        return new StandardizedPrompt(instructions, messages);
    }
}
