// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Gateway.VercelOidc;

/// <summary>Maps to <c>AccessTokenMissingError</c> in <c>@vercel/oidc</c> <c>auth-errors.ts</c>.</summary>
internal sealed class AccessTokenMissingError : AiSdkException
{
    public AccessTokenMissingError()
        : base("No authentication found. Please log in with the Vercel CLI (vercel login).") { }
}

/// <summary>Maps to <c>RefreshAccessTokenFailedError</c> in <c>@vercel/oidc</c> <c>auth-errors.ts</c>.</summary>
internal sealed class RefreshAccessTokenFailedError : AiSdkException
{
    public RefreshAccessTokenFailedError(Exception? cause)
        : base("Failed to refresh authentication token.", cause!) { }

    public RefreshAccessTokenFailedError(string cause)
        : this(new InvalidOperationException(cause)) { }
}
