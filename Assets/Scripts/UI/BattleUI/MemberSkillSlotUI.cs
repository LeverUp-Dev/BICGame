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
                Debug.LogError("���� UI�� �����Ǿ����� member �����Ͱ� �������� �ʽ��ϴ�.");
                return;
            }

            BattleManager.Instance.UseSkill(member, index);
        }
    }
}
