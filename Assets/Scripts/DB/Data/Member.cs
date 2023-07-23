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

        /* ����â UI �ݹ� */
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
                if (Database.Instance.Skills.TryGetValue(save.skillSlot[i], out Skill value))
                    SkillSlot[i] = value;
                else
                    Debug.LogError($"\"{save.skillSlot[i]}\" ��ų�� �����ͺ��̽��� �������� �ʽ��ϴ�.");
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
