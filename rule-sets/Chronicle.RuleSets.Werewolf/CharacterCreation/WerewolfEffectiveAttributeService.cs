namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

using System.Collections.ObjectModel;

public static class WerewolfEffectiveAttributeService
{
    public static IReadOnlyDictionary<string, int> ComputeEffectiveAttributes(
        IReadOnlyDictionary<string, int?> baseAttributes,
        string currentForm)
    {
        ArgumentNullException.ThrowIfNull(baseAttributes);
        ArgumentNullException.ThrowIfNull(currentForm);

        var formDefinition = WerewolfFormCatalog.Entries.FirstOrDefault(f => StringComparer.Ordinal.Equals(f.FormId, currentForm));
        if (formDefinition is null)
        {
            return new Dictionary<string, int>(StringComparer.Ordinal);
        }

        var effective = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var attributeId in WerewolfAttributeIdentifiers.Supported)
        {
            var baseValue = baseAttributes.TryGetValue(attributeId, out var value) ? value ?? 0 : 0;
            var modifier = formDefinition.AttributeModifiers.FirstOrDefault(m => StringComparer.Ordinal.Equals(m.AttributeId, attributeId));
            var effectiveValue = modifier is null
                ? baseValue
                : modifier.IsAbsolute
                    ? modifier.Value
                    : baseValue + modifier.Value;
            effective[attributeId] = Math.Max(0, effectiveValue);
        }

        return new ReadOnlyDictionary<string, int>(effective);
    }

    public static int GetEffectiveAttribute(
        IReadOnlyDictionary<string, int?> baseAttributes,
        string currentForm,
        string attributeId)
    {
        ArgumentNullException.ThrowIfNull(baseAttributes);
        ArgumentNullException.ThrowIfNull(currentForm);
        ArgumentNullException.ThrowIfNull(attributeId);

        var effective = ComputeEffectiveAttributes(baseAttributes, currentForm);
        return effective.TryGetValue(attributeId, out var value) ? value : 0;
    }
}
