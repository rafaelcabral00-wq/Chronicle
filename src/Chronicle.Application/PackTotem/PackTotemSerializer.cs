using System.Text.Json;
using System.Text.Json.Serialization;
using Chronicle.Domain.PackTotem;


namespace Chronicle.Application.PackTotem;

/// <summary>
/// Serialises a <see cref="PackTotemAggregate"/> to and from a JSON
/// payload suitable for the E1 <c>Document</c> model. The aggregate
/// itself remains persistence-agnostic; this is the Application-side
/// mapping boundary.
/// </summary>
public static class PackTotemSerializer
{
    public const string ContentType = "pack-totem-aggregate/v1";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serialises the aggregate state to a JSON string.
    /// </summary>
    public static string Serialize(PackTotemState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return JsonSerializer.Serialize(state, Options);
    }

    /// <summary>
    /// Rehydrates a <see cref="PackTotemState"/> from a JSON payload.
    /// </summary>
    public static PackTotemState Deserialize(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            throw new ArgumentException("Payload JSON must not be empty.", nameof(payloadJson));
        }
        var state = JsonSerializer.Deserialize<PackTotemState>(payloadJson, Options)
            ?? throw new InvalidOperationException("Pack/Totem payload deserialised to null.");
        return state;
    }
}
