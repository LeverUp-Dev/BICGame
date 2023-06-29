using System;

namespace Hypocrites.DB.Data
{
    using DB.Save;
    using Defines;

    public class BeingData
    {
        public string Name { get; private set; }
        public int Level { get; set; }

        public int Health { get; set;}
        public int Mana { get; set;}

        public int Strength { get; set;}
        public int Dexterity { get; set;}
        public int Intelligence { get; set;}
        public int Vitality { get; set;}
        public int Luck { get; set;}

        public BeingData()
        {
            BeingSave save = new BeingSave();

            save.name = "";
            save.level = 1;

            save.health = BeingConstants.MAX_STAT_HEALTH;
            save.mana = BeingConstants.MAX_STAT_MANA;

            save.strength = BeingConstants.INIT_STAT_STRENGTH;
            save.dexterity = BeingConstants.INIT_STAT_DEXTERITY;
            save.intelligence = BeingConstants.INIT_STAT_INTELLIGENCE;
            save.vitality = BeingConstants.INIT_STAT_VITALITY;
            save.luck = BeingConstants.INIT_STAT_LUCK;

            LoadSave(save);
        }

        public BeingData(BeingSave save)
        {
            LoadSave(save);
        }

        /// <summary>
        /// BeingSave(Json 파일 입출력용 클래스)의 정보를 복사한다
        /// </summary>
        /// <param name="save">설정할 BeingSave</param>
        void LoadSave(BeingSave save)
        {
            Name = save.name;
            Level = save.level;

            Health = save.health;
            Mana = save.mana;

            Strength = save.strength;
            Dexterity = save.dexterity;
            Intelligence = save.intelligence;
            Vitality = save.vitality;
            Luck = save.luck;
        }

        public virtual void Dealt(int damage)
        {
            Health -= damage;

            if (Health < 0)
                Health = 0;
        }

        public virtual void Healed(int healPoint)
        {
            Health += healPoint;

            if (Health > BeingConstants.MAX_STAT_HEALTH)
                Health = BeingConstants.MAX_STAT_HEALTH;
        }
    }
}
