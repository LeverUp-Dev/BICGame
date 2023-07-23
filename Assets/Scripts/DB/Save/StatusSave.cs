using Hypocrites.DB.Data;
using Hypocrites.Defines;
using System;

namespace Hypocrites.DB.Save
{
    [Serializable]
    public class StatusSave
    {
        public BeingClass @class;

        public float maxHealth;
        public float health;
        public float maxMana;
        public float mana;
        public float shield;

        public float physicalOffense;
        public float magicalOffense;
        public float physicalDefense;
        public float magicalDefense;

        public float cooltimeReduction;
        public float healthRegeneration;
        public float manaRegeneration;
        public float evasionChance;
        public float criticalChance;
        public float criticalDamage;

        public float strength;
        public float intelligence;
        public float dexterity;
        public float faith;

        public void Load(Status status)
        {
            @class = status.Class;

            maxHealth = status.MaxHealth;
            health = status.Health;
            maxMana = status.MaxMana;
            mana = status.Mana;

            physicalOffense = status.PhysicalOffense;
            magicalOffense = status.MagicalOffense;
            physicalDefense = status.PhysicalDefense;
            magicalDefense = status.MagicalDefense;

            cooltimeReduction = status.CooltimeReduction;
            healthRegeneration = status.HealthRegeneration;
            manaRegeneration = status.ManaRegeneration;
            evasionChance = status.EvasionChance;
            criticalChance = status.CriticalChance;
            criticalDamage = status.CriticalDamage;

            strength = status.Strength;
            intelligence = status.Intelligence;
            dexterity = status.Dexterity;
            faith = status.Faith;
        }
    }
}
