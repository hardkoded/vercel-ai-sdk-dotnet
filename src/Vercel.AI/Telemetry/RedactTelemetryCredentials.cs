// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI;

/// <summary>
/// Applies credential redaction on telemetry-only copies. Maps to <c>redactTelemetryCredentials</c> in
/// <c>telemetry/redact-telemetry-credentials.ts</c>. Only the download error rule is ported so far.
/// </summary>
internal static class RedactTelemetryCredentialsHelper
{
    private const string Redacted = "[redacted]";

    /// <summary>Replaces a download error with a copy that has no URL. Any other error is returned as is.</summary>
    internal static Exception RedactTelemetryCredentials(Exception error)
    {
        return error is DownloadError download
            ? new DownloadError(
                Redacted,
                "Failed to download a file.",
                download.StatusCode,
                download.StatusText
            )
            : error;
    }
}
