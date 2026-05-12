using System;
using System.Collections.Generic;

namespace Games180_FinalProject_EddyZhang;

public class UpgradeDefinition
{
    public string Id;
    public string Name;
    public string Description;
    public UpgradeCategory Category;
    public PlayerForm? Form;

    public UpgradeDefinition(string id, string name, string description, UpgradeCategory category, PlayerForm? form)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        Form = form;
    }
}

public static class UpgradeCatalog
{
    public static List<UpgradeDefinition> GeneralUpgrades = new List<UpgradeDefinition>
    {
        new UpgradeDefinition("battle_instinct", "Battle Instinct", "No longer take knockback while attacking.", UpgradeCategory.Defense, null),
        new UpgradeDefinition("tactical_mastery", "Tactical Mastery", "-25% skill cooldown.", UpgradeCategory.Skill, null),
        new UpgradeDefinition("berserker", "Berserker", "+25% attack speed.", UpgradeCategory.Speed, null),
        new UpgradeDefinition("master_defense", "Master of Defense", "After a successful parry, reduce the next parry cooldown by 60%.", UpgradeCategory.Defense, null),
        new UpgradeDefinition("iron_resolve", "Iron Resolve", "-20% damage received while attacking.", UpgradeCategory.Defense, null),
        new UpgradeDefinition("distant_strike", "Distant Strike", "Normal attacks fire a small forward projectile.", UpgradeCategory.Magic, null),
        new UpgradeDefinition("vital_force", "Vital Force", "+30% max HP.", UpgradeCategory.Defense, null),
        new UpgradeDefinition("sharpened_edge", "Sharpened Edge", "+15 attack damage.", UpgradeCategory.Attack, null),
        new UpgradeDefinition("fleet_foot", "Fleet Foot", "+10% movement speed.", UpgradeCategory.Speed, null),
        new UpgradeDefinition("steady_hand", "Steady Hand", "-30% dash cooldown.", UpgradeCategory.Speed, null)
    };

    public static List<UpgradeDefinition> FormUpgrades = new List<UpgradeDefinition>
    {
        new UpgradeDefinition("phantom_blade", "Phantom Blade", "+25% attack range.", UpgradeCategory.Hero, PlayerForm.Hero),
        new UpgradeDefinition("omnistrike", "Omnistrike", "Master Strike counter hits in a circular area.", UpgradeCategory.Hero, PlayerForm.Hero),
        new UpgradeDefinition("heavy_blade", "Heavy Blade", "If the player has not attacked for 2.0s, the next normal attack deals +30% damage.", UpgradeCategory.Hero, PlayerForm.Hero),
        new UpgradeDefinition("divine_protection", "Divine Protection", "+20% movement speed for 1.0s after using Stellar Blade.", UpgradeCategory.Divine, PlayerForm.Divine),
        new UpgradeDefinition("star_explosion", "Star Explosion", "Stellar Blade explodes on impact, dealing small AoE damage.", UpgradeCategory.Divine, PlayerForm.Divine),
        new UpgradeDefinition("holy_reach", "Holy Reach", "+25% Stellar Blade size and range.", UpgradeCategory.Divine, PlayerForm.Divine),
        new UpgradeDefinition("killing_spree", "Killing Spree", "+10% attack damage per enemy killed in the current encounter, max +40%, resets after encounter.", UpgradeCategory.Demon, PlayerForm.Demon),
        new UpgradeDefinition("mayhem", "Mayhem", "-35% dash cooldown.", UpgradeCategory.Demon, PlayerForm.Demon),
        new UpgradeDefinition("storm_blood", "Storm of Blood", "Activating Blood Daggers clears all enemy projectiles currently on screen.", UpgradeCategory.Demon, PlayerForm.Demon)
    };

    public static UpgradeDefinition RoyalResolve = new UpgradeDefinition("royal_resolve", "Royal Resolve", "+10% max HP and -20% parry cooldown.", UpgradeCategory.Knight, PlayerForm.Knight);
    public static UpgradeDefinition ShieldBlessing = new UpgradeDefinition("event_shield", "Shield Blessing", "-10% damage taken.", UpgradeCategory.Defense, null);
    public static UpgradeDefinition FuryBlessing = new UpgradeDefinition("event_fury", "Fury Blessing", "+15% attack speed.", UpgradeCategory.Speed, null);
    public static UpgradeDefinition CursedPower = new UpgradeDefinition("event_curse", "Cursed Power", "-20% skill cooldown.", UpgradeCategory.Skill, null);

    public static UpgradeDefinition Find(string id)
    {
        if (RoyalResolve.Id == id)
        {
            return RoyalResolve;
        }
        if (ShieldBlessing.Id == id)
        {
            return ShieldBlessing;
        }
        if (FuryBlessing.Id == id)
        {
            return FuryBlessing;
        }
        if (CursedPower.Id == id)
        {
            return CursedPower;
        }

        for (int i = 0; i < GeneralUpgrades.Count; i++)
        {
            if (GeneralUpgrades[i].Id == id)
            {
                return GeneralUpgrades[i];
            }
        }

        for (int i = 0; i < FormUpgrades.Count; i++)
        {
            if (FormUpgrades[i].Id == id)
            {
                return FormUpgrades[i];
            }
        }

        return null;
    }

    public static UpgradeDefinition GetRandomGeneral(Random random, List<string> acquired)
    {
        return GetRandomFromPool(random, GeneralUpgrades, acquired);
    }

    public static UpgradeDefinition GetRandomForm(Random random, PlayerForm form, List<string> acquired)
    {
        List<UpgradeDefinition> pool = new List<UpgradeDefinition>();
        for (int i = 0; i < FormUpgrades.Count; i++)
        {
            if (FormUpgrades[i].Form.HasValue && FormUpgrades[i].Form.Value == form)
            {
                pool.Add(FormUpgrades[i]);
            }
        }

        return GetRandomFromPool(random, pool, acquired);
    }

    private static UpgradeDefinition GetRandomFromPool(Random random, List<UpgradeDefinition> pool, List<string> acquired)
    {
        List<UpgradeDefinition> available = new List<UpgradeDefinition>();
        for (int i = 0; i < pool.Count; i++)
        {
            if (!acquired.Contains(pool[i].Id))
            {
                available.Add(pool[i]);
            }
        }

        if (available.Count == 0)
        {
            return pool[random.Next(pool.Count)];
        }

        return available[random.Next(available.Count)];
    }
}
