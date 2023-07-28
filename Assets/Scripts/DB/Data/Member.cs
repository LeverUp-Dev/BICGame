using UnityEngine;

namespace Hypocrites.DB.Data
{
    using DB.Save;
    using Defines;
    using Skill;

    public class Member : Being
    {
        public Sprite Portrait { get; private set; }
        public bool IsMember { get; set; }
        public string PortraitPath { get; private set; }
        public Skill[] SkillSlot { get; private set; }

        /* 상태창 UI 콜백 */
        public delegate void OnHpChanged(int max, int cur);
        public OnHpChanged onHpChanged;

        public Member(MemberSave save) : base(save)
        {
            PortraitPath = save.portraitPath;
            Portrait = Resources.Load<Sprite>(save.portraitPath);
            IsMember = save.isMember;

            SkillSlot = new Skill[4];
            for (int i = 0; i < save.skillSlot.Length; i++)
            {
                Skill skill = null;

                for (int j = 0; j < Skills.Count; i++)
                {
                    if (Skills[i].Name.Equals(save.skillSlot[i]))
                    {
                        skill = Skills[i];
                        break;
                    }
                }

                if (skill != null)
                    SkillSlot[i] = skill;
                else
                    Debug.LogError($"\"{save.skillSlot[i]}\" 스킬이 {Name}에게 존재하지 않습니다.");
            }
        }

        public override void Dealt(SkillType type, float damage, bool isTrueDamage = false)
        {
            base.Dealt(type, damage, isTrueDamage);

            onHpChanged(BeingConstants.MAX_STAT_HEALTH, (int)Status.Health);
        }

        public override void Healed(float healPoint)
        {
            base.Healed(healPoint);

            onHpChanged(BeingConstants.MAX_STAT_HEALTH, (int)Status.Health);
        }
    }
}
