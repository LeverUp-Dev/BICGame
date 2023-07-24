using System;

namespace Hypocrites.DB.Save
{
    using Defines;
    using Skill;

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
    }

    [Serializable]
    public struct SkillStatusSave
    {
        public string name;
        public bool isLocked;
        public bool isCooltime;

        public SkillStatusSave(string name, SkillStatus skillStatus)
        {
            this.name = name;
            isLocked = skillStatus.IsLocked;
            isCooltime = skillStatus.IsCooltime;
        }
    }

    [Serializable]
    public struct EffectStatusSave
    {
        public string name;
        public bool isDotActivated;

        public EffectStatusSave(string name, EffectStatus effectStatus)
        {
            this.name = name;
            isDotActivated = effectStatus.IsDotActivated;
        }
    }
}
