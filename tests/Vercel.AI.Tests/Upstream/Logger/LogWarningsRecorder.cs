// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using WarningLog = Vercel.AI.Util.LogWarnings;

namespace Vercel.AI.Tests.Upstream.Logger;

/// <summary>Replaces the process-wide warning emitters for one test and restores them on dispose.</summary>
internal sealed class LogWarningsRecorder : IDisposable
{
    public LogWarningsRecorder(object? logger = null)
    {
        WarningLog.ResetState();
        WarningLog.Logger = logger;
        WarningLog.ProcessEmitWarning = delegate (string message, string type, string? code) { Process.Add((message, type, code)); };
        WarningLog.ConsoleWarn = delegate (string message) { Console.Add(message); };
    }

    public List<(string Message, string Type, string? Code)> Process { get; } = new List<(string, string, string?)>();

    public List<string> Console { get; } = new List<string>();

    public void Dispose()
    {
        WarningLog.Logger = null;
        WarningLog.ProcessEmitWarning = delegate { };
        WarningLog.ConsoleWarn = delegate { };
        WarningLog.ResetState();
    }
}
