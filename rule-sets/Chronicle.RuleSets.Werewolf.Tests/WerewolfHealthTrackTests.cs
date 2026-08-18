using Chronicle.RuleSets.Werewolf.CharacterCreation;
using Xunit;

namespace Chronicle.RuleSets.Werewolf.Tests;

public sealed class WerewolfHealthTrackTests
{
    [Fact]
    public void ComputeEmptyDamageReturnsHealthy()
    {
        var track = WerewolfHealthTrackComputer.Compute([]);

        Assert.Equal(0, track.TotalDamage);
        Assert.Equal(0, track.WoundPenalty);
        Assert.Equal(WerewolfHealthState.Healthy, track.HealthState);
        Assert.Equal(WerewolfHealthLevelName.Escoriado, track.CurrentLevel);
    }

    [Fact]
    public void ComputeSingleBashingReturnsWounded()
    {
        var track = WerewolfHealthTrackComputer.Compute([new(WerewolfDamageCategory.Bashing, 1)]);

        Assert.Equal(1, track.TotalDamage);
        Assert.Equal(-1, track.WoundPenalty);
        Assert.Equal(WerewolfHealthState.Wounded, track.HealthState);
        Assert.Equal(WerewolfHealthLevelName.Machucado, track.CurrentLevel);
    }

    [Fact]
    public void ComputeMaxDamageReturnsIncapacitated()
    {
        var marks = new List<WerewolfDamageMark>();
        for (var i = 0; i < 6; i++)
        {
            marks.Add(new(WerewolfDamageCategory.Bashing, 1));
        }

        var track = WerewolfHealthTrackComputer.Compute(marks);

        Assert.Equal(6, track.TotalDamage);
        Assert.Equal(0, track.WoundPenalty);
        Assert.Equal(WerewolfHealthState.Incapacitated, track.HealthState);
        Assert.Equal(WerewolfHealthLevelName.Incapacitado, track.CurrentLevel);
    }

    [Fact]
    public void ComputeBashingOverflowReturnsUnconscious()
    {
        var marks = new List<WerewolfDamageMark>();
        for (var i = 0; i < 8; i++)
        {
            marks.Add(new(WerewolfDamageCategory.Bashing, 1));
        }

        var track = WerewolfHealthTrackComputer.Compute(marks);

        Assert.Equal(8, track.TotalDamage);
        Assert.Equal(WerewolfHealthState.Unconscious, track.HealthState);
        Assert.Equal(WerewolfDamageCategory.Bashing, track.FatalDamageType);
        Assert.Equal(WerewolfHealthLevelName.Incapacitado, track.CurrentLevel);
    }

    [Fact]
    public void ComputeLethalOverflowReturnsNearDeath()
    {
        var marks = new List<WerewolfDamageMark>();
        for (var i = 0; i < 7; i++)
        {
            marks.Add(new(WerewolfDamageCategory.Lethal, 1));
        }

        var track = WerewolfHealthTrackComputer.Compute(marks);

        Assert.Equal(7, track.TotalDamage);
        Assert.Equal(WerewolfHealthState.NearDeath, track.HealthState);
        Assert.Equal(WerewolfDamageCategory.Lethal, track.FatalDamageType);
        Assert.Equal(WerewolfHealthLevelName.Incapacitado, track.CurrentLevel);
    }

    [Fact]
    public void ComputeAggravatedOverflowReturnsDead()
    {
        var marks = new List<WerewolfDamageMark>();
        for (var i = 0; i < 7; i++)
        {
            marks.Add(new(WerewolfDamageCategory.Aggravated, 1));
        }

        var track = WerewolfHealthTrackComputer.Compute(marks);

        Assert.Equal(7, track.TotalDamage);
        Assert.Equal(WerewolfHealthState.Dead, track.HealthState);
        Assert.Equal(WerewolfDamageCategory.Aggravated, track.FatalDamageType);
        Assert.Equal(WerewolfHealthLevelName.Incapacitado, track.CurrentLevel);
    }

    [Fact]
    public void ComputeWeakenedImmuneSystemStartsAtMachucado()
    {
        var track = WerewolfHealthTrackComputer.Compute([], hasWeakenedImmuneSystem: true);

        Assert.Equal(0, track.TotalDamage);
        Assert.Equal(-1, track.WoundPenalty);
        Assert.Equal(WerewolfHealthState.Wounded, track.HealthState);
        Assert.Equal(WerewolfHealthLevelName.Machucado, track.CurrentLevel);
    }

    [Fact]
    public void ComputeMixedDamageSumsCorrectly()
    {
        var marks = new List<WerewolfDamageMark>
        {
            new(WerewolfDamageCategory.Bashing, 2),
            new(WerewolfDamageCategory.Lethal, 1),
            new(WerewolfDamageCategory.Aggravated, 1)
        };

        var track = WerewolfHealthTrackComputer.Compute(marks);

        Assert.Equal(4, track.TotalDamage);
        Assert.Equal(2, track.BashingCount);
        Assert.Equal(1, track.LethalCount);
        Assert.Equal(1, track.AggravatedCount);
        Assert.Equal(-2, track.WoundPenalty);
        Assert.Equal(WerewolfHealthState.Wounded, track.HealthState);
        Assert.Equal(WerewolfHealthLevelName.Espancado, track.CurrentLevel);
    }
}
