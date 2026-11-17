// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Gateway.VercelOidc;

/// <summary>Maps to <c>VercelOidcTokenError</c> in <c>@vercel/oidc</c> <c>token-error.ts</c>.</summary>
internal sealed class VercelOidcTokenError : AiSdkException
{
    public VercelOidcTokenError(string message, Exception? cause = null)
        : base(message, cause!) { }
}
