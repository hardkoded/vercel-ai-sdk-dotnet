// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Host environment used by user-agent detection.</summary>
public sealed class RuntimeGlobals
{
    /// <summary>Browser window, when present. A non-null value means the runtime is a browser.</summary>
    public object? Window { get; set; }

    /// <summary>Navigator user agent, when <c>navigator.userAgent</c> exists.</summary>
    public string? NavigatorUserAgent { get; set; }

    /// <summary>True when <c>navigator</c> exists, even if the user agent is empty.</summary>
    public bool HasNavigator { get; set; }

    /// <summary><c>process.versions.node</c>, when present.</summary>
    public string? NodeVersion { get; set; }

    /// <summary><c>process.version</c>.</summary>
    public string? ProcessVersion { get; set; }

    /// <summary>True when <c>EdgeRuntime</c> is set.</summary>
    public bool EdgeRuntime { get; set; }
}

/// <summary>Runtime user-agent fragments. Maps to <c>getRuntimeEnvironmentUserAgent</c> and <c>isBrowserRuntime</c>.</summary>
public static class RuntimeEnvironmentInfo
{
    /// <summary>Returns a <c>runtime/...</c> user-agent fragment for <paramref name="globals"/>.</summary>
    public static string GetRuntimeEnvironmentUserAgent(RuntimeGlobals? globals = null)
    {
        if (globals is null)
        {
            return "runtime/unknown";
        }

        if (globals.Window != null && globals.Window is not JsUndefined)
        {
            return "runtime/browser";
        }

        if (globals.HasNavigator && globals.NavigatorUserAgent != null)
        {
            return "runtime/" + globals.NavigatorUserAgent.ToLowerInvariant();
        }

        if (globals.NodeVersion != null)
        {
            return "runtime/node.js/" + (globals.ProcessVersion ?? string.Empty);
        }

        if (globals.EdgeRuntime)
        {
            return "runtime/vercel-edge";
        }

        return "runtime/unknown";
    }

    /// <summary>True when <paramref name="globals"/> has a window.</summary>
    public static bool IsBrowserRuntime(RuntimeGlobals? globals = null)
    {
        return globals?.Window != null && globals.Window is not JsUndefined;
    }
}
