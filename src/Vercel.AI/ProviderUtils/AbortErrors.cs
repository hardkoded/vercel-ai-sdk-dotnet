// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Recognizes abort and timeout errors. Maps to <c>isAbortError</c>.</summary>
public static class AbortErrors
{
    /// <summary>
    /// True for errors named <c>AbortError</c>, <c>ResponseAborted</c>, or <c>TimeoutError</c>,
    /// and for <see cref="OperationCanceledException"/>.
    /// </summary>
    public static bool IsAbortError(object? error)
    {
        if (error is DomException dom)
        {
            return IsAbortName(dom.ErrorName);
        }

        if (error is JsError js)
        {
            return IsAbortName(js.ErrorName);
        }

        if (error is OperationCanceledException)
        {
            return true;
        }

        return false;
    }

    private static bool IsAbortName(string name)
    {
        return name == "AbortError" || name == "ResponseAborted" || name == "TimeoutError";
    }
}
