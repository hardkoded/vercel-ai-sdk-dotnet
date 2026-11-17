// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using System.Runtime.InteropServices;

namespace Vercel.AI.Gateway.VercelOidc;

/// <summary>.NET-only helper for the owner-only file modes <c>@vercel/oidc</c> sets through <c>fs</c>.</summary>
internal static class FileModes
{
    private const int UserRead = 0x100;
    private const int UserWrite = 0x80;
    private const int UserExecute = 0x40;
    private const int GroupRead = 0x20;
    private const int GroupWrite = 0x10;
    private const int GroupExecute = 0x8;

    /// <summary>
    /// Writes a file with mode 0600, or 0660 when <paramref name="groupAccess"/> is set, on platforms that have file modes.
    /// The content goes to a new file with a unique name, which is restricted before it receives the secret and then replaces the target.
    /// </summary>
    public static void WriteRestricted(string path, string content, bool groupAccess = false)
    {
        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temp, "");
            TrySetMode(temp, UserRead | UserWrite | (groupAccess ? GroupRead | GroupWrite : 0));
            File.WriteAllText(temp, content);
#if NET
            File.Move(temp, path, overwrite: true);
#else
            if (File.Exists(path))
            {
                File.Replace(temp, path, null);
            }
            else
            {
                File.Move(temp, path);
            }
#endif
        }
        catch
        {
            try
            {
                File.Delete(temp);
            }
            catch (Exception exception)
                when (exception is IOException or UnauthorizedAccessException)
            {
                // The original failure is the one to report.
            }

            throw;
        }
    }

    /// <summary>Sets mode 0770 on a directory this code created, on platforms that have file modes.</summary>
    public static void TryRestrictDirectory(string path) =>
        TrySetMode(
            path,
            UserRead | UserWrite | UserExecute | GroupRead | GroupWrite | GroupExecute
        );

    /// <summary>
    /// Sets a Unix mode. Does nothing on Windows, and on runtimes older than .NET 7 that have no <c>File.SetUnixFileMode</c>.
    /// </summary>
    private static void TrySetMode(string path, int mode)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

#if NET
        File.SetUnixFileMode(path, (UnixFileMode)mode);
#else
        var modeType = typeof(File).Assembly.GetType("System.IO.UnixFileMode");
        var method = modeType is null
            ? null
            : typeof(File).GetMethod("SetUnixFileMode", [typeof(string), modeType]);
        method?.Invoke(null, [path, Enum.ToObject(modeType!, mode)]);
#endif
    }
}
