using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.DB.Data
{
    using DB.Save;
    using Defines;
    using Event.SO;
    using Skill;

    public class Being
    {
        public string Name { get; private set; }

        /* �������ͽ� ���� */
        public Status Status { get; private set; }
        public Status AdditionalStatus { get; private set; } // ��ų�� ���� ���� ����ġ

        /* ��ų ���� */
        public SkillEventSO SkillRemoveEvent { get; private set; }
        public Dictionary<string, Skill> Skills { get; private set; } // �����ͺ��̽����� �ش� ������ �´� ��ų ���� �ε�
        public HashSet<Skill> Effects { get; private set; } // ���� ���� ���� ���� ����� ���

        public Being(BeingSave save)
        {
            Effects = new HashSet<Skill>();

            Load(save);
        }

        /// <summary>
        /// BeingSave(Json ���� ����¿� Ŭ����)�� ������ �����Ѵ�
        /// </summary>
        /// <param name="save">������ BeingSave</param>
        void Load(BeingSave save)
        {
            Name = save.name;

            Status = new Status(save.status);
            AdditionalStatus= new Status(save.additionalStatus);

            SkillRemoveEvent = Resources.Load<SkillEventSO>(save.skillRemoveEventPath);
            SkillRemoveEvent.action += RemoveEffect;

            Skills = new Dictionary<string, Skill>();
            foreach (string effect in save.effects)
            {
                Skills.Add(effect, Database.Instance.Skills[effect]);
            }
        }

        public virtual void Dealt(SkillType type, float damage, bool isTrueDamage = false)
        {
            float finalDamage = damage;

            // ���� ���
            if (Status.Shield > 0)
            {
                finalDamage = damage - Status.Shield;
                Status.Shield = finalDamage < 0 ? Status.Shield - damage : 0;
            }

            float defense = type == SkillType.PHYSICAL ? Status.PhysicalDefense : Status.MagicalDefense;
            finalDamage = (finalDamage - defense / 2) / 2;

            // ��� ���� Ȯ��
            if (Status.Health <= finalDamage)
            {
                /* ���� */
                if (Effects.Contains(Database.Instance.Skills["���������"]))
                {
                    Status.Health = 1;
                    while (Effects.Count > 0)
                    {
                        Skill effect = null;
                        foreach (Skill e in Effects)
                        {
                            effect = e;
                            break;
                        }
                        Effects.Remove(effect);
                    }

                    return;
                }

                /* TODO : ��� ó�� */
            }

            Status.Health -= finalDamage;
        }

        public virtual void Healed(float healPoint)
        {
            Status.Health += healPoint;

            if (Status.Health > BeingConstants.MAX_STAT_HEALTH)
                Status.Health = BeingConstants.MAX_STAT_HEALTH;
        }

        // TODO : targets�� params�� ����?
        public void UseSkill(Skill skill, Being[] targets)
        {
            if (skill.TargetingType == SkillTargetingType.ITSELF)
            {
                SetEffect(this, skill);
                return;
            }

            for (int i = 0; i < targets.Length; ++i)
            {
                targets[i].SetEffect(this, skill);
            }
        }

        public void SetEffect(Being caster, Skill effect)
        {
            AdditionalStatus.Add(effect.BuffValue);

            if (effect.PenaltyValue != null)
            {
                Status.Sub(effect.PenaltyValue);
            }

            effect.Use(caster, this);
            Effects.Add(effect);
        }

        public void RemoveEffect(Skill effect)
        {
            AdditionalStatus.Sub(effect.BuffValue);

            effect.StopEffectInterval();
            Effects.Remove(effect);
        }
    }
}
