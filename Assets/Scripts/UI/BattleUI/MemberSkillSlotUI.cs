using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

namespace Hypocrites.UI.BattleUI
{
    using Skill;
    using DB.Data;
    using Manager;

    public class MemberSkillSlotUI : MonoBehaviour
    {
        public Button[] skillButtons;
        public TextMeshProUGUI[] skillButtonTexts;
        public Image[] skillCooltimeImages;

        Member member;
        float[] skillCooltimes;

        Queue<int> cooltimeSettingQueue;

        private void Update()
        {
            if (cooltimeSettingQueue.Count > 0)
                StartCoroutine(SetCooltimeCoroutine(cooltimeSettingQueue.Dequeue()));
        }

        public void Initialize(Member member)
        {
            this.member = member;
            Skill[] skillSlot = member.SkillSlot;

            if (skillCooltimes == null)
                skillCooltimes = new float[4];

            for (int i = 0; i < skillSlot.Length; ++i)
            {
                if (skillSlot[i] is null)
                {
                    skillButtonTexts[i].text = "";
                    continue;
                }

                skillButtonTexts[i].text = skillSlot[i].Name;
                skillCooltimes[i] = skillSlot[i].Cooltime / 1000;
            }

            cooltimeSettingQueue = new Queue<int>();
        }

        public void UseSkill(int index)
        {
            if (member == null)
            {
                Debug.LogError("동료 UI는 생성되었지만 member 데이터가 존재하지 않습니다.");
                return;
            }

            BattleManager.Instance.UseSkill(member, index, SetCooltime);
        }

        public void SetCooltime(int index)
        {
            cooltimeSettingQueue.Enqueue(index);
        }

        IEnumerator SetCooltimeCoroutine(int index)
        {
            if (index < 0 || index > 4)
            {
                Debug.LogError($"{index}는 스킬 슬롯 범위를 초과합니다.");
                yield break;
            }

            Image cooltimeImage = skillCooltimeImages[index];
            float cooltime = skillCooltimes[index];

            skillButtons[index].enabled = false;
            cooltimeImage.fillAmount = 1;

            while (cooltime > 0)
            {
                cooltime -= Time.deltaTime;
                cooltimeImage.fillAmount = cooltime / skillCooltimes[index];
                yield return new WaitForFixedUpdate();
            }

            skillButtons[index].enabled = true;
            cooltimeImage.fillAmount = 0;
        }
    }
}
