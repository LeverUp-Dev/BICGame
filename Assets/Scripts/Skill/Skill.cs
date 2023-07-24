using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace Hypocrites.Skill
{
    using Defines;
    using Event.SO;
    using DB.Data;
    using DB.Save;
    using Manager;
    using GSM = Manager.GameStateManager;
    using static UnityEngine.GraphicsBuffer;

    /*
     *  Skill�� Effect�� ����
     *  : Skill�� Being�� �����ϰ� �ִ� ��ų, Effect�� Being�� target���� ��ų�� ����� �������� ������ ��ų(����, ��Ʈ�� ��)
     */
    public class SkillStatus
    {
        public bool IsLocked { get; set; }
        public bool IsCooltime { get; set; }

        public SkillStatus(SkillStatusSave save)
        {
            IsLocked = save.isLocked;
            IsCooltime = save.isCooltime;
        }
    }

    public class EffectStatus
    {
        public bool IsDotActivated { get; set; }

        public EffectStatus(bool isDotActivated = false)
        {
            IsDotActivated = isDotActivated;
        }

        public EffectStatus(EffectStatusSave save)
        {
            IsDotActivated = save.isDotActivated;
        }
    }

    public class Skill
    {
        public string Name { get; private set; }

        public Status BuffValue { get; private set; }
        public Status PenaltyValue { get; private set; }

        public int Remains { get; private set; } // ms
        public int Interval { get; private set; } // ms
        public int Cooltime { get; private set; }
        public int PercentageDamage { get; private set; }
        public int PercentageHeal { get; private set; }
        public int AttackCount { get; private set; }
        public int RemainCount { get; private set; }

        public SkillType Type { get; private set; }
        public SkillAttackType AttackType { get; private set; }
        public SkillTargetingType TargetingType { get; private set; }

        delegate void Start(Being caster, Being target);
        Start starter;

        public Skill(SkillSave save)
        {
            Load(save);

            if (AttackType == SkillAttackType.DOT)
            {
                starter = StartEffectInterval;
            }
            else if (AttackType == SkillAttackType.UTILITY)
            {
                starter = StartUtil;
            }
            else
            {
                starter = StartEffect;
            }
        }

        void Load(SkillSave save)
        {
            Name = save.name;

            BuffValue = new Status(save.buffValue);
            PenaltyValue = new Status(save.penaltyValue);

            Remains = save.remains;
            Interval = save.interval;
            Cooltime = save.cooltime;
            PercentageDamage = save.percentageDamage;
            PercentageHeal = save.percentageHeal;
            AttackCount = save.attackCount;
            RemainCount = save.remainCount;
            Type = save.type;
            AttackType = save.attackType;
            TargetingType = save.targetingType;
        }

        public async void Use(Being caster, Being target)
        {
            await Task.Run(() => {
                // ���� ���� ��쿣 ������ ��ų ��� �Ұ���
                if (GSM.Instance.state == GameState.OnBattle && TargetingType == SkillTargetingType.NONE)
                    return;

                // ������ ���� ��쿣 ���� ��ų ��� �Ұ���
                if (GSM.Instance.state != GameState.OnBattle && TargetingType != SkillTargetingType.NONE)
                    return;

                starter(caster, target);

                if (Remains > 0)
                    Thread.Sleep(Remains);

                target.RemoveEffect(this);
            });
        }

        #region ��ų ��� ���� �޼ҵ�
        void StartEffectInterval(Being caster, Being target)
        {
            target.EffectStatuses[this].IsDotActivated = true;

            StartEffectIntervalCoroutine(caster, target);
        }

        async void StartEffectIntervalCoroutine(Being caster, Being target)
        {
            await Task.Run(() =>
            {
                EffectStatus effectStatus = target.EffectStatuses[this];

                while (effectStatus.IsDotActivated)
                {
                    Thread.Sleep(Interval);
                    Execute(caster, target);
                }
            });
        }

        public void StopEffectInterval(Being target)
        {
            target.EffectStatuses[this].IsDotActivated = false;
        }

        void StartEffect(Being caster, Being target)
        {
            Execute(caster, target);
        }

        void Execute(Being caster, Being target)
        {
            /*
            * ������ ���
            */
            float casterDamage = Type == SkillType.PHYSICAL ? caster.Status.PhysicalOffense : caster.Status.MagicalOffense;
            float finalDamage = casterDamage + casterDamage * (PercentageDamage / 100);

            for (int i = 0; i < AttackCount; ++i)
            {
                if (PercentageDamage > 0)
                    target.Dealt(Type, finalDamage/* + Random.Range(0, (int)(finalDamage / 10))*/, AttackType == SkillAttackType.DOT);

                if (PercentageHeal > 0)
                    target.Healed(caster.Status.MaxHealth * (PercentageHeal / 100));
            }
        }

        void StartUtil(Being caster, Being target)
        {
            // status ������� ó���� �� �Ǵ� ����
        }
        #endregion
    }
}
