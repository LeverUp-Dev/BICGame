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

        /* 스테이터스 정보 */
        public Status Status { get; private set; }
        public Status AdditionalStatus { get; private set; } // 스킬에 의한 스탯 증감치

        /* 스킬 정보 */
        public SkillEventSO SkillRemoveEvent { get; private set; }
        public Dictionary<string, Skill> Skills { get; private set; } // 데이터베이스에서 해당 직업에 맞는 스킬 정보 로드
        public HashSet<Skill> Effects { get; private set; } // 현재 적용 중인 버프 디버프 목록

        public Being(BeingSave save)
        {
            Effects = new HashSet<Skill>();

            Load(save);
        }

        /// <summary>
        /// BeingSave(Json 파일 입출력용 클래스)의 정보를 복사한다
        /// </summary>
        /// <param name="save">설정할 BeingSave</param>
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

            // 쉴드 계산
            if (Status.Shield > 0)
            {
                finalDamage = damage - Status.Shield;
                Status.Shield = finalDamage < 0 ? Status.Shield - damage : 0;
            }

            float defense = type == SkillType.PHYSICAL ? Status.PhysicalDefense : Status.MagicalDefense;
            finalDamage = (finalDamage - defense / 2) / 2;

            // 사망 여부 확인
            if (Status.Health <= finalDamage)
            {
                /* 죽음 */
                if (Effects.Contains(Database.Instance.Skills["사망방어버프"]))
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

                /* TODO : 사망 처리 */
            }

            Status.Health -= finalDamage;
        }

        public virtual void Healed(float healPoint)
        {
            Status.Health += healPoint;

            if (Status.Health > BeingConstants.MAX_STAT_HEALTH)
                Status.Health = BeingConstants.MAX_STAT_HEALTH;
        }

        // TODO : targets를 params로 변경?
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
