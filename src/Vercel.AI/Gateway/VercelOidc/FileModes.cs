// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway.VercelOidc;

/// <summary>.NET-only helper for the owner-only file modes <c>@vercel/oidc</c> sets through <c>fs</c>.</summary>
internal static class FileModes
{
    /// <summary>
    /// Writes a file with mode 0600, or 0660 when <paramref name="groupAccess"/> is set, on platforms that have file modes.
    /// </summary>
    public static void WriteRestricted(string path, string content, bool groupAccess = false)
    {
#if NET
        if (!OperatingSystem.IsWindows())
        {
            var mode =
                UnixFileMode.UserRead
                | UnixFileMode.UserWrite
                | (groupAccess ? UnixFileMode.GroupRead | UnixFileMode.GroupWrite : 0);
            var options = new FileStreamOptions
            {
                Mode = FileMode.Create,
                Access = FileAccess.Write,
                UnixCreateMode = mode,
            };
            using (var stream = new FileStream(path, options))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(content);
            }

            if (groupAccess)
            {
                File.SetUnixFileMode(path, mode);
            }

            return;
        }
#endif
        File.WriteAllText(path, content);
    }

    /// <summary>Sets mode 0770 on a directory this code created, on platforms that have file modes.</summary>
    public static void TryRestrictDirectory(string path)
    {
#if NET
        if (!OperatingSystem.IsWindows())
        {
            try
            {
                File.SetUnixFileMode(
                    path,
                    UnixFileMode.UserRead
                        | UnixFileMode.UserWrite
                        | UnixFileMode.UserExecute
                        | UnixFileMode.GroupRead
                        | UnixFileMode.GroupWrite
                        | UnixFileMode.GroupExecute
                );
            }
            catch (Exception exception)
                when (exception is IOException or UnauthorizedAccessException)
            {
                // Best effort, as with fs.mkdirSync's mode.
            }
        }
#endif
    }
}
