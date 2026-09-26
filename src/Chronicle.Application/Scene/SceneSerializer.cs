using System.Text.Json;
using System.Text.Json.Serialization;
using Chronicle.Domain.Scene;


namespace Chronicle.Application.Scene;

/// <summary>
/// Serialises a <see cref="SceneAggregate"/> to and from a JSON
/// payload suitable for the E1 <c>Document</c> model. The aggregate
/// itself remains persistence-agnostic; this is the Application-side
/// mapping boundary.
/// </summary>
public static class SceneSerializer
{
    public const string ContentType = "scene-aggregate/v1";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serialises the aggregate state to a JSON string.
    /// </summary>
    public static string Serialize(SceneState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return JsonSerializer.Serialize(state, Options);
    }

    /// <summary>
    /// Rehydrates a <see cref="SceneState"/> from a JSON payload.
    /// </summary>
    public static SceneState Deserialize(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            throw new ArgumentException("Payload JSON must not be empty.", nameof(payloadJson));
        }
        var state = JsonSerializer.Deserialize<SceneState>(payloadJson, Options)
            ?? throw new InvalidOperationException("Scene payload deserialised to null.");
        return state;
    }
}