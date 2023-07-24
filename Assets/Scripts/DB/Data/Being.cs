using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.DB.Data
{
    using DB.Save;
    using Defines;
    using Event.SO;
    using Skill;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEditor.Experimental.GraphView;

    public class Being
    {
        public string Name { get; private set; }

        /* 스테이터스 정보 */
        public Status Status { get; private set; }
        public Status AdditionalStatus { get; private set; } // 스킬에 의한 스탯 증감치

        /* 스킬 정보 */
        public List<Skill> Skills { get; private set; } // 데이터베이스에서 해당 직업에 맞는 스킬 정보 로드
        public Dictionary<Skill, SkillStatus> SkillStatuses { get; private set; } // 보유 중인 스킬 상태(해금 여부, 활성화 여부 등)
        public HashSet<Skill> Effects { get; private set; } // 현재 적용 중인 버프 디버프 목록
        public Dictionary<Skill, EffectStatus> EffectStatuses { get; private set; } // 적용 중인 스킬 상태

        public Being(BeingSave save)
        {
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
            
            // 소유한 스킬 정보 로드
            Skills = new List<Skill>();
            SkillStatuses = new Dictionary<Skill, SkillStatus>();
            for (int i = 0; i < save.skills.Length; i++)
            {
                Skill skill = Database.Instance.Skills[save.skills[i]];
                Skills.Add(skill);
                SkillStatuses.Add(skill, new SkillStatus(save.skillStatuses[i]));
            }

            // 적용 중인 효과 정보 로드
            Effects = new HashSet<Skill>();
            EffectStatuses = new Dictionary<Skill, EffectStatus>();
            for (int i = 0; i < save.effects.Length; i++)
            {
                Skill effect = Database.Instance.Skills[save.effects[i]];
                Effects.Add(effect);
                EffectStatuses.Add(effect, new EffectStatus(save.effectStatuses[i]));
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

        public void UseSkill(Skill skill, Being[] targets)
        {
            if (SkillStatuses[skill].IsCooltime)
                return;

            SetCooltime(skill);

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
                Status.Sub(effect.PenaltyValue);

            Effects.Add(effect);
            EffectStatuses.Add(effect, new EffectStatus());

            effect.Use(caster, this);
        }

        public void RemoveEffect(Skill effect)
        {
            AdditionalStatus.Sub(effect.BuffValue);

            effect.StopEffectInterval(this);

            Effects.Remove(effect);
            EffectStatuses.Remove(effect);
        }

        async void SetCooltime(Skill skill)
        {
            SkillStatuses[skill].IsCooltime = true;

            await Task.Run(() =>
            {
                Thread.Sleep(skill.Cooltime);
                SkillStatuses[skill].IsCooltime = false;
            });
        }
    }
}
