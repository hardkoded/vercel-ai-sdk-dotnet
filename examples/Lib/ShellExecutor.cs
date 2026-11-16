// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text;

namespace Examples.Lib;

/// <summary>Outcome of a shell command: a timeout or an exit code.</summary>
public abstract record ShellOutcome
{
    /// <summary>The command timed out.</summary>
    public sealed record Timeout : ShellOutcome;

    /// <summary>The command exited.</summary>
    public sealed record Exit(int ExitCode) : ShellOutcome;
}

/// <summary>Result of <see cref="ShellExecutor.ExecuteShellCommandAsync"/>.</summary>
public sealed record ShellCommandResult(string Stdout, string Stderr, ShellOutcome Outcome);

/// <summary>Port of examples/ai-functions/src/lib/shell-executor.ts.</summary>
public static class ShellExecutor
{
    private const int MaxBuffer = 10 * 1024 * 1024;

    /// <summary>Runs <paramref name="command"/> in the platform shell. Defaults to a 60 second timeout.</summary>
    public static async Task<ShellCommandResult> ExecuteShellCommandAsync(
        string command,
        int? timeoutMs = null,
        CancellationToken cancellationToken = default)
    {
        var timeout = timeoutMs ?? 60_000; // Default 60 seconds

        var startInfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add(OperatingSystem.IsWindows() ? "/c" : "-c");
        startInfo.ArgumentList.Add(command);

        using var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return new ShellCommandResult(string.Empty, error.ToString(), new ShellOutcome.Exit(1));
        }

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);

        var stdout = ReadBoundedAsync(process.StandardOutput, process, timeoutSource.Token);
        var stderr = ReadBoundedAsync(process.StandardError, process, timeoutSource.Token);

        try
        {
            await process.WaitForExitAsync(timeoutSource.Token).ConfigureAwait(false);
            await Task.WhenAll(stdout, stderr).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            KillTree(process);
            return new ShellCommandResult(
                await SafeAsync(stdout).ConfigureAwait(false),
                await SafeAsync(stderr).ConfigureAwait(false),
                new ShellOutcome.Timeout());
        }
        catch (OperationCanceledException)
        {
            KillTree(process);
            throw;
        }

        return new ShellCommandResult(
            await stdout.ConfigureAwait(false),
            await stderr.ConfigureAwait(false),
            new ShellOutcome.Exit(process.ExitCode));
    }

    private static async Task<string> SafeAsync(Task<string> read)
    {
        try
        {
            return await read.ConfigureAwait(false);
        }
        catch (Exception error) when (error is OperationCanceledException or IOException)
        {
            return string.Empty;
        }
    }

    private static async Task<string> ReadBoundedAsync(StreamReader reader, Process process, CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();
        var buffer = new char[4096];
        try
        {
            int read;
            while ((read = await reader.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false)) > 0)
            {
                builder.Append(buffer, 0, read);
                if (builder.Length > MaxBuffer)
                {
                    KillTree(process);
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // The caller reports the timeout.
        }

        return builder.ToString();
    }

    private static void KillTree(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
            // Already exited.
        }
    }
}
