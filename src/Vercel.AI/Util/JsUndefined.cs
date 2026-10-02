// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>
/// Sentinel for a JavaScript <c>undefined</c> value. Distinct from <c>null</c>.
/// </summary>
public sealed class JsUndefined
{
    private JsUndefined()
    {
    }

    /// <summary>The single undefined value.</summary>
    public static JsUndefined Value { get; } = new JsUndefined();
}
