using System;

namespace Hypocrites.DB.Save
{
    using DB.Data;
    using Skill;

    [Serializable]
    public class BeingSave
    {
        public string name;

        public StatusSave status;
        public StatusSave additionalStatus;

        public string[] skills;
        public SkillStatusSave[] skillStatuses;
        public string[] effects;
        public EffectStatusSave[] effectStatuses;

        public BeingSave()
        {

        }

        public BeingSave(Being data)
        {
            name = data.Name;

            status.Load(data.Status);
            additionalStatus.Load(data.AdditionalStatus);

            // ������ ��ų ���� ����
            int i;
            skills = new string[data.Skills.Count];
            skillStatuses = new SkillStatusSave[data.Skills.Count];
            for (i = 0; i < data.Skills.Count; i++)
            {
                Skill skill = data.Skills[i];
                string name = skill.Name;

                skills[i] = name;
                skillStatuses[i] = new SkillStatusSave(name, data.SkillStatuses[skill]);
            }

            // ���� ���� ȿ�� ���� ����
            i = 0;
            effects = new string[data.Effects.Count];
            effectStatuses = new EffectStatusSave[data.Effects.Count];
            foreach (Skill effect in data.Effects)
            {
                string name = effect.Name;
                effects[i++] = name;
                effectStatuses[i] = new EffectStatusSave(name, data.EffectStatuses[effect]);
            }
        }
    }
}
