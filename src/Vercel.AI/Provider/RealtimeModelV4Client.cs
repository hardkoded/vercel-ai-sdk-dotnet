// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Provider;

/// <summary>
/// The client-side codec of a realtime model. Maps to <c>parseServerEvent</c>, <c>createServerEventParser</c>,
/// <c>serializeClientEvent</c> and <c>getHealthCheckResponse</c> of <c>RealtimeModelV4</c>. It is a separate
/// interface because <c>netstandard2.0</c> has no default interface members.
/// </summary>
public interface IClientRealtimeModel : IRealtimeModel
{
    /// <summary>Parses a raw JSON event into one or more normalized events. Maps to <c>parseServerEvent</c>.</summary>
    IReadOnlyList<RealtimeModelV4ServerEvent> ParseServerEvent(JsonElement raw);

    /// <summary>
    /// Creates a raw-event parser per connection, to discard on disconnect. Returns null when the model has none.
    /// Maps to <c>createServerEventParser</c>.
    /// </summary>
    Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>>? CreateServerEventParser();

    /// <summary>Serializes a normalized client event into the provider wire format. Maps to <c>serializeClientEvent</c>.</summary>
    Task<JsonNode?> SerializeClientEventAsync(
        RealtimeModelV4ClientEvent clientEvent,
        RealtimeModelV4SessionIntent? intent = null
    );

    /// <summary>
    /// Returns a message to send back for a raw incoming message, or null when none is needed. Maps to
    /// <c>getHealthCheckResponse</c>.
    /// </summary>
    JsonNode? GetHealthCheckResponse(JsonElement raw);
}
