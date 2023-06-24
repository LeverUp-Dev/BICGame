using System;

namespace Hypocrites.DB.Save
{
    using DB.Data;

    [Serializable]
    public class BeingSave
    {
        public string name;

        public int level;

        public int health;
        public int mana;

        public int strength;
        public int dexterity;
        public int intelligence;
        public int vitality;
        public int luck;

        public BeingSave()
        {

        }

        public BeingSave(BeingData data)
        {
            name = data.Name;
            level = data.Level;
             
            health = data.Health;
            mana = data.Mana;
             
            strength = data.Strength;
            dexterity = data.Dexterity;
            intelligence = data.Intelligence;
            vitality = data.Vitality;
            luck = data.Luck;
        }
    }
}
