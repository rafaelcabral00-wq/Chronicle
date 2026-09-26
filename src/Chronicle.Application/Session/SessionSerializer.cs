using System.Text.Json;
using System.Text.Json.Serialization;
using Chronicle.Domain.Session;


namespace Chronicle.Application.Session;

/// <summary>
/// Serialises a <see cref="SessionAggregate"/> to and from a JSON
/// payload suitable for the E1 <c>Document</c> model. The aggregate
/// itself remains persistence-agnostic; this is the Application-side
/// mapping boundary.
/// </summary>
public static class SessionSerializer
{
    public const string ContentType = "session-aggregate/v1";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serialises the aggregate state to a JSON string.
    /// </summary>
    public static string Serialize(SessionState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return JsonSerializer.Serialize(state, Options);
    }

    /// <summary>
    /// Rehydrates a <see cref="SessionState"/> from a JSON payload.
    /// </summary>
    public static SessionState Deserialize(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            throw new ArgumentException("Payload JSON must not be empty.", nameof(payloadJson));
        }
        var state = JsonSerializer.Deserialize<SessionState>(payloadJson, Options)
            ?? throw new InvalidOperationException("Session payload deserialised to null.");
        return state;
    }
}