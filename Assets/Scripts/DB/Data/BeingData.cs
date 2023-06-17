using System;

namespace Hypocrites.DB.Data
{
    using DB.Save;
    using Defines;

    public class BeingData
    {
        public string Name { get; private set; }
        public int Level { get; private set; }

        public int Health { get; private set;}
        public int Mana { get; private set;}

        public int Strength { get; private set;}
        public int Dexterity { get; private set;}
        public int Intelligence { get; private set;}
        public int Vitality { get; private set;}
        public int Luck { get; private set;}

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
        
        /// <summary>
        /// 체력을 감소시킨다
        /// </summary>
        /// <param name="damage">damage만큼 체력 감소</param>
        /// <returns>체력이 0 이하로 떨어지면 false, 아니면 true</returns>
        public bool Dealt(int damage)
        {
            Health -= damage;

            if (Health < 0)
                Health = 0;
            
            return Health != 0;
        }
    }
}
