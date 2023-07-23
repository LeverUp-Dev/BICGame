namespace Hypocrites.Defines
{
    public class BeingConstants
    {
        public const int MAX_STAT_HEALTH = 100;
        public const int MAX_STAT_MANA = 100;
        public const int MAX_STAT = 20;

        public const int INIT_STAT_PHYSICAL_OFFENSE = 0;
        public const int INIT_STAT_MAGICAL_OFFENSE = 0;
        public const int INIT_STAT_PHYSICAL_DEFENSE = 0;
        public const int INIT_STAT_MAGICAL_DEFENSE = 0;

        public const int INIT_STAT_COOLTIME_REDUCTION = 0;
        public const int INIT_STAT_HEALTH_REGENERATION = 0;
        public const int INIT_STAT_MANA_REGENERATION = 0;
        public const int INIT_STAT_EVASION_CHANCE = 0;
        public const int INIT_STAT_CRITICAL_CHANCE = 10;
        public const int INIT_STAT_CRITICAL_DAMAGE = 100;

        public const int INIT_STAT_STRENGTH = 10;
        public const int INIT_STAT_INTELLIGENCE = 10;
        public const int INIT_STAT_DEXTERITY = 10;
        public const int INIT_STAT_FAITH = 10;
    }

    public enum BeingClass
    {
        ENEMY, // Àû
        WARRIOR,
        MAGICIAN,
        ARCHOR,
        PRIEST
    }

    public enum BeingStatusType
    {
        STRENGTH,
        INTELLIGENCE,
        DEXTERITY,
        FAITH
    }
}
