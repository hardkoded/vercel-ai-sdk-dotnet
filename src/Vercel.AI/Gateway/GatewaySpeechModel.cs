// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway;

/// <summary>
/// Gateway streaming speech model. Maps to <c>GatewaySpeechModel</c> in <c>gateway-speech-model.ts</c>. Only the URL
/// helper used by <c>experimental_speech.getToken</c> is ported here; the model itself is tracked in #752.
/// </summary>
public sealed class GatewaySpeechModel
{
    private GatewaySpeechModel() { }

    /// <summary>
    /// Gateway streaming speech WebSocket URL: the HTTP(S) base upgraded to WS(S), with the model id in
    /// <c>?ai-model-id=</c> (slash-safe for qualified ids like <c>elevenlabs/eleven-flash-v2.5</c>). Maps to <c>toGatewaySpeechUrl</c>.
    /// </summary>
    public static string ToGatewaySpeechUrl(string baseUrl, string modelId)
    {
        var trimmed = baseUrl.TrimEnd('/');
        var wsBase = trimmed.StartsWith("http", StringComparison.Ordinal)
            ? "ws" + trimmed.Substring(4)
            : trimmed;
        return new Uri(wsBase + "/speech-model").AbsoluteUri
            + "?ai-model-id="
            + GatewayRealtimeModel.FormEncode(modelId);
    }
}
