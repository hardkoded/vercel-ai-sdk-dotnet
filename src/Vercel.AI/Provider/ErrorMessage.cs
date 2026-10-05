// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Util;

namespace Vercel.AI.Provider;

/// <summary>Formats an unknown thrown value the way <c>getErrorMessage</c> does.</summary>
public static class ErrorMessage
{
    /// <summary>
    /// Returns a display string for <paramref name="error"/>.
    /// Null and undefined become <c>unknown error</c>, strings are returned as-is,
    /// exceptions use <see cref="Exception.ToString"/>, and other values are JSON.
    /// </summary>
    public static string GetErrorMessage(object? error)
    {
        if (error == null || error is JsUndefined)
        {
            return "unknown error";
        }

        var text = error as string;
        if (text != null)
        {
            return text;
        }

        var exception = error as Exception;
        if (exception != null)
        {
            return exception.ToString();
        }

        return JsonSerializer.Serialize(error);
    }
}

/// <summary>JavaScript <c>Error</c> analog whose <see cref="ToString"/> omits the stack.</summary>
public class JsError : Exception
{
    /// <summary>Creates an error with <paramref name="message"/>.</summary>
    public JsError(string? message)
        : base(message ?? string.Empty)
    {
    }

    /// <summary>JavaScript <c>error.name</c>.</summary>
    public virtual string ErrorName
    {
        get { return "Error"; }
    }

    /// <summary>Returns <c>Name</c> or <c>Name: message</c>, matching <c>Error.prototype.toString</c>.</summary>
    public override string ToString()
    {
        if (string.IsNullOrEmpty(Message))
        {
            return ErrorName;
        }

        return ErrorName + ": " + Message;
    }
}

/// <summary>JavaScript <c>TypeError</c> analog.</summary>
public class JsTypeError : JsError
{
    /// <summary>Creates a type error.</summary>
    public JsTypeError(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public override string ErrorName
    {
        get { return "TypeError"; }
    }
}

/// <summary>JavaScript <c>RangeError</c> analog.</summary>
public class JsRangeError : JsError
{
    /// <summary>Creates a range error.</summary>
    public JsRangeError(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public override string ErrorName
    {
        get { return "RangeError"; }
    }
}
