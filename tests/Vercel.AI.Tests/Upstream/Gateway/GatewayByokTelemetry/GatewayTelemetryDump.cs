// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayByokTelemetry;

/// <summary>Stands in for <c>JSON.stringify(error)</c>: writes every message, cause and request body the error exposes.</summary>
internal static class GatewayTelemetryDump
{
    /// <summary>Stands in for <c>JSON.stringify(event)</c>: the whole event, including its error.</summary>
    internal static string Dump(TelemetryErrorEvent telemetryEvent)
    {
        return string.Join(
            "\n",
            telemetryEvent.CallId,
            telemetryEvent.AiVersion,
            telemetryEvent.FunctionId,
            Dump(telemetryEvent.Error)
        );
    }

    internal static string Dump(Exception error)
    {
        var text = new StringBuilder();
        for (var current = error; current != null; current = current.InnerException)
        {
            text.AppendLine(current.Message);
            if (current is GatewayError { Cause: { } cause })
            {
                text.AppendLine(Dump(cause));
            }

            var body = current switch
            {
                ApiException api => api.RequestBodyValues,
                Vercel.AI.Util.ApiCallError call => call.RequestBodyValues,
                _ => null,
            };
            text.AppendLine(
                body switch
                {
                    null => string.Empty,
                    TelemetryRedactedValue annotated => annotated.Value.GetRawText(),
                    _ => JsonSerializer.Serialize(body),
                }
            );
        }

        return text.ToString();
    }
}
