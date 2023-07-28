using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites.UI.BattleUI
{
    using Skill;
    using DB.Data;
    using Manager;

    public class MemberSkillSlotUI : MonoBehaviour
    {
        public Button[] skillButtons;
        public TextMeshProUGUI[] skillButtonTexts;

        Member member;

        public void Initialize(Member member)
        {
            this.member = member;
            Skill[] skillSlot = member.SkillSlot;

            for (int i = 0; i < skillSlot.Length; ++i)
            {
                if (skillSlot[i] != null)
                    skillButtonTexts[i].text = skillSlot[i].Name;
                else
                    skillButtonTexts[i].text = "";
            }
        }

        public void UseSkill(int index)
        {
            if (member == null)
            {
                Debug.LogError("동료 UI는 생성되었지만 member 데이터가 존재하지 않습니다.");
                return;
            }

            BattleManager.Instance.UseSkill(member, index);
        }
    }
}
