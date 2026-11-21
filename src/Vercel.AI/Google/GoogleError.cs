// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Google;

/// <summary>Port of <c>google-error.ts</c>.</summary>
internal static class GoogleError
{
    /// <summary>Google RPC status names can be sent without a numeric HTTP error code.</summary>
    internal static int? MapGoogleErrorStatusToHttpStatusCode(string? status) =>
        status switch
        {
            "CANCELLED" => 499,
            "INVALID_ARGUMENT" or "FAILED_PRECONDITION" or "OUT_OF_RANGE" => 400,
            "DEADLINE_EXCEEDED" => 504,
            "NOT_FOUND" => 404,
            "ALREADY_EXISTS" or "ABORTED" => 409,
            "PERMISSION_DENIED" => 403,
            "UNAUTHENTICATED" => 401,
            "RESOURCE_EXHAUSTED" => 429,
            "UNIMPLEMENTED" => 501,
            "UNKNOWN" or "INTERNAL" or "DATA_LOSS" => 500,
            "UNAVAILABLE" => 503,
            _ => null,
        };
}
