using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Hypocrites.Player
{
    public class Player : MonoBehaviour
    {
        public int Health { get; private set; }
        public int Mana { get; private set; }

        public int Strength { get; private set; }
        public int Dexterity { get; private set; }
        public int Intelligence { get; private set; }
        public int Vitality { get; private set; }
        public int Luck { get; private set; }

        public Player()
        {
            Health = PlayerConstants.MAX_STAT_HEALTH;
            Mana = PlayerConstants.MAX_STAT_MANA;

            Strength = PlayerConstants.INIT_STAT_STRENGTH;
            Dexterity = PlayerConstants.INIT_STAT_DEXTERITY;
            Intelligence = PlayerConstants.INIT_STAT_INTELLIGENCE;
            Vitality = PlayerConstants.INIT_STAT_VITALITY;
            Luck = PlayerConstants.INIT_STAT_LUCK;
        }

        public void Dealt(int damage)
        {
            Health -= damage;

            Debug.Log("ouch " + Health);
        }
    }
}
