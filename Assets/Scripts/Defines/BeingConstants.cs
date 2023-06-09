namespace Hypocrites.Defines
{
    public class BeingConstants
    {
        public const int MAX_STAT_HEALTH = 100;
        public const int MAX_STAT_MANA = 100;
        public const int MAX_STAT_STRENGTH = 100;
        public const int MAX_STAT_DEXTERITY = 100;
        public const int MAX_STAT_INTELLIGENCE = 100;
        public const int MAX_STAT_VITALITY = 100;
        public const int MAX_STAT_LUCK = 100;

        public const int INIT_STAT_STRENGTH = 10;
        public const int INIT_STAT_DEXTERITY = 10;
        public const int INIT_STAT_INTELLIGENCE = 10;
        public const int INIT_STAT_VITALITY = 10;
        public const int INIT_STAT_LUCK = 5;
    }

    public enum BeingStatusType
    {
        STRENGTH,
        DEXTERITY,
        INTELLIGENCE,
        VITALITY,
        LUCK
    }
}
