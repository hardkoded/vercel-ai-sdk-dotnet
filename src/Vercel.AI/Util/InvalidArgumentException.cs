// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>
/// An argument failed validation. Maps to upstream <c>InvalidArgumentError</c>
/// and derives from <see cref="ArgumentException"/>.
/// </summary>
public sealed class InvalidArgumentException : ArgumentException
{
    private readonly string _message;

    /// <summary>Creates an exception for <paramref name="parameter"/>.</summary>
    public InvalidArgumentException(string parameter, string message)
        : this(parameter, null, message)
    {
    }

    /// <summary>Creates an exception that also records the rejected <paramref name="value"/>.</summary>
    public InvalidArgumentException(string parameter, object? value, string message)
        : base(message, parameter)
    {
        Parameter = parameter;
        Value = value;
        _message = "Invalid argument for parameter " + parameter + ": " + message;
    }

    /// <summary>Upstream message, without the runtime parameter suffix.</summary>
    public override string Message
    {
        get { return _message; }
    }

    /// <summary>The parameter name from the upstream error.</summary>
    public string Parameter { get; }

    /// <summary>The rejected value, when the caller supplied one.</summary>
    public object? Value { get; }

    /// <summary>Returns whether <paramref name="error"/> is this exception type.</summary>
    public static bool IsInstance(object? error)
    {
        return error is InvalidArgumentException;
    }
}
