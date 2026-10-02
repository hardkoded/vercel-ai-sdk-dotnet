// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.InteropServices;

namespace Vercel.AI.Mcp;

/// <summary>Builds the environment passed to an MCP stdio child process.</summary>
public static class McpEnvironment
{
    private static readonly string[] WindowsKeys =
    {
        "APPDATA",
        "HOMEDRIVE",
        "HOMEPATH",
        "LOCALAPPDATA",
        "PATH",
        "PROCESSOR_ARCHITECTURE",
        "SYSTEMDRIVE",
        "SYSTEMROOT",
        "TEMP",
        "USERNAME",
        "USERPROFILE",
    };

    private static readonly string[] UnixKeys =
    {
        "HOME",
        "LOGNAME",
        "PATH",
        "SHELL",
        "TERM",
        "USER",
    };

    /// <summary>
    /// Copies <paramref name="customEnvironment"/> and fills in the platform variables a child process should inherit.
    /// Values that start with <c>()</c> are skipped. The input dictionary is not modified.
    /// </summary>
    public static IReadOnlyDictionary<string, string> GetEnvironment(IReadOnlyDictionary<string, string>? customEnvironment = null)
    {
        var environment = new Dictionary<string, string>(StringComparer.Ordinal);
        if (customEnvironment != null)
        {
            foreach (var pair in customEnvironment)
            {
                environment[pair.Key] = pair.Value;
            }
        }

        var inherited = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? WindowsKeys : UnixKeys;
        for (var index = 0; index < inherited.Length; index++)
        {
            var key = inherited[index];
            var value = Environment.GetEnvironmentVariable(key);
            if (value == null || value.StartsWith("()", StringComparison.Ordinal))
            {
                continue;
            }

            environment[key] = value;
        }

        return environment;
    }
}
