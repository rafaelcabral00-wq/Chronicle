using System.Text.Json;
using System.Text.Json.Serialization;
using Chronicle.Domain.Campaign;


namespace Chronicle.Application.Campaign;

/// <summary>
/// Serialises a <see cref="CampaignAggregate"/> to and from a JSON
/// payload suitable for the E1 <c>Document</c> model. The aggregate
/// itself remains persistence-agnostic; this is the Application-side
/// mapping boundary.
/// </summary>
public static class CampaignSerializer
{
    public const string ContentType = "campaign-aggregate/v1";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serialises the aggregate state to a JSON string.
    /// </summary>
    public static string Serialize(CampaignState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return JsonSerializer.Serialize(state, Options);
    }

    /// <summary>
    /// Rehydrates a <see cref="CampaignState"/> from a JSON payload.
    /// </summary>
    public static CampaignState Deserialize(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            throw new ArgumentException("Payload JSON must not be empty.", nameof(payloadJson));
        }
        var state = JsonSerializer.Deserialize<CampaignState>(payloadJson, Options)
            ?? throw new InvalidOperationException("Campaign payload deserialised to null.");
        return state;
    }
}