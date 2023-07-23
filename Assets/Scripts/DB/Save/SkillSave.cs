using Hypocrites.Defines;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.DB.Save
{
    [Serializable]
    public class SkillSave
    {
        public string name;

        public string skillRemoveEventPath;

        public StatusSave buffValue;
        public StatusSave penaltyValue;

        public int remains;  // ms
        public int interval; // ms
        public int cooltime;
        public int percentageDamage;
        public int percentageHeal;
        public int attackCount;
        public int remainCount;

        public SkillType type;
        public SkillAttackType attackType;
        public SkillTargetingType targetingType;

        public bool isLocked;
        public bool isDotActivated;
    }
}
