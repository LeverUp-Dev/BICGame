namespace Hypocrites.DB.Data
{
    using Defines;
    using DB.Save;

    public class Status
    {
        public BeingClass Class { get; private set; }

        public float MaxHealth { get; set; }
        public float Health { get; set; }
        public float MaxMana { get; set; }
        public float Mana { get; set; }
        public float Shield { get; set; }

        public float PhysicalOffense { get; private set; }
        public float MagicalOffense { get; private set; }
        public float PhysicalDefense { get; private set; }
        public float MagicalDefense { get; private set; }

        public float CooltimeReduction { get; private set; }
        public float HealthRegeneration { get; private set; }
        public float ManaRegeneration { get; private set; }
        public float EvasionChance { get; private set; }
        public float CriticalChance { get; private set; }
        public float CriticalDamage { get; private set; }

        public float Strength { get; private set; }
        public float Intelligence { get; private set; }
        public float Dexterity { get; private set; }
        public float Faith { get; private set; }

        delegate float Calc(float l, float r);

        public Status()
        {

        }

        public Status(StatusSave save)
        {
            Load(save);
        }

        public void Load(StatusSave save)
        {
            Class = save.@class;

            MaxHealth = save.maxHealth;
            Health = save.health;
            MaxMana = save.maxMana;
            Mana = save.mana;
            Shield = save.shield;

            PhysicalOffense = save.physicalOffense;
            MagicalOffense = save.magicalOffense;
            PhysicalDefense = save.physicalDefense;
            MagicalDefense = save.magicalDefense;
            CooltimeReduction = save.cooltimeReduction;
            HealthRegeneration = save.healthRegeneration;
            ManaRegeneration = save.manaRegeneration;
            EvasionChance = save.evasionChance;
            CriticalChance = save.criticalChance;
            CriticalDamage = save.criticalDamage;

            Strength = save.strength;
            Intelligence = save.intelligence;
            Dexterity = save.dexterity;
            Faith = save.faith;
        }

        void CalculateStatus(Status status, Calc Calculate)
        {
            Strength = Calculate(Strength, status.Strength);
            Intelligence = Calculate(Intelligence, status.Intelligence);
            Dexterity = Calculate(Dexterity, status.Dexterity);
            Faith = Calculate(Faith, status.Faith);

            CalculateStatusBasedValue();

            // 단순 합으로 갈 거면 아래와 같이, 아니라면 CalculateStatusBasedValue 내부에 계산식 구현
            CooltimeReduction = Calculate(CooltimeReduction, status.CooltimeReduction);
            HealthRegeneration = Calculate(HealthRegeneration, status.HealthRegeneration);
            ManaRegeneration = Calculate(ManaRegeneration, status.ManaRegeneration);
            EvasionChance = Calculate(EvasionChance, status.EvasionChance);
            CriticalChance = Calculate(CriticalChance, status.CriticalChance);
            CriticalDamage = Calculate(CriticalDamage, status.CriticalDamage);
        }

        void CalculateStatusBasedValue()
        {
            switch (Class)
            {
                case BeingClass.ENEMY:
                    return;

                case BeingClass.WARRIOR:
                    PhysicalOffense = Strength;
                    MagicalOffense = Intelligence;
                    break;
                case BeingClass.MAGICIAN:
                    PhysicalOffense = Strength;
                    MagicalOffense = Intelligence;
                    break;
                case BeingClass.ARCHOR:
                    PhysicalOffense = Dexterity;
                    MagicalOffense = Intelligence;
                    break;
                case BeingClass.PRIEST:
                    PhysicalOffense = Strength;
                    MagicalOffense = Faith;
                    break;
            }

            PhysicalDefense = Strength;
            MagicalDefense = Intelligence;
        }

        public void Add(Status status)
        {
            CalculateStatus(status, (l, r) => { return l + r; });
        }

        public void Sub(Status status)
        {
            CalculateStatus(status, (l, r) => { return l - r; });
        }
    }
}