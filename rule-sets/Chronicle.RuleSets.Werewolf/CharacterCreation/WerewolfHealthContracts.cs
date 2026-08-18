namespace Chronicle.RuleSets.Werewolf.CharacterCreation;

public enum WerewolfDamageCategory
{
    Bashing,
    Lethal,
    Aggravated
}

public sealed record WerewolfDamageMark(
    WerewolfDamageCategory Category,
    int Amount);

public enum WerewolfHealthLevelName
{
    Escoriado,
    Machucado,
    Ferido,
    FeridoGravemente,
    Espancado,
    Aleijado,
    Incapacitado
}

public sealed record WerewolfHealthLevelDefinition(
    WerewolfHealthLevelName Name,
    int Penalty,
    string MovementEffect);

public static class WerewolfHealthLevelDefinitions
{
    public static readonly IReadOnlyList<WerewolfHealthLevelDefinition> All = [
        new(WerewolfHealthLevelName.Escoriado, 0, "Levemente escoriado; sem penalidades."),
        new(WerewolfHealthLevelName.Machucado, -1, "Machucado superficialmente; sem dificuldades de movimentação."),
        new(WerewolfHealthLevelName.Ferido, -1, "Movimentação moderadamente inibida (metade da velocidade máxima de corrida)."),
        new(WerewolfHealthLevelName.FeridoGravemente, -2, "Incapaz de correr ou de se deslocar e atacar no mesmo turno."),
        new(WerewolfHealthLevelName.Espancado, -2, "Gravemente ferido; consegue apenas mancar (cerca de 3 metros por turno)."),
        new(WerewolfHealthLevelName.Aleijado, -5, "Desastrosamente ferido; consegue apenas se arrastar (cerca de 1 metro por turno)."),
        new(WerewolfHealthLevelName.Incapacitado, 0, "Incapaz de se movimentar; provavelmente inconsciente. Receber mais um nível de dano resulta em morte.")
    ];

    public static int Count => All.Count;

    public static WerewolfHealthLevelDefinition Get(WerewolfHealthLevelName name) =>
        All.First(d => d.Name == name);
}
