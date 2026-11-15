// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.GladiaTranscriptionModel;

internal static class GladiaFixtures
{
    public const string Text = "Galileo was an American robotic space program that studied the planet Jupiter and its moons, as well as several other solar system bodies. Named after the Italian astronomer Galileo Galilei, the Galileo spacecraft consisted of an orbiter and an atmospheric entry probe. It was delivered into Earth orbit on October 18, 1989, by Space Shuttle Atlantis on the STS-34 mission. and arrived at Jupiter on December 7, 1995, after gravity-assist flybys of Venus and Earth, and became the first spacecraft to orbit Jupiter.";

    public static AudioInput Audio()
    {
        return new AudioInput(new byte[] { 1, 2, 3, 4 }, "audio/wav", "audio.wav");
    }

    public static JsonObject Result(bool metadata = false)
    {
        var speaker = metadata ? "\"speaker\":\"speaker-1\"" : "\"speaker\":1";
        return JsonNode.Parse("{\"status\":\"done\",\"result\":{\"metadata\":{\"audio_duration\":36.74},\"transcription\":{\"full_transcript\":" + JsonSerializer.Serialize(Text) + ",\"languages\":[\"en\"],\"utterances\":["
            + "{\"text\":\"Galileo was an American robotic space program that studied the planet Jupiter and its moons,\",\"start\":0.14,\"end\":5.341,\"speaker\":0,\"confidence\":0.91,\"language\":\"en\",\"words\":[{\"word\":\"Galileo\"}]},"
            + "{\"text\":\"as well as several other solar system bodies.\",\"start\":5.662,\"end\":8.099," + speaker + "},"
            + "{\"text\":\"Named after the Italian astronomer Galileo Galilei,\",\"start\":9.138,\"end\":12.122},"
            + "{\"text\":\"the Galileo spacecraft consisted of an orbiter and an atmospheric entry probe.\",\"start\":12.419,\"end\":17.248},"
            + "{\"text\":\"It was delivered into Earth orbit on October 18,\",\"start\":18.185,\"end\":20.919},"
            + "{\"text\":\"1989,\",\"start\":20.92,\"end\":22.498},"
            + "{\"text\":\"by Space Shuttle Atlantis on the STS-34 mission.\",\"start\":22.499,\"end\":25.966},"
            + "{\"text\":\"and arrived at Jupiter on December 7,\",\"start\":26.4,\"end\":28.525},"
            + "{\"text\":\"1995,\",\"start\":28.526,\"end\":29.847},"
            + "{\"text\":\"after gravity-assist flybys of Venus and Earth,\",\"start\":30.247,\"end\":32.851},"
            + "{\"text\":\"and became the first spacecraft to orbit Jupiter.\",\"start\":33.132,\"end\":35.816}"
            + "]}}}")!.AsObject();
    }

    public static ParityHandler Handler(bool foreign = false, bool metadata = false, JsonObject? result = null)
    {
        var resultUrl = foreign
            ? "https://cdn.evil.example/v2/pre-recorded/result"
            : "https://api.gladia.io/v2/pre-recorded/7b0137fd-5fd6-4d08-b9bf-4cdf9876797d";
        var resultBody = (result ?? Result(metadata)).ToJsonString();
        return new ParityHandler(call =>
        {
            if (call.Uri.AbsolutePath.EndsWith("/upload", StringComparison.Ordinal))
            {
                return ParityHandler.Json("{\"audio_url\":\"https://api.gladia.io/file/7403f025-be30-4335-ae29-20131e0adbd6\"}");
            }

            if (call.Method == HttpMethod.Post)
            {
                return ParityHandler.Json("{\"id\":\"7b0137fd-5fd6-4d08-b9bf-4cdf9876797d\",\"result_url\":\"" + resultUrl + "\"}");
            }

            return ParityHandler.Json(resultBody);
        });
    }
}
